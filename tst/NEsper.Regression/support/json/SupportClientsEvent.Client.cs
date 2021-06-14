using System;
using System.Collections.Generic;

namespace com.espertech.esper.regressionlib.support.json
{
    public partial class SupportClientsEvent
    {
        public class Client {

            public long _id;
            public int index;
            public Guid guid;
            public bool isActive;
            public decimal balance;
            public string picture;
            public int age;
            public EyeColor eyeColor;
            public string name;
            public string gender;
            public string company;
            public string[] emails;
            public long[] phones;
            public string address;
            public string about;
            public DateTimeOffset registered;
            public double latitude;
            public double longitude;
            public IList<string> tags;
            public IList<Partner> partners;

            protected bool Equals(Client other)
            {
                return _id == other._id &&
                       index == other.index &&
                       guid.Equals(other.guid) &&
                       isActive == other.isActive &&
                       balance == other.balance &&
                       picture == other.picture &&
                       age == other.age &&
                       eyeColor == other.eyeColor &&
                       name == other.name &&
                       gender == other.gender &&
                       company == other.company &&
                       Equals(emails, other.emails) &&
                       Equals(phones, other.phones) &&
                       address == other.address &&
                       about == other.about &&
                       registered.Equals(other.registered) &&
                       (Math.Abs(latitude - other.latitude) < 3) &&
                       (Math.Abs(longitude - other.longitude) < 3) &&
                       Equals(tags, other.tags) &&
                       Equals(partners, other.partners);
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

                return Equals((Client) obj);
            }

            public override int GetHashCode()
            {
                unchecked {
                    var hashCode = _id.GetHashCode();
                    hashCode = (hashCode * 397) ^ index;
                    hashCode = (hashCode * 397) ^ guid.GetHashCode();
                    hashCode = (hashCode * 397) ^ isActive.GetHashCode();
                    hashCode = (hashCode * 397) ^ balance.GetHashCode();
                    hashCode = (hashCode * 397) ^ (picture != null ? picture.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ age;
                    hashCode = (hashCode * 397) ^ (int) eyeColor;
                    hashCode = (hashCode * 397) ^ (name != null ? name.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (gender != null ? gender.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (company != null ? company.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (emails != null ? emails.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (phones != null ? phones.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (address != null ? address.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (about != null ? about.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ registered.GetHashCode();
                    hashCode = (hashCode * 397) ^ latitude.GetHashCode();
                    hashCode = (hashCode * 397) ^ longitude.GetHashCode();
                    hashCode = (hashCode * 397) ^ (tags != null ? tags.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (partners != null ? partners.GetHashCode() : 0);
                    return hashCode;
                }
            }
        }
    }
}