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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.script.core
{
    public class ScriptCodegenFieldSharable : CodegenFieldSharable
    {
        private readonly string scriptName;
        private readonly int parameterNumber;
        private readonly ScriptDescriptorCompileTime scriptDescriptor;
        private readonly CodegenClassScope classScope;

        public ScriptCodegenFieldSharable(
            ScriptDescriptorCompileTime scriptDescriptor,
            CodegenClassScope classScope)
        {
            this.scriptName = scriptDescriptor.ScriptName;
            this.parameterNumber = scriptDescriptor.ParameterNames.Length;
            this.scriptDescriptor = scriptDescriptor;
            this.classScope = classScope;
        }

        public Type Type()
        {
            return typeof(ScriptEvaluator);
        }

        public CodegenExpression InitCtorScoped()
        {
            return StaticMethod(
                typeof(ScriptEvaluatorCompilerRuntime), "compileScriptEval", scriptDescriptor.Make(classScope.PackageScope.InitMethod, classScope));
        }

        public override bool Equals(object o)
        {
            if (this == o) return true;
            if (o == null || GetType() != o.GetType()) return false;

            ScriptCodegenFieldSharable that = (ScriptCodegenFieldSharable) o;

            if (parameterNumber != that.parameterNumber) return false;
            return scriptName.Equals(that.scriptName);
        }

        public override int GetHashCode()
        {
            int result = scriptName.GetHashCode();
            result = 31 * result + parameterNumber;
            return result;
        }
    }
} // end of namespace