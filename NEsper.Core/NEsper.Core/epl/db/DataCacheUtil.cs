///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.epl.db
{
    public class DataCacheUtil {
        public static Object GetLookupKey(Object[] methodParams, int numInputParameters) {
            if (numInputParameters == 0) {
                return typeof(Object);
            } else if (numInputParameters == 1) {
                return methodParams[0];
            } else {
                if (methodParams.Length == numInputParameters) {
                    return new MultiKeyUntyped(methodParams);
                }
                var lookupKeys = new Object[numInputParameters];
                Array.Copy(methodParams, 0, lookupKeys, 0, numInputParameters);
                return new MultiKeyUntyped(lookupKeys);
            }
        }
    }
} // end of namespace
