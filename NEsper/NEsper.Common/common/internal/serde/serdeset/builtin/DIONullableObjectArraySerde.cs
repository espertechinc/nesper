///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.serde;
using com.espertech.esper.compat.io;

namespace com.espertech.esper.common.@internal.serde.serdeset.builtin
{
	public class DIONullableObjectArraySerde : DataInputOutputSerdeBase<object[]>
	{
		private readonly Type _componentType;
		private readonly DataInputOutputSerde _componentBinding;

		public DIONullableObjectArraySerde(
			Type componentType,
			DataInputOutputSerde componentBinding)
		{
			_componentType = componentType;
			_componentBinding = componentBinding;
		}

		public override void Write(
			object[] @object,
			DataOutput output,
			byte[] unitKey,
			EventBeanCollatedWriter writer)
		{
			WriteInternal(@object, output, unitKey, writer);
		}

		public override object[] ReadValue(
			DataInput input,
			byte[] unitKey)
		{
			return ReadInternal(input, unitKey);
		}

		private void WriteInternal(
			object[] @object,
			DataOutput output,
			byte[] unitKey,
			EventBeanCollatedWriter writer)
		{
			if (@object == null) {
				output.WriteInt(-1);
				return;
			}

			output.WriteInt(@object.Length);
			foreach (object i in @object) {
				_componentBinding.Write(i, output, unitKey, writer);
			}
		}

		private object[] ReadInternal(
			DataInput input,
			byte[] unitKey)
		{
			int len = input.ReadInt();
			if (len == -1) {
				return null;
			}

			var array = Array.CreateInstance(_componentType, len);
			for (int i = 0; i < len; i++) {
				var value = _componentBinding.Read(input, unitKey);
				array.SetValue(value, i);
			}

			return (object[]) array;
		}
	}
} // end of namespace
