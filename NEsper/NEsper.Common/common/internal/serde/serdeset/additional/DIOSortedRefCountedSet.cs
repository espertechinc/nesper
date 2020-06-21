///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.common.client.serde;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.io;

namespace com.espertech.esper.common.@internal.serde.serdeset.additional
{
	public class DIOSortedRefCountedSet : DataInputOutputSerde<SortedRefCountedSet<object>>
	{
		private readonly DataInputOutputSerde<object> _inner;

		public DIOSortedRefCountedSet(DataInputOutputSerde<object> inner)
		{
			this._inner = inner;
		}

		public void Write(
			SortedRefCountedSet<object> valueSet,
			DataOutput output,
			byte[] unitKey,
			EventBeanCollatedWriter writer)
		{
			output.WriteInt(valueSet.RefSet.Count);
			foreach (var entry in valueSet.RefSet) {
				_inner.Write(entry.Key, output, unitKey, writer);
				output.WriteInt(entry.Value);
			}

			output.WriteLong(valueSet.CountPoints);
		}

		public SortedRefCountedSet<object> Read(
			DataInput input,
			byte[] unitKey)
		{
			var valueSet = new SortedRefCountedSet<object>();
			var refSet = valueSet.RefSet;
			int size = input.ReadInt();
			for (int i = 0; i < size; i++) {
				var key = _inner.Read(input, unitKey);
				var @ref = input.ReadInt();
				refSet.Put(key, @ref);
			}

			valueSet.CountPoints = input.ReadLong();
			return valueSet;
		}
	}
} // end of namespace
