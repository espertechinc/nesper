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
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.compat.function;

namespace com.espertech.esper.common.@internal.bytecodemodel.model.statement
{
    public class CodegenStatementAssignArrayElement2Dim : CodegenStatementBase
    {
        private readonly CodegenExpression _array;
        private readonly CodegenExpression _expression;
        private readonly CodegenExpression _indexOne;
        private readonly CodegenExpression _indexTwo;

        public CodegenStatementAssignArrayElement2Dim(
            CodegenExpression array,
            CodegenExpression indexOne,
            CodegenExpression indexTwo,
            CodegenExpression expression)
        {
            _array = array;
            _indexOne = indexOne;
            _indexTwo = indexTwo;
            _expression = expression;
        }

        public override void RenderStatement(
            StringBuilder builder,
            bool isInnerClass)
        {
            var indent = new CodegenIndent(true);
            _array.Render(builder, isInnerClass, 1, indent);
            builder.Append("[");
            _indexOne.Render(builder, isInnerClass, 1, indent);
            builder.Append("][");
            _indexTwo.Render(builder, isInnerClass, 1, indent);
            builder.Append("]=");
            _expression.Render(builder, isInnerClass, 1, indent);
        }

        public override void MergeClasses(ISet<Type> classes)
        {
            _array.MergeClasses(classes);
            _indexOne.MergeClasses(classes);
            _indexTwo.MergeClasses(classes);
            _expression.MergeClasses(classes);
        }

        public override void TraverseExpressions(Consumer<CodegenExpression> consumer)
        {
            consumer.Invoke(_array);
            consumer.Invoke(_indexOne);
            consumer.Invoke(_indexTwo);
            consumer.Invoke(_expression);
        }
    }
} // end of namespace