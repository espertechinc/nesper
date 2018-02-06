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
    public class CodegenExpressionLocalMethod : ICodegenExpression
    {
        private readonly string method;
        private readonly ICodegenExpression expression;

        public CodegenExpressionLocalMethod(string method, ICodegenExpression expression)
        {
            this.method = method;
            this.expression = expression;
        }

        public void Render(StringBuilder builder, IDictionary<Type, string> imports)
        {
            builder.Append(method).Append("(");
            expression.Render(builder, imports);
            builder.Append(")");
        }

        public void MergeClasses(ICollection<Type> classes)
        {
            expression.MergeClasses(classes);
        }
    }
} // end of namespace