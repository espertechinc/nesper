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

//import static com.espertech.esper.codegen.core.CodeGenerationHelper.appendClassName;

namespace com.espertech.esper.codegen.model.expression
{
    public class CodegenExpressionStaticMethodTakingRefs : ICodegenExpression
    {
        private readonly Type target;
        private readonly string methodName;
        private readonly string[] refs;

        public CodegenExpressionStaticMethodTakingRefs(Type target, string methodName, string[] refs)
        {
            this.target = target;
            this.methodName = methodName;
            this.refs = refs;
        }

        public void Render(StringBuilder builder, IDictionary<Type, string> imports)
        {
            CodeGenerationHelper.AppendClassName(builder, target, null, imports);
            builder.Append(".");
            builder.Append(methodName);
            builder.Append("(");
            string delimiter = "";
            foreach (string parameter in refs)
            {
                builder.Append(delimiter);
                builder.Append(parameter);
                delimiter = ",";
            }
            builder.Append(")");
        }

        public void MergeClasses(ICollection<Type> classes)
        {
            classes.Add(target);
        }
    }
} // end of namespace