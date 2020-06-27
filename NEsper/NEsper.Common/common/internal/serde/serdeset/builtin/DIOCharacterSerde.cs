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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.io;

namespace com.espertech.esper.common.@internal.serde.serdeset.builtin
{
	/// <summary>
	/// Binding for non-null character values.
	/// </summary>
	public class DIOCharacterSerde : DataInputOutputSerdeBase<char>
	{
		public static readonly DIOCharacterSerde INSTANCE = new DIOCharacterSerde();

		private DIOCharacterSerde()
		{
		}

		public override void Write(
			char @object,
			DataOutput output,
			byte[] pageFullKey,
			EventBeanCollatedWriter writer)
		{
			Write(@object, output);
		}

		public void Write(
			char @object,
			DataOutput stream)
		{
			stream.WriteChar(@object);
		}

		public override char Read(
			DataInput s,
			byte[] resourceKey)
		{
			return s.ReadChar();
		}

		public char Read(DataInput input)
		{
			return input.ReadChar();
		}
	}
} // end of namespace
