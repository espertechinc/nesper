///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.compat
{
    public class NoCache<TK,TV> : ICache<TK,TV> where TK : class
    {
        public bool TryGet(TK key, out TV value)
        {
            value = default(TV);
            return false;
        }

        public TV Get(TK key)
        {
            return default(TV);
        }

        public TV Put(TK key, TV value)
        {
            return value;
        }

        public void Invalidate()
        {
        }
    }
}
