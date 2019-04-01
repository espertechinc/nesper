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
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.enummethod.dot;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.dot.core;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval
{
	public class ExprDotForgeAggregate : ExprDotForgeEnumMethodBase {

	    public override EventType[] GetAddStreamTypes(string enumMethodUsedName, IList<string> goesToNames, EventType inputEventType, Type collectionComponentType, IList<ExprDotEvalParam> bodiesAndParameters, StatementRawInfo statementRawInfo, StatementCompileTimeServices services) {
	        EventType evalEventType;
	        if (inputEventType == null) {
	            evalEventType = ExprDotNodeUtility.MakeTransientOAType(enumMethodUsedName, goesToNames.Get(1), collectionComponentType, statementRawInfo, services);
	        } else {
	            evalEventType = inputEventType;
	        }

	        Type initializationType = bodiesAndParameters[0].BodyForge.EvaluationType;
	        EventType typeResult = ExprDotNodeUtility.MakeTransientOAType(enumMethodUsedName, goesToNames[0], initializationType, statementRawInfo, services);

	        return new EventType[]{typeResult, evalEventType};
	    }

	    public override EnumForge GetEnumForge(StreamTypeService streamTypeService, string enumMethodUsedName, IList<ExprDotEvalParam> bodiesAndParameters, EventType inputEventType, Type collectionComponentType, int numStreamsIncoming, bool disablePropertyExpressionEventCollCache, StatementRawInfo statementRawInfo, StatementCompileTimeServices services) {
	        ExprDotEvalParam initValueParam = bodiesAndParameters[0];
	        ExprForge initValueEval = initValueParam.BodyForge;
	        base.TypeInfo = EPTypeHelper.SingleValue(Boxing.GetBoxedType(initValueEval.EvaluationType));

	        ExprDotEvalParamLambda resultAndAdd = (ExprDotEvalParamLambda) bodiesAndParameters.Get(1);

	        if (inputEventType != null) {
	            return new EnumAggregateEventsForge(initValueEval,
	                    resultAndAdd.BodyForge, resultAndAdd.StreamCountIncoming,
	                    (ObjectArrayEventType) resultAndAdd.GoesToTypes[0]);
	        } else {
	            return new EnumAggregateScalarForge(initValueEval,
	                    resultAndAdd.BodyForge, resultAndAdd.StreamCountIncoming,
	                    (ObjectArrayEventType) resultAndAdd.GoesToTypes[0],
	                    (ObjectArrayEventType) resultAndAdd.GoesToTypes[1]);
	        }
	    }
	}
} // end of namespace