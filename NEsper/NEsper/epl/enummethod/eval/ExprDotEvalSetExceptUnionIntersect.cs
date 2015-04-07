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
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.dot;
using com.espertech.esper.epl.rettype;
using com.espertech.esper.events;

namespace com.espertech.esper.epl.enummethod.eval
{
    public class ExprDotEvalSetExceptUnionIntersect : ExprDotEvalEnumMethodBase
    {
        public override EventType[] GetAddStreamTypes(
            string enumMethodUsedName,
            IList<string> goesToNames,
            EventType inputEventType,
            Type collectionComponentType,
            IList<ExprDotEvalParam> bodiesAndParameters,
            EventAdapterService eventAdapterService)
        {
            return new EventType[]
            {
            };
        }

        public override EnumEval GetEnumEval(
            MethodResolutionService methodResolutionService,
            EventAdapterService eventAdapterService,
            StreamTypeService streamTypeService,
            String statementId,
            String enumMethodUsedName,
            IList<ExprDotEvalParam> bodiesAndParameters,
            EventType inputEventType,
            Type collectionComponentType,
            int numStreamsIncoming,
            bool disablePropertyExpressionEventCollCache)
        {
            ExprDotEvalParam first = bodiesAndParameters[0];

            ExprDotEnumerationSource enumSrc = ExprDotNodeUtility.GetEnumerationSource(
                first.Body, streamTypeService, eventAdapterService, statementId, true,
                disablePropertyExpressionEventCollCache);
            if (inputEventType != null)
            {
                base.TypeInfo = EPTypeHelper.CollectionOfEvents(inputEventType);
            }
            else
            {
                base.TypeInfo = EPTypeHelper.CollectionOfSingleValue(collectionComponentType);
            }

            if (enumSrc.Enumeration == null)
            {
                String message = "Enumeration method '" + enumMethodUsedName +
                                 "' requires an expression yielding an event-collection as input paramater";
                throw new ExprValidationException(message);
            }

            var setType = enumSrc.Enumeration.GetEventTypeCollection(eventAdapterService, statementId);
            if (!Equals(setType, inputEventType))
            {
                bool isSubtype = EventTypeUtility.IsTypeOrSubTypeOf(setType, inputEventType);
                if (!isSubtype)
                {
                    String message = "Enumeration method '" + enumMethodUsedName + "' expects event type '" +
                                     inputEventType.Name + "' but receives event type '" +
                                     enumSrc.Enumeration.GetEventTypeCollection(eventAdapterService, statementId).Name +
                                     "'";
                    throw new ExprValidationException(message);
                }
            }

            if (EnumMethodEnum == EnumMethodEnum.UNION)
            {
                return new EnumEvalUnion(numStreamsIncoming, enumSrc.Enumeration, inputEventType == null);
            }
            else if (EnumMethodEnum == EnumMethodEnum.INTERSECT)
            {
                return new EnumEvalIntersect(numStreamsIncoming, enumSrc.Enumeration, inputEventType == null);
            }
            else if (EnumMethodEnum == EnumMethodEnum.EXCEPT)
            {
                return new EnumEvalExcept(numStreamsIncoming, enumSrc.Enumeration, inputEventType == null);
            }
            else
            {
                throw new ArgumentException("Invalid enumeration method for this factory: " + EnumMethodEnum);
            }
        }
    }
}