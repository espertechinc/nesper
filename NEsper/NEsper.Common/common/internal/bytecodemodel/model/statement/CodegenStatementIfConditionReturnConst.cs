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

using com.espertech.esper.common.@internal.bytecodemodel.model.expression;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionUtil;

namespace com.espertech.esper.common.@internal.bytecodemodel.model.statement
{
    public class CodegenStatementIfConditionReturnConst : CodegenStatementBase
    {
        private readonly CodegenExpression condition;
        private readonly object constant;

        public CodegenStatementIfConditionReturnConst(
            CodegenExpression condition,
            object constant)
        {
            this.condition = condition;
            this.constant = constant;
        }

        public override void RenderStatement(
            StringBuilder builder,
            bool isInnerClass)
        {
            builder.Append("if (");
            condition.Render(builder, isInnerClass);
            builder.Append(") return ");
            RenderConstant(builder, constant);
        }

        public override void MergeClasses(ISet<Type> classes)
        {
            condition.MergeClasses(classes);
        }
    }
} // end of namespace