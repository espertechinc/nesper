///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.serde;
using com.espertech.esper.compat;
using com.espertech.esper.compat.io;

namespace com.espertech.esper.common.@internal.serde.serdeset.builtin
{
    public class DIODateTimeSerde : DataInputOutputSerdeBase<DateTime>
    {
        public static readonly DIODateTimeSerde INSTANCE = new DIODateTimeSerde();

        private DIODateTimeSerde()
        {
        }

        public void Write(
            DateTime @object,
            DataOutput output)
        {
            WriteInternal(@object, output);
        }

        public DateTime Read(DataInput input)
        {
            return ReadInternal(input);
        }

        public override void Write(
            DateTime @object,
            DataOutput output,
            byte[] unitKey,
            EventBeanCollatedWriter writer)
        {
            WriteInternal(@object, output);
        }

        public override DateTime ReadValue(
            DataInput input,
            byte[] unitKey)
        {
            return ReadInternal(input);
        }

        internal static void WriteInternal(
            DateTime @object,
            DataOutput output)
        {
            output.WriteLong(DateTimeHelper.UtcNanos(@object));
        }

        internal static DateTime ReadInternal(DataInput input)
        {
            var utcNanos = input.ReadLong();
            return utcNanos.TimeFromNanos();
        }
    }
} // end of namespace