///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.client.context
{
    /// <summary>Context partition identifier for hash context. </summary>
    [Serializable]
    public class ContextPartitionIdentifierHash : ContextPartitionIdentifier
    {
        private int _hash;

        /// <summary>Ctor. </summary>
        public ContextPartitionIdentifierHash()
        {
        }
    
        /// <summary>Ctor. </summary>
        /// <param name="hash">code</param>
        public ContextPartitionIdentifierHash(int hash)
        {
            _hash = hash;
        }

        /// <summary>Returns the hash code. </summary>
        /// <value>hash code</value>
        public int Hash
        {
            get { return _hash; }
            set { _hash = value; }
        }

        public override bool CompareTo(ContextPartitionIdentifier other)
        {
            return other is ContextPartitionIdentifierHash && _hash == ((ContextPartitionIdentifierHash) other)._hash;
        }

        public override String ToString()
        {
            return "ContextPartitionIdentifierHash{" +
                    "hash=" + _hash +
                    '}';
        }
    }
}
