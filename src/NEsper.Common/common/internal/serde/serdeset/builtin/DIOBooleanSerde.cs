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
    ///     Binding for non-null boolean values.
    /// </summary>
    public class DIOBooleanSerde : DataInputOutputSerdeBase<bool>
    {
        public static readonly DIOBooleanSerde INSTANCE = new DIOBooleanSerde();

        private DIOBooleanSerde()
        {
        }

        public override void Write(
            bool @object,
            DataOutput output,
            byte[] pageFullKey,
            EventBeanCollatedWriter writer)
        {
            output.WriteBoolean(@object);
        }

        public void Write(
            bool @object,
            DataOutput stream)
        {
            stream.WriteBoolean(@object);
        }

        public override bool ReadValue(
            DataInput s,
            byte[] resourceKey)
        {
            return s.ReadBoolean();
        }

        public bool Read(DataInput input)
        {
            return input.ReadBoolean();
        }
    }
} // end of namespace