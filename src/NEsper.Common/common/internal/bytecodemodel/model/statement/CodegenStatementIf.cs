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
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;

namespace com.espertech.esper.common.@internal.bytecodemodel.model.statement
{
    public class CodegenStatementIf : CodegenStatementWBlockBase
    {
        private readonly IList<CodegenStatementIfConditionBlock> _blocks =
            new List<CodegenStatementIfConditionBlock>();

        private CodegenBlock _optionalElse;

        public CodegenStatementIf(CodegenBlock parent)
            : base(parent)
        {
        }

        public CodegenBlock IfBlock(CodegenExpression condition)
        {
            if (!_blocks.IsEmpty()) {
                throw new IllegalStateException("Use add-else instead");
            }

            var block = new CodegenBlock(this);
            _blocks.Add(new CodegenStatementIfConditionBlock(condition, block));
            return block;
        }

        public CodegenBlock AddElseIf(CodegenExpression condition)
        {
            if (_blocks.IsEmpty()) {
                throw new IllegalStateException("Use if-block instead");
            }

            var block = new CodegenBlock(this);
            _blocks.Add(new CodegenStatementIfConditionBlock(condition, block));
            return block;
        }

        public CodegenBlock AddElse()
        {
            if (_blocks.IsEmpty()) {
                throw new IllegalStateException("Use if-block instead");
            }

            if (_optionalElse != null) {
                throw new IllegalStateException("Else already found");
            }

            _optionalElse = new CodegenBlock(this);
            return _optionalElse;
        }

        public override void Render(
            StringBuilder builder,
            bool isInnerClass,
            int level,
            CodegenIndent indent)
        {
            var enumerator = _blocks.GetEnumerator();
            enumerator.MoveNext();

            var first = enumerator.Current;
            first.Render(builder, isInnerClass, level, indent);

            while (enumerator.MoveNext()) {
                builder.Append(" else ");
                enumerator.Current.Render(builder, isInnerClass, level, indent);
            }

            if (_optionalElse != null) {
                builder.Append(" else {\n");
                _optionalElse.Render(builder, isInnerClass, level + 1, indent);
                indent.Indent(builder, level);
                builder.Append("}");
            }

            builder.Append("\n");
        }

        public override void MergeClasses(ISet<Type> classes)
        {
            foreach (var pair in _blocks) {
                pair.MergeClasses(classes);
            }

            _optionalElse?.MergeClasses(classes);
        }

        public override void TraverseExpressions(Consumer<CodegenExpression> consumer)
        {
            foreach (var pair in _blocks) {
                pair.TraverseExpressions(consumer);
            }

            _optionalElse?.TraverseExpressions(consumer);
        }
    }
} // end of namespace