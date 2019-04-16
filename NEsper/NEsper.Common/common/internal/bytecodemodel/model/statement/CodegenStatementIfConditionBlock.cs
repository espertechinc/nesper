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
    public class CodegenStatementIfConditionBlock
    {
        private readonly CodegenExpression condition;
        private readonly CodegenBlock block;

        public CodegenStatementIfConditionBlock(
            CodegenExpression condition,
            CodegenBlock block)
        {
            this.condition = condition;
            this.block = block;
        }

        public CodegenExpression Condition {
            get => condition;
        }

        public CodegenBlock Block {
            get => block;
        }

        public void MergeClasses(ISet<Type> classes)
        {
            condition.MergeClasses(classes);
            block.MergeClasses(classes);
        }

        public void Render(
            StringBuilder builder,
            IDictionary<Type, string> imports,
            bool isInnerClass,
            int level,
            CodegenIndent indent)
        {
            builder.Append("if (");
            condition.Render(builder, imports, isInnerClass);
            builder.Append(") {\n");
            block.Render(builder, imports, isInnerClass, level + 1, indent);
            indent.Indent(builder, level);
            builder.Append("}");
        }
    }
} // end of namespace