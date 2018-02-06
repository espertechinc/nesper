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

// import static com.espertech.esper.codegen.core.CodeGenerationHelper.appendClassName;

namespace com.espertech.esper.codegen.model.statement
{
    public class CodegenStatementDeclareVar : CodegenStatementBase
    {
        private readonly Type clazz;
        private readonly string var;
        private readonly ICodegenExpression initializer;

        public CodegenStatementDeclareVar(Type clazz, string var, ICodegenExpression initializer)
        {
            this.clazz = clazz;
            this.var = var;
            this.initializer = initializer;
        }

        public override void RenderStatement(StringBuilder builder, IDictionary<Type, string> imports)
        {
            CodeGenerationHelper.AppendClassName(builder, clazz, null, imports);
            builder.Append(" ").Append(var).Append("=");
            initializer.Render(builder, imports);
        }

        public override void MergeClasses(ICollection<Type> classes)
        {
            classes.Add(clazz);
            initializer.MergeClasses(classes);
        }
    }
} // end of namespace