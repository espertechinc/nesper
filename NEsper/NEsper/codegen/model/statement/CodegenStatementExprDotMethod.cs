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

using com.espertech.esper.codegen.core;
using com.espertech.esper.codegen.model.expression;

//import static com.espertech.esper.codegen.model.expression.CodegenExpressionBuilder.mergeClassesExpressions;
//import static com.espertech.esper.codegen.model.expression.CodegenExpressionBuilder.renderExpressions;

namespace com.espertech.esper.codegen.model.statement
{
    public class CodegenStatementExprDotMethod : CodegenStatementBase
    {
        private readonly ICodegenExpression expression;
        private readonly string method;
        private readonly ICodegenExpression[] parameters;

        public CodegenStatementExprDotMethod(ICodegenExpression expression, string method, ICodegenExpression[] parameters)
        {
            this.expression = expression;
            this.method = method;
            this.parameters = parameters;
        }

        public override void RenderStatement(StringBuilder builder, IDictionary<Type, string> imports)
        {
            if (expression is CodegenExpressionRef)
            {
                expression.Render(builder, imports);
            }
            else
            {
                builder.Append("(");
                expression.Render(builder, imports);
                builder.Append(")");
            }
            builder.Append('.').Append(method).Append("(");
            CodegenExpressionBuilder.RenderExpressions(builder, parameters, imports);
            builder.Append(")");
        }

        public override void MergeClasses(ICollection<Type> classes)
        {
            expression.MergeClasses(classes);
            CodegenExpressionBuilder.MergeClassesExpressions(classes, parameters);
        }
    }
} // end of namespace