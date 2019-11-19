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
    public class ContextControllerKeyedUtil
    {
        protected internal static ContextControllerKeyedSvc GetService(
            ContextControllerKeyedFactory factory,
            ContextManagerRealization realization)
        {
            if (factory.FactoryEnv.IsRoot) {
                return new ContextControllerKeyedSvcLevelOne();
            }

            return new ContextControllerKeyedSvcLevelAny();
        }

        protected internal static Type[] ValidateContextDesc(
            string contextName,
            ContextSpecKeyed partitionSpec)
        {
            if (partitionSpec.Items.IsEmpty()) {
                throw new ExprValidationException("Empty list of partition items");
            }

            // verify properties exist
            foreach (var item in partitionSpec.Items) {
                var type = item.FilterSpecCompiled.FilterForEventType;
                foreach (var property in item.PropertyNames) {
                    var getter = type.GetGetter(property);
                    if (getter == null) {
                        throw new ExprValidationException(
                            "For context '" +
                            contextName +
                            "' property name '" +
                            property +
                            "' not found on type " +
                            type.Name);
                    }
                }
            }

            // verify property number and types compatible
            var firstItem = partitionSpec.Items[0];
            if (partitionSpec.Items.Count > 1) {
                // verify the same filter event type is only listed once

                for (var i = 0; i < partitionSpec.Items.Count; i++) {
                    var compareTo = partitionSpec.Items[i].FilterSpecCompiled.FilterForEventType;

                    for (var j = 0; j < partitionSpec.Items.Count; j++) {
                        if (i == j) {
                            continue;
                        }

                        EventType compareFrom = partitionSpec.Items[j].FilterSpecCompiled.FilterForEventType;
                        if (compareFrom == compareTo) {
                            throw new ExprValidationException(
                                "For context '" +
                                contextName +
                                "' the event type '" +
                                compareFrom.Name +
                                "' is listed twice");
                        }

                        if (EventTypeUtility.IsTypeOrSubTypeOf(compareFrom, compareTo) ||
                            EventTypeUtility.IsTypeOrSubTypeOf(compareTo, compareFrom)) {
                            throw new ExprValidationException(
                                "For context '" +
                                contextName +
                                "' the event type '" +
                                compareFrom.Name +
                                "' is listed twice: Event type '" +
                                compareFrom.Name +
                                "' is a subtype or supertype of event type '" +
                                compareTo.Name +
                                "'");
                        }
                    }
                }

                // build property type information
                var names = new string[firstItem.PropertyNames.Count];
                var types = new Type[firstItem.PropertyNames.Count];
                var typesBoxed = new Type[firstItem.PropertyNames.Count];
                for (var i = 0; i < firstItem.PropertyNames.Count; i++) {
                    var property = firstItem.PropertyNames[i];
                    names[i] = property;
                    types[i] = firstItem.FilterSpecCompiled.FilterForEventType.GetPropertyType(property);
                    typesBoxed[i] = types[i].GetBoxedType();
                }

                // compare property types and numbers
                for (var item = 1; item < partitionSpec.Items.Count; item++) {
                    ContextSpecKeyedItem nextItem = partitionSpec.Items[item];

                    // compare number of properties
                    if (nextItem.PropertyNames.Count != types.Length) {
                        throw new ExprValidationException(
                            "For context '" +
                            contextName +
                            "' expected the same number of property names for each event type, found " +
                            types.Length +
                            " properties for event type '" +
                            firstItem.FilterSpecCompiled.FilterForEventType.Name +
                            "' and " +
                            nextItem.PropertyNames.Count +
                            " properties for event type '" +
                            nextItem.FilterSpecCompiled.FilterForEventType.Name +
                            "'");
                    }

                    // compare property types
                    for (var i = 0; i < nextItem.PropertyNames.Count; i++) {
                        var property = nextItem.PropertyNames[i];
                        var type = nextItem.FilterSpecCompiled.FilterForEventType.GetPropertyType(property)
                            .GetBoxedType();
                        var typeBoxed = type.GetBoxedType();
                        var left = TypeHelper.IsSubclassOrImplementsInterface(typeBoxed, typesBoxed[i]);
                        var right = TypeHelper.IsSubclassOrImplementsInterface(typesBoxed[i], typeBoxed);
                        if (typeBoxed != typesBoxed[i] && !left && !right) {
                            throw new ExprValidationException(
                                "For context '" +
                                contextName +
                                "' for context '" +
                                contextName +
                                "' found mismatch of property types, property '" +
                                names[i] +
                                "' of type '" +
                                types[i].CleanName() +
                                "' compared to property '" +
                                property +
                                "' of type '" +
                                typeBoxed.CleanName() +
                                "'");
                        }
                    }
                }
            }

            var propertyTypes = new Type[firstItem.PropertyNames.Count];
            for (var i = 0; i < firstItem.PropertyNames.Count; i++) {
                var property = firstItem.PropertyNames[i];
                propertyTypes[i] = firstItem.FilterSpecCompiled.FilterForEventType.GetPropertyType(property);
            }

            return propertyTypes;
        }

        public static FilterValueSetParam[][] GetAddendumFilters(
            object getterKey,
            FilterSpecActivatable filtersSpec,
            ContextControllerDetailKeyed keyedSpec,
            bool includePartition,
            ContextControllerStatementDesc optionalStatementDesc,
            AgentInstanceContext agentInstanceContext)
        {
            // determine whether create-named-window
            var isCreateWindow = optionalStatementDesc != null &&
                                 optionalStatementDesc.Lightweight.StatementContext.StatementInformationals
                                     .StatementType ==
                                 StatementType.CREATE_WINDOW;
            ContextControllerDetailKeyedItem foundPartition = null;

            if (!isCreateWindow) {
                foreach (var partitionItem in keyedSpec.Items) {
                    var typeOrSubtype = EventTypeUtility.IsTypeOrSubTypeOf(
                        filtersSpec.FilterForEventType,
                        partitionItem.FilterSpecActivatable.FilterForEventType);
                    if (typeOrSubtype) {
                        foundPartition = partitionItem;
                        break;
                    }
                }
            }
            else {
                var factory = (StatementAgentInstanceFactoryCreateNW) optionalStatementDesc.Lightweight.StatementContext
                    .StatementAIFactoryProvider
                    .Factory;
                var declaredAsName = factory.AsEventTypeName;
                foreach (var partitionItem in keyedSpec.Items) {
                    if (partitionItem.FilterSpecActivatable.FilterForEventType.Name.Equals(declaredAsName)) {
                        foundPartition = partitionItem;
                        break;
                    }
                }
            }

            if (foundPartition == null) {
                return null;
            }

            var lookupables = foundPartition.Lookupables;
            var addendumFilters = new FilterValueSetParam[lookupables.Length];
            if (lookupables.Length == 1) {
                addendumFilters[0] = GetFilterMayEqualOrNull(lookupables[0], getterKey);
            }
            else {
                var keys = getterKey is HashableMultiKey ? ((HashableMultiKey) getterKey).Keys : (object[]) getterKey;
                for (var i = 0; i < lookupables.Length; i++) {
                    addendumFilters[i] = GetFilterMayEqualOrNull(lookupables[i], keys[i]);
                }
            }

            var addendum = new FilterValueSetParam[1][];
            addendum[0] = addendumFilters;

            var partitionFilters = foundPartition.FilterSpecActivatable.GetValueSet(
                null,
                null,
                agentInstanceContext,
                agentInstanceContext.StatementContextFilterEvalEnv);
            if (partitionFilters != null && includePartition) {
                addendum = FilterAddendumUtil.AddAddendum(partitionFilters, addendum[0]);
            }

            return addendum;
        }

        public static ContextControllerDetailKeyedItem FindInitMatchingKey(
            ContextControllerDetailKeyedItem[] items,
            ContextConditionDescriptorFilter init)
        {
            ContextControllerDetailKeyedItem found = null;
            foreach (var item in items) {
                if (item.FilterSpecActivatable.FilterForEventType == init.FilterSpecActivatable.FilterForEventType) {
                    found = item;
                    break;
                }
            }

            if (found == null) {
                throw new ArgumentException(
                    "Failed to find matching partition for type '" + init.FilterSpecActivatable.FilterForEventType);
            }

            return found;
        }

        private static FilterValueSetParam GetFilterMayEqualOrNull(
            ExprFilterSpecLookupable lookupable,
            object keyValue)
        {
            return new FilterValueSetParamImpl(lookupable, FilterOperator.IS, keyValue);
        }

        public static void PopulatePriorMatch(
            string optionalInitCondAsName,
            MatchedEventMap matchedEventMap,
            EventBean triggeringEvent)
        {
            var tag = matchedEventMap.Meta.GetTagFor(optionalInitCondAsName);
            if (tag == -1) {
                return;
            }

            matchedEventMap.Add(tag, triggeringEvent);
        }
    }
} // end of namespace