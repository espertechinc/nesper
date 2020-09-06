///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.datetime;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.pattern
{
	public class PatternMicrosecondResolutionCrontab : RegressionExecution
	{
		public void Run(RegressionEnvironment env)
		{
			RunSequenceMicro(
				env,
				"2013-08-23T08:05:00.000u000",
				"select * from pattern [ every timer:at(*, *, *, *, *, *, *, *, 200) ]",
				new string[] {
					"2013-08-23T08:05:00.000u200",
					"2013-08-23T08:05:00.001u200",
					"2013-08-23T08:05:00.002u200",
					"2013-08-23T08:05:00.003u200"
				});

			RunSequenceMicro(
				env,
				"2013-08-23T08:05:00.000u000",
				"select * from pattern [ every timer:at(*, *, *, *, *, *, *, *, [200,201,202,300,500]) ]",
				new string[] {
					"2013-08-23T08:05:00.000u200",
					"2013-08-23T08:05:00.000u201",
					"2013-08-23T08:05:00.000u202",
					"2013-08-23T08:05:00.000u300",
					"2013-08-23T08:05:00.000u500",
					"2013-08-23T08:05:00.001u200",
					"2013-08-23T08:05:00.001u201",
				});

			RunSequenceMicro(
				env,
				"2013-08-23T08:05:00.000u373",
				"select * from pattern [ every timer:at(*, *, *, *, *, *, *, * / 5, 0) ]",
				new string[] {
					"2013-08-23T08:05:00.005u000",
					"2013-08-23T08:05:00.010u000",
					"2013-08-23T08:05:00.015u000",
					"2013-08-23T08:05:00.020u000"
				});

			RunSequenceMicro(
				env,
				"2013-08-23T08:05:00.000u373",
				"select * from pattern [ every timer:at(*, *, *, *, *, * / 5, *, 0, 373) ]",
				new string[] {
					"2013-08-23T08:05:05.000u373",
					"2013-08-23T08:05:10.000u373",
					"2013-08-23T08:05:15.000u373",
					"2013-08-23T08:05:20.000u373"
				});

			RunSequenceMicro(
				env,
				"2013-08-23T08:05:00.000u000",
				"select * from pattern [ every timer:at(10, 9, *, *, *, 2, *, 373, 243) ]",
				new string[] {
					"2013-08-23T09:10:02.373u243",
					"2013-08-24T09:10:02.373u243",
					"2013-08-25T09:10:02.373u243"
				});
		}

		private static void RunSequenceMicro(
			RegressionEnvironment env,
			string startTime,
			string epl,
			string[] times)
		{
			// Comment-me-in: System.out.println("Start from " + startTime);
			env.AdvanceTime(ParseWithMicro(startTime));

			env.CompileDeploy("@Name('s0') " + epl).AddListener("s0");
			RunSequenceMilliseconds(env, times);

			env.UndeployAll();
		}

		private static long ParseWithMicro(string startTime)
		{
			string[] parts = startTime.Split('u');
			long millis = DateTimeParsingFunctions.ParseDefaultMSec(parts[0]);
			int micro = Int32.Parse(parts[1]);
			return 1000 * millis + micro;
		}

		private static string PrintMicro(long time)
		{
			var dateTime = DateTimeHelper.TimeFromMicros(time);
			return dateTime + " u" + time % 1000;
		}

		private static void RunSequenceMilliseconds(
			RegressionEnvironment env,
			string[] times)
		{
			foreach (string next in times) {
				// send right-before time
				long nextLong = ParseWithMicro(next);
				env.AdvanceTime(nextLong - 1);
				// Comment-me-in: System.out.println("Advance to " + printMicro(nextLong));
				Assert.IsFalse(env.Listener("s0").IsInvoked, "unexpected callback at " + next);

				// send right-after time
				env.AdvanceTime(nextLong);
				// Comment-me-in: System.out.println("Advance to " + printMicro(nextLong));
				Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked(), "missing callback at " + next);

				Assert.IsFalse(env.Listener("s0").IsInvoked);
			}
		}
	}
} // end of namespace
