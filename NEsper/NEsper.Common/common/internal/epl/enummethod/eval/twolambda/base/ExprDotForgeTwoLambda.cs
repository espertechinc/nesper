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
using com.espertech.esper.common.@internal.epl.enummethod.dot;
using com.espertech.esper.common.@internal.epl.enummethod.eval;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.dot.core;
using com.espertech.esper.common.@internal.epl.methodbase;
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.enummethodeval.twolambda.@base
{
	public abstract class ExprDotForgeTwoLambda : ExprDotForgeEnumMethodBase
	{

		protected abstract EPType ReturnType(
			EventType inputEventType,
			Type collectionComponentType);

		protected abstract TwoLambdaThreeFormEventPlainFactory.ForgeFunction TwoParamEventPlain();
		protected abstract TwoLambdaThreeFormEventPlusFactory.ForgeFunction TwoParamEventPlus();
		protected abstract TwoLambdaThreeFormScalarFactory.ForgeFunction TwoParamScalar();

		public override EnumForgeDescFactory GetForgeFactory(
			DotMethodFP footprint,
			IList<ExprNode> parameters,
			EnumMethodEnum enumMethod,
			string enumMethodUsedName,
			EventType inputEventType,
			Type collectionComponentType,
			ExprValidationContext validationContext)
		{
			if (parameters.Count < 2) {
				throw new IllegalStateException();
			}

			EPType returnType = ReturnType(inputEventType, collectionComponentType);
			ExprLambdaGoesNode lambdaFirst = (ExprLambdaGoesNode) parameters[0];
			ExprLambdaGoesNode lambdaSecond = (ExprLambdaGoesNode) parameters[1];
			if (lambdaFirst.GoesToNames.Count != lambdaSecond.GoesToNames.Count) {
				throw new ExprValidationException(
					"Enumeration method '" + enumMethodUsedName + "' expected the same number of parameters for both the key and the value expression");
			}

			int numParameters = lambdaFirst.GoesToNames.Count;

			if (inputEventType != null) {
				string streamNameFirst = lambdaFirst.GoesToNames[0];
				string streamNameSecond = lambdaSecond.GoesToNames[0];
				if (numParameters == 1) {
					return new TwoLambdaThreeFormEventPlainFactory(inputEventType, streamNameFirst, streamNameSecond, returnType, TwoParamEventPlain());
				}

				IDictionary<string, object> fieldsFirst = new Dictionary<string, object>();
				IDictionary<string, object> fieldsSecond = new Dictionary<string, object>();
				fieldsFirst.Put(lambdaFirst.GoesToNames[1], typeof(int?));
				fieldsSecond.Put(lambdaSecond.GoesToNames[1], typeof(int?));
				if (numParameters > 2) {
					fieldsFirst.Put(lambdaFirst.GoesToNames[2], typeof(int?));
					fieldsSecond.Put(lambdaSecond.GoesToNames[2], typeof(int?));
				}

				ObjectArrayEventType typeFirst = ExprDotNodeUtility.MakeTransientOAType(
					enumMethodUsedName,
					fieldsFirst,
					validationContext.StatementRawInfo,
					validationContext.StatementCompileTimeService);
				ObjectArrayEventType typeSecond = ExprDotNodeUtility.MakeTransientOAType(
					enumMethodUsedName,
					fieldsSecond,
					validationContext.StatementRawInfo,
					validationContext.StatementCompileTimeService);
				return new TwoLambdaThreeFormEventPlusFactory(
					inputEventType,
					streamNameFirst,
					streamNameSecond,
					typeFirst,
					typeSecond,
					lambdaFirst.GoesToNames.Count,
					returnType,
					TwoParamEventPlus());
			}
			else {

				IDictionary<string, object> fieldsFirst = new Dictionary<string, object>();
				IDictionary<string, object> fieldsSecond = new Dictionary<string, object>();
				fieldsFirst.Put(lambdaFirst.GoesToNames[0], collectionComponentType);
				fieldsSecond.Put(lambdaSecond.GoesToNames[0], collectionComponentType);
				if (numParameters > 1) {
					fieldsFirst.Put(lambdaFirst.GoesToNames[1], typeof(int?));
					fieldsSecond.Put(lambdaSecond.GoesToNames[1], typeof(int?));
				}

				if (numParameters > 2) {
					fieldsFirst.Put(lambdaFirst.GoesToNames[2], typeof(int?));
					fieldsSecond.Put(lambdaSecond.GoesToNames[2], typeof(int?));
				}

				ObjectArrayEventType typeFirst = ExprDotNodeUtility.MakeTransientOAType(
					enumMethodUsedName,
					fieldsFirst,
					validationContext.StatementRawInfo,
					validationContext.StatementCompileTimeService);
				ObjectArrayEventType typeSecond = ExprDotNodeUtility.MakeTransientOAType(
					enumMethodUsedName,
					fieldsSecond,
					validationContext.StatementRawInfo,
					validationContext.StatementCompileTimeService);

				return new TwoLambdaThreeFormScalarFactory(
					typeFirst,
					typeSecond,
					lambdaFirst.GoesToNames.Count,
					returnType,
					TwoParamScalar());
			}
		}
	}
} // end of namespace
