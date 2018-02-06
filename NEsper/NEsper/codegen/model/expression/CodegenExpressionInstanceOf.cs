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

// import static com.espertech.esper.codegen.core.CodeGenerationHelper.appendClassName;

namespace com.espertech.esper.codegen.model.expression
{
    public class CodegenExpressionInstanceOf : ICodegenExpression
    {
        private readonly ICodegenExpression lhs;
        private readonly Type clazz;
        private readonly bool not;

        public CodegenExpressionInstanceOf(ICodegenExpression lhs, Type clazz, bool not)
        {
            this.lhs = lhs;
            this.clazz = clazz;
            this.not = not;
        }

        public void Render(StringBuilder builder, IDictionary<Type, string> imports)
        {
            if (not)
            {
                builder.Append("!(");
            }
            lhs.Render(builder, imports);
            builder.Append(" ").Append("instanceof ");
            CodeGenerationHelper.AppendClassName(builder, clazz, null, imports);
            if (not)
            {
                builder.Append(")");
            }
        }

        public void MergeClasses(ICollection<Type> classes)
        {
            lhs.MergeClasses(classes);
            classes.Add(clazz);
        }
    }
} // end of namespace