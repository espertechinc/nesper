///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;

namespace com.espertech.esper.common.@internal.context.aifactory.update
{
    /// <summary>
    ///     Routing implementation that allows to pre-process events.
    /// </summary>
    public class InternalEventRouterDescFactory
    {
        public static InternalEventRouterDescForge GetValidatePreprocessing(
            EventType eventType, UpdateDesc desc, Attribute[] annotations)
        {
            if (!(eventType is EventTypeSPI)) {
                throw new ExprValidationException(
                    "Update statements require the event type to implement the " + typeof(EventTypeSPI) + " interface");
            }

            var eventTypeSPI = (EventTypeSPI) eventType;

            var size = desc.Assignments.Count;
            var wideners = new TypeWidenerSPI[size];
            var properties = new string[size];
            var expressions = new ExprNode[size];
            for (var i = 0; i < size; i++) {
                var onSet = desc.Assignments[i];
                var assignmentPair = ExprNodeUtilityValidate.CheckGetAssignmentToProp(onSet.Expression);
                if (assignmentPair == null) {
                    throw new ExprValidationException(
                        "Missing property assignment expression in assignment number " + i);
                }

                properties[i] = assignmentPair.First;
                expressions[i] = assignmentPair.Second;
                var writableProperty = eventTypeSPI.GetWritableProperty(assignmentPair.First);

                if (writableProperty == null) {
                    throw new ExprValidationException(
                        "Property '" + assignmentPair.First + "' is not available for write access");
                }

                try {
                    wideners[i] = TypeWidenerFactory.GetCheckPropertyAssignType(
                        ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(assignmentPair.Second),
                        assignmentPair.Second.Forge.EvaluationType,
                        writableProperty.PropertyType, assignmentPair.First, false, null, null);
                }
                catch (TypeWidenerException ex) {
                    throw new ExprValidationException(ex.Message, ex);
                }
            }

            // check copy-able
            var copyMethod = eventTypeSPI.GetCopyMethodForge(properties);
            if (copyMethod == null) {
                throw new ExprValidationException(
                    "The update-clause requires the underlying event representation to support copy (via Serializable by default)");
            }

            return new InternalEventRouterDescForge(
                copyMethod, wideners, eventType, annotations, desc.OptionalWhereClause,
                properties, expressions);
        }
    }
} // end of namespace