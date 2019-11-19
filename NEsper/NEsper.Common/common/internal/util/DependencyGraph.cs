///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.util
{
    /// <summary>
    ///     Model of dependency of lookup, in which one stream supplies values for lookup in another stream.
    /// </summary>
    public class DependencyGraph
    {
        private readonly bool _allowDependencySame;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="numStreams">number of streams</param>
        /// <param name="allowDependencySame">if set to <c>true</c> [allow dependency same].</param>
        public DependencyGraph(
            int numStreams,
            bool allowDependencySame)
        {
            NumStreams = numStreams;
            _allowDependencySame = allowDependencySame;
            Dependencies = new Dictionary<int, ICollection<int>>();
        }

        /// <summary>Returns the number of streams. </summary>
        /// <value>number of streams</value>
        public int NumStreams { get; }

        /// <summary>
        ///     Returns a map of stream number and the streams dependencies.
        /// </summary>
        /// <value>map of dependencies</value>
        public IDictionary<int, ICollection<int>> Dependencies { get; }

        /// <summary>
        ///     Returns a set of stream numbers that are the root dependencies, i.e. the dependencies with the deepest graph.
        /// </summary>
        /// <value>set of stream number of streams</value>
        public ICollection<int> RootNodes {
            get {
                var rootNodes = new HashSet<int>();

                for (var i = 0; i < NumStreams; i++) {
                    var found = Dependencies.Any(entry => entry.Value.Contains(i));
                    if (!found) {
                        rootNodes.Add(i);
                    }
                }

                return rootNodes;
            }
        }

        /// <summary>Returns any circular dependency as a stack of stream numbers, or null if none exist. </summary>
        /// <value>circular dependency stack</value>
        public IEnumerable<int> FirstCircularDependency {
            get {
                for (var i = 0; i < NumStreams; i++) {
                    var deepDependencies = new Stack<int>();
                    deepDependencies.Push(i);

                    var isCircular = RecursiveDeepDepends(deepDependencies, i);
                    if (isCircular) {
                        return deepDependencies.Reverse();
                    }
                }

                return null;
            }
        }

        public override string ToString()
        {
            var writer = new StringWriter();

            var count = 0;
            foreach (var entry in Dependencies) {
                count++;
                writer.WriteLine("Record {0}: from={1} to={2}", count, entry.Key, entry.Value.RenderAny());
            }

            return writer.ToString();
        }

        /// <summary>
        ///     Adds dependencies that a target may have on required streams.
        /// </summary>
        /// <param name="target">the stream having dependencies on one or more other streams</param>
        /// <param name="requiredStreams">the streams that the target stream has a dependency on</param>
        public void AddDependency(
            int target,
            ICollection<int> requiredStreams)
        {
            if (requiredStreams.Contains(target)) {
                throw new ArgumentException("Dependency between same streams is not allowed for stream " + target);
            }

            var toSet = Dependencies.Get(target);
            if (toSet != null) {
                throw new ArgumentException("Dependencies from stream " + target + " already in collection");
            }

            Dependencies.Put(target, requiredStreams);
        }

        /// <summary>
        ///     Adds a single dependency of target on a required streams.
        /// </summary>
        /// <param name="target">the stream having dependencies on one or more other streams</param>
        /// <param name="from">a single required streams that the target stream has a dependency on</param>
        public void AddDependency(
            int target,
            int from)
        {
            if (target == from && !_allowDependencySame) {
                throw new ArgumentException("Dependency between same streams is not allowed for stream " + target);
            }

            var toSet = Dependencies.Get(target);
            if (toSet == null) {
                toSet = new SortedSet<int>();
                Dependencies.Put(target, toSet);
            }

            toSet.Add(from);
        }

        /// <summary>
        ///     Returns true if the stream asked for has a dependency.
        /// </summary>
        /// <param name="stream">to check dependency for</param>
        /// <returns>true if a dependency exist, false if not</returns>
        public bool HasDependency(int stream)
        {
            var dep = Dependencies.Get(stream);
            if (dep != null) {
                return dep.IsNotEmpty();
            }

            return false;
        }

        /// <summary>Returns the set of dependent streams for a given stream. </summary>
        /// <param name="stream">to return dependent streams for</param>
        /// <returns>set of stream numbers of stream providing properties</returns>
        public ICollection<int> GetDependenciesForStream(int stream)
        {
            var dep = Dependencies.Get(stream);
            if (dep != null) {
                return dep;
            }

            return new int[0];
        }

        /// <summary>
        ///     Return the root nodes ignoring the nodes provided.
        /// </summary>
        /// <param name="ignoreList">nodes to be ignored</param>
        /// <returns>root nodes</returns>
        public ICollection<int> GetRootNodes(ICollection<int> ignoreList)
        {
            var rootNodes = new HashSet<int>();

            for (var i = 0; i < NumStreams; i++) {
                if (ignoreList.Contains(i)) {
                    continue;
                }

                var found = Dependencies
                    .Where(entry => entry.Value.Contains(i))
                    .Any(entry => !ignoreList.Contains(entry.Key));
                if (!found) {
                    rootNodes.Add(i);
                }
            }

            return rootNodes;
        }

        private bool RecursiveDeepDepends(
            Stack<int> deepDependencies,
            int currentStream)
        {
            var required = Dependencies.Get(currentStream);
            if (required == null) {
                return false;
            }

            foreach (var stream in required) {
                if (deepDependencies.Contains(stream)) {
                    return true;
                }

                deepDependencies.Push(stream);
                var isDeep = RecursiveDeepDepends(deepDependencies, stream);
                if (isDeep) {
                    return true;
                }

                deepDependencies.Pop();
            }

            return false;
        }

        /// <summary>
        ///     Check if the given stream has any dependencies, direct or indirect, to any of the streams that are not in the
        ///     ignore list.
        /// </summary>
        public bool HasUnsatisfiedDependency(
            int navigableStream,
            ICollection<int> ignoreList)
        {
            var deepDependencies = new HashSet<int>();
            RecursivePopulateDependencies(navigableStream, deepDependencies);

            foreach (var dependency in deepDependencies) {
                if (!ignoreList.Contains(dependency)) {
                    return true;
                }
            }

            return false;
        }

        private void RecursivePopulateDependencies(
            int navigableStream,
            ICollection<int> deepDependencies)
        {
            var dependencies = GetDependenciesForStream(navigableStream);
            deepDependencies.AddAll(dependencies);
            foreach (var dependency in dependencies) {
                RecursivePopulateDependencies(dependency, deepDependencies);
            }
        }
    }
}