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
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.core.CodeGenerationHelper;

namespace com.espertech.esper.common.@internal.bytecodemodel.core
{
    public class CodegenMethodWGraph
    {
        private readonly IList<Type> thrown;

        public CodegenMethodWGraph(
            string name,
            CodegenMethodFootprint footprint,
            CodegenBlock block,
            bool isPublic,
            IList<Type> thrown)
        {
            Name = name;
            Footprint = footprint;
            Block = block;
            IsPublic = isPublic;
            this.thrown = thrown;
        }

        public string Name { get; }

        public CodegenMethodFootprint Footprint { get; }

        public CodegenBlock Block { get; }

        public bool IsPublic { get; set; }

        public bool IsStatic { get; set; }

        public void MergeClasses(ISet<Type> classes)
        {
            Footprint.MergeClasses(classes);
            Block.MergeClasses(classes);
        }

        public void Render(
            StringBuilder builder,
            bool isPublic,
            bool isInnerClass,
            CodegenIndent indent,
            int additionalIndent)
        {
            if (Footprint.OptionalComment != null) {
                indent.Indent(builder, 1 + additionalIndent);
                builder.Append("// ").Append(Footprint.OptionalComment).Append("\n");
            }

            indent.Indent(builder, 1 + additionalIndent);
            if (isPublic) {
                builder.Append("public ");
            }

            if (IsStatic) {
                builder.Append("static ");
            }

            if (Footprint.ReturnType != null) {
                AppendClassName(builder, Footprint.ReturnType);
            }
            else {
                builder.Append(Footprint.ReturnTypeName);
            }

            builder.Append(" ").Append(Name);
            builder.Append("(");
            var delimiter = "";
            foreach (var param in Footprint.Params) {
                builder.Append(delimiter);
                param.Render(builder);
                delimiter = ",";
            }

            builder.Append(")");
            if (!thrown.IsEmpty()) {
                builder.Append(" throws ");
                var delimiterThrown = "";
                foreach (var ex in thrown) {
                    builder.Append(delimiterThrown);
                    AppendClassName(builder, ex);
                    delimiterThrown = ",";
                }
            }

            builder.Append("{\n");
            Block.Render(builder, isInnerClass, 2 + additionalIndent, indent);
            indent.Indent(builder, 1 + additionalIndent);
            builder.Append("}\n");
        }

        public CodegenMethodWGraph SetStatic(bool aStatic)
        {
            IsStatic = aStatic;
            return this;
        }
    }
} // end of namespace