///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.serde;
using com.espertech.esper.compat.io;

namespace com.espertech.esper.common.@internal.serde.serdeset.builtin
{
    public class DIODateTimeArrayNullableSerde : DataInputOutputSerdeBase<DateTime?[]>
    {
        public static readonly DIODateTimeArrayNullableSerde INSTANCE = new DIODateTimeArrayNullableSerde();

        private DIODateTimeArrayNullableSerde()
        {
        }

        public void Write(
            DateTime?[] @object,
            DataOutput output)
        {
            WriteInternal(@object, output);
        }

        public DateTime?[] Read(DataInput input)
        {
            return ReadInternal(input);
        }

        public override void Write(
            DateTime?[] @object,
            DataOutput output,
            byte[] unitKey,
            EventBeanCollatedWriter writer)
        {
            WriteInternal(@object, output);
        }

        public override DateTime?[] ReadValue(
            DataInput input,
            byte[] unitKey)
        {
            return ReadInternal(input);
        }

        private void WriteInternal(
            DateTime?[] @object,
            DataOutput output)
        {
            if (@object == null) {
                output.WriteInt(-1);
                return;
            }

            output.WriteInt(@object.Length);
            foreach (var value in @object) {
                DIONullableDateTimeSerde.INSTANCE.Write(value, output);
            }
        }

        private DateTime?[] ReadInternal(DataInput input)
        {
            var len = input.ReadInt();
            if (len == -1) {
                return null;
            }

            var array = new DateTime?[len];
            for (var i = 0; i < len; i++) {
                array[i] = DIODateTimeSerde.INSTANCE.Read(input);
            }

            return array;
        }
    }
} // end of namespace