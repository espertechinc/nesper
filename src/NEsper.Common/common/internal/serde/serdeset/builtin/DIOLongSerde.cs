///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.serde;
using com.espertech.esper.compat.io;

namespace com.espertech.esper.common.@internal.serde.serdeset.builtin
{
    /// <summary>
    /// Binding for non-null long values.
    /// </summary>
    public class DIOLongSerde : DataInputOutputSerdeBase<long>
    {
        public static readonly DIOLongSerde INSTANCE = new DIOLongSerde();

        private DIOLongSerde()
        {
        }

        public override void Write(
            long @object,
            DataOutput output,
            byte[] pageFullKey,
            EventBeanCollatedWriter writer)
        {
            output.WriteLong(@object);
        }

        public void Write(
            long @object,
            DataOutput stream)
        {
            stream.WriteLong(@object);
        }

        public override long ReadValue(
            DataInput s,
            byte[] resourceKey)
        {
            return s.ReadLong();
        }

        public long Read(DataInput input)
        {
            return input.ReadLong();
        }
    }
} // end of namespace