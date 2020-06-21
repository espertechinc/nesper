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

using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.util;
using com.espertech.esper.compat.function;

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
            bool isInnerClass,
            int level,
            CodegenIndent indent)
        {
            RenderClass(target, builder);
        }

        public void MergeClasses(ISet<Type> classes)
        {
            classes.AddToSet(target);
        }

        public static void RenderClass(
            Type clazz,
            StringBuilder builder)
        {
            builder.Append("typeof(");
            AppendClassName(builder, clazz);
            builder.Append(")");
        }
        
        public void TraverseExpressions(Consumer<CodegenExpression> consumer)
        {
        }
    }
} // end of namespace