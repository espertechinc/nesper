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
    public class DIOSkipSerde : DataInputOutputSerde
    {
        public static readonly DIOSkipSerde INSTANCE = new DIOSkipSerde();

        private DIOSkipSerde()
        {
        }

        public void Write(
            object @object,
            DataOutput output,
            byte[] pageFullKey,
            EventBeanCollatedWriter writer)
        {
        }

        public object Read(
            DataInput s,
            byte[] resourceKey)
        {
            return null;
        }

        public void Write(
            object @object,
            DataOutput output)
        {
        }

        public object Read(DataInput input)
        {
            return null;
        }
    }
} // end of namespace