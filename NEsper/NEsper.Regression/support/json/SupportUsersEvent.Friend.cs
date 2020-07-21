namespace com.espertech.esper.regressionlib.support.json
{
    public partial class SupportUsersEvent
    {
        public class Friend
        {
            public string id;
            public string name;

            public static Friend Create(
                string id,
                string name)
            {
                var friend = new Friend();
                friend.id = id;
                friend.name = name;
                return friend;
            }

            protected bool Equals(Friend other)
            {
                return id == other.id && name == other.name;
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

                return Equals((Friend) obj);
            }

            public override int GetHashCode()
            {
                unchecked {
                    return ((id != null ? id.GetHashCode() : 0) * 397) ^ (name != null ? name.GetHashCode() : 0);
                }
            }
        }
    }
}