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

namespace com.espertech.esper.common.@internal.bytecodemodel.model.statement
{
    public class CodegenStatementIf : CodegenStatementWBlockBase
    {
        private readonly IList<CodegenStatementIfConditionBlock> blocks = new List<CodegenStatementIfConditionBlock>();
        private CodegenBlock optionalElse;

        public CodegenStatementIf(CodegenBlock parent)
            : base(parent)
        {
        }

        public CodegenBlock IfBlock(CodegenExpression condition)
        {
            if (!blocks.IsEmpty()) {
                throw new IllegalStateException("Use add-else instead");
            }

            var block = new CodegenBlock(this);
            blocks.Add(new CodegenStatementIfConditionBlock(condition, block));
            return block;
        }

        public CodegenBlock AddElseIf(CodegenExpression condition)
        {
            if (blocks.IsEmpty()) {
                throw new IllegalStateException("Use if-block instead");
            }

            var block = new CodegenBlock(this);
            blocks.Add(new CodegenStatementIfConditionBlock(condition, block));
            return block;
        }

        public CodegenBlock AddElse()
        {
            if (blocks.IsEmpty()) {
                throw new IllegalStateException("Use if-block instead");
            }

            if (optionalElse != null) {
                throw new IllegalStateException("Else already found");
            }

            optionalElse = new CodegenBlock(this);
            return optionalElse;
        }

        public override void Render(
            StringBuilder builder,
            IDictionary<Type, string> imports,
            bool isInnerClass,
            int level,
            CodegenIndent indent)
        {
            var enumerator = blocks.GetEnumerator();
            enumerator.MoveNext();

            var first = enumerator.Current;
            first.Render(builder, imports, isInnerClass, level, indent);

            while (enumerator.MoveNext()) {
                builder.Append(" else ");
                enumerator.Current.Render(builder, imports, isInnerClass, level, indent);
            }

            if (optionalElse != null) {
                builder.Append(" else {\n");
                optionalElse.Render(builder, imports, isInnerClass, level + 1, indent);
                indent.Indent(builder, level);
                builder.Append("}");
            }

            builder.Append("\n");
        }

        public override void MergeClasses(ISet<Type> classes)
        {
            foreach (var pair in blocks) {
                pair.MergeClasses(classes);
            }

            if (optionalElse != null) {
                optionalElse.MergeClasses(classes);
            }
        }
    }
} // end of namespace