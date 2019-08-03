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
using com.espertech.esper.common.@internal.bytecodemodel.util;

using static com.espertech.esper.common.@internal.bytecodemodel.core.CodeGenerationHelper;

namespace com.espertech.esper.common.@internal.bytecodemodel.model.statement
{
    public class CodegenStatementForEach : CodegenStatementWBlockBase
    {
        private readonly string @ref;
        private readonly CodegenExpression target;
        private readonly Type type;

        public CodegenStatementForEach(
            CodegenBlock parent,
            Type type,
            string @ref,
            CodegenExpression target)
            : base(parent)
        {
            this.type = type;
            this.@ref = @ref;
            this.target = target;
        }

        public CodegenBlock Block { get; set; }

        public override void Render(
            StringBuilder builder,
            bool isInnerClass,
            int level,
            CodegenIndent indent)
        {
            builder.Append("foreach (");
            AppendClassName(builder, type);
            builder.Append(" ").Append(@ref).Append(" in ");
            target.Render(builder, isInnerClass, level, indent);
            builder.Append(") {\n");
            Block.Render(builder, isInnerClass, level + 1, indent);
            indent.Indent(builder, level);
            builder.Append("}\n");
        }

        public override void MergeClasses(ISet<Type> classes)
        {
            classes.AddToSet(type);
            Block.MergeClasses(classes);
            target.MergeClasses(classes);
        }
    }
} // end of namespace