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
using com.espertech.esper.common.@internal.bytecodemodel.util;

using static com.espertech.esper.common.@internal.bytecodemodel.core.CodeGenerationHelper;

namespace com.espertech.esper.common.@internal.bytecodemodel.core
{
    public class CodegenPropertyWGraph
    {
        public CodegenPropertyWGraph(
            string name,
            Type returnType,
            string returnTypeName,
            string optionalComment,
            CodegenBlock getterBlock,
            CodegenBlock setterBlock,
            bool isPublic,
            bool isOverride)
        {
            if (returnType == null && returnTypeName == null)
            {
                throw new ArgumentException("Invalid null return type");
            }

            Name = name;
            GetterBlock = getterBlock;
            SetterBlock = setterBlock;
            IsPublic = isPublic;
            IsOverride = isOverride;
            ReturnType = returnType;
            ReturnTypeName = returnTypeName;
            OptionalComment = optionalComment;
        }

        public string Name { get; }

        public CodegenBlock GetterBlock { get; }

        public CodegenBlock SetterBlock { get; }

        public string OptionalComment { get; }

        public Type ReturnType { get; }

        public string ReturnTypeName { get; }

        public bool IsPublic { get; set; }

        public bool IsOverride { get; set; }

        public void MergeClasses(ISet<Type> classes)
        {
            classes.AddToSet(ReturnType);
            GetterBlock.MergeClasses(classes);
            SetterBlock.MergeClasses(classes);
        }

        public void Render(
            StringBuilder builder,
            bool isPublic,
            bool isInnerClass,
            CodegenIndent indent,
            int additionalIndent)
        {
            if (OptionalComment != null) {
                indent.Indent(builder, 1 + additionalIndent);
                builder.Append("// ");
                builder.Append(OptionalComment);
                builder.Append("\n");
            }

            indent.Indent(builder, 1 + additionalIndent);
            if (isPublic) {
                builder.Append("public ");
            }

            if (IsOverride) {
                builder.Append("override ");
            }

            if (ReturnType != null) {
                AppendClassName(builder, ReturnType);
            }
            else {
                builder.Append(ReturnTypeName);
            }

            builder.Append(" ");
            builder.Append(Name);
            builder.Append(" {\n");

            if (GetterBlock.IsNotEmpty()) {
                indent.Indent(builder, additionalIndent + 2);
                builder.Append("get {\n");
                GetterBlock.Render(builder, isInnerClass, 3 + additionalIndent, indent);
                indent.Indent(builder, additionalIndent + 2);
                builder.Append("}\n");
            }

            if (SetterBlock.IsNotEmpty()) {
                indent.Indent(builder, additionalIndent + 2);
                builder.Append("set {\n");
                SetterBlock.Render(builder, isInnerClass, 3 + additionalIndent, indent);
                indent.Indent(builder, additionalIndent + 2);
                builder.Append("}\n");
            }

            indent.Indent(builder, 1 + additionalIndent);
            builder.Append("}\n");
        }
    }
} // end of namespace