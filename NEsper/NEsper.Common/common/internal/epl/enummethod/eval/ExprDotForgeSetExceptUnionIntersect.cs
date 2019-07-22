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
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval
{
    public class ExprDotForgeSetExceptUnionIntersect : ExprDotForgeEnumMethodBase
    {
        public override EventType[] GetAddStreamTypes(
            string enumMethodUsedName,
            IList<string> goesToNames,
            EventType inputEventType,
            Type collectionComponentType,
            IList<ExprDotEvalParam> bodiesAndParameters,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services)
        {
            return new EventType[] { };
        }

        public override EnumForge GetEnumForge(
            StreamTypeService streamTypeService,
            string enumMethodUsedName,
            IList<ExprDotEvalParam> bodiesAndParameters,
            EventType inputEventType,
            Type collectionComponentType,
            int numStreamsIncoming,
            bool disablePropertyExpressionEventCollCache,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services)
        {
            ExprDotEvalParam first = bodiesAndParameters[0];

            ExprDotEnumerationSourceForge enumSrc = ExprDotNodeUtility.GetEnumerationSource(
                first.Body,
                streamTypeService,
                true,
                disablePropertyExpressionEventCollCache,
                statementRawInfo,
                services);
            if (inputEventType != null) {
                base.TypeInfo = EPTypeHelper.CollectionOfEvents(inputEventType);
            }
            else {
                base.TypeInfo = EPTypeHelper.CollectionOfSingleValue(collectionComponentType);
            }

            if (inputEventType != null) {
                EventType setType = enumSrc.Enumeration == null
                    ? null
                    : enumSrc.Enumeration.GetEventTypeCollection(statementRawInfo, services);
                if (setType == null) {
                    string message = "Enumeration method '" +
                                     enumMethodUsedName +
                                     "' requires an expression yielding a " +
                                     "collection of events of type '" +
                                     inputEventType.Name +
                                     "' as input parameter";
                    throw new ExprValidationException(message);
                }

                if (setType != inputEventType) {
                    bool isSubtype = EventTypeUtility.IsTypeOrSubTypeOf(setType, inputEventType);
                    if (!isSubtype) {
                        string message = "Enumeration method '" +
                                         enumMethodUsedName +
                                         "' expects event type '" +
                                         inputEventType.Name +
                                         "' but receives event type '" +
                                         setType.Name +
                                         "'";
                        throw new ExprValidationException(message);
                    }
                }
            }
            else {
                Type setType = enumSrc.Enumeration == null ? null : enumSrc.Enumeration.ComponentTypeCollection;
                if (setType == null) {
                    string message = "Enumeration method '" +
                                     enumMethodUsedName +
                                     "' requires an expression yielding a " +
                                     "collection of values of type '" +
                                     collectionComponentType.Name +
                                     "' as input parameter";
                    throw new ExprValidationException(message);
                }

                if (!TypeHelper.IsAssignmentCompatible(setType, collectionComponentType)) {
                    string message = "Enumeration method '" +
                                     enumMethodUsedName +
                                     "' expects scalar type '" +
                                     collectionComponentType.Name +
                                     "' but receives event type '" +
                                     setType.Name +
                                     "'";
                    throw new ExprValidationException(message);
                }
            }

            if (this.EnumMethodEnum == EnumMethodEnum.UNION) {
                return new EnumUnionForge(numStreamsIncoming, enumSrc.Enumeration, inputEventType == null);
            }
            else if (this.EnumMethodEnum == EnumMethodEnum.INTERSECT) {
                return new EnumIntersectForge(numStreamsIncoming, enumSrc.Enumeration, inputEventType == null);
            }
            else if (this.EnumMethodEnum == EnumMethodEnum.EXCEPT) {
                return new EnumExceptForge(numStreamsIncoming, enumSrc.Enumeration, inputEventType == null);
            }
            else {
                throw new ArgumentException("Invalid enumeration method for this factory: " + this.EnumMethodEnum);
            }
        }
    }
} // end of namespace