namespace NEsper.Examples.OHLC
{
    public class OHLCBarValue
    {
        private long MinuteValue { get; }
        private double? First { get; }
        private double? Last { get; }
        private double? Max { get; }
        private double? Min { get; }

        public OHLCBarValue(
            long minuteValue,
            double? first,
            double? last,
            double? max,
            double? min)
        {
            MinuteValue = minuteValue;
            First = first;
            Last = last;
            Max = max;
            Min = min;
        }
    }
}
