///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;


using com.espertech.esper.common.client.json.util;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

using Newtonsoft.Json.Linq;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.@event.json
{
	public class EventJsonDocSamples
	{
		public static IList<RegressionExecution> Executions()
		{
			IList<RegressionExecution> execs = new List<RegressionExecution>();
			execs.Add(new EventJsonDocSamplesCarLocUpdate());
			execs.Add(new EventJsonDocSamplesBook());
			execs.Add(new EventJsonDocSamplesCake());
			execs.Add(new EventJsonDocDynamicEmpty());
			execs.Add(new EventJsonDocApplicationClass());
			return execs;
		}

		internal class EventJsonDocApplicationClass : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var epl = "@public @buseventtype create json schema JsonEvent(person " +
				          typeof(MyLocalPersonEvent).FullName +
				          ");\n" +
				          "@Name('s0') select * from JsonEvent;\n";
				env.CompileDeploy(epl).AddListener("s0");

				var uuid = Guid.NewGuid();
				var json = new JObject(
					new JProperty(
						"person",
						new JObject(
							new JProperty("name", "Joe"),
							new JProperty("id", uuid.ToString()))));
				env.SendEventJson(json.ToString(), "JsonEvent");
				var person = (MyLocalPersonEvent) env.Listener("s0").AssertOneGetNewAndReset().Get("person");

				Assert.AreEqual("Joe", person.name);
				Assert.AreEqual(uuid, person.id);

				env.UndeployAll();
			}
		}

		internal class EventJsonDocDynamicEmpty : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var epl = "@JsonSchema(dynamic=true) @public @buseventtype create json schema SensorEvent();\n" +
				          "@Name('s0') select entityID? as entityId, temperature? as temperature, status? as status, \n" +
				          "\tentityName? as entityName, vt? as vt, flags? as flags from SensorEvent;\n" +
				          "@Name('s1') select entityName?.english as englishEntityName from SensorEvent";
				env.CompileDeploy(epl).AddListener("s0").AddListener("s1");

				var json = "{\n" +
				           "   \"entityID\":\"cd9f930e\",\n" +
				           "   \"temperature\" : 70,\n" +
				           "   \"status\" : true,\n" +
				           "   \"entityName\":{\n" +
				           "      \"english\":\"Cooling Water Temperature\"\n" +
				           "   },\n" +
				           "   \"vt\":[\"2014-08-20T15:30:23.524Z\"],\n" +
				           "   \"flags\" : null\n" +
				           "}";
				env.SendEventJson(json, "SensorEvent");

				EPAssertionUtil.AssertProps(
					env.Listener("s0").AssertOneGetNewAndReset(),
					"entityId,temperature,status,entityName,vt,flags".SplitCsv(),
					new object[] {
						"cd9f930e", 70, true, Collections.SingletonMap("english", "Cooling Water Temperature"), new object[] {"2014-08-20T15:30:23.524Z"},
						null
					});
				EPAssertionUtil.AssertProps(
					env.Listener("s1").AssertOneGetNewAndReset(),
					"englishEntityName".SplitCsv(),
					new object[] {"Cooling Water Temperature"});

				var sender = (EventSenderJson) env.Runtime.EventService.GetEventSender("SensorEvent");
				sender.Parse(json);

				env.UndeployAll();
			}
		}

		internal class EventJsonDocSamplesCarLocUpdate : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var path = new RegressionPath();
				env.CompileDeploy("@public @buseventtype create json schema CarLocUpdateEvent(carId string, direction int)", path);
				env.CompileDeploy("@Name('s0') select carId, direction, count(*) as cnt from CarLocUpdateEvent(direction = 1)#time(1 min)", path)
					.AddListener("s0");

				var @event = "{" +
				             "  \"carId\" : \"A123456\",\n" +
				             "  \"direction\" : 1\n" +
				             "}";
				env.SendEventJson(@event, "CarLocUpdateEvent");

				EPAssertionUtil.AssertProps(
					env.Listener("s0").AssertOneGetNewAndReset(),
					"carId,direction,cnt".SplitCsv(),
					new object[] {"A123456", 1, 1L});

				env.UndeployAll();
			}
		}

		internal class EventJsonDocSamplesBook : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var epl = "create json schema Names(lastname string, firstname string);\n" +
				          "@public @buseventtype create json schema BookEvent(isbn string, author Names, editor Names, title string, category string[]);\n" +
				          "@Name('s0') select isbn, author.lastname as authorName, editor.lastname as editorName, \n" +
				          "  category[0] as primaryCategory from BookEvent;\n";
				env.CompileDeploy(epl).AddListener("s0");

				var json = "{ \n" +
				           "  \"isbn\": \"123-456-222\",  \n" +
				           " \"author\": \n" +
				           "    {\n" +
				           "      \"lastname\": \"Doe\",\n" +
				           "      \"firstname\": \"Jane\"\n" +
				           "    },\n" +
				           "\"editor\": \n" +
				           "    {\n" +
				           "      \"lastname\": \"Smith\",\n" +
				           "      \"firstname\": \"Jane\"\n" +
				           "    },\n" +
				           "  \"title\": \"The Ultimate Database Study Guide\",  \n" +
				           "  \"category\": [\"Non-Fiction\", \"Technology\"]\n" +
				           " }";
				env.SendEventJson(json, "BookEvent");

				EPAssertionUtil.AssertProps(
					env.Listener("s0").AssertOneGetNewAndReset(),
					"isbn,authorName,editorName,primaryCategory".SplitCsv(),
					new object[] {"123-456-222", "Doe", "Smith", "Non-Fiction"});

				env.UndeployAll();
			}
		}

		internal class EventJsonDocSamplesCake : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var epl = "create json schema IdAndType(int string, type string);\n" +
				          "create json schema Batters(machine string, batter IdAndType[]);\n" +
				          "@public @buseventtype create json schema CakeEvent(id string, type string, name string, batters Batters, topping IdAndType[]);\n" +
				          "@Name('s0') select name, batters.batter[0].type as firstBatterType,\n" +
				          "  topping[0].type as firstToppingType, batters.machine as batterMachine, batters.batter.countOf() as countBatters,\n" +
				          "  topping.countOf() as countToppings from CakeEvent;\n";
				env.CompileDeploy(epl).AddListener("s0");

				var json = "{\n" +
				           "  \"id\": \"0001\",\n" +
				           "  \"type\": \"donut\",\n" +
				           "  \"name\": \"Cake\",\n" +
				           "  \"batters\": \t\n" +
				           "  {\n" +
				           "    \"machine\": \"machine A\",\n" +
				           "    \"batter\":\n" +
				           "    [\n" +
				           "      { \"id\": \"1001\", \"type\": \"Regular\" },\n" +
				           "      { \"id\": \"1002\", \"type\": \"Chocolate\" },\n" +
				           "      { \"id\": \"1003\", \"type\": \"Blueberry\" },\n" +
				           "      { \"id\": \"1004\", \"type\": \"Devil's Food\" }\n" +
				           "    ]\n" +
				           "  },\n" +
				           "  \"topping\":\n" +
				           "  [\n" +
				           "    { \"id\": \"5001\", \"type\": \"None\" },\n" +
				           "    { \"id\": \"5002\", \"type\": \"Glazed\" },\n" +
				           "    { \"id\": \"5005\", \"type\": \"Sugar\" },\n" +
				           "    { \"id\": \"5007\", \"type\": \"Powdered Sugar\" }\n" +
				           "  ]\n" +
				           "}";
				env.SendEventJson(json, "CakeEvent");

				EPAssertionUtil.AssertProps(
					env.Listener("s0").AssertOneGetNewAndReset(),
					"name,firstBatterType,firstToppingType,batterMachine,countBatters,countToppings".SplitCsv(),
					new object[] {"Cake", "Regular", "None", "machine A", 4, 4});

				env.UndeployAll();
			}
		}

		public class MyLocalPersonEvent
		{
			public string name;
			public Guid id;
		}
	}
} // end of namespace
