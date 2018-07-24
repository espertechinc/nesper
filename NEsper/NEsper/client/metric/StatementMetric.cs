///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;

namespace com.espertech.esper.client.metric
{
    /// <summary>
    /// Reports statement-level instrumentation values.
    /// </summary>
    public class StatementMetric : MetricEvent
    {
#if DEBUG_STATEMENT_METRIC
        private readonly Guid _id = Guid.NewGuid();
#endif
        /// <summary>
        /// Gets or sets engine timestamp.
        /// </summary>
        public long Timestamp { get; set; }

        /// <summary>Returns statement name. </summary>
        /// <returns>statement name</returns>
        public String StatementName { get; private set; }

        private long _cpuTime;
        private long _wallTime;
        private long _numOutputRStream;
        private long _numOutputIStream;
        private long _numInput;


        /// <summary>Ctor. </summary>
        /// <param name="engineURI">engine URI</param>
        /// <param name="statementName">statement name</param>
        public StatementMetric(String engineURI, String statementName)
            : base(engineURI)
        {
            StatementName = statementName;
            _wallTime = 0L;
            _cpuTime = 0L;
            _numOutputIStream = 0L;
            _numOutputRStream = 0L;
            _numInput = 0L;
        }

        public double CpuTime => Interlocked.Read(ref _cpuTime);

        public double WallTime => Interlocked.Read(ref _wallTime);

        /// <summary>Returns number of output rows in remove stream. </summary>
        /// <returns>number of output rows in remove stream</returns>
        public long OutputRStreamCount => Interlocked.Read(ref _numOutputRStream);

        /// <summary>Returns number of output rows in insert stream. </summary>
        /// <returns>number of output rows in insert stream</returns>
        public long OutputIStreamCount => Interlocked.Read(ref _numOutputIStream);

        /// <summary>Adds number of output rows in insert stream. </summary>
        /// <param name="numIStream">to add</param>
        public void AddNumOutputIStream(int numIStream)
        {
            Interlocked.Add(ref _numOutputIStream, numIStream);
#if DEBUG_STATEMENT_METRIC
            Debug.WriteLine("{0}: Reporting num output istream {1}, {2}", _id, outputIStreamCount, numIStream);
#endif
        }

        /// <summary>Adds number of output rows in remove stream. </summary>
        /// <param name="numRStream">to add</param>
        public void AddNumOutputRStream(int numRStream)
        {
            Interlocked.Add(ref _numOutputRStream, numRStream);
#if DEBUG_STATEMENT_METRIC
            Debug.WriteLine("{0}: Reporting num output rstream {1}, {2}", _id, outputRStreamCount, numRStream);
#endif
        }

        public void IncrementTime(long pCpuTime, long pWallTime)
        {
            Interlocked.Add(ref _cpuTime, pCpuTime);
            Interlocked.Add(ref _wallTime, pWallTime);
#if DEBUG_STATEMENT_METRIC
            Debug.WriteLine("{0}: Reporting time {1}/{2}, {3}/{4}", _id, pCpuTime, cpuTime, pWallTime, wallTime);
#endif
        }

        public long NumInput => Interlocked.Read(ref _numInput);

        public void AddNumInput(long numInputAdd)
        {
            Interlocked.Add(ref _numInput, numInputAdd);
        }
    }
}
