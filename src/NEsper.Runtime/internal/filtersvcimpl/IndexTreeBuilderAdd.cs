///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Reflection;
using System.Threading;

using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.filtersvc;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.runtime.@internal.filtersvcimpl
{
    public class IndexTreeBuilderAdd
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private IndexTreeBuilderAdd()
        {
        }

        /// <summary>
        ///     Add a filter callback according to the filter specification to the top node returning
        ///     information to be used to remove the filter callback.
        /// </summary>
        /// <param name="valueSet">is the filter definition</param>
        /// <param name="filterCallback">is the callback to be added</param>
        /// <param name="topNode">node to be added to any subnode beneath it</param>
        /// <param name="lockFactory">lock factory</param>
        public static void Add(
            FilterValueSetParam[][] valueSet,
            FilterHandle filterCallback,
            FilterHandleSetNode topNode,
            FilterServiceGranularLockFactory lockFactory)
        {
            if (ExecutionPathDebugLog.IsDebugEnabled && Log.IsDebugEnabled) {
                Log.Debug(
                    ".add (" + Thread.CurrentThread.ManagedThreadId + ") Adding filter callback, " +
                    "  topNode=" + topNode +
                    "  filterCallback=" + filterCallback);
            }

            if (valueSet.Length == 0) {
                AddToNode(new ArrayDeque<FilterValueSetParam>(1), filterCallback, topNode, lockFactory);
            }
            else {
                var remainingParameters = new ArrayDeque<FilterValueSetParam>();
                for (var i = 0; i < valueSet.Length; i++) {
                    remainingParameters.Clear();
                    remainingParameters.AddAll(valueSet[i]);
                    AddToNode(remainingParameters, filterCallback, topNode, lockFactory);
                }
            }
        }

        /// <summary>Add to the current node building up the tree path information.</summary>
        /// <param name="remainingParameters">any remaining parameters</param>
        /// <param name="filterCallback">the filter callback</param>
        /// <param name="currentNode">is the node to add to</param>
        /// <param name="lockFactory">the lock factory</param>
        private static void AddToNode(
            ArrayDeque<FilterValueSetParam> remainingParameters,
            FilterHandle filterCallback,
            FilterHandleSetNode currentNode,
            FilterServiceGranularLockFactory lockFactory)
        {
            // If no parameters are specified, add to current node, and done
            if (remainingParameters.IsEmpty()) {
                using (currentNode.NodeRWLock.WriteLock.Acquire())
                {
                    currentNode.Add(filterCallback);
                }

                return;
            }

            // Need to find an existing index that matches one of the filter parameters
            Pair<FilterValueSetParam, FilterParamIndexBase> pair;
            using (currentNode.NodeRWLock.ReadLock.Acquire())
            {
                pair = IndexHelper.FindIndex(remainingParameters, currentNode.Indizes);

                // Found an index matching a filter parameter
                if (pair != null)
                {
                    remainingParameters.Remove(pair.First);
                    var filterForValue = pair.First.FilterForValue;
                    var index = pair.Second;
                    AddToIndex(remainingParameters, filterCallback, index, filterForValue, lockFactory);
                    return;
                }
            }

            // An index for any of the filter parameters was not found, create one
            using (currentNode.NodeRWLock.WriteLock.Acquire())
            {
                pair = IndexHelper.FindIndex(remainingParameters, currentNode.Indizes);

                // Attempt to find an index again this time under a write lock
                if (pair != null)
                {
                    remainingParameters.Remove(pair.First);
                    var filterForValue = pair.First.FilterForValue;
                    var indexInner = pair.Second;
                    AddToIndex(remainingParameters, filterCallback, indexInner, filterForValue, lockFactory);
                    return;
                }

                // No index found that matches any parameters, create a new one
                // Pick the next parameter for an index
                var parameterPickedForIndex = remainingParameters.RemoveFirst();
                var index = IndexFactory.CreateIndex(parameterPickedForIndex.Lookupable, lockFactory, parameterPickedForIndex.FilterOperator);

                currentNode.Add(index);
                AddToIndex(remainingParameters, filterCallback, index, parameterPickedForIndex.FilterForValue, lockFactory);
            }
        }

        /// <summary>
        ///     Add to an index the value to filter for.
        /// </summary>
        /// <param name="index">is the index to add to</param>
        /// <param name="filterForValue">is the filter parameter value to add</param>
        /// <param name="remainingParameters">any remaining parameters</param>
        /// <param name="filterCallback">the filter callback</param>
        /// <param name="lockFactory">the lock factory</param>
        private static void AddToIndex(
            ArrayDeque<FilterValueSetParam> remainingParameters,
            FilterHandle filterCallback,
            FilterParamIndexBase index,
            object filterForValue,
            FilterServiceGranularLockFactory lockFactory)
        {
            EventEvaluator eventEvaluator;

            using (index.ReadWriteLock.ReadLock.Acquire())
            { 
                eventEvaluator = index.Get(filterForValue);

                // The filter parameter value already existed in bean, add and release locks
                if (eventEvaluator != null) {
                    var added = AddToEvaluator(remainingParameters, filterCallback, eventEvaluator, lockFactory);
                    if (added) {
                        return;
                    }
                }
            }

            // new filter parameter value, need a write lock
            using (index.ReadWriteLock.WriteLock.Acquire())
            {
                eventEvaluator = index.Get(filterForValue);

                // It may exist now since another thread could have added the entry
                if (eventEvaluator != null)
                {
                    var added = AddToEvaluator(remainingParameters, filterCallback, eventEvaluator, lockFactory);
                    if (added)
                    {
                        return;
                    }

                    // The found eventEvaluator must be converted to a new FilterHandleSetNode
                    var nextIndexInner = (FilterParamIndexBase) eventEvaluator;
                    var newNode = new FilterHandleSetNode(lockFactory.ObtainNew());
                    newNode.Add(nextIndexInner);
                    index.Remove(filterForValue);
                    index.Put(filterForValue, newNode);
                    AddToNode(remainingParameters, filterCallback, newNode, lockFactory);

                    return;
                }

                // The index does not currently have this filterCallback value,
                // if there are no remaining parameters, create a node
                if (remainingParameters.IsEmpty())
                {
                    var node = new FilterHandleSetNode(lockFactory.ObtainNew());
                    AddToNode(remainingParameters, filterCallback, node, lockFactory);
                    index.Put(filterForValue, node);
                    return;
                }

                // If there are remaining parameters, create a new index for the next parameter
                var parameterPickedForIndex = remainingParameters.RemoveFirst();

                var nextIndex = IndexFactory.CreateIndex(parameterPickedForIndex.Lookupable, lockFactory, parameterPickedForIndex.FilterOperator);

                index.Put(filterForValue, nextIndex);
                AddToIndex(remainingParameters, filterCallback, nextIndex, parameterPickedForIndex.FilterForValue, lockFactory);
            }
        }

        /// <summary>Add filter callback to an event evaluator, which could be either an index node or a set node.</summary>
        /// <param name="remainingParameters">any remaining parameters</param>
        /// <param name="filterCallback">the filter callback</param>
        /// <param name="eventEvaluator">to add the filterCallback to.</param>
        /// <param name="lockFactory">
        ///   <para>the lock factory</para>
        /// </param>
        /// <returns>boolean indicating if the eventEvaluator was successfully added</returns>
        private static bool AddToEvaluator(
            ArrayDeque<FilterValueSetParam> remainingParameters,
            FilterHandle filterCallback,
            EventEvaluator eventEvaluator,
            FilterServiceGranularLockFactory lockFactory)
        {
            if (eventEvaluator is FilterHandleSetNode) {
                var node = (FilterHandleSetNode) eventEvaluator;
                AddToNode(remainingParameters, filterCallback, node, lockFactory);
                return true;
            }

            // Check if the next index matches any of the remaining filterCallback parameters
            var nextIndex = (FilterParamIndexBase) eventEvaluator;

            var parameter = IndexHelper.FindParameter(remainingParameters, nextIndex);
            if (parameter != null) {
                remainingParameters.Remove(parameter);
                AddToIndex(remainingParameters, filterCallback, nextIndex, parameter.FilterForValue, lockFactory);
                return true;
            }

            // This eventEvaluator does not work with any of the remaining filter parameters
            return false;
        }
    }
} // end of namespace