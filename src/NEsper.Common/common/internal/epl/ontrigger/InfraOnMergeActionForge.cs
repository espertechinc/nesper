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
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.ontrigger
{
    public abstract class InfraOnMergeActionForge
    {
        protected internal readonly ExprNode OptionalFilter;

        protected internal InfraOnMergeActionForge(ExprNode optionalFilter)
        {
            OptionalFilter = optionalFilter;
        }

        public abstract CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope);

        protected CodegenExpression MakeFilter(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            return OptionalFilter == null
                ? ConstantNull()
                : ExprNodeUtilityCodegen.CodegenEvaluator(OptionalFilter.Forge, method, GetType(), classScope);
        }

        public static CodegenExpression MakeActions(
            IList<InfraOnMergeActionForge> actions,
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(
                typeof(IList<InfraOnMergeAction>),
                typeof(InfraOnMergeActionForge),
                classScope);
            method.Block.DeclareVar<IList<InfraOnMergeAction>>(
                "list",
                NewInstance<List<InfraOnMergeAction>>(Constant(actions.Count)));
            foreach (var item in actions) {
                method.Block.ExprDotMethod(Ref("list"), "Add", item.Make(method, symbols, classScope));
            }

            method.Block.MethodReturn(Ref("list"));
            return LocalMethod(method);
        }
    }
} // end of namespace