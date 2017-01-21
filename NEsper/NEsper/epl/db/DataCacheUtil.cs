///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.collection;

namespace com.espertech.esper.epl.db
{
    public class DataCacheUtil
    {
        public static Object GetLookupKey(Object[] lookupKeys)
        {
            Object key;
            if (lookupKeys.Length == 0)
            {
                key = typeof (Object);
            }
            else if (lookupKeys.Length > 1)
            {
                key = new MultiKeyUntyped(lookupKeys);
            }
            else
            {
                key = lookupKeys[0];
            }
            return key;
        }
    }
}