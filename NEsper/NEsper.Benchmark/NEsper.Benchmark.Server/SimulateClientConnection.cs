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
    /// A thread started by the Server when running in simulation mode.
    /// It acts as ClientConnection
    /// </summary>
    /// <author>Alexandre Vasseur http://avasseur.blogspot.com</author>
	public class SimulateClientConnection
    {
	    private static readonly IDictionary<int, SimulateClientConnection> CLIENT_CONNECTIONS = new Dictionary<int, SimulateClientConnection>();
        private static readonly Object CLIENT_CONNECTIONS_LOCK = new Object();

	    public static void DumpStats(int statSec) {
	        long totalCount = 0;
	        int cnx = 0;
	        SimulateClientConnection any = null;
            lock (CLIENT_CONNECTIONS_LOCK)
            {
                foreach (SimulateClientConnection m in CLIENT_CONNECTIONS.Values)
                {
                    cnx++;
                    totalCount += m.countLast10sLast;
                    any = m;
                }
            }
	        if (any != null) {
	            Console.WriteLine("Throughput {0:F}", (float) totalCount/statSec);
	        }
	    }

        private int iterationsLeft = -1;
	    private readonly int simulationRate;
	    private readonly CEPProvider.ICEPProvider cepProvider;
        private readonly Thread thread;
        private readonly Executor executor;
	    private readonly int statSec;
	    private long countLast10sLast = 0;
	    private long countLast10s = 0;
	    private long lastThroughputTick;
	    private readonly int myID;
	    private static int ID = 0;

        private readonly HighResolutionTimeProvider _highResolutionTimeProvider =
                HighResolutionTimeProvider.Instance;

	    public SimulateClientConnection(int simulationRate, int iterations, Executor executor, CEPProvider.ICEPProvider cepProvider, int statSec)
        {
	        lastThroughputTick = _highResolutionTimeProvider.CurrentTime;

	        thread = new Thread(Run);
	        thread.Name = "EsperServer-cnx-" + ID++;

	        this.iterationsLeft = iterations;
	        this.simulationRate = simulationRate;
	        this.executor = executor;
	        this.cepProvider = cepProvider;
	        this.statSec = statSec;
	        myID = ID - 1;

	        // simulationRate event / s
	        // 10ms ~ simulationRate / 1E2
            lock (CLIENT_CONNECTIONS_LOCK)
            {
                CLIENT_CONNECTIONS.Put(myID, this);
            }
        }

        /// <summary>
        /// Starts this instance.
        /// </summary>
        public void Start()
        {
            thread.Start();
        }

        /// <summary>
        /// Waits for completion.
        /// </summary>
        public void WaitForCompletion()
        {
            thread.Join();
        }

	    public void Run() {
	        Console.WriteLine("Event per s = {0}", simulationRate);
	        int eventPer10Millis = (simulationRate / 100);
	        Console.WriteLine("Event per 10ms = " + Math.Max(eventPer10Millis, 1));
	        MarketData[] market = new MarketData[Symbols.SYMBOLS.Length];
	        for (int i = 0; i < market.Length; i++) {
	            market[i] = new MarketData(Symbols.SYMBOLS[i], Symbols.NextPrice(10), Symbols.NextVolume(10));
	        }

	        try {
	            int tickerIndex = 0;
                while(--iterationsLeft != 0) {
	                long ms = _highResolutionTimeProvider.CurrentTime;
	                for (int i = 0; i < eventPer10Millis; i++) {
	                    tickerIndex = tickerIndex % Symbols.SYMBOLS.Length;
	                    MarketData eventObj = market[tickerIndex++];
	                    //note the cloning here, although we don't change volume or price
	                    MarketData simulatedEvent = (MarketData) eventObj.Clone();
	                    executor.Execute(
	                        delegate
	                            {
                                    long ns = _highResolutionTimeProvider.CurrentTime;
	                                cepProvider.SendEvent(simulatedEvent);
                                    long nsDone = _highResolutionTimeProvider.CurrentTime;
	                                var statsInstance = StatsHolder.All;
                                    statsInstance.Engine.Update(nsDone - ns);
                                    statsInstance.Server.Update(nsDone - simulatedEvent.InTime);
                                    statsInstance.EndToEnd.Update((nsDone - simulatedEvent.Time) / 1000000);
	                            });
	                    //stats
	                    countLast10s++;
	                }

	                var currentTime = _highResolutionTimeProvider.CurrentTime;
	                if (currentTime - lastThroughputTick > statSec * 1E9) {
	                    //System.out.Println("Avg["+myID+"] " + countLast10s/10 + " active " + executor.GetPoolSize() + " pending " + executor.GetQueue().Count);
	                    countLast10sLast = countLast10s;
	                    countLast10s = 0;
	                    lastThroughputTick = currentTime;
	                }
	                // going to fast compared to target rate
	                var deltaTime = currentTime - ms;
	                if (deltaTime < 10000) {
	                    var slowDown = (int) Math.Max(1, 10000 - deltaTime);
	                    Thread.Sleep(slowDown);
	                }
	            }
	        } catch (Exception e) {
	            Console.WriteLine("Error receiving data from market. Did market disconnect?");
	            Console.WriteLine("Error message: {0}", e.Message);
                Console.WriteLine(e.StackTrace);
            }
            finally
	        {
                lock (CLIENT_CONNECTIONS_LOCK)
	            {
	                CLIENT_CONNECTIONS.Remove(myID);
	            }
	        }
	    }
	}
} // End of namespace
