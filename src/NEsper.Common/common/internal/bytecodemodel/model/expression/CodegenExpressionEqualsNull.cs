///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;

using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.compat.function;

namespace com.espertech.esper.common.@internal.bytecodemodel.model.expression
{
    public class CodegenExpressionEqualsNull : CodegenExpression
    {
        private readonly CodegenExpression _lhs;
        private readonly bool _not;

        public CodegenExpressionEqualsNull(
            CodegenExpression lhs,
            bool not)
        {
            _lhs = lhs;
            _not = not;
        }

        public void Render(
            StringBuilder builder,
            bool isInnerClass,
            int level,
            CodegenIndent indent)
        {
            _lhs.Render(builder, isInnerClass, level, indent);
            builder.Append(" ");
            builder.Append(_not ? "!=" : "==");
            builder.Append(" null");
        }

        public void MergeClasses(ISet<Type> classes)
        {
            _lhs.MergeClasses(classes);
        }

        public void TraverseExpressions(Consumer<CodegenExpression> consumer)
        {
            consumer.Invoke(_lhs);
        }
    }
} // end of namespace