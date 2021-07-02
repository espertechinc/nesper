///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.common.@internal.util;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.agg.@base
{
    public class ExprAggregateNodeGroupKey : ExprNodeBase,
        ExprForge,
        ExprEvaluator
    {
        private readonly int _numGroupKeys;
        private readonly int _groupKeyIndex;
        private readonly Type _returnType;
        private readonly CodegenFieldName _aggregationResultFutureMemberName;

        public ExprAggregateNodeGroupKey(
            int numGroupKeys,
            int groupKeyIndex,
            Type returnType,
            CodegenFieldName aggregationResultFutureMemberName)
        {
            this._numGroupKeys = numGroupKeys;
            this._groupKeyIndex = groupKeyIndex;
            this._returnType = returnType;
            this._aggregationResultFutureMemberName = aggregationResultFutureMemberName;
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
            if (_returnType.IsNullType()) {
                return ConstantNull();
            }
            CodegenExpression future = classScope.NamespaceScope.AddOrGetDefaultFieldWellKnown(
                _aggregationResultFutureMemberName,
                typeof(AggregationResultFuture));
            CodegenMethod method = parent.MakeChild(_returnType, GetType(), classScope);
            
            method.Block.DeclareVar<object>("key", ExprDotMethod(future, "GetGroupKey", ExprDotName(symbol.GetAddExprEvalCtx(method), "AgentInstanceId")));
            method.Block
                .IfCondition(InstanceOf(Ref("key"), typeof(MultiKey)))
                .DeclareVar<MultiKey>("mk", Cast(typeof(MultiKey), Ref("key")))
                .BlockReturn(CodegenLegoCast.CastSafeFromObjectType(_returnType, ExprDotMethod(Ref("mk"), "GetKey", Constant(_groupKeyIndex))));

            method.Block.IfCondition(InstanceOf(Ref("key"), typeof(MultiKeyArrayWrap)))
                .DeclareVar<MultiKeyArrayWrap>("mk", Cast(typeof(MultiKeyArrayWrap), Ref("key")))
                .BlockReturn(CodegenLegoCast.CastSafeFromObjectType(_returnType, ExprDotName(Ref("mk"), "Array")));

            method.Block.MethodReturn(CodegenLegoCast.CastSafeFromObjectType(_returnType, Ref("key")));
            
            return LocalMethod(method);
        }

        public Type EvaluationType {
            get => _returnType;
        }

        public ExprForgeConstantType ForgeConstantType {
            get => ExprForgeConstantType.NONCONST;
        }

        public override ExprForge Forge {
            get => this;
        }

        ExprNodeRenderable ExprForge.ExprForgeRenderable => ForgeRenderable;

        public ExprNode ForgeRenderable {
            get => this;
        }

        public ExprEvaluator ExprEvaluator {
            get => this;
        }

        public override void ToPrecedenceFreeEPL(
            TextWriter writer,
            ExprNodeRenderableFlags flags)
        {
        }

        public override ExprPrecedenceEnum Precedence {
            get => ExprPrecedenceEnum.UNARY;
        }

        public string ToExpressionString(ExprPrecedenceEnum precedence)
        {
            return null;
        }

        public bool IsConstantResult {
            get => false;
        }

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