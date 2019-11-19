///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.collection;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.ops;
using com.espertech.esper.common.@internal.epl.expression.visitor;
using com.espertech.esper.common.@internal.@event.avro;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;
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
                var assignment = assignments[i];
                EventBeanUpdateItemForge updateItem;

                // determine whether this is a "property=value" assignment, we use property setters in this case
                var possibleAssignment = ExprNodeUtilityValidate.CheckGetAssignmentToProp(assignment.Expression);

                // handle assignment "property = value"
                if (possibleAssignment != null) {
                    var propertyName = possibleAssignment.First;
                    var writableProperty = eventTypeSPI.GetWritableProperty(propertyName);

                    // check assignment to indexed or mapped property
                    if (writableProperty == null) {
                        var nameWriteablePair = CheckIndexedOrMappedProp(
                            possibleAssignment.First,
                            updatedWindowOrTableName,
                            updatedAlias,
                            eventTypeSPI);
                        propertyName = nameWriteablePair.First;
                        writableProperty = nameWriteablePair.Second;
                    }

                    var evaluator = possibleAssignment.Second.Forge;
                    var writers = eventTypeSPI.GetWriter(propertyName);
                    var notNullableField = writableProperty.PropertyType.IsValueType;

                    properties.Add(propertyName);
                    TypeWidenerSPI widener;
                    try {
                        widener = TypeWidenerFactory.GetCheckPropertyAssignType(
                            ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(possibleAssignment.Second),
                            possibleAssignment.Second.Forge.EvaluationType,
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
                    if (optionalTriggeringEventType != null && possibleAssignment.Second is ExprIdentNode) {
                        var node = (ExprIdentNode) possibleAssignment.Second;
                        var fragmentRHS = optionalTriggeringEventType.GetFragmentType(node.ResolvedPropertyName);
                        var fragmentLHS = eventTypeSPI.GetFragmentType(possibleAssignment.First);
                        if (fragmentRHS != null &&
                            fragmentLHS != null &&
                            !EventTypeUtility.IsTypeOrSubTypeOf(
                                fragmentRHS.FragmentType,
                                fragmentLHS.FragmentType)) {
                            throw new ExprValidationException(
                                "Invalid assignment to property '" +
                                possibleAssignment.First +
                                "' event type '" +
                                fragmentLHS.FragmentType.Name +
                                "' from event type '" +
                                fragmentRHS.FragmentType.Name +
                                "'");
                        }
                    }

                    updateItem = new EventBeanUpdateItemForge(
                        evaluator,
                        propertyName,
                        writers,
                        notNullableField,
                        widener);
                }
                else {
                    // handle non-assignment, i.e. UDF or other expression
                    updateItem = new EventBeanUpdateItemForge(assignment.Expression.Forge, null, null, false, null);
                }

                updateItems.Add(updateItem);
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

        private static ISet<string> DeterminePropertiesInitialValue(IList<OnTriggerSetAssignment> assignments)
        {
            ISet<string> props = new HashSet<string>();
            var visitor = new ExprNodeIdentifierCollectVisitor();
            foreach (var assignment in assignments) {
                if (assignment.Expression is ExprEqualsNode) {
                    assignment.Expression.ChildNodes[1].Accept(visitor);
                }
                else {
                    assignment.Expression.Accept(visitor);
                }

                foreach (var node in visitor.ExprProperties) {
                    if (node.StreamId == 2) {
                        props.Add(node.ResolvedPropertyName);
                    }
                }
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