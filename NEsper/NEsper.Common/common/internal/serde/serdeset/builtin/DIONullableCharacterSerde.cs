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
	///     Binding for nullable character values.
	/// </summary>
	public class DIONullableCharacterSerde : DataInputOutputSerdeBase<char?>
    {
        public static readonly DIONullableCharacterSerde INSTANCE = new DIONullableCharacterSerde();

        private DIONullableCharacterSerde()
        {
        }

        public override char? Read(
            DataInput input,
            byte[] resourceKey)
        {
            return ReadInternal(input);
        }

        public override void Write(
            char? @object,
            DataOutput output,
            byte[] pageFullKey,
            EventBeanCollatedWriter writer)
        {
            Write(@object, output);
        }

        public void Write(
            char? @object,
            DataOutput stream)
        {
            var isNull = @object == null;
            stream.WriteBoolean(isNull);
            if (!isNull) {
                stream.WriteChar(@object.Value);
            }
        }

        public char? Read(DataInput input)
        {
            return ReadInternal(input);
        }

        private char? ReadInternal(DataInput input)
        {
            var isNull = input.ReadBoolean();
            if (isNull) {
                return null;
            }

            return input.ReadChar();
        }
    }
} // end of namespace