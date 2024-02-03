///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.streamtype;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.output.polled
{
    /// <summary>
    /// Output condition handling crontab-at schedule output.
    /// </summary>
    public sealed class OutputConditionPolledCrontabFactoryForge : OutputConditionPolledFactoryForge
    {
        private readonly ExprNode[] expressions;

        public OutputConditionPolledCrontabFactoryForge(
            IList<ExprNode> list,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services)
        {
            var validationContext =
                new ExprValidationContextBuilder(new StreamTypeServiceImpl(false), statementRawInfo, services).Build();
            expressions = new ExprNode[list.Count];
            var count = 0;
            foreach (var parameters in list) {
                var node = ExprNodeUtilityValidate.GetValidatedSubtree(
                    ExprNodeOrigin.OUTPUTLIMIT,
                    parameters,
                    validationContext);
                expressions[count++] = node;
            }
        }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            return NewInstance<OutputConditionPolledCrontabFactory>(
                ExprNodeUtilityCodegen.CodegenEvaluators(expressions, parent, GetType(), classScope));
        }
    }
} // end of namespace