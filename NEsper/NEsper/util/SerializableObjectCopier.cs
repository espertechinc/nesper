///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using com.espertech.esper.compat.container;

namespace com.espertech.esper.util
{
    /// <summary>
    /// Utility class for copying serializable objects via object input and output streams.
    /// </summary>
    public class SerializableObjectCopier
    {
        /// <summary>
        /// Deep copies the input object.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="orig">is the object to be copied, must be serializable</param>
        /// <returns>
        /// copied object
        /// </returns>
        /// <throws>IOException if the streams returned an exception</throws>
        /// <throws>ClassNotFoundException if the de-serialize fails</throws>
        public static Object Copy(IContainer container, Object orig)
        {
            // Create the formatter
            var formatter = new BinaryFormatter();
            formatter.FilterLevel = TypeFilterLevel.Full;
#if NETFRAMEWORK
            formatter.AssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Full;
#endif
            formatter.Context = new StreamingContext(StreamingContextStates.Clone, container);

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
