///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.json.util;
using com.espertech.esper.regressionlib.framework;

namespace com.espertech.esper.regressionlib.suite.@event.json
{
	public class EventJsonEventSender
	{
		public static IList<RegressionExecution> Executions()
		{
			IList<RegressionExecution> execs = new List<RegressionExecution>();
			execs.Add(new EventJsonEventSenderParseAndSend());
			return execs;
		}

		internal class EventJsonEventSenderParseAndSend : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string epl =
					"@public @buseventtype @JsonSchema create json schema MyEvent(p1 string);\n" +
					"@Name('s0') select * from MyEvent;\n";
				env.CompileDeploy(epl).AddListener("s0");

				EventSenderJson sender = (EventSenderJson) env.Runtime.EventService.GetEventSender("MyEvent");
				JsonEventObject underlying = (JsonEventObject) sender.Parse("{\"p1\": \"abc\"}");

				sender.SendEvent(underlying);
				env.Listener("s0").AssertInvokedAndReset();

				env.UndeployAll();
			}
		}
	}
} // end of namespace
