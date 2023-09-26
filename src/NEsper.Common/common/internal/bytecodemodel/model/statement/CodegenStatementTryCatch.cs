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

using static com.espertech.esper.common.@internal.bytecodemodel.core.CodeGenerationHelper;

namespace com.espertech.esper.common.@internal.bytecodemodel.model.statement
{
    public class CodegenStatementTryCatch : CodegenStatementWBlockBase
    {
        private readonly IList<CodegenStatementTryCatchCatchBlock> _catchBlocks =
            new List<CodegenStatementTryCatchCatchBlock>(1);

        private CodegenBlock _finallyBlock;
        private CodegenBlock _tryBlock;

        public CodegenStatementTryCatch(CodegenBlock parent)
            : base(parent)
        {
        }

        public CodegenBlock Try {
            get => _tryBlock;
            set {
                if (_tryBlock != null) {
                    throw new IllegalStateException("Try-block already provided");
                }

                _tryBlock = value;
            }
        }

        public CodegenBlock AddCatch(
            Type ex,
            string name)
        {
            var block = new CodegenBlock(this);
            _catchBlocks.Add(new CodegenStatementTryCatchCatchBlock(ex, name, block));
            return block;
        }

        public CodegenBlock TryFinally()
        {
            if (_finallyBlock != null) {
                throw new IllegalStateException("Finally already set");
            }

            _finallyBlock = new CodegenBlock(this);
            return _finallyBlock;
        }

        public override void Render(
            StringBuilder builder,
            bool isInnerClass,
            int level,
            CodegenIndent indent)
        {
            builder.Append("try {\n");
            _tryBlock.Render(builder, isInnerClass, level + 1, indent);
            indent.Indent(builder, level);
            builder.Append("}");

            var delimiter = "";
            foreach (var pair in _catchBlocks) {
                builder.Append(delimiter);
                builder.Append(" catch (");
                AppendClassName(builder, pair.Ex);
                builder.Append(' ');
                builder.Append(pair.Name);
                builder.Append(") {\n");
                pair.Block.Render(builder, isInnerClass, level + 1, indent);
                indent.Indent(builder, level);
                builder.Append("}");
                delimiter = "\n";
            }

            if (_finallyBlock != null) {
                builder.Append("\n");
                indent.Indent(builder, level);
                builder.Append("finally {\n");
                _finallyBlock.Render(builder, isInnerClass, level + 1, indent);
                indent.Indent(builder, level);
                builder.Append("}");
            }

            builder.Append("\n");
        }

        public override void MergeClasses(ISet<Type> classes)
        {
            _tryBlock.MergeClasses(classes);
            foreach (var pair in _catchBlocks) {
                pair.MergeClasses(classes);
            }

            _finallyBlock?.MergeClasses(classes);
        }

        public override void TraverseExpressions(Consumer<CodegenExpression> consumer)
        {
            _tryBlock.TraverseExpressions(consumer);
            foreach (var pair in _catchBlocks) {
                pair.TraverseExpressions(consumer);
            }

            _finallyBlock?.TraverseExpressions(consumer);
        }
    }
} // end of namespace