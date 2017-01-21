///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.util
{
    public class SerializerUtil
    {
        /// <summary>Serialize object to byte array. </summary>
        /// <param name="underlying">to serialize</param>
        /// <returns>byte array</returns>
        public static byte[] ObjectToByteArr(Object underlying)
        {
            return SerializerFactory.Serialize(
                new[] {SerializerFactory.OBJECT_SERIALIZER},
                new[] {underlying});
        }

        /// <summary>Deserialize byte arry to object. </summary>
        /// <param name="bytes">to read</param>
        /// <returns>object</returns>
        public static Object ByteArrToObject(byte[] bytes)
        {
            if (bytes == null)
            {
                return null;
            }

            return SerializerFactory.Deserialize(
                1, bytes, new[] { SerializerFactory.OBJECT_SERIALIZER })[0];
        }
    }
}
