///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


namespace com.espertech.esper.compat
{
    public interface ICache<TK, TV> where TK : class
    {
        bool TryGet(TK key, out TV value);
        TV Get(TK key);
        TV Put(TK key, TV value);
        void Invalidate();
    }
}
