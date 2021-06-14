///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.serde;
using com.espertech.esper.compat.io;

namespace com.espertech.esper.common.@internal.serde.serdeset.builtin
{
	public class DIOSetSerde : DataInputOutputSerdeBase<ISet<object>>
	{
		private readonly DataInputOutputSerde inner;

		public DIOSetSerde(DataInputOutputSerde inner)
		{
			this.inner = inner;
		}

		public override void Write(
			ISet<object> set,
			DataOutput output,
			byte[] unitKey,
			EventBeanCollatedWriter writer)
		{
			output.WriteInt(set.Count);
			foreach (object @object in set) {
				inner.Write(@object, output, unitKey, writer);
			}
		}

		public override ISet<object> ReadValue(
			DataInput input,
			byte[] unitKey)
		{
			var size = input.ReadInt();
			var set = new HashSet<object>();
			for (int i = 0; i < size; i++) {
				set.Add(inner.Read(input, unitKey));
			}

			return set;
		}
	}
} // end of namespace
