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
	public class DIOUnsupportedSerde<TE> : DataInputOutputSerde<TE>
	{
		public readonly static DIOUnsupportedSerde<TE> INSTANCE = new DIOUnsupportedSerde<TE>();

		private DIOUnsupportedSerde()
		{
		}

		public void Write(
			TE @object,
			DataOutput output,
			byte[] unitKey,
			EventBeanCollatedWriter writer)
		{
			throw new UnsupportedOperationException("Operation not supported");
		}

		public TE Read(
			DataInput input,
			byte[] unitKey)
		{
			throw new UnsupportedOperationException("Operation not supported");
		}
	}
} // end of namespace
