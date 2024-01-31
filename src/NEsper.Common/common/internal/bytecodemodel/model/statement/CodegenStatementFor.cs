///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.compat.function;

using static com.espertech.esper.common.@internal.bytecodemodel.core.CodeGenerationHelper;

namespace com.espertech.esper.common.@internal.bytecodemodel.model.statement
{
    public class CodegenStatementFor : CodegenStatementWBlockBase
    {
        private readonly CodegenExpression _increment;
        private readonly CodegenExpression _initialization;
        private readonly string _name;
        private readonly CodegenExpression _termination;
        private readonly Type _type;

        public CodegenStatementFor(
            CodegenBlock parent,
            Type type,
            string name,
            CodegenExpression initialization,
            CodegenExpression termination,
            CodegenExpression increment)
            : base(parent)
        {
            _type = type;
            _name = name;
            _initialization = initialization;
            _termination = termination;
            _increment = increment;
        }

        public CodegenBlock Block { get; set; }

        public override void Render(
            StringBuilder builder,
            bool isInnerClass,
            int level,
            CodegenIndent indent)
        {
            builder.Append("for (");
            AppendClassName(builder, _type);
            builder.Append(" ").Append(_name).Append("=");
            _initialization.Render(builder, isInnerClass, level, indent);
            builder.Append("; ");
            _termination.Render(builder, isInnerClass, level, indent);
            builder.Append("; ");
            _increment.Render(builder, isInnerClass, level, indent);
            builder.Append(") {\n");
            Block.Render(builder, isInnerClass, level + 1, indent);
            indent.Indent(builder, level);
            builder.Append("}\n");
        }

        public override void MergeClasses(ISet<Type> classes)
        {
            Block.MergeClasses(classes);
            _initialization.MergeClasses(classes);
            _termination.MergeClasses(classes);
            _increment.MergeClasses(classes);
        }

        public override void TraverseExpressions(Consumer<CodegenExpression> consumer)
        {
            Block.TraverseExpressions(consumer);
            consumer.Invoke(_initialization);
            consumer.Invoke(_termination);
            consumer.Invoke(_increment);
        }
    }
} // end of namespace