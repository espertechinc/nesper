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
using static com.espertech.esper.common.@internal.bytecodemodel.core.CodeGenerationHelper;

namespace com.espertech.esper.common.@internal.bytecodemodel.model.expression
{
    public class CodegenExpressionClass : CodegenExpression
    {
        private readonly Type target;

        public CodegenExpressionClass(Type target)
        {
            if (target == null) {
                throw new ArgumentException("Invalid null target");
            }

            this.target = target;
        }

        public void Render(
            StringBuilder builder,
            IDictionary<Type, string> imports,
            bool isInnerClass)
        {
            RenderClass(target, builder, imports);
        }

        public void MergeClasses(ISet<Type> classes)
        {
            classes.Add(target);
        }

        public static void RenderClass(
            Type clazz,
            StringBuilder builder,
            IDictionary<Type, string> imports)
        {
            AppendClassName(builder, clazz, null, imports);
            builder.Append(".");
            builder.Append("class");
        }
    }
} // end of namespace