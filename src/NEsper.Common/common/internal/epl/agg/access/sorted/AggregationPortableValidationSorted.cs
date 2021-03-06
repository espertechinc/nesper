///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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

		public string AggFuncName {
			get => this.aggFuncName;
			set => this.aggFuncName = value;
		}

		public EventType ContainedEventType {
			get => this.containedEventType;
			set => this.containedEventType = value;
		}

		public Type[] OptionalCriteriaTypes {
			get => this.optionalCriteriaTypes;
			set => this.optionalCriteriaTypes = value;
		}

		public void ValidateIntoTableCompatible(
			string tableExpression,
			AggregationPortableValidation intoTableAgg,
			string intoExpression,
			AggregationForgeFactory factory)
		{
			AggregationValidationUtil.ValidateAggregationType(this, tableExpression, intoTableAgg, intoExpression);
			var other = (AggregationPortableValidationSorted) intoTableAgg;
			AggregationValidationUtil.ValidateEventType(this.containedEventType, other.containedEventType);
			AggregationValidationUtil.ValidateAggFuncName(aggFuncName, other.aggFuncName);
		}

		public CodegenExpression Make(
			CodegenMethodScope parent,
			ModuleTableInitializeSymbol symbols,
			CodegenClassScope classScope)
		{
			var method = parent.MakeChild(typeof(AggregationPortableValidationSorted), this.GetType(), classScope);
			method.Block
				.DeclareVar<AggregationPortableValidationSorted>("v", NewInstance(typeof(AggregationPortableValidationSorted)))
				.SetProperty(Ref("v"), "AggFuncName", Constant(aggFuncName))
				.SetProperty(Ref("v"), "ContainedEventType", EventTypeUtility.ResolveTypeCodegen(containedEventType, symbols.GetAddInitSvc(method)))
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
			if (name == "maxby" || name == "minby") {
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
			switch (name) {
				case "maxby": {
					var forgeX = new AggregationMethodSortedMinMaxByForge(componentType, true);
					return new AggregationMultiFunctionMethodDesc(forgeX, null, null, containedEventType);
				}

				case "minby": {
					var forgeX = new AggregationMethodSortedMinMaxByForge(componentType, false);
					return new AggregationMultiFunctionMethodDesc(forgeX, null, null, containedEventType);
				}

				case "sorted": {
					var forgeX = new AggregationMethodSortedWindowForge(TypeHelper.GetArrayType(componentType));
					return new AggregationMultiFunctionMethodDesc(forgeX, containedEventType, null, null);
				}
			}

			// validate all parameters
			for (var i = 0; i < @params.Length; i++) {
				@params[i] = ExprNodeUtilityValidate.GetValidatedSubtree(ExprNodeOrigin.AGGPARAM, @params[i], validationContext);
			}

			// determine method
			var methodEnum = EnumHelper.Parse<AggregationMethodSortedEnum>(aggMethodName);

			// validate footprint
			var footprintProvided = DotMethodUtil.GetProvidedFootprint(Arrays.AsList(@params));
			var aggregationMethodSortedFootprintEnum = methodEnum.GetFootprint();
			var footprints = aggregationMethodSortedFootprintEnum.GetFP();
			DotMethodUtil.ValidateParametersDetermineFootprint(
				footprints,
				DotMethodTypeEnum.AGGMETHOD,
				aggMethodName,
				footprintProvided,
				DotMethodInputTypeMatcherImpl.DEFAULT_ALL);

			var keyType = optionalCriteriaTypes == null
				? typeof(IComparable)
				: (optionalCriteriaTypes.Length == 1 ? optionalCriteriaTypes[0] : typeof(HashableMultiKey));
			var resultType = methodEnum.GetResultType(containedEventType.UnderlyingType, keyType);

			AggregationMethodForge forge;
			if (aggregationMethodSortedFootprintEnum == AggregationMethodSortedFootprintEnum.SUBMAP) {
				ValidateKeyType(aggMethodName, 0, keyType, @params[0]);
				ValidateKeyType(aggMethodName, 2, keyType, @params[2]);
				forge = new AggregationMethodSortedSubmapForge(@params[0], @params[1], @params[2], @params[3], componentType, methodEnum, resultType);
			}
			else if (aggregationMethodSortedFootprintEnum == AggregationMethodSortedFootprintEnum.KEYONLY) {
				ValidateKeyType(aggMethodName, 0, keyType, @params[0]);
				forge = new AggregationMethodSortedKeyedForge(@params[0], componentType, methodEnum, resultType);
			}
			else {
				forge = new AggregationMethodSortedNoParamForge(componentType, methodEnum, resultType);
			}

			var eventTypeCollection = methodEnum.IsReturnsCollectionOfEvents() ? containedEventType : null;
			var eventTypeSingle = methodEnum.IsReturnsSingleEvent() ? containedEventType : null;
			return new AggregationMultiFunctionMethodDesc(forge, eventTypeCollection, null, eventTypeSingle);
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
	}
} // end of namespace
