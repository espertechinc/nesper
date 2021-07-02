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
        private readonly ViewableActivatorForge _activator;
        private readonly SubSelectStrategyFactoryForge _strategyFactoryForge;
        private readonly int _subqueryNumber;

        public SubSelectFactoryForge(
            int subqueryNumber,
            ViewableActivatorForge activator,
            SubSelectStrategyFactoryForge strategyFactoryForge)
        {
            _subqueryNumber = subqueryNumber;
            _activator = activator;
            _strategyFactoryForge = strategyFactoryForge;
        }

        public IList<ViewFactoryForge> ViewForges => _strategyFactoryForge.ViewForges;

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(SubSelectFactory), GetType(), classScope);
            method.Block
                .DeclareVarNewInstance<SubSelectFactory>("factory")
                .SetProperty(Ref("factory"), "SubqueryNumber", Constant(_subqueryNumber))
                .SetProperty(Ref("factory"), "Activator", _activator.MakeCodegen(method, symbols, classScope))
                .SetProperty(
                    Ref("factory"),
                    "StrategyFactory",
                    _strategyFactoryForge.MakeCodegen(method, symbols, classScope))
                .SetProperty(Ref("factory"), "HasAggregation", Constant(_strategyFactoryForge.HasAggregation))
                .SetProperty(Ref("factory"), "HasPrior", Constant(_strategyFactoryForge.HasPrior))
                .SetProperty(Ref("factory"), "HasPrevious", Constant(_strategyFactoryForge.HasPrevious))
                .ExprDotMethod(symbols.GetAddInitSvc(method), "AddReadyCallback", Ref("factory"))
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
            var method = parent.MakeChild(typeof(IDictionary<int, SubSelectFactory>), generator, classScope);
            method.Block
                .DeclareVar<IDictionary<int, SubSelectFactory>>(
                    "subselects",
                    NewInstance(typeof(LinkedHashMap<int, SubSelectFactory>)));
            foreach (var entry in subselects) {
                method.Block.ExprDotMethod(
                    Ref("subselects"),
                    "Put",
                    Constant(entry.Key.SubselectNumber),
                    entry.Value.Make(method, symbols, classScope));
            }

            method.Block.MethodReturn(Ref("subselects"));
            return LocalMethod(method);
        }
    }
} // end of namespace