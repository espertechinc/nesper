using System;
using System.Collections.Generic;

namespace com.espertech.esper.regressionlib.support.json
{
    public partial class SupportUsersEvent
    {
        public class User
        {
            public string _id;
            public string about;
            public string address;
            public int age;
            public string balance;
            public string company;
            public string email;
            public string eyeColor;
            public string favoriteFruit;
            public IList<Friend> friends;
            public string gender;
            public string greeting;
            public string guid;
            public int index;
            public bool isActive;
            public double latitude;
            public double longitude;
            public string name;
            public string phone;
            public string picture;
            public string registered;
            public IList<string> tags;

            protected bool Equals(User other)
            {
                return _id == other._id &&
                       index == other.index &&
                       guid == other.guid &&
                       isActive == other.isActive &&
                       balance == other.balance &&
                       picture == other.picture &&
                       age == other.age &&
                       eyeColor == other.eyeColor &&
                       name == other.name &&
                       gender == other.gender &&
                       company == other.company &&
                       email == other.email &&
                       phone == other.phone &&
                       address == other.address &&
                       about == other.about &&
                       registered == other.registered &&
                       Math.Abs(latitude - other.latitude) < 3 &&
                       Math.Abs(longitude - other.longitude) < 3 &&
                       Equals(tags, other.tags) &&
                       Equals(friends, other.friends) &&
                       greeting == other.greeting &&
                       favoriteFruit == other.favoriteFruit;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) {
                    return false;
                }

                if (ReferenceEquals(this, obj)) {
                    return true;
                }

                if (obj.GetType() != GetType()) {
                    return false;
                }

                return Equals((User) obj);
            }

            public override int GetHashCode()
            {
                unchecked {
                    var hashCode = _id != null ? _id.GetHashCode() : 0;
                    hashCode = (hashCode * 397) ^ index;
                    hashCode = (hashCode * 397) ^ (guid != null ? guid.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ isActive.GetHashCode();
                    hashCode = (hashCode * 397) ^ (balance != null ? balance.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (picture != null ? picture.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ age;
                    hashCode = (hashCode * 397) ^ (eyeColor != null ? eyeColor.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (name != null ? name.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (gender != null ? gender.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (company != null ? company.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (email != null ? email.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (phone != null ? phone.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (address != null ? address.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (about != null ? about.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (registered != null ? registered.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ latitude.GetHashCode();
                    hashCode = (hashCode * 397) ^ longitude.GetHashCode();
                    hashCode = (hashCode * 397) ^ (tags != null ? tags.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (friends != null ? friends.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (greeting != null ? greeting.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (favoriteFruit != null ? favoriteFruit.GetHashCode() : 0);
                    return hashCode;
                }
            }
        }
    }
}