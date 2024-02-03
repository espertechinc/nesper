///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
    public class DIONullableDateTimeOffsetSerde : DataInputOutputSerdeBase<DateTimeOffset?>
    {
        public static readonly DIONullableDateTimeOffsetSerde INSTANCE = new DIONullableDateTimeOffsetSerde();

        private DIONullableDateTimeOffsetSerde()
        {
        }

        public override void Write(
            DateTimeOffset? @object,
            DataOutput output,
            byte[] unitKey,
            EventBeanCollatedWriter writer)
        {
            if (@object == null) {
                output.WriteBoolean(false);
            }
            else {
                output.WriteBoolean(true);
                DIODateTimeOffsetSerde.WriteInternal(@object.Value, output);
            }
        }

        public override DateTimeOffset? ReadValue(
            DataInput input,
            byte[] unitKey)
        {
            if (input.ReadBoolean()) {
                return DIODateTimeOffsetSerde.ReadInternal(input);
            }
            else {
                return null;
            }
        }
    }
} // end of namespace