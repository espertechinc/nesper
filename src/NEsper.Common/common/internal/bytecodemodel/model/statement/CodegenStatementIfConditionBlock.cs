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

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.compat.function;

namespace com.espertech.esper.common.@internal.bytecodemodel.model.statement
{
    public class CodegenStatementIfConditionBlock
    {
        private readonly CodegenExpression _condition;
        private readonly CodegenBlock _block;

        public CodegenStatementIfConditionBlock(
            CodegenExpression condition,
            CodegenBlock block)
        {
            _condition = condition;
            _block = block;
        }

        public CodegenExpression Condition {
            get => _condition;
        }

        public CodegenBlock Block {
            get => _block;
        }

        public void MergeClasses(ISet<Type> classes)
        {
            _condition.MergeClasses(classes);
            _block.MergeClasses(classes);
        }

        public void Render(
            StringBuilder builder,
            bool isInnerClass,
            int level,
            CodegenIndent indent)
        {
            builder.Append("if (");
            _condition.Render(builder, isInnerClass, level, indent);
            builder.Append(") {\n");
            _block.Render(builder, isInnerClass, level + 1, indent);
            indent.Indent(builder, level);
            builder.Append("}");
        }

        public void TraverseExpressions(Consumer<CodegenExpression> consumer)
        {
            consumer.Invoke(_condition);
            _block.TraverseExpressions(consumer);
        }
    }
} // end of namespace