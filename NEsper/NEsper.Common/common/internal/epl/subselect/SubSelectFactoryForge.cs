///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.activator;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.subquery;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.subselect
{
    public class SubSelectFactoryForge
    {
        private readonly ViewableActivatorForge activator;
        private readonly SubSelectStrategyFactoryForge strategyFactoryForge;
        private readonly int subqueryNumber;

        public SubSelectFactoryForge(
            int subqueryNumber,
            ViewableActivatorForge activator,
            SubSelectStrategyFactoryForge strategyFactoryForge)
        {
            this.subqueryNumber = subqueryNumber;
            this.activator = activator;
            this.strategyFactoryForge = strategyFactoryForge;
        }

        public IList<ViewFactoryForge> ViewForges => strategyFactoryForge.ViewForges;

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(SubSelectFactory), GetType(), classScope);
            method.Block
                .DeclareVar(typeof(SubSelectFactory), "factory", NewInstance(typeof(SubSelectFactory)))
                .SetProperty(Ref("factory"), "SubqueryNumber", Constant(subqueryNumber))
                .SetProperty(Ref("factory"), "Activator", activator.MakeCodegen(method, symbols, classScope))
                .SetProperty(Ref("factory"), "StrategyFactory", strategyFactoryForge.MakeCodegen(method, symbols, classScope))
                .SetProperty(Ref("factory"), "HasAggregation", Constant(strategyFactoryForge.HasAggregation))
                .SetProperty(Ref("factory"), "HasPrior", Constant(strategyFactoryForge.HasPrior))
                .SetProperty(Ref("factory"), "HasPrevious", Constant(strategyFactoryForge.HasPrevious))
                .ExprDotMethod(symbols.GetAddInitSvc(method), "addReadyCallback", Ref("factory"))
                .MethodReturn(Ref("factory"));
            return LocalMethod(method);
        }

        public static CodegenExpression CodegenInitMap(
            IDictionary<ExprSubselectNode, SubSelectFactoryForge> subselects,
            Type generator,
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(IDictionary<object, object>), generator, classScope);
            method.Block
                .DeclareVar(
                    typeof(IDictionary<object, object>), "subselects",
                    NewInstance(typeof(LinkedHashMap<object, object>), Constant(subselects.Count + 2)));
            foreach (var entry in subselects) {
                method.Block.ExprDotMethod(
                    Ref("subselects"), "put", Constant(entry.Key.SubselectNumber),
                    entry.Value.Make(method, symbols, classScope));
            }

            method.Block.MethodReturn(Ref("subselects"));
            return LocalMethod(method);
        }
    }
} // end of namespace