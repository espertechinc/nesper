namespace com.espertech.esper.regressionlib.suite.resultset.aggregate
{
    public partial class ResultSetAggregationMethodSorted
    {
        public class MySubmapEvent
        {
            private readonly int fromKey;
            private readonly bool fromInclusive;
            private readonly int toKey;
            private readonly bool toInclusive;

            public MySubmapEvent(
                int fromKey,
                bool fromInclusive,
                int toKey,
                bool toInclusive)
            {
                this.fromKey = fromKey;
                this.fromInclusive = fromInclusive;
                this.toKey = toKey;
                this.toInclusive = toInclusive;
            }

            public int FromKey => fromKey;

            public bool IsFromInclusive => fromInclusive;

            public int ToKey => toKey;

            public bool IsToInclusive => toInclusive;
        }
    }
}