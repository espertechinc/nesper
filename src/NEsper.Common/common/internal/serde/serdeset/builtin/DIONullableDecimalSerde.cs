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
    /// Binding for nullable decimal values.
    /// </summary>
    public class DIONullableDecimalSerde : DataInputOutputSerdeBase<decimal?>
    {
        public static readonly DIONullableDecimalSerde INSTANCE = new DIONullableDecimalSerde();

        private DIONullableDecimalSerde()
        {
        }

        public override void Write(
            decimal? @object,
            DataOutput output,
            byte[] pageFullKey,
            EventBeanCollatedWriter writer)
        {
            Write(@object, output);
        }

        public void Write(
            decimal? @object,
            DataOutput stream)
        {
            var isNull = @object == null;
            stream.WriteBoolean(isNull);
            if (!isNull) {
                stream.WriteDecimal(@object.Value);
            }
        }

        public decimal? Read(DataInput input)
        {
            return ReadInternal(input);
        }

        public override decimal? ReadValue(
            DataInput input,
            byte[] resourceKey)
        {
            return ReadInternal(input);
        }

        private decimal? ReadInternal(DataInput input)
        {
            var isNull = input.ReadBoolean();
            if (isNull) {
                return null;
            }

            return input.ReadDecimal();
        }
    }
} // end of namespace