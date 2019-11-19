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

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.lookupplansubord
{
    public class SubordinateWMatchExprQueryPlanForge
    {
        public SubordinateWMatchExprQueryPlanForge(
            SubordWMatchExprLookupStrategyFactoryForge strategy,
            SubordinateQueryIndexDescForge[] indexes)
        {
            Strategy = strategy;
            Indexes = indexes;
        }

        public SubordWMatchExprLookupStrategyFactoryForge Strategy { get; }

        public SubordinateQueryIndexDescForge[] Indexes { get; }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(SubordinateWMatchExprQueryPlan), GetType(), classScope);

            method.Block
                .DeclareVar<SubordWMatchExprLookupStrategyFactory>(
                    "strategy",
                    Strategy.Make(parent, symbols, classScope))
                .DeclareVar<SubordinateQueryIndexDesc[]>(
                    "indexes",
                    Indexes == null
                        ? ConstantNull()
                        : NewArrayByLength(typeof(SubordinateQueryIndexDesc), Constant(Indexes.Length)));

            if (Indexes != null) {
                for (var i = 0; i < Indexes.Length; i++) {
                    method.Block.AssignArrayElement(
                        "indexes",
                        Constant(i),
                        Indexes[i].Make(method, symbols, classScope));
                }
            }

            method.Block.MethodReturn(
                NewInstance<SubordinateWMatchExprQueryPlan>(Ref("strategy"), Ref("indexes")));
            return LocalMethod(method);
        }
    }
} // end of namespace