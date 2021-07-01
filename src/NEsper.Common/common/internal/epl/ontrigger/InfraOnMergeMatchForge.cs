///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.ontrigger
{
    public class InfraOnMergeMatchForge
    {
        private readonly ExprNode optionalCond;
        private readonly IList<InfraOnMergeActionForge> actions;

        public InfraOnMergeMatchForge(
            ExprNode optionalCond,
            IList<InfraOnMergeActionForge> actions)
        {
            this.optionalCond = optionalCond;
            this.actions = actions;
        }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            CodegenMethod method = parent.MakeChild(typeof(InfraOnMergeMatch), this.GetType(), classScope);
            CodegenExpression evaluator = optionalCond == null
                ? ConstantNull()
                : ExprNodeUtilityCodegen.CodegenEvaluator(optionalCond.Forge, method, this.GetType(), classScope);
            CodegenExpression actionsList = InfraOnMergeActionForge.MakeActions(actions, method, symbols, classScope);
            method.Block.MethodReturn(NewInstance<InfraOnMergeMatch>(evaluator, actionsList));
            return LocalMethod(method);
        }
    }
} // end of namespace