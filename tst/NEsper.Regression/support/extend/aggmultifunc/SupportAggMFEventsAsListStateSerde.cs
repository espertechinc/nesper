///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.hook.aggmultifunc;
using com.espertech.esper.common.client.serde;
using com.espertech.esper.common.@internal.serde.serdeset.builtin;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.io;

namespace com.espertech.esper.regressionlib.support.extend.aggmultifunc
{
	public class SupportAggMFEventsAsListStateSerde
	{
		public static void Write(
			DataOutput output,
			EventBeanCollatedWriter writer,
			AggregationMultiFunctionState stateMF)
		{
			var state = (SupportAggMFEventsAsListState)stateMF;
			output.WriteInt(state.Events.Count);
			foreach (var supportBean in state.Events) {
				DIOSerializableObjectSerde.SerializeTo(supportBean, output);
			}
		}

		public static AggregationMultiFunctionState Read(DataInput input)
		{
			var size = input.ReadInt();
			IList<SupportBean> events = new List<SupportBean>();
			for (var i = 0; i < size; i++) {
				events.Add((SupportBean)DIOSerializableObjectSerde.DeserializeFrom(input));
			}

			return new SupportAggMFEventsAsListState(events);
		}
	}
} // end of namespace
