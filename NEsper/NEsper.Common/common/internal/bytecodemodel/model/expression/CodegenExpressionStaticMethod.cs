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

using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.util;
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
        private readonly Type[] _methodTypeArgs;
        private readonly CodegenExpression[] _params;

        public CodegenExpressionStaticMethod(
            Type target,
            string methodName,
            Type[] methodTypeArgs,
            CodegenExpression[] @params)
        {
            this._target = target;
            this._targetClassName = null;
            this._methodName = methodName;
            this._methodTypeArgs = methodTypeArgs;
            this._params = @params;
        }


        public CodegenExpressionStaticMethod(
            Type target,
            string methodName,
            CodegenExpression[] @params)
        {
            this._target = target;
            this._targetClassName = null;
            this._methodName = methodName;
            this._methodTypeArgs = new Type[0];
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
            this._methodTypeArgs = new Type[0];
            this._params = @params;
        }

        public void Render(
            StringBuilder builder,
            bool isInnerClass,
            int level,
            CodegenIndent indent)
        {
            if (_target != null) {
                AppendClassName(builder, _target);
            }
            else {
                builder.Append(_targetClassName);
            }

            builder.Append(".");
            builder.Append(_methodName);

            if (_methodTypeArgs.Length > 0) {
                var delimiter = "";

                builder.Append('<');

                foreach (var methodTypeArg in _methodTypeArgs) {
                    builder.Append(delimiter);
                    AppendClassName(builder, methodTypeArg);
                    delimiter = ",";
                }

                builder.Append('>');
            }

            builder.Append("(");
            RenderExpressions(builder, _params, isInnerClass);
            builder.Append(")");
        }

        public void MergeClasses(ISet<Type> classes)
        {
            classes.AddToSet(_target);
            foreach (var methodTypeArg in _methodTypeArgs) {
                classes.AddToSet(methodTypeArg);
            }
            CodegenExpressionBuilder.MergeClassesExpressions(classes, _params);
        }
    }
} // end of namespace