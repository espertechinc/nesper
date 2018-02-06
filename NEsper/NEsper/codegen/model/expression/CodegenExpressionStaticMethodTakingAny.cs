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
// import static com.espertech.esper.codegen.model.expression.CodegenExpressionBuilder.renderExpressions;

namespace com.espertech.esper.codegen.model.expression
{
    public class CodegenExpressionStaticMethodTakingAny : ICodegenExpression
    {
        private readonly Type target;
        private readonly string methodName;
        private readonly ICodegenExpression[] parameters;

        public CodegenExpressionStaticMethodTakingAny(Type target, string methodName, ICodegenExpression[] parameters)
        {
            this.target = target;
            this.methodName = methodName;
            this.parameters = parameters;
        }

        public void Render(StringBuilder builder, IDictionary<Type, string> imports)
        {
            CodeGenerationHelper.AppendClassName(builder, target, null, imports);
            builder.Append(".");
            builder.Append(methodName);
            builder.Append("(");
            CodegenExpressionBuilder.RenderExpressions(builder, parameters, imports);
            builder.Append(")");
        }

        public void MergeClasses(ICollection<Type> classes)
        {
            classes.Add(target);
            CodegenExpressionBuilder.MergeClassesExpressions(classes, parameters);
        }
    }
} // end of namespace