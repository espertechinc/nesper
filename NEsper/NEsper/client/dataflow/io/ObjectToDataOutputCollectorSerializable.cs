///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.util;

namespace com.espertech.esper.client.dataflow.io
{
    /// <summary>Writes a {@link java.io.Serializable} object to {@link java.io.DataOutput}. <para />The output contains the byte array length integer followed by the byte array of the serialized object. </summary>
    public class ObjectToDataOutputCollectorSerializable : ObjectToDataOutputCollector {
    
        public void Collect(ObjectToDataOutputCollectorContext context) {
            byte[] bytes = SerializerUtil.ObjectToByteArr(context.Event);
            context.DataOutput.WriteInt(bytes.Length);
            context.DataOutput.Write(bytes);
        }
    }
}
