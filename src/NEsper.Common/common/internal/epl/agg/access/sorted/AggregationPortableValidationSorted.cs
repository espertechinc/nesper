///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.hook.aggmultifunc;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.methodbase;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.agg.access.sorted
{
    public class AggregationPortableValidationSorted : AggregationPortableValidation
    {
        private string aggFuncName;
        private EventType containedEventType;
        private Type[] optionalCriteriaTypes;

        public AggregationPortableValidationSorted()
        {
        }

        public AggregationPortableValidationSorted(
            string aggFuncName,
            EventType containedEventType,
            Type[] optionalCriteriaTypes)
        {
            this.aggFuncName = aggFuncName;
            this.containedEventType = containedEventType;
            this.optionalCriteriaTypes = optionalCriteriaTypes;
        }

        public void ValidateIntoTableCompatible(
            string tableExpression,
            AggregationPortableValidation intoTableAgg,
            string intoExpression,
            AggregationForgeFactory factory)
        {
            AggregationValidationUtil.ValidateAggregationType(this, tableExpression, intoTableAgg, intoExpression);
            var other = (AggregationPortableValidationSorted)intoTableAgg;
            AggregationValidationUtil.ValidateEventType(containedEventType, other.containedEventType);
            AggregationValidationUtil.ValidateAggFuncName(aggFuncName, other.aggFuncName);
        }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            ModuleTableInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(AggregationPortableValidationSorted), GetType(), classScope);
            method.Block.DeclareVarNewInstance(typeof(AggregationPortableValidationSorted), "v")
                .SetProperty(Ref("v"), "AggFuncName", Constant(aggFuncName))
                .SetProperty(
                    Ref("v"),
                    "ContainedEventType",
                    EventTypeUtility.ResolveTypeCodegen(containedEventType, symbols.GetAddInitSvc(method)))
                .SetProperty(Ref("v"), "OptionalCriteriaTypes", Constant(optionalCriteriaTypes))
                .MethodReturn(Ref("v"));
            return LocalMethod(method);
        }

        public bool IsAggregationMethod(
            string nameMixed,
            ExprNode[] parameters,
            ExprValidationContext validationContext)
        {
            var name = nameMixed.ToLowerInvariant();
            if (name.Equals("maxby") || name.Equals("minby")) {
                return parameters.Length == 0;
            }

			var methodEnum = EnumHelper.ParseBoxed<AggregationMethodSortedEnum>(nameMixed);
            return name.Equals("sorted") || methodEnum != null;
        }

        public AggregationMultiFunctionMethodDesc ValidateAggregationMethod(
            ExprValidationContext validationContext,
            string aggMethodName,
            ExprNode[] @params)
        {
            var name = aggMethodName.ToLowerInvariant();
            var componentType = containedEventType.UnderlyingType;
            if (name.Equals("maxby") || name.Equals("minby")) {
                var forge = new AggregationMethodSortedMinMaxByForge(componentType, name.Equals("maxby"));
                return new AggregationMultiFunctionMethodDesc(forge, null, null, containedEventType);
            }
            else if (name.Equals("sorted")) {
                var arrayType = TypeHelper.GetArrayType(componentType);
                var forge = new AggregationMethodSortedWindowForge(arrayType);
                return new AggregationMultiFunctionMethodDesc(forge, containedEventType, null, null);
            }

            // validate all parameters
            for (var i = 0; i < @params.Length; i++) {
                @params[i] = ExprNodeUtilityValidate.GetValidatedSubtree(
                    ExprNodeOrigin.AGGPARAM,
                    @params[i],
                    validationContext);
            }

            // determine method
			var methodEnum = EnumHelper.Parse<AggregationMethodSortedEnum>(aggMethodName);

            // validate footprint
            var footprintProvided = DotMethodUtil.GetProvidedFootprint(Arrays.AsList(@params));
            var footprints = methodEnum.GetFootprint().GetFP();
            DotMethodUtil.ValidateParametersDetermineFootprint(
                footprints,
                DotMethodTypeEnum.AGGMETHOD,
                aggMethodName,
                footprintProvided,
                DotMethodInputTypeMatcher.DEFAULT_ALL);
            Type keyType;
            if (optionalCriteriaTypes == null) {
                keyType = typeof(IComparable);
            }
            else {
                if (optionalCriteriaTypes.Length == 1) {
                    keyType = optionalCriteriaTypes[0];
                }
                else {
                    keyType = typeof(HashableMultiKey);
                }
            }

            var underlyingType = containedEventType.UnderlyingType;
            var resultType = methodEnum.GetResultType(underlyingType, keyType);
            AggregationMethodForge forgeX;
            if (methodEnum.GetFootprint() == AggregationMethodSortedFootprintEnum.SUBMAP) {
                ValidateKeyType(aggMethodName, 0, keyType, @params[0]);
                ValidateKeyType(aggMethodName, 2, keyType, @params[2]);
                forgeX = new AggregationMethodSortedSubmapForge(
                    @params[0],
                    @params[1],
                    @params[2],
                    @params[3],
                    componentType,
                    methodEnum,
                    resultType);
            }
            else if (methodEnum.GetFootprint() == AggregationMethodSortedFootprintEnum.KEYONLY) {
                ValidateKeyType(aggMethodName, 0, keyType, @params[0]);
                forgeX = new AggregationMethodSortedKeyedForge(@params[0], componentType, methodEnum, resultType);
            }
            else {
                forgeX = new AggregationMethodSortedNoParamForge(componentType, methodEnum, resultType);
            }

            var eventTypeCollection = methodEnum.IsReturnsCollectionOfEvents() ? containedEventType : null;
            var eventTypeSingle = methodEnum.IsReturnsSingleEvent() ? containedEventType : null;
            return new AggregationMultiFunctionMethodDesc(forgeX, eventTypeCollection, null, eventTypeSingle);
        }

        private void ValidateKeyType(
            string aggMethodName,
            int parameterNumber,
            Type keyType,
            ExprNode validated)
        {
            var keyBoxed = keyType.GetBoxedType();
            var providedBoxed = validated.Forge.EvaluationType.GetBoxedType();
            if (keyBoxed != providedBoxed) {
                throw new ExprValidationException(
                    "Method '" +
                    aggMethodName +
                    "' for parameter " +
                    parameterNumber +
                    " requires a key of type '" +
                    keyBoxed.CleanName() +
                    "' but receives '" +
                    providedBoxed.CleanName() +
                    "'");
            }
        }

        public string AggFuncName {
            set => aggFuncName = value;
        }

        public EventType ContainedEventType {
            set => containedEventType = value;
        }

        public Type[] OptionalCriteriaTypes {
            set => optionalCriteriaTypes = value;
        }
    }
} // end of namespace