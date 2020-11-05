///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.json.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

using static com.espertech.esper.regressionlib.support.json.SupportJsonEventTypeUtil;

namespace com.espertech.esper.regressionlib.suite.@event.json
{
	public class EventJsonUnderlying
	{
		public static IList<RegressionExecution> Executions()
		{
			IList<RegressionExecution> execs = new List<RegressionExecution>();
			execs.Add(new EventJsonUnderlyingMapDynamicZeroDeclared());
			execs.Add(new EventJsonUnderlyingMapDynamicOneDeclared());
			execs.Add(new EventJsonUnderlyingMapDynamicTwoDeclared());
			execs.Add(new EventJsonUnderlyingMapNonDynamicZeroDeclared());
			execs.Add(new EventJsonUnderlyingMapNonDynamicOneDeclared());
			execs.Add(new EventJsonUnderlyingMapNonDynamicTwoDeclared());
			return execs;
		}

		internal class EventJsonUnderlyingMapNonDynamicZeroDeclared : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				env.CompileDeploy(
						"@public @buseventtype create json schema JsonEvent();\n" +
						"@Name('s0') select *  from JsonEvent#keepall")
					.AddListener("s0");

				IDictionary<string, object> actualOne = SendJsonGetUnderlying(env, "{\"a\" : 1, \"b\": 2, \"c\": 3}\n");
				CompareDictionaries(new LinkedHashMap<string, object>(), actualOne);

				env.Milestone(0);

				using (IEnumerator<EventBean> it = env.Statement("s0").GetEnumerator()) {
					CompareMapWBean(new LinkedHashMap<string, object>(), it.Advance());
				}

				env.UndeployAll();
			}
		}

		internal class EventJsonUnderlyingMapNonDynamicOneDeclared : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				env.CompileDeploy(
						"@public @buseventtype create json schema JsonEvent(a int);\n" +
						"@Name('s0') select *  from JsonEvent#keepall")
					.AddListener("s0");

				IDictionary<string, object> actualOne = SendJsonGetUnderlying(env, "{\"a\" : 1, \"b\": 2, \"c\": 3}\n");
				IDictionary<string, object> expectedOne = new LinkedHashMap<string, object>();
				expectedOne.Put("a", 1);
				CompareDictionaries(expectedOne, actualOne);

				IDictionary<string, object> actualTwo = SendJsonGetUnderlying(env, "{\"a\" : 10}\n");
				IDictionary<string, object> expectedTwo = new LinkedHashMap<string, object>();
				expectedTwo.Put("a", 10);
				CompareDictionaries(expectedTwo, actualTwo);

				IDictionary<string, object> actualThree = SendJsonGetUnderlying(env, "{}\n");
				IDictionary<string, object> expectedThree = new LinkedHashMap<string, object>();
				expectedThree.Put("a", null);
				CompareDictionaries(expectedThree, actualThree);

				env.Milestone(0);

				using (IEnumerator<EventBean> it = env.Statement("s0").GetEnumerator()) {
					CompareMapWBean(expectedOne, it.Advance());
					CompareMapWBean(expectedTwo, it.Advance());
					CompareMapWBean(expectedThree, it.Advance());
				}

				env.UndeployAll();
			}
		}

		internal class EventJsonUnderlyingMapNonDynamicTwoDeclared : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				env.CompileDeploy(
						"@public @buseventtype create json schema JsonEvent(a int, b int);\n" +
						"@Name('s0') select *  from JsonEvent#keepall")
					.AddListener("s0");

				IDictionary<string, object> actualOne = SendJsonGetUnderlying(env, "{\"a\" : 1, \"b\": 2, \"c\": 3}\n");
				IDictionary<string, object> expectedOne = new LinkedHashMap<string, object>();
				expectedOne.Put("a", 1);
				expectedOne.Put("b", 2);
				CompareDictionaries(expectedOne, actualOne);

				IDictionary<string, object> actualTwo = SendJsonGetUnderlying(env, "{\"a\" : 10}\n");
				IDictionary<string, object> expectedTwo = new LinkedHashMap<string, object>();
				expectedTwo.Put("a", 10);
				expectedTwo.Put("b", null);
				CompareDictionaries(expectedTwo, actualTwo);

				IDictionary<string, object> actualThree = SendJsonGetUnderlying(env, "{}\n");
				IDictionary<string, object> expectedThree = new LinkedHashMap<string, object>();
				expectedThree.Put("a", null);
				expectedThree.Put("b", null);
				CompareDictionaries(expectedThree, actualThree);

				env.Milestone(0);

				using (IEnumerator<EventBean> it = env.Statement("s0").GetEnumerator()) {
					CompareMapWBean(expectedOne, it.Advance());
					CompareMapWBean(expectedTwo, it.Advance());
					CompareMapWBean(expectedThree, it.Advance());
				}

				env.UndeployAll();
			}
		}

		internal class EventJsonUnderlyingMapDynamicZeroDeclared : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				env.CompileDeploy(
						"@JsonSchema(Dynamic=true) @public @buseventtype create json schema JsonEvent();\n" +
						"@Name('s0') select *  from JsonEvent#keepall")
					.AddListener("s0");

				IDictionary<string, object> actualOne = SendJsonGetUnderlying(env, "{\"a\" : 1, \"b\": 2, \"c\": 3}\n");
				IDictionary<string, object> expectedOne = new LinkedHashMap<string, object>();
				expectedOne.Put("a", 1);
				expectedOne.Put("b", 2);
				expectedOne.Put("c", 3);
				CompareDictionaries(expectedOne, actualOne);

				IDictionary<string, object> actualTwo = SendJsonGetUnderlying(env, "{\"a\" : 10}\n");
				IDictionary<string, object> expectedTwo = new LinkedHashMap<string, object>();
				expectedTwo.Put("a", 10);
				CompareDictionaries(expectedTwo, actualTwo);

				IDictionary<string, object> actualThree = SendJsonGetUnderlying(env, "{\"a\" : null, \"c\": 101, \"d\": 102}\n");
				IDictionary<string, object> expectedThree = new LinkedHashMap<string, object>();
				expectedThree.Put("a", null);
				expectedThree.Put("c", 101);
				expectedThree.Put("d", 102);
				CompareDictionaries(expectedThree, actualThree);

				IDictionary<string, object> actualFour = SendJsonGetUnderlying(env, "{}\n");
				CompareDictionaries(new LinkedHashMap<string, object>(), actualFour);

				env.Milestone(0);

				using (IEnumerator<EventBean> it = env.Statement("s0").GetEnumerator()) {
					CompareMapWBean(expectedOne, it.Advance());
					CompareMapWBean(expectedTwo, it.Advance());
					CompareMapWBean(expectedThree, it.Advance());
					CompareMapWBean(new LinkedHashMap<string, object>(), it.Advance());
				}

				env.UndeployAll();
			}
		}

		internal class EventJsonUnderlyingMapDynamicOneDeclared : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				env.CompileDeploy(
						"@JsonSchema(Dynamic=true) @public @buseventtype create json schema JsonEvent(a int);\n" +
						"@Name('s0') select *  from JsonEvent#keepall")
					.AddListener("s0");

				IDictionary<string, object> actualOne = SendJsonGetUnderlying(env, "{\"a\" : 1, \"b\": 2, \"c\": 3}\n");
				IDictionary<string, object> expectedOne = new LinkedHashMap<string, object>();
				expectedOne.Put("a", 1);
				expectedOne.Put("b", 2);
				expectedOne.Put("c", 3);
				CompareDictionaries(expectedOne, actualOne);

				IDictionary<string, object> actualTwo = SendJsonGetUnderlying(env, "{\"a\" : 10}\n");
				IDictionary<string, object> expectedTwo = new LinkedHashMap<string, object>();
				expectedTwo.Put("a", 10);
				CompareDictionaries(expectedTwo, actualTwo);

				IDictionary<string, object> actualThree = SendJsonGetUnderlying(env, "{\"a\" : null, \"c\": 101, \"d\": 102}\n");
				IDictionary<string, object> expectedThree = new LinkedHashMap<string, object>();
				expectedThree.Put("a", null);
				expectedThree.Put("c", 101);
				expectedThree.Put("d", 102);
				CompareDictionaries(expectedThree, actualThree);

				IDictionary<string, object> actualFour = SendJsonGetUnderlying(env, "{}\n");
				IDictionary<string, object> expectedFour = new LinkedHashMap<string, object>();
				expectedFour.Put("a", null);
				CompareDictionaries(expectedFour, actualFour);

				env.Milestone(0);

				using (IEnumerator<EventBean> it = env.Statement("s0").GetEnumerator()) {
					CompareMapWBean(expectedOne, it.Advance());
					CompareMapWBean(expectedTwo, it.Advance());
					CompareMapWBean(expectedThree, it.Advance());
					CompareMapWBean(expectedFour, it.Advance());
				}

				env.UndeployAll();
			}
		}

		internal class EventJsonUnderlyingMapDynamicTwoDeclared : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				env.CompileDeploy(
						"@JsonSchema(Dynamic=true) @public @buseventtype create json schema JsonEvent(a int, b int);\n" +
						"@Name('s0') select *  from JsonEvent#keepall")
					.AddListener("s0");

				IDictionary<string, object> actualOne = SendJsonGetUnderlying(env, "{\"a\" : 1, \"b\": 2, \"c\": 3}\n");
				IDictionary<string, object> expectedOne = new LinkedHashMap<string, object>();
				expectedOne.Put("a", 1);
				expectedOne.Put("b", 2);
				expectedOne.Put("c", 3);
				CompareDictionaries(expectedOne, actualOne);

				IDictionary<string, object> actualTwo = SendJsonGetUnderlying(env, "{\"a\" : 10}\n");
				IDictionary<string, object> expectedTwo = new LinkedHashMap<string, object>();
				expectedTwo.Put("a", 10);
				expectedTwo.Put("b", null);
				CompareDictionaries(expectedTwo, actualTwo);

				IDictionary<string, object> actualThree = SendJsonGetUnderlying(env, "{\"a\" : null, \"c\": 101, \"d\": 102}\n");
				IDictionary<string, object> expectedThree = new LinkedHashMap<string, object>();
				expectedThree.Put("a", null);
				expectedThree.Put("b", null);
				expectedThree.Put("c", 101);
				expectedThree.Put("d", 102);
				CompareDictionaries(expectedThree, actualThree);

				IDictionary<string, object> actualFour = SendJsonGetUnderlying(env, "{}\n");
				IDictionary<string, object> expectedFour = new LinkedHashMap<string, object>();
				expectedFour.Put("a", null);
				expectedFour.Put("b", null);
				CompareDictionaries(expectedFour, actualFour);

				env.Milestone(0);

				using (IEnumerator<EventBean> it = env.Statement("s0").GetEnumerator()) {
					CompareMapWBean(expectedOne, it.Advance());
					CompareMapWBean(expectedTwo, it.Advance());
					CompareMapWBean(expectedThree, it.Advance());
					CompareMapWBean(expectedFour, it.Advance());
				}

				env.UndeployAll();
			}
		}

		private static JsonEventObject SendJsonGetUnderlying(
			RegressionEnvironment env,
			string json)
		{
			env.SendEventJson(json, "JsonEvent");
			EventBean eventBean = env.Listener("s0").AssertOneGetNewAndReset();
			return (JsonEventObject) eventBean.Underlying;
		}

		private static void CompareMapWBean(
			IDictionary<string, object> expected,
			EventBean @event)
		{
			CompareDictionaries(expected, (IDictionary<string, object>) @event.Underlying);
		}
	}
} // end of namespace
