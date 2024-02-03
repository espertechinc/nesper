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

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.compat.function;

namespace com.espertech.esper.common.@internal.bytecodemodel.model.statement
{
    public class CodegenStatementWhileOrDo : CodegenStatementWBlockBase
    {
        private CodegenBlock _block;
        private readonly CodegenExpression _condition;
        private readonly bool _isWhile;

        public CodegenStatementWhileOrDo(
            CodegenBlock parent,
            CodegenExpression condition,
            bool isWhile)
            : base(parent)
        {
            _condition = condition;
            _isWhile = isWhile;
        }

        public CodegenBlock Block {
            get => _block;
            set => _block = value;
        }

        public override void Render(
            StringBuilder builder,
            bool isInnerClass,
            int level,
            CodegenIndent indent)
        {
            if (_isWhile) {
                builder.Append("while (");
                _condition.Render(builder, isInnerClass, level + 1, indent);
                builder.Append(") {\n");
            }
            else {
                builder.Append("do {\n");
            }

            _block.Render(builder, isInnerClass, level + 1, indent);
            indent.Indent(builder, level);
            builder.Append("}\n");
            if (!_isWhile) {
                indent.Indent(builder, level);
                builder.Append("while (");
                _condition.Render(builder, isInnerClass, level + 1, indent);
                builder.Append(");\n");
            }
        }

        public override void MergeClasses(ISet<Type> classes)
        {
            Block.MergeClasses(classes);
            _condition.MergeClasses(classes);
        }

        public override void TraverseExpressions(Consumer<CodegenExpression> consumer)
        {
            _block.TraverseExpressions(consumer);
            consumer.Invoke(_condition);
        }
    }
} // end of namespace