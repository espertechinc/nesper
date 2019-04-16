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

namespace com.espertech.esper.common.@internal.bytecodemodel.model.expression
{
    public class CodegenExpressionExprDotUnderlying : CodegenExpression
    {
        private readonly CodegenExpression _expression;

        public CodegenExpressionExprDotUnderlying(CodegenExpression expression)
        {
            this._expression = expression;
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

            builder.Append(".getUnderlying()");
        }

        public void MergeClasses(ISet<Type> classes)
        {
            _expression.MergeClasses(classes);
        }
    }
} // end of namespace