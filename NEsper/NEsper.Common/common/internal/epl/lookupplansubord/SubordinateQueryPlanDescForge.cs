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
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.lookupplansubord
{
    public class SubordinateQueryPlanDescForge
    {
        public SubordinateQueryPlanDescForge(
            SubordTableLookupStrategyFactoryForge lookupStrategyFactory,
            SubordinateQueryIndexDescForge[] indexDescs)
        {
            LookupStrategyFactory = lookupStrategyFactory;
            IndexDescs = indexDescs;
        }

        public SubordTableLookupStrategyFactoryForge LookupStrategyFactory { get; }

        public SubordinateQueryIndexDescForge[] IndexDescs { get; }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var builder = new SAIFFInitializeBuilder(
                typeof(SubordinateQueryPlanDesc), GetType(), "strategy", parent, symbols, classScope);
            var numIndex = IndexDescs == null ? 0 : IndexDescs.Length;
            var indexDescArray = new CodegenExpression[numIndex];
            for (var i = 0; i < numIndex; i++) {
                indexDescArray[i] = IndexDescs[i].Make(builder.Method(), symbols, classScope);
            }

            return builder.Expression(
                    "lookupStrategyFactory", LookupStrategyFactory.Make(builder.Method(), symbols, classScope))
                .Expression("indexDescs", NewArrayWithInit(typeof(SubordinateQueryIndexDesc), indexDescArray))
                .Build();
        }
    }
} // end of namespace