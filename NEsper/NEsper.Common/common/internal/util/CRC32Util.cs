///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.util
{
	public class CRC32Util {
	    public static long ComputeCRC32(string name) {
	        CRC32 crc32 = new CRC32();
	        crc32.Update(name.GetBytes(Charset.ForName("UTF-8")));
	        return crc32.Value;
	    }
	}
} // end of namespace