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
using com.espertech.esper.compat.threading;

namespace com.espertech.esper.filter
{
	/// <summary>
	/// Each implementation of this abstract class represents an index of filter parameter constants supplied in filter
	/// parameters in filter specifications that feature the same event property and operator.
	/// <p>
	/// For example, a filter with a parameter of "count EQUALS 10" would be represented as index
	/// for a property named "count" and for a filter operator typed "EQUALS". The index
	/// would store a value of "10" in its internal structure.
	/// </p>
	/// <p>
	/// Implementations make sure that the type of the Object constant in get and put calls matches the event property type.
	/// </p>
	/// </summary>
	public abstract class FilterParamIndexBase : EventEvaluator
	{
	    private readonly FilterOperator _filterOperator;

	    /// <summary>Constructor.</summary>
	    /// <param name="filterOperator">is the type of comparison performed.</param>
	    protected FilterParamIndexBase(FilterOperator filterOperator)
	    {
	        _filterOperator = filterOperator;
	    }

	    /// <summary>
	    /// Get the event evaluation instance associated with the constant. Returns null if no entry found for the constant.
	    /// The calling class must make sure that access to the underlying resource is protected
	    /// for multi-threaded access, the ReadWriteLock property must supply a lock for this purpose.
	    /// 
	    /// Store the event evaluation instance for the given constant. Can override an existing value
	    /// for the same constant.
	    /// The calling class must make sure that access to the underlying resource is protected
	    /// for multi-threaded access, the ReadWriteLock property must supply a lock for this purpose.
	    /// </summary>
	    /// <param name="filterConstant">
	    /// is the constant supplied in the event filter parameter
	    /// </param>
	    /// <returns>
	    /// event evaluator stored for the filter constant, or null if not found
	    /// </returns>
	    public abstract EventEvaluator this[Object filterConstant] { get; set; }

	    /// <summary>
	    /// Remove the event evaluation instance for the given constant. Returns true if
	    /// the constant was found, or false if not.
	    /// The calling class must make sure that access to the underlying resource is protected
	    /// for multi-threaded writes, the ReadWriteLock property must supply a lock for this purpose.
	    /// </summary>
	    /// <param name="filterConstant">is the value supplied in the filter paremeter</param>
	    public abstract void Remove(Object filterConstant);

        /// <summary>
        /// Return the number of distinct filter parameter constants stored, which can be an expensive call.
        /// The calling class must make sure that access to the underlying resource is protected
        /// for multi-threaded writes, the ReadWriteLock property must supply a lock for this purpose.
        /// </summary>
        /// <returns>Number of entries in index</returns>
        public abstract int Count { get ; }

        /// <summary>
        /// Return empty indicator.
        /// The calling class must make sure that access to the underlying resource is protected
        /// for multi-threaded writes, the ReadWriteLock method must supply a lock for this purpose.
        /// </summary>
        public abstract bool IsEmpty { get; }

        /// <summary>Supplies the lock for protected access.</summary>
        /// <returns>lock</returns>
        public abstract IReaderWriterLock ReadWriteLock { get; }

	    /// <summary>Returns the filter operator that the index matches for.</summary>
	    /// <returns>filter operator</returns>
	    public FilterOperator FilterOperator
	    {
            get { return _filterOperator; }
	    }

        /// <summary>
        /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </returns>
	    public override String ToString()
	    {
	        return "filterOperator=" + _filterOperator;
	    }

        /// <summary>
        /// Perform the matching of an event based on the event property values, adding any callbacks for matches found to the matches list.
        /// </summary>
        /// <param name="theTheEvent">is the event object wrapper to obtain event property values from</param>
        /// <param name="matches">accumulates the matching filter callbacks</param>
	    public abstract void MatchEvent(EventBean theTheEvent, ICollection<FilterHandle> matches);
	}
} // End of namespace
