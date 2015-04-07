///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using com.espertech.esper.compat.collections;
using NEsper.Benchmark.Common;

using com.espertech.esper.compat;

namespace NEsper.Benchmark.Server
{
    /// <summary>
    /// The main Esper Server thread listens on the given port.
    /// It bootstrap an ESP/CEP engine (defaults to Esper) and registers EPL Statement(s) into it based
    /// on the given -mode argument.
    /// Statements are read from an statements.properties file in the classpath
    /// If statements contains '$' the '$' is replaced by a symbol string, so as to register one statement per symbol.
    /// <para/>
    /// Based on -queue, the server implements a direct handoff to the ESP/CEP engine, or uses a synchronous queue
    /// (somewhat an indirect direct handoff), or uses a FIFO queue where each events is put/take one by one from the queue.
    /// Usually with few clients sending a lot of events, use the direct handoff, else consider using queues. Consumer thread
    /// can be configured using -thread (it will range up to #processor x #thread).
    /// When queues is full, overload policy triggers execution on the caller side.
    /// <para/>
    /// To simulate an ESP/CEP listener work, use -sleep.
    /// <para/>
    /// Use -stat to control how often percentile stats are displayed. At each display stats are reset.
    /// <para/>
    /// If you use -rate nxM (n threads, M event/s), the server will simulate the load for a standalone simulation without
    /// any remote Client(s).
    /// </summary>
	public class Server 
    {
        private IPAddress address = IPAddress.Any;
        private readonly Thread thread;
	    private readonly int port;
        private readonly int sleepListenerMillis;
        private readonly int statSec;
        private readonly int simulationRate;
        private readonly int simulationThread;
        private readonly String mode;
        private readonly Executor executor;
        private readonly Timer timer;
        private readonly IPMode ipMode;
        private readonly int simulationIterations;

	    public static readonly int DEFAULT_PORT = 6789;
        public static readonly int? DEFAULT_THREADCORE = null;
        public static readonly int? DEFAULT_QUEUEMAX = null;
	    public static readonly int DEFAULT_SLEEP = 0;
	    public static readonly int DEFAULT_SIMULATION_RATE = -1;//-1: no simulation
	    public static readonly int DEFAULT_SIMULATION_THREAD = -1;//-1: no simulation
	    public static readonly int DEFAULT_STAT = 5;
        public static readonly IPMode DEFAULT_IPMODE = IPMode.TCP;
	    public static readonly String DEFAULT_MODE = "NOOP";
	    public static readonly Properties MODES = new Properties();
        public static readonly String DEFAULT_EXECUTOR = "inline";
        public static readonly HashSet<String> AVAILABLE_EXECUTORS =
            new HashSet<String>(new String[] {"inline", "unbound", "bound", "mqueue"});

	    private CEPProvider.ICEPProvider cepProvider;

	    public Server(String mode, String hostOrAddr, int port, IPMode ipMode, Executor executor, int sleep, int statSec, int simulationThread, int simulationRate, int simulationIterations)
        {
            thread = new Thread(Run);
            thread.Name = "EsperServer-main";

            if (!String.IsNullOrEmpty(hostOrAddr)) {
                address = ResolveHostOrAddress(hostOrAddr);
            }

	        this.mode = mode;
	        this.port = port;
            this.ipMode = ipMode;
            this.executor = executor;
	        this.sleepListenerMillis = sleep;
	        this.statSec = statSec;
	        this.simulationThread = simulationThread;
	        this.simulationRate = simulationRate;
	        this.simulationIterations = simulationIterations;

	        // turn on stat dump
	        timer = new Timer(DisplayStatistics, null, 0L, statSec*1000);
	    }

        /// <summary>
        /// Resolves the host or address.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <returns></returns>
        private static IPAddress ResolveHostOrAddress(String host)
        {
            try
            {
                return IPAddress.Parse(host);
            }
            catch (FormatException)
            {
            }

            return Dns.GetHostAddresses(host)[0];
        }

        public void DisplayStatistics(Object userData)
        {
            StatsHolder.Dump("engine");
            StatsHolder.Dump("server");
            StatsHolder.Dump("endToEnd");
            StatsHolder.Reset();
            if (simulationRate <= 0)
            {
                ClientConnection.DumpStats(statSec);
            }
            else
            {
                SimulateClientConnection.DumpStats(statSec);
            }
        }

        /// <summary>
        /// Gets or sets the provider.
        /// </summary>
        /// <value>The provider.</value>
        public CEPProvider.ICEPProvider Provider
        {
            get { return cepProvider; }
            set { cepProvider = value; }
        }

        /// <summary>
        /// Starts this instance.
        /// </summary>
        public void Start()
	    {
	        lock (this)
	        {
	            // register ESP/CEP engine
	            cepProvider = CEPProvider.GetCEPProvider();
	            cepProvider.Init(sleepListenerMillis);

	            // register statements
	            String suffix = MODES.Get("_SUFFIX");
	            if (mode == "NOOP")
	            {
	                ;
	            }
	            else
	            {
	                String stmtString = MODES.Get(mode) + " " + suffix;
	                Console.WriteLine("Using " + mode + " : " + stmtString);

	                if (MODES.Get(mode).IndexOf('$') < 0)
	                {
	                    cepProvider.RegisterStatement(stmtString, mode);
	                    Console.WriteLine("\nStatements registered # 1 only");
	                }
	                else
	                {
	                    // create a stmt for each symbol
	                    for (int i = 0; i < Symbols.SYMBOLS.Length; i++)
	                    {
	                        if (i%100 == 0) Console.WriteLine(".");
	                        String ticker = Symbols.SYMBOLS[i];
	                        cepProvider.RegisterStatement(stmtString.Replace("\\$", ticker), mode + "-" + ticker);
	                    }
	                    Console.WriteLine("\nStatements registered # " + Symbols.SYMBOLS.Length);
	                }
	            }
	        }

	        thread.Start();
	    }

        /// <summary>
        /// Yields execution until the server has stopped.
        /// </summary>
        public void WaitForCompletion()
        {
            thread.Join();
        }

        /// <summary>
        /// Runs this instance.
        /// </summary>
        public void Run()
        {
            if (simulationRate <= 0)
            {
                RunServer();
            }
            else
            {
                RunSimulation();
            }
        }

        /// <summary>
        /// Runs the server.
        /// </summary>
        public void RunServer()
	    {
            IPEndPoint endpoint = new IPEndPoint(address, port);

            if (ipMode == IPMode.TCP)
            {
                Console.WriteLine("Server accepting TCP connections on port {0}", port);
                
                TcpListener listener = new TcpListener(endpoint);
                listener.Start();

                do
                {
                    TcpClient client = listener.AcceptTcpClient();
                    Console.WriteLine("Client connected to server.");
                    ClientConnection clientConnection = new TcpClientConnection(client, executor, cepProvider, statSec);
                    clientConnection.Start();
                } while (true);
            }
            else if ( ipMode == IPMode.UDP )
            {
                Console.WriteLine("Server accepting UDP datagrams on port {0}", port);

                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                socket.Bind(endpoint);

                UdpClientConnection clientConnection = new UdpClientConnection(socket, executor, cepProvider, statSec);
                clientConnection.Start();

                while( true )
                {
                    Console.ReadLine();
                }
            }
	    }

        /// <summary>
        /// Runs the simulation.
        /// </summary>
        public void RunSimulation()
        {
            Console.WriteLine(
                "Server in sumulation mode with event/s {0} x {1} = {2}",
                simulationThread,
                simulationRate,
                simulationThread*simulationRate);

	        SimulateClientConnection[] sims = new SimulateClientConnection[simulationThread];
	        for (int i = 0; i < sims.Length; i++) {
	            sims[i] = new SimulateClientConnection(simulationRate, simulationIterations, executor, cepProvider, statSec);
	            sims[i].Start();
	        }

            foreach (SimulateClientConnection sim in sims) {
                sim.WaitForCompletion();
            }
	    }

	    public static void Main(String[] argv)
	    {
	        //Console.SetOut(new DummyTextWriter(Console.Out));

	        // load modes
	        NameValueCollection baseInfo = (NameValueCollection) ConfigurationManager.GetSection("StatementProperties");
	        for (int ii = 0; ii < baseInfo.Count; ii++)
	            MODES.Add(baseInfo.GetKey(ii), baseInfo.Get(ii));

	        MODES.Put("NOOP", "");

	        int port = DEFAULT_PORT;
	        int? threadCore = DEFAULT_THREADCORE;
	        int? queueMax = DEFAULT_QUEUEMAX;
	        int sleep = DEFAULT_SLEEP;
	        int simulationRate = DEFAULT_SIMULATION_RATE;
	        int simulationThread = DEFAULT_SIMULATION_THREAD;
	        int simulationIterations = -1;
	        IPMode ipMode = DEFAULT_IPMODE;
	        String mode = DEFAULT_MODE;
	        int stats = DEFAULT_STAT;
	        String executorType = DEFAULT_EXECUTOR;
	        String addr = null;

	        for (int i = 0; i < argv.Length; i++)
	        {
	            switch (argv[i])
	            {
                    case "-prio":
	                    i++;
	                    SetProcessPriority(argv[i]);
	                    break;
                    case "-addr":
	                    i++;
	                    addr = argv[i];
	                    break;
	                case "-port":
	                    i++;
	                    port = Int32.Parse(argv[i]);
	                    break;
	                case "-thread":
	                    i++;
	                    threadCore = Int32.Parse(argv[i]);
	                    break;
	                case "-queue":
	                    i++;
	                    queueMax = Int32.Parse(argv[i]);
	                    break;
	                case "-sleep":
	                    i++;
	                    sleep = Int32.Parse(argv[i]);
	                    break;
	                case "-stat":
	                    i++;
	                    stats = Int32.Parse(argv[i]);
	                    break;
	                case "-mode":
	                    i++;
	                    mode = argv[i];
	                    if (MODES.Get(mode) == null)
	                    {
	                        Console.Error.WriteLine("Unknown mode");
	                        PrintUsage();
	                    }
	                    break;
                    case "-rate":
	                    {
	                        i++;
	                        var parts = argv[i].Split('x');
                            if (parts.Length == 2) {
	                            simulationThread = Int32.Parse(parts[0]);
                                simulationRate = Int32.Parse(parts[1]);
                                simulationIterations = -1;
                            } else if (parts.Length == 3) {
                                simulationThread = Int32.Parse(parts[0]);
                                simulationRate = Int32.Parse(parts[1]);
                                simulationIterations = Int32.Parse(parts[2]);
                            } else {
                                Console.Error.WriteLine("Invalid rate specification");
                                PrintUsage();
                            }
	                        
	                    }
	                    break;

                    case "-udp":
	                    ipMode = IPMode.UDP;
	                    break;

                    case "-tcp":
	                    ipMode = IPMode.TCP;
	                    break;

                    case "-exec":
	                    i++;
	                    executorType = argv[i];
                        if (!AVAILABLE_EXECUTORS.Contains( executorType ))
                        {
                            Console.Error.WriteLine("Unknown executor");
                            PrintUsage();
                        }
	                    break;
	                default:
	                    PrintUsage();
	                    break;
	            }
	        }

	        Executor executor = CreateExecutor(executorType, threadCore, queueMax);
	        Server bs = new Server(mode, addr, port, ipMode, executor, sleep, stats, simulationThread, simulationRate, simulationIterations);
	        bs.Start();
            bs.WaitForCompletion();
	    }

        private static void SetProcessPriority(string value)
        {
            var priorityClass = EnumHelper.Parse<ProcessPriorityClass>(value, true);
            Process.GetCurrentProcess().PriorityBoostEnabled = true; 
            Process.GetCurrentProcess().PriorityClass = priorityClass;
        }

        /// <summary>
        /// Creates the executor.  QueueMax determines the maximum queue that the executor
        /// should use; it is optional and even may not be valid for the type of executor
        /// that is created.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="threadCore">The thread core.</param>
        /// <param name="queueMax">The queue max.</param>
        /// <returns></returns>
        private static Executor CreateExecutor( String type, int? threadCore, int? queueMax)
        {
            switch (type)
            {
                case "inline":
                    Console.WriteLine("Using inline executor, cpu#{0}", Environment.ProcessorCount);
                    return new InlineExecutor();
                case "unbound":
                    Console.WriteLine("Using ThreadPoolExecutor, cpu#{0} threadCore#{1} queue#{2}",
                                      Environment.ProcessorCount,
                                      threadCore,
                                      queueMax);
                    return new ThreadPoolExecutor(queueMax);
                case "mqueue":
                    Console.WriteLine("Using QueueExecutor, cpu#{0} threadCore#{1} queue#{2}",
                                      Environment.ProcessorCount,
                                      threadCore,
                                      queueMax);
                    return new MultiQueueExecutor((threadCore ?? 1) * Environment.ProcessorCount, queueMax);
                case "bound":
                    Console.WriteLine("Using QueueExecutor, cpu#{0} threadCore#{1} queue#{2}",
                                      Environment.ProcessorCount,
                                      threadCore,
                                      queueMax);
                    return new QueueExecutor((threadCore ?? 1)*Environment.ProcessorCount);
                default:
                    throw new ArgumentException("invalid executor type", type);
            }
        }

	    private static void PrintUsage() {
	        Console.Error.WriteLine("usage: com.espertech.esper.example.benchmark.server.Server <-port #> <-thread #> <-queue #> <-sleep #> <-stat #> <-rate #x#> <-tcp|-udp> <-exec xyz> <-mode xyz>");
	        Console.Error.WriteLine("defaults:");
	        Console.Error.WriteLine("  -port:    " + DEFAULT_PORT);
	        Console.Error.WriteLine("  -thread:  " + DEFAULT_THREADCORE);
            Console.Error.WriteLine("  -queue:   " + DEFAULT_QUEUEMAX);
	        Console.Error.WriteLine("  -tcp      " + "(default)");
            Console.Error.WriteLine("  -udp      ");
            Console.Error.WriteLine("  -exec:    " + "(default " + DEFAULT_EXECUTOR + ", choose from " + AVAILABLE_EXECUTORS.Render());
	        Console.Error.WriteLine("  -sleep:   " + DEFAULT_SLEEP + "(no sleep)");
	        Console.Error.WriteLine("  -stat:    " + DEFAULT_STAT + "(s)");
	        Console.Error.WriteLine("  -rate:    " + DEFAULT_SIMULATION_RATE + "(no standalone simulation, else <n>x<evt/s>[x<iterations>] such as 2x1000x10)");
	        Console.Error.WriteLine("  -mode:    " + "(default " + DEFAULT_MODE + ", choose from " + MODES.Keys.Render() + ")");
	        Console.Error.WriteLine("Modes are read from statements.properties in the classpath");
	        Environment.Exit(1);
	    }
	}
} // End of namespace
