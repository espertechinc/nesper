///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.serde;
using com.espertech.esper.compat.io;

namespace com.espertech.esper.common.@internal.@event.json.serde
{
    public class DIOJsonAnyValueSerde : DataInputOutputSerde
    {
        public static readonly DIOJsonAnyValueSerde INSTANCE = new DIOJsonAnyValueSerde();

        private DIOJsonAnyValueSerde()
        {
        }

        public void Write(
            object @object,
            DataOutput output,
            byte[] unitKey,
            EventBeanCollatedWriter writer)
        {
            DIOJsonSerdeHelper.WriteValue(@object, output);
        }

        public object Read(
            DataInput input,
            byte[] unitKey)
        {
            return DIOJsonSerdeHelper.ReadValue(input);
        }
    }
} // end of namespace