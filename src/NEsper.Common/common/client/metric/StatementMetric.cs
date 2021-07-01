///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Threading;

using com.espertech.esper.compat.diagnostics;

namespace com.espertech.esper.common.client.metric
{
    /// <summary>
    ///     Reports statement-level instrumentation values.
    /// </summary>
    public class StatementMetric : MetricEvent
    {
        private PerformanceMetrics _performanceMetrics;
        private long _numOutputRStream;
        private long _numOutputIStream;
        private long _numInput;

        /// <summary>
        /// Initializes a new instance of the <see cref="StatementMetric"/> class.
        /// </summary>
        /// <param name="runtimeURI">The runtime URI.</param>
        /// <param name="deploymentId">The deployment identifier.</param>
        /// <param name="statementName">Name of the statement.</param>
        public StatementMetric(
            string runtimeURI,
            string deploymentId,
            string statementName)
            : base(runtimeURI)
        {
            DeploymentId = deploymentId;
            StatementName = statementName;
            _performanceMetrics = default(PerformanceMetrics);
            _numOutputIStream = 0L;
            _numOutputRStream = 0L;
            _numInput = 0L;
        }

        public string DeploymentId { get; }

        /// <summary>
        ///     Gets or sets engine timestamp.
        /// </summary>
        public long Timestamp { get; set; }

        /// <summary>Returns statement name. </summary>
        /// <returns>statement name</returns>
        public string StatementName { get; }

        /// <summary>
        /// Returns the performance metrics.
        /// </summary>
        public PerformanceMetrics PerformanceMetrics => _performanceMetrics;

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
        }

        /// <summary>Adds number of output rows in remove stream. </summary>
        /// <param name="numRStream">to add</param>
        public void AddNumOutputRStream(int numRStream)
        {
            Interlocked.Add(ref _numOutputRStream, numRStream);
        }

        public void AddMetrics(PerformanceMetrics performanceMetrics)
        {
            lock (this) {
                _performanceMetrics.UserTime += performanceMetrics.UserTime;
                _performanceMetrics.PrivTime += performanceMetrics.PrivTime;
                _performanceMetrics.TotalTime += performanceMetrics.TotalTime;
            }
        }

        public long NumInput => Interlocked.Read(ref _numInput);

        public void AddNumInput(long numInputAdd)
        {
            Interlocked.Add(ref _numInput, numInputAdd);
        }
    }
}