///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;

using com.espertech.esper.codegen.model.expression;

namespace com.espertech.esper.codegen.model.statement
{
    public class CodegenStatementIfConditionReturnConst : CodegenStatementBase
    {
        private readonly ICodegenExpression condition;
        private readonly Object constant;

        public CodegenStatementIfConditionReturnConst(ICodegenExpression condition, Object constant)
        {
            this.condition = condition;
            this.constant = constant;
        }

        public override void RenderStatement(StringBuilder builder, IDictionary<Type, string> imports)
        {
            builder.Append("if (");
            condition.Render(builder, imports);
            builder.Append(") return ");
            CodegenExpressionUtil.RenderConstant(builder, constant);
        }

        public override void MergeClasses(ICollection<Type> classes)
        {
            condition.MergeClasses(classes);
        }
    }
} // end of namespace