///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.named;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.spec.util;
using com.espertech.esper.events;
using com.espertech.esper.filter;
using com.espertech.esper.util;

namespace com.espertech.esper.core.context.mgr
{
    public class ContextControllerPartitionedUtil {
    
        internal static Type[] ValidateContextDesc(string contextName, ContextDetailPartitioned segmentedSpec) {
    
            if (segmentedSpec.Items.IsEmpty()) {
                throw new ExprValidationException("Empty list of partition items");
            }
    
            // verify properties exist
            foreach (ContextDetailPartitionItem item in segmentedSpec.Items) {
                EventType type = item.FilterSpecCompiled.FilterForEventType;
                foreach (string property in item.PropertyNames) {
                    EventPropertyGetter getter = type.GetGetter(property);
                    if (getter == null) {
                        throw new ExprValidationException("For context '" + contextName + "' property name '" + property + "' not found on type " + type.Name);
                    }
                }
            }
    
            // verify property number and types compatible
            ContextDetailPartitionItem firstItem = segmentedSpec.Items[0];
            if (segmentedSpec.Items.Count > 1) {
                // verify the same filter event type is only listed once
    
                for (int i = 0; i < segmentedSpec.Items.Count; i++) {
                    EventType compareTo = segmentedSpec.Items.Get(i).FilterSpecCompiled.FilterForEventType;
    
                    for (int j = 0; j < segmentedSpec.Items.Count; j++) {
                        if (i == j) {
                            continue;
                        }
    
                        EventType compareFrom = segmentedSpec.Items.Get(j).FilterSpecCompiled.FilterForEventType;
                        if (compareFrom == compareTo) {
                            throw new ExprValidationException("For context '" + contextName + "' the event type '" + compareFrom.Name + "' is listed twice");
                        }
                        if (EventTypeUtility.IsTypeOrSubTypeOf(compareFrom, compareTo) || EventTypeUtility.IsTypeOrSubTypeOf(compareTo, compareFrom)) {
                            throw new ExprValidationException("For context '" + contextName + "' the event type '" + compareFrom.Name + "' is listed twice: Event type '" +
                                    compareFrom.Name + "' is a subtype or supertype of event type '" + compareTo.Name + "'");
                        }
    
                    }
                }
    
                // build property type information
                var names = new string[firstItem.PropertyNames.Count];
                var types = new Type[firstItem.PropertyNames.Count];
                var typesBoxed = new Type[firstItem.PropertyNames.Count];
                for (int i = 0; i < firstItem.PropertyNames.Count; i++) {
                    string property = firstItem.PropertyNames.Get(i);
                    names[i] = property;
                    types[i] = firstItem.FilterSpecCompiled.FilterForEventType.GetPropertyType(property);
                    typesBoxed[i] = TypeHelper.GetBoxedType(types[i]);
                }
    
                // compare property types and numbers
                for (int item = 1; item < segmentedSpec.Items.Count; item++) {
                    ContextDetailPartitionItem nextItem = segmentedSpec.Items.Get(item);
    
                    // compare number of properties
                    if (nextItem.PropertyNames.Count != types.Length) {
                        throw new ExprValidationException("For context '" + contextName + "' expected the same number of property names for each event type, found " +
                                types.Length + " properties for event type '" + firstItem.FilterSpecCompiled.FilterForEventType.Name +
                                "' and " + nextItem.PropertyNames.Count + " properties for event type '" + nextItem.FilterSpecCompiled.FilterForEventType.Name + "'");
                    }
    
                    // compare property types
                    for (int i = 0; i < nextItem.PropertyNames.Count; i++) {
                        string property = nextItem.PropertyNames.Get(i);
                        Type type = TypeHelper.GetBoxedType(nextItem.FilterSpecCompiled.FilterForEventType.GetPropertyType(property));
                        Type typeBoxed = TypeHelper.GetBoxedType(type);
                        bool left = TypeHelper.IsSubclassOrImplementsInterface(typeBoxed, typesBoxed[i]);
                        bool right = TypeHelper.IsSubclassOrImplementsInterface(typesBoxed[i], typeBoxed);
                        if (typeBoxed != typesBoxed[i] && !left && !right) {
                            throw new ExprValidationException("For context '" + contextName + "' for context '" + contextName + "' found mismatch of property types, property '" + names[i] +
                                    "' of type '" + TypeHelper.GetTypeNameFullyQualPretty(types[i]) +
                                    "' compared to property '" + property +
                                    "' of type '" + TypeHelper.GetTypeNameFullyQualPretty(typeBoxed) + "'");
                        }
                    }
                }
            }
    
            var propertyTypes = new Type[firstItem.PropertyNames.Count];
            for (int i = 0; i < firstItem.PropertyNames.Count; i++) {
                string property = firstItem.PropertyNames.Get(i);
                propertyTypes[i] = firstItem.FilterSpecCompiled.FilterForEventType.GetPropertyType(property);
            }
            return propertyTypes;
        }
    
        internal static void ValidateStatementForContext(string contextName, ContextControllerStatementBase statement, StatementSpecCompiledAnalyzerResult streamAnalysis, ICollection<EventType> itemEventTypes, NamedWindowMgmtService namedWindowMgmtService)
                {
            List<FilterSpecCompiled> filters = streamAnalysis.Filters;
    
            bool isCreateWindow = statement.StatementSpec.CreateWindowDesc != null;
    
            // if no create-window: at least one of the filters must match one of the filters specified by the context
            if (!isCreateWindow) {
                foreach (FilterSpecCompiled filter in filters) {
                    foreach (EventType itemEventType in itemEventTypes) {
                        EventType stmtFilterType = filter.FilterForEventType;
                        if (stmtFilterType == itemEventType) {
                            return;
                        }
                        if (EventTypeUtility.IsTypeOrSubTypeOf(stmtFilterType, itemEventType)) {
                            return;
                        }
    
                        NamedWindowProcessor processor = namedWindowMgmtService.GetProcessor(stmtFilterType.Name);
                        if (processor != null && processor.ContextName != null && processor.ContextName.Equals(contextName)) {
                            return;
                        }
                    }
                }
    
                if (!filters.IsEmpty()) {
                    throw new ExprValidationException(GetTypeValidationMessage(contextName, filters[0].FilterForEventType.Name));
                }
                return;
            }
    
            // validate create-window with column definition: not allowed, requires typed
            if (statement.StatementSpec.CreateWindowDesc.Columns != null &&
                    statement.StatementSpec.CreateWindowDesc.Columns.Count > 0) {
                throw new ExprValidationException("Segmented context '" + contextName +
                        "' requires that named windows are associated to an existing event type and that the event type is listed among the partitions defined by the create-context statement");
            }
    
            // validate create-window declared type
            string declaredAsName = statement.StatementSpec.CreateWindowDesc.AsEventTypeName;
            if (declaredAsName != null) {
                foreach (EventType itemEventType in itemEventTypes) {
                    if (itemEventType.Name.Equals(declaredAsName)) {
                        return;
                    }
                }
    
                throw new ExprValidationException(GetTypeValidationMessage(contextName, declaredAsName));
            }
        }
    
        // Compare filters in statement with filters in segmented context, addendum filter compilation
        public static void PopulateAddendumFilters(Object keyValue, List<FilterSpecCompiled> filtersSpecs, ContextDetailPartitioned segmentedSpec, StatementSpecCompiled optionalStatementSpecCompiled, IdentityDictionary<FilterSpecCompiled, FilterValueSetParam[][]> addendums) {
            foreach (FilterSpecCompiled filtersSpec in filtersSpecs) {
                FilterValueSetParam[][] addendum = GetAddendumFilters(keyValue, filtersSpec, segmentedSpec, optionalStatementSpecCompiled);
                if (addendum == null) {
                    continue;
                }
    
                FilterValueSetParam[][] existing = addendums.Get(filtersSpec);
                if (existing != null) {
                    addendum = ContextControllerAddendumUtil.MultiplyAddendum(existing, addendum);
                }
                addendums.Put(filtersSpec, addendum);
            }
        }
    
        public static FilterValueSetParam[][] GetAddendumFilters(Object keyValue, FilterSpecCompiled filtersSpec, ContextDetailPartitioned segmentedSpec, StatementSpecCompiled optionalStatementSpecCompiled) {
    
            // determine whether create-named-window
            bool isCreateWindow = optionalStatementSpecCompiled != null && optionalStatementSpecCompiled.CreateWindowDesc != null;
            ContextDetailPartitionItem foundPartition = null;
    
            if (!isCreateWindow) {
                foreach (ContextDetailPartitionItem partitionItem in segmentedSpec.Items) {
                    bool typeOrSubtype = EventTypeUtility.IsTypeOrSubTypeOf(filtersSpec.FilterForEventType, partitionItem.FilterSpecCompiled.FilterForEventType);
                    if (typeOrSubtype) {
                        foundPartition = partitionItem;
                    }
                }
            } else {
                string declaredAsName = optionalStatementSpecCompiled.CreateWindowDesc.AsEventTypeName;
                if (declaredAsName == null) {
                    return null;
                }
                foreach (ContextDetailPartitionItem partitionItem in segmentedSpec.Items) {
                    if (partitionItem.FilterSpecCompiled.FilterForEventType.Name.Equals(declaredAsName)) {
                        foundPartition = partitionItem;
                        break;
                    }
                }
            }
    
            if (foundPartition == null) {
                return null;
            }
    
            var addendumFilters = new List<FilterValueSetParam>(foundPartition.PropertyNames.Count);
            if (foundPartition.PropertyNames.Count == 1) {
                string propertyName = foundPartition.PropertyNames[0];
                EventPropertyGetter getter = foundPartition.FilterSpecCompiled.FilterForEventType.GetGetter(propertyName);
                Type resultType = foundPartition.FilterSpecCompiled.FilterForEventType.GetPropertyType(propertyName);
                var lookupable = new FilterSpecLookupable(propertyName, getter, resultType, false);
                FilterValueSetParam filter = GetFilterMayEqualOrNull(lookupable, keyValue);
                addendumFilters.Add(filter);
            } else {
                Object[] keys = ((MultiKeyUntyped) keyValue).Keys;
                for (int i = 0; i < foundPartition.PropertyNames.Count; i++) {
                    string partitionPropertyName = foundPartition.PropertyNames.Get(i);
                    EventPropertyGetter getter = foundPartition.FilterSpecCompiled.FilterForEventType.GetGetter(partitionPropertyName);
                    Type resultType = foundPartition.FilterSpecCompiled.FilterForEventType.GetPropertyType(partitionPropertyName);
                    var lookupable = new FilterSpecLookupable(partitionPropertyName, getter, resultType, false);
                    FilterValueSetParam filter = GetFilterMayEqualOrNull(lookupable, keys[i]);
                    addendumFilters.Add(filter);
                }
            }
    
            var addendum = new FilterValueSetParam[1][];
            addendum[0] = addendumFilters.ToArray(new FilterValueSetParam[addendumFilters.Count]);
    
            FilterValueSetParam[][] partitionFilters = foundPartition.ParametersCompiled;
            if (partitionFilters != null) {
                addendum = ContextControllerAddendumUtil.AddAddendum(partitionFilters, addendum[0]);
            }
    
            return addendum;
        }
    
        private static FilterValueSetParam GetFilterMayEqualOrNull(FilterSpecLookupable lookupable, Object keyValue) {
            return new FilterValueSetParamImpl(lookupable, FilterOperator.IS, keyValue);
        }
    
        private static string GetTypeValidationMessage(string contextName, string typeNameEx) {
            return "Segmented context '" + contextName + "' requires that any of the event types that are listed in the segmented context also appear in any of the filter expressions of the statement, type '" + typeNameEx + "' is not one of the types listed";
        }
    }
} // end of namespace
