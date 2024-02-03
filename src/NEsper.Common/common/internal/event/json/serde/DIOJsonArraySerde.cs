///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.serde;
using com.espertech.esper.compat.io;

namespace com.espertech.esper.common.@internal.@event.json.serde
{
    public class DIOJsonArraySerde : DataInputOutputSerde<object[]>
    {
        public static readonly DIOJsonArraySerde INSTANCE = new DIOJsonArraySerde();

        private DIOJsonArraySerde()
        {
        }

        public void Write(
            object @object,
            DataOutput output,
            byte[] unitKey,
            EventBeanCollatedWriter writer)
        {
            Write((object[])@object, output, unitKey, writer);
        }

        object DataInputOutputSerde.Read(
            DataInput input,
            byte[] unitKey)
        {
            return ReadValue(input, unitKey);
        }

        public void Write(
            object[] @object,
            DataOutput output,
            byte[] unitKey,
            EventBeanCollatedWriter writer)
        {
            output.WriteBoolean(@object != null);
            if (@object != null) {
                DIOJsonSerdeHelper.WriteArray(@object, output);
            }
        }

        public object[] ReadValue(
            DataInput input,
            byte[] unitKey)
        {
            var notNull = input.ReadBoolean();
            return notNull ? DIOJsonSerdeHelper.ReadArray(input) : null;
        }
    }
} // end of namespace