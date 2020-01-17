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
        private static readonly object CLIENT_CONNECTIONS_LOCK = new object();

	    public static void DumpStats(int statSec) {
	        long totalCount = 0;
	        var cnx = 0;
	        SimulateClientConnection any = null;
            lock (CLIENT_CONNECTIONS_LOCK)
            {
                foreach (var m in CLIENT_CONNECTIONS.Values)
                {
                    cnx++;
                    totalCount += m._countLast10SLast;
                    any = m;
                }
            }
	        if (any != null) {
	            Console.WriteLine("Throughput {0:F}", (float) totalCount/statSec);
	        }
	    }

        private int _iterationsLeft = -1;
	    private readonly int _simulationRate;
	    private readonly CEPProvider.ICEPProvider _cepProvider;
        private readonly Thread _thread;
        private readonly Executor _executor;
	    private readonly int _statSec;
	    private long _countLast10SLast = 0;
	    private long _countLast10S = 0;
	    private long _lastThroughputTick;
	    private readonly int _myId;
	    private static int _id = 0;

        private readonly HighResolutionTimeProvider _highResolutionTimeProvider =
                HighResolutionTimeProvider.Instance;

	    public SimulateClientConnection(int simulationRate, int iterations, Executor executor, CEPProvider.ICEPProvider cepProvider, int statSec)
        {
	        _lastThroughputTick = _highResolutionTimeProvider.CurrentTime;

	        _thread = new Thread(Run);
	        _thread.Name = "EsperServer-cnx-" + _id++;

	        _iterationsLeft = iterations;
	        _simulationRate = simulationRate;
	        _executor = executor;
	        _cepProvider = cepProvider;
	        _statSec = statSec;
	        _myId = _id - 1;

	        // simulationRate event / s
	        // 10ms ~ simulationRate / 1E2
            lock (CLIENT_CONNECTIONS_LOCK)
            {
                CLIENT_CONNECTIONS.Put(_myId, this);
            }
        }

        /// <summary>
        /// Starts this instance.
        /// </summary>
        public void Start()
        {
            _thread.Start();
        }

        /// <summary>
        /// Waits for completion.
        /// </summary>
        public void WaitForCompletion()
        {
            _thread.Join();
        }

	    public void Run() {
	        Console.WriteLine("Event per s = {0}", _simulationRate);
	        var eventPer10Millis = (_simulationRate / 100);
	        Console.WriteLine("Event per 10ms = " + Math.Max(eventPer10Millis, 1));
	        var market = new MarketData[Symbols.SYMBOLS.Length];
	        for (var i = 0; i < market.Length; i++) {
	            market[i] = new MarketData(Symbols.SYMBOLS[i], Symbols.NextPrice(10), Symbols.NextVolume(10));
	        }

	        try {
	            var tickerIndex = 0;
                while(--_iterationsLeft != 0) {
	                var ms = _highResolutionTimeProvider.CurrentTime;
	                for (var i = 0; i < eventPer10Millis; i++) {
	                    tickerIndex = tickerIndex % Symbols.SYMBOLS.Length;
	                    var eventObj = market[tickerIndex++];
	                    //note the cloning here, although we don't change volume or price
	                    var simulatedEvent = (MarketData) eventObj.Clone();
	                    _executor.Execute(
	                        delegate
	                            {
                                    var ns = _highResolutionTimeProvider.CurrentTime;
	                                _cepProvider.SendMarketDataEvent(simulatedEvent);
                                    var nsDone = _highResolutionTimeProvider.CurrentTime;
	                                var statsInstance = StatsHolder.All;
                                    statsInstance.Engine.Update(nsDone - ns);
                                    statsInstance.Server.Update(nsDone - simulatedEvent.InTime);
                                    statsInstance.EndToEnd.Update((nsDone - simulatedEvent.Time) / 1000000);
	                            });
	                    //stats
	                    _countLast10S++;
	                }

	                var currentTime = _highResolutionTimeProvider.CurrentTime;
	                if (currentTime - _lastThroughputTick > _statSec * 1E9) {
	                    //System.out.Println("Avg["+myID+"] " + countLast10s/10 + " active " + executor.GetPoolSize() + " pending " + executor.GetQueue().Count);
	                    _countLast10SLast = _countLast10S;
	                    _countLast10S = 0;
	                    _lastThroughputTick = currentTime;
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
	                CLIENT_CONNECTIONS.Remove(_myId);
	            }
	        }
	    }
	}
} // End of namespace
