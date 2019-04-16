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
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.bytecodemodel.model.expression
{
    public class CodegenExpressionExprDotMethod : CodegenExpression
    {
        private readonly CodegenExpression _expression;
        private readonly string _method;
        private readonly CodegenExpression[] _params;

        public CodegenExpressionExprDotMethod(
            CodegenExpression expression,
            string method,
            CodegenExpression[] @params)
        {
            if (expression == null) {
                throw new ArgumentException("Null expression");
            }

            foreach (var param in @params) {
                if (param == null) {
                    throw new ArgumentException("Null parameter expression");
                }
            }

            this._expression = expression;
            this._method = method;
            this._params = @params;
        }

        public void Render(
            StringBuilder builder,
            IDictionary<Type, string> imports,
            bool isInnerClass)
        {
            if (_expression is CodegenExpressionRef) {
                _expression.Render(builder, imports, isInnerClass);
            }
            else {
                builder.Append("(");
                _expression.Render(builder, imports, isInnerClass);
                builder.Append(")");
            }

            builder.Append('.').Append(_method).Append("(");
            RenderExpressions(builder, _params, imports, isInnerClass);
            builder.Append(")");
        }

        public void MergeClasses(ISet<Type> classes)
        {
            _expression.MergeClasses(classes);
            MergeClassesExpressions(classes, _params);
        }
    }
} // end of namespace