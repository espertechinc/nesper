///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.filtersvc;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.threading.locks;

namespace com.espertech.esper.runtime.@internal.filtersvcimpl
{
    /// <summary>
    /// Each implementation of this abstract class represents an index of filter parameter constants supplied in filter
    /// parameters in filter specifications that feature the same event property and operator.
    /// <para />For example, a filter with a parameter of "count EQUALS 10" would be represented as index
    /// for a property named "count" and for a filter operator typed "EQUALS". The index
    /// would store a value of "10" in its internal structure.
    /// <para />Implementations make sure that the type of the Object constant in get and put calls matches the event property type.
    /// </summary>
    public abstract class FilterParamIndexBase : EventEvaluator, FilterHandleSize
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="filterOperator">is the type of comparison performed.</param>
        public FilterParamIndexBase(FilterOperator filterOperator)
        {
            this.FilterOperator = filterOperator;
        }

        /// <summary>
        /// Get the event evaluation instance associated with the constant. Returns null if no entry found for the constant.
        /// The calling class must make sure that access to the underlying resource is protected
        /// for multi-threaded access, the getReadWriteLock() method must supply a lock for this purpose.
        /// </summary>
        /// <param name="filterConstant">is the constant supplied in the event filter parameter</param>
        /// <returns>event evaluator stored for the filter constant, or null if not found</returns>
        public abstract EventEvaluator Get(object filterConstant);

        /// <summary>
        /// Store the event evaluation instance for the given constant. Can override an existing value
        /// for the same constant.
        /// The calling class must make sure that access to the underlying resource is protected
        /// for multi-threaded access, the getReadWriteLock() method must supply a lock for this purpose.
        /// </summary>
        /// <param name="filterConstant">is the constant supplied in the filter parameter</param>
        /// <param name="evaluator">to be stored for the constant</param>
        public abstract void Put(object filterConstant, EventEvaluator evaluator);

        /// <summary>
        /// Remove the event evaluation instance for the given constant. Returns true if
        /// the constant was found, or false if not.
        /// The calling class must make sure that access to the underlying resource is protected
        /// for multi-threaded writes, the getReadWriteLock() method must supply a lock for this purpose.
        /// </summary>
        /// <param name="filterConstant">is the value supplied in the filter paremeter</param>
        public abstract void Remove(object filterConstant);

        /// <summary>
        /// Return the number of distinct filter parameter constants stored, which can be an expensive call.
        /// The calling class must make sure that access to the underlying resource is protected
        /// for multi-threaded writes, the getReadWriteLock() method must supply a lock for this purpose.
        /// </summary>
        /// <value>Number of entries in index</value>
        public abstract int CountExpensive { get; }

        /// <summary>
        /// Return empty indicator.
        /// The calling class must make sure that access to the underlying resource is protected
        /// for multi-threaded writes, the getReadWriteLock() method must supply a lock for this purpose.
        /// </summary>
        /// <value>empty flag</value>
        public abstract bool IsEmpty { get; }

        /// <summary>
	    /// Supplies the lock for protected access.
	    /// </summary>
	    /// <returns>lock</returns>
	    public abstract IReaderWriterLock ReadWriteLock { get; }

        /// <summary>
        /// Returns the filter operator that the index matches for.
        /// </summary>
        /// <returns>filter operator</returns>
        public FilterOperator FilterOperator { get; }

        public int FilterCallbackCount
        {
            get => CountExpensive;
        }

        public override string ToString()
        {
            return "filterOperator=" + FilterOperator;
        }

        public abstract void MatchEvent(
            EventBean theEvent,
            ICollection<FilterHandle> matches,
            ExprEvaluatorContext ctx);

        public abstract void GetTraverseStatement(
            EventTypeIndexTraverse traverse,
            ICollection<int> statementIds,
            ArrayDeque<FilterItem> evaluatorStack);
    }
} // end of namespace