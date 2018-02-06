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

namespace com.espertech.esper.codegen.model.expression
{
    public class CodegenExpressionNewInstance : ICodegenExpression
    {
        private readonly Type clazz;
        private readonly ICodegenExpression[] parameters;

        public CodegenExpressionNewInstance(Type clazz, ICodegenExpression[] parameters)
        {
            this.clazz = clazz;
            this.parameters = parameters;
        }

        public void Render(StringBuilder builder, IDictionary<Type, string> imports)
        {
            builder.Append("new ");
            CodeGenerationHelper.AppendClassName(builder, clazz, null, imports);
            builder.Append("(");
            CodegenExpressionBuilder.RenderExpressions(builder, parameters, imports);
            builder.Append(")");
        }

        public void MergeClasses(ICollection<Type> classes)
        {
            classes.Add(clazz);
            CodegenExpressionBuilder.MergeClassesExpressions(classes, parameters);
        }
    }
} // end of namespace