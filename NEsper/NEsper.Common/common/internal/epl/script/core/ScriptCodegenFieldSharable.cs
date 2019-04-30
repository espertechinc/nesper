///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.script.core
{
    public class ScriptCodegenFieldSharable : CodegenFieldSharable
    {
        private readonly string _scriptName;
        private readonly int _parameterNumber;
        private readonly ScriptDescriptorCompileTime _scriptDescriptor;
        private readonly CodegenClassScope _classScope;

        public ScriptCodegenFieldSharable(
            ScriptDescriptorCompileTime scriptDescriptor,
            CodegenClassScope classScope)
        {
            _scriptName = scriptDescriptor.ScriptName;
            _parameterNumber = scriptDescriptor.ParameterNames.Length;
            _scriptDescriptor = scriptDescriptor;
            _classScope = classScope;
        }

        public Type Type()
        {
            return typeof(ScriptEvaluator);
        }

        public CodegenExpression InitCtorScoped()
        {
            return StaticMethod(
                typeof(ScriptEvaluatorCompilerRuntime), "compileScriptEval",
                _scriptDescriptor.Make(_classScope.NamespaceScope.InitMethod, _classScope));
        }

        public override bool Equals(object o)
        {
            if (this == o)
            {
                return true;
            }

            if (o == null || GetType() != o.GetType())
            {
                return false;
            }

            ScriptCodegenFieldSharable that = (ScriptCodegenFieldSharable) o;

            if (_parameterNumber != that._parameterNumber)
            {
                return false;
            }

            return _scriptName.Equals(that._scriptName);
        }

        public override int GetHashCode()
        {
            int result = _scriptName.GetHashCode();
            result = 31 * result + _parameterNumber;
            return result;
        }
    }
} // end of namespace