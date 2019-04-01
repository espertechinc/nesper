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
    public class CodegenStatementForIntSimple : CodegenStatementWBlockBase
    {
        private readonly string @ref;
        private readonly CodegenExpression upperLimit;

        public CodegenStatementForIntSimple(CodegenBlock parent, string @ref, CodegenExpression upperLimit) : base(
            parent)
        {
            this.@ref = @ref;
            this.upperLimit = upperLimit;
        }

        public CodegenBlock Block { get; set; }

        public override void Render(
            StringBuilder builder, IDictionary<Type, string> imports, bool isInnerClass, int level,
            CodegenIndent indent)
        {
            builder.Append("for (int ").Append(@ref).Append("=0; ").Append(@ref).Append("<");
            upperLimit.Render(builder, imports, isInnerClass);
            builder.Append("; ").Append(@ref).Append("++) {\n");
            Block.Render(builder, imports, isInnerClass, level + 1, indent);
            indent.Indent(builder, level);
            builder.Append("}\n");
        }

        public override void MergeClasses(ISet<Type> classes)
        {
            Block.MergeClasses(classes);
            upperLimit.MergeClasses(classes);
        }
    }
} // end of namespace