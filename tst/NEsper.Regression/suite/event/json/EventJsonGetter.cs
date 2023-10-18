///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;

using Newtonsoft.Json.Linq;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.@event.json
{
	/// <summary>
	/// Most getter tests can be found in Event+Infra.
	/// </summary>
	public class EventJsonGetter
	{
		public static IList<RegressionExecution> Executions()
		{
			IList<RegressionExecution> execs = new List<RegressionExecution>();
			execs.Add(new EventJsonGetterMapType());
			return execs;
		}

		internal class EventJsonGetterMapType : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var mapType = typeof(Properties).FullName;
				
				env.CompileDeploy(
						$"@public @buseventtype create json schema JsonEvent(prop {mapType});\n" +
						"@name('s0') select * from JsonEvent")
					.AddListener("s0");

				env.SendEventJson(
					new JObject(
							new JProperty(
								"prop",
								new JObject(
									new JProperty("x", "y"))))
						.ToString(),
					"JsonEvent");

				env.AssertEventNew(
					"s0",
					@event => {
						var getterMapped = @event.EventType.GetGetter("prop('x')");

						Assert.AreEqual("y", getterMapped.Get(@event));
						Assert.IsNull(@event.EventType.GetGetter("prop.somefield?"));
					});
				
				env.UndeployAll();
			}
		}
	}
} // end of namespace
