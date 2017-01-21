///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.util;

namespace com.espertech.esper.compat
{
    [Serializable]
    public class Blob
    {
        private readonly byte[] _data;
        private readonly int _hash;

        /// <summary>
        /// Gets the data.
        /// </summary>
        /// <value>
        /// The data.
        /// </value>
        public byte[] Data
        {
            get { return _data; }
        }

        /// <summary>
        /// Gets the hash.
        /// </summary>
        /// <value>
        /// The hash.
        /// </value>
        public int Hash
        {
            get { return _hash; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Blob"/> class.
        /// </summary>
        /// <param name="data">The data.</param>
        public Blob(byte[] data)
        {
            _data = data;
            _hash = MurmurHash.Hash(data, 0, data.Length, 0);
        }

        protected bool Equals(Blob other)
        {
            if (_hash != other._hash)
                return false;
            if (_data.Length != other._data.Length)
                return false;

            unchecked
            {
                int length = _data.Length;
                for (int ii = 0; ii < length; ii++)
                {
                    if (_data[ii] != other._data[ii])
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != this.GetType())
                return false;
            return Equals((Blob) obj);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return _hash;
            }
        }
    }
}
