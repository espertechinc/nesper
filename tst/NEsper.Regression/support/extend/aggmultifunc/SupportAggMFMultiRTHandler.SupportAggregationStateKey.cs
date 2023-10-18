using com.espertech.esper.common.client.hook.aggmultifunc;

namespace com.espertech.esper.regressionlib.support.extend.aggmultifunc
{
    public partial class SupportAggMFMultiRTHandler
    {
        private class SupportAggregationStateKey : AggregationMultiFunctionStateKey {
            public string Id { get; }

            internal SupportAggregationStateKey(string id) {
                this.Id = id;
            }

            protected bool Equals(SupportAggregationStateKey other)
            {
                return Id == other.Id;
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

                return Equals((SupportAggregationStateKey)obj);
            }

            public override int GetHashCode()
            {
                return (Id != null ? Id.GetHashCode() : 0);
            }
        }
    }
}