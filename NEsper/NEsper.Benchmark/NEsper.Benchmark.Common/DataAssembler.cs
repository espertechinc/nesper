///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Threading;

namespace NEsper.Benchmark.Common
{
    public class DataAssembler
    {
        /// <summary>
        /// Gets or sets the length of the max message.
        /// </summary>
        /// <value>The length of the max message.</value>
        public int MaxMessageLength { get; set; }

        /// <summary>
        /// Gets or sets the max message delay.
        /// </summary>
        /// <value>The max message delay.</value>
        private int maxMessageDelay;

        /// <summary>
        /// Gets or sets the max message delay.
        /// </summary>
        /// <value>The max message delay.</value>
        public int MaxMessageDelay
        {
            get { return maxMessageDelay; }
            set
            {
                maxMessageDelay = value;
                maxMessageDelayTimer = new Timer(
                    DelayFlushTimer,
                    null,
                    value,
                    value);
            }
        }

        private Timer maxMessageDelayTimer;

        /// <summary>
        /// Output memory buffer.
        /// </summary>
        private readonly ByteBuffer outMemBuffer =
            new ByteBuffer(8192 * 64);

        /// <summary>
        /// Input memory buffer.
        /// </summary>
        private readonly ByteBuffer inMemBuffer =
            new ByteBuffer(8192 * 64);
            //new ByteBuffer(65536 * 256);

        /// <summary>
        /// Occurs when a message needs to be written.
        /// </summary>
        public event ByteWriter WriteMessage;

        /// <summary>
        /// Occurs when a market data event arrives.
        /// </summary>
        public event MarketDataEventHandler MarketDataEvent;

        /// <summary>
        /// Flush stream latch is used to ensure that only a single person
        /// is flushing the outbound stream at a time.
        /// </summary>
        private int flushStreamLatch = 1;

        /// <summary>
        /// Delays the flush timer.
        /// </summary>
        /// <param name="userData">The user data.</param>
        private void DelayFlushTimer(Object userData)
        {
            FlushStream();
        }

        /// <summary>
        /// Creates an MD5 hash.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        private static byte[] CreateHashMD5(byte[] data)
        {
            return ByteUtil.ComputeMD5Hash(data, 0, data.Length);
        }

        /// <summary>
        /// Creates a null hash.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        private static byte[] CreateHashNone(byte[] data)
        {
            return null;
        }

        /// <summary>
        /// Flushes the stream.
        /// </summary>
        private void FlushStream()
        {
            if (Interlocked.CompareExchange(ref flushStreamLatch, 0, 1) == 1)
            {
                try
                {
                    lock (outMemBuffer)
                    {
                        if (outMemBuffer.Length == 0)
                        {
                            return;
                        }

                        // Datagrams need to be shipped intact and as
                        // complete packages or we end up with fragmentation
                        // which is unhealthy for reassembly.

                        if ( WriteMessage != null ) {
                            byte[] data = outMemBuffer.ReadAll();
                            byte[] hash = hashFactory(data);
                            // Create a second buffer to copy the hash and data into
                            byte[] wContainer;
                            if (hash == null) {
                                wContainer = new byte[4 + data.Length];
                                Array.Copy(BitConverter.GetBytes(0), 0, wContainer, 0, 4);
                                Array.Copy(data, 0, wContainer, 4, data.Length);
                            } else {
                                wContainer = new byte[ 4 + data.Length + hash.Length];
                                Array.Copy(BitConverter.GetBytes(hash.Length), 0, wContainer, 0, 4);
                                Array.Copy(hash, 0, wContainer, 4, hash.Length);
                                Array.Copy(data, 0, wContainer, 4 + hash.Length, data.Length);
                            }

                            // End Sanity Check
                            WriteMessage(wContainer, 0, wContainer.Length);
                        }

                        outMemBuffer.Reset();
                    }
                }
                finally
                {
                    Interlocked.Exchange(ref flushStreamLatch, 1);
                }
            }
        }

        /// <summary>
        /// Checks the stream to see if we have exceeded the buffer limit.
        /// </summary>
        /// <param name="numBytes">The num bytes.</param>
        private void CheckStreamOrFlush(int numBytes)
        {
            long maxSize = MaxMessageLength;
            long curSize = outMemBuffer.Length;
            long extSize = curSize + numBytes;
            if (extSize > maxSize)
            {
                FlushStream();
            }
        }

        private int dataIndex;

        /// <summary>
        /// Decodes the available messages.
        /// </summary>
        private void DecodeMessages()
        {
            do
            {
                if (inMemBuffer.Length < 8)
                {
                    break; // not enough information is available
                }

                //Console.WriteLine("MB> {0}", ByteUtil.ToHexString(inMemBuffer.ReadAll(false)));

                int eMessageSize = ReadInt32(
                    inMemBuffer[0],
                    inMemBuffer[1],
                    inMemBuffer[2],
                    inMemBuffer[3]);
                int eMessageSequence = ReadInt32(
                    inMemBuffer[4],
                    inMemBuffer[5],
                    inMemBuffer[6],
                    inMemBuffer[7]);

                dataIndex++;

                if (inMemBuffer.Length < (eMessageSize + 8))
                {
                    Console.WriteLine("DataAssembler corruption @ {2}: {0} / {1}", inMemBuffer.Length, eMessageSize + 4, dataIndex);
                    break;
                }

                inMemBuffer.UnsafeAdvance(8);

                // inMemoryStream indicates that the entire message is available!
                var eMessageBody = new byte[eMessageSize];
                inMemBuffer.Read(eMessageBody, 0, eMessageSize);

                // Push the payload into the inboundMessageQueue
                if (MarketDataEvent != null) {
                    MarketData mdEvent = MarketData.Deserialize(eMessageBody);
                    MarketDataEvent(this, mdEvent);
                }
            } while (true);
        }

        /// <summary>
        /// Writes the market data event.
        /// </summary>
        /// <param name="mdEvent">The md event.</param>
        public int Serialize(MarketData mdEvent)
        {
            byte[] data = MarketData.SerializeAsArray(mdEvent);
            // Create the message header

            int eMessageSequence = 0;
            int eMessageSize = data.Length;
            byte[] eMessageHeader = new byte[8];

            // Check the stream and flush if necessary before adding
            // the current buffer.
            CheckStreamOrFlush(eMessageSize + 8);

            lock (outMemBuffer) {
                // Write size into the next 3-octets
                eMessageHeader[0] = (byte) (eMessageSize & 0x000000ff);
                eMessageHeader[1] = (byte) ((eMessageSize & 0x0000ff00) >> 8);
                eMessageHeader[2] = (byte) ((eMessageSize & 0x00ff0000) >> 16);
                eMessageHeader[3] = (byte) ((eMessageSize & 0xff000000) >> 24);

                eMessageHeader[4] = (byte) (eMessageSequence & 0x000000ff);
                eMessageHeader[5] = (byte) ((eMessageSequence & 0x0000ff00) >> 8);
                eMessageHeader[6] = (byte) ((eMessageSequence & 0x00ff0000) >> 16);
                eMessageHeader[7] = (byte) ((eMessageSequence & 0xff000000) >> 32);

                outMemBuffer.Write(eMessageHeader, 0, 8);
                outMemBuffer.Write(data, 0, data.Length);
            }

            // Check the stream again
            CheckStreamOrFlush(0);
            // Return the bytes transmitted
            return data.Length;
        }

        /// <summary>
        /// Applies an MD5 hash checking algorithm.
        /// </summary>
        /// <param name="dataHash">The data hash.</param>
        /// <param name="data">The data.</param>

        private static void TestHashMD5(byte[] dataHash, byte[] data)
        {
            byte[] realHash = ByteUtil.ComputeMD5Hash(data, 0, data.Length);

            var _realHash = ByteUtil.ToHexString(realHash);
            var _dataHash = ByteUtil.ToHexString(dataHash);
            if (_realHash != _dataHash)
            {
                throw new ApplicationException("Incoming data packet did not pass hash validation");
            }
        }

        /// <summary>
        /// Applies a null hash check.
        /// </summary>
        /// <param name="hash">The hash.</param>
        /// <param name="data">The data.</param>
        private static void TestHashNone(byte[] hash, byte[] data)
        {
        }

        public void Deserialize( byte[] packet, int offset, int length )
        {
            try {
                int hashLength = BitConverter.ToInt32(packet, offset);
                // Get the hash
                byte[] hash = ByteUtil.Extract(packet, offset + 4, hashLength);
                byte[] data = ByteUtil.Extract(packet, offset + 4 + hashLength, length - offset - 4 - hashLength);
                // Check the data against the hash
                hashTester(hash, data);
                // Write the contents to the memory buffer
                inMemBuffer.Write(data, 0, data.Length);
                // Deserialize any messages that are ready to be processed
                DecodeMessages();
            } catch( ApplicationException e ) {
                Console.WriteLine("ERROR: {0}" + e.Message);
            }
        }

        public static Int32 ReadInt32(byte a, byte b, byte c, byte d)
        {
            return (a | b << 8 | c << 16 | d << 24);
        }

        private readonly HashFactory hashFactory = CreateHashMD5;
        private readonly HashTester hashTester = TestHashMD5;
    }

    /// <summary>
    /// Encodes and returns a hash.
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public delegate byte[] HashFactory(byte[] data);

    /// <summary>
    /// Decodes and tests a hash.
    /// </summary>
    /// <param name="data"></param>
    public delegate void HashTester(byte[] hash, byte[] data);
}
