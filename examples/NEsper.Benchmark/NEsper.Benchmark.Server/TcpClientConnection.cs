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
using System.Threading;

namespace NEsper.Benchmark.Server
{
    public class TcpClientConnection : ClientConnection
    {
        private readonly Socket _socket;
        private readonly Thread _readThread;

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpClientConnection"/> class.
        /// </summary>
        /// <param name="socketChannel">The socket channel.</param>
        /// <param name="executor">The executor.</param>
        /// <param name="cepProvider">The cep provider.</param>
        /// <param name="statSec">The stat sec.</param>
	    public TcpClientConnection(TcpClient socketChannel, Executor executor, CEPProvider.ICEPProvider cepProvider, int statSec)
            : base(executor, cepProvider, statSec)
        {
            _socket = socketChannel.Client;

            _readThread = new Thread(ProcessConnection);
            _readThread.Name = "EsperServer-cnx-" + MyID;
        }

        /// <summary>
        /// Starts this instance.
        /// </summary>
        public override void Start()
        {
            base.Start();

            _readThread.Start();
        }

        /// <summary>
        /// Stops this instance.
        /// </summary>
        public override void Stop()
        {
            base.Stop();

            _readThread.Join();
        }

        /// <summary>
        /// Processes the connection.
        /// </summary>
        public void ProcessConnection()
        {
            try
            {
                var dataBuffer = new byte[65536];

                while (true)
                {
                    var byteCount = _socket.Receive(dataBuffer);
                    if ( byteCount > 0 ) {
                        DataAssembler.Deserialize(dataBuffer, 0, byteCount);
                    }
                }
            }
            catch (IOException e)
            {
                Console.Error.WriteLine("Error receiving data from client. Did client disconnect?");
                Console.Error.WriteLine("Error message: {0}", e.Message);
                Console.Error.WriteLine(e.StackTrace);
            }
            finally
            {
                _socket.Close();
                RemoveSelf();
                StatsHolder.Remove(StatsHolder.Engine);
                StatsHolder.Remove(StatsHolder.Server);
                StatsHolder.Remove(StatsHolder.EndToEnd);
            }
        }
    }
}
