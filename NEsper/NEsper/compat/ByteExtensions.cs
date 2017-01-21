///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using Nito.KitchenSink.CRC;

namespace com.espertech.esper.compat
{
    public static class ByteExtensions
    {
        public static int GetCrc32(this byte[] input)
        {
            var algo = new CRC32();
            var hash = algo.ComputeHash(input);
            return BitConverter.ToInt32(hash, 0);
        }
    }
}
