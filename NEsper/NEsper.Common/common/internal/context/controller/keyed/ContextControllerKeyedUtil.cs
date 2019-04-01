///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.context.aifactory.createwindow;
using com.espertech.esper.common.@internal.context.controller.condition;
using com.espertech.esper.common.@internal.context.mgr;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.context.controller.keyed
{
	public class ContextControllerKeyedUtil {

	    protected internal static ContextControllerKeyedSvc GetService(ContextControllerKeyedFactory factory, ContextManagerRealization realization) {
	        if (factory.FactoryEnv.IsRoot) {
	            return new ContextControllerKeyedSvcLevelOne();
	        }
	        return new ContextControllerKeyedSvcLevelAny();
	    }

	    protected internal static Type[] ValidateContextDesc(string contextName, ContextSpecKeyed partitionSpec) {

	        if (partitionSpec.Items.IsEmpty()) {
	            throw new ExprValidationException("Empty list of partition items");
	        }

	        // verify properties exist
	        foreach (ContextSpecKeyedItem item in partitionSpec.Items) {
	            EventType type = item.FilterSpecCompiled.FilterForEventType;
	            foreach (string property in item.PropertyNames) {
	                EventPropertyGetter getter = type.GetGetter(property);
	                if (getter == null) {
	                    throw new ExprValidationException("For context '" + contextName + "' property name '" + property + "' not found on type " + type.Name);
	                }
	            }
	        }

	        // verify property number and types compatible
	        ContextSpecKeyedItem firstItem = partitionSpec.Items[0];
	        if (partitionSpec.Items.Count > 1) {
	            // verify the same filter event type is only listed once

	            for (int i = 0; i < partitionSpec.Items.Count; i++) {
	                EventType compareTo = partitionSpec.Items.Get(i).FilterSpecCompiled.FilterForEventType;

	                for (int j = 0; j < partitionSpec.Items.Count; j++) {
	                    if (i == j) {
	                        continue;
	                    }

	                    EventType compareFrom = partitionSpec.Items.Get(j).FilterSpecCompiled.FilterForEventType;
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
	            string[] names = new string[firstItem.PropertyNames.Count];
	            Type[] types = new Type[firstItem.PropertyNames.Count];
	            Type[] typesBoxed = new Type[firstItem.PropertyNames.Count];
	            for (int i = 0; i < firstItem.PropertyNames.Count; i++) {
	                string property = firstItem.PropertyNames.Get(i);
	                names[i] = property;
	                types[i] = firstItem.FilterSpecCompiled.FilterForEventType.GetPropertyType(property);
	                typesBoxed[i] = Boxing.GetBoxedType(types[i]);
	            }

	            // compare property types and numbers
	            for (int item = 1; item < partitionSpec.Items.Count; item++) {
	                ContextSpecKeyedItem nextItem = partitionSpec.Items.Get(item);

	                // compare number of properties
	                if (nextItem.PropertyNames.Count != types.Length) {
	                    throw new ExprValidationException("For context '" + contextName + "' expected the same number of property names for each event type, found " +
	                            types.Length + " properties for event type '" + firstItem.FilterSpecCompiled.FilterForEventType.Name +
	                            "' and " + nextItem.PropertyNames.Count + " properties for event type '" + nextItem.FilterSpecCompiled.FilterForEventType.Name + "'");
	                }

	                // compare property types
	                for (int i = 0; i < nextItem.PropertyNames.Count; i++) {
	                    string property = nextItem.PropertyNames.Get(i);
	                    Type type = Boxing.GetBoxedType(nextItem.FilterSpecCompiled.FilterForEventType.GetPropertyType(property));
	                    Type typeBoxed = Boxing.GetBoxedType(type);
	                    bool left = TypeHelper.IsSubclassOrImplementsInterface(typeBoxed, typesBoxed[i]);
	                    bool right = TypeHelper.IsSubclassOrImplementsInterface(typesBoxed[i], typeBoxed);
	                    if (typeBoxed != typesBoxed[i] && !left && !right) {
	                        throw new ExprValidationException("For context '" + contextName + "' for context '" + contextName + "' found mismatch of property types, property '" + names[i] +
	                                "' of type '" + TypeHelper.GetClassNameFullyQualPretty(types[i]) +
	                                "' compared to property '" + property +
	                                "' of type '" + TypeHelper.GetClassNameFullyQualPretty(typeBoxed) + "'");
	                    }
	                }
	            }
	        }

	        Type[] propertyTypes = new Type[firstItem.PropertyNames.Count];
	        for (int i = 0; i < firstItem.PropertyNames.Count; i++) {
	            string property = firstItem.PropertyNames.Get(i);
	            propertyTypes[i] = firstItem.FilterSpecCompiled.FilterForEventType.GetPropertyType(property);
	        }
	        return propertyTypes;
	    }

	    public static FilterValueSetParam[][] GetAddendumFilters(object getterKey, FilterSpecActivatable filtersSpec, ContextControllerDetailKeyed keyedSpec, bool includePartition, ContextControllerStatementDesc optionalStatementDesc, AgentInstanceContext agentInstanceContext) {

	        // determine whether create-named-window
	        bool isCreateWindow = optionalStatementDesc != null && optionalStatementDesc.Lightweight.StatementContext.StatementInformationals.StatementType == StatementType.CREATE_WINDOW;
	        ContextControllerDetailKeyedItem foundPartition = null;

	        if (!isCreateWindow) {
	            foreach (ContextControllerDetailKeyedItem partitionItem in keyedSpec.Items) {
	                bool typeOrSubtype = EventTypeUtility.IsTypeOrSubTypeOf(filtersSpec.FilterForEventType, partitionItem.FilterSpecActivatable.FilterForEventType);
	                if (typeOrSubtype) {
	                    foundPartition = partitionItem;
	                    break;
	                }
	            }
	        } else {
	            StatementAgentInstanceFactoryCreateNW factory = (StatementAgentInstanceFactoryCreateNW) optionalStatementDesc.Lightweight.StatementContext.StatementAIFactoryProvider.Factory;
	            string declaredAsName = factory.AsEventTypeName;
	            foreach (ContextControllerDetailKeyedItem partitionItem in keyedSpec.Items) {
	                if (partitionItem.FilterSpecActivatable.FilterForEventType.Name.Equals(declaredAsName)) {
	                    foundPartition = partitionItem;
	                    break;
	                }
	            }
	        }

	        if (foundPartition == null) {
	            return null;
	        }

	        ExprFilterSpecLookupable[] lookupables = foundPartition.Lookupables;
	        FilterValueSetParam[] addendumFilters = new FilterValueSetParam[lookupables.Length];
	        if (lookupables.Length == 1) {
	            addendumFilters[0] = GetFilterMayEqualOrNull(lookupables[0], getterKey);
	        } else {
	            object[] keys = getterKey is HashableMultiKey ? ((HashableMultiKey) getterKey).Keys : (object[]) getterKey;
	            for (int i = 0; i < lookupables.Length; i++) {
	                addendumFilters[i] = GetFilterMayEqualOrNull(lookupables[i], keys[i]);
	            }
	        }

	        FilterValueSetParam[][] addendum = new FilterValueSetParam[1][];
	        addendum[0] = addendumFilters;

	        FilterValueSetParam[][] partitionFilters = foundPartition.FilterSpecActivatable.GetValueSet(null, null, agentInstanceContext, agentInstanceContext.StatementContextFilterEvalEnv);
	        if (partitionFilters != null && includePartition) {
	            addendum = FilterAddendumUtil.AddAddendum(partitionFilters, addendum[0]);
	        }

	        return addendum;
	    }

	    public static ContextControllerDetailKeyedItem FindInitMatchingKey(ContextControllerDetailKeyedItem[] items, ContextConditionDescriptorFilter init) {
	        ContextControllerDetailKeyedItem found = null;
	        foreach (ContextControllerDetailKeyedItem item in items) {
	            if (item.FilterSpecActivatable.FilterForEventType == init.FilterSpecActivatable.FilterForEventType) {
	                found = item;
	                break;
	            }
	        }
	        if (found == null) {
	            throw new ArgumentException("Failed to find matching partition for type '" + init.FilterSpecActivatable.FilterForEventType);
	        }
	        return found;
	    }

	    private static FilterValueSetParam GetFilterMayEqualOrNull(ExprFilterSpecLookupable lookupable, object keyValue) {
	        return new FilterValueSetParamImpl(lookupable, FilterOperator.IS, keyValue);
	    }

	    public static void PopulatePriorMatch(string optionalInitCondAsName, MatchedEventMap matchedEventMap, EventBean triggeringEvent) {
	        int tag = matchedEventMap.Meta.GetTagFor(optionalInitCondAsName);
	        if (tag == -1) {
	            return;
	        }
	        matchedEventMap.Add(tag, triggeringEvent);
	    }
	}
} // end of namespace