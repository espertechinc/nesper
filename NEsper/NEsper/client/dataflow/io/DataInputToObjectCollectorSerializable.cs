///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.compat.logging;
using com.espertech.esper.events;
using com.espertech.esper.util;

namespace com.espertech.esper.client.dataflow.io
{
    /// <summary>Reads a {@link java.io.Serializable} from {@link java.io.DataInput} and emits the resulting object. <para />The input must carry an int-typed number of bytes followed by the serialized object. </summary>
    public class DataInputToObjectCollectorSerializable : DataInputToObjectCollector {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        public void Collect(DataInputToObjectCollectorContext context) {
            int size = context.DataInput.ReadInt();
            byte[] bytes = new byte[size];
            context.DataInput.ReadFully(bytes);
            Object event = SerializerUtil.ByteArrToObject(bytes);
            if (log.IsDebugEnabled) {
                log.Debug("Submitting event " + EventBeanUtility.SummarizeUnderlying(@event));
            }
            context.Emitter.Submit(@event);
        }
    }
}
