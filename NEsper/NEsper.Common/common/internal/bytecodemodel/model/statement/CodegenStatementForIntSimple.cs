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
using com.espertech.esper.compat.function;

namespace com.espertech.esper.common.@internal.bytecodemodel.model.statement
{
    public class CodegenStatementForIntSimple : CodegenStatementWBlockBase
    {
        private readonly string _ref;
        private readonly CodegenExpression _upperLimit;

        public CodegenStatementForIntSimple(
            CodegenBlock parent,
            string @ref,
            CodegenExpression upperLimit)
            : base(
                parent)
        {
            _ref = @ref;
            _upperLimit = upperLimit;
        }

        public CodegenBlock Block { get; set; }

        public override void Render(
            StringBuilder builder,
            bool isInnerClass,
            int level,
            CodegenIndent indent)
        {
            builder
                .Append("for (int ")
                .Append(_ref)
                .Append("=0; ")
                .Append(_ref)
                .Append("<");

            _upperLimit.Render(builder, isInnerClass, level, indent);

            builder
                .Append("; ")
                .Append(_ref)
                .Append("++) {\n");

            Block.Render(builder, isInnerClass, level + 1, indent);
            indent.Indent(builder, level);
            builder.Append("}\n");
        }

        public override void MergeClasses(ISet<Type> classes)
        {
            Block.MergeClasses(classes);
            _upperLimit.MergeClasses(classes);
        }
        
        public void TraverseExpressions(Consumer<CodegenExpression> consumer)
        {
            consumer.Invoke(_upperLimit);
            Block.TraverseExpressions(consumer);
        }
    }
} // end of namespace