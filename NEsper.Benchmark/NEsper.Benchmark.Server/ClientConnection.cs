///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Threading;

using NEsper.Benchmark.Common;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace NEsper.Benchmark.Server
{
    /// <summary>
    /// The ClientConnection handles unmarshalling from the connected client socket and delegates the event to
    /// the underlying ESP/CEP engine by using/or not the executor policy.
    /// Each ClientConnection manages a throughput statistic (evt/10s) over a 10s batched window
    /// </summary>
    /// <unknown>@See Server</unknown>
    /// <author>Alexandre Vasseur http://avasseur.blogspot.com</author>
	abstract public class ClientConnection 
    {
        private static readonly IDictionary<int, ClientConnection> CLIENT_CONNECTIONS = new Dictionary<int, ClientConnection>();
        private static readonly object CLIENT_CONNECTIONS_LOCK = new object();

	    public static void DumpStats(int statSec)
        {
	        long totalCount = 0;
	        var cnx = 0;
	        ClientConnection any = null;

            lock (CLIENT_CONNECTIONS_LOCK)
            {
                foreach (var m in CLIENT_CONNECTIONS.Values)
                {
                    cnx++;
                    totalCount += m._countForStatSecLast;
                    any = m;
                }
            }

	        if (any != null) {
	            Console.WriteLine("Throughput {0:F} (active {1} pending {2} cnx {3})",
	                              (float) totalCount/statSec,
	                              any._executor.ThreadCount,
	                              any._executor.QueueDepth,
	                              cnx);
	        }
	    }

        private static int _id = 0;

	    private readonly CEPProvider.ICEPProvider _cepProvider;

        private readonly Executor _executor;
	    private readonly int _statSec;
	    private long _countForStatSec = 0;
	    private long _countForStatSecLast = 0;
	    private long _lastThroughputTick = Environment.TickCount;
	    private readonly int myID;
        private readonly DataAssembler _dataAssembler;

        /// <summary>
        /// Gets the data assembler.
        /// </summary>
        /// <value>The data assembler.</value>
        public DataAssembler DataAssembler
        {
            get { return _dataAssembler; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientConnection"/> class based on
        /// the TCP socket channel.
        /// </summary>
        /// <param name="executor">The executor.</param>
        /// <param name="cepProvider">The cep provider.</param>
        /// <param name="statSec">The stat sec.</param>
        protected ClientConnection(Executor executor, CEPProvider.ICEPProvider cepProvider, int statSec)
        {
            myID = Interlocked.Increment(ref _id) - 1;
            _dataAssembler = new DataAssembler();
            _dataAssembler.MarketDataEvent += EnqueueEvent;
            _executor = executor;
	        _cepProvider = cepProvider;
	        _statSec = statSec;

            lock (CLIENT_CONNECTIONS_LOCK)
            {
                CLIENT_CONNECTIONS.Put(myID, this);
            }
        }

        /// <summary>
        /// Gets my ID.
        /// </summary>
        /// <value>My ID.</value>
        public int MyID
        {
            get { return myID; }
        }

        /// <summary>
        /// Stops this instance.
        /// </summary>
        public virtual void Stop()
        {
        }

        /// <summary>
        /// Starts this instance.
        /// </summary>
        public virtual void Start()
        {
        }

        /// <summary>
        /// Enqueues the event.
        /// </summary>
        /// <param name="mdEvent">The md event.</param>
        protected void EnqueueEvent(object sender, MarketData mdEvent)
        {
            var nsTop = PerformanceObserver.NanoTime;

            _executor.Execute(
                delegate {
                    if (mdEvent != null)
                    {
                        var ns = PerformanceObserver.NanoTime;
                        _cepProvider.SendMarketDataEvent(mdEvent);
                        var nsDone = PerformanceObserver.NanoTime;
                        var msDelta = (nsDone - mdEvent.Time) / 1000000;
                        var nsDelta1 = nsDone - ns;
                        var nsDelta2 = nsDone - nsTop;
                        //Console.WriteLine("D1:{0,10},\tD2:{1,15},\tMS:{2,6},\tQD:{3}", nsDelta1, nsDelta2, msDelta, e.QueueDepth);
                        StatsHolder.Engine.Update(nsDelta1);
                        StatsHolder.Server.Update(nsDelta2);
                        StatsHolder.EndToEnd.Update(msDelta);
                    }
                });

            //stats
            _countForStatSec++;
            if (Environment.TickCount - _lastThroughputTick > _statSec * 1E3)
            {
                _countForStatSecLast = _countForStatSec;
                _countForStatSec = 0;
                _lastThroughputTick = Environment.TickCount;
            }
        }

        /// <summary>
        /// Removes the self.
        /// </summary>
        protected void RemoveSelf()
        {
            lock (CLIENT_CONNECTIONS_LOCK)
            {
                CLIENT_CONNECTIONS.Remove(myID);
            }
        }
	}
} // End of namespace
