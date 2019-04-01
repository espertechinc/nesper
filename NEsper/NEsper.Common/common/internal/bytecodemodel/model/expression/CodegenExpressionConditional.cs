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
    public class CodegenExpressionConditional : CodegenExpression
    {
        private readonly CodegenExpression _condition;
        private readonly CodegenExpression _expressionFalse;
        private readonly CodegenExpression _expressionTrue;

        public CodegenExpressionConditional(
            CodegenExpression condition, CodegenExpression expressionTrue, CodegenExpression expressionFalse)
        {
            this._condition = condition;
            this._expressionTrue = expressionTrue;
            this._expressionFalse = expressionFalse;
        }

        public void Render(StringBuilder builder, IDictionary<Type, string> imports, bool isInnerClass)
        {
            builder.Append("(");
            _condition.Render(builder, imports, isInnerClass);
            builder.Append(" ? ");
            _expressionTrue.Render(builder, imports, isInnerClass);
            builder.Append(" : ");
            _expressionFalse.Render(builder, imports, isInnerClass);
            builder.Append(")");
        }

        public void MergeClasses(ISet<Type> classes)
        {
            _condition.MergeClasses(classes);
            _expressionTrue.MergeClasses(classes);
            _expressionFalse.MergeClasses(classes);
        }
    }
} // end of namespace