///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.hook.aggmultifunc;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.agg.accessagg;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.agg.access.linear
{
    public class AggregationPortableValidationLinear : AggregationPortableValidation
    {
        public AggregationPortableValidationLinear()
        {
        }

        public AggregationPortableValidationLinear(EventType containedEventType)
        {
            ContainedEventType = containedEventType;
        }

        public EventType ContainedEventType { get; set; }

        public void ValidateIntoTableCompatible(
            string tableExpression,
            AggregationPortableValidation intoTableAgg,
            string intoExpression,
            AggregationForgeFactory factory)
        {
            AggregationValidationUtil.ValidateAggregationType(this, tableExpression, intoTableAgg, intoExpression);
            var other = (AggregationPortableValidationLinear) intoTableAgg;
            AggregationValidationUtil.ValidateEventType(ContainedEventType, other.ContainedEventType);
        }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            ModuleTableInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(AggregationPortableValidationLinear), GetType(), classScope);
            method.Block
                .DeclareVar<AggregationPortableValidationLinear>(
                    "v",
                    NewInstance(typeof(AggregationPortableValidationLinear)))
                .SetProperty(
                    Ref("v"),
                    "ContainedEventType",
                    EventTypeUtility.ResolveTypeCodegen(ContainedEventType, symbols.GetAddInitSvc(method)))
                .MethodReturn(Ref("v"));
            return LocalMethod(method);
        }

        public bool IsAggregationMethod(
            string name,
            ExprNode[] parameters,
            ExprValidationContext validationContext)
        {
            name = name.ToLowerInvariant();
            return AggregationAccessorLinearTypeExtensions.FromString(name) != null ||
                   (name == "countevents") || 
                   (name == "listreference");
        }

        public AggregationMultiFunctionMethodDesc ValidateAggregationMethod(
            ExprValidationContext validationContext,
            String aggMethodName,
            ExprNode[] @params)
        {
            aggMethodName = aggMethodName.ToLowerInvariant();
            if ((aggMethodName == "countevents") || (aggMethodName == "listreference")) {
                if (@params.Length > 0) {
                    throw new ExprValidationException("Invalid number of parameters");
                }

                var provider = typeof(AggregationMethodLinearCount);
                var result = typeof(int?);
                if (aggMethodName == "listreference") {
                    provider = typeof(AggregationMethodLinearListReference);
                    result = typeof(IList<EventBean>);
                }

                return new AggregationMultiFunctionMethodDesc(new AggregationMethodLinearNoParamForge(provider, result), null, null, null);
            }

            var methodType = AggregationAccessorLinearTypeExtensions.FromString(aggMethodName);
            if (methodType == AggregationAccessorLinearType.FIRST || methodType == AggregationAccessorLinearType.LAST) {
                return HandleMethodFirstLast(@params, methodType.Value, validationContext);
            }
            else {
                return HandleMethodWindow(@params, validationContext);
            }
        }

        private AggregationMultiFunctionMethodDesc HandleMethodWindow(
            ExprNode[] childNodes,
            ExprValidationContext validationContext)
        {
            if (childNodes.Length == 0 || (childNodes.Length == 1 && childNodes[0] is ExprWildcard)) {
                var componentType = ContainedEventType.UnderlyingType;
                var forge = new AggregationMethodLinearWindowForge(
                    TypeHelper.GetArrayType(componentType),
                    null);
                return new AggregationMultiFunctionMethodDesc(forge, ContainedEventType, null, null);
            }

            if (childNodes.Length == 1) {
                // Expressions apply to events held, thereby validate in terms of event value expressions
                var paramNode = childNodes[0];
                var streams = TableCompileTimeUtil.StreamTypeFromTableColumn(ContainedEventType);
                var localValidationContext = new ExprValidationContext(streams, validationContext);
                paramNode = ExprNodeUtilityValidate.GetValidatedSubtree(ExprNodeOrigin.AGGPARAM, paramNode, localValidationContext);
                var paramNodeType = paramNode.Forge.EvaluationType.GetBoxedType();
                var forge = new AggregationMethodLinearWindowForge(TypeHelper.GetArrayType(paramNodeType), paramNode);
                return new AggregationMultiFunctionMethodDesc(forge, null, paramNodeType, null);
            }

            throw new ExprValidationException("Invalid number of parameters");
        }

        private AggregationMultiFunctionMethodDesc HandleMethodFirstLast(
            ExprNode[] childNodes,
            AggregationAccessorLinearType methodType,
            ExprValidationContext validationContext)
        {
            var underlyingType = ContainedEventType.UnderlyingType;
            if (childNodes.Length == 0) {
                var forge = new AggregationMethodLinearFirstLastForge(underlyingType, methodType, null);
                return new AggregationMultiFunctionMethodDesc(forge, null, null, ContainedEventType);
            }

            if (childNodes.Length == 1) {
                if (childNodes[0] is ExprWildcard) {
                    var forgeX = new AggregationMethodLinearFirstLastForge(underlyingType, methodType, null);
                    return new AggregationMultiFunctionMethodDesc(forgeX, null, null, ContainedEventType);
                }

                if (childNodes[0] is ExprStreamUnderlyingNode) {
                    throw new ExprValidationException("Stream-wildcard is not allowed for table column access");
                }

                // Expressions apply to events held, thereby validate in terms of event value expressions
                var paramNode = childNodes[0];
                var streams = TableCompileTimeUtil.StreamTypeFromTableColumn(ContainedEventType);
                var localValidationContext = new ExprValidationContext(streams, validationContext);
                paramNode = ExprNodeUtilityValidate.GetValidatedSubtree(ExprNodeOrigin.AGGPARAM, paramNode, localValidationContext);
                var forge = new AggregationMethodLinearFirstLastForge(paramNode.Forge.EvaluationType, methodType, paramNode);
                return new AggregationMultiFunctionMethodDesc(forge, null, null, null);
            }

            if (childNodes.Length == 2) {
                int? constant = null;
                var indexEvalNode = childNodes[1];
                var indexEvalType = indexEvalNode.Forge.EvaluationType;
                if (indexEvalType != typeof(int?) && indexEvalType != typeof(int)) {
                    throw new ExprValidationException(GetErrorPrefix(methodType) + " requires a constant index expression that returns an integer value");
                }

                ExprNode indexExpr;
                if (indexEvalNode.Forge.ForgeConstantType == ExprForgeConstantType.COMPILETIMECONST) {
                    constant = indexEvalNode.Forge.ExprEvaluator.Evaluate(null, true, null).AsBoxedInt32();
                    indexExpr = null;
                }
                else {
                    indexExpr = indexEvalNode;
                }

                var forge = new AggregationMethodLinearFirstLastIndexForge(
                    underlyingType,
                    methodType,
                    constant,
                    indexExpr);
                return new AggregationMultiFunctionMethodDesc(forge, null, null, ContainedEventType);
            }

            throw new ExprValidationException("Invalid number of parameters");
        }

        private static String GetErrorPrefix(AggregationAccessorLinearType stateType)
        {
            return ExprAggMultiFunctionUtil.GetErrorPrefix(stateType.ToString().ToLowerInvariant());
        }
    }
} // end of namespace