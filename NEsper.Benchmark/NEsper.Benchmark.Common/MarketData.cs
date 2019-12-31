///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;

using com.espertech.esper.compat;

namespace NEsper.Benchmark.Common
{
    /// <summary>
    /// The actual event.
    /// The time property (ms) is the send time from the client sender, and can be used for end to end latency providing Client(s)
    /// and server OS clocks are in sync.
    /// The inTime property is the unmarshal (local) time (ns).
    /// </summary>
    /// <author>Alexandre Vasseur http://avasseur.blogspot.com</author>
	public class MarketData
    {
        public static readonly int SIZE = Symbols.SIZE + 8 + 4 + 8;
        private static readonly Encoding encoding = Encoding.ASCII;

        static MarketData()
        {
            int bytesPerEvent = SIZE + 4; // Add in the packing header
            int bitsPerEvent = bytesPerEvent*8;
            const int megaBit = 1024*1024;
            const int gigaBit = 1024*megaBit;

            Console.WriteLine("MarketData event = {0} bits = {1} bytes", bitsPerEvent, bytesPerEvent);
            Console.WriteLine("  100 Mbit/s {1,10} <==> {0}k events/s", 100*megaBit/bitsPerEvent/1000, 100*megaBit);
            Console.WriteLine("    1 Gbit/s {1,10} <==> {0}k events/s", gigaBit/bitsPerEvent/1000, gigaBit);
	    }

	    private String ticker;
	    private double price;
	    private int volume;

	    private long time;//ms
	    private readonly long inTime;

        public MarketData(String ticker, double price, int volume, long time)
        {
            this.ticker = ticker;
            this.price = price;
            this.volume = volume;
            this.time = time;
            this.inTime = HighResolutionTimeProvider.Instance.CurrentTime;
        }

	    public MarketData(String ticker, double price, int volume)
        {
	        this.ticker = ticker;
	        this.price = price;
	        this.volume = volume;
            this.time = this.inTime = HighResolutionTimeProvider.Instance.CurrentTime;
        }

        public string Ticker
        {
            get { return ticker; }
            set { ticker = value; }
        }

        public double Price
        {
            get { return price; }
            set { price = value; }
        }

        public int Volume
        {
            get { return volume; }
            set { volume = value; }
        }

        public long Time
        {
            get { return time; }
            set { time = value; }
        }

        public long InTime
        {
            get { return inTime; }
        }

        public static byte[] SerializeAsArray(MarketData md)
        {
            var bTicker = new byte[Symbols.SIZE];
            encoding.GetBytes(md.ticker, 0, md.ticker.Length, bTicker, 0);

            byte[] bPrice = BitConverter.GetBytes(md.Price);
            byte[] bVolume = BitConverter.GetBytes(md.Volume);
            byte[] bTickCount = BitConverter.GetBytes(md.Time);

            var mdArray = new byte[SIZE];
            Array.Copy(bTicker, 0, mdArray, 0, Symbols.SIZE);
            Array.Copy(bPrice, 0, mdArray, Symbols.SIZE, 8);
            Array.Copy(bVolume, 0, mdArray, Symbols.SIZE + 8, 4);
            Array.Copy(bTickCount, 0, mdArray, Symbols.SIZE + 12, 8);

            //Console.WriteLine("{0}", md.Time);

            return mdArray;
        }

        public static MarketData Deserialize(byte[] mdArray)
        {
            var md = new MarketData(
                encoding.GetString(mdArray, 0, Symbols.SIZE),
                BitConverter.ToDouble(mdArray, Symbols.SIZE),
                BitConverter.ToInt32(mdArray, Symbols.SIZE + 8),
                BitConverter.ToInt64(mdArray, Symbols.SIZE + 12));

            return md;
        }

        public static IList<ArraySegment<byte>> CreateArraySegment()
        {
            return new[]
                {
                    new ArraySegment<byte>(new byte[Symbols.SIZE]),
                    new ArraySegment<byte>(new byte[20])
                };
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString()
        {
            return string.Format("Ticker: {0}, Price: {1}, Volume: {2}, Time: {3}, InTime: {4}", ticker, price, volume, time, inTime);
        }

        //public override String ToString()
        //{
        //    return ticker+" : "+time+" : "+price+" : "+volume;
        //}

	    public Object Clone()
        {
	        return new MarketData(ticker, price, volume);
	    }
	}

    public delegate void MarketDataEventHandler(Object sender, MarketData e);

} // End of namespace
