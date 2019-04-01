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

namespace com.espertech.esper.common.@internal.bytecodemodel.model.statement
{
    public class CodegenStatementWhileOrDo : CodegenStatementWBlockBase
    {
        private readonly CodegenExpression condition;
        private readonly bool isWhile;

        public CodegenStatementWhileOrDo(CodegenBlock parent, CodegenExpression condition, bool isWhile) : base(parent)
        {
            this.condition = condition;
            this.isWhile = isWhile;
        }

        public CodegenBlock Block { get; set; }

        public override void Render(
            StringBuilder builder, IDictionary<Type, string> imports, bool isInnerClass, int level,
            CodegenIndent indent)
        {
            if (isWhile) {
                builder.Append("while (");
                condition.Render(builder, imports, isInnerClass);
                builder.Append(") {\n");
            }
            else {
                builder.Append("do {\n");
            }

            Block.Render(builder, imports, isInnerClass, level + 1, indent);
            indent.Indent(builder, level);
            builder.Append("}\n");
            if (!isWhile) {
                indent.Indent(builder, level);
                builder.Append("while (");
                condition.Render(builder, imports, isInnerClass);
                builder.Append(");\n");
            }
        }

        public override void MergeClasses(ISet<Type> classes)
        {
            Block.MergeClasses(classes);
            condition.MergeClasses(classes);
        }
    }
} // end of namespace