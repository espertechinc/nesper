///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

using static
    com.espertech.esper.common.@internal.@event.eventtyperepo.EventTypeRepositoryMapTypeUtil; //ToTypesReferences;

namespace com.espertech.esper.common.@internal.@event.eventtyperepo
{
    public class EventTypeRepositoryUtil
    {
        public static IList<string> GetCreationOrder<T>(
            ICollection<string> firstSet,
            ICollection<string> secondSet,
            IDictionary<string, T> configurations)
            where T : ConfigurationCommonEventTypeWithSupertype
        {
            IList<string> creationOrder = new List<string>();
            creationOrder.AddAll(firstSet);
            creationOrder.AddAll(secondSet);

            ICollection<string> dependentOrder;
            try {
                var typesReferences = ToTypesReferences(configurations);
                dependentOrder = GraphUtil.GetTopDownOrder(typesReferences);
            }
            catch (GraphCircularDependencyException e) {
                throw new ConfigurationException(
                    "Error configuring event types, dependency graph between map type names is circular: " + e.Message,
                    e);
            }

            if (dependentOrder.IsEmpty() || dependentOrder.Count < 2) {
                return creationOrder;
            }

            var dependents = dependentOrder.ToArray();
            for (var i = 1; i < dependents.Length; i++) {
                var indexSuper = creationOrder.IndexOf(dependents[i - 1]);
                var indexSub = creationOrder.IndexOf(dependents[i]);
                if (indexSuper == -1 || indexSub == -1) {
                    continue;
                }

                if (indexSuper > indexSub) {
                    creationOrder.RemoveAt(indexSub);
                    if (indexSuper == creationOrder.Count) {
                        creationOrder.Add(dependents[i]);
                    }
                    else {
                        creationOrder.Insert(indexSuper + 1, dependents[i]);
                    }
                }
            }

            return creationOrder;
        }
    }
} // end of namespace