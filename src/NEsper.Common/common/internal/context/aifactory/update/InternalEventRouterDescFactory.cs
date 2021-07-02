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
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.epl.expression.assign;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.context.aifactory.update
{
	/// <summary>
	///     Routing implementation that allows to pre-process events.
	/// </summary>
	public class InternalEventRouterDescFactory
    {
        public static InternalEventRouterDescForge GetValidatePreprocessing(
            EventType eventType,
            UpdateDesc desc,
            Attribute[] annotations)
        {
            if (!(eventType is EventTypeSPI)) {
                throw new ExprValidationException("Update statements require the event type to implement the " + typeof(EventTypeSPI) + " interface");
            }

            var eventTypeSPI = (EventTypeSPI) eventType;

            var wideners = new List<TypeWidenerSPI>();
            var properties = new List<string>();
            var propertiesTouched = new List<string>();
            var expressions = new List<ExprNode>();
            var specialWriters = new List<InternalEventRouterWriterForge>();

            for (var i = 0; i < desc.Assignments.Count; i++) {
                var onSet = desc.Assignments[i];
                var assignmentDesc = onSet.Validated;

                try {
                    if (assignmentDesc is ExprAssignmentStraight assignment) {
                        var lhs = assignment.Lhs;

                        if (lhs is ExprAssignmentLHSIdent ident) {
                            var propertyName = ident.Ident;
                            var writableProperty = eventTypeSPI.GetWritableProperty(propertyName);
                            if (writableProperty == null) {
                                throw new ExprValidationException("Property '" + propertyName + "' is not available for write access");
                            }

                            TypeWidenerSPI widener;
                            try {
                                widener = TypeWidenerFactory.GetCheckPropertyAssignType(
                                    ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(assignment.Rhs),
                                    assignment.Rhs.Forge.EvaluationType,
                                    writableProperty.PropertyType,
                                    propertyName,
                                    false,
                                    null,
                                    null);
                            }
                            catch (TypeWidenerException ex) {
                                throw new ExprValidationException(ex.Message, ex);
                            }

                            properties.Add(propertyName);
                            propertiesTouched.Add(propertyName);
                            expressions.Add(assignment.Rhs);
                            wideners.Add(widener);
                        }
                        else if (lhs is ExprAssignmentLHSIdentWSubprop subprop) {
                            throw new ExprValidationException("Property '" + subprop.SubpropertyName + "' is not available for write access");
                        }
                        else if (lhs is ExprAssignmentLHSArrayElement arrayElement) {
                            var propertyName = lhs.Ident;
                            var writableProperty = eventTypeSPI.GetWritableProperty(propertyName);
                            if (writableProperty == null) {
                                throw new ExprValidationException("Property '" + propertyName + "' is not available for write access");
                            }

                            var writablePropertyType = writableProperty.PropertyType;
                            if (writablePropertyType.IsNullTypeSafe() || !writablePropertyType.IsArray) {
                                throw new ExprValidationException("Property '" + propertyName + "' type is not array");
                            }

                            TypeWidenerSPI widener;
                            try {
                                widener = TypeWidenerFactory.GetCheckPropertyAssignType(
                                    ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(assignment.Rhs),
                                    assignment.Rhs.Forge.EvaluationType,
                                    writablePropertyType.GetElementType(),
                                    propertyName,
                                    false,
                                    null,
                                    null);
                            }
                            catch (TypeWidenerException ex) {
                                throw new ExprValidationException(ex.Message, ex);
                            }

                            var special = new InternalEventRouterWriterArrayElementForge(
                                arrayElement.IndexExpression,
                                assignment.Rhs,
                                widener,
                                propertyName);
                            specialWriters.Add(special);
                        }
                        else {
                            throw new IllegalStateException("Unrecognized left hande side assignment " + lhs);
                        }
                    }
                    else if (assignmentDesc is ExprAssignmentCurly curly) {
                        var special = new InternalEventRouterWriterCurlyForge(curly.Expression);
                        specialWriters.Add(special);
                    }
                    else {
                        throw new IllegalStateException("Unrecognized assignment " + assignmentDesc);
                    }
                }
                catch (ExprValidationException ex) {
                    throw new ExprValidationException(
                        "Failed to validate assignment expression '" +
                        ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(assignmentDesc.OriginalExpression) +
                        "': " +
                        ex.Message,
                        ex);
                }
            }

            // check copy-able
            var copyMethod = eventTypeSPI.GetCopyMethodForge(propertiesTouched.ToArray());
            if (copyMethod == null) {
                throw new ExprValidationException(
                    "The update-clause requires the underlying event representation to support copy (via Serializable by default)");
            }

            return new InternalEventRouterDescForge(
                copyMethod,
                wideners.ToArray(),
                eventType,
                annotations,
                desc.OptionalWhereClause,
                properties.ToArray(),
                expressions.ToArray(),
                specialWriters.ToArray());
        }
    }
} // end of namespace