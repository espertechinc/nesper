///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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
        private IPAddress _address = IPAddress.Any;
        private readonly Thread _thread;
	    private readonly int _port;
        private readonly int _sleepListenerMillis;
        private readonly int _statSec;
        private readonly int _simulationRate;
        private readonly int _simulationThread;
        private readonly string _mode;
        private readonly Executor _executor;
        private readonly Timer _timer;
        private readonly IPMode _ipMode;
        private readonly int _simulationIterations;

	    public static readonly int DEFAULT_PORT = 6789;
        public static readonly int? DEFAULT_THREADCORE = null;
        public static readonly int? DEFAULT_QUEUEMAX = null;
	    public static readonly int DEFAULT_SLEEP = 0;
	    public static readonly int DEFAULT_SIMULATION_RATE = -1;//-1: no simulation
	    public static readonly int DEFAULT_SIMULATION_THREAD = -1;//-1: no simulation
	    public static readonly int DEFAULT_STAT = 5;
        public static readonly IPMode DEFAULT_IPMODE = IPMode.TCP;
	    public static readonly string DEFAULT_MODE = "NOOP";
	    public static readonly Properties MODES = new Properties();
        public static readonly string DEFAULT_EXECUTOR = "inline";
        public static readonly HashSet<string> AVAILABLE_EXECUTORS =
            new HashSet<string>(new string[] {"inline", "unbound", "bound", "mqueue"});

	    private CEPProvider.ICEPProvider _cepProvider;

	    public Server(string mode, string hostOrAddr, int port, IPMode ipMode, Executor executor, int sleep, int statSec, int simulationThread, int simulationRate, int simulationIterations)
        {
            _thread = new Thread(Run);
            _thread.Name = "EsperServer-main";

            if (!string.IsNullOrEmpty(hostOrAddr)) {
                _address = ResolveHostOrAddress(hostOrAddr);
            }

	        _mode = mode;
	        _port = port;
            _ipMode = ipMode;
            _executor = executor;
	        _sleepListenerMillis = sleep;
	        _statSec = statSec;
	        _simulationThread = simulationThread;
	        _simulationRate = simulationRate;
	        _simulationIterations = simulationIterations;

	        // turn on stat dump
	        _timer = new Timer(DisplayStatistics, null, 0L, statSec*1000);
	    }

        /// <summary>
        /// Resolves the host or address.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <returns></returns>
        private static IPAddress ResolveHostOrAddress(string host)
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

        public void DisplayStatistics(object userData)
        {
            StatsHolder.Dump("engine");
            StatsHolder.Dump("server");
            StatsHolder.Dump("endToEnd");
            StatsHolder.Reset();
            if (_simulationRate <= 0)
            {
                ClientConnection.DumpStats(_statSec);
            }
            else
            {
                SimulateClientConnection.DumpStats(_statSec);
            }
        }

        /// <summary>
        /// Gets or sets the provider.
        /// </summary>
        /// <value>The provider.</value>
        public CEPProvider.ICEPProvider Provider
        {
            get => _cepProvider;
            set => _cepProvider = value;
        }

        /// <summary>
        /// Starts this instance.
        /// </summary>
        public void Start()
	    {
	        lock (this)
	        {
	            // register ESP/CEP engine
	            _cepProvider = CEPProvider.GetCEPProvider();
	            _cepProvider.Init(_sleepListenerMillis);

	            // register statements
	            var suffix = MODES.Get("_SUFFIX");
	            if (_mode == "NOOP")
	            {
	                ;
	            }
	            else
	            {
	                var stmtString = MODES.Get(_mode) + " " + suffix;
	                Console.WriteLine("Using " + _mode + " : " + stmtString);

	                if (MODES.Get(_mode).IndexOf('$') < 0)
	                {
	                    _cepProvider.RegisterStatement(stmtString, _mode);
	                    Console.WriteLine("\nStatements registered # 1 only");
	                }
	                else
	                {
	                    // create a stmt for each symbol
	                    for (var i = 0; i < Symbols.SYMBOLS.Length; i++)
	                    {
	                        if (i%100 == 0) Console.WriteLine(".");
	                        var ticker = Symbols.SYMBOLS[i];
	                        _cepProvider.RegisterStatement(stmtString.Replace("\\$", ticker), _mode + "-" + ticker);
	                    }
	                    Console.WriteLine("\nStatements registered # " + Symbols.SYMBOLS.Length);
	                }
	            }
	        }

	        _thread.Start();
	    }

        /// <summary>
        /// Yields execution until the server has stopped.
        /// </summary>
        public void WaitForCompletion()
        {
            _thread.Join();
        }

        /// <summary>
        /// Runs this instance.
        /// </summary>
        public void Run()
        {
            if (_simulationRate <= 0)
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
            var endpoint = new IPEndPoint(_address, _port);

            if (_ipMode == IPMode.TCP)
            {
                Console.WriteLine("Server accepting TCP connections on port {0}", _port);
                
                var listener = new TcpListener(endpoint);
                listener.Start();

                do
                {
                    var client = listener.AcceptTcpClient();
                    Console.WriteLine("Client connected to server.");
                    ClientConnection clientConnection = new TcpClientConnection(client, _executor, _cepProvider, _statSec);
                    clientConnection.Start();
                } while (true);
            }
            else if ( _ipMode == IPMode.UDP )
            {
                Console.WriteLine("Server accepting UDP datagrams on port {0}", _port);

                var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                socket.Bind(endpoint);

                var clientConnection = new UdpClientConnection(socket, _executor, _cepProvider, _statSec);
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
                _simulationThread,
                _simulationRate,
                _simulationThread*_simulationRate);

	        var sims = new SimulateClientConnection[_simulationThread];
	        for (var i = 0; i < sims.Length; i++) {
	            sims[i] = new SimulateClientConnection(_simulationRate, _simulationIterations, _executor, _cepProvider, _statSec);
	            sims[i].Start();
	        }

            foreach (var sim in sims) {
                sim.WaitForCompletion();
            }
	    }

	    public static void Main(string[] argv)
	    {
	        //Console.SetOut(new DummyTextWriter(Console.Out));

	        // load modes
	        // - this needs to be revised as it is no longer applicable
#if false
	        var baseInfo = (NameValueCollection) ConfigurationManager.GetSection("StatementProperties");
	        for (var ii = 0; ii < baseInfo.Count; ii++) {
		        MODES.Put(baseInfo.GetKey(ii), baseInfo.Get(ii));
	        }
#endif

	        MODES.Put("NOOP", "");

	        var port = DEFAULT_PORT;
	        var threadCore = DEFAULT_THREADCORE;
	        var queueMax = DEFAULT_QUEUEMAX;
	        var sleep = DEFAULT_SLEEP;
	        var simulationRate = DEFAULT_SIMULATION_RATE;
	        var simulationThread = DEFAULT_SIMULATION_THREAD;
	        var simulationIterations = -1;
	        var ipMode = DEFAULT_IPMODE;
	        var mode = DEFAULT_MODE;
	        var stats = DEFAULT_STAT;
	        var executorType = DEFAULT_EXECUTOR;
	        string addr = null;

	        for (var i = 0; i < argv.Length; i++)
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
	                    port = int.Parse(argv[i]);
	                    break;
	                case "-thread":
	                    i++;
	                    threadCore = int.Parse(argv[i]);
	                    break;
	                case "-queue":
	                    i++;
	                    queueMax = int.Parse(argv[i]);
	                    break;
	                case "-sleep":
	                    i++;
	                    sleep = int.Parse(argv[i]);
	                    break;
	                case "-stat":
	                    i++;
	                    stats = int.Parse(argv[i]);
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
	                            simulationThread = int.Parse(parts[0]);
                                simulationRate = int.Parse(parts[1]);
                                simulationIterations = -1;
                            } else if (parts.Length == 3) {
                                simulationThread = int.Parse(parts[0]);
                                simulationRate = int.Parse(parts[1]);
                                simulationIterations = int.Parse(parts[2]);
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

	        var executor = CreateExecutor(executorType, threadCore, queueMax);
	        var bs = new Server(mode, addr, port, ipMode, executor, sleep, stats, simulationThread, simulationRate, simulationIterations);
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
        private static Executor CreateExecutor( string type, int? threadCore, int? queueMax)
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
            Console.Error.WriteLine("  -exec:    " + "(default " + DEFAULT_EXECUTOR + ", choose from " + AVAILABLE_EXECUTORS.RenderAny());
	        Console.Error.WriteLine("  -sleep:   " + DEFAULT_SLEEP + "(no sleep)");
	        Console.Error.WriteLine("  -stat:    " + DEFAULT_STAT + "(s)");
	        Console.Error.WriteLine("  -rate:    " + DEFAULT_SIMULATION_RATE + "(no standalone simulation, else <n>x<evt/s>[x<iterations>] such as 2x1000x10)");
	        Console.Error.WriteLine("  -mode:    " + "(default " + DEFAULT_MODE + ", choose from " + MODES.Keys.RenderAny() + ")");
	        Console.Error.WriteLine("Modes are read from statements.properties in the classpath");
	        Environment.Exit(1);
	    }
	}
} // End of namespace
