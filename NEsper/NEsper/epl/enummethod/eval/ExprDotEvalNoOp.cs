///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.enummethod.dot;
using com.espertech.esper.epl.rettype;
using com.espertech.esper.events;

namespace com.espertech.esper.epl.enummethod.eval
{
    public class ExprDotEvalNoOp : ExprDotEvalEnumMethodBase
    {
        private EventType _resultEventType;

        public override EventType[] GetAddStreamTypes(String enumMethodUsedName, IList<String> goesToNames, EventType inputEventType, Type collectionComponentType, IList<ExprDotEvalParam> bodiesAndParameters, EventAdapterService eventAdapterService)
        {
            return new EventType[] {};
        }
    
        public override EnumEval GetEnumEval(MethodResolutionService methodResolutionService, EventAdapterService eventAdapterService, StreamTypeService streamTypeService, String statementId, String enumMethodUsedName, IList<ExprDotEvalParam> bodiesAndParameters, EventType inputEventType, Type collectionComponentType, int numStreamsIncoming, bool disablePropertyExpressionEventCollCache)
        {
            TypeInfo = EPTypeHelper.CollectionOfEvents(inputEventType);
            _resultEventType = inputEventType;
            return new EnumEvalNoOp(numStreamsIncoming);
        }
    }
}
