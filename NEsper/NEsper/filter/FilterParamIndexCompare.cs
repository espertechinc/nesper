///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.filter
{
    /// <summary>
    /// MapIndex for filter parameter constants for the comparison operators (less, greater, etc). 
    /// The implementation is based on the SortedMap implementation of TreeMap. The index only 
    /// accepts numeric constants. It keeps a lower and upper bounds of all constants in the 
    /// index for fast range checking, since the assumption is that frequently values fall 
    /// within a range.
    /// </summary>
    public sealed class FilterParamIndexCompare : FilterParamIndexLookupableBase
    {
        private readonly OrderedDictionary<Object, EventEvaluator> _constantsMap;
        private readonly IReaderWriterLock _constantsMapRwLock;

        private double? _lowerBounds;
        private double? _upperBounds;

        public FilterParamIndexCompare(FilterSpecLookupable lookupable, IReaderWriterLock readWriteLock, FilterOperator filterOperator)
            : base(filterOperator, lookupable)
        {
            _constantsMap = new OrderedDictionary<Object, EventEvaluator>();
            _constantsMapRwLock = readWriteLock;

            if ((filterOperator != FilterOperator.GREATER) &&
                (filterOperator != FilterOperator.GREATER_OR_EQUAL) &&
                (filterOperator != FilterOperator.LESS) &&
                (filterOperator != FilterOperator.LESS_OR_EQUAL))
            {
                throw new ArgumentException("Invalid filter operator for index of " + filterOperator);
            }
        }

        public override EventEvaluator Get(Object filterConstant)
        {
            return _constantsMap.Get(filterConstant);
        }

        public override void Put(Object filterConstant, EventEvaluator matcher)
        {
            _constantsMap.Put(filterConstant, matcher);

            // Update bounds
            var constant = filterConstant.AsDouble();
            if ((_lowerBounds == null) || (constant < _lowerBounds))
            {
                _lowerBounds = constant;
            }
            if ((_upperBounds == null) || (constant > _upperBounds))
            {
                _upperBounds = constant;
            }
        }

        public override void Remove(Object filterConstant)
        {
            if (_constantsMap.Delete(filterConstant) != null)
            {
                UpdateBounds();
            }
        }

        public override int Count
        {
            get { return _constantsMap.Count; }
        }

        public override bool IsEmpty
        {
            get { return _constantsMap.IsEmpty(); }
        }

        public override IReaderWriterLock ReadWriteLock
        {
            get { return _constantsMapRwLock; }
        }

        public override void MatchEvent(EventBean theEvent, ICollection<FilterHandle> matches)
        {
            var propertyValue = Lookupable.Getter.Get(theEvent);
            var returnValue = new Mutable<bool?>(false);

            using (Instrument.With(
                i => i.QFilterReverseIndex(this, propertyValue),
                i => i.AFilterReverseIndex(returnValue.Value)))
            {
                if (propertyValue == null)
                {
                    return;
                }

                // A undefine lower bound indicates an empty index
                if (_lowerBounds == null)
                {
                    return;
                }

                var filterOperator = FilterOperator;
                var propertyValueDouble = propertyValue.AsDouble();

                // Based on current lower and upper bounds check if the property value falls outside - shortcut submap generation
                if ((filterOperator == FilterOperator.GREATER) && (propertyValueDouble <= _lowerBounds))
                {
                    return;
                }
                else if ((filterOperator == FilterOperator.GREATER_OR_EQUAL) && (propertyValueDouble < _lowerBounds))
                {
                    return;
                }
                else if ((filterOperator == FilterOperator.LESS) && (propertyValueDouble >= _upperBounds))
                {
                    return;
                }
                else if ((filterOperator == FilterOperator.LESS_OR_EQUAL) && (propertyValueDouble > _upperBounds))
                {
                    return;
                }

                // Look up in table
                using (_constantsMapRwLock.AcquireReadLock())
                {
                    // Get the head or tail end of the map depending on comparison type
                    IDictionary<Object, EventEvaluator> subMap;

                    if ((filterOperator == FilterOperator.GREATER) ||
                        (filterOperator == FilterOperator.GREATER_OR_EQUAL))
                    {
                        // At the head of the map are those with a lower numeric constants
                        subMap = _constantsMap.Head(propertyValue);
                    }
                    else
                    {
                        subMap = _constantsMap.Tail(propertyValue);
                    }

                    // All entries in the subMap are elgibile, with an exception
                    EventEvaluator exactEquals = null;
                    if (filterOperator == FilterOperator.LESS)
                    {
                        exactEquals = _constantsMap.Get(propertyValue);
                    }

                    foreach (EventEvaluator matcher in subMap.Values)
                    {
                        // For the LESS comparison type we ignore the exactly equal case
                        // The subMap is sorted ascending, thus the exactly equals case is the first
                        if (exactEquals != null)
                        {
                            exactEquals = null;
                            continue;
                        }

                        matcher.MatchEvent(theEvent, matches);
                    }

                    if (filterOperator == FilterOperator.GREATER_OR_EQUAL)
                    {
                        EventEvaluator matcher = _constantsMap.Get(propertyValue);
                        if (matcher != null)
                        {
                            matcher.MatchEvent(theEvent, matches);
                        }
                    }
                }

                returnValue.Value = null;
            }
        }

        private void UpdateBounds()
        {
            if (_constantsMap.IsEmpty())
            {
                _lowerBounds = null;
                _upperBounds = null;
                return;
            }
            _lowerBounds = (_constantsMap.Keys.First()).AsDouble();
            _upperBounds = (_constantsMap.Keys.Last()).AsDouble();
        }

        private static readonly ILog log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
