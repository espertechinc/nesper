///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
    public class DIOMultiKeyArrayCharSerde : DIOMultiKeyArraySerde<MultiKeyArrayChar>
    {
        public static readonly DIOMultiKeyArrayCharSerde INSTANCE = new DIOMultiKeyArrayCharSerde();

        public Type ComponentType => typeof(char);

        public void Write(
            object @object,
            DataOutput output,
            byte[] unitKey,
            EventBeanCollatedWriter writer)
        {
            Write((MultiKeyArrayChar) @object, output, unitKey, writer);
        }

        public object Read(
            DataInput input,
            byte[] unitKey)
        {
            return ReadValue(input, unitKey);
        }
        
        public MultiKeyArrayChar ReadValue(
            DataInput input,
            byte[] unitKey)
        {
            return new MultiKeyArrayChar(ReadInternal(input));
        }

        public void Write(
            MultiKeyArrayChar mk,
            DataOutput output,
            byte[] unitKey,
            EventBeanCollatedWriter writer)
        {
            WriteInternal(mk.Keys, output);
        }

        private void WriteInternal(
            char[] @object,
            DataOutput output)
        {
            if (@object == null) {
                output.WriteInt(-1);
                return;
            }

            output.WriteInt(@object.Length);
            foreach (var i in @object) {
                output.WriteChar(i);
            }
        }

        private char[] ReadInternal(DataInput input)
        {
            var len = input.ReadInt();
            if (len == -1) {
                return null;
            }

            var array = new char[len];
            for (var i = 0; i < len; i++) {
                array[i] = input.ReadChar();
            }

            return array;
        }
    }
} // end of namespace