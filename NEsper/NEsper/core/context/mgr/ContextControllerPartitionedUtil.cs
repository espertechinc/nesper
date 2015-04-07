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
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression;
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

        internal static Type[] ValidateContextDesc(String contextName, ContextDetailPartitioned segmentedSpec)
        {
            if (segmentedSpec.Items.IsEmpty())
            {
                throw new ExprValidationException("Empty list of partition items");
            }

            // verify properties exist
            foreach (ContextDetailPartitionItem item in segmentedSpec.Items)
            {
                EventType type = item.FilterSpecCompiled.FilterForEventType;
                foreach (String property in item.PropertyNames)
                {
                    EventPropertyGetter getter = type.GetGetter(property);
                    if (getter == null)
                    {
                        throw new ExprValidationException(
                            "For context '" + contextName + "' property name '" + property + "' not found on type " +
                            type.Name);
                    }
                }
            }

            // verify property number and types compatible
            ContextDetailPartitionItem firstItem = segmentedSpec.Items[0];
            if (segmentedSpec.Items.Count > 1)
            {
                // verify the same filter event type is only listed once

                for (int i = 0; i < segmentedSpec.Items.Count; i++)
                {
                    var compareTo = segmentedSpec.Items[i].FilterSpecCompiled.FilterForEventType;

                    for (int j = 0; j < segmentedSpec.Items.Count; j++)
                    {
                        if (i == j)
                        {
                            continue;
                        }

                        var compareFrom = segmentedSpec.Items[j].FilterSpecCompiled.FilterForEventType;
                        if (compareFrom == compareTo)
                        {
                            throw new ExprValidationException(
                                "For context '" + contextName + "' the event type '" + compareFrom.Name +
                                "' is listed twice");
                        }
                        if (EventTypeUtility.IsTypeOrSubTypeOf(compareFrom, compareTo) ||
                            EventTypeUtility.IsTypeOrSubTypeOf(compareTo, compareFrom))
                        {
                            throw new ExprValidationException(
                                "For context '" + contextName + "' the event type '" + compareFrom.Name +
                                "' is listed twice: Event type '" +
                                compareFrom.Name + "' is a subtype or supertype of event type '" + compareTo.Name + "'");
                        }

                    }
                }

                // build property type information
                var names = new String[firstItem.PropertyNames.Count];
                var types = new Type[firstItem.PropertyNames.Count];
                var typesBoxed = new Type[firstItem.PropertyNames.Count];
                for (int i = 0; i < firstItem.PropertyNames.Count; i++)
                {
                    String property = firstItem.PropertyNames[i];
                    names[i] = property;
                    types[i] = firstItem.FilterSpecCompiled.FilterForEventType.GetPropertyType(property);
                    typesBoxed[i] = types[i].GetBoxedType();
                }

                // compare property types and numbers
                for (int item = 1; item < segmentedSpec.Items.Count; item++)
                {
                    ContextDetailPartitionItem nextItem = segmentedSpec.Items[item];

                    // compare number of properties
                    if (nextItem.PropertyNames.Count != types.Length)
                    {
                        throw new ExprValidationException(
                            string.Format(
                                "For context '{0}' expected the same number of property names for each event type, found {1} properties for event type '{2}' and {3} properties for event type '{4}'",
                                contextName,
                                types.Length, 
                                firstItem.FilterSpecCompiled.FilterForEventType.Name,
                                nextItem.PropertyNames.Count, 
                                nextItem.FilterSpecCompiled.FilterForEventType.Name));
                    }

                    // compare property types
                    for (int i = 0; i < nextItem.PropertyNames.Count; i++)
                    {
                        String property = nextItem.PropertyNames[i];
                        var type =
                            nextItem.FilterSpecCompiled.FilterForEventType.GetPropertyType(property).GetBoxedType();
                        var typeBoxed = type.GetBoxedType();
                        var left = TypeHelper.IsSubclassOrImplementsInterface(typeBoxed, typesBoxed[i]);
                        var right = TypeHelper.IsSubclassOrImplementsInterface(typesBoxed[i], typeBoxed);
                        if (typeBoxed != typesBoxed[i] && !left && !right)
                        {
                            throw new ExprValidationException(
                                string.Format(
                                    "For context '{0}' for context '{0}' found mismatch of property types, property '{1}' of type '{2}' compared to property '{3}' of type '{4}'", 
                                    contextName, 
                                    names[i], 
                                    TypeHelper.GetTypeNameFullyQualPretty(types[i]), 
                                    property, 
                                    TypeHelper.GetTypeNameFullyQualPretty(typeBoxed)));
                        }
                    }
                }
            }

            var propertyTypes = new Type[firstItem.PropertyNames.Count];
            for (int i = 0; i < firstItem.PropertyNames.Count; i++)
            {
                String property = firstItem.PropertyNames[i];
                propertyTypes[i] = firstItem.FilterSpecCompiled.FilterForEventType.GetPropertyType(property);
            }
            return propertyTypes;
        }

        internal static void ValidateStatementForContext(String contextName, ContextControllerStatementBase statement, StatementSpecCompiledAnalyzerResult streamAnalysis, ICollection<EventType> itemEventTypes, NamedWindowService namedWindowService)
        {
            IList<FilterSpecCompiled> filters = streamAnalysis.Filters;
    
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
    
                        NamedWindowProcessor processor = namedWindowService.GetProcessor(stmtFilterType.Name);
                        if (processor != null && processor.ContextName != null && processor.ContextName.Equals(contextName)) {
                            return;
                        }
                    }
                }
    
                if (filters.IsNotEmpty()) {
                    throw new ExprValidationException(GetTypeValidationMessage(contextName, filters[0].FilterForEventType.Name));
                }
                return;
            }
    
            // validate create-window
            String declaredAsName = statement.StatementSpec.CreateWindowDesc.AsEventTypeName;
            if (declaredAsName != null)
            {
                if (itemEventTypes.Any(itemEventType => itemEventType.Name == declaredAsName))
                {
                    return;
                }

                throw new ExprValidationException(GetTypeValidationMessage(contextName, declaredAsName));
            }
        }
    
        // Compare filters in statement with filters in segmented context, addendum filter compilation
        public static void PopulateAddendumFilters(
            Object keyValue,
            IList<FilterSpecCompiled> filtersSpecs,
            ContextDetailPartitioned segmentedSpec,
            StatementSpecCompiled optionalStatementSpecCompiled,
            IDictionary<FilterSpecCompiled, FilterValueSetParam[][]> addendums)
        {
            // determine whether create-named-window
            bool isCreateWindow = optionalStatementSpecCompiled != null && optionalStatementSpecCompiled.CreateWindowDesc != null;
            if (!isCreateWindow) {
                foreach (FilterSpecCompiled filtersSpec in filtersSpecs) {
    
                    ContextDetailPartitionItem foundPartition = null;
                    foreach (ContextDetailPartitionItem partitionItem in segmentedSpec.Items) {
                        bool typeOrSubtype = EventTypeUtility.IsTypeOrSubTypeOf(filtersSpec.FilterForEventType, partitionItem.FilterSpecCompiled.FilterForEventType);
                        if (typeOrSubtype) {
                            foundPartition = partitionItem;
                        }
                    }
    
                    if (foundPartition == null) {
                        continue;
                    }
    
                    var addendumFilters = new List<FilterValueSetParam>(foundPartition.PropertyNames.Count);
                    if (foundPartition.PropertyNames.Count == 1) {
                        var propertyName = foundPartition.PropertyNames[0];
                        var getter = foundPartition.FilterSpecCompiled.FilterForEventType.GetGetter(propertyName);
                        var resultType = foundPartition.FilterSpecCompiled.FilterForEventType.GetPropertyType(propertyName);
                        var lookupable = new FilterSpecLookupable(propertyName, getter, resultType);
                        var filter = new FilterValueSetParamImpl(lookupable, FilterOperator.EQUAL, keyValue);
                        addendumFilters.Add(filter);
                    }
                    else {
                        var keys = ((MultiKeyUntyped) keyValue).Keys;
                        for (int i = 0; i < foundPartition.PropertyNames.Count; i++) {
                            var partitionPropertyName = foundPartition.PropertyNames[i];
                            var getter = foundPartition.FilterSpecCompiled.FilterForEventType.GetGetter(partitionPropertyName);
                            var resultType = foundPartition.FilterSpecCompiled.FilterForEventType.GetPropertyType(partitionPropertyName);
                            var lookupable = new FilterSpecLookupable(partitionPropertyName, getter, resultType);
                            var filter = new FilterValueSetParamImpl(lookupable, FilterOperator.EQUAL, keys[i]);
                            addendumFilters.Add(filter);
                        }
                    }
    
                    // add those predefined filter parameters, if any
                    FilterValueSetParam[][] partitionFilters = foundPartition.ParametersCompiled;
    
                    // add to existing if any are present
                    AddAddendums(addendums, addendumFilters, filtersSpec, partitionFilters);
                }
            }
            // handle segmented context for create-window
            else {
                String declaredAsName = optionalStatementSpecCompiled.CreateWindowDesc.AsEventTypeName;
                if (declaredAsName != null) {
                    foreach (FilterSpecCompiled filtersSpec in filtersSpecs) {
    
                        ContextDetailPartitionItem foundPartition = null;
                        foreach (ContextDetailPartitionItem partitionItem in segmentedSpec.Items) {
                            if (partitionItem.FilterSpecCompiled.FilterForEventType.Name.Equals(declaredAsName)) {
                                foundPartition = partitionItem;
                                break;
                            }
                        }
    
                        if (foundPartition == null) {
                            continue;
                        }
    
                        var addendumFilters = new List<FilterValueSetParam>(foundPartition.PropertyNames.Count);
                        var propertyNumber = 0;
                        foreach (String partitionPropertyName in foundPartition.PropertyNames) {
                            var getter = foundPartition.FilterSpecCompiled.FilterForEventType.GetGetter(partitionPropertyName);
                            var resultType = foundPartition.FilterSpecCompiled.FilterForEventType.GetPropertyType(partitionPropertyName);
                            var lookupable = new FilterSpecLookupable(partitionPropertyName, getter, resultType);
    
                            Object propertyValue;
                            if (keyValue is MultiKeyUntyped) {
                                propertyValue = ((MultiKeyUntyped) keyValue).Get(propertyNumber);
                            }
                            else {
                                propertyValue = keyValue;
                            }
    
                            FilterValueSetParam filter = new FilterValueSetParamImpl(lookupable, FilterOperator.EQUAL, propertyValue);
                            addendumFilters.Add(filter);
                            propertyNumber++;
                        }
    
                        // add to existing if any are present
                        AddAddendums(addendums, addendumFilters, filtersSpec, foundPartition.ParametersCompiled);
                    }
                }
            }
        }

        private static void AddAddendums(
            IDictionary<FilterSpecCompiled, FilterValueSetParam[][]> addendums,
            IList<FilterValueSetParam> addendumFilters,
            FilterSpecCompiled filtersSpec,
            FilterValueSetParam[][] optionalPartitionFilters)
        {
            FilterValueSetParam[][] params2Dim = new FilterValueSetParam[1][];
            params2Dim[0] = addendumFilters.ToArray();

            FilterValueSetParam[][] addendum;
            FilterValueSetParam[][] existing = addendums.Get(filtersSpec);
            if (existing != null)
            {
                addendum = ContextControllerAddendumUtil.MultiplyAddendum(existing, params2Dim);
            }
            else
            {
                addendum = params2Dim;
            }

            if (optionalPartitionFilters != null)
            {
                addendum = ContextControllerAddendumUtil.MultiplyAddendum(addendum, optionalPartitionFilters);
            }

            addendums[filtersSpec] = addendum;
        }
    
        private static String GetTypeValidationMessage(String contextName, String typeNameEx) {
            return "Segmented context '" + contextName + "' requires that any of the event types that are listed in the segmented context also appear in any of the filter expressions of the statement, type '" + typeNameEx + "' is not one of the types listed";
        }
    }
}
