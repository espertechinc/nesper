///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace com.espertech.esper.compat.collections
{
    public partial class OrderedListDictionary<TK, TV>
    {
        internal class KeyValuePairComparer : IComparer<KeyValuePair<TK, TV>>
        {
            private readonly IComparer<TK> _keyComparer;
            private readonly bool _invert;

            public KeyValuePairComparer(IComparer<TK> keyComparer, bool invert)
            {
                _keyComparer = keyComparer;
                _invert = invert;
            }

            public int Compare(KeyValuePair<TK, TV> x, KeyValuePair<TK, TV> y)
            {
                return _invert
                    ? -_keyComparer.Compare(x.Key, y.Key)
                    : _keyComparer.Compare(x.Key, y.Key);
            }

            public KeyValuePairComparer Invert()
            {
                return new KeyValuePairComparer(_keyComparer, !_invert);
            }

            public IComparer<TK> KeyComparer => _invert ? _keyComparer.Inverse() : _keyComparer;
        }
    }
}