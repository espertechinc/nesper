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
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.io;

namespace com.espertech.esper.common.@internal.serde.serdeset.multikey
{
	public class DIOMultiKeyArrayObjectSerde : DataInputOutputSerde<MultiKeyArrayObject>
	{
		public readonly static DIOMultiKeyArrayObjectSerde INSTANCE = new DIOMultiKeyArrayObjectSerde();

		public void Write(
			MultiKeyArrayObject mk,
			DataOutput output,
			byte[] unitKey,
			EventBeanCollatedWriter writer)
		{
			WriteInternal(mk.Keys, output);
		}

		public MultiKeyArrayObject Read(
			DataInput input,
			byte[] unitKey)
		{
			return new MultiKeyArrayObject(ReadInternal(input));
		}

		private void WriteInternal(
			object[] @object,
			DataOutput output)
		{
			if (@object == null) {
				output.WriteInt(-1);
				return;
			}

			output.WriteInt(@object.Length);
			ByteArrayOutputStream baos = new ByteArrayOutputStream();
			ObjectOutputStream oos = new ObjectOutputStream(baos);
			foreach (object i in @object) {
				oos.WriteObject(i);
			}

			oos.Close();

			byte[] result = baos.ToByteArray();
			output.WriteInt(result.Length);
			output.Write(result);
			baos.Close();
		}

		private object[] ReadInternal(DataInput input)
		{
			int len = input.ReadInt();
			if (len == -1) {
				return null;
			}

			object[] array = new object[len];
			int size = input.ReadInt();
			byte[] buf = new byte[size];
			input.ReadFully(buf);

			ByteArrayInputStream bais = new ByteArrayInputStream(buf);
			try {
				ObjectInputStream ois = new ObjectInputStreamWithTCCL(bais);
				for (int i = 0; i < array.Length; i++) {
					array[i] = ois.ReadObject();
				}
			}
			catch (IOException e) {
				if (e.Message != null) {
					throw new RuntimeException("IO error de-serializing object: " + e.Message, e);
				}
				else {
					throw new RuntimeException("IO error de-serializing object", e);
				}
			}
			catch (TypeLoadException e) {
				throw new RuntimeException("Class not found de-serializing object: " + e.Message, e);
			}

			return array;
		}
	}
} // end of namespace
