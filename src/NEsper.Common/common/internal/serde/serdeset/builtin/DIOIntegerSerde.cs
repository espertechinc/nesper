///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
    /// Binding for non-null integer values.
    /// </summary>
    public class DIOIntegerSerde : DataInputOutputSerdeBase<int>
    {
        public static readonly DIOIntegerSerde INSTANCE = new DIOIntegerSerde();

        private DIOIntegerSerde()
        {
        }

        public override void Write(
            int @object,
            DataOutput output,
            byte[] pageFullKey,
            EventBeanCollatedWriter writer)
        {
            Write(@object, output);
        }

        public void Write(
            int @object,
            DataOutput stream)
        {
            stream.WriteInt(@object);
        }

        public override int ReadValue(
            DataInput s,
            byte[] resourceKey)
        {
            return s.ReadInt();
        }

        public int Read(DataInput input)
        {
            return input.ReadInt();
        }
    }
} // end of namespace