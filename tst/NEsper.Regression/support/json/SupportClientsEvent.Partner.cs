using System;

namespace com.espertech.esper.regressionlib.support.json
{
    public partial class SupportClientsEvent
    {
        public class Partner {

            public long id;
            public string name;
            public DateTimeOffset since;

            public Partner() {
            }

            public static Partner Create(long id, string name, DateTimeOffset since) {
                Partner partner = new Partner();
                partner.id = id;
                partner.name = name;
                partner.since = since;
                return partner;
            }

            protected bool Equals(Partner other)
            {
                return id == other.id && name == other.name && since.Equals(other.since);
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

                return Equals((Partner) obj);
            }

            public override int GetHashCode()
            {
                unchecked {
                    var hashCode = id.GetHashCode();
                    hashCode = (hashCode * 397) ^ (name != null ? name.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ since.GetHashCode();
                    return hashCode;
                }
            }
        }
    }
}