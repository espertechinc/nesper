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
using com.espertech.esper.compat.function;

namespace com.espertech.esper.common.@internal.bytecodemodel.model.expression
{
    public class CodegenExpressionEqualsReference : CodegenExpression
    {
        private readonly bool _isNot;
        private readonly CodegenExpression _lhs;
        private readonly CodegenExpression _rhs;

        public CodegenExpressionEqualsReference(
            CodegenExpression lhs,
            CodegenExpression rhs,
            bool isNot)
        {
            _lhs = lhs;
            _rhs = rhs;
            _isNot = isNot;
        }

        public void Render(
            StringBuilder builder,
            bool isInnerClass,
            int level,
            CodegenIndent indent)
        {
            builder.Append("(");
            _lhs.Render(builder, isInnerClass, level, indent);
            builder.Append(_isNot ? "!=" : "==");
            _rhs.Render(builder, isInnerClass, level, indent);
            builder.Append(")");
        }

        public void MergeClasses(ISet<Type> classes)
        {
            _lhs.MergeClasses(classes);
            _rhs.MergeClasses(classes);
        }

        public void TraverseExpressions(Consumer<CodegenExpression> consumer)
        {
            consumer.Invoke(_lhs);
            consumer.Invoke(_rhs);
        }
    }
} // end of namespace