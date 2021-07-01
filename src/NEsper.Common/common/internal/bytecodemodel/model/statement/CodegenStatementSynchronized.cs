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
using com.espertech.esper.compat;
using com.espertech.esper.compat.function;

namespace com.espertech.esper.common.@internal.bytecodemodel.model.statement
{
    public class CodegenStatementSynchronized : CodegenStatementWBlockBase
    {
        private CodegenBlock _block;
        private readonly CodegenExpression _expression;

        public CodegenStatementSynchronized(
            CodegenBlock parent,
            CodegenExpression expression)
            : base(parent)
        {
            _expression = expression;
        }

        public override void Render(
            StringBuilder builder,
            bool isInnerClass,
            int level,
            CodegenIndent indent)
        {
            builder.Append("lock (");
            _expression.Render(builder, isInnerClass, level + 1, indent);
            builder.Append(") {\n");
            _block.Render(builder, isInnerClass, level + 1, indent);
            indent.Indent(builder, level);
            builder.Append("}\n");
        }

        public override void MergeClasses(ISet<Type> classes)
        {
            _expression.MergeClasses(classes);
            _block.MergeClasses(classes);
        }

        public CodegenBlock MakeBlock()
        {
            if (_block != null) {
                throw new IllegalStateException("Block already allocated");
            }

            _block = new CodegenBlock(this);
            return _block;
        }
        
        public override void TraverseExpressions(Consumer<CodegenExpression> consumer)
        {
            consumer.Invoke(_expression);
            _block.TraverseExpressions(consumer);
        }
    }
} // end of namespace