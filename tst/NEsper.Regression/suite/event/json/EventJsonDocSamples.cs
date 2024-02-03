///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.json.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

using Newtonsoft.Json.Linq;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.@event.json
{
    public class EventJsonDocSamples
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithSamplesCarLocUpdate(execs);
            WithSamplesBook(execs);
            WithSamplesCake(execs);
            WithDynamicEmpty(execs);
            WithApplicationClass(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithApplicationClass(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventJsonDocApplicationClass());
            return execs;
        }

        public static IList<RegressionExecution> WithDynamicEmpty(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventJsonDocDynamicEmpty());
            return execs;
        }

        public static IList<RegressionExecution> WithSamplesCake(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventJsonDocSamplesCake());
            return execs;
        }

        public static IList<RegressionExecution> WithSamplesBook(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventJsonDocSamplesBook());
            return execs;
        }

        public static IList<RegressionExecution> WithSamplesCarLocUpdate(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventJsonDocSamplesCarLocUpdate());
            return execs;
        }

        private class EventJsonDocApplicationClass : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@public @buseventtype create json schema JsonEvent(person " +
                          typeof(MyLocalPersonEvent).FullName +
                          ");\n" +
                          "@name('s0') select * from JsonEvent;\n";
                env.CompileDeploy(epl).AddListener("s0");

                var uuid = new Guid();
                var json = new JObject(
                    new JProperty(
                        "person",
                        new JObject(
                            new JProperty("name", "Joe"),
                            new JProperty("Id", uuid.ToString())
                        )));
                env.SendEventJson(json.ToString(), "JsonEvent");
                env.AssertEventNew(
                    "s0",
                    @event => {
                        var person = (MyLocalPersonEvent)@event.Get("person");
                        ClassicAssert.AreEqual("Joe", person.name);
                        ClassicAssert.AreEqual(uuid, person.id);
                    });

                env.UndeployAll();
            }
        }

        private class EventJsonDocDynamicEmpty : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@JsonSchema(Dynamic=true) @public @buseventtype create json schema SensorEvent();\n" +
                          "@name('s0') select entityID? as entityId, temperature? as temperature, status? as status, \n" +
                          "\tentityName? as entityName, vt? as vt, flags? as flags from SensorEvent;\n" +
                          "@name('s1') select entityName?.english as englishEntityName from SensorEvent";
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

                env.AssertPropsNew(
                    "s0",
                    "entityId,temperature,status,entityName,vt,flags".SplitCsv(),
                    new object[] {
                        "cd9f930e", 70, true, Collections.SingletonMap("english", "Cooling Water Temperature"),
                        new object[] { "2014-08-20T15:30:23.524Z" },
                        null
                    });
                env.AssertPropsNew(
                    "s1",
                    "englishEntityName".SplitCsv(),
                    new object[] { "Cooling Water Temperature" });

                env.AssertThat(
                    () => {
                        var sender = (EventSenderJson)env.Runtime.EventService.GetEventSender("SensorEvent");
                        sender.Parse(json);
                    });

                env.UndeployAll();
            }
        }

        private class EventJsonDocSamplesCarLocUpdate : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@public @buseventtype create json schema CarLocUpdateEvent(CarId string, Direction int)",
                    path);
                env.CompileDeploy(
                        "@name('s0') select CarId, Direction, count(*) as cnt from CarLocUpdateEvent(Direction = 1)#time(1 min)",
                        path)
                    .AddListener("s0");

                var @event = "{" +
                             "  \"CarId\" : \"A123456\",\n" +
                             "  \"Direction\" : 1\n" +
                             "}";
                env.SendEventJson(@event, "CarLocUpdateEvent");

                env.AssertPropsNew(
                    "s0",
                    "CarId,Direction,cnt".SplitCsv(),
                    new object[] { "A123456", 1, 1L });

                env.UndeployAll();
            }
        }

        private class EventJsonDocSamplesBook : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create json schema Names(lastname string, firstname string);\n" +
                          "@public @buseventtype create json schema BookEvent(isbn string, Author Names, editor Names, title string, category string[]);\n" +
                          "@name('s0') select isbn, Author.lastname as authorName, editor.lastname as editorName, \n" +
                          "  category[0] as primaryCategory from BookEvent;\n";
                env.CompileDeploy(epl).AddListener("s0");

                var json = "{ \n" +
                           "  \"isbn\": \"123-456-222\",  \n" +
                           " \"Author\": \n" +
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

                env.AssertPropsNew(
                    "s0",
                    "isbn,authorName,editorName,primaryCategory".SplitCsv(),
                    new object[] { "123-456-222", "Doe", "Smith", "Non-Fiction" });

                env.UndeployAll();
            }
        }

        private class EventJsonDocSamplesCake : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create json schema IdAndType(int string, type string);\n" +
                          "create json schema Batters(machine string, batter IdAndType[]);\n" +
                          "@public @buseventtype create json schema CakeEvent(Id string, type string, name string, batters Batters, topping IdAndType[]);\n" +
                          "@name('s0') select name, batters.batter[0].type as firstBatterType,\n" +
                          "  topping[0].type as firstToppingType, batters.machine as batterMachine, batters.batter.countOf() as countBatters,\n" +
                          "  topping.countOf() as countToppings from CakeEvent;\n";
                env.CompileDeploy(epl).AddListener("s0");

                var json = "{\n" +
                           "  \"Id\": \"0001\",\n" +
                           "  \"type\": \"donut\",\n" +
                           "  \"name\": \"Cake\",\n" +
                           "  \"batters\": \t\n" +
                           "  {\n" +
                           "    \"machine\": \"machine A\",\n" +
                           "    \"batter\":\n" +
                           "    [\n" +
                           "      { \"Id\": \"1001\", \"type\": \"Regular\" },\n" +
                           "      { \"Id\": \"1002\", \"type\": \"Chocolate\" },\n" +
                           "      { \"Id\": \"1003\", \"type\": \"Blueberry\" },\n" +
                           "      { \"Id\": \"1004\", \"type\": \"Devil's Food\" }\n" +
                           "    ]\n" +
                           "  },\n" +
                           "  \"topping\":\n" +
                           "  [\n" +
                           "    { \"Id\": \"5001\", \"type\": \"None\" },\n" +
                           "    { \"Id\": \"5002\", \"type\": \"Glazed\" },\n" +
                           "    { \"Id\": \"5005\", \"type\": \"Sugar\" },\n" +
                           "    { \"Id\": \"5007\", \"type\": \"Powdered Sugar\" }\n" +
                           "  ]\n" +
                           "}";
                env.SendEventJson(json, "CakeEvent");

                env.AssertPropsNew(
                    "s0",
                    "name,firstBatterType,firstToppingType,batterMachine,countBatters,countToppings".SplitCsv(),
                    new object[] { "Cake", "Regular", "None", "machine A", 4, 4 });

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