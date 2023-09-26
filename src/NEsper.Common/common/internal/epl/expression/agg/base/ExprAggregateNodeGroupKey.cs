///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.bytecodemodel.name;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.agg.@base
{
    public class ExprAggregateNodeGroupKey : ExprNodeBase,
        ExprForge,
        ExprEvaluator
    {
        private readonly int numGroupKeys;
        private readonly int groupKeyIndex;
        private readonly Type returnType;
        private readonly CodegenFieldName aggregationResultFutureMemberName;

        public ExprAggregateNodeGroupKey(
            int numGroupKeys,
            int groupKeyIndex,
            Type returnType,
            CodegenFieldName aggregationResultFutureMemberName)
        {
            this.numGroupKeys = numGroupKeys;
            this.groupKeyIndex = groupKeyIndex;
            this.returnType = returnType;
            this.aggregationResultFutureMemberName = aggregationResultFutureMemberName;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            throw ExprNodeUtilityMake.MakeUnsupportedCompileTime();
        }

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope parent,
            ExprForgeCodegenSymbol symbol,
            CodegenClassScope classScope)
        {
            if (returnType == null) {
                return ConstantNull();
            }

            CodegenExpression future = classScope.NamespaceScope.AddOrGetFieldWellKnown(
                aggregationResultFutureMemberName,
                typeof(AggregationResultFuture));
            var method = parent.MakeChild(returnType, GetType(), classScope);
            method.Block
                .DeclareVar<object>(
                    "key",
                    ExprDotMethod(
                        future,
                        "getGroupKey",
                        ExprDotName(symbol.GetAddExprEvalCtx(method), "AgentInstanceId")));

            method.Block.IfCondition(InstanceOf(Ref("key"), typeof(MultiKey)))
                .DeclareVar<MultiKey>("mk", Cast(typeof(MultiKey), Ref("key")))
                .BlockReturn(
                    CodegenLegoCast.CastSafeFromObjectType(
                        returnType,
                        ExprDotMethod(Ref("mk"), "getKey", Constant(groupKeyIndex))));

            method.Block.IfCondition(InstanceOf(Ref("key"), typeof(MultiKeyArrayWrap)))
                .DeclareVar<MultiKeyArrayWrap>("mk", Cast(typeof(MultiKeyArrayWrap), Ref("key")))
                .BlockReturn(CodegenLegoCast.CastSafeFromObjectType(returnType, ExprDotName(Ref("mk"), "Array")));

            method.Block.MethodReturn(CodegenLegoCast.CastSafeFromObjectType(returnType, Ref("key")));
            return LocalMethod(method);
        }

        public Type EvaluationType => returnType;

        public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

        public override ExprForge Forge => this;

        public ExprNode ForgeRenderable => this;

        public ExprNodeRenderable ExprForgeRenderable => ForgeRenderable;

        public ExprEvaluator ExprEvaluator => this;

        public override void ToPrecedenceFreeEPL(
            TextWriter writer,
            ExprNodeRenderableFlags flags)
        {
        }

        public override ExprPrecedenceEnum Precedence => ExprPrecedenceEnum.UNARY;

        public string ToExpressionString(ExprPrecedenceEnum precedence)
        {
            return null;
        }

        public bool IsConstantResult => false;

        public override bool EqualsNode(
            ExprNode node,
            bool ignoreStreamPrefix)
        {
            return false;
        }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            // not required
            return null;
        }
    }
} // end of namespace