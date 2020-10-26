///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.enummethod.dot;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.dot.core;
using com.espertech.esper.common.@internal.epl.methodbase;
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.aggregate
{
	public class ExprDotForgeAggregate : ExprDotForgeEnumMethodBase
	{

		public override EnumForgeDescFactory GetForgeFactory(
			DotMethodFP footprint,
			IList<ExprNode> parameters,
			EnumMethodEnum enumMethod,
			string enumMethodUsedName,
			EventType inputEventType,
			Type collectionComponentType,
			ExprValidationContext validationContext)
		{
			var goesNode = (ExprLambdaGoesNode) parameters[1];
			var numParameters = goesNode.GoesToNames.Count;
			var firstName = goesNode.GoesToNames[0];
			var secondName = goesNode.GoesToNames[1];

			IDictionary<string, object> fields = new Dictionary<string, object>();
			var initializationType = parameters[0].Forge.EvaluationType;
			fields.Put(firstName, initializationType);
			if (inputEventType == null) {
				fields.Put(secondName, collectionComponentType);
			}

			if (numParameters > 2) {
				fields.Put(goesNode.GoesToNames[2], typeof(int));
				if (numParameters > 3) {
					fields.Put(goesNode.GoesToNames[3], typeof(int));
				}
			}

			var evalEventType = ExprDotNodeUtility.MakeTransientOAType(
				enumMethodUsedName,
				fields,
				validationContext.StatementRawInfo,
				validationContext.StatementCompileTimeService);
			if (inputEventType == null) {
				return new EnumForgeDescFactoryAggregateScalar(evalEventType);
			}

			return new EnumForgeDescFactoryAggregateEvent(evalEventType, inputEventType, secondName, numParameters);
		}

		private class EnumForgeDescFactoryAggregateScalar : EnumForgeDescFactory
		{
			private readonly ObjectArrayEventType _evalEventType;

			public EnumForgeDescFactoryAggregateScalar(ObjectArrayEventType evalEventType)
			{
				_evalEventType = evalEventType;
			}

			public EnumForgeLambdaDesc GetLambdaStreamTypesForParameter(int parameterNum)
			{
				return new EnumForgeLambdaDesc(new EventType[] {_evalEventType}, new string[] {_evalEventType.Name});
			}

			public EnumForgeDesc MakeEnumForgeDesc(
				IList<ExprDotEvalParam> bodiesAndParameters,
				int streamCountIncoming,
				StatementCompileTimeServices services)
			{
				var init = bodiesAndParameters[0].BodyForge;
				var compute = (ExprDotEvalParamLambda) bodiesAndParameters[1];
				EnumAggregateScalar forge = new EnumAggregateScalar(streamCountIncoming, init, compute.BodyForge, _evalEventType, compute.GoesToNames.Count);
				var type = EPTypeHelper.SingleValue(init.EvaluationType.GetBoxedType());
				return new EnumForgeDesc(type, forge);
			}
		}

		private class EnumForgeDescFactoryAggregateEvent : EnumForgeDescFactory
		{
			private readonly ObjectArrayEventType _evalEventType;
			private readonly EventType _inputEventType;
			private readonly string _streamName;
			private readonly int _numParameters;

			public EnumForgeDescFactoryAggregateEvent(
				ObjectArrayEventType evalEventType,
				EventType inputEventType,
				string streamName,
				int numParameters)
			{
				_evalEventType = evalEventType;
				_inputEventType = inputEventType;
				_streamName = streamName;
				_numParameters = numParameters;
			}

			public EnumForgeLambdaDesc GetLambdaStreamTypesForParameter(int parameterNum)
			{
				return new EnumForgeLambdaDesc(
					new EventType[] {
						_evalEventType, _inputEventType
					},
					new string[] {
						_evalEventType.Name, _streamName
					});
			}

			public EnumForgeDesc MakeEnumForgeDesc(
				IList<ExprDotEvalParam> bodiesAndParameters,
				int streamCountIncoming,
				StatementCompileTimeServices services)
			{
				var init = bodiesAndParameters[0].BodyForge;
				var compute = (ExprDotEvalParamLambda) bodiesAndParameters[1];
				var forge = new EnumAggregateEvent(streamCountIncoming, init, compute.BodyForge, _evalEventType, _numParameters);
				var type = EPTypeHelper.SingleValue(init.EvaluationType.GetBoxedType());
				return new EnumForgeDesc(type, forge);
			}
		}
	}
} // end of namespace
