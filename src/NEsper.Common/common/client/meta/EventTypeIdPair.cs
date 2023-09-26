///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.client.meta
{
    /// <summary>
    ///     Pair of public and protected event type id.
    ///     <para />
    ///     Preconfigured event types only have a public id. Their public id is derived from the event type name.
    ///     <para />
    ///     All other event types have a public id and protected id.
    ///     Their public id is derived from the deployment id.
    ///     Their protected id is derived from the event type name.
    /// </summary>
    public class EventTypeIdPair
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="publicId">public id</param>
        /// <param name="protectedId">protected if</param>
        public EventTypeIdPair(
            long publicId,
            long protectedId)
        {
            PublicId = publicId;
            ProtectedId = protectedId;
        }

        /// <summary>
        ///     Returns the public id
        /// </summary>
        /// <returns>public id</returns>
        public long PublicId { get; }

        /// <summary>
        ///     Returns the protected id
        /// </summary>
        /// <returns>protected id</returns>
        public long ProtectedId { get; }

        public override bool Equals(object o)
        {
            if (this == o) {
                return true;
            }

            if (o == null || GetType() != o.GetType()) {
                return false;
            }

            var that = (EventTypeIdPair)o;

            if (PublicId != that.PublicId) {
                return false;
            }

            return ProtectedId == that.ProtectedId;
        }

        public override int GetHashCode()
        {
            unchecked {
                return (PublicId.GetHashCode() * 397) ^ ProtectedId.GetHashCode();
            }
        }

        /// <summary>
        ///     Returns an unassigned value that has -1 as the public and protected id
        /// </summary>
        /// <returns>pair with unassigned (-1) values</returns>
        public static EventTypeIdPair Unassigned()
        {
            return new EventTypeIdPair(-1, -1);
        }
    }
} // end of namespace