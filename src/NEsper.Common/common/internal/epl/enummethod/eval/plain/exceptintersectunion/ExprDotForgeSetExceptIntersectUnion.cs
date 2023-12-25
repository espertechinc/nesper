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
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.dot.core;
using com.espertech.esper.common.@internal.epl.methodbase;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;


namespace com.espertech.esper.common.@internal.epl.enummethod.eval.plain.exceptintersectunion
{
    public partial class ExprDotForgeSetExceptIntersectUnion : ExprDotForgeEnumMethodBase
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
            var first = parameters[0];

            var enumSrc = ExprDotNodeUtility.GetEnumerationSource(
                first,
                validationContext.StreamTypeService,
                true,
                validationContext.IsDisablePropertyExpressionEventCollCache,
                validationContext.StatementRawInfo,
                validationContext.StatementCompileTimeService);
            EPChainableType type;
            if (inputEventType != null) {
                type = EPChainableTypeHelper.CollectionOfEvents(inputEventType);
            }
            else {
                type = EPChainableTypeHelper.CollectionOfSingleValue(collectionComponentType);
            }

            if (inputEventType != null) {
                var setType = enumSrc.Enumeration?.GetEventTypeCollection(
                    validationContext.StatementRawInfo,
                    validationContext.StatementCompileTimeService);
                if (setType == null) {
                    var message = "Enumeration method '" +
                                  enumMethodUsedName +
                                  "' requires an expression yielding a " +
                                  "collection of events of type '" +
                                  inputEventType.Name +
                                  "' as input parameter";
                    throw new ExprValidationException(message);
                }

                if (setType != inputEventType) {
                    var isSubtype = EventTypeUtility.IsTypeOrSubTypeOf(setType, inputEventType);
                    if (!isSubtype) {
                        var message = "Enumeration method '" +
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
                Type setType;
                if (enumSrc.Enumeration == null || enumSrc.Enumeration.ComponentTypeCollection == null) {
                    setType = null;
                }
                else {
                    setType = enumSrc.Enumeration.ComponentTypeCollection;
                }

                if (setType == null) {
                    var message = "Enumeration method '" +
                                  enumMethodUsedName +
                                  "' requires an expression yielding a " +
                                  "collection of values of type '" +
                                  collectionComponentType.CleanName() +
                                  "' as input parameter";
                    throw new ExprValidationException(message);
                }

                if (!setType.IsAssignmentCompatible(collectionComponentType)) {
                    var message = "Enumeration method '" +
                                  enumMethodUsedName +
                                  "' expects scalar type '" +
                                  collectionComponentType.CleanName() +
                                  "' but receives event type '" +
                                  setType.CleanName() +
                                  "'";
                    throw new ExprValidationException(message);
                }
            }

            return new EnumForgeDescFactoryEIU(enumMethod, type, enumSrc);
        }
    }
} // end of namespace