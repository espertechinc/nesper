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
    /// Binding for nullable short-typed values.
    /// </summary>
    public class DIONullableShortSerde : DataInputOutputSerdeBase<short?>
    {
        public static readonly DIONullableShortSerde INSTANCE = new DIONullableShortSerde();

        private DIONullableShortSerde()
        {
        }

        public override void Write(
            short? @object,
            DataOutput output,
            byte[] pageFullKey,
            EventBeanCollatedWriter writer)
        {
            Write(@object, output);
        }

        public void Write(
            short? @object,
            DataOutput stream)
        {
            var isNull = @object == null;
            stream.WriteBoolean(isNull);
            if (!isNull) {
                stream.WriteShort(@object.Value);
            }
        }

        public short? Read(DataInput input)
        {
            return ReadInternal(input);
        }

        public override short? ReadValue(
            DataInput input,
            byte[] resourceKey)
        {
            return ReadInternal(input);
        }

        private short? ReadInternal(DataInput input)
        {
            var isNull = input.ReadBoolean();
            if (isNull) {
                return null;
            }

            return input.ReadShort();
        }
    }
} // end of namespace