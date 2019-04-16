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

namespace com.espertech.esper.common.@internal.bytecodemodel.model.statement
{
    public class CodegenStatementSynchronized : CodegenStatementWBlockBase
    {
        private CodegenBlock block;
        private readonly CodegenExpression expression;

        public CodegenStatementSynchronized(
            CodegenBlock parent,
            CodegenExpression expression)
            : base(parent)
        {
            this.expression = expression;
        }

        public override void Render(
            StringBuilder builder,
            IDictionary<Type, string> imports,
            bool isInnerClass,
            int level,
            CodegenIndent indent)
        {
            builder.Append("synchronized (");
            expression.Render(builder, imports, isInnerClass);
            builder.Append(") {\n");
            block.Render(builder, imports, isInnerClass, level + 1, indent);
            indent.Indent(builder, level);
            builder.Append("}\n");
        }

        public override void MergeClasses(ISet<Type> classes)
        {
            expression.MergeClasses(classes);
            block.MergeClasses(classes);
        }

        public CodegenBlock MakeBlock()
        {
            if (block != null) {
                throw new IllegalStateException("Block already allocated");
            }

            block = new CodegenBlock(this);
            return block;
        }
    }
} // end of namespace