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
    public abstract class InfraOnMergeActionForge
    {
        internal readonly ExprNode optionalFilter;

        public InfraOnMergeActionForge(ExprNode optionalFilter)
        {
            this.optionalFilter = optionalFilter;
        }

        protected abstract CodegenExpression Make(
            CodegenMethodScope parent, SAIFFInitializeSymbol symbols, CodegenClassScope classScope);

        protected CodegenExpression MakeFilter(CodegenMethod method, CodegenClassScope classScope)
        {
            return optionalFilter == null
                ? ConstantNull()
                : ExprNodeUtilityCodegen.CodegenEvaluator(optionalFilter.Forge, method, GetType(), classScope);
        }

        public static CodegenExpression MakeActions(
            IList<InfraOnMergeActionForge> actions, CodegenMethodScope parent, SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(IList<object>), typeof(InfraOnMergeActionForge), classScope);
            method.Block.DeclareVar(
                typeof(IList<object>), typeof(InfraOnMergeAction), "list",
                NewInstance(typeof(List<object>), Constant(actions.Count)));
            foreach (var item in actions) {
                method.Block.ExprDotMethod(Ref("list"), "add", item.Make(method, symbols, classScope));
            }

            method.Block.MethodReturn(Ref("list"));
            return LocalMethod(method);
        }
    }
} // end of namespace