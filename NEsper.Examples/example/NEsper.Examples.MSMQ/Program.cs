///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;
using System.Xml.Linq;

using com.espertech.esper.common.client.util;
using com.espertech.esper.compat;
using com.espertech.esper.compiler.client;
using com.espertech.esper.container;
using com.espertech.esper.runtime.client;

using Configuration = com.espertech.esper.common.client.configuration.Configuration;

namespace NEsper.Examples.MSMQ
{
    class Program
    {
        private static EPRuntime _runtime;
        /// <summary>
        /// Egress point for events
        /// </summary>
        private static MsmqPublisher _publisher;
        /// <summary>
        /// Ingress point for events
        /// </summary>
        private static MsmqConsumer _consumer;
        /// <summary>
        /// Randomizer for pricing
        /// </summary>
        private static Random _random;

        /// <summary>
        /// Number of events consumed
        /// </summary>
        private static long _eventsConsumed;

        /// <summary>
        /// Reset event for completion
        /// </summary>
        private static ManualResetEvent _consumerEvent;

        /// <summary>
        /// Constant indicator flow
        /// </summary>
        private const string MqPath = @".\private$\stock-ticker";

        /// <summary>
        /// Events
        /// </summary>
        private const int EventCount = 100000;


        static void Main()
        {
            _random = new Random();

            var container = ContainerExtensions.CreateDefaultContainer();

            // Creates a statement that looks for market data events.
            var configuration = new Configuration(container);
            configuration.Common.AddEventType<MarketDataTrade>();
            configuration.Common.AddEventType<EndOfTest>();
            // set to true to decouple event processing from msmq latency
            configuration.Runtime.Threading.IsThreadPoolOutbound = false;

            _runtime = EPRuntimeProvider.GetDefaultRuntime(configuration);
            CompileDeploy("select * from MarketDataTrade").
                Events += (sender, eventArgs) => _publisher.SendEvent(eventArgs);
            CompileDeploy("select * from EndOfTest")
                .Events += (sender, eventArgs) => _publisher.SendEvent(eventArgs);
         
            // create a publisher
            _publisher = new MsmqPublisher(MqPath);
            // create a consumer
            _consumer = new MsmqConsumer(MqPath, ConsumeEvent);
            _consumerEvent = new ManualResetEvent(false);

            // mark the starting time
            var milliTime = PerformanceObserver.TimeMillis(
                delegate
                    {
                        // send some events
                        SendEvents();

                        // wait for events
                        _consumerEvent.WaitOne();
                    });

            Console.WriteLine("Recv: {0} events in {1} ms", _eventsConsumed, milliTime);
            Console.ReadLine();
        }

        /// <summary>
        /// Sends the events.
        /// </summary>
        static void SendEvents()
        {
            var sender = _runtime.EventService.GetEventSender("MarketDataTrade");
            var milliTime = PerformanceObserver.TimeMillis(
                delegate
                    {
                        for (int ii = 0; ii < EventCount; ii++) {
                            var trade = new MarketDataTrade(
                                "GOOG", 100*_random.Next(1, 100), 500.0m + _random.Next(0, 100));
                            sender.SendEvent(trade);
                        }
                    });

            Console.WriteLine("Send: {0} events in {1} us", EventCount, milliTime);

            _runtime.EventService.SendEventBean(new EndOfTest(), "EndOfTest");
        }

        /// <summary>
        /// Consumes the event coming from the queue.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="updateEventArgs">The <see cref="UpdateEventArgs"/> instance containing the event data.</param>
        static void ConsumeEvent(Object sender, UpdateEventArgs updateEventArgs)
        {
            foreach(var eventBean in updateEventArgs.NewEvents) {
                var element = (XElement) eventBean.Underlying;
                switch(element.Name.LocalName) {
                    case "MarketDataTrade":
                        Interlocked.Increment(ref _eventsConsumed);
                        break;
                    case "EndOfTest":
                        _consumerEvent.Set();
                        break;
                }
            }
        }
        
        public static EPStatement CompileDeploy(string epl)
        {
            var args = new CompilerArguments();
            args.Path.Add(_runtime.RuntimePath);
            args.Options.AccessModifierNamedWindow = env => NameAccessModifier.PUBLIC;
            args.Configuration.Compiler.ByteCode.AllowSubscriber = true;

            var compiled = EPCompilerProvider.Compiler.Compile(epl, args);
            var deployment = _runtime.DeploymentService.Deploy(compiled);
            return deployment.Statements[0];
        }
    }

    public class MarketDataTrade
    {
        /// <summary>
        /// Gets or sets the symbol.
        /// </summary>
        /// <value>The symbol.</value>
        public string Symbol { get; set; }
        /// <summary>
        /// Gets or sets the size.
        /// </summary>
        /// <value>The size.</value>
        public int Size { get; set; }
        /// <summary>
        /// Gets or sets the price.
        /// </summary>
        /// <value>The price.</value>
        public decimal Price { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MarketDataTrade"/> class.
        /// </summary>
        /// <param name="symbol">The symbol.</param>
        /// <param name="size">The size.</param>
        /// <param name="price">The price.</param>
        public MarketDataTrade(string symbol, int size, decimal price)
        {
            Symbol = symbol;
            Size = size;
            Price = price;
        }

        public MarketDataTrade()
        {
        }
    }

    /// <summary>
    /// An event that indicates we're done
    /// </summary>
    public class EndOfTest
    {
    }
}
