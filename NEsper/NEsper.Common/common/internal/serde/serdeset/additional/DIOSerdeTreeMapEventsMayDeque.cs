///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.serde;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.io;

namespace com.espertech.esper.common.@internal.serde.serdeset.additional
{
    internal interface DIOSerdeTreeMapEventsMayDeque
    {
        void Write(
            IDictionary<object, object> @object,
            DataOutput output,
            byte[] unitKey,
            EventBeanCollatedWriter writer);

        void Read(
            IDictionary<object, object> @object,
            DataInput input,
            byte[] unitKey);
    }
} // end of namespace