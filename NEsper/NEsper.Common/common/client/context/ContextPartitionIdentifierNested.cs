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
    /// <summary>
    /// Context partition identifier for nested contexts.
    /// </summary>
    [Serializable]
    public class ContextPartitionIdentifierNested : ContextPartitionIdentifier
    {
        private ContextPartitionIdentifier[] _identifiers;

        /// <summary>Ctor. </summary>
        public ContextPartitionIdentifierNested() {
        }
    
        /// <summary>Ctor. </summary>
        /// <param name="identifiers">nested identifiers, count should match nesting level of context</param>
        public ContextPartitionIdentifierNested(ContextPartitionIdentifier[] identifiers) {
            _identifiers = identifiers;
        }

        /// <summary>Returns nested partition identifiers. </summary>
        /// <value>identifiers</value>
        public ContextPartitionIdentifier[] Identifiers
        {
            get { return _identifiers; }
            set { this._identifiers = value; }
        }

        public override bool CompareTo(ContextPartitionIdentifier other) {
            if (!(other is ContextPartitionIdentifierNested)) {
                return false;
            }
            var nestedOther = (ContextPartitionIdentifierNested) other;
            if (nestedOther.Identifiers.Length != _identifiers.Length) {
                return false;
            }
            for (int i = 0; i < _identifiers.Length; i++) {
                if (!_identifiers[i].CompareTo(nestedOther.Identifiers[i])) {
                    return false;
                }
            }
            return true;
        }
    
        public override String ToString() {
            return "ContextPartitionIdentifierNested{" +
                    "identifiers=" + (_identifiers) +
                    '}';
        }
    }
}
