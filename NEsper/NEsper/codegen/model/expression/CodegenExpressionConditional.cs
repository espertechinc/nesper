///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;

namespace com.espertech.esper.codegen.model.expression
{
    public class CodegenExpressionConditional : ICodegenExpression
    {
        private readonly ICodegenExpression condition;
        private readonly ICodegenExpression expressionTrue;
        private readonly ICodegenExpression expressionFalse;

        public CodegenExpressionConditional(ICodegenExpression condition, ICodegenExpression expressionTrue, ICodegenExpression expressionFalse)
        {
            this.condition = condition;
            this.expressionTrue = expressionTrue;
            this.expressionFalse = expressionFalse;
        }

        public void Render(StringBuilder builder, IDictionary<Type, string> imports)
        {
            builder.Append("(");
            condition.Render(builder, imports);
            builder.Append(" ? ");
            expressionTrue.Render(builder, imports);
            builder.Append(" : ");
            expressionFalse.Render(builder, imports);
            builder.Append(")");
        }

        public void MergeClasses(ICollection<Type> classes)
        {
            condition.MergeClasses(classes);
            expressionTrue.MergeClasses(classes);
            expressionFalse.MergeClasses(classes);
        }
    }
} // end of namespace