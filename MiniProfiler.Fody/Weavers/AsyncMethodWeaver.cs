using System;
using System.Collections.Generic;
using System.Linq;
using MiniProfiler.Fody.Helpers;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace MiniProfiler.Fody.Weavers
{
    internal class AsyncMethodWeaver : MethodWeaverBase
    {
        private readonly TypeDefinition _generatedType;
        private FieldDefinition _profilerStepFieldRef;
        private MethodBody _moveNextBody;

        internal AsyncMethodWeaver(TypeReferenceProvider typeReferenceProvider,
            MethodReferenceProvider methodReferenceProvider,
            MethodDefinition methodDefinition)
            : base(typeReferenceProvider, methodReferenceProvider, methodDefinition)
        {
            var asyncAttribute = methodDefinition.CustomAttributes.Single(it => it.AttributeType.FullName.Equals(_typeReferenceProvider.AsyncStateMachineAttribute.FullName));
            _generatedType = asyncAttribute.ConstructorArguments[0].Value as TypeDefinition;
        }

        protected override void WeaveProfilerEnter()
        {
            var instructions = CreateProfilerStepInstructions();
            _body.InsertAtTheBeginning(instructions);

            ExtendGeneratedTypeWithProfilerStepField();

            var genVar = _body.Variables.FirstOrDefault(it => it.VariableType.GetElementType().FullName.Equals(_generatedType.FullName));
            if (genVar == null)
            {
                throw new ApplicationException($"Cannot find async statemachine for async method {this._methodDefinition.Name}.");
            }

            var processor = _body.GetILProcessor();

            var instrs = new List<Instruction>();
            Instruction instr;

            //search the first ldloc or ldloca that uses this variable and insert our param passing block
            if (!_generatedType.IsValueType)
            {
                instr = _body.Instructions.FirstOrDefault(it => it.OpCode == OpCodes.Ldloc && it.Operand == genVar);
                instrs.Add(Instruction.Create(OpCodes.Ldloc, genVar));
            }
            else
            {
                instr = _body.Instructions.FirstOrDefault(it => it.OpCode == OpCodes.Ldloca && it.Operand == genVar);
                instrs.Add(Instruction.Create(OpCodes.Ldloca, genVar));
            }

            instrs.Add(Instruction.Create(OpCodes.Ldloc, ProfilerStepVariable));
            instrs.Add(Instruction.Create(OpCodes.Stfld, _profilerStepFieldRef));
            instr.InsertBefore(processor, instrs);
        }

        protected override void WeaveProfilerLeave()
        {
            var moveNextDefinition = _generatedType.Methods.Single(it => it.Name.Equals("MoveNext", StringComparison.OrdinalIgnoreCase));
            _moveNextBody = moveNextDefinition.Body;

            _moveNextBody.SimplifyMacros();

            var setResultInstr = _moveNextBody.Instructions.FirstOrDefault(IsCallSetResult);
            var setExceptionInstr = _moveNextBody.Instructions.FirstOrDefault(IsCallSetException);

            var processor = _moveNextBody.GetILProcessor();

            if (setResultInstr != null)
            {
                var disposeInstructions = CreateDisposeInstructions(setResultInstr);
                setResultInstr.InsertBefore(processor, disposeInstructions);
            }

            if (setExceptionInstr != null)
            {
                var disposeInstructions = CreateDisposeInstructions(setExceptionInstr);
                setExceptionInstr.InsertBefore(processor, disposeInstructions);
            }

            _moveNextBody.InitLocals = true;
            _moveNextBody.OptimizeMacros();
        }

        private List<Instruction> CreateDisposeInstructions(Instruction instruction)
        {
            var disposeInstructions = new List<Instruction>();

            var nop = Instruction.Create(OpCodes.Nop);
            disposeInstructions.AddRange(new[]
            {
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldfld, _profilerStepFieldRef),
                Instruction.Create(OpCodes.Brfalse_S, instruction),

                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldfld, _profilerStepFieldRef),
                Instruction.Create(OpCodes.Callvirt, _methodReferenceProvider.GetDispose()),

                //nop
            });
            return disposeInstructions;
        }

        private static bool IsCallSetResult(Instruction instr)
        {
            if (instr.OpCode != OpCodes.Call) return false;
            var methodRef = instr.Operand as MethodReference;
            if (methodRef == null) return false;

            return (methodRef.Name.Equals("SetResult", StringComparison.OrdinalIgnoreCase) &&
                    (methodRef.DeclaringType.FullName.StartsWith("System.Runtime.CompilerServices.AsyncTaskMethodBuilder", StringComparison.OrdinalIgnoreCase)));
        }

        private static bool IsCallSetException(Instruction instr)
        {
            if (instr.OpCode != OpCodes.Call) return false;
            var methodRef = instr.Operand as MethodReference;
            if (methodRef == null) return false;

            return (methodRef.Name.Equals("SetException", StringComparison.OrdinalIgnoreCase) &&
                    (methodRef.DeclaringType.FullName.StartsWith("System.Runtime.CompilerServices.AsyncTaskMethodBuilder", StringComparison.OrdinalIgnoreCase)));
        }

        private void ExtendGeneratedTypeWithProfilerStepField()
        {
            var profilerStepField = new FieldDefinition(ProfilerStepVarName, FieldAttributes.Public, _typeReferenceProvider.Disposable);
            _generatedType.Fields.Add(profilerStepField);
            _profilerStepFieldRef = profilerStepField;
        }
    }
}