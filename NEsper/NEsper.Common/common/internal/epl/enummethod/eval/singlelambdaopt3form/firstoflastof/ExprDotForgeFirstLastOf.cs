///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.enummethod.dot;
using com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.@base;
using com.espertech.esper.common.@internal.rettype;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.firstoflastof
{
	public class ExprDotForgeFirstLastOf : ExprDotForgeLambdaThreeForm
	{

		protected override EPType InitAndNoParamsReturnType(
			EventType inputEventType,
			Type collectionComponentType)
		{
			if (inputEventType != null) {
				return EPTypeHelper.SingleEvent(inputEventType);
			}

			return EPTypeHelper.SingleValue(collectionComponentType);
		}

		protected override ThreeFormNoParamFactory.ForgeFunction NoParamsForge(
			EnumMethodEnum enumMethod,
			EPType type,
			StatementCompileTimeServices services)
		{
			if (enumMethod == EnumMethodEnum.FIRSTOF) {
				return streamCountIncoming => new EnumFirstOf(streamCountIncoming, type);
			}
			else {
				return streamCountIncoming => new EnumLastOf(streamCountIncoming, type);
			}
		}

		protected override Func<ExprDotEvalParamLambda, EPType> InitAndSingleParamReturnType(
			EventType inputEventType,
			Type collectionComponentType)
		{
			return lambda => InitAndNoParamsReturnType(inputEventType, collectionComponentType);
		}

		protected override ThreeFormEventPlainFactory.ForgeFunction SingleParamEventPlain(EnumMethodEnum enumMethod)
		{
			return (
				lambda,
				typeInfo,
				services) => {
				if (enumMethod == EnumMethodEnum.FIRSTOF) {
					return new EnumFirstOfEvent(lambda);
				}
				else {
					return new EnumLastOfEvent(lambda);
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
				if (enumMethod == EnumMethodEnum.FIRSTOF) {
					return new EnumFirstOfEventPlus(lambda, fieldType, numParameters);
				}
				else {
					return new EnumLastOfEventPlus(lambda, fieldType, numParameters);
				}
			};
		}

		protected override ThreeFormScalarFactory.ForgeFunction SingleParamScalar(EnumMethodEnum enumMethod)
		{
			return (
				lambda,
				eventType,
				numParams,
				typeInfo,
				services) => {
				if (enumMethod == EnumMethodEnum.FIRSTOF) {
					return new EnumFirstOfScalar(lambda, eventType, numParams, typeInfo);
				}
				else {
					return new EnumLastOfScalar(lambda, eventType, numParams, typeInfo);
				}
			};
		}
	}
} // end of namespace
