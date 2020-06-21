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
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.@base
{
	public abstract class ExprDotForgeLambdaThreeForm : ExprDotForgeEnumMethodBase
	{
		protected abstract EPType InitAndNoParamsReturnType(
			EventType inputEventType,
			Type collectionComponentType);

		protected abstract ThreeFormNoParamFactory.ForgeFunction NoParamsForge(
			EnumMethodEnum enumMethod,
			EPType type,
			StatementCompileTimeServices services);

		protected abstract Func<ExprDotEvalParamLambda, EPType> InitAndSingleParamReturnType(
			EventType inputEventType,
			Type collectionComponentType);

		protected abstract ThreeFormEventPlainFactory.ForgeFunction SingleParamEventPlain(EnumMethodEnum enumMethod);
		protected abstract ThreeFormEventPlusFactory.ForgeFunction SingleParamEventPlus(EnumMethodEnum enumMethod);
		protected abstract ThreeFormScalarFactory.ForgeFunction SingleParamScalar(EnumMethodEnum enumMethod);

		public override EnumForgeDescFactory GetForgeFactory(
			DotMethodFP footprint,
			IList<ExprNode> parameters,
			EnumMethodEnum enumMethod,
			string enumMethodUsedName,
			EventType inputEventType,
			Type collectionComponentType,
			ExprValidationContext validationContext)
		{
			if (parameters.IsEmpty()) {
				var typeX = InitAndNoParamsReturnType(inputEventType, collectionComponentType);
				return new ThreeFormNoParamFactory(typeX, NoParamsForge(enumMethod, typeX, validationContext.StatementCompileTimeService));
			}

			var goesNode = (ExprLambdaGoesNode) parameters[0];
			var goesToNames = goesNode.GoesToNames;

			if (inputEventType != null) {
				var streamName = goesToNames[0];
				if (goesToNames.Count == 1) {
					return new ThreeFormEventPlainFactory(
						InitAndSingleParamReturnType(inputEventType, collectionComponentType),
						inputEventType,
						streamName,
						SingleParamEventPlain(enumMethod));
				}

				IDictionary<string, object> fieldsX = new LinkedHashMap<string, object>();
				fieldsX.Put(goesToNames[1], typeof(int?));
				if (goesToNames.Count > 2) {
					fieldsX.Put(goesToNames[2], typeof(int?));
				}

				var fieldType = ExprDotNodeUtility.MakeTransientOAType(
					enumMethodUsedName,
					fieldsX,
					validationContext.StatementRawInfo,
					validationContext.StatementCompileTimeService);
				return new ThreeFormEventPlusFactory(
					InitAndSingleParamReturnType(inputEventType, collectionComponentType),
					inputEventType,
					streamName,
					fieldType,
					goesToNames.Count,
					SingleParamEventPlus(enumMethod));
			}

			var fields = new LinkedHashMap<string, object>();
			fields.Put(goesToNames[0], collectionComponentType);
			if (goesToNames.Count > 1) {
				fields.Put(goesToNames[1], typeof(int?));
			}

			if (goesToNames.Count > 2) {
				fields.Put(goesToNames[2], typeof(int?));
			}

			var type = ExprDotNodeUtility.MakeTransientOAType(
				enumMethodUsedName,
				fields,
				validationContext.StatementRawInfo,
				validationContext.StatementCompileTimeService);
			return new ThreeFormScalarFactory(
				InitAndSingleParamReturnType(inputEventType, collectionComponentType),
				type,
				goesToNames.Count,
				SingleParamScalar(enumMethod));
		}
	}
} // end of namespace
