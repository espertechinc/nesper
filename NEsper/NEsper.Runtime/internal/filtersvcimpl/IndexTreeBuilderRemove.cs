///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Reflection;
using System.Threading;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.filtersvc;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.runtime.@internal.filtersvcimpl
{
    public sealed class IndexTreeBuilderRemove
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private IndexTreeBuilderRemove()
        {
        }

        /// <summary>
        ///     Remove an filterCallback from the given top node. The IndexTreePath instance passed in must be the
        ///     same as obtained when the same filterCallback was added.
        /// </summary>
        /// <param name="filterCallback">filter callback  to be removed</param>
        /// <param name="topNode">The top tree node beneath which the filterCallback was added</param>
        /// <param name="eventType">event type</param>
        /// <param name="params">params</param>
        public static void Remove(
            EventType eventType,
            FilterHandle filterCallback,
            FilterValueSetParam[] @params,
            FilterHandleSetNode topNode)
        {
            if (ExecutionPathDebugLog.IsDebugEnabled && Log.IsDebugEnabled) {
                Log.Debug(
                    ".remove (" + Thread.CurrentThread.ManagedThreadId + ") Removing filterCallback " +
                    " type " + eventType.Name +
                    " topNode=" + topNode +
                    " filterCallback=" + filterCallback);
            }

            var isRemoved = RemoveFromNode(filterCallback, topNode, @params, 0);

            if (!isRemoved) {
                Log.Warn(
                    ".removeFromNode (" + Thread.CurrentThread.ManagedThreadId +
                    ") Could not find the filterCallback to be removed within the supplied node, params=" +
                    @params.RenderAny() + "  filterCallback=" + filterCallback);
            }
        }

        private static bool RemoveFromNode(
            FilterHandle filterCallback,
            FilterHandleSetNode currentNode,
            FilterValueSetParam[] @params,
            int currentLevel)
        {
            // No remaining filter parameters
            if (currentLevel == @params.Length) {
                using (currentNode.NodeRWLock.WriteLock.Acquire())
                {
                    return currentNode.Remove(filterCallback);
                }
            }

            if (currentLevel > @params.Length) {
                Log.Warn(
                    ".removeFromNode (" + Thread.CurrentThread.ManagedThreadId + ") Current level exceed parameter length, node=" + currentNode +
                    "  filterCallback=" + filterCallback);
                return false;
            }

            // Remove from index
            using (currentNode.NodeRWLock.WriteLock.Acquire())
            {
                FilterParamIndexBase indexFound = null;

                // find matching index
                foreach (var index in currentNode.Indizes)
                {
                    for (var i = 0; i < @params.Length; i++)
                    {
                        var param = @params[i];
                        // if property-based index, we prefer this in matching
                        if (index is FilterParamIndexLookupableBase)
                        {
                            var baseIndex = (FilterParamIndexLookupableBase) index;
                            if (param.Lookupable.Expression.Equals(baseIndex.Lookupable.Expression) &&
                                param.FilterOperator.Equals(baseIndex.FilterOperator))
                            {
                                var found = RemoveFromIndex(filterCallback, index, @params, currentLevel + 1, param.FilterForValue);
                                if (found)
                                {
                                    indexFound = baseIndex;
                                    break;
                                }
                            }
                        }
                        else if (index is FilterParamIndexBooleanExpr && currentLevel == @params.Length - 1)
                        {
                            // if boolean-expression then match only if this is the last parameter,
                            // all others considered are higher order and sort ahead
                            if (param.FilterOperator.Equals(FilterOperator.BOOLEAN_EXPRESSION))
                            {
                                var booleanIndex = (FilterParamIndexBooleanExpr) index;
                                bool found = booleanIndex.RemoveMayNotExist(param.FilterForValue);
                                if (found)
                                {
                                    indexFound = booleanIndex;
                                    break;
                                }
                            }
                        }
                    }

                    if (indexFound != null)
                    {
                        break;
                    }
                }

                if (indexFound == null)
                {
                    return false;
                }

                // Remove the index if the index is now empty
                if (indexFound.IsEmpty)
                {
                    var isRemoved = currentNode.Remove(indexFound);

                    if (!isRemoved)
                    {
                        Log.Warn(
                            ".removeFromNode (" + Thread.CurrentThread.ManagedThreadId + ") Could not find the index in index list for removal, index=" +
                            indexFound + "  filterCallback=" + filterCallback);
                        return true;
                    }
                }

                return true;
            }
        }

        private static bool RemoveFromIndex(
            FilterHandle filterCallback,
            FilterParamIndexBase index,
            FilterValueSetParam[] @params,
            int currentLevel,
            object filterForValue)
        {
            using (index.ReadWriteLock.WriteLock.Acquire())
            {
                EventEvaluator eventEvaluator = index.Get(filterForValue);

                if (eventEvaluator == null)
                {
                    // This is possible as there can be another path
                    return false;
                }

                if (eventEvaluator is FilterHandleSetNode)
                {
                    var node = (FilterHandleSetNode) eventEvaluator;
                    var found = RemoveFromNode(filterCallback, node, @params, currentLevel);
                    if (!found)
                    {
                        return false;
                    }

                    var isEmpty = node.IsEmpty();
                    if (isEmpty)
                    {
                        // Since we are holding a write lock to this index, there should not be a chance that
                        // another thread had been adding anything to this FilterHandleSetNode
                        index.Remove(filterForValue);
                    }

                    return true;
                }

                var nextIndex = (FilterParamIndexBase) eventEvaluator;
                FilterParamIndexBase indexFound = null;
                if (nextIndex is FilterParamIndexLookupableBase)
                {
                    var baseIndex = (FilterParamIndexLookupableBase) nextIndex;
                    for (var i = 0; i < @params.Length; i++)
                    {
                        var param = @params[i];
                        if (param.Lookupable.Expression.Equals(baseIndex.Lookupable.Expression) &&
                            param.FilterOperator.Equals(baseIndex.FilterOperator))
                        {
                            var found = RemoveFromIndex(filterCallback, baseIndex, @params, currentLevel + 1, param.FilterForValue);
                            if (found)
                            {
                                indexFound = baseIndex;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    var booleanIndex = (FilterParamIndexBooleanExpr) nextIndex;
                    for (var i = 0; i < @params.Length; i++)
                    {
                        var param = @params[i];
                        // if boolean-expression then match only if this is the last parameter,
                        // all others considered are higher order and sort ahead
                        if (param.FilterOperator.Equals(FilterOperator.BOOLEAN_EXPRESSION))
                        {
                            bool found = booleanIndex.RemoveMayNotExist(param.FilterForValue);
                            if (found)
                            {
                                indexFound = booleanIndex;
                                break;
                            }
                        }
                    }
                }

                if (indexFound == null)
                {
                    return false;
                }

                var indexIsEmpty = nextIndex.IsEmpty;
                if (indexIsEmpty)
                {
                    // Since we are holding a write lock to this index, there should not be a chance that
                    // another thread had been adding anything to this FilterHandleSetNode
                    index.Remove(filterForValue);
                }

                return true;
            }
        }
    }
} // end of namespace