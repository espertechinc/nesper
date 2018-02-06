///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;

using NEsper.Benchmark.Common;

namespace NEsper.Benchmark.Client
{
    /// <summary>
    /// A client that sends MarketData information over a TCP socket to a remote server.
    /// Run with no args to see available options
    /// </summary>
    /// <unknown>@see MarketClient</unknown>
    /// <author>Alexandre Vasseur http://avasseur.blogspot.com</author>
    public class Client
    {
        public const int MINIMUM_RATE = 1000;
        public const int DEFAULT_PORT = 6789;
        public const int DEFAULT_RATE = 4000;
        public const String DEFAULT_HOST = "localhost";
        public const IPMode DEFAULT_IPMODE = IPMode.TCP;
        public const int DEFAULT_DELAY = 0;

        internal String host;
        internal int port;
        internal int rate;
        internal IPMode ipMode;
        internal int mtu;
        internal int totalEvents;

        public Client(String host, int port, IPMode ipMode, int rate, int totalEvents, int mtu)
        {
            this.host = host;
            this.port = port;
            this.rate = rate;
            this.ipMode = ipMode;
            this.totalEvents = totalEvents;
            this.mtu = mtu;
        }

        public static void Main(String[] argv)
        {
            var delay = DEFAULT_DELAY;
            var rate = Math.Max(DEFAULT_RATE, MINIMUM_RATE);
            var port = DEFAULT_PORT;
            var host = DEFAULT_HOST;
            var ipMode = DEFAULT_IPMODE;
            var mtu = 1024;
            var totalEvents = Int32.MaxValue;

            for (var i = 0; i < argv.Length; i++)
            {
                switch (argv[i].ToLowerInvariant())
                {
                    case "-rate":
                        i++;
                        rate = Int32.Parse(argv[i]);
                        if (rate < MINIMUM_RATE)
                        {
                            Console.Error.WriteLine("[WARNING] Minimum rate is " + MINIMUM_RATE);
                            rate = MINIMUM_RATE;
                        }
                        break;
                    case "-mtu":
                        i++;
                        mtu = Int32.Parse(argv[i]);
                        break;
                    case "-port":
                        i++;
                        port = Int32.Parse(argv[i]);
                        break;
                    case "-host":
                        i++;
                        host = argv[i];
                        break;
                    case "-delay":
                        i++;
                        delay = Int32.Parse(argv[i]);
                        break;
                    case "-tcp":
                        ipMode = IPMode.TCP;
                        break;
                    case "-udp":
                        ipMode = IPMode.UDP;
                        break;
                    case "-max":
                    case "-maxe":
                    case "-maxevents":
                        i++;
                        totalEvents = Int32.Parse(argv[i]);
                        break;
                    default:
                        PrintUsage();
                        break;
                }
            }

            if ( delay != 0 )
            {
                Console.WriteLine("Delaying execution by {0} milliseconds", delay);
                Thread.Sleep(delay);
            }

            var ms = new MarketClient(new Client(host, port, ipMode, rate, totalEvents, mtu));

            var thread = new Thread(ms.Run);
            thread.Start();
            thread.Join();
        }

        private static void PrintUsage()
        {
            Console.Error.WriteLine("{0} <-host hostname> <-port #> <-tcp|-udp> <-rate #>", Environment.GetCommandLineArgs()[0]);
            Console.Error.WriteLine("defaults:");
            Console.Error.WriteLine("    Rate: " + DEFAULT_RATE + " msg/s");
            Console.Error.WriteLine("    Host: " + DEFAULT_HOST);
            Console.Error.WriteLine("    Port: " + DEFAULT_PORT);
            Console.Error.WriteLine("    IPMode: " + DEFAULT_IPMODE);
            Environment.Exit(1);
        }
    }
} // End of namespace
