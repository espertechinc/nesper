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

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.ops;
using com.espertech.esper.epl.expression.visitor;
using com.espertech.esper.epl.spec;
using com.espertech.esper.events;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.updatehelper
{
    public class EventBeanUpdateHelperFactory
    {
        public static EventBeanUpdateHelper Make(
            string updatedWindowOrTableName,
            EventTypeSPI eventTypeSPI,
            IList<OnTriggerSetAssignment> assignments,
            string updatedAlias,
            EventType optionalTriggeringEventType,
            bool isCopyOnWrite)
        {
            IList<EventBeanUpdateItem> updateItems = new List<EventBeanUpdateItem>();
            IList<string> properties = new List<string>();
    
            for (var i = 0; i < assignments.Count; i++)
            {
                var assignment = assignments[i];
                EventBeanUpdateItem updateItem;
    
                // determine whether this is a "property=value" assignment, we use property setters in this case
                var possibleAssignment = ExprNodeUtility.CheckGetAssignmentToProp(assignment.Expression);
    
                // handle assignment "property = value"
                if (possibleAssignment != null) {
    
                    var propertyName = possibleAssignment.First;
                    var writableProperty = eventTypeSPI.GetWritableProperty(propertyName);
    
                    // check assignment to indexed or mapped property
                    if (writableProperty == null) {
                        var nameWriteablePair = CheckIndexedOrMappedProp(possibleAssignment.First, updatedWindowOrTableName, updatedAlias, eventTypeSPI);
                        propertyName = nameWriteablePair.First;
                        writableProperty = nameWriteablePair.Second;
                    }
    
                    var evaluator = possibleAssignment.Second.ExprEvaluator;
                    var writers = eventTypeSPI.GetWriter(propertyName);
                    var notNullableField = writableProperty.PropertyType.IsPrimitive;
    
                    properties.Add(propertyName);
                    var widener = TypeWidenerFactory.GetCheckPropertyAssignType(ExprNodeUtility.ToExpressionStringMinPrecedenceSafe(possibleAssignment.Second), possibleAssignment.Second.ExprEvaluator.ReturnType,
                            writableProperty.PropertyType, propertyName);
    
                    // check event type assignment
                    if (optionalTriggeringEventType != null && possibleAssignment.Second is ExprIdentNode) {
                        var node = (ExprIdentNode) possibleAssignment.Second;
                        var fragmentRHS = optionalTriggeringEventType.GetFragmentType(node.ResolvedPropertyName);
                        var fragmentLHS = eventTypeSPI.GetFragmentType(possibleAssignment.First);
                        if (fragmentRHS != null && fragmentLHS != null && !EventTypeUtility.IsTypeOrSubTypeOf(fragmentRHS.FragmentType, fragmentLHS.FragmentType)) {
                            throw new ExprValidationException("Invalid assignment to property '" +
                                possibleAssignment.First + "' event type '" + fragmentLHS.FragmentType.Name +
                                "' from event type '" + fragmentRHS.FragmentType.Name + "'");
                        }
                    }
    
                    updateItem = new EventBeanUpdateItem(evaluator, propertyName, writers, notNullableField, widener);
                }
                // handle non-assignment, i.e. UDF or other expression
                else {
                    var evaluator = assignment.Expression.ExprEvaluator;
                    updateItem = new EventBeanUpdateItem(evaluator, null, null, false, null);
                }
    
                updateItems.Add(updateItem);
            }
    
            // copy-on-write is the default event semantics as events are immutable
            EventBeanCopyMethod copyMethod;
            if (isCopyOnWrite) {
                // obtain copy method
                IList<string> propertiesUniqueList = new List<string>(new HashSet<string>(properties));
                var propertiesArray = propertiesUniqueList.ToArray();
                copyMethod = eventTypeSPI.GetCopyMethod(propertiesArray);
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
                    copyMethod = eventTypeSPI.GetCopyMethod(propertiesInitialValueArray);
                }
            }
    
            var updateItemsArray = updateItems.ToArray();
            return new EventBeanUpdateHelper(copyMethod, updateItemsArray);
        }

        private static ISet<string> DeterminePropertiesInitialValue(IEnumerable<OnTriggerSetAssignment> assignments)
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

            var indexDot = propertyName.IndexOf('.');
            if ((namedWindowAlias != null) && (indexDot != -1))
            {
                var prefix = propertyName.Substring(0, indexDot);
                var name = propertyName.Substring(indexDot + 1);
                if (prefix.Equals(namedWindowAlias))
                {
                    writableProperty = eventTypeSPI.GetWritableProperty(name);
                    propertyName = name;
                }
            }
            if (writableProperty == null && indexDot != -1)
            {
                var prefix = propertyName.Substring(0, indexDot);
                var name = propertyName.Substring(indexDot + 1);
                if (prefix.Equals(updatedWindowOrTableName))
                {
                    writableProperty = eventTypeSPI.GetWritableProperty(name);
                    propertyName = name;
                }
            }
            if (writableProperty == null)
            {
                throw new ExprValidationException("Property '" + propertyName + "' is not available for write access");
            }
            return new Pair<string, EventPropertyDescriptor>(propertyName, writableProperty);
        }
    }
}
