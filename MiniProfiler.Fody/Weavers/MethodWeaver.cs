using System.Collections.Generic;
using System.Linq;
using MiniProfiler.Fody.Helpers;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace MiniProfiler.Fody.Weavers
{
    internal class MethodWeaver : MethodWeaverBase
    {
        private Instruction _firstInstructionAfterTraceEnter;

        internal MethodWeaver(TypeReferenceProvider typeReferenceProvider,
            MethodReferenceProvider methodReferenceProvider,
            MethodDefinition methodDefinition)
            : base(typeReferenceProvider, methodReferenceProvider, methodDefinition)
        {

        }

        protected override void WeaveProfilerEnter()
        {
            _firstInstructionAfterTraceEnter = _body.Instructions.FirstOrDefault();

            var instructions = new List<Instruction>();
            instructions.AddRange(LoadMethodNameOnStack());
            instructions.Add(Instruction.Create(OpCodes.Call, _methodReferenceProvider.GetProfilerStepStart()));
            instructions.Add(Instruction.Create(OpCodes.Stloc, ProfilerStepVariable));

            _body.InsertAtTheBeginning(instructions);
        }

        protected override void WeaveProfilerLeave()
        {
            VariableDefinition returnValueDef = null;
            if (HasReturnValue)
            {
                returnValueDef = _body.DeclareVariable("$returnValue", ReturnType);
            }

            var allReturns = _body.Instructions.Where(instr => instr.OpCode == OpCodes.Ret).ToList();

            var handlerStart = CreateFinallyHandlerAtTheEnd();
            var profilerReturnStart = CreateProfilerReturnAtTheEnd(returnValueDef);

            //add exception handler 
            if (!_isEmptyBody)
            {
                _body.ExceptionHandlers.Add(new ExceptionHandler(ExceptionHandlerType.Finally)
                {
                    TryStart = _firstInstructionAfterTraceEnter,
                    TryEnd = handlerStart,
                    HandlerStart = handlerStart,
                    HandlerEnd = profilerReturnStart
                });
            }

            foreach (var @return in allReturns)
            {
                ChangeReturnToLeaveProfilerReturn(@return, returnValueDef, profilerReturnStart);
            }
        }

        private void ChangeReturnToLeaveProfilerReturn(Instruction @return, VariableDefinition returnValueDef, Instruction actualReturn)
        {
            var instructions = new List<Instruction>();

            if (HasReturnValue)
            {
                instructions.Add(Instruction.Create(OpCodes.Stloc, returnValueDef)); //store it in local variable
            }
            instructions.Add(Instruction.Create(OpCodes.Leave, actualReturn));

            _body.Replace(@return, instructions);
        }

        private Instruction CreateFinallyHandlerAtTheEnd()
        {
            var instructions = new List<Instruction>();

            var nop = Instruction.Create(OpCodes.Nop);
            instructions.AddRange(new[]
            {
                Instruction.Create(OpCodes.Ldloc, ProfilerStepVariable),
                Instruction.Create(OpCodes.Ldnull),
                Instruction.Create(OpCodes.Cgt_Un),
                Instruction.Create(OpCodes.Brfalse_S, nop),
                Instruction.Create(OpCodes.Nop),
                Instruction.Create(OpCodes.Ldloc, ProfilerStepVariable),
                Instruction.Create(OpCodes.Callvirt, _methodReferenceProvider.GetDispose()),
                Instruction.Create(OpCodes.Nop),
                nop
            });

            instructions.Add(Instruction.Create(OpCodes.Endfinally));

            return _body.AddAtTheEnd(instructions);
        }

        private Instruction CreateProfilerReturnAtTheEnd(VariableDefinition returnValueDef)
        {
            var instructions = new List<Instruction>();

            if (HasReturnValue)
            {
                instructions.Add(Instruction.Create(OpCodes.Ldloc, returnValueDef)); //read from local variable
            }

            instructions.Add(Instruction.Create(OpCodes.Ret));

            return _body.AddAtTheEnd(instructions);
        }
    }
}