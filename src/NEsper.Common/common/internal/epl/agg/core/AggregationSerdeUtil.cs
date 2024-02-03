///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.compat.io;

namespace com.espertech.esper.common.@internal.epl.agg.core
{
    public class AggregationSerdeUtil
    {
        public static void WriteVersion(
            short version,
            DataOutput output)
        {
            output.WriteShort(version);
        }

        public static void ReadVersionChecked(
            short versionExpected,
            DataInput input)
        {
            var version = input.ReadShort();
            if (version != versionExpected) {
                throw new EPException(
                    "Serde version mismatch, expected version " + versionExpected + " but received version " + version);
            }
        }
    }
} // end of namespace