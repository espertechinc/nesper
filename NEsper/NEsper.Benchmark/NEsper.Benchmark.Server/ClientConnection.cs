///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Threading;
using com.espertech.esper.compat.collections;
using NEsper.Benchmark.Common;

using com.espertech.esper.compat;

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
        private static readonly Object CLIENT_CONNECTIONS_LOCK = new Object();

	    public static void DumpStats(int statSec)
        {
	        long totalCount = 0;
	        int cnx = 0;
	        ClientConnection any = null;

            lock (CLIENT_CONNECTIONS_LOCK)
            {
                foreach (ClientConnection m in CLIENT_CONNECTIONS.Values)
                {
                    cnx++;
                    totalCount += m.countForStatSecLast;
                    any = m;
                }
            }

	        if (any != null) {
	            Console.WriteLine("Throughput {0:F} (active {1} pending {2} cnx {3})",
	                              (float) totalCount/statSec,
	                              any.executor.ThreadCount,
	                              any.executor.QueueDepth,
	                              cnx);
	        }
	    }

        private static int ID = 0;

	    private readonly CEPProvider.ICEPProvider cepProvider;

        private readonly Executor executor;
	    private readonly int statSec;
	    private long countForStatSec = 0;
	    private long countForStatSecLast = 0;
	    private long lastThroughputTick = Environment.TickCount;
	    private readonly int myID;
        private readonly DataAssembler dataAssembler;

        /// <summary>
        /// Gets the data assembler.
        /// </summary>
        /// <value>The data assembler.</value>
        public DataAssembler DataAssembler
        {
            get { return dataAssembler; }
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
            this.myID = Interlocked.Increment(ref ID) - 1;
            this.dataAssembler = new DataAssembler();
            this.dataAssembler.MarketDataEvent += EnqueueEvent;
            this.executor = executor;
	        this.cepProvider = cepProvider;
	        this.statSec = statSec;

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
        protected void EnqueueEvent(Object sender, MarketData mdEvent)
        {
            long nsTop = PerformanceObserver.NanoTime;

            executor.Execute(
                delegate {
                    if (mdEvent != null)
                    {
                        long ns = PerformanceObserver.NanoTime;
                        cepProvider.SendEvent(mdEvent);
                        long nsDone = PerformanceObserver.NanoTime;
                        long msDelta = (nsDone - mdEvent.Time) / 1000000;
                        long nsDelta1 = nsDone - ns;
                        long nsDelta2 = nsDone - nsTop;
                        //Console.WriteLine("D1:{0,10},\tD2:{1,15},\tMS:{2,6},\tQD:{3}", nsDelta1, nsDelta2, msDelta, e.QueueDepth);
                        StatsHolder.Engine.Update(nsDelta1);
                        StatsHolder.Server.Update(nsDelta2);
                        StatsHolder.EndToEnd.Update(msDelta);
                    }
                });

            //stats
            countForStatSec++;
            if (Environment.TickCount - lastThroughputTick > statSec * 1E3)
            {
                countForStatSecLast = countForStatSec;
                countForStatSec = 0;
                lastThroughputTick = Environment.TickCount;
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
