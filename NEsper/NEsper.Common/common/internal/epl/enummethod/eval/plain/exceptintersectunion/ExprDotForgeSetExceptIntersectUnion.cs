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
using com.espertech.esper.common.@internal.compile.stage3;
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
    public class ExprDotForgeSetExceptIntersectUnion : ExprDotForgeEnumMethodBase
    {
        public override EnumForgeDescFactory GetForgeFactory(
            DotMethodFP footprint,
            IList<ExprNode> parameters,
            EnumMethodEnum enumMethod,
            String enumMethodUsedName,
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

            EPType type;
            if (inputEventType != null) {
                type = EPTypeHelper.CollectionOfEvents(inputEventType);
            }
            else {
                type = EPTypeHelper.CollectionOfSingleValue(collectionComponentType, null);
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
                var setType = enumSrc.Enumeration?.ComponentTypeCollection;
                if (setType == null) {
                    var message = "Enumeration method '" +
                                  enumMethodUsedName +
                                  "' requires an expression yielding a " +
                                  "collection of values of type '" +
                                  collectionComponentType.Name +
                                  "' as input parameter";
                    throw new ExprValidationException(message);
                }

                if (!setType.IsAssignmentCompatible(collectionComponentType)) {
                    var message = "Enumeration method '" +
                                  enumMethodUsedName +
                                  "' expects scalar type '" +
                                  collectionComponentType.Name +
                                  "' but receives event type '" +
                                  setType.Name +
                                  "'";
                    throw new ExprValidationException(message);
                }
            }

            return new EnumForgeDescFactoryEIU(enumMethod, type, enumSrc);
        }

        private class EnumForgeDescFactoryEIU : EnumForgeDescFactory
        {
            private readonly EnumMethodEnum _enumMethod;
            private readonly EPType _type;
            private readonly ExprDotEnumerationSourceForge _enumSrc;

            public EnumForgeDescFactoryEIU(
                EnumMethodEnum enumMethod,
                EPType type,
                ExprDotEnumerationSourceForge enumSrc)
            {
                _enumMethod = enumMethod;
                _type = type;
                _enumSrc = enumSrc;
            }

            public EnumForgeLambdaDesc GetLambdaStreamTypesForParameter(int parameterNum)
            {
                throw new IllegalStateException("No lambda expected");
            }

            public EnumForgeDesc MakeEnumForgeDesc(
                IList<ExprDotEvalParam> bodiesAndParameters,
                int streamCountIncoming,
                StatementCompileTimeServices services)
            {
                var scalar = _type is ClassMultiValuedEPType;
                EnumForge forge = _enumMethod switch {
                    EnumMethodEnum.UNION => new EnumUnionForge(streamCountIncoming, _enumSrc.Enumeration, scalar),
                    EnumMethodEnum.INTERSECT => new EnumIntersectForge(streamCountIncoming, _enumSrc.Enumeration, scalar),
                    EnumMethodEnum.EXCEPT => new EnumExceptForge(streamCountIncoming, _enumSrc.Enumeration, scalar),
                    _ => throw new ArgumentException("Invalid enumeration method for this factory: " + _enumMethod)
                };

                return new EnumForgeDesc(_type, forge);
            }
        }
    }
} // end of namespace