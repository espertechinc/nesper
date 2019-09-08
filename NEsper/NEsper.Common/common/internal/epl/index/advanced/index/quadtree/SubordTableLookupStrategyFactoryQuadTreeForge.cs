///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.lookup;
using com.espertech.esper.common.@internal.util;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.index.advanced.index.quadtree
{
    public class SubordTableLookupStrategyFactoryQuadTreeForge : SubordTableLookupStrategyFactoryForge
    {
        private readonly ExprForge height;
        private readonly bool isNWOnTrigger;
        private readonly int streamCountOuter;
        private readonly ExprForge width;
        private readonly ExprForge x;
        private readonly ExprForge y;

        public SubordTableLookupStrategyFactoryQuadTreeForge(
            ExprForge x,
            ExprForge y,
            ExprForge width,
            ExprForge height,
            bool isNWOnTrigger,
            int streamCountOuter,
            LookupStrategyDesc lookupStrategyDesc)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
            this.isNWOnTrigger = isNWOnTrigger;
            this.streamCountOuter = streamCountOuter;
            LookupStrategyDesc = lookupStrategyDesc;
        }

        public LookupStrategyDesc LookupStrategyDesc { get; }

        public string ToQueryPlan()
        {
            return GetType().GetSimpleName();
        }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var methodNode = parent.MakeChild(typeof(SubordTableLookupStrategyFactoryQuadTree), GetType(), classScope);
            Func<ExprForge, CodegenExpression> toExpr = forge =>
                ExprNodeUtilityCodegen.CodegenEvaluator(forge, methodNode, GetType(), classScope);
            methodNode.Block
                .DeclareVar<SubordTableLookupStrategyFactoryQuadTree>(
                    "sts",
                    NewInstance(typeof(SubordTableLookupStrategyFactoryQuadTree)))
                .SetProperty(Ref("sts"), "X", toExpr.Invoke(x))
                .SetProperty(Ref("sts"), "Y", toExpr.Invoke(y))
                .SetProperty(Ref("sts"), "Width", toExpr.Invoke(width))
                .SetProperty(Ref("sts"), "Height", toExpr.Invoke(height))
                .SetProperty(Ref("sts"), "IsNwOnTrigger", Constant(isNWOnTrigger))
                .SetProperty(Ref("sts"), "StreamCountOuter", Constant(streamCountOuter))
                .SetProperty(Ref("sts"), "LookupExpressions", Constant(LookupStrategyDesc.ExpressionsTexts))
                .MethodReturn(Ref("sts"));
            return LocalMethod(methodNode);
        }
    }
} // end of namespace