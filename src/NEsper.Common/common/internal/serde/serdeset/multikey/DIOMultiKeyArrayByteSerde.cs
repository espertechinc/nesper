///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.serde;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.compat.io;

namespace com.espertech.esper.common.@internal.serde.serdeset.multikey
{
    public class DIOMultiKeyArrayByteSerde : DIOMultiKeyArraySerde<MultiKeyArrayByte>
    {
        public static readonly DIOMultiKeyArrayByteSerde INSTANCE = new DIOMultiKeyArrayByteSerde();

        public Type ComponentType => typeof(byte);

        public void Write(
            object @object,
            DataOutput output,
            byte[] unitKey,
            EventBeanCollatedWriter writer)
        {
            Write((MultiKeyArrayByte) @object, output, unitKey, writer);
        }

        public object Read(
            DataInput input,
            byte[] unitKey)
        {
            return ReadValue(input, unitKey);
        }
        
        public void Write(
            MultiKeyArrayByte mk,
            DataOutput output,
            byte[] unitKey,
            EventBeanCollatedWriter writer)
        {
            WriteInternal(mk.Keys, output);
        }

        public MultiKeyArrayByte ReadValue(
            DataInput input,
            byte[] unitKey)
        {
            return new MultiKeyArrayByte(ReadInternal(input));
        }

        private void WriteInternal(
            byte[] @object,
            DataOutput output)
        {
            if (@object == null) {
                output.WriteInt(-1);
                return;
            }

            output.WriteInt(@object.Length);
            foreach (var i in @object) {
                output.WriteByte(i);
            }
        }

        private byte[] ReadInternal(DataInput input)
        {
            var len = input.ReadInt();
            if (len == -1) {
                return null;
            }

            var array = new byte[len];
            for (var i = 0; i < len; i++) {
                array[i] = input.ReadByte();
            }

            return array;
        }
    }
} // end of namespace