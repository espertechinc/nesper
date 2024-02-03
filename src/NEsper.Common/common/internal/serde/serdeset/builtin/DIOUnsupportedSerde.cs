///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.serde;
using com.espertech.esper.compat;
using com.espertech.esper.compat.io;

namespace com.espertech.esper.common.@internal.serde.serdeset.builtin
{
    public class DIOUnsupportedSerde<TE> : DataInputOutputSerdeBase<TE>
    {
        public static readonly DIOUnsupportedSerde<TE> INSTANCE = new DIOUnsupportedSerde<TE>();

        private DIOUnsupportedSerde()
        {
        }

        public override void Write(
            TE @object,
            DataOutput output,
            byte[] unitKey,
            EventBeanCollatedWriter writer)
        {
            throw new UnsupportedOperationException("Operation not supported");
        }

        public override TE ReadValue(
            DataInput input,
            byte[] unitKey)
        {
            throw new UnsupportedOperationException("Operation not supported");
        }
    }
} // end of namespace