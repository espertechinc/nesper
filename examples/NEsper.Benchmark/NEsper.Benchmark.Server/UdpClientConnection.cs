///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Net.Sockets;

namespace NEsper.Benchmark.Server
{
    public class UdpClientConnection : ClientConnection
    {
        private readonly Socket _socket;
        private readonly byte[] _dataArray;

        /// <summary>
        /// Initializes a new instance of the <see cref="UdpClientConnection"/> class.
        /// </summary>
        /// <param name="socket">The socket channel.</param>
        /// <param name="executor">The executor.</param>
        /// <param name="cepProvider">The cep provider.</param>
        /// <param name="statSec">The stat sec.</param>
	    public UdpClientConnection(Socket socket, Executor executor, CEPProvider.ICEPProvider cepProvider, int statSec)
            : base(executor, cepProvider, statSec)
        {
            _dataArray = new byte[64 * 1024]; // maximum udp datagram is 64k

            _socket = socket;
            _socket.BeginReceive(_dataArray, 0, _dataArray.Length, SocketFlags.None, ReceiveCallback, null);
        }

        /// <summary>
        /// Receives the callback.
        /// </summary>
        /// <param name="ar">The ar.</param>
        private void ReceiveCallback( IAsyncResult ar )
        {
            try
            {
                SocketError errorCode;
                var bytesReceived = _socket.EndReceive(ar, out errorCode);
                if (bytesReceived > 0) {
                    DataAssembler.Deserialize(_dataArray, 0, bytesReceived);
                }
            }
            catch (IOException e)
            {
                Console.Error.WriteLine("Error receiving data from client. Did client disconnect?");
                Console.Error.WriteLine("Error message: {0}", e.Message);
                Console.Error.WriteLine(e.StackTrace);
            }

            _socket.BeginReceive(_dataArray, 0, _dataArray.Length, SocketFlags.None, ReceiveCallback, null);
        }
    }
}
