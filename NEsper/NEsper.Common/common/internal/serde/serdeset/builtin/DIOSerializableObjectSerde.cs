///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.common.client.serde;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.io;

namespace com.espertech.esper.common.@internal.serde.serdeset.builtin
{
	/// <summary>
	/// Serde that serializes and de-serializes using <seealso cref="ObjectInputStream" /> and <seealso cref="ObjectOutputStream" />.
	/// </summary>
	public class DIOSerializableObjectSerde : DataInputOutputSerde<object> {

	    /// <summary>
	    /// Instance.
	    /// </summary>
	    public readonly static DIOSerializableObjectSerde INSTANCE = new DIOSerializableObjectSerde();

	    private DIOSerializableObjectSerde() {
	    }

	    public void Write(object @object, DataOutput output, byte[] pageFullKey, EventBeanCollatedWriter writer) {
	        byte[] objectBytes = ObjectToByteArr(@object);
	        output.WriteInt(objectBytes.Length);
	        output.Write(objectBytes);
	    }

	    public object Read(DataInput input, byte[] resourceKey) {
	        int size = input.ReadInt();
	        byte[] buf = new byte[size];
	        input.ReadFully(buf);
	        return ByteArrToObject(buf);
	    }

	    /// <summary>
	    /// Serialize object to byte array.
	    /// </summary>
	    /// <param name="underlying">to serialize</param>
	    /// <returns>byte array</returns>
	    public static byte[] ObjectToByteArr(object underlying) {
	        FastByteArrayOutputStream baos = new FastByteArrayOutputStream();
	        ObjectOutputStream oos;
	        try {
	            oos = new ObjectOutputStream(baos);
	            oos.WriteObject(underlying);
	            oos.Close();
	            baos.Close();
	        } catch (IOException e) {
	            throw new RuntimeException("IO error serializing object: " + e.Message, e);
	        }

	        return baos.ByteArrayFast;
	    }

	    /// <summary>
	    /// Deserialize byte arry to object.
	    /// </summary>
	    /// <param name="bytes">to read</param>
	    /// <returns>object</returns>
	    public static object ByteArrToObject(byte[] bytes) {
	        FastByteArrayInputStream bais = new FastByteArrayInputStream(bytes);
	        try {
	            ObjectInputStream ois = new ObjectInputStreamWithTCCL(bais);
	            return ois.ReadObject();
	        } catch (IOException e) {
	            if (e.Message != null) {
	                throw new RuntimeException("IO error de-serializing object: " + e.Message, e);
	            }
	            throw new RuntimeException("IO error de-serializing object", e);
	        } catch (TypeLoadException e) {
	            throw new RuntimeException("Class not found de-serializing object: " + e.Message, e);
	        }
	    }

	    /// <summary>
	    /// Serialize object
	    /// </summary>
	    /// <param name="value">value to serialize</param>
	    /// <param name="output">output stream</param>
	    /// <throws>IOException when a problem occurs</throws>
	    public static void SerializeTo(object value, DataOutput output) {
	        FastByteArrayOutputStream baos = new FastByteArrayOutputStream();
	        ObjectOutputStream oos = new ObjectOutputStream(baos);
	        oos.WriteObject(value);
	        oos.Close();

	        byte[] result = baos.ByteArrayWithCopy;
	        output.WriteInt(result.Length);
	        output.Write(result);
	        baos.Close();
	    }

	    /// <summary>
	    /// Deserialize object
	    /// </summary>
	    /// <param name="input">input stream</param>
	    /// <returns>value</returns>
	    /// <throws>IOException when a problem occurs</throws>
	    public static object DeserializeFrom(DataInput input) {
	        int size = input.ReadInt();
	        byte[] buf = new byte[size];
	        input.ReadFully(buf);

	        FastByteArrayInputStream bais = new FastByteArrayInputStream(buf);
	        try {
	            ObjectInputStream ois = new ObjectInputStreamWithTCCL(bais);
	            return ois.ReadObject();
	        } catch (IOException e) {
	            if (e.Message != null) {
	                throw new RuntimeException("IO error de-serializing object: " + e.Message, e);
	            } else {
	                throw new RuntimeException("IO error de-serializing object", e);
	            }
	        } catch (TypeLoadException e) {
	            throw new RuntimeException("Class not found de-serializing object: " + e.Message, e);
	        }
	    }
	}
} // end of namespace
