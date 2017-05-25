using System;
using System.Collections.Generic;
using System.Text;
using MiniProfiler.Fody.Helpers;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace MiniProfiler.Fody.Weavers
{
    internal abstract class MethodWeaverBase
    {
        protected readonly TypeReferenceProvider _typeReferenceProvider;
        protected readonly MethodReferenceProvider _methodReferenceProvider;
        protected readonly MethodDefinition _methodDefinition;
        protected readonly MethodBody _body;
        protected readonly bool _isEmptyBody;

        internal MethodWeaverBase(TypeReferenceProvider typeReferenceProvider, MethodReferenceProvider methodReferenceProvider, MethodDefinition methodDefinition)
        {
            _typeReferenceProvider = typeReferenceProvider;
            _methodReferenceProvider = methodReferenceProvider;
            _methodDefinition = methodDefinition;
            _body = methodDefinition.Body;
            _isEmptyBody = (_body.Instructions.Count == 0);
        }

        private string PrettyMethodName
        {
            get
            {
                //check if method name is generated and prettyfy it
                var position = _methodDefinition.Name.IndexOf(">", StringComparison.OrdinalIgnoreCase);
                return position > 1 ? _methodDefinition.Name.Substring(1, position - 1) : _methodDefinition.Name;
            }
        }

        protected virtual TypeReference ReturnType
        {
            get { return _methodDefinition.ReturnType; }
        }

        protected virtual bool HasReturnValue
        {
            get { return (ReturnType.MetadataType != MetadataType.Void); }
        }


        private VariableDefinition _profilerStepVariable;

        protected VariableDefinition ProfilerStepVariable
        {
            get
            {
                if (_profilerStepVariable == null)
                {
                    _profilerStepVariable = _body.DeclareVariable("$step", _typeReferenceProvider.Disposable);
                }

                return _profilerStepVariable;
            }
        }

        public void Execute(bool addProfiler)
        {
            _body.SimplifyMacros();

            if (addProfiler)
            {
                WeaveProfilerEnter();
                WeaveProfilerLeave();
            }

            _body.InitLocals = true;
            _body.OptimizeMacros();
        }

        protected abstract void WeaveProfilerLeave();

        protected abstract void WeaveProfilerEnter();

        protected IEnumerable<Instruction> LoadMethodNameOnStack()
        {
            var sb = new StringBuilder();

            sb.Append(_methodDefinition.DeclaringType.FullName);
            sb.Append(_methodDefinition.IsStatic ? ":" : ".");

            sb.Append(PrettyMethodName);
            sb.Append("(");
            for (var i = 0; i < _methodDefinition.Parameters.Count; i++)
            {
                var paramDef = _methodDefinition.Parameters[i];
                if (paramDef.IsOut) sb.Append("out ");
                sb.Append(paramDef.ParameterType.Name);
                if (i < _methodDefinition.Parameters.Count - 1) sb.Append(", ");
            }
            sb.Append(")");

            return new[]
            {
                Instruction.Create(OpCodes.Ldstr, sb.ToString())
            };
        }

    }
}