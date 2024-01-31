///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.script.core
{
    public class ScriptCodegenFieldSharable : CodegenFieldSharable
    {
        private readonly string _scriptName;
        private readonly ExprNode[] _parameters;
        private readonly ScriptDescriptorCompileTime _scriptDescriptor;
        private readonly CodegenClassScope _classScope;

        public ScriptCodegenFieldSharable(
            ScriptDescriptorCompileTime scriptDescriptor,
            CodegenClassScope classScope)
        {
            _scriptName = scriptDescriptor.ScriptName;
            _parameters = scriptDescriptor.Parameters;
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
                typeof(ScriptEvaluatorCompilerRuntime),
                "CompileScriptEval",
                _scriptDescriptor.Make(_classScope.NamespaceScope.InitMethod, _classScope));
        }

        public override bool Equals(object o)
        {
            if (this == o) {
                return true;
            }

            if (o == null || GetType() != o.GetType()) {
                return false;
            }

            var that = (ScriptCodegenFieldSharable)o;
            if (!ExprNodeUtilityCompare.DeepEquals(_parameters, that._parameters, false)) {
                return false;
            }

            return _scriptName.Equals(that._scriptName);
        }

        public override int GetHashCode()
        {
            var result = _scriptName.GetHashCode();
            result = 31 * result + _parameters.Length;
            return result;
        }
    }
} // end of namespace