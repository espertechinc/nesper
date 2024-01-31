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

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionUtil;

namespace com.espertech.esper.common.@internal.bytecodemodel.model.statement
{
    public class CodegenStatementIfConditionReturnConst : CodegenStatementBase
    {
        private readonly CodegenExpression _condition;
        private readonly object _constant;

        public CodegenStatementIfConditionReturnConst(
            CodegenExpression condition,
            object constant)
        {
            _condition = condition;
            _constant = constant;
        }

        public override void RenderStatement(
            StringBuilder builder,
            bool isInnerClass)
        {
            builder.Append("if (");
            _condition.Render(builder, isInnerClass, 4, new CodegenIndent(true));
            builder.Append(") return ");
            RenderConstant(builder, _constant);
        }

        public override void MergeClasses(ISet<Type> classes)
        {
            _condition.MergeClasses(classes);
        }

        public override void TraverseExpressions(Consumer<CodegenExpression> consumer)
        {
            consumer.Invoke(_condition);
        }
    }
} // end of namespace