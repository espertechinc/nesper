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
using com.espertech.esper.util;

namespace com.espertech.esper.codegen.model.expression
{
    public class CodegenExpressionNewArray : ICodegenExpression
    {
        private readonly Type component;
        private readonly ICodegenExpression expression;

        public CodegenExpressionNewArray(Type component, ICodegenExpression expression)
        {
            this.component = component;
            this.expression = expression;
        }

        public void Render(StringBuilder builder, IDictionary<Type, string> imports)
        {
            int numDimensions = TypeHelper.GetNumberOfDimensions(component);
            Type outermostType = TypeHelper.GetComponentTypeOutermost(component);
            builder.Append("new ");
            CodeGenerationHelper.AppendClassName(builder, outermostType, null, imports);
            builder.Append("[");
            expression.Render(builder, imports);
            builder.Append("]");
            for (int i = 0; i < numDimensions; i++)
            {
                builder.Append("[]");
            }
        }

        public void MergeClasses(ICollection<Type> classes)
        {
            classes.Add(component);
            expression.MergeClasses(classes);
        }
    }
} // end of namespace