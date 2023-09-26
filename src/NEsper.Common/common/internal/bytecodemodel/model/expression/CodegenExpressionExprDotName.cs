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

namespace com.espertech.esper.common.@internal.bytecodemodel.model.expression
{
    public class CodegenExpressionExprDotName : CodegenExpression
    {
        private readonly CodegenExpression _lhs;
        private readonly string _name;

        public CodegenExpressionExprDotName(
            CodegenExpression lhs,
            string name)
        {
            _lhs = lhs;
            _name = name;
        }

        public void Render(
            StringBuilder builder,
            bool isInnerClass,
            int level,
            CodegenIndent indent)
        {
            if (_lhs is CodegenExpressionRef) {
                _lhs.Render(builder, isInnerClass, level, indent);
            }
            else {
                builder.Append("(");
                _lhs.Render(builder, isInnerClass, level, indent);
                builder.Append(")");
            }

            builder.Append('.');
            builder.Append(_name.CodeInclusionName());
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