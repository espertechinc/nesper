///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.epl.expression.assign;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.visitor;
using com.espertech.esper.common.@internal.@event.avro;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.updatehelper
{
    public class EventBeanUpdateHelperForgeFactory
    {
        public static EventBeanUpdateHelperForge Make(
            string updatedWindowOrTableName,
            EventTypeSPI eventTypeSPI,
            IList<OnTriggerSetAssignment> assignments,
            string updatedAlias,
            EventType optionalTriggeringEventType,
            bool isCopyOnWrite,
            string statementName,
            EventTypeAvroHandler avroHandler)
        {
            IList<EventBeanUpdateItemForge> updateItems = new List<EventBeanUpdateItemForge>();
            IList<string> properties = new List<string>();

            var typeWidenerCustomizer = avroHandler.GetTypeWidenerCustomizer(eventTypeSPI);

            for (var i = 0; i < assignments.Count; i++) {
                var desc = assignments[i];
                var assignment = desc.Validated;
                if (assignment == null) {
                    throw new IllegalStateException("Assignment has not been validated");
                }

                try {
                    EventBeanUpdateItemForge updateItem;
                    if (assignment is ExprAssignmentStraight straight) {
                        // handle assignment "property = value"
                        if (straight.Lhs is ExprAssignmentLHSIdent ident) {
                            var propertyName = ident.Ident;
                            var writableProperty = eventTypeSPI.GetWritableProperty(propertyName);

                            // check assignment to indexed or mapped property
                            if (writableProperty == null) {
                                var nameWriteablePair = CheckIndexedOrMappedProp(
                                    propertyName,
                                    updatedWindowOrTableName,
                                    updatedAlias,
                                    eventTypeSPI);
                                propertyName = nameWriteablePair.First;
                                writableProperty = nameWriteablePair.Second;
                            }

                            var type = writableProperty.PropertyType;
                            var rhsExpr = straight.Rhs;
                            var rhsForge = rhsExpr.Forge;
                            var writer = eventTypeSPI.GetWriter(propertyName);
                            bool notNullableField = type.CanNotBeNull();

                            properties.Add(propertyName);
                            TypeWidenerSPI widener;
                            var evalType = rhsForge.EvaluationType;
                            try {
                                widener = TypeWidenerFactory.GetCheckPropertyAssignType(
                                    ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(rhsExpr),
                                    evalType,
                                    writableProperty.PropertyType,
                                    propertyName,
                                    false,
                                    typeWidenerCustomizer,
                                    statementName);
                            }
                            catch (TypeWidenerException ex) {
                                throw new ExprValidationException(ex.Message, ex);
                            }

                            // check event type assignment
                            var useUntypedAssignment = false;
                            var useTriggeringEvent = false;
                            if (optionalTriggeringEventType != null) {
                                // handle RHS is ident node
                                if (rhsExpr is ExprIdentNode node) {
                                    var fragmentRHS =
                                        optionalTriggeringEventType.GetFragmentType(node.ResolvedPropertyName);
                                    var fragmentLHS = eventTypeSPI.GetFragmentType(propertyName);
                                    if (fragmentRHS != null && fragmentLHS != null) {
                                        if (!EventTypeUtility.IsTypeOrSubTypeOf(
                                                fragmentRHS.FragmentType,
                                                fragmentLHS.FragmentType)) {
                                            throw MakeEventTypeMismatch(
                                                propertyName,
                                                fragmentLHS.FragmentType,
                                                fragmentRHS.FragmentType);
                                        }
                                    }

                                    // we don't need to cast if it is a self-assignment and LHS is an event and target needs no writer
                                    if (node.StreamId == 0 &&
                                        fragmentLHS != null &&
                                        eventTypeSPI is BaseNestableEventType) {
                                        useUntypedAssignment = true;
                                    }
                                }

                                // handle RHS is a stream of the triggering event itself
                                if (rhsExpr is ExprStreamUnderlyingNode und) {
                                    if (und.StreamId == 1) {
                                        var fragmentLHS = eventTypeSPI.GetFragmentType(propertyName);
                                        if (fragmentLHS != null &&
                                            optionalTriggeringEventType is BaseNestableEventType &&
                                            !EventTypeUtility.IsTypeOrSubTypeOf(
                                                optionalTriggeringEventType,
                                                fragmentLHS.FragmentType)) {
                                            throw MakeEventTypeMismatch(
                                                propertyName,
                                                fragmentLHS.FragmentType,
                                                optionalTriggeringEventType);
                                        }

                                        // we use the event itself for assignment and target needs no writer
                                        if (eventTypeSPI is BaseNestableEventType) {
                                            useUntypedAssignment = true;
                                            useTriggeringEvent = true;
                                        }
                                    }
                                }
                            }

                            updateItem = new EventBeanUpdateItemForge(
                                rhsForge,
                                propertyName,
                                writer,
                                notNullableField,
                                widener,
                                useUntypedAssignment,
                                useTriggeringEvent,
                                null);
                        }
                        else if (straight.Lhs is ExprAssignmentLHSArrayElement arrayElementLhs) {
                            // handle "property[expr] = value"
                            var arrayPropertyName = arrayElementLhs.Ident;
                            var rhs = straight.Rhs;
                            var evaluationType = rhs.Forge.EvaluationType;
                            var propertyType = eventTypeSPI.GetPropertyType(arrayPropertyName);
                            if (!eventTypeSPI.IsProperty(arrayPropertyName)) {
                                throw new ExprValidationException(
                                    "Property '" + arrayPropertyName + "' could not be found");
                            }

                            if (propertyType == null || !propertyType.IsArray) {
                                throw new ExprValidationException(
                                    "Property '" + arrayPropertyName + "' is not an array");
                            }

                            if (evaluationType == null) {
                                throw new ExprValidationException(
                                    "Right-hand-side evaluation returns null-typed value for '" +
                                    arrayPropertyName +
                                    "'");
                            }

                            var evaluationClass = evaluationType;
                            var propertyClass = propertyType;
                            var getter = eventTypeSPI.GetGetterSPI(arrayPropertyName);
                            Type componentType = propertyClass.GetComponentType();
                            if (!evaluationClass.IsAssignmentCompatible(componentType)) {
                                throw new ExprValidationException(
                                    "Invalid assignment to property '" +
                                    arrayPropertyName +
                                    "' component type '" +
                                    componentType.CleanName() +
                                    "' from expression returning '" +
                                    evaluationType.CleanName() +
                                    "'");
                            }

                            TypeWidenerSPI widener;
                            try {
                                widener = TypeWidenerFactory.GetCheckPropertyAssignType(
                                    ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(straight.Rhs),
                                    evaluationType,
                                    componentType,
                                    arrayPropertyName,
                                    false,
                                    typeWidenerCustomizer,
                                    statementName);
                            }
                            catch (TypeWidenerException ex) {
                                throw new ExprValidationException(ex.Message, ex);
                            }

                            var arrayInfo = new EventBeanUpdateItemArray(
                                arrayPropertyName,
                                arrayElementLhs.IndexExpression,
                                propertyClass,
                                getter);
                            updateItem = new EventBeanUpdateItemForge(
                                rhs.Forge,
                                arrayPropertyName,
                                null,
                                false,
                                widener,
                                false,
                                false,
                                arrayInfo);
                        }
                        else {
                            throw new IllegalStateException("Unrecognized LHS assignment " + straight);
                        }
                    }
                    else if (assignment is ExprAssignmentCurly dot) {
                        // handle non-assignment, i.e. UDF or other expression
                        updateItem = new EventBeanUpdateItemForge(
                            dot.Expression.Forge,
                            null,
                            null,
                            false,
                            null,
                            false,
                            false,
                            null);
                    }
                    else {
                        throw new IllegalStateException("Unrecognized assignment " + assignment);
                    }

                    updateItems.Add(updateItem);
                }
                catch (ExprValidationException ex) {
                    throw new ExprValidationException(
                        "Failed to validate assignment expression '" +
                        ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(assignment.OriginalExpression) +
                        "': " +
                        ex.Message,
                        ex);
                }
            }

            // copy-on-write is the default event semantics as events are immutable
            EventBeanCopyMethodForge copyMethod;
            if (isCopyOnWrite) {
                // obtain copy method
                IList<string> propertiesUniqueList = new List<string>(new HashSet<string>(properties));
                var propertiesArray = propertiesUniqueList.ToArray();
                copyMethod = eventTypeSPI.GetCopyMethodForge(propertiesArray);
                if (copyMethod == null) {
                    throw new ExprValidationException("Event type does not support event bean copy");
                }
            }
            else {
                // for in-place update, determine assignment expressions to use "initial" to access prior-change values
                // the copy-method is optional
                copyMethod = null;
                var propertiesInitialValue = DeterminePropertiesInitialValue(assignments);
                if (!propertiesInitialValue.IsEmpty()) {
                    var propertiesInitialValueArray = propertiesInitialValue.ToArray();
                    copyMethod = eventTypeSPI.GetCopyMethodForge(propertiesInitialValueArray);
                }
            }

            var updateItemsArray = updateItems.ToArray();
            return new EventBeanUpdateHelperForge(eventTypeSPI, copyMethod, updateItemsArray);
        }

        private static ExprValidationException MakeEventTypeMismatch(
            string propertyName,
            EventType lhs,
            EventType rhs)
        {
            return new ExprValidationException(
                "Invalid assignment to property '" +
                propertyName +
                "' event type '" +
                lhs.Name +
                "' from event type '" +
                rhs.Name +
                "'");
        }

        private static ISet<string> DeterminePropertiesInitialValue(IList<OnTriggerSetAssignment> assignments)
        {
            ISet<string> props = new HashSet<string>();
            var visitor = new ExprNodeIdentifierCollectVisitor();
            foreach (var assignment in assignments) {
                assignment.Validated.Accept(visitor);
                foreach (var node in visitor.ExprProperties) {
                    if (node.StreamId == 2) {
                        props.Add(node.ResolvedPropertyName);
                    }
                }

                visitor.Reset();
            }

            return props;
        }

        private static Pair<string, EventPropertyDescriptor> CheckIndexedOrMappedProp(
            string propertyName,
            string updatedWindowOrTableName,
            string namedWindowAlias,
            EventTypeSPI eventTypeSPI)
        {
            EventPropertyDescriptor writableProperty = null;

            var indexDot = propertyName.IndexOf(".");
            if (namedWindowAlias != null && indexDot != -1) {
                var prefix = StringValue.UnescapeBacktick(propertyName.Substring(0, indexDot));
                var name = propertyName.Substring(indexDot + 1);
                if (prefix.Equals(namedWindowAlias)) {
                    writableProperty = eventTypeSPI.GetWritableProperty(name);
                    propertyName = name;
                }
            }

            if (writableProperty == null && indexDot != -1) {
                var prefix = propertyName.Substring(0, indexDot);
                var name = propertyName.Substring(indexDot + 1);
                if (prefix.Equals(updatedWindowOrTableName)) {
                    writableProperty = eventTypeSPI.GetWritableProperty(name);
                    propertyName = name;
                }
            }

            if (writableProperty == null) {
                throw new ExprValidationException("Property '" + propertyName + "' is not available for write access");
            }

            return new Pair<string, EventPropertyDescriptor>(propertyName, writableProperty);
        }
    }
} // end of namespace