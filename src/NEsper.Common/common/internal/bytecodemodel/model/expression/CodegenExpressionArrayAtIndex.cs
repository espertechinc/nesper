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
    public class CodegenExpressionArrayAtIndex : CodegenExpression
    {
        private readonly CodegenExpression _expression;
        private readonly CodegenExpression _index;

        public CodegenExpressionArrayAtIndex(
            CodegenExpression expression,
            CodegenExpression index)
        {
            _expression = expression;
            _index = index;
        }

        public void Render(
            StringBuilder builder,
            bool isInnerClass,
            int level,
            CodegenIndent indent)
        {
            _expression.Render(builder, isInnerClass, level, indent);
            builder.Append("[");
            _index.Render(builder, isInnerClass, level, indent);
            builder.Append("]");
        }

        public void MergeClasses(ISet<Type> classes)
        {
            _expression.MergeClasses(classes);
            _index.MergeClasses(classes);
        }
        
        public void TraverseExpressions(Consumer<CodegenExpression> consumer) {
            consumer.Invoke(_expression);
            consumer.Invoke(_index);
        }
    }
} // end of namespace