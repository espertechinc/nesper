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
using com.espertech.esper.util;

namespace com.espertech.esper.filter
{
    /// <summary>
    /// Builder manipulates a tree structure consisting of <seealso cref="FilterHandleSetNode" /> and <seealso cref="FilterParamIndexBase" /> instances.
    /// Filters can be added to a top node (an instance of FilterHandleSetNode) via the add method. This method returns
    /// an instance of <seealso cref="EventTypeIndexBuilderIndexLookupablePair" /> which represents an element in the tree path (list of indizes) that the filter callback was
    /// added to. To remove filters the same IndexTreePath instance must be passed in.
    /// <para>
    /// The implementation is designed to be multithread-safe in conjunction with the node classes manipulated by this class.
    /// </para>
    /// </summary>
    public sealed class IndexTreeBuilder {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        private IndexTreeBuilder() {
        }
    
        /// <summary>
        /// Add a filter callback according to the filter specification to the top node returning
        /// information to be used to remove the filter callback.
        /// </summary>
        /// <param name="filterValueSet">is the filter definition</param>
        /// <param name="filterCallback">is the callback to be added</param>
        /// <param name="topNode">node to be added to any subnode beneath it</param>
        /// <param name="lockFactory">lock factory</param>
        /// <returns>
        /// an encapsulation of information need to allow for safe removal of the filter tree.
        /// </returns>
        public static ArrayDeque<EventTypeIndexBuilderIndexLookupablePair>[] Add(FilterValueSet filterValueSet,
                                                                                 FilterHandle filterCallback,
                                                                                 FilterHandleSetNode topNode,
                                                                                 FilterServiceGranularLockFactory lockFactory) {
            if ((ExecutionPathDebugLog.isDebugEnabled) && (Log.IsDebugEnabled)) {
                Log.Debug(".add (" + Thread.CurrentThread().Id + ") Adding filter callback, " +
                        "  topNode=" + topNode +
                        "  filterCallback=" + filterCallback);
            }
    
            ArrayDeque<EventTypeIndexBuilderIndexLookupablePair>[] treePathInfo;
            if (filterValueSet.Parameters.Length == 0) {
                treePathInfo = AllocateTreePath(1);
                treePathInfo[0] = new ArrayDeque<EventTypeIndexBuilderIndexLookupablePair>(1);
                AddToNode(new ArrayDeque<FilterValueSetParam>(1), filterCallback, topNode, treePathInfo[0], lockFactory);
            } else {
                treePathInfo = AllocateTreePath(filterValueSet.Parameters.Length);
                var remainingParameters = new ArrayDeque<FilterValueSetParam>(4);
                for (int i = 0; i < filterValueSet.Parameters.Length; i++) {
                    treePathInfo[i] = new ArrayDeque<EventTypeIndexBuilderIndexLookupablePair>(filterValueSet.Parameters[i].Length);
                    remainingParameters.Clear();
                    Collections.AddAll(remainingParameters, filterValueSet.Parameters[i]);
                    AddToNode(remainingParameters, filterCallback, topNode, treePathInfo[i], lockFactory);
                }
            }
    
            return treePathInfo;
        }
    
        /// <summary>
        /// Remove an filterCallback from the given top node. The IndexTreePath instance passed in must be the
        /// same as obtained when the same filterCallback was added.
        /// </summary>
        /// <param name="filterCallback">filter callback  to be removed</param>
        /// <param name="treePathInfo">encapsulates information need to allow for safe removal of the filterCallback</param>
        /// <param name="topNode">The top tree node beneath which the filterCallback was added</param>
        /// <param name="eventType">event type</param>
        public static void Remove(
                EventType eventType,
                FilterHandle filterCallback,
                EventTypeIndexBuilderIndexLookupablePair[] treePathInfo,
                FilterHandleSetNode topNode) {
            if ((ExecutionPathDebugLog.isDebugEnabled) && (Log.IsDebugEnabled)) {
                Log.Debug(".remove (" + Thread.CurrentThread().Id + ") Removing filterCallback " +
                        " type " + eventType.Name +
                        " topNode=" + topNode +
                        " filterCallback=" + filterCallback);
            }
    
            RemoveFromNode(filterCallback, topNode, treePathInfo, 0);
        }
    
        /// <summary>
        /// Add to the current node building up the tree path information.
        /// </summary>
        /// <param name="currentNode">is the node to add to</param>
        /// <param name="treePathInfo">is filled with information about which indizes were chosen to add the filter to</param>
        private static void AddToNode(ArrayDeque<FilterValueSetParam> remainingParameters,
                                      FilterHandle filterCallback,
                                      FilterHandleSetNode currentNode,
                                      ArrayDeque<EventTypeIndexBuilderIndexLookupablePair> treePathInfo,
                                      FilterServiceGranularLockFactory lockFactory) {
            if ((ExecutionPathDebugLog.isDebugEnabled) && (Log.IsDebugEnabled)) {
                Log.Debug(".addToNode (" + Thread.CurrentThread().Id + ") Adding filterCallback, node=" + currentNode +
                        "  remainingParameters=" + PrintRemainingParameters(remainingParameters));
            }
    
            // If no parameters are specified, add to current node, and done
            if (remainingParameters.IsEmpty()) {
                currentNode.NodeRWLock.WriteLock().Lock();
                try {
                    currentNode.Add(filterCallback);
                } finally {
                    currentNode.NodeRWLock.WriteLock().Unlock();
                }
                return;
            }
    
            // Need to find an existing index that matches one of the filter parameters
            currentNode.NodeRWLock.ReadLock().Lock();
            Pair<FilterValueSetParam, FilterParamIndexBase> pair;
            try {
                pair = IndexHelper.FindIndex(remainingParameters, currentNode.Indizes);
    
                // Found an index matching a filter parameter
                if (pair != null) {
                    remainingParameters.Remove(pair.First);
                    Object filterForValue = pair.First.FilterForValue;
                    FilterParamIndexBase index = pair.Second;
                    treePathInfo.Add(new EventTypeIndexBuilderIndexLookupablePair(index, filterForValue));
                    AddToIndex(remainingParameters, filterCallback, index, filterForValue, treePathInfo, lockFactory);
                    return;
                }
            } finally {
                currentNode.NodeRWLock.ReadLock().Unlock();
            }
    
            // An index for any of the filter parameters was not found, create one
            currentNode.NodeRWLock.WriteLock().Lock();
            try {
                pair = IndexHelper.FindIndex(remainingParameters, currentNode.Indizes);
    
                // Attempt to find an index again this time under a write lock
                if (pair != null) {
                    remainingParameters.Remove(pair.First);
                    Object filterForValue = pair.First.FilterForValue;
                    FilterParamIndexBase index = pair.Second;
                    treePathInfo.Add(new EventTypeIndexBuilderIndexLookupablePair(index, filterForValue));
                    AddToIndex(remainingParameters, filterCallback, index, filterForValue, treePathInfo, lockFactory);
                    return;
                }
    
                // No index found that matches any parameters, create a new one
                // Pick the next parameter for an index
                FilterValueSetParam parameterPickedForIndex = remainingParameters.RemoveFirst();
    
                FilterParamIndexBase index = IndexFactory.CreateIndex(parameterPickedForIndex.Lookupable, lockFactory, parameterPickedForIndex.FilterOperator);
    
                currentNode.Indizes.Add(index);
                treePathInfo.Add(new EventTypeIndexBuilderIndexLookupablePair(index, parameterPickedForIndex.FilterForValue));
                AddToIndex(remainingParameters, filterCallback, index, parameterPickedForIndex.FilterForValue, treePathInfo, lockFactory);
            } finally {
                currentNode.NodeRWLock.WriteLock().Unlock();
            }
        }
    
        // Remove an filterCallback from the current node, return true if the node is the node is empty now
        private static bool RemoveFromNode(FilterHandle filterCallback,
                                              FilterHandleSetNode currentNode,
                                              EventTypeIndexBuilderIndexLookupablePair[] treePathInfo,
                                              int treePathPosition) {
            EventTypeIndexBuilderIndexLookupablePair nextPair = treePathPosition < treePathInfo.Length ? treePathInfo[treePathPosition++] : null;
    
            // No remaining filter parameters
            if (nextPair == null) {
                currentNode.NodeRWLock.WriteLock().Lock();
    
                try {
                    bool isRemoved = currentNode.Remove(filterCallback);
                    bool isEmpty = currentNode.IsEmpty();
    
                    if (!isRemoved) {
                        Log.Warn(".removeFromNode (" + Thread.CurrentThread().Id + ") Could not find the filterCallback to be removed within the supplied node , node=" +
                                currentNode + "  filterCallback=" + filterCallback);
                    }
    
                    return isEmpty;
                } finally {
                    currentNode.NodeRWLock.WriteLock().Unlock();
                }
            }
    
            // Remove from index
            FilterParamIndexBase nextIndex = nextPair.Index;
            Object filteredForValue = nextPair.Lookupable;
    
            currentNode.NodeRWLock.WriteLock().Lock();
            try {
                bool isEmpty = RemoveFromIndex(filterCallback, nextIndex, treePathInfo, treePathPosition, filteredForValue);
    
                if (!isEmpty) {
                    return false;
                }
    
                // Remove the index if the index is now empty
                if (nextIndex.Count == 0) {
                    bool isRemoved = currentNode.Remove(nextIndex);
    
                    if (!isRemoved) {
                        Log.Warn(".removeFromNode (" + Thread.CurrentThread().Id + ") Could not find the index in index list for removal, index=" +
                                nextIndex.ToString() + "  filterCallback=" + filterCallback);
                        return false;
                    }
                }
    
                return CurrentNode.IsEmpty();
            } finally {
                currentNode.NodeRWLock.WriteLock().Unlock();
            }
        }
    
        // Remove filterCallback from index, returning true if index empty after removal
        private static bool RemoveFromIndex(FilterHandle filterCallback,
                                               FilterParamIndexBase index,
                                               EventTypeIndexBuilderIndexLookupablePair[] treePathInfo,
                                               int treePathPosition,
                                               Object filterForValue) {
            index.ReadWriteLock.WriteLock().Lock();
            try {
                EventEvaluator eventEvaluator = index.Get(filterForValue);
    
                if (eventEvaluator == null) {
                    Log.Warn(".removeFromIndex (" + Thread.CurrentThread().Id + ") Could not find the filterCallback value in index, index=" +
                            index.ToString() + "  value=" + filterForValue.ToString() + "  filterCallback=" + filterCallback);
                    return false;
                }
    
                if (eventEvaluator is FilterHandleSetNode) {
                    FilterHandleSetNode node = (FilterHandleSetNode) eventEvaluator;
                    bool isEmpty = RemoveFromNode(filterCallback, node, treePathInfo, treePathPosition);
    
                    if (isEmpty) {
                        // Since we are holding a write lock to this index, there should not be a chance that
                        // another thread had been adding anything to this FilterHandleSetNode
                        index.Remove(filterForValue);
                    }
                    int size = index.Count;
    
                    return size == 0;
                }
    
                FilterParamIndexBase nextIndex = (FilterParamIndexBase) eventEvaluator;
                EventTypeIndexBuilderIndexLookupablePair nextPair = treePathPosition < treePathInfo.Length ? treePathInfo[treePathPosition++] : null;
    
                if (nextPair == null) {
                    Log.Error(".removeFromIndex Expected an inner index to this index, this=" + filterCallback.ToString());
                    assert false;
                    return false;
                }
    
                if (nextPair.Index != nextIndex) {
                    Log.Error(".removeFromIndex Expected an index for filterCallback that differs from the found index, this=" + filterCallback.ToString() +
                            "  expected=" + nextPair.Index);
                    assert false;
                    return false;
                }
    
                Object nextExpressionValue = nextPair.Lookupable;
    
                bool isEmpty = RemoveFromIndex(filterCallback, nextPair.Index, treePathInfo, treePathPosition, nextExpressionValue);
    
                if (isEmpty) {
                    // Since we are holding a write lock to this index, there should not be a chance that
                    // another thread had been adding anything to this FilterHandleSetNode
                    index.Remove(filterForValue);
                }
                int size = index.Count;
    
                return size == 0;
            } finally {
                index.ReadWriteLock.WriteLock().Unlock();
            }
        }
    
        /// <summary>
        /// Add to an index the value to filter for.
        /// </summary>
        /// <param name="index">is the index to add to</param>
        /// <param name="filterForValue">is the filter parameter value to add</param>
        /// <param name="treePathInfo">is the specification to fill on where is was added</param>
        private static void AddToIndex(ArrayDeque<FilterValueSetParam> remainingParameters,
                                       FilterHandle filterCallback,
                                       FilterParamIndexBase index,
                                       Object filterForValue,
                                       ArrayDeque<EventTypeIndexBuilderIndexLookupablePair> treePathInfo,
                                       FilterServiceGranularLockFactory lockFactory) {
            if ((ExecutionPathDebugLog.isDebugEnabled) && (Log.IsDebugEnabled)) {
                Log.Debug(".addToIndex (" + Thread.CurrentThread().Id + ") Adding to index " +
                        index.ToString() +
                        "  expressionValue=" + filterForValue);
            }
    
            index.ReadWriteLock.ReadLock().Lock();
            EventEvaluator eventEvaluator;
            try {
                eventEvaluator = index.Get(filterForValue);
    
                // The filter parameter value already existed in bean, add and release locks
                if (eventEvaluator != null) {
                    bool added = AddToEvaluator(remainingParameters, filterCallback, eventEvaluator, treePathInfo, lockFactory);
                    if (added) {
                        return;
                    }
                }
            } finally {
                index.ReadWriteLock.ReadLock().Unlock();
            }
    
            // new filter parameter value, need a write lock
            index.ReadWriteLock.WriteLock().Lock();
            try {
                eventEvaluator = index.Get(filterForValue);
    
                // It may exist now since another thread could have added the entry
                if (eventEvaluator != null) {
                    bool added = AddToEvaluator(remainingParameters, filterCallback, eventEvaluator, treePathInfo, lockFactory);
                    if (added) {
                        return;
                    }
    
                    // The found eventEvaluator must be converted to a new FilterHandleSetNode
                    FilterParamIndexBase nextIndex = (FilterParamIndexBase) eventEvaluator;
                    var newNode = new FilterHandleSetNode(lockFactory.ObtainNew());
                    newNode.Add(nextIndex);
                    index.Remove(filterForValue);
                    index.Put(filterForValue, newNode);
                    AddToNode(remainingParameters, filterCallback, newNode, treePathInfo, lockFactory);
    
                    return;
                }
    
                // The index does not currently have this filterCallback value,
                // if there are no remaining parameters, create a node
                if (remainingParameters.IsEmpty()) {
                    var node = new FilterHandleSetNode(lockFactory.ObtainNew());
                    AddToNode(remainingParameters, filterCallback, node, treePathInfo, lockFactory);
                    index.Put(filterForValue, node);
                    return;
                }
    
                // If there are remaining parameters, create a new index for the next parameter
                FilterValueSetParam parameterPickedForIndex = remainingParameters.RemoveFirst();
    
                FilterParamIndexBase nextIndex = IndexFactory.CreateIndex(parameterPickedForIndex.Lookupable, lockFactory, parameterPickedForIndex.FilterOperator);
    
                index.Put(filterForValue, nextIndex);
                treePathInfo.Add(new EventTypeIndexBuilderIndexLookupablePair(nextIndex, parameterPickedForIndex.FilterForValue));
                AddToIndex(remainingParameters, filterCallback, nextIndex, parameterPickedForIndex.FilterForValue, treePathInfo, lockFactory);
            } finally {
                index.ReadWriteLock.WriteLock().Unlock();
            }
        }
    
        /// <summary>
        /// Add filter callback to an event evaluator, which could be either an index node or a set node.
        /// </summary>
        /// <param name="eventEvaluator">to add the filterCallback to.</param>
        /// <param name="treePathInfo">is for holding the information on where the add occured</param>
        /// <returns>
        /// bool indicating if the eventEvaluator was successfully added
        /// </returns>
        private static bool AddToEvaluator(ArrayDeque<FilterValueSetParam> remainingParameters,
                                              FilterHandle filterCallback,
                                              EventEvaluator eventEvaluator,
                                              ArrayDeque<EventTypeIndexBuilderIndexLookupablePair> treePathInfo,
                                              FilterServiceGranularLockFactory lockFactory) {
            if (eventEvaluator is FilterHandleSetNode) {
                FilterHandleSetNode node = (FilterHandleSetNode) eventEvaluator;
                AddToNode(remainingParameters, filterCallback, node, treePathInfo, lockFactory);
                return true;
            }
    
            // Check if the next index matches any of the remaining filterCallback parameters
            FilterParamIndexBase nextIndex = (FilterParamIndexBase) eventEvaluator;
    
            FilterValueSetParam parameter = IndexHelper.FindParameter(remainingParameters, nextIndex);
            if (parameter != null) {
                remainingParameters.Remove(parameter);
                treePathInfo.Add(new EventTypeIndexBuilderIndexLookupablePair(nextIndex, parameter.FilterForValue));
                AddToIndex(remainingParameters, filterCallback, nextIndex, parameter.FilterForValue, treePathInfo, lockFactory);
                return true;
            }
    
            // This eventEvaluator does not work with any of the remaining filter parameters
            return false;
        }
    
        private static string PrintRemainingParameters(ArrayDeque<FilterValueSetParam> remainingParameters) {
            var buffer = new StringBuilder();
    
            int count = 0;
            foreach (FilterValueSetParam parameter in remainingParameters) {
                buffer.Append("  Param(").Append(count).Append(')');
                buffer.Append(" property=").Append(parameter.Lookupable);
                buffer.Append(" operator=").Append(parameter.FilterOperator);
                buffer.Append(" value=").Append(parameter.FilterForValue);
                count++;
            }
    
            return Buffer.ToString();
        }
    
        private static ArrayDeque<EventTypeIndexBuilderIndexLookupablePair>[] AllocateTreePath(int size) {
            return (ArrayDeque<EventTypeIndexBuilderIndexLookupablePair>[])
                    new ArrayDeque[size];
        }
    }
} // end of namespace
