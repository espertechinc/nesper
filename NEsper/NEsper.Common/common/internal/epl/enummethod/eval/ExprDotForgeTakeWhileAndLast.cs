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
using com.espertech.esper.common.@internal.epl.expression.dot.core;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval
{
	public class ExprDotForgeTakeWhileAndLast : ExprDotForgeEnumMethodBase {

	    public override EventType[] GetAddStreamTypes(string enumMethodUsedName, IList<string> goesToNames, EventType inputEventType, Type collectionComponentType, IList<ExprDotEvalParam> bodiesAndParameters, StatementRawInfo statementRawInfo, StatementCompileTimeServices services) {
	        EventType firstParamType;
	        if (inputEventType == null) {
	            firstParamType = ExprDotNodeUtility.MakeTransientOAType(enumMethodUsedName, goesToNames[0], collectionComponentType, statementRawInfo, services);
	        } else {
	            firstParamType = inputEventType;
	        }

	        if (goesToNames.Count == 1) {
	            return new EventType[]{firstParamType};
	        }

	        ObjectArrayEventType indexEventType = ExprDotNodeUtility.MakeTransientOAType(enumMethodUsedName, goesToNames.Get(1), typeof(int), statementRawInfo, services);
	        return new EventType[]{firstParamType, indexEventType};
	    }

	    public override EnumForge GetEnumForge(StreamTypeService streamTypeService, string enumMethodUsedName, IList<ExprDotEvalParam> bodiesAndParameters, EventType inputEventType, Type collectionComponentType, int numStreamsIncoming, bool disablePropertyExpressionEventCollCache, StatementRawInfo statementRawInfo, StatementCompileTimeServices services) {

	        ExprDotEvalParamLambda first = (ExprDotEvalParamLambda) bodiesAndParameters[0];

	        if (inputEventType != null) {
	            base.TypeInfo = EPTypeHelper.CollectionOfEvents(inputEventType);
	            if (first.GoesToNames.Count == 1) {
	                if (this.EnumMethodEnum == EnumMethodEnum.TAKEWHILELAST) {
	                    return new EnumTakeWhileLastEventsForge(first.BodyForge, first.StreamCountIncoming);
	                }
	                return new EnumTakeWhileEventsForge(first.BodyForge, first.StreamCountIncoming);
	            }

	            if (this.EnumMethodEnum == EnumMethodEnum.TAKEWHILELAST) {
	                return new EnumTakeWhileLastIndexEventsForge(first.BodyForge, first.StreamCountIncoming, (ObjectArrayEventType) first.GoesToTypes[1]);
	            }
	            return new EnumTakeWhileIndexEventsForge(first.BodyForge, first.StreamCountIncoming, (ObjectArrayEventType) first.GoesToTypes[1]);
	        }

	        base.TypeInfo = EPTypeHelper.CollectionOfSingleValue(collectionComponentType);
	        if (first.GoesToNames.Count == 1) {
	            if (this.EnumMethodEnum == EnumMethodEnum.TAKEWHILELAST) {
	                return new EnumTakeWhileLastScalarForge(first.BodyForge, first.StreamCountIncoming, (ObjectArrayEventType) first.GoesToTypes[0]);
	            }
	            return new EnumTakeWhileScalarForge(first.BodyForge, first.StreamCountIncoming, (ObjectArrayEventType) first.GoesToTypes[0]);
	        }

	        if (this.EnumMethodEnum == EnumMethodEnum.TAKEWHILELAST) {
	            return new EnumTakeWhileLastIndexScalarForge(first.BodyForge, first.StreamCountIncoming, (ObjectArrayEventType) first.GoesToTypes[0], (ObjectArrayEventType) first.GoesToTypes[1]);
	        }
	        return new EnumTakeWhileIndexScalarForge(first.BodyForge, first.StreamCountIncoming, (ObjectArrayEventType) first.GoesToTypes[0], (ObjectArrayEventType) first.GoesToTypes[1]);
	    }
	}
} // end of namespace