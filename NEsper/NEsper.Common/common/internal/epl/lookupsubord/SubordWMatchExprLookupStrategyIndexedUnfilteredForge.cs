///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.lookup;
using com.espertech.esper.common.@internal.epl.lookupplansubord;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.lookupsubord
{
    public class SubordWMatchExprLookupStrategyIndexedUnfilteredForge : SubordWMatchExprLookupStrategyFactoryForge
    {
        public SubordWMatchExprLookupStrategyIndexedUnfilteredForge(
            SubordTableLookupStrategyFactoryForge lookupStrategyFactory)
        {
            OptionalInnerStrategy = lookupStrategyFactory;
        }

        public SubordTableLookupStrategyFactoryForge OptionalInnerStrategy { get; }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(
                typeof(SubordWMatchExprLookupStrategyIndexedUnfilteredFactory),
                GetType(),
                classScope);
            method.Block
                .DeclareVar<SubordTableLookupStrategyFactory>(
                    "lookup",
                    OptionalInnerStrategy.Make(method, symbols, classScope))
                .MethodReturn(
                    NewInstance<SubordWMatchExprLookupStrategyIndexedUnfilteredFactory>(Ref("lookup")));
            return LocalMethod(method);
        }

        public string ToQueryPlan()
        {
            return GetType().Name + " " + " strategy " + OptionalInnerStrategy.ToQueryPlan();
        }
    }
} // end of namespace