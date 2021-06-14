using System;

namespace NEsper.Examples.OHLC
{
    public class OHLCTick
    {
        private string Ticker { get; }
        private double Price { get; }
        private long Timestamp { get; }

        public OHLCTick(
            string ticker,
            double price,
            long timestamp)
        {
            Ticker = ticker;
            Price = price;
            Timestamp = timestamp;
        }

        protected bool Equals(OHLCTick other)
        {
            return Ticker == other.Ticker && Price.Equals(other.Price) && Timestamp == other.Timestamp;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != this.GetType()) {
                return false;
            }

            return Equals((OHLCTick) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Ticker, Price, Timestamp);
        }

        public override string ToString()
        {
            return $"{nameof(Ticker)}: {Ticker}, {nameof(Price)}: {Price}, {nameof(Timestamp)}: {Timestamp}";
        }
    }
}
