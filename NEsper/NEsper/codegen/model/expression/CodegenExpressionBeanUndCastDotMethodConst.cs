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
// import static com.espertech.esper.codegen.model.expression.CodegenExpressionUtil.renderConstant;

namespace com.espertech.esper.codegen.model.expression
{
    public class CodegenExpressionBeanUndCastDotMethodConst : ICodegenExpression
    {
        private readonly Type clazz;
        private readonly ICodegenExpression expression;
        private readonly string method;
        private readonly Object constant;

        public CodegenExpressionBeanUndCastDotMethodConst(Type clazz, ICodegenExpression expression, string method, Object constant)
        {
            this.clazz = clazz;
            this.expression = expression;
            this.method = method;
            this.constant = constant;
        }

        public void Render(StringBuilder builder, IDictionary<Type, string> imports)
        {
            builder.Append("((");
            CodeGenerationHelper.AppendClassName(builder, clazz, null, imports);
            builder.Append(")");
            expression.Render(builder, imports);
            builder.Append(".Underlying).");
            builder.Append(method).Append("(");
            CodegenExpressionUtil.RenderConstant(builder, constant);
            builder.Append(")");
        }

        public void MergeClasses(ICollection<Type> classes)
        {
            expression.MergeClasses(classes);
            classes.Add(clazz);
        }
    }
} // end of namespace