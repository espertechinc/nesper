///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.json.util;
using com.espertech.esper.common.client.render;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.datetime;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.json;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

namespace com.espertech.esper.regressionlib.suite.@event.json
{
	public class EventJsonProvidedUnderlyingClass
	{
		public static IList<RegressionExecution> Executions()
		{
			IList<RegressionExecution> execs = new List<RegressionExecution>();
			execs.Add(new EventJsonProvidedClassUsersEvent());
			execs.Add(new EventJsonProvidedClassUsersEventWithCreateSchema());
			execs.Add(new EventJsonProvidedClassClientsEvent());
			execs.Add(new EventJsonProvidedClassClientsEventWithCreateSchema());
			execs.Add(new EventJsonProvidedClassInvalid());
			execs.Add(new EventJsonProvidedClassFieldTypeMismatchInvalid());
			execs.Add(new EventJsonProvidedClassCreateSchemaTypeMismatchInvalid());
			execs.Add(new EventJsonProvidedClassSetNullForPrimitive());
			execs.Add(new EventJsonProvidedClassWArrayPatternInsert());
			return execs;
		}

		internal class EventJsonProvidedClassWArrayPatternInsert : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				env.AdvanceTime(0);
				string epl =
					"@public @buseventtype @JsonSchema(className='" +
					typeof(MyLocalJsonProvidedEventOne).FullName +
					"') create json schema EventOne();\n" +
					"@public @buseventtype @JsonSchema(className='" +
					typeof(MyLocalJsonProvidedEventTwo).FullName +
					"') create json schema EventTwo();\n" +
					"@public @buseventtype @JsonSchema(className='" +
					typeof(MyLocalJsonProvidedEventOut).FullName +
					"') create json schema EventOut();\n" +
					"@name('s0') insert into EventOut select s as startEvent, e as endEvents from pattern [" +
					"every s=EventOne -> e=EventTwo(id=s.id) until timer:interval(10 sec)]";
				env.CompileDeploy(epl).AddListener("s0");

				env.SendEventJson("{\"id\":\"G1\"}", "EventOne");
				env.SendEventJson("{\"id\":\"G1\",\"val\":2}", "EventTwo");
				env.SendEventJson("{\"id\":\"G1\",\"val\":3}", "EventTwo");
				env.AdvanceTime(10000);

				MyLocalJsonProvidedEventOut @out = (MyLocalJsonProvidedEventOut) env.Listener("s0").AssertOneGetNewAndReset().Underlying;
				Assert.AreEqual("G1", @out.startEvent.id);
				Assert.AreEqual("G1", @out.endEvents[0].id);
				Assert.AreEqual(2, @out.endEvents[0].val);
				Assert.AreEqual("G1", @out.endEvents[1].id);
				Assert.AreEqual(3, @out.endEvents[1].val);

				env.UndeployAll();
			}
		}

		internal class EventJsonProvidedClassSetNullForPrimitive : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string epl = "@public @buseventtype @JsonSchema(className='" +
				             typeof(MyLocalJsonProvidedPrimitiveInt).FullName +
				             "') create json schema MySchema();\n" +
				             "insert into MySchema select intBoxed as primitiveInt from SupportBean;\n" +
				             "@name('s0') select * from MySchema;\n";
				env.CompileDeploy(epl).AddListener("s0");

				env.SendEventBean(new SupportBean());
				Assert.AreEqual(-1, env.Listener("s0").AssertOneGetNewAndReset().Get("primitiveInt"));

				env.UndeployAll();
			}
		}

		internal class EventJsonProvidedClassCreateSchemaTypeMismatchInvalid : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string prefix = "@JsonSchema(className='" + typeof(MyLocalJsonProvidedStringInt).FullName + "') ";
				string epl = prefix + "create json schema MySchema(c0 int)";
				TryInvalidSchema(
					env,
					epl,
					typeof(MyLocalJsonProvidedStringInt),
					"Public field 'c0' of class '%CLASS%' declared as type 'java.lang.String' cannot receive a value of type 'java.lang.Integer'");
			}
		}

		internal class EventJsonProvidedClassFieldTypeMismatchInvalid : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string prefix = EventRepresentationChoice.JSONCLASSPROVIDED.GetAnnotationTextWJsonProvided(typeof(MyLocalJsonProvidedStringInt));
				TryInvalidSchema(
					env,
					prefix + "select 0 as dummy from SupportBean",
					typeof(MyLocalJsonProvidedStringInt),
					"Failed to find public field 'dummy' on class '%CLASS%'");
				TryInvalidSchema(
					env,
					prefix + "select 0 as c0 from SupportBean",
					typeof(MyLocalJsonProvidedStringInt),
					"Public field 'c0' of class '%CLASS%' declared as type 'java.lang.String' cannot receive a value of type 'java.lang.Integer'");
				TryInvalidSchema(
					env,
					prefix + "select new {a=0} as c0 from SupportBean",
					typeof(MyLocalJsonProvidedStringInt),
					"Public field 'c0' of class '%CLASS%' declared as type 'java.lang.String' cannot receive a value of type 'java.util.Map'");
			}
		}

		internal class EventJsonProvidedClassInvalid : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string epl;

				epl = "@JsonSchema(dynamic=true, className='" + typeof(SupportClientsEvent).FullName + "') create json schema Clients()";
				TryInvalidCompile(
					env,
					epl,
					"The dynamic flag is not supported when used with a provided JSON event class");

				epl = "create json schema ABC();\n" +
				      "@JsonSchema(className='" +
				      typeof(SupportClientsEvent).FullName +
				      "') create json schema Clients() inherits ABC";
				TryInvalidCompile(
					env,
					epl,
					"Specifying a supertype is not supported with a provided JSON event class");

				epl = "@JsonSchema(className='" + typeof(MyLocalNonPublicInvalid).FullName + "') create json schema Clients()";
				TryInvalidCompile(
					env,
					epl,
					"Provided JSON event class is not public");

				epl = "@JsonSchema(className='" + typeof(MyLocalNoDefaultCtorInvalid).FullName + "') create json schema Clients()";
				TryInvalidCompile(
					env,
					epl,
					"Provided JSON event class does not have a public default constructor or is a non-static inner class");

				epl = "@JsonSchema(className='" + typeof(MyLocalInstanceInvalid).FullName + "') create json schema Clients()";
				TryInvalidCompile(
					env,
					epl,
					"Provided JSON event class does not have a public default constructor or is a non-static inner class");
			}
		}

		internal class EventJsonProvidedClassClientsEventWithCreateSchema : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string schema =
					"create json schema Partner(id long, name string, since java.time.OffsetDateTime);\n" +
					"create json schema Client(" +
					"_id long,\n" +
					"`index` int,\n" +
					"guid java.util.UUID,\n" +
					"isActive boolean,\n" +
					"balance BigDecimal,\n" +
					"picture string,\n" +
					"age int,\n" +
					"eyeColor " +
					typeof(SupportClientsEvent.EyeColor).FullName +
					",\n" +
					"name string,\n" +
					"gender string,\n" +
					"company string,\n" +
					"emails string[],\n" +
					"phones long[],\n" +
					"address string,\n" +
					"about string,\n" +
					"registered java.time.LocalDate,\n" +
					"latitude double,\n" +
					"longitude double,\n" +
					"tags string[],\n" +
					"partners Partner[]\n" +
					");\n" +
					"@public @buseventtype create json schema Clients(clients Client[]);\n" +
					"@name('s0') select * from Clients;\n";
				env.CompileDeploy(schema).AddListener("s0");

				env.SendEventJson(ClientsJson, "Clients");

				JsonEventObject @event = (JsonEventObject) env.Listener("s0").AssertOneGetNewAndReset().Underlying;
				Assert.AreEqual(ClientsJsonReplaceWhitespace, @event.ToString());

				env.UndeployAll();
			}
		}

		internal class EventJsonProvidedClassUsersEventWithCreateSchema : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string schema =
					"create json schema Friend(id string, name string);\n" +
					"create json schema User(" +
					"_id string,\n" +
					"`index` int,\n" +
					"guid string,\n" +
					"isActive boolean,\n" +
					"balance string,\n" +
					"picture string,\n" +
					"age int,\n" +
					"eyeColor string,\n" +
					"name string,\n" +
					"gender string,\n" +
					"company string,\n" +
					"email string,\n" +
					"phone string,\n" +
					"address string,\n" +
					"about string,\n" +
					"registered string,\n" +
					"latitude double,\n" +
					"longitude double,\n" +
					"tags string[],\n" +
					"friends Friend[],\n" +
					"greeting string,\n" +
					"favoriteFruit string\n" +
					");\n" +
					"@public @buseventtype create json schema Users(users User[]);\n" +
					"@name('s0') select * from Users;\n";
				env.CompileDeploy(schema).AddListener("s0");

				env.SendEventJson(UsersJson, "Users");

				JsonEventObject @event = (JsonEventObject) env.Listener("s0").AssertOneGetNewAndReset().Underlying;
				Assert.AreEqual(UsersJsonReplaceWhitespace, @event.ToString());

				env.UndeployAll();
			}
		}

		internal class EventJsonProvidedClassClientsEvent : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string epl = "@public @buseventtype @JsonSchema(className='" +
				             typeof(SupportClientsEvent).FullName +
				             "') create json schema Clients();\n" +
				             "@name('s0') select * from Clients;";
				env.CompileDeploy(epl).AddListener("s0");

				// try sender parse-only
				EventSenderJson sender = (EventSenderJson) env.Runtime.EventService.GetEventSender("Clients");
				SupportClientsEvent clients = (SupportClientsEvent) sender.Parse(ClientsJson);
				AssertClientsPremade(clients);

				// try send-event
				sender.SendEvent(ClientsJson);
				EventBean @event = env.Listener("s0").AssertOneGetNewAndReset();
				AssertClientsPremade((SupportClientsEvent) @event.Underlying);

				// try write
				JSONEventRenderer render = env.Runtime.RenderEventService.GetJSONRenderer(@event.EventType);
				Assert.AreEqual(ClientsJsonReplaceWhitespace, render.Render(@event));

				env.UndeployAll();
			}

			private void AssertClientsPremade(SupportClientsEvent clients)
			{
				Assert.AreEqual(1, clients.clients.Count);
				SupportClientsEvent.Client first = clients.clients[0];
				Assert.AreEqual(ClientObject.clients[0], first);
			}
		}

		internal class EventJsonProvidedClassUsersEvent : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string epl = "@public @buseventtype @JsonSchema(className='" +
				             typeof(SupportUsersEvent).FullName +
				             "') create json schema Users();\n" +
				             "@name('s0') select * from Users;";
				env.CompileDeploy(epl).AddListener("s0");

				// try sender parse-only
				EventSenderJson sender = (EventSenderJson) env.Runtime.EventService.GetEventSender("Users");
				SupportUsersEvent users = (SupportUsersEvent) sender.Parse(UsersJson);
				AssertUsersPremade(users);

				// try send-event
				sender.SendEvent(UsersJson);
				EventBean @event = env.Listener("s0").AssertOneGetNewAndReset();
				AssertUsersPremade((SupportUsersEvent) @event.Underlying);

				// try write
				JSONEventRenderer render = env.Runtime.RenderEventService.GetJSONRenderer(@event.EventType);
				Assert.AreEqual(UsersJsonReplaceWhitespace, render.Render(@event));

				env.UndeployAll();
			}

			private void AssertUsersPremade(SupportUsersEvent users)
			{
				Assert.AreEqual(2, users.users.Count);
				SupportUsersEvent.User first = users.users[0];
				Assert.AreEqual("45166552176594981065", first._id);
				SupportUsersEvent.User second = users.users[1];
				Assert.AreEqual("23504426278646846580", second._id);
				EPAssertionUtil.AssertEqualsExactOrder(UserObject.users.ToArray(), users.users.ToArray());
			}
		}

		private static SupportUsersEvent UserObject {
			get {
				SupportUsersEvent.Friend u0f0 = new SupportUsersEvent.Friend();
				u0f0.id = "3987";
				u0f0.name = "dWwKYheGgTZejIMYdglXvvrWAzUqsk";
				SupportUsersEvent.Friend u0f1 = new SupportUsersEvent.Friend();
				u0f1.id = "4673";
				u0f1.name = "EqVIiZyuhSCkWXvqSxgyQihZaiwSra";

				SupportUsersEvent.User u0 = new SupportUsersEvent.User();
				u0._id = "45166552176594981065";
				u0.index = 692815193;
				u0.guid = "oLzFhQttjjCGmijYulZg";
				u0.isActive = true;
				u0.balance = "XtMtTkSfmQtyRHS1086c";
				u0.picture = "i0wzskJJ2SvxXL1UbXEzy332JksricBvitJkeKt3JcoZGx10JxhbdkQ8YoyJ0cL1MGFwC9bpAzQXSFBEcAUQ8lGQekvJZDeJ5C5p";
				u0.age = 23;
				u0.eyeColor = "XqoN9IzOBVixZhrofJpd";
				u0.name = "xBavaMCv6j0eYkT6HMcB";
				u0.gender = "VnuP3BaA3flaA6dLGvqO";
				u0.company = "L9yT2IsGTjOgQc0prb4r";
				u0.email = "rfmlFaVxGBSZFybTIKz0";
				u0.phone = "vZsxzv8DlzimJauTSBre";
				u0.address = "fZgFDv9tX1oonnVjcNVv";
				u0.about = "WysqSAN1psGsJBCFSR7P";
				u0.registered = "Lsw4RK5gtyNWGYp9dDhy";
				u0.latitude = 2.6395313895198393;
				u0.longitude = 110.5363758848371;
				u0.tags = Arrays.AsList("Hx6qJTHe8y", "23vYh8ILj6", "geU64sSQgH", "ezNI8Gx5vq");
				u0.friends = Arrays.AsList(u0f0, u0f1);
				u0.greeting = "xfS8vUXYq4wzufBLP6CY";
				u0.favoriteFruit = "KT0tVAxXRawtbeQIWAot";

				SupportUsersEvent.User u1 = new SupportUsersEvent.User();
				u1._id = "23504426278646846580";
				u1.index = 675066974;
				u1.guid = "MfiCc1n1WfG6d6iXcdNf";
				u1.isActive = true;
				u1.balance = "OQEwTOBvwK0b8dJYFpBU";
				u1.picture = "avtMGQxSrO1h86V7KVaKaWOWohfCnENnMfKcLbydRSMq2eHc533hC4n7GMwsGhXz10EyVBhnP1LUFZ0ooZd9GmIynRomjCjP8tEN";
				u1.age = 33;
				u1.eyeColor = "Fjsm1nmwyphAw7DRnfZ7";
				u1.name = "NnjrrCj1TTObhT9gHMH2";
				u1.gender = "ISVVoyQ4cbEjQVoFy5z0";
				u1.company = "AfcGdkzUQMzg69yjvmL5";
				u1.email = "mXLtlNEJjw5heFiYykwV";
				u1.phone = "zXbn9iJ5ljRHForNOa79";
				u1.address = "XXQUcaDIX2qpyZKtw8zl";
				u1.about = "GBVYHdxZYgGCey6yogEi";
				u1.registered = "bTJynDeyvZRbsYQIW9ys";
				u1.latitude = 16.675958191062414;
				u1.longitude = 114.20858157883556;
				u1.tags = EmptyList<string>.Instance;
				u1.friends = EmptyList<SupportUsersEvent.Friend>.Instance;
				u1.greeting = "EQqKZyiGnlyHeZf9ojnl";
				u1.favoriteFruit = "9aUx0u6G840i0EeKFM4Z";

				SupportUsersEvent @event = new SupportUsersEvent();
				@event.users = Arrays.AsList(u0, u1);
				return @event;
			}
		}

		private static string UsersJsonReplaceWhitespace => UsersJson.Replace("\n", "").Replace(" ", "");

		private static string ClientsJsonReplaceWhitespace => ClientsJson.Replace("\n", "").Replace(" ", "");

		private static void TryInvalidSchema(
			RegressionEnvironment env,
			string epl,
			Type provided,
			string message)
		{
			TryInvalidCompile(env, epl, message.Replace("%CLASS%", provided.FullName));
		}

		private static string UsersJson =>
			"{\n" +
			"  \"users\": [\n" +
			"    {\n" +
			"      \"_id\": \"45166552176594981065\",\n" +
			"      \"index\": 692815193,\n" +
			"      \"guid\": \"oLzFhQttjjCGmijYulZg\",\n" +
			"      \"isActive\": true,\n" +
			"      \"balance\": \"XtMtTkSfmQtyRHS1086c\",\n" +
			"      \"picture\": \"i0wzskJJ2SvxXL1UbXEzy332JksricBvitJkeKt3JcoZGx10JxhbdkQ8YoyJ0cL1MGFwC9bpAzQXSFBEcAUQ8lGQekvJZDeJ5C5p\",\n" +
			"      \"age\": 23,\n" +
			"      \"eyeColor\": \"XqoN9IzOBVixZhrofJpd\",\n" +
			"      \"name\": \"xBavaMCv6j0eYkT6HMcB\",\n" +
			"      \"gender\": \"VnuP3BaA3flaA6dLGvqO\",\n" +
			"      \"company\": \"L9yT2IsGTjOgQc0prb4r\",\n" +
			"      \"email\": \"rfmlFaVxGBSZFybTIKz0\",\n" +
			"      \"phone\": \"vZsxzv8DlzimJauTSBre\",\n" +
			"      \"address\": \"fZgFDv9tX1oonnVjcNVv\",\n" +
			"      \"about\": \"WysqSAN1psGsJBCFSR7P\",\n" +
			"      \"registered\": \"Lsw4RK5gtyNWGYp9dDhy\",\n" +
			"      \"latitude\": 2.6395313895198393,\n" +
			"      \"longitude\": 110.5363758848371,\n" +
			"      \"tags\": [\n" +
			"        \"Hx6qJTHe8y\",\n" +
			"        \"23vYh8ILj6\",\n" +
			"        \"geU64sSQgH\",\n" +
			"        \"ezNI8Gx5vq\"\n" +
			"      ],\n" +
			"      \"friends\": [\n" +
			"        {\n" +
			"          \"id\": \"3987\",\n" +
			"          \"name\": \"dWwKYheGgTZejIMYdglXvvrWAzUqsk\"\n" +
			"        },\n" +
			"        {\n" +
			"          \"id\": \"4673\",\n" +
			"          \"name\": \"EqVIiZyuhSCkWXvqSxgyQihZaiwSra\"\n" +
			"        }\n" +
			"      ],\n" +
			"      \"greeting\": \"xfS8vUXYq4wzufBLP6CY\",\n" +
			"      \"favoriteFruit\": \"KT0tVAxXRawtbeQIWAot\"\n" +
			"    },\n" +
			"    {\n" +
			"      \"_id\": \"23504426278646846580\",\n" +
			"      \"index\": 675066974,\n" +
			"      \"guid\": \"MfiCc1n1WfG6d6iXcdNf\",\n" +
			"      \"isActive\": true,\n" +
			"      \"balance\": \"OQEwTOBvwK0b8dJYFpBU\",\n" +
			"      \"picture\": \"avtMGQxSrO1h86V7KVaKaWOWohfCnENnMfKcLbydRSMq2eHc533hC4n7GMwsGhXz10EyVBhnP1LUFZ0ooZd9GmIynRomjCjP8tEN\",\n" +
			"      \"age\": 33,\n" +
			"      \"eyeColor\": \"Fjsm1nmwyphAw7DRnfZ7\",\n" +
			"      \"name\": \"NnjrrCj1TTObhT9gHMH2\",\n" +
			"      \"gender\": \"ISVVoyQ4cbEjQVoFy5z0\",\n" +
			"      \"company\": \"AfcGdkzUQMzg69yjvmL5\",\n" +
			"      \"email\": \"mXLtlNEJjw5heFiYykwV\",\n" +
			"      \"phone\": \"zXbn9iJ5ljRHForNOa79\",\n" +
			"      \"address\": \"XXQUcaDIX2qpyZKtw8zl\",\n" +
			"      \"about\": \"GBVYHdxZYgGCey6yogEi\",\n" +
			"      \"registered\": \"bTJynDeyvZRbsYQIW9ys\",\n" +
			"      \"latitude\": 16.675958191062414,\n" +
			"      \"longitude\": 114.20858157883556,\n" +
			"      \"tags\": [],\n" +
			"      \"friends\": [],\n" +
			"      \"greeting\": \"EQqKZyiGnlyHeZf9ojnl\",\n" +
			"      \"favoriteFruit\": \"9aUx0u6G840i0EeKFM4Z\"\n" +
			"    }\n" +
			"  ]\n" +
			"}";

		private static string ClientsJson =>
			"{\n" +
			"  \"clients\": [\n" +
			"    {\n" +
			"      \"_id\": 4063715686146184700,\n" +
			"      \"index\": 1951037102,\n" +
			"      \"guid\": \"b7dc7f66-4f6d-4f03-14d7-83da210dfba6\",\n" +
			"      \"isActive\": true,\n" +
			"      \"balance\": 0.8509300187678505,\n" +
			"      \"picture\": \"TB6izKKNN5ihBLFiRekRmcntxaVAke1rL7rhUDQPACG4DxrLCvOfKNjy5KZl9Rg0QUknq7RFifVbZg4RbnVjdEThMdD1UAZQk3Le\",\n" +
			"      \"age\": 9,\n" +
			"      \"eyeColor\": \"BROWN\",\n" +
			"      \"name\": \"PTgbx3rVSkXaSVlKV2SK\",\n" +
			"      \"gender\": \"D7TzVALMRaVmCEkC8bzT\",\n" +
			"      \"company\": \"m4dcMP9VIFlniImW4Ezc\",\n" +
			"      \"emails\": [\n" +
			"        \"puYIMDORrusZXRUZjMQM\",\n" +
			"        \"vxMKjpYtPjJPRvDYuCjZ\"\n" +
			"      ],\n" +
			"      \"phones\": [\n" +
			"        1206223281\n" +
			"      ],\n" +
			"      \"address\": \"Hf2YGJnogcwkIwj5hTJz\",\n" +
			"      \"about\": \"d0FcpUETNRV2ky15EmBc\",\n" +
			"      \"registered\": \"1961-10-09\",\n" +
			"      \"latitude\": 26.91225115361936,\n" +
			"      \"longitude\": 74.26256260138875,\n" +
			"      \"tags\": [],\n" +
			"      \"partners\": [\n" +
			"        {\n" +
			"          \"id\": -4413101314901277000,\n" +
			"          \"name\": \"YjiSvZzaXYhJMkZddxlVPdHfoIthbY\",\n" +
			"          \"since\": \"1974-11-01T07:58:27.373380998Z\"\n" +
			"        },\n" +
			"        {\n" +
			"          \"id\": -7309654308880836000,\n" +
			"          \"name\": \"HxHDrtpnXAxCooxasYVLZLqYImRLzW\",\n" +
			"          \"since\": \"1927-02-02T14:34:09.672667878Z\"\n" +
			"        }\n" +
			"      ]\n" +
			"    }\n" +
			"  ]\n" +
			"}";

		private static SupportClientsEvent ClientObject {
			get {
				SupportClientsEvent.Client client = new SupportClientsEvent.Client();
				client._id = 4063715686146184700L;
				client.index = 1951037102;
				client.guid = Guid.Parse("b7dc7f66-4f6d-4f03-14d7-83da210dfba6");
				client.isActive = true;
				client.balance = 0.8509300187678505m;
				client.picture = "TB6izKKNN5ihBLFiRekRmcntxaVAke1rL7rhUDQPACG4DxrLCvOfKNjy5KZl9Rg0QUknq7RFifVbZg4RbnVjdEThMdD1UAZQk3Le";
				client.age = 9;
				client.eyeColor = SupportClientsEvent.EyeColor.BROWN;
				client.name = "PTgbx3rVSkXaSVlKV2SK";
				client.gender = "D7TzVALMRaVmCEkC8bzT";
				client.company = "m4dcMP9VIFlniImW4Ezc";
				client.emails = new string[] {"puYIMDORrusZXRUZjMQM", "vxMKjpYtPjJPRvDYuCjZ"};
				client.phones = new long[] {1206223281};
				client.address = "Hf2YGJnogcwkIwj5hTJz";
				client.about = "d0FcpUETNRV2ky15EmBc";
				client.registered = DateTimeParsingFunctions.ParseDefaultDateTimeOffset("1961-10-09").DateTime;
				client.latitude = 26.91225115361936;
				client.longitude = 74.26256260138875;
				client.tags = EmptyList<string>.Instance;
				;

				SupportClientsEvent.Partner partnerOne = new SupportClientsEvent.Partner();
				partnerOne.id = -4413101314901277000L;
				partnerOne.name = "YjiSvZzaXYhJMkZddxlVPdHfoIthbY";
				partnerOne.since = DateTimeParsingFunctions.ParseDefault("1974-11-01T07:58:27.373380998Z");
				SupportClientsEvent.Partner partnerTwo = new SupportClientsEvent.Partner();
				partnerTwo.id = -7309654308880836000L;
				partnerTwo.name = "HxHDrtpnXAxCooxasYVLZLqYImRLzW";
				partnerTwo.since = DateTimeParsingFunctions.ParseDefault("1927-02-02T14:34:09.672667878Z");
				client.partners = Arrays.AsList(partnerOne, partnerTwo);

				SupportClientsEvent @event = new SupportClientsEvent();
				@event.clients = Collections.SingletonList(client);
				return @event;
			}
		}

		[Serializable]
		public class MyLocalNoDefaultCtorInvalid
		{
			public MyLocalNoDefaultCtorInvalid(string id)
			{
			}
		}

		[Serializable]
		private class MyLocalNonPublicInvalid
		{
			public MyLocalNonPublicInvalid()
			{
			}
		}

		[Serializable]
		public class MyLocalInstanceInvalid
		{
			public MyLocalInstanceInvalid()
			{
			}
		}

		[Serializable]
		public class MyLocalJsonProvidedStringInt
		{
			public string c0;
		}

		[Serializable]
		public class MyLocalJsonProvidedPrimitiveInt
		{
			public int primitiveInt = -1;
		}

		[Serializable]
		public class MyLocalJsonProvidedEventOne
		{
			public string id;
		}

		[Serializable]
		public class MyLocalJsonProvidedEventTwo
		{
			public string id;
			public int val;
		}

		[Serializable]
		public class MyLocalJsonProvidedEventOut
		{
			public MyLocalJsonProvidedEventOne startEvent;
			public MyLocalJsonProvidedEventTwo[] endEvents;
		}
	}
} // end of namespace
