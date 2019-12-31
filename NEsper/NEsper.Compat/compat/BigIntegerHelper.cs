///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Numerics;

namespace com.espertech.esper.compat
{
    /// <summary>
    /// Utility class for interoperability with BigInteger.
    /// </summary>
    public class BigIntegerHelper
    {
        public static BigInteger ValueOf(int value)
        {
            return new BigInteger(value);
        }

        public static BigInteger ValueOf(long value)
        {
            return new BigInteger(value);
        }
    }
}