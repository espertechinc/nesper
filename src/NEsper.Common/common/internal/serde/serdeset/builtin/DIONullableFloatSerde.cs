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
    /// Binding for nullable float values.
    /// </summary>
    public class DIONullableFloatSerde : DataInputOutputSerdeBase<float?>
    {
        public static readonly DIONullableFloatSerde INSTANCE = new DIONullableFloatSerde();

        private DIONullableFloatSerde()
        {
        }

        public override void Write(
            float? @object,
            DataOutput output,
            byte[] pageFullKey,
            EventBeanCollatedWriter writer)
        {
            Write(@object, output);
        }

        public void Write(
            float? @object,
            DataOutput stream)
        {
            var isNull = @object == null;
            stream.WriteBoolean(isNull);
            if (!isNull) {
                stream.WriteFloat(@object.Value);
            }
        }

        public float? Read(DataInput input)
        {
            return ReadInternal(input);
        }

        public override float? ReadValue(
            DataInput input,
            byte[] resourceKey)
        {
            return ReadInternal(input);
        }

        private float? ReadInternal(DataInput input)
        {
            var isNull = input.ReadBoolean();
            if (isNull) {
                return null;
            }

            return input.ReadFloat();
        }
    }
} // end of namespace