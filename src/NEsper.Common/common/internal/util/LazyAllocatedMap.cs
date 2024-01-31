///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace com.espertech.esper.common.@internal.util
{
    public class LazyAllocatedMap<TK, TV>
    {
        private IDictionary<TK, TV> _inner;

        public IDictionary<TK, TV> Map {
            get {
                lock (this) {
                    return _inner ??= new Dictionary<TK, TV>();
                }
            }
        }
    }
}