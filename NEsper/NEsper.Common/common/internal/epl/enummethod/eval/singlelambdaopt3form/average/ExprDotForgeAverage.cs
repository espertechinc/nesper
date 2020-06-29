///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Numerics;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.enummethod.dot;
using com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.@base;
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;


namespace com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.average
{
	public class ExprDotForgeAverage : ExprDotForgeLambdaThreeForm
	{
		protected override EPType InitAndNoParamsReturnType(
			EventType inputEventType,
			Type collectionComponentType)
		{
			if (collectionComponentType.IsBigInteger()) {
				return EPTypeHelper.SingleValue(typeof(BigInteger?));
			}
			else if (collectionComponentType.IsDecimal()) {
				return EPTypeHelper.SingleValue(typeof(decimal?));
			}
			else {
				return EPTypeHelper.SingleValue(typeof(double?));
			}
		}

		protected override ThreeFormNoParamFactory.ForgeFunction NoParamsForge(
			EnumMethodEnum enumMethod,
			EPType type,
			StatementCompileTimeServices services)
		{
			if (type.GetNormalizedClass().IsBigInteger()) {
				return streamCountIncoming => new EnumAverageBigIntegerScalarNoParam(streamCountIncoming);
				// services.ImportServiceCompileTime.DefaultMathContext
			}
			else if (type.GetNormalizedClass().IsDecimal()) {
				return streamCountIncoming => new EnumAverageDecimalScalarNoParam(
					streamCountIncoming);
			}
			else if (type.GetNormalizedClass().IsDouble()) {
				return streamCountIncoming => new EnumAverageDoubleScalarNoParam(
					streamCountIncoming);
			}
			else {
				throw new ArgumentException("Failed to find a suitable scalar no-param");
			}
		}

		protected override Func<ExprDotEvalParamLambda, EPType> InitAndSingleParamReturnType(
			EventType inputEventType,
			Type collectionComponentType)
		{
			return lambda => {
				var returnType = lambda.BodyForge.EvaluationType;
				if (returnType.IsBigInteger()) {
					return EPTypeHelper.SingleValue(typeof(BigInteger?));
				}
				else if (returnType.IsDecimal()) {
					return EPTypeHelper.SingleValue(typeof(decimal?));
				}
				else {
					return EPTypeHelper.SingleValue(typeof(double?));
				}
			};
		}

		protected override ThreeFormEventPlainFactory.ForgeFunction SingleParamEventPlain(EnumMethodEnum enumMethod)
		{
			return (
				lambda,
				typeInfo,
				services) => {

				if (typeInfo.GetNormalizedClass().IsBigInteger()) {
					return new EnumAverageBigIntegerEvent(lambda);
					// services.ImportServiceCompileTime.DefaultMathContext
				}
				else if (typeInfo.GetNormalizedClass().IsDecimal()) {
					return new EnumAverageDecimalEvent(lambda);
				}
				else if (typeInfo.GetNormalizedClass().IsDouble()) {
					return new EnumAverageDoubleEvent(lambda);
				}
				else {
					throw new ArgumentException("Failed to find a suitable event");
				}
			};
		}

		protected override ThreeFormEventPlusFactory.ForgeFunction SingleParamEventPlus(EnumMethodEnum enumMethod)
		{
			return (
				lambda,
				fieldType,
				numParameters,
				typeInfo,
				services) => {

				if (typeInfo.GetNormalizedClass().IsBigInteger()) {
					return new EnumAverageBigIntegerEventPlus(lambda, fieldType, numParameters);
				}
				else if (typeInfo.GetNormalizedClass().IsDecimal()) {
					return new EnumAverageDecimalEventPlus(lambda, fieldType, numParameters);
				}
				else if (typeInfo.GetNormalizedClass().IsDouble()) {
					return new EnumAverageDoubleEventPlus(lambda, fieldType, numParameters);
				}
				else {
					throw new ArgumentException("Failed to find a suitable event");
				}
			};
		}

		protected override ThreeFormScalarFactory.ForgeFunction SingleParamScalar(EnumMethodEnum enumMethod)
		{
			return (
				lambda,
				fieldType,
				numParams,
				typeInfo,
				services) => {

				if (typeInfo.GetNormalizedClass().IsBigInteger()) {
					return new EnumAverageBigIntegerScalar(lambda, fieldType, numParams);
				}
				else if (typeInfo.GetNormalizedClass().IsDecimal()) {
					return new EnumAverageDecimalScalar(lambda, fieldType, numParams);
				}
				else if (typeInfo.GetNormalizedClass().IsDouble()) {
					return new EnumAverageDoubleScalar(lambda, fieldType, numParams);
				}
				else {
					throw new ArgumentException("Failed to find a suitable scalar");
				}
			};
		}
	}
} // end of namespace
