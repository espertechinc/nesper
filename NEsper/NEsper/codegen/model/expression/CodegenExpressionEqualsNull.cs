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
    public class CodegenExpressionEqualsNull : ICodegenExpression
    {
        private readonly ICodegenExpression lhs;
        private readonly bool not;

        public CodegenExpressionEqualsNull(ICodegenExpression lhs, bool not)
        {
            this.lhs = lhs;
            this.not = not;
        }

        public void Render(StringBuilder builder, IDictionary<Type, string> imports)
        {
            lhs.Render(builder, imports);
            builder.Append(" ");
            builder.Append(not ? "!=" : "==");
            builder.Append(" null");
        }

        public void MergeClasses(ICollection<Type> classes)
        {
            lhs.MergeClasses(classes);
        }
    }
} // end of namespace