///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.serde;
using com.espertech.esper.compat.io;

namespace com.espertech.esper.common.@internal.serde.serdeset.builtin
{
	public class DIOBoxedDoubleArrayNullableSerde : DataInputOutputSerdeBase<double?[]>
	{
		public static readonly DIOBoxedDoubleArrayNullableSerde INSTANCE = new DIOBoxedDoubleArrayNullableSerde();

		private DIOBoxedDoubleArrayNullableSerde()
		{
		}

		public void Write(
			double?[] @object,
			DataOutput output)
		{
			WriteInternal(@object, output);
		}

		public double?[] Read(DataInput input)
		{
			return ReadInternal(input);
		}

		public override void Write(
			double?[] @object,
			DataOutput output,
			byte[] unitKey,
			EventBeanCollatedWriter writer)
		{
			WriteInternal(@object, output);
		}

		public override double?[] ReadValue(
			DataInput input,
			byte[] unitKey)
		{
			return ReadInternal(input);
		}

		private void WriteInternal(
			double?[] @object,
			DataOutput output)
		{
			if (@object == null) {
				output.WriteInt(-1);
				return;
			}

			output.WriteInt(@object.Length);
			foreach (double? i in @object) {
				DIONullableDoubleSerde.INSTANCE.Write(i, output);
			}
		}

		private double?[] ReadInternal(DataInput input)
		{
			int len = input.ReadInt();
			if (len == -1) {
				return null;
			}

			double?[] array = new double?[len];
			for (int i = 0; i < len; i++) {
				array[i] = DIONullableDoubleSerde.INSTANCE.Read(input);
			}

			return array;
		}
	}
} // end of namespace
