///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;
using System.Threading;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.configuration;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compiler.client;
using com.espertech.esper.container;
using com.espertech.esper.runtime.client;

namespace NEsper.Benchmark.Perforator
{
    public class PerforatorMain
    {
        private static IContainer _container;

        const int ThreadCount = 1;

        static void Main()
        {
            LoggerNLog.BasicConfig();
            LoggerNLog.Register();

            Process.GetCurrentProcess().PriorityBoostEnabled = true;
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.RealTime;

            SetupEsper();

#if false
            PropertyAccess.MeasureNative();
            PropertyAccess.MeasureNativeLambda();
            PropertyAccess.MeasureMagic();

            ThreadLocal.MeasureXperThreadLocal();
            ThreadLocal.MeasureSlimThreadLocal();
            ThreadLocal.MeasureFastThreadLocal();
            ThreadLocal.MeasureSystemThreadLocal();
#endif

            //PerformanceTestIdentNode();
            PerformanceTestQuery();
        }

        private static void PerformanceTestQuery()
        {
            var statementArray = new[]
            {
                TextWinLengthWithGroup,
                TextWinLength,
                TextAggregationWithTimeWindow,
                TextAggregationWithNoWindow,
                TextWithPropertyNames,
                TextWithWildcard,
            };

            var statementActions = new Runnable[]
            {
                TestWinLengthWithGroup,
                TestWinLength,
                TestAggregationWithTimeWindow,
                TestAggregationWithNoWindow,
                TestWithPropertyNames,
                TestWithWildcard,
            };

            for (var nn = 0; nn < statementArray.Length; nn++ ) {
                var statementText = statementArray[nn];
                var statementAction = statementActions[nn];
                Console.WriteLine("=>> {0}", statementText);

                SetupEPL(statementText);

                for (var ii = 0; ii <= 0; ii++)
                {
                    _eventCounter = 0;

                    var eventTime = (decimal) PerformanceObserver.TimeMicro(statementAction);
                    var eventCount = (decimal)_eventCounter;

                    Console.WriteLine("Measurements done at {0} threads", ThreadCount);
                    Console.WriteLine("\t{0:N0} events", eventCount);
                    Console.WriteLine("\t{0:N0} microseconds", eventTime);
                    Console.WriteLine("\t{0:N3} events per second", eventCount * 1000000.0m / eventTime);
                }

                Console.WriteLine();
            }
        }

        #region TestMethods

        // individual methods are used to obtain unique stack traces in the profile

        private static void TestWinLengthWithGroup() { SendBidEventsMT(TextWinLengthWithGroup); }
        private static void TestWinLength() { SendBidEventsMT(TextWinLength); }
        private static void TestAggregationWithTimeWindow() { SendBidEventsMT(TextAggregationWithTimeWindow); }
        private static void TestAggregationWithNoWindow() { SendBidEventsMT(TextAggregationWithNoWindow); }
        private static void TestWithPropertyNames() { SendBidEventsMT(TextWithPropertyNames); }
        private static void TestWithWildcard() { SendBidEventsMT(TextWithWildcard); }

        #endregion

        private static EPRuntime _runtime;
        private static EPStatement _masterStatement;
        private static long _eventCounter;

        /// <summary>
        /// Setup the esper.
        /// </summary>
        static void SetupEsper()
        {
            _container = ContainerExtensions.CreateDefaultContainer();

            var configuration = new Configuration(_container);
            configuration.Common.AddEventType(typeof(BidData));
            configuration.Compiler.Expression.IsUdfCache = true;
            configuration.Runtime.Logging.IsEnableExecutionDebug = false;
            configuration.Runtime.Logging.IsEnableTimerDebug = false;
            configuration.Runtime.MetricsReporting.IsEnableMetricsReporting = false;
            configuration.Runtime.MetricsReporting.IsThreading = false;

            _runtime = EPRuntimeProvider.GetDefaultRuntime(configuration);
        }

        // Baseline
        const string TextWinLength =
            @"select Symbol, TradeTime, avg(BidPrice), sum(BidPrice), count(*) " +
            @"  from BidData.win:length(1000)";
        const string TextWinLengthWithGroup =
            @"select Symbol, TradeTime, BidPrice, avg(BidPrice), sum(BidPrice) " +
            @"  from BidData.std:groupwin(Symbol).win:length(10000)";
        // Faster - Aggregation
        const string TextAggregationWithNoWindow =
            @"select Symbol, TradeTime, BidPrice, avg(BidPrice), sum(BidPrice) " +
            @"  from BidData";
        const string TextAggregationWithTimeWindow =
            @"select Symbol, TradeTime, avg(BidPrice), sum(BidPrice), count(*) " +
            @"  from BidData.win:time(2 seconds)";
        // Fastest
        const string TextWithWildcard =
            @"select *" +
            @"  from BidData";
        const string TextWithPropertyNames =
            @"select Symbol, TradeTime, BidPrice " +
            @"  from BidData";

        static void SetupEPL(string statementText)
        {
            _runtime.DeploymentService.UndeployAll();

            _masterStatement = CompileDeploy(statementText).Statements[0];
            _masterStatement.Events += (sender, e) => _eventCounter += e.NewEvents.Length;
        }
        
        static EPDeployment CompileDeploy(string epl)
        {
            try {
                var args = new CompilerArguments(_runtime.ConfigurationDeepCopy);
                args.Path.Add(_runtime.RuntimePath);
			    
                var compiled = EPCompilerProvider.Compiler.Compile(epl, args);
                return _runtime.DeploymentService.Deploy(compiled);
            }
            catch (Exception ex) {
                throw new EPRuntimeException(ex);
            }
        }

        static void SendBidEventsMT(string statementName)
        {
            var threadList = new Thread[ThreadCount];

            for (var ii = 0; ii < threadList.Length; ii++) {
                var bidEventDriver = new BidEventDriver(_runtime);
                var thread = new Thread(bidEventDriver.SendBidEvents);
                thread.Name = string.Format("{0}:{1}", statementName, ii);
                thread.Start();
                threadList[ii] = thread;
            }

            for (var ii = 0 ; ii < threadList.Length ; ii++ ) {
                threadList[ii].Join();
            }
        }
    }

    /// <summary>
    /// BidEventDriver sends BidData events into the runtime.
    /// </summary>
    public class BidEventDriver
    {
        private const int ThreadTime = 20000;
        private static readonly Random Rand = new Random();
        private static readonly string[] SymbolList = new string[]
        {
            "IBM",
            "MOT",
            "GOOG",
            "GS",
            "JPM",
            "BCA",
            "XOM",
            "SSO",
        };

        private readonly EPRuntime _runtime;
        private readonly EventSender _sender;

        /// <summary>
        /// Initializes a new instance of the <see cref="BidEventDriver"/> class.
        /// </summary>
        /// <param name="runtime">The esp runtime.</param>
        public BidEventDriver(EPRuntime runtime)
        {
            _runtime = runtime;
            _sender = _runtime.EventService.GetEventSender(typeof(BidData).Name);
        }

        public void SendBidEvent()
        {
            var bidEvent = new BidData(
                SymbolList[Rand.Next(SymbolList.Length)],
                0L,
                Rand.NextDouble());
            _sender.SendEvent(bidEvent);
        }

        public void SendBidEvents()
        {
            var eventCount = 0;
            var isAlive = new bool[1] { true };
            using (new Timer(
                delegate { isAlive[0] = false; },
                null,
                ThreadTime,
                Timeout.Infinite))
            {
#if false
                SendBidEvent();
#else
                while (isAlive[0])
                { 
                    SendBidEvent();
                    eventCount++;
                }
#endif
            }

            Console.WriteLine("Thread Done:\t{0:N0} events", eventCount);
        }
    }

    public class BidData
    {
        public string Symbol { get; set; }
        public long TradeTime { get; set; }
        public double BidPrice { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BidData"/> class.
        /// </summary>
        /// <param name="symbol">The symbol.</param>
        /// <param name="tradeTime">The java trade time.</param>
        /// <param name="bidPrice">The bid price.</param>
        public BidData(string symbol, long tradeTime, double bidPrice)
        {
            Symbol = symbol;
            TradeTime = tradeTime;
            BidPrice = bidPrice;
        }
    }
}
