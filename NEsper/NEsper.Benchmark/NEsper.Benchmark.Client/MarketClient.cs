///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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
        private readonly Client _client;
        private readonly MarketData[] _market;
        private readonly SendDelegate _sendMethod;

        private UdpClient _uchannel;
        private TcpClient _tchannel;

        private Socket _socket;
        private readonly IPEndPoint _endPoint;
        private readonly DataAssembler _dataAssembler = new DataAssembler();

        private long _bytesTransmitted;

        public MarketClient(Client client)
        {
            this._client = client;
            if (client.ipMode == IPMode.TCP) {
                _dataAssembler.MaxMessageDelay = 200;
                _dataAssembler.MaxMessageLength = 65536;
                _dataAssembler.WriteMessage += WriteWithTCP;
            }
            else {
                _dataAssembler.MaxMessageDelay = 200;
                _dataAssembler.MaxMessageLength = client.mtu;
                _dataAssembler.WriteMessage += WriteWithUDP;
            }

            _sendMethod = _dataAssembler.Serialize;
            _endPoint = new IPEndPoint(ResolveHostOrAddress(client.host), client.port);
            _market = new MarketData[Symbols.SYMBOLS.Length];
            for (int i = 0; i < _market.Length; i++)
            {
                _market[i] = new MarketData(Symbols.SYMBOLS[i], Symbols.NextPrice(10), Symbols.NextVolume(10));
            }
            Console.WriteLine("MarketData with {0} symbols", _market.Length);
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
            int bytesOut = _socket.Send(data, offset, length, SocketFlags.None);
            if (bytesOut > 0) {
                Interlocked.Add(ref _bytesTransmitted, bytesOut);
            }
        }

        private void WriteWithUDP(byte[] data, int offset, int length)
        {
            int bytesOut = _socket.SendTo(data, offset, length, SocketFlags.None, _endPoint);
            if (bytesOut > 0) {
                Interlocked.Add(ref _bytesTransmitted, bytesOut);
            }
        }

        public const int Kbps = 1024;
        public const int Mbps = 1024*Kbps;
        public const int Gbps = 1024*Mbps;

        public static String RenderByteCount(long bytes)
        {
            double bitCount = bytes*8;

            //Console.WriteLine("bytes: {0}, bits: {1} / {2} / {3}",
            //    bytes,
            //    bitCount,
            //    bitCount / 1024,
            //    bitCount / 1024 / 1024
            //    );

            if (bitCount < 5 * Kbps)
            {
                return String.Format("{0} bps", bitCount);
            }
            if (bitCount < 5 * Mbps)
            {
                return String.Format("{0:F2} Kbps", bitCount / Kbps);
            }

            return String.Format("{0:F2} Mbps", bitCount / Mbps);
        }

        public void Run()
        {
            switch( _client.ipMode )
            {
                case IPMode.TCP:
                    _tchannel = new TcpClient(_client.host, _client.port);
                    Console.WriteLine("Client connected using TCP to {0}:{1}, rate {2} msg/s",
                                      _client.host,
                                      _client.port,
                                      _client.rate);
                    _socket = _tchannel.Client;
                    _socket.ReceiveBufferSize = 65536;
                    _socket.SendBufferSize = 65536*8;
                    _socket.DontFragment = false;
                    _socket.NoDelay = true;

                    Console.WriteLine("SocketStatistics:");
                    Console.WriteLine("\tDontFragment: {0}", _socket.DontFragment);
                    Console.WriteLine("\tNoDelay: {0}", _socket.NoDelay);
                    Console.WriteLine("\tReceiveBufferSize: {0}", _socket.ReceiveBufferSize);
                    Console.WriteLine("\tReceiveTimeout: {0}", _socket.ReceiveTimeout);
                    Console.WriteLine("\tSendBufferSize: {0}", _socket.SendBufferSize);
                    Console.WriteLine("\tSendTimeout: {0}", _socket.SendTimeout);
                    break;
                case IPMode.UDP:
                    Console.WriteLine("Client sending using UDP to {0}:{1}, rate {2} msg/s",
                                      _client.host,
                                      _client.port,
                                      _client.rate);

                    _uchannel = new UdpClient(_client.host, _client.port);
                    _socket = _uchannel.Client;
                    _socket.ReceiveBufferSize = 65536*8;
                    _socket.SendBufferSize = 65536*8;
                    _socket.DontFragment = false;

                    Console.WriteLine("SocketStatistics:");
                    Console.WriteLine("\tDontFragment: {0}", _socket.DontFragment);
                    Console.WriteLine("\tReceiveBufferSize: {0}", _socket.ReceiveBufferSize);
                    Console.WriteLine("\tReceiveTimeout: {0}", _socket.ReceiveTimeout);
                    Console.WriteLine("\tSendBufferSize: {0}", _socket.SendBufferSize);
                    Console.WriteLine("\tSendTimeout: {0}", _socket.SendTimeout);
                    break;
            }

            _eventPer50Ms = _client.rate / 20;
            _tickerIndex = 0;
            _countLast5S = 0;
            _sleepLast5S = 0;
            _lastThroughputTick = Environment.TickCount;
            _eventCount = 0;
            _maxEventCount = _client.totalEvents;

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

        private int _eventPer50Ms;
        private int _tickerIndex;
        private int _countLast5S;
        private long _sleepLast5S;
        private long _lastThroughputTick;
        private long _eventCount;
        private long _maxEventCount;

        private void HandleTimerEvents(object userData)
        {
            for (int i = 0; i < _eventPer50Ms; i++) {
                if (_eventCount++ >= _maxEventCount) {
                    break;
                }

                _tickerIndex = _tickerIndex%Symbols.SYMBOLS.Length;
                MarketData md = _market[_tickerIndex++];
                md.Price = Symbols.NextPrice(md.Price);
                md.Volume = Symbols.NextVolume(10);
                md.Time = DateTime.Now.Ticks;

                _bytesTransmitted += _sendMethod.Invoke(md);

                _countLast5S++;
            }

            // info
            int tickCount = Environment.TickCount;
            if ((tickCount - _lastThroughputTick) > 5000) {
                long mByteCount = Interlocked.Exchange(ref _bytesTransmitted, 0L);
                long mDelta = tickCount - _lastThroughputTick;
                Console.WriteLine(
                    "Sent {0} in {1}(ms) avg ns/msg {2}(ns) avg {3}(msg/s) sleep {4}(ms) velocity {5}",
                    _countLast5S,
                    mDelta,
                    (float) 1E6*_countLast5S/(tickCount - _lastThroughputTick),
                    _countLast5S/5,
                    _sleepLast5S,
                    RenderByteCount(mByteCount*1000/mDelta));

                _countLast5S = 0;
                _sleepLast5S = 0;
                _lastThroughputTick = tickCount;
            }
        }

        private delegate int SendDelegate(MarketData mdEvent);
    }
} // End of namespace
