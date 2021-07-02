///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.events;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.support.events.SupportGenericColUtil; // assertPropertyEPTypes

namespace com.espertech.esper.regressionlib.suite.infra.nwtable
{
	public class InfraNWTableCreate
	{
		public static ICollection<RegressionExecution> Executions()
		{
			var execs = new List<RegressionExecution>();
			execs.Add(new InfraCreateGenericColType(true));
			execs.Add(new InfraCreateGenericColType(false));
			return execs;
		}

		internal class InfraCreateGenericColType : RegressionExecution
		{
			private readonly bool _namedWindow;

			public InfraCreateGenericColType(bool namedWindow)
			{
				_namedWindow = namedWindow;
			}

			public void Run(RegressionEnvironment env)
			{
				var epl = "@public @buseventtype create schema MyInputEvent(" + SupportGenericColUtil.AllNamesAndTypes() + ");\n";
				epl += "@name('infra')";
				epl += _namedWindow ? "create window MyInfra#keepall as (" : "create table MyInfra as (";
				epl += SupportGenericColUtil.AllNamesAndTypes();
				epl += ");\n";
				epl += "on MyInputEvent merge MyInfra insert select " + SupportGenericColUtil.AllNames() + ";\n";

				env.CompileDeploy(epl);
				AssertPropertyTypes(env.Statement("infra").EventType);

				env.SendEventMap(SupportGenericColUtil.SampleEvent, "MyInputEvent");

				env.Milestone(0);

				var enumerator = env.GetEnumerator("infra");
				Assert.IsTrue(enumerator.MoveNext());
				var @event = enumerator.Current;
				SupportGenericColUtil.Compare(@event);

				env.UndeployAll();
			}
		}
	}
} // end of namespace
