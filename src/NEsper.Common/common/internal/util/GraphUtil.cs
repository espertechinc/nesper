///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.util
{
    using DataMap = IDictionary<string, object>;

    /// <summary>
    ///     Utility for working with acyclic graph: determines cyclic dependency and dependency-satisfying processing order.
    /// </summary>
    public class GraphUtil
    {
        /// <summary>
        ///     Deep-merge a map into another map returning a result map.
        ///     <para />
        ///     Copies all values present in the original map to a new map, adding additional value present in
        ///     the second map passed in, ignoring same-key values in the second map that are present in the original.
        ///     <para />
        ///     If the value is a Map itself, repeats the operation on the Map value.
        /// </summary>
        /// <param name="original">nestable Map of entries to retain and not overwrite</param>
        /// <param name="additional">nestable Map of entries to add to the original</param>
        /// <returns>
        ///     merge of original and additional nestable map
        /// </returns>
        public static IDictionary<string, object> MergeNestableMap(
            IDictionary<string, object> original,
            IDictionary<string, object> additional)
        {
            var result = new LinkedHashMap<string, object>(original);

            foreach (var additionalEntry in additional) {
                var name = additionalEntry.Key;
                var additionalValue = additionalEntry.Value;

                var originalValue = original.Get(name);

                if (originalValue is DataMap innerOriginal &&
                    additionalValue is DataMap innerAdditional) {
                    object newValue = MergeNestableMap(innerOriginal, innerAdditional);
                    result.Put(name, newValue);
                    continue;
                }

                if (original.ContainsKey(name)) {
                    continue;
                }

                result.Put(name, additionalValue);
            }

            return result;
        }

        /// <summary>Check cyclic dependency and determine processing order for the given graph. </summary>
        /// <param name="graph">is represented as child nodes that have one or more parent nodes that they are dependent on</param>
        /// <returns>
        ///     set of parent and child nodes in order such that no node's dependency is not satisfiedby a prior nodein the
        ///     set
        /// </returns>
        /// <throws>GraphCircularDependencyException if a dependency has been detected</throws>
        public static ICollection<string> GetTopDownOrder(IDictionary<string, ICollection<string>> graph)
        {
            var circularDependency = GetFirstCircularDependency(graph);
            if (circularDependency != null) {
                throw new GraphCircularDependencyException(
                    $"Circular dependency detected between {circularDependency.RenderAny()}");
            }

            var reversedGraph = new Dictionary<string, ICollection<string>>();

            // Reversed the graph - build a list of children per parent
            foreach (var entry in graph) {
                var parents = entry.Value;
                var child = entry.Key;

                foreach (var parent in parents) {
                    var childList = reversedGraph.Get(parent);
                    if (childList == null) {
                        childList = new FIFOHashSet<string>();
                        reversedGraph.Put(parent, childList);
                    }

                    childList.Add(child);
                }
            }

            // Determine all root nodes, which are those without parent
            var roots = new SortedSet<string>();
            foreach (var parents in graph.Values) {
                if (parents == null) {
                    continue;
                }

                foreach (var parent in parents) {
                    // node not itself a child
                    if (!graph.ContainsKey(parent)) {
                        roots.Add(parent);
                    }
                }
            }

            // for each root, recursively add its child nodes, this becomes the default order
            ICollection<string> graphFlattened = new FIFOHashSet<string>();
            foreach (var root in roots) {
                RecursiveAdd(graphFlattened, root, reversedGraph);
            }

            // now walk down the default order and for each node ensure all parents are created
            ICollection<string> created = new FIFOHashSet<string>();
            ICollection<string> removeList = new HashSet<string>();
            while (graphFlattened.IsNotEmpty()) {
                removeList.Clear();
                foreach (var node in graphFlattened) {
                    if (!RecursiveParentsCreated(node, created, graph)) {
                        continue;
                    }

                    created.Add(node);
                    removeList.Add(node);
                }

                graphFlattened.RemoveAll(removeList);
            }

            return created;
        }

        // Determine if all the node's parents and their parents have been added to the created set
        private static bool RecursiveParentsCreated(
            string node,
            ICollection<string> created,
            IDictionary<string, ICollection<string>> graph)
        {
            var parents = graph.Get(node);
            if (parents == null) {
                return true;
            }

            foreach (var parent in parents) {
                if (!created.Contains(parent)) {
                    return false;
                }

                var allParentsCreated = RecursiveParentsCreated(parent, created, graph);
                if (!allParentsCreated) {
                    return false;
                }
            }

            return true;
        }

        private static void RecursiveAdd(
            ICollection<string> graphFlattened,
            string root,
            IDictionary<string, ICollection<string>> reversedGraph)
        {
            graphFlattened.Add(root);
            var childNodes = reversedGraph.Get(root);
            if (childNodes == null) {
                return;
            }

            foreach (var child in childNodes) {
                RecursiveAdd(graphFlattened, child, reversedGraph);
            }
        }

        /// <summary>Returns any circular dependency as a stack of stream numbers, or null if none exist. </summary>
        /// <param name="graph">the dependency graph</param>
        /// <returns>circular dependency stack</returns>
        private static Stack<string> GetFirstCircularDependency(IDictionary<string, ICollection<string>> graph)
        {
            foreach (var child in graph.Keys) {
                var deepDependencies = new Stack<string>();
                deepDependencies.Push(child);

                var isCircular = RecursiveDeepDepends(deepDependencies, child, graph);
                if (isCircular) {
                    return deepDependencies;
                }
            }

            return null;
        }

        private static bool RecursiveDeepDepends(
            Stack<string> deepDependencies,
            string currentChild,
            IDictionary<string, ICollection<string>> graph)
        {
            var required = graph.Get(currentChild);
            if (required == null) {
                return false;
            }

            foreach (var parent in required) {
                if (deepDependencies.Contains(parent)) {
                    return true;
                }

                deepDependencies.Push(parent);
                var isDeep = RecursiveDeepDepends(deepDependencies, parent, graph);
                if (isDeep) {
                    return true;
                }

                deepDependencies.Pop();
            }

            return false;
        }
    }
}