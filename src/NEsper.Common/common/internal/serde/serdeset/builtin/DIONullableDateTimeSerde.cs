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
    public class DIONullableDateTimeSerde : DataInputOutputSerdeBase<DateTime?>
    {
        public static readonly DIONullableDateTimeSerde INSTANCE = new DIONullableDateTimeSerde();

        private DIONullableDateTimeSerde()
        {
        }

        public void Write(
            DateTime? @object,
            DataOutput output)
        {
            if (@object == null) {
                output.WriteBoolean(false);
            }
            else {
                output.WriteBoolean(true);
                DIODateTimeSerde.WriteInternal(@object.Value, output);
            }
        }

        public override void Write(
            DateTime? @object,
            DataOutput output,
            byte[] unitKey,
            EventBeanCollatedWriter writer)
        {
            Write(@object, output);
        }

        public DateTime? Read(DataInput input)
        {
            if (input.ReadBoolean()) {
                return DIODateTimeSerde.ReadInternal(input);
            }
            else {
                return null;
            }
        }

        public override DateTime? ReadValue(
            DataInput input,
            byte[] unitKey)
        {
            return Read(input);
        }
    }
} // end of namespace