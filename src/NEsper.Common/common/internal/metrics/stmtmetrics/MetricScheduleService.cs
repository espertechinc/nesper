///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.metrics.stmtmetrics
{
    /// <summary>
    ///     Scheduling for metrics execution is handled by this service.
    /// </summary>
    public sealed class MetricScheduleService : MetricTimeSource
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IOrderedDictionary<long, IList<MetricExec>> _timeHandleMap;

        // Current time - used for evaluation as well as for adding new handles
        private long? _currentTime;

        /// <summary>Constructor. </summary>
        public MetricScheduleService()
        {
            _timeHandleMap = new OrderedListDictionary<long, IList<MetricExec>>();
        }

        /// <summary>Returns nearest scheduled time. </summary>
        /// <returns>nearest scheduled time, or null if none/empty schedule.</returns>
        public long? NearestTime { get; private set; }

        /// <summary>
        ///     Gets or sets the current time.
        /// </summary>
        /// <value>The current time.</value>
        public long CurrentTime {
            get => _currentTime.GetValueOrDefault();
            set {
                lock (this) {
                    _currentTime = value;
                }
            }
        }

        /// <summary>Clears the schedule. </summary>
        public void Clear()
        {
            Log.Debug("Clearing scheduling service");
            _timeHandleMap.Clear();
            NearestTime = null;
        }

        /// <summary>Adds an execution to the schedule. </summary>
        /// <param name="afterMSec">offset to add at</param>
        /// <param name="execution">execution to add</param>
        public void Add(
            long afterMSec,
            MetricExec execution)
        {
            lock (this) {
                if (execution == null) {
                    throw new ArgumentException("Unexpected parameters : null execution");
                }

                var triggerOnTime = _currentTime.GetValueOrDefault() + afterMSec;
                var handleSet = _timeHandleMap.Get(triggerOnTime);
                if (handleSet == null) {
                    handleSet = new List<MetricExec>();
                    _timeHandleMap.Put(triggerOnTime, handleSet);
                }

                handleSet.Add(execution);

                NearestTime = _timeHandleMap.Keys.First();
            }
        }

        /// <summary>Evaluate the schedule and populates executions, if any. </summary>
        /// <param name="handles">to populate</param>
        public void Evaluate(ICollection<MetricExec> handles)
        {
            lock (this) {
                var current = _currentTime.GetValueOrDefault() + 1;
                var headMap = _timeHandleMap.Where(keyValuePair => keyValuePair.Key < current);

                // First determine all triggers to shoot
                var removeKeys = new List<long>();
                foreach (var entry in headMap) {
                    var key = entry.Key;
                    var value = entry.Value;
                    removeKeys.Add(key);
                    foreach (var handle in value) {
                        handles.Add(handle);
                    }
                }

                // Remove all triggered msec values
                foreach (var key in removeKeys) {
                    _timeHandleMap.Remove(key);
                }

                if (_timeHandleMap.IsNotEmpty()) {
                    NearestTime = _timeHandleMap.Keys.First();
                }
                else {
                    NearestTime = null;
                }
            }
        }

        /// <summary>Remove from schedule an execution. </summary>
        /// <param name="metricExec">to remove</param>
        public void Remove(MetricExec metricExec)
        {
            foreach (var entry in _timeHandleMap) {
                entry.Value.Remove(metricExec);
            }
        }
    }
}