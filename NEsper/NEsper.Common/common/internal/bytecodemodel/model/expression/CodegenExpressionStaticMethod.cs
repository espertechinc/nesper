///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.core.CodeGenerationHelper;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.bytecodemodel.model.expression
{
    public class CodegenExpressionStaticMethod : CodegenExpression
    {
        private readonly Type _target;
        private readonly string _targetClassName;
        private readonly string _methodName;
        private readonly CodegenExpression[] _params;

        public CodegenExpressionStaticMethod(
            Type target,
            string methodName,
            CodegenExpression[] @params)
        {
            this._target = target;
            this._targetClassName = null;
            this._methodName = methodName;
            this._params = @params;
        }

        public CodegenExpressionStaticMethod(
            string targetClassName,
            string methodName,
            CodegenExpression[] @params)
        {
            this._target = null;
            this._targetClassName = targetClassName;
            this._methodName = methodName;
            this._params = @params;
        }

        public void Render(
            StringBuilder builder,
            IDictionary<Type, string> imports,
            bool isInnerClass)
        {
            if (_target != null) {
                AppendClassName(builder, _target, null, imports);
            }
            else {
                builder.Append(_targetClassName);
            }

            builder.Append(".");
            builder.Append(_methodName);
            builder.Append("(");
            RenderExpressions(builder, _params, imports, isInnerClass);
            builder.Append(")");
        }

        public void MergeClasses(ISet<Type> classes)
        {
            classes.Add(_target);
            CodegenExpressionBuilder.MergeClassesExpressions(classes, _params);
        }
    }
} // end of namespace