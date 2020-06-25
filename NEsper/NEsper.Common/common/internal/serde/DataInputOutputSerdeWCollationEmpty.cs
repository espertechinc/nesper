///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.serde;
using com.espertech.esper.compat.io;

namespace com.espertech.esper.common.@internal.serde
{
    public class DataInputOutputSerdeWCollationEmpty : DataInputOutputSerdeWCollation<object>
    {
        public static readonly DataInputOutputSerdeWCollationEmpty INSTANCE = new DataInputOutputSerdeWCollationEmpty();

        private DataInputOutputSerdeWCollationEmpty()
        {
        }

        public void Write(
            object @object,
            DataOutput output,
            byte[] unitKey,
            EventBeanCollatedWriter writer)
        {
        }

        public object Read(
            DataInput input,
            byte[] unitKey)
        {
            return null;
        }
    }
} // end of namespace