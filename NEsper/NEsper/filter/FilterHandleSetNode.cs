///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.threading;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.filter
{
    /// <summary>
    /// This class holds a list of indizes storing filter constants in <seealso cref="FilterParamIndexBase"/> 
    /// nodes and a set of <seealso cref="FilterHandle"/>. An instance of this class represents a leaf-node 
    /// (no indizes stored, just filter callbacks) but can also be non-leaf (some indizes exist) in a filter 
    /// evaluation tree. Events are evaluated by asking each of the indizes to evaluate the event and by adding 
    /// any filter callbacks in this node to the "matches" list of callbacks.
    /// </summary>
    public sealed class FilterHandleSetNode : EventEvaluator
    {
        private readonly LinkedHashSet<FilterHandle> _callbackSet;
        private readonly List<FilterParamIndexBase> _indizes;
        private readonly IReaderWriterLock _nodeRwLock;

        /// <summary>Constructor. </summary>
        public FilterHandleSetNode(IReaderWriterLock readWriteLock)
        {
            _callbackSet = new LinkedHashSet<FilterHandle>();
            _indizes = new List<FilterParamIndexBase>();
            _nodeRwLock = readWriteLock;
        }

        /// <summary>
        /// Returns an indication of whether there are any callbacks or index nodes at all in this set. 
        /// NOTE: the client to this method must use the read-write lock of this object to lock, if 
        /// required by the client code.
        /// </summary>
        /// <returns>
        /// true if there are neither indizes nor filter callbacks stored, false if either exist.
        /// </returns>
        internal bool IsEmpty()
        {
            return _callbackSet.IsEmpty() && _indizes.Count == 0;
        }

        /// <summary>
        /// Returns the number of filter callbacks stored. NOTE: the client to this method must use 
        /// the read-write lock of this object to lock, if required by the client code.
        /// </summary>
        /// <value>number of filter callbacks stored</value>
        internal int FilterCallbackCount
        {
            get { return _callbackSet.Count; }
        }

        /// <summary>
        /// Returns to lock to use for making changes to the filter callback or inzides collections stored by this node.
        /// </summary>
        /// <value>lock to use in multithreaded environment</value>
        internal IReaderWriterLock NodeRWLock
        {
            get { return _nodeRwLock; }
        }

        /// <summary>Returns list of indexes - not returning an iterator. Client classes should not change this collection. </summary>
        /// <value>list of indizes</value>
        public IList<FilterParamIndexBase> Indizes
        {
            get { return _indizes; }
        }

        /// <summary>Evaluate an event by asking each index to match the event. Any filter callbacks at this node automatically match the event and do not need to be further evaluated, and are thus added to the "matches" list of callbacks. NOTE: This client should not use the lock before calling this method. </summary>
        /// <param name="theEvent">is the event wrapper supplying the event property values</param>
        /// <param name="matches">is the list of callbacks to add to for any matches found</param>
        public void MatchEvent(EventBean theEvent, ICollection<FilterHandle> matches)
        {
            using(_nodeRwLock.ReadLock.Acquire())
            {
                if (InstrumentationHelper.ENABLED)
                {
                    if (_indizes.IsNotEmpty())
                    {
                        InstrumentationHelper.Get().QFilterHandleSetIndexes(_indizes);
                    }
                }

                // Ask each of the indizes to match against the attribute values
                var length = _indizes.Count;
                for(int ii = 0 ; ii < length ; ii++)
                {
                    _indizes[ii].MatchEvent(theEvent, matches);
                }

                if (InstrumentationHelper.ENABLED)
                {
                    if (_indizes.IsNotEmpty())
                    {
                        InstrumentationHelper.Get().AFilterHandleSetIndexes();
                    }
                }

                if (InstrumentationHelper.ENABLED)
                {
                    if (_callbackSet.IsNotEmpty())
                    {
                        InstrumentationHelper.Get().QaFilterHandleSetCallbacks(_callbackSet);
                    }
                }

                // Add each filter callback stored in this node to the matching list
                _callbackSet.ForEach(matches.Add);

                //foreach (FilterHandle filterCallback in _callbackSet)
                //{
                //    matches.Add(filterCallback);
                //}
            }
        }

        /// <summary>
        /// Returns an indication whether the filter callback exists in this node.
        /// NOTE: the client to this method must use the read-write lock of this object 
        /// to lock, if required by the client code.
        /// </summary>
        /// <param name="filterCallback">is the filter callback to check for</param>
        /// <returns>true if callback found, false if not</returns>
        internal bool Contains(FilterHandle filterCallback)
        {
            return _callbackSet.Contains(filterCallback);
        }

        /// <summary>
        /// Add an index. The same index can be added twice - there is no checking done.
        /// NOTE: the client to this method must use the read-write lock of this object
        /// to lock, if required by the client code.
        /// </summary>
        /// <param name="index">index to add</param>
        internal void Add(FilterParamIndexBase index)
        {
            _indizes.Add(index);
        }

        /// <summary>
        /// Remove an index, returning true if it was found and removed or false if not in collection. 
        /// NOTE: the client to this method must use the read-write lock of this object to lock, if 
        /// required by the client code.
        /// </summary>
        /// <param name="index">is the index to remove</param>
        /// <returns>true if found, false if not existing</returns>
        internal bool Remove(FilterParamIndexBase index)
        {
            return _indizes.Remove(index);
        }

        /// <summary>
        /// Add a filter callback. The filter callback set allows adding the same callback twice with no effect. 
        /// If a client to the class needs to check that the callback already existed, the contains method does that. 
        /// NOTE: the client to this method must use the read-write lock of this object to lock, if required 
        /// by the client code.
        /// </summary>
        /// <param name="filterCallback">is the callback to add</param>
        internal void Add(FilterHandle filterCallback)
        {
            _callbackSet.Add(filterCallback);
        }

        /// <summary>
        /// Remove a filter callback, returning true if it was found and removed or false if not in collection.
        /// NOTE: the client to this method must use the read-write lock of this object to lock, if required by the client code.
        /// </summary>
        /// <param name="filterCallback">is the callback to remove</param>
        /// <returns>true if found, false if not existing</returns>
        internal bool Remove(FilterHandle filterCallback)
        {
            return _callbackSet.Remove(filterCallback);
        }

        /// <summary>
        /// Gets the callback set.
        /// </summary>
        /// <value></value>
        public ICollection<FilterHandle> CallbackSet
        {
            get { return _callbackSet; }
        }
    }
}
