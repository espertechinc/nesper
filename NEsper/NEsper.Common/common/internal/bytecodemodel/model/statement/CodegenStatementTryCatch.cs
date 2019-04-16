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
using com.espertech.esper.compat;
using static com.espertech.esper.common.@internal.bytecodemodel.core.CodeGenerationHelper;

namespace com.espertech.esper.common.@internal.bytecodemodel.model.statement
{
    public class CodegenStatementTryCatch : CodegenStatementWBlockBase
    {
        private readonly IList<CodegenStatementTryCatchCatchBlock> catchBlocks =
            new List<CodegenStatementTryCatchCatchBlock>(1);

        private CodegenBlock finallyBlock;
        private CodegenBlock tryBlock;

        public CodegenStatementTryCatch(CodegenBlock parent)
            : base(parent)
        {
        }

        public CodegenBlock Try {
            get => tryBlock;
            set {
                if (tryBlock != null) {
                    throw new IllegalStateException("Try-block already provided");
                }

                tryBlock = value;
            }
        }

        public CodegenBlock AddCatch(
            Type ex,
            string name)
        {
            var block = new CodegenBlock(this);
            catchBlocks.Add(new CodegenStatementTryCatchCatchBlock(ex, name, block));
            return block;
        }

        public CodegenBlock TryFinally()
        {
            if (finallyBlock != null) {
                throw new IllegalStateException("Finally already set");
            }

            finallyBlock = new CodegenBlock(this);
            return finallyBlock;
        }

        public override void Render(
            StringBuilder builder,
            IDictionary<Type, string> imports,
            bool isInnerClass,
            int level,
            CodegenIndent indent)
        {
            builder.Append("try {\n");
            tryBlock.Render(builder, imports, isInnerClass, level + 1, indent);
            indent.Indent(builder, level);
            builder.Append("}");

            var delimiter = "";
            foreach (var pair in catchBlocks) {
                builder.Append(delimiter);
                builder.Append(" catch (");
                AppendClassName(builder, pair.Ex, null, imports);
                builder.Append(' ');
                builder.Append(pair.Name);
                builder.Append(") {\n");
                pair.Block.Render(builder, imports, isInnerClass, level + 1, indent);
                indent.Indent(builder, level);
                builder.Append("}");
                delimiter = "\n";
            }

            if (finallyBlock != null) {
                builder.Append("\n");
                indent.Indent(builder, level);
                builder.Append("finally {\n");
                finallyBlock.Render(builder, imports, isInnerClass, level + 1, indent);
                indent.Indent(builder, level);
                builder.Append("}");
            }

            builder.Append("\n");
        }

        public override void MergeClasses(ISet<Type> classes)
        {
            tryBlock.MergeClasses(classes);
            foreach (var pair in catchBlocks) {
                pair.MergeClasses(classes);
            }

            if (finallyBlock != null) {
                finallyBlock.MergeClasses(classes);
            }
        }
    }
} // end of namespace