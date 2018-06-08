///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using Force.Crc32;

namespace com.espertech.esper.compat
{
    public static class ByteExtensions
    {
        public static long GetCrc32(this byte[] input)
        {
            return Crc32Algorithm.Compute(input);
        }
    }
}
