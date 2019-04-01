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

using com.espertech.esper.common.@internal.bytecodemodel.model.expression;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.bytecodemodel.model.statement
{
    public class CodegenStatementExprDotMethod : CodegenStatementBase
    {
        private readonly CodegenExpression expression;
        private readonly string method;
        private readonly CodegenExpression[] @params;

        public CodegenStatementExprDotMethod(CodegenExpression expression, string method, CodegenExpression[] @params)
        {
            this.expression = expression;
            this.method = method;
            this.@params = @params;
        }

        public override void RenderStatement(StringBuilder builder, IDictionary<Type, string> imports, bool isInnerClass)
        {
            if (expression is CodegenExpressionRef)
            {
                expression.Render(builder, imports, isInnerClass);
            }
            else
            {
                builder.Append("(");
                expression.Render(builder, imports, isInnerClass);
                builder.Append(")");
            }
            builder.Append('.').Append(method).Append("(");
            RenderExpressions(builder, @params, imports, isInnerClass);
            builder.Append(")");
        }

        public override void MergeClasses(ISet<Type> classes)
        {
            expression.MergeClasses(classes);
            MergeClassesExpressions(classes, @params);
        }
    }
} // end of namespace