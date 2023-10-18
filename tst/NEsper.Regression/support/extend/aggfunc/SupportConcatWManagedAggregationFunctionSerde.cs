///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Text;

using com.espertech.esper.common.client.hook.aggfunc;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.io;

namespace com.espertech.esper.regressionlib.support.extend.aggfunc
{
	public class SupportConcatWManagedAggregationFunctionSerde
	{
		public static void Write(
			DataOutput output,
			AggregationFunction value)
		{
			var agg = (SupportConcatWManagedAggregationFunction)value;
			var stringValue = (string) agg.Value;
			output.WriteUTF(stringValue);
		}

		public static AggregationFunction Read(DataInput input)
		{
			var current = input.ReadUTF();
			if (string.IsNullOrWhiteSpace(current)) {
				return new SupportConcatWManagedAggregationFunction();
			}
			return new SupportConcatWManagedAggregationFunction(new StringBuilder(current));
		}
	}
} // end of namespace
