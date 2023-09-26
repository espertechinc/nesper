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
    public class DIOBoxedCharacterArrayNullableSerde : DataInputOutputSerdeBase<char?[]>
    {
        public static readonly DIOBoxedCharacterArrayNullableSerde INSTANCE = new DIOBoxedCharacterArrayNullableSerde();

        private DIOBoxedCharacterArrayNullableSerde()
        {
        }

        public void Write(
            char?[] @object,
            DataOutput output)
        {
            WriteInternal(@object, output);
        }

        public char?[] Read(DataInput input)
        {
            return ReadInternal(input);
        }

        public override void Write(
            char?[] @object,
            DataOutput output,
            byte[] unitKey,
            EventBeanCollatedWriter writer)
        {
            WriteInternal(@object, output);
        }

        public override char?[] ReadValue(
            DataInput input,
            byte[] unitKey)
        {
            return ReadInternal(input);
        }

        private void WriteInternal(
            char?[] @object,
            DataOutput output)
        {
            if (@object == null) {
                output.WriteInt(-1);
                return;
            }

            output.WriteInt(@object.Length);
            foreach (var i in @object) {
                DIONullableCharacterSerde.INSTANCE.Write(i, output);
            }
        }

        private char?[] ReadInternal(DataInput input)
        {
            var len = input.ReadInt();
            if (len == -1) {
                return null;
            }

            var array = new char?[len];
            for (var i = 0; i < len; i++) {
                array[i] = DIONullableCharacterSerde.INSTANCE.Read(input);
            }

            return array;
        }
    }
} // end of namespace