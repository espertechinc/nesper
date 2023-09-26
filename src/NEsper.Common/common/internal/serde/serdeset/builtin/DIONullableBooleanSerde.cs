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
    /// Binding for nullable boolean values.
    /// </summary>
    public class DIONullableBooleanSerde : DataInputOutputSerdeBase<bool?>
    {
        public static readonly DIONullableBooleanSerde INSTANCE = new DIONullableBooleanSerde();

        private DIONullableBooleanSerde()
        {
        }

        public override void Write(
            bool? @object,
            DataOutput output,
            byte[] pageFullKey,
            EventBeanCollatedWriter writer)
        {
            Write(@object, output);
        }

        public void Write(
            bool? value,
            DataOutput stream)
        {
            var isNull = value == null;
            stream.WriteBoolean(isNull);
            if (!isNull) {
                stream.WriteBoolean(value.Value);
            }
        }

        public bool? Read(DataInput input)
        {
            return ReadInternal(input);
        }

        public override bool? ReadValue(
            DataInput input,
            byte[] resourceKey)
        {
            return ReadInternal(input);
        }

        private bool? ReadInternal(DataInput input)
        {
            var isNull = input.ReadBoolean();
            if (isNull) {
                return null;
            }

            return input.ReadBoolean();
        }
    }
} // end of namespace