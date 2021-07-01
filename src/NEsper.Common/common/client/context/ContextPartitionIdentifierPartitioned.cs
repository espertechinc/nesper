///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.client.context
{
    /// <summary>
    /// Context partition identifier for segmented contexts.
    /// </summary>
    [Serializable]
    public class ContextPartitionIdentifierPartitioned : ContextPartitionIdentifier
    {
        private object[] _keys;

        /// <summary>Ctor. </summary>
        public ContextPartitionIdentifierPartitioned()
        {
        }

        /// <summary>Ctor. </summary>
        /// <param name="keys">partitioning keys</param>
        public ContextPartitionIdentifierPartitioned(object[] keys)
        {
            _keys = keys;
        }

        /// <summary>Returns the partition keys. </summary>
        /// <value>keys</value>
        public object[] Keys {
            get { return _keys; }
            set { _keys = value; }
        }

        public override bool CompareTo(ContextPartitionIdentifier other)
        {
            if (!(other is ContextPartitionIdentifierPartitioned)) {
                return false;
            }

            return Collections.AreEqual(_keys, ((ContextPartitionIdentifierPartitioned) other)._keys);
        }

        public override string ToString()
        {
            return "ContextPartitionIdentifierPartitioned{" +
                   "keys=" +
                   (_keys) +
                   '}';
        }
    }
}