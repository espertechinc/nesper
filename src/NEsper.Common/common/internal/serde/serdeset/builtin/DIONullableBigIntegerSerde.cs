///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Numerics;

using com.espertech.esper.common.client.serde;
using com.espertech.esper.compat.io;

namespace com.espertech.esper.common.@internal.serde.serdeset.builtin
{
    /// <summary>
    /// Binding for nullable boolean values.
    /// </summary>
    public class DIONullableBigIntegerSerde : DataInputOutputSerdeBase<BigInteger?>
    {
        public static readonly DIONullableBigIntegerSerde INSTANCE = new DIONullableBigIntegerSerde();

        private DIONullableBigIntegerSerde()
        {
        }

        public override void Write(
            BigInteger? @object,
            DataOutput output,
            byte[] pageFullKey,
            EventBeanCollatedWriter writer)
        {
            Write(@object, output);
        }

        public void Write(
            BigInteger? bigInteger,
            DataOutput stream)
        {
            var isNull = bigInteger == null;
            stream.WriteBoolean(isNull);
            if (!isNull) {
                DIOBigIntegerUtil.WriteBigInt(bigInteger.Value, stream);
            }
        }

        public BigInteger? Read(DataInput input)
        {
            return ReadInternal(input);
        }

        public override BigInteger? ReadValue(
            DataInput input,
            byte[] resourceKey)
        {
            return ReadInternal(input);
        }

        private BigInteger? ReadInternal(DataInput input)
        {
            var isNull = input.ReadBoolean();
            if (isNull) {
                return null;
            }

            return DIOBigIntegerUtil.ReadBigInt(input);
        }
    }
} // end of namespace