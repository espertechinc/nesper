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

using static com.espertech.esper.common.@internal.bytecodemodel.core.CodeGenerationHelper;

namespace com.espertech.esper.common.@internal.bytecodemodel.model.statement
{
    public class CodegenStatementFor : CodegenStatementWBlockBase
    {
        private readonly CodegenExpression increment;
        private readonly CodegenExpression initialization;
        private readonly string name;
        private readonly CodegenExpression termination;
        private readonly Type type;

        public CodegenStatementFor(
            CodegenBlock parent,
            Type type,
            string name,
            CodegenExpression initialization,
            CodegenExpression termination,
            CodegenExpression increment)
            : base(parent)
        {
            this.type = type;
            this.name = name;
            this.initialization = initialization;
            this.termination = termination;
            this.increment = increment;
        }

        public CodegenBlock Block { get; set; }

        public override void Render(
            StringBuilder builder,
            bool isInnerClass,
            int level,
            CodegenIndent indent)
        {
            builder.Append("for (");
            AppendClassName(builder, type);
            builder.Append(" ").Append(name).Append("=");
            initialization.Render(builder, isInnerClass);
            builder.Append("; ");
            termination.Render(builder, isInnerClass);
            builder.Append("; ");
            increment.Render(builder, isInnerClass);
            builder.Append(") {\n");
            Block.Render(builder, isInnerClass, level + 1, indent);
            indent.Indent(builder, level);
            builder.Append("}\n");
        }

        public override void MergeClasses(ISet<Type> classes)
        {
            Block.MergeClasses(classes);
            initialization.MergeClasses(classes);
            termination.MergeClasses(classes);
            increment.MergeClasses(classes);
        }
    }
} // end of namespace