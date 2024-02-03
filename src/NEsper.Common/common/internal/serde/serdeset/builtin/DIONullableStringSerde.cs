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
    /// Binding for (nullable) String-typed values.
    /// </summary>
    public class DIONullableStringSerde : DataInputOutputSerde<string>
    {
        public static readonly DIONullableStringSerde INSTANCE = new DIONullableStringSerde();

        private DIONullableStringSerde()
        {
        }

        public void Write(
            object @object,
            DataOutput output,
            byte[] unitKey,
            EventBeanCollatedWriter writer)
        {
            Write((string)@object, output, unitKey, writer);
        }

        public void Write(
            string @object,
            DataOutput output,
            byte[] unitKey,
            EventBeanCollatedWriter writer)
        {
            Write(@object, output);
        }

        public void Write(
            string @object,
            DataOutput stream)
        {
            if (@object != null) {
                stream.WriteBoolean(true);
                stream.WriteUTF(@object);
            }
            else {
                stream.WriteBoolean(false);
            }
        }

        public string Read(DataInput input)
        {
            return ReadInternal(input);
        }

        public object Read(
            DataInput input,
            byte[] unitKey)
        {
            return ReadInternal(input);
        }

        public string ReadValue(
            DataInput input,
            byte[] unitKey)
        {
            return ReadInternal(input);
        }

        private string ReadInternal(DataInput input)
        {
            if (input.ReadBoolean()) {
                return input.ReadUTF();
            }

            return null;
        }
    }
} // end of namespace