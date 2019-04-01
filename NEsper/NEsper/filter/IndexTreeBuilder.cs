///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.util;

namespace com.espertech.esper.filter
{
    /// <summary>
    /// Builder manipulates a tree structure consisting of <seealso cref="FilterHandleSetNode" /> 
    /// and <seealso cref="FilterParamIndexBase" /> instances. Filters can be added to a top node 
    /// (an instance of FilterHandleSetNode) via the add method. This method returns an instance 
    /// of <seealso cref="EventTypeIndexBuilderIndexLookupablePair" /> which represents an element 
    /// in the tree path (list of indizes) that the filter callback was added to. To remove filters 
    /// the same IndexTreePath instance must be passed in.
    /// <para />
    /// The implementation is designed to be multithread-safe in conjunction with the node classes 
    /// manipulated by this class.
    /// </summary>
    public sealed class IndexTreeBuilder
    {
        private IndexTreeBuilder() { }

        /// <summary>
        /// Add a filter callback according to the filter specification to the top node returning information to be used to remove the filter callback.
        /// </summary>
        /// <param name="filterValueSet">is the filter definition</param>
        /// <param name="filterCallback">is the callback to be added</param>
        /// <param name="topNode">node to be added to any subnode beneath it</param>
        /// <param name="lockFactory">The lock factory.</param>
        /// <returns>
        /// an encapsulation of information need to allow for safe removal of the filter tree.
        /// </returns>
        public static ArrayDeque<EventTypeIndexBuilderIndexLookupablePair>[] Add(
            FilterValueSet filterValueSet,
            FilterHandle filterCallback,
            FilterHandleSetNode topNode,
            FilterServiceGranularLockFactory lockFactory)
        {
            if ((ExecutionPathDebugLog.IsEnabled) && (Log.IsDebugEnabled))
            {
                Log.Debug(".add (" + Thread.CurrentThread.ManagedThreadId + ") Adding filter callback, " +
                          "  topNode=" + topNode +
                          "  filterCallback=" + filterCallback);
            }

            ArrayDeque<EventTypeIndexBuilderIndexLookupablePair>[] treePathInfo;
            if (filterValueSet.Parameters.Length == 0)
            {
                treePathInfo = AllocateTreePath(1);
                treePathInfo[0] = new ArrayDeque<EventTypeIndexBuilderIndexLookupablePair>(1);
#if DEBUG && DIAGNOSTICS
                System.Diagnostics.Debug.WriteLine("{0}: Add -> AddToNode[1]: {0}", Thread.CurrentThread.ManagedThreadId, topNode);
#endif
                AddToNode(new ArrayDeque<FilterValueSetParam>(1), filterCallback, topNode, treePathInfo[0], lockFactory);
            }
            else
            {
                treePathInfo = AllocateTreePath(filterValueSet.Parameters.Length);
                var remainingParameters = new ArrayDeque<FilterValueSetParam>(4);
                for (int i = 0; i < filterValueSet.Parameters.Length; i++)
                {
                    treePathInfo[i] = new ArrayDeque<EventTypeIndexBuilderIndexLookupablePair>(filterValueSet.Parameters[i].Length);
                    remainingParameters.Clear();
                    remainingParameters.AddAll(filterValueSet.Parameters[i]);
#if DEBUG && DIAGNOSTICS
                    System.Diagnostics.Debug.WriteLine("{0}: Add -> AddToNode[0]: {1}", Thread.CurrentThread.ManagedThreadId, topNode);
#endif
                    AddToNode(remainingParameters, filterCallback, topNode, treePathInfo[i], lockFactory);
                }
            }

            return treePathInfo;
        }

        /// <summary>
        /// Remove an filterCallback from the given top node. The IndexTreePath instance passed in must be the same as obtained when the same filterCallback was added.
        /// </summary>
        /// <param name="eventType">Type of the event.</param>
        /// <param name="filterCallback">filter callback  to be removed</param>
        /// <param name="treePathInfo">encapsulates information need to allow for safe removal of the filterCallback</param>
        /// <param name="topNode">The top tree node beneath which the filterCallback was added</param>
        public static void Remove(
            EventType eventType,
            FilterHandle filterCallback,
            EventTypeIndexBuilderIndexLookupablePair[] treePathInfo,
            FilterHandleSetNode topNode)
        {
            if ((ExecutionPathDebugLog.IsEnabled) && (Log.IsDebugEnabled))
            {
                Log.Debug(".remove (" + Thread.CurrentThread.ManagedThreadId + ") Removing filterCallback " +
                          " type " + eventType.Name +
                          "  topNode=" + topNode +
                          "  filterCallback=" + filterCallback);
            }

            RemoveFromNode(filterCallback, topNode, treePathInfo, 0);
        }

        /// <summary>
        /// Add to the current node building up the tree path information.
        /// </summary>
        /// <param name="remainingParameters">The remaining parameters.</param>
        /// <param name="filterCallback">The filter callback.</param>
        /// <param name="currentNode">is the node to add to</param>
        /// <param name="treePathInfo">is filled with information about which indizes were chosen to add the filter to</param>
        /// <param name="lockFactory">The lock factory.</param>
        private static void AddToNode(
            ArrayDeque<FilterValueSetParam> remainingParameters,
            FilterHandle filterCallback,
            FilterHandleSetNode currentNode,
            ArrayDeque<EventTypeIndexBuilderIndexLookupablePair> treePathInfo,
            FilterServiceGranularLockFactory lockFactory)
        {
            if ((ExecutionPathDebugLog.IsEnabled) && (Log.IsDebugEnabled))
            {
                Log.Debug(".addToNode (" + Thread.CurrentThread.ManagedThreadId + ") Adding filterCallback, node=" + currentNode +
                          "  remainingParameters=" + PrintRemainingParameters(remainingParameters));
            }

            // If no parameters are specified, add to current node, and done
            if (remainingParameters.IsEmpty())
            {
                using (currentNode.NodeRWLock.AcquireWriteLock())
                {
#if DEBUG && DIAGNOSTICS
                    System.Diagnostics.Debug.WriteLine("{0}: AddToNode[1] -> Add: {1}", Thread.CurrentThread.ManagedThreadId, currentNode);
#endif
                    currentNode.Add(filterCallback);
                }
                return;
            }

            // Need to find an existing index that matches one of the filter parameters
            Pair<FilterValueSetParam, FilterParamIndexBase> pair;
            using (currentNode.NodeRWLock.AcquireReadLock())
            {
                pair = IndexHelper.FindIndex(remainingParameters, currentNode.Indizes);

                // Found an index matching a filter parameter
                if (pair != null)
                {
                    remainingParameters.Remove(pair.First);
                    var filterForValue = pair.First.FilterForValue;
                    var index = pair.Second;
                    treePathInfo.Add(new EventTypeIndexBuilderIndexLookupablePair(index, filterForValue));
#if DEBUG && DIAGNOSTICS
                    System.Diagnostics.Debug.WriteLine(string.Format("{0}: AddToNode[2] -> AddToIndex: {1} / {2}", Thread.CurrentThread.ManagedThreadId, currentNode, index));
#endif
                    AddToIndex(remainingParameters, filterCallback, index, filterForValue, treePathInfo, lockFactory);
                    return;
                }
            }

            // An index for any of the filter parameters was not found, create one
            using (currentNode.NodeRWLock.AcquireWriteLock())
            {
                pair = IndexHelper.FindIndex(remainingParameters, currentNode.Indizes);

                // Attempt to find an index again this time under a write lock
                if (pair != null)
                {
                    remainingParameters.Remove(pair.First);
                    var filterForValue = pair.First.FilterForValue;
                    var indexX = pair.Second;
                    treePathInfo.Add(new EventTypeIndexBuilderIndexLookupablePair(indexX, filterForValue));
#if DEBUG && DIAGNOSTICS
                    System.Diagnostics.Debug.WriteLine("{0}: AddToNode[3] -> AddToIndex: {1} / {2}", Thread.CurrentThread.ManagedThreadId, currentNode, indexX);
#endif
                    AddToIndex(remainingParameters, filterCallback, indexX, filterForValue, treePathInfo, lockFactory);
                    return;
                }

                // No index found that matches any parameters, create a new one
                // Pick the next parameter for an index
                FilterValueSetParam parameterPickedForIndex = remainingParameters.RemoveFirst();

                var index = IndexFactory.CreateIndex(parameterPickedForIndex.Lookupable, lockFactory, parameterPickedForIndex.FilterOperator);

                currentNode.Indizes.Add(index);
                treePathInfo.Add(new EventTypeIndexBuilderIndexLookupablePair(index, parameterPickedForIndex.FilterForValue));
#if DEBUG && DIAGNOSTICS
                System.Diagnostics.Debug.WriteLine("{0}: AddToNode[4] -> AddToIndex: {1}", Thread.CurrentThread.ManagedThreadId, index);
#endif
                AddToIndex(remainingParameters, filterCallback, index, parameterPickedForIndex.FilterForValue, treePathInfo, lockFactory);
            }
        }

        // Remove an filterCallback from the current node, return true if the node is the node is empty now
        private static bool RemoveFromNode(
            FilterHandle filterCallback,
            FilterHandleSetNode currentNode,
            EventTypeIndexBuilderIndexLookupablePair[] treePathInfo,
            int treePathPosition)
        {
            var nextPair = treePathPosition < treePathInfo.Length ? treePathInfo[treePathPosition++] : null;

            // No remaining filter parameters
            if (nextPair == null)
            {
                using (currentNode.NodeRWLock.AcquireWriteLock())
                {
                    var isRemoved = currentNode.Remove(filterCallback);
                    var isEmpty = currentNode.IsEmpty();

                    if (!isRemoved)
                    {
                        Log.Warn(".removeFromNode (" + Thread.CurrentThread.ManagedThreadId + ") Could not find the filterCallback to be removed within the supplied node , node=" +
                                currentNode + "  filterCallback=" + filterCallback);
                    }

                    return isEmpty;
                }
            }

            // Remove from index
            var nextIndex = nextPair.Index;
            var filteredForValue = nextPair.Lookupable;

            using (currentNode.NodeRWLock.AcquireWriteLock())
            {
                var isEmpty = RemoveFromIndex(filterCallback, nextIndex, treePathInfo, treePathPosition, filteredForValue);

                if (!isEmpty)
                {
                    return false;
                }

                // Remove the index if the index is now empty
                if (nextIndex.IsEmpty)
                {
                    var isRemoved = currentNode.Remove(nextIndex);

                    if (!isRemoved)
                    {
                        Log.Warn(".removeFromNode (" + Thread.CurrentThread.ManagedThreadId + ") Could not find the index in index list for removal, index=" +
                                nextIndex + "  filterCallback=" + filterCallback);
                        return false;
                    }
                }

                return currentNode.IsEmpty();
            }
        }

        // Remove filterCallback from index, returning true if index empty after removal
        private static bool RemoveFromIndex(
            FilterHandle filterCallback,
            FilterParamIndexBase index,
            EventTypeIndexBuilderIndexLookupablePair[] treePathInfo,
            int treePathPosition,
            Object filterForValue)
        {
            using (index.ReadWriteLock.AcquireWriteLock())
            {
                EventEvaluator eventEvaluator = index[filterForValue];

                if (eventEvaluator == null)
                {
                    Log.Warn(".removeFromIndex ({0}) Could not find the filterCallback value in index, index={1}  value={2}  filterCallback={3}",
                        Thread.CurrentThread.ManagedThreadId, index, filterForValue, filterCallback);
                    return false;
                }

                if (eventEvaluator is FilterHandleSetNode)
                {
                    var node = (FilterHandleSetNode)eventEvaluator;
                    var isEmptyX = RemoveFromNode(filterCallback, node, treePathInfo, treePathPosition);
                    if (isEmptyX)
                    {
                        // Since we are holding a write lock to this index, there should not be a chance that
                        // another thread had been adding anything to this FilterHandleSetNode
                        index.Remove(filterForValue);
                    }

                    return index.IsEmpty;
                }

                var nextIndex = (FilterParamIndexBase)eventEvaluator;
                var nextPair = treePathPosition < treePathInfo.Length ? treePathInfo[treePathPosition++] : null;

                if (nextPair == null)
                {
                    Log.Fatal(".removeFromIndex Expected an inner index to this index, this=" + filterCallback);
                    System.Diagnostics.Debug.Assert(false);
                    return false;
                }

                if (nextPair.Index != nextIndex)
                {
                    Log.Fatal(".removeFromIndex Expected an index for filterCallback that differs from the found index, this=" + filterCallback +
                            "  expected=" + nextPair.Index);
                    System.Diagnostics.Debug.Assert(false);
                    return false;
                }

                var nextExpressionValue = nextPair.Lookupable;

                var isEmpty = RemoveFromIndex(filterCallback, nextPair.Index, treePathInfo, treePathPosition, nextExpressionValue);
                if (isEmpty)
                {
                    // Since we are holding a write lock to this index, there should not be a chance that
                    // another thread had been adding anything to this FilterHandleSetNode
                    index.Remove(filterForValue);
                }

                return index.IsEmpty;
            }
        }

        /// <summary>
        /// Add to an index the value to filter for.
        /// </summary>
        /// <param name="remainingParameters">The remaining parameters.</param>
        /// <param name="filterCallback">The filter callback.</param>
        /// <param name="index">is the index to add to</param>
        /// <param name="filterForValue">is the filter parameter value to add</param>
        /// <param name="treePathInfo">is the specification to fill on where is was added</param>
        /// <param name="lockFactory">The lock factory.</param>
        private static void AddToIndex(
            ArrayDeque<FilterValueSetParam> remainingParameters,
            FilterHandle filterCallback,
            FilterParamIndexBase index,
            Object filterForValue,
            ArrayDeque<EventTypeIndexBuilderIndexLookupablePair> treePathInfo,
            FilterServiceGranularLockFactory lockFactory)
        {
            if ((ExecutionPathDebugLog.IsEnabled) && (Log.IsDebugEnabled))
            {
                Log.Debug(".addToIndex ({0}) Adding to index {1}  expressionValue={2}",
                    Thread.CurrentThread.ManagedThreadId, index, filterForValue);
            }

#if DEBUG && DIAGNOSTICS
            System.Diagnostics.Debug.WriteLine("{0}: AddToIndex[-2] - Adding to index {1}  expressionValue={2}",
                Thread.CurrentThread.ManagedThreadId, index, filterForValue);
#endif

            EventEvaluator eventEvaluator;

            using (index.ReadWriteLock.AcquireReadLock())
            {
                eventEvaluator = index[filterForValue];
#if DEBUG && DIAGNOSTICS
                System.Diagnostics.Debug.WriteLine("{0}: AddToIndex[-1] - eventEvaluator = {1}",
                    Thread.CurrentThread.ManagedThreadId, eventEvaluator);
#endif

                // The filter parameter value already existed in bean, add and release locks
                if (eventEvaluator != null)
                {
#if DEBUG && DIAGNOSTICS
                    System.Diagnostics.Debug.WriteLine("{0}: AddToIndex[0] -> AddToEvaluator: {1}", Thread.CurrentThread.ManagedThreadId, eventEvaluator);
#endif
                    var added = AddToEvaluator(remainingParameters, filterCallback, eventEvaluator, treePathInfo, lockFactory);
                    if (added)
                    {
                        return;
                    }
                }
            }

            // new filter parameter value, need a write lock
            using (index.ReadWriteLock.AcquireWriteLock())
            {
                eventEvaluator = index[filterForValue];

                // It may exist now since another thread could have added the entry
                if (eventEvaluator != null)
                {
#if DEBUG && DIAGNOSTICS
                    System.Diagnostics.Debug.WriteLine("{0}: AddToIndex[1] -> AddToEvaluator: {1}", Thread.CurrentThread.ManagedThreadId, eventEvaluator);
#endif

                    var added = AddToEvaluator(remainingParameters, filterCallback, eventEvaluator, treePathInfo, lockFactory);
                    if (added)
                    {
                        return;
                    }

                    // The found eventEvaluator must be converted to a new FilterHandleSetNode
                    var nextIndexX = (FilterParamIndexBase) eventEvaluator;
#if DEBUG && DIAGNOSTICS
                    System.Diagnostics.Debug.WriteLine("{0}: new[2]", Thread.CurrentThread.ManagedThreadId);
#endif
                    var newNode = new FilterHandleSetNode(lockFactory.ObtainNew());
                    newNode.Add(nextIndexX);
                    index.Remove(filterForValue);
                    index[filterForValue] = newNode;
                    AddToNode(remainingParameters, filterCallback, newNode, treePathInfo, lockFactory);

                    return;
                }

                // The index does not currently have this filterCallback value,
                // if there are no remaining parameters, create a node
                if (remainingParameters.IsEmpty())
                {
#if DEBUG && DIAGNOSTICS
                    System.Diagnostics.Debug.WriteLine("{0}: new[3]", Thread.CurrentThread.ManagedThreadId);
#endif
                    var node = new FilterHandleSetNode(lockFactory.ObtainNew());
                    AddToNode(remainingParameters, filterCallback, node, treePathInfo, lockFactory);
                    index[filterForValue] = node;
                    return;
                }

                // If there are remaining parameters, create a new index for the next parameter
                FilterValueSetParam parameterPickedForIndex = remainingParameters.RemoveFirst();

                var nextIndex = IndexFactory.CreateIndex(parameterPickedForIndex.Lookupable, lockFactory, parameterPickedForIndex.FilterOperator);

                index[filterForValue] = nextIndex;
                treePathInfo.Add(new EventTypeIndexBuilderIndexLookupablePair(nextIndex, parameterPickedForIndex.FilterForValue));
#if DEBUG && DIAGNOSTICS
                System.Diagnostics.Debug.WriteLine("{0}: AddToIndex[2] -> AddToEvaluator: {1}", Thread.CurrentThread.ManagedThreadId, eventEvaluator);
#endif
                AddToIndex(remainingParameters, filterCallback, nextIndex, parameterPickedForIndex.FilterForValue, treePathInfo, lockFactory);
            }
        }

        /// <summary>
        /// Add filter callback to an event evaluator, which could be either an index node or a set node.
        /// </summary>
        /// <param name="remainingParameters">The remaining parameters.</param>
        /// <param name="filterCallback">The filter callback.</param>
        /// <param name="eventEvaluator">to add the filterCallback to.</param>
        /// <param name="treePathInfo">is for holding the information on where the add occured</param>
        /// <param name="lockFactory">The lock factory.</param>
        /// <returns>
        /// bool indicating if the eventEvaluator was successfully added
        /// </returns>
        private static bool AddToEvaluator(
            ArrayDeque<FilterValueSetParam> remainingParameters,
            FilterHandle filterCallback,
            EventEvaluator eventEvaluator,
            ArrayDeque<EventTypeIndexBuilderIndexLookupablePair> treePathInfo,
            FilterServiceGranularLockFactory lockFactory)
        {
            if (eventEvaluator is FilterHandleSetNode)
            {
                var node = (FilterHandleSetNode)eventEvaluator;
#if DEBUG && DIAGNOSTICS
                System.Diagnostics.Debug.WriteLine("{0}: AddToEvaluator: {1}", Thread.CurrentThread.ManagedThreadId, node);
#endif
                AddToNode(remainingParameters, filterCallback, node, treePathInfo, lockFactory);
                return true;
            }

            // Check if the next index matches any of the remaining filterCallback parameters
            var nextIndex = (FilterParamIndexBase)eventEvaluator;

            var parameter = IndexHelper.FindParameter(remainingParameters, nextIndex);
            if (parameter != null)
            {
                remainingParameters.Remove(parameter);
                treePathInfo.Add(new EventTypeIndexBuilderIndexLookupablePair(nextIndex, parameter.FilterForValue));
#if DEBUG && DIAGNOSTICS
                System.Diagnostics.Debug.WriteLine("{0}: AddToEvaluator -> AddToIndex: {1}", Thread.CurrentThread.ManagedThreadId, nextIndex);
#endif
                AddToIndex(remainingParameters, filterCallback, nextIndex, parameter.FilterForValue, treePathInfo, lockFactory);
                return true;
            }

            // This eventEvaluator does not work with any of the remaining filter parameters
            return false;
        }

        private static String PrintRemainingParameters(IEnumerable<FilterValueSetParam> remainingParameters)
        {
            var buffer = new StringBuilder();

            var count = 0;
            foreach (var parameter in remainingParameters)
            {
                buffer.Append("  Param(").Append(count).Append(')');
                buffer.Append(" property=").Append(parameter.Lookupable);
                buffer.Append(" operator=").Append(parameter.FilterOperator);
                buffer.Append(" value=").Append(parameter.FilterForValue);
                count++;
            }

            return buffer.ToString();
        }

        private static ArrayDeque<EventTypeIndexBuilderIndexLookupablePair>[] AllocateTreePath(int size)
        {
            return new ArrayDeque<EventTypeIndexBuilderIndexLookupablePair>[size];
        }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
