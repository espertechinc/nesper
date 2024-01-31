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

using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.compat.function;

namespace com.espertech.esper.common.@internal.bytecodemodel.model.statement
{
    public class CodegenStatementAssignCompound : CodegenStatementBase
    {
        private readonly CodegenExpression _assignment;
        private readonly CodegenExpression _lhs;
        private readonly string _operator;

        public CodegenStatementAssignCompound(
            CodegenExpression lhs,
            string @operator,
            CodegenExpression assignment)
        {
            _lhs = lhs;
            _operator = @operator;
            _assignment = assignment;
        }

        public override void RenderStatement(
            StringBuilder builder,
            bool isInnerClass)
        {
            var indent = new CodegenIndent(true);
            _lhs.Render(builder, isInnerClass, 1, indent);
            builder.Append(_operator);
            builder.Append("=");
            _assignment.Render(builder, isInnerClass, 1, indent);
        }

        public override void MergeClasses(ISet<Type> classes)
        {
            _assignment.MergeClasses(classes);
        }

        public override void TraverseExpressions(Consumer<CodegenExpression> consumer)
        {
            consumer.Invoke(_lhs);
            consumer.Invoke(_assignment);
        }
    }
} // end of namespace