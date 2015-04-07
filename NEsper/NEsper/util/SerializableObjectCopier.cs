///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace com.espertech.esper.util
{
    /// <summary>
    /// Utility class for copying serializable objects via object input and output streams.
    /// </summary>
    public class SerializableObjectCopier
    {
        /// <summary>Deep copies the input object. </summary>
        /// <param name="orig">is the object to be copied, must be serializable</param>
        /// <returns>copied object</returns>
        /// <throws>IOException if the streams returned an exception</throws>
        /// <throws>ClassNotFoundException if the de-serialize fails</throws>
        public static Object Copy(Object orig)
        {
            // Create the formatter
            IFormatter formatter = new BinaryFormatter();
            using (MemoryStream stream = new MemoryStream())
            {
                // Serialize the object graph to the stream
                formatter.Serialize(stream, orig);
                // Rewind the stream
                stream.Seek(0, SeekOrigin.Begin);
                // Deserialize the object graph from the stream
                return formatter.Deserialize(stream);
            }
        }
    }
}
