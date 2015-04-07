///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using com.espertech.esper.compat;

using NEsper.Benchmark.Common;

namespace NEsper.Benchmark.Client
{
    /// <summary>
    /// A thread that sends market data (symbol, volume, price) at the target rate to the remote host
    /// </summary>
    public class MarketClient
    {
        private readonly Client client;
        private readonly MarketData[] market;
        private readonly SendDelegate sendMethod;

        private UdpClient uchannel;
        private TcpClient tchannel;

        private Socket socket;
        private readonly IPEndPoint endPoint;
        private readonly DataAssembler dataAssembler = new DataAssembler();

        private long bytesTransmitted;

        public MarketClient(Client client)
        {
            this.client = client;
            if (client.ipMode == IPMode.TCP) {
                dataAssembler.MaxMessageDelay = 200;
                dataAssembler.MaxMessageLength = 65536;
                dataAssembler.WriteMessage += WriteWithTCP;
            }
            else {
                dataAssembler.MaxMessageDelay = 200;
                dataAssembler.MaxMessageLength = client.mtu;
                dataAssembler.WriteMessage += WriteWithUDP;
            }

            sendMethod = dataAssembler.Serialize;
            endPoint = new IPEndPoint(ResolveHostOrAddress(client.host), client.port);
            market = new MarketData[Symbols.SYMBOLS.Length];
            for (int i = 0; i < market.Length; i++)
            {
                market[i] = new MarketData(Symbols.SYMBOLS[i], Symbols.NextPrice(10), Symbols.NextVolume(10));
            }
            Console.WriteLine("MarketData with {0} symbols", market.Length);
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
            catch( FormatException )
            {
            }

            return Dns.GetHostAddresses(host)[0];
        }

        private void WriteWithTCP(byte[] data, int offset, int length)
        {
            int bytesOut = socket.Send(data, offset, length, SocketFlags.None);
            if (bytesOut > 0) {
                Interlocked.Add(ref bytesTransmitted, bytesOut);
            }
        }

        private void WriteWithUDP(byte[] data, int offset, int length)
        {
            int bytesOut = socket.SendTo(data, offset, length, SocketFlags.None, endPoint);
            if (bytesOut > 0) {
                Interlocked.Add(ref bytesTransmitted, bytesOut);
            }
        }

        public const int KBPS = 1024;
        public const int MBPS = 1024*KBPS;
        public const int GBPS = 1024*MBPS;

        public static String RenderByteCount(long bytes)
        {
            double bitCount = bytes*8;

            //Console.WriteLine("bytes: {0}, bits: {1} / {2} / {3}",
            //    bytes,
            //    bitCount,
            //    bitCount / 1024,
            //    bitCount / 1024 / 1024
            //    );

            if (bitCount < 5 * KBPS)
            {
                return String.Format("{0} bps", bitCount);
            }
            if (bitCount < 5 * MBPS)
            {
                return String.Format("{0:F2} Kbps", bitCount / KBPS);
            }

            return String.Format("{0:F2} Mbps", bitCount / MBPS);
        }

        public void Run()
        {
            switch( client.ipMode )
            {
                case IPMode.TCP:
                    tchannel = new TcpClient(client.host, client.port);
                    Console.WriteLine("Client connected using TCP to {0}:{1}, rate {2} msg/s",
                                      client.host,
                                      client.port,
                                      client.rate);
                    socket = tchannel.Client;
                    socket.ReceiveBufferSize = 65536;
                    socket.SendBufferSize = 65536*8;
                    socket.DontFragment = false;
                    socket.NoDelay = true;

                    Console.WriteLine("SocketStatistics:");
                    Console.WriteLine("\tDontFragment: {0}", socket.DontFragment);
                    Console.WriteLine("\tNoDelay: {0}", socket.NoDelay);
                    Console.WriteLine("\tReceiveBufferSize: {0}", socket.ReceiveBufferSize);
                    Console.WriteLine("\tReceiveTimeout: {0}", socket.ReceiveTimeout);
                    Console.WriteLine("\tSendBufferSize: {0}", socket.SendBufferSize);
                    Console.WriteLine("\tSendTimeout: {0}", socket.SendTimeout);
                    break;
                case IPMode.UDP:
                    Console.WriteLine("Client sending using UDP to {0}:{1}, rate {2} msg/s",
                                      client.host,
                                      client.port,
                                      client.rate);

                    uchannel = new UdpClient(client.host, client.port);
                    socket = uchannel.Client;
                    socket.ReceiveBufferSize = 65536*8;
                    socket.SendBufferSize = 65536*8;
                    socket.DontFragment = false;

                    Console.WriteLine("SocketStatistics:");
                    Console.WriteLine("\tDontFragment: {0}", socket.DontFragment);
                    Console.WriteLine("\tReceiveBufferSize: {0}", socket.ReceiveBufferSize);
                    Console.WriteLine("\tReceiveTimeout: {0}", socket.ReceiveTimeout);
                    Console.WriteLine("\tSendBufferSize: {0}", socket.SendBufferSize);
                    Console.WriteLine("\tSendTimeout: {0}", socket.SendTimeout);
                    break;
            }

            eventPer50ms = client.rate / 20;
            tickerIndex = 0;
            countLast5s = 0;
            sleepLast5s = 0;
            lastThroughputTick = Environment.TickCount;
            eventCount = 0;
            maxEventCount = client.totalEvents;

            try
            {
                var highPerformanceTimer = new HighResolutionTimer(
                    HandleTimerEvents,
                    null,
                    0,
                    50);

                Console.WriteLine(">> Press Any Key To Exit <<");
                Console.ReadKey();

                highPerformanceTimer.Dispose();
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Error sending data to server.  Did server disconnect");
                Console.Error.WriteLine("Error message: {0}", e.Message);
                Console.Error.WriteLine(e.StackTrace);
            }
        }

        private int eventPer50ms;
        private int tickerIndex;
        private int countLast5s;
        private long sleepLast5s;
        private long lastThroughputTick;
        private long eventCount;
        private long maxEventCount;

        private void HandleTimerEvents(object userData)
        {
            for (int i = 0; i < eventPer50ms; i++) {
                if (eventCount++ >= maxEventCount) {
                    break;
                }

                tickerIndex = tickerIndex%Symbols.SYMBOLS.Length;
                MarketData md = market[tickerIndex++];
                md.Price = Symbols.NextPrice(md.Price);
                md.Volume = Symbols.NextVolume(10);
                md.Time = DateTime.Now.Ticks;

                bytesTransmitted += sendMethod.Invoke(md);

                countLast5s++;
            }

            // info
            int tickCount = Environment.TickCount;
            if ((tickCount - lastThroughputTick) > 5000) {
                long mByteCount = Interlocked.Exchange(ref bytesTransmitted, 0L);
                long mDelta = tickCount - lastThroughputTick;
                Console.WriteLine(
                    "Sent {0} in {1}(ms) avg ns/msg {2}(ns) avg {3}(msg/s) sleep {4}(ms) velocity {5}",
                    countLast5s,
                    mDelta,
                    (float) 1E6*countLast5s/(tickCount - lastThroughputTick),
                    countLast5s/5,
                    sleepLast5s,
                    RenderByteCount(mByteCount*1000/mDelta));

                countLast5s = 0;
                sleepLast5s = 0;
                lastThroughputTick = tickCount;
            }
        }

        private delegate int SendDelegate(MarketData mdEvent);
    }
} // End of namespace
