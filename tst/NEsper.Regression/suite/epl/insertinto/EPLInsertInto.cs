///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using Avro.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.json.util;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;
using com.espertech.esper.compat.magic;
using com.espertech.esper.compiler.client;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.runtime.client.scopetest;

using NEsper.Avro.Extensions;
using NEsper.Avro.Util.Support;

using Newtonsoft.Json.Linq;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;
using static com.espertech.esper.regressionlib.support.util.SupportAdminUtil;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;
using SupportBeanSimple = com.espertech.esper.regressionlib.support.bean.SupportBeanSimple;

namespace com.espertech.esper.regressionlib.suite.epl.insertinto
{
    public class EPLInsertInto
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithAssertionWildcardRecast(execs);
            WithJoinWildcard(execs);
            WithWithOutputLimitAndSort(execs);
            WithStaggeredWithWildcard(execs);
            WithInsertFromPattern(execs);
            WithInsertIntoPlusPattern(execs);
            WithNullType(execs);
            WithChain(execs);
            WithMultiBeanToMulti(execs);
            WithSingleBeanToMulti(execs);
            WithProvidePartialCols(execs);
            WithRStreamOMToStmt(execs);
            WithNamedColsOMToStmt(execs);
            WithNamedColsEPLToOMStmt(execs);
            WithNamedColsSimple(execs);
            WithNamedColsStateless(execs);
            WithNamedColsWildcard(execs);
            WithNamedColsJoin(execs);
            WithNamedColsJoinWildcard(execs);
            WithUnnamedSimple(execs);
            WithUnnamedWildcard(execs);
            WithUnnamedJoin(execs);
            WithTypeMismatchInvalid(execs);
            WithEventRepresentationsSimple(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithEventRepresentationsSimple(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoEventRepresentationsSimple());
            return execs;
        }

        public static IList<RegressionExecution> WithTypeMismatchInvalid(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoTypeMismatchInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithUnnamedJoin(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoUnnamedJoin());
            return execs;
        }

        public static IList<RegressionExecution> WithUnnamedWildcard(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoUnnamedWildcard());
            return execs;
        }

        public static IList<RegressionExecution> WithUnnamedSimple(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoUnnamedSimple());
            return execs;
        }

        public static IList<RegressionExecution> WithNamedColsJoinWildcard(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoNamedColsJoinWildcard());
            return execs;
        }

        public static IList<RegressionExecution> WithNamedColsJoin(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoNamedColsJoin());
            return execs;
        }

        public static IList<RegressionExecution> WithNamedColsWildcard(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoNamedColsWildcard());
            return execs;
        }

        public static IList<RegressionExecution> WithNamedColsStateless(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoNamedColsStateless());
            return execs;
        }

        public static IList<RegressionExecution> WithNamedColsSimple(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoNamedColsSimple());
            return execs;
        }

        public static IList<RegressionExecution> WithNamedColsEPLToOMStmt(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoNamedColsEPLToOMStmt());
            return execs;
        }

        public static IList<RegressionExecution> WithNamedColsOMToStmt(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoNamedColsOMToStmt());
            return execs;
        }

        public static IList<RegressionExecution> WithRStreamOMToStmt(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoRStreamOMToStmt());
            return execs;
        }

        public static IList<RegressionExecution> WithProvidePartialCols(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoProvidePartialCols());
            return execs;
        }

        public static IList<RegressionExecution> WithSingleBeanToMulti(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoSingleBeanToMulti());
            return execs;
        }

        public static IList<RegressionExecution> WithMultiBeanToMulti(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoMultiBeanToMulti());
            return execs;
        }

        public static IList<RegressionExecution> WithChain(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoChain());
            return execs;
        }

        public static IList<RegressionExecution> WithNullType(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoNullType());
            return execs;
        }

        public static IList<RegressionExecution> WithInsertIntoPlusPattern(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoInsertIntoPlusPattern());
            return execs;
        }

        public static IList<RegressionExecution> WithInsertFromPattern(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoInsertFromPattern());
            return execs;
        }

        public static IList<RegressionExecution> WithStaggeredWithWildcard(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoStaggeredWithWildcard());
            return execs;
        }

        public static IList<RegressionExecution> WithWithOutputLimitAndSort(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoWithOutputLimitAndSort());
            return execs;
        }

        public static IList<RegressionExecution> WithJoinWildcard(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoJoinWildcard());
            return execs;
        }

        public static IList<RegressionExecution> WithAssertionWildcardRecast(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoAssertionWildcardRecast());
            return execs;
        }

        private static void TryAssertsVariant(
            RegressionEnvironment env,
            string stmtText,
            EPStatementObjectModel model,
            string typeName)
        {
            typeName = TypeHelper.MaskTypeName(typeName);

            var path = new RegressionPath();
            // Attach listener to feed
            if (model != null) {
                model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("fl"));
                env.CompileDeploy(model, path);
            }
            else {
                env.CompileDeploy("@Name('fl') " + stmtText, path);
            }

            env.AddListener("fl");

            // send event for joins to match on
            env.SendEventBean(new SupportBean_A("myId"));

            // Attach delta statement to statement and add listener
            stmtText = "@Name('rld') select MIN(delta) as minD, max(delta) as maxD " +
                       "from " +
                       typeName +
                       "#time(60)";
            env.CompileDeploy(stmtText, path).AddListener("rld");

            // Attach prodict statement to statement and add listener
            stmtText = "@Name('rlp') select min(product) as minP, max(product) as maxP " +
                       "from " +
                       typeName +
                       "#time(60)";
            env.CompileDeploy(stmtText, path).AddListener("rlp");

            env.AdvanceTime(0); // Set the time to 0 seconds

            // send events
            SendEvent(env, 20, 10);
            AssertReceivedFeed(env.Listener("fl"), 10, 200);
            AssertReceivedMinMax(env.Listener("rld"), env.Listener("rlp"), 10, 10, 200, 200);

            SendEvent(env, 50, 25);
            AssertReceivedFeed(env.Listener("fl"), 25, 25 * 50);
            AssertReceivedMinMax(env.Listener("rld"), env.Listener("rlp"), 10, 25, 200, 1250);

            SendEvent(env, 5, 2);
            AssertReceivedFeed(env.Listener("fl"), 3, 2 * 5);
            AssertReceivedMinMax(env.Listener("rld"), env.Listener("rlp"), 3, 25, 10, 1250);

            env.AdvanceTime(10 * 1000); // Set the time to 10 seconds

            SendEvent(env, 13, 1);
            AssertReceivedFeed(env.Listener("fl"), 12, 13);
            AssertReceivedMinMax(env.Listener("rld"), env.Listener("rlp"), 3, 25, 10, 1250);

            env.AdvanceTime(61 * 1000); // Set the time to 61 seconds
            AssertReceivedMinMax(env.Listener("rld"), env.Listener("rlp"), 12, 12, 13, 13);
        }

        private static void AssertReceivedMinMax(
            SupportListener resultListenerDelta,
            SupportListener resultListenerProduct,
            int minDelta,
            int maxDelta,
            int minProduct,
            int maxProduct)
        {
            Assert.AreEqual(1, resultListenerDelta.NewDataList.Count);
            Assert.AreEqual(1, resultListenerDelta.LastNewData.Length);
            Assert.AreEqual(1, resultListenerProduct.NewDataList.Count);
            Assert.AreEqual(1, resultListenerProduct.LastNewData.Length);
            Assert.AreEqual(minDelta, resultListenerDelta.LastNewData[0].Get("minD"));
            Assert.AreEqual(maxDelta, resultListenerDelta.LastNewData[0].Get("maxD"));
            Assert.AreEqual(minProduct, resultListenerProduct.LastNewData[0].Get("minP"));
            Assert.AreEqual(maxProduct, resultListenerProduct.LastNewData[0].Get("maxP"));
            resultListenerDelta.Reset();
            resultListenerProduct.Reset();
        }

        private static void AssertReceivedFeed(
            SupportListener feedListener,
            int delta,
            int product)
        {
            Assert.AreEqual(1, feedListener.NewDataList.Count);
            Assert.AreEqual(1, feedListener.LastNewData.Length);
            Assert.AreEqual(delta, feedListener.LastNewData[0].Get("delta"));
            Assert.AreEqual(product, feedListener.LastNewData[0].Get("product"));
            feedListener.Reset();
        }

        private static SupportBean SendEvent(
            RegressionEnvironment env,
            int intPrimitive,
            int intBoxed)
        {
            var bean = new SupportBean();
            bean.TheString = "myId";
            bean.IntPrimitive = intPrimitive;
            bean.IntBoxed = intBoxed;
            env.SendEventBean(bean);
            return bean;
        }

        private static void AssertSimple(
            SupportListener listener,
            string myString,
            int myInt,
            string additionalString,
            int additionalInt)
        {
            Assert.IsTrue(listener.GetAndClearIsInvoked());
            var eventBean = listener.LastNewData[0];
            Assert.AreEqual(myString, eventBean.Get("MyString"));
            Assert.AreEqual(myInt, eventBean.Get("MyInt"));
            if (additionalString != null) {
                Assert.AreEqual(additionalString, eventBean.Get("concat"));
                Assert.AreEqual(additionalInt, eventBean.Get("summed"));
            }
        }

        private static void SendEvent(
            RegressionEnvironment env,
            string symbol,
            double price)
        {
            var bean = new SupportMarketDataBean(symbol, price, null, null);
            env.SendEventBean(bean);
        }

        private static void SendSimpleEvent(
            RegressionEnvironment env,
            string theString,
            int val)
        {
            env.SendEventBean(new SupportBeanSimple(theString, val));
        }

        private static void AssertJoinWildcard(
            EventRepresentationChoice? rep,
            SupportListener listener,
            object eventS0,
            object eventS1)
        {
            Assert.IsTrue(listener.GetAndClearIsInvoked());
            Assert.AreEqual(1, listener.LastNewData.Length);
            Assert.AreEqual(2, listener.LastNewData[0].EventType.PropertyNames.Length);
            Assert.IsTrue(listener.LastNewData[0].EventType.IsProperty("S0"));
            Assert.IsTrue(listener.LastNewData[0].EventType.IsProperty("S1"));
            if (rep != null && (rep.Value.IsJsonEvent() || rep.Value.IsJsonProvidedClassEvent())) {
                Assert.AreEqual(
                    listener.LastNewData[0].Get("S0").ToString().RemoveWhitespace(),
                    eventS0.ToString().RemoveWhitespace());
                Assert.AreEqual(
                    listener.LastNewData[0].Get("S1").ToString().RemoveWhitespace(),
                    eventS1.ToString().RemoveWhitespace());
            }
            else {
                Assert.AreSame(eventS0, listener.LastNewData[0].Get("S0"));
                Assert.AreSame(eventS1, listener.LastNewData[0].Get("S1"));
            }

            Assert.IsTrue(rep == null || rep.Value.MatchesClass(listener.LastNewData[0].Underlying.GetType()));
        }

        private static void TryAssertionJoinWildcard(
            RegressionEnvironment env,
            bool bean,
            EventRepresentationChoice? rep)
        {
            string schema;

            if (bean) {
                schema = "@Name('schema1') create schema S0 as " +
                         typeof(SupportBean).FullName +
                         ";\n" +
                         "@Name('schema2') create schema S1 as " +
                         typeof(SupportBean_A).FullName +
                         ";\n";
            }
            else if (rep == null) {
                throw new ArgumentException(nameof(rep));
            }
            else if (rep.Value.IsMapEvent()) {
                Console.WriteLine($"Rep = {rep.Value}");
                schema = "@Name('schema1') create map schema S0 as (TheString string);\n" +
                         "@Name('schema2') create map schema S1 as (Id string);\n";
            }
            else if (rep.Value.IsObjectArrayEvent()) {
                Console.WriteLine($"Rep = {rep.Value}");
                schema = "@Name('schema1') create objectarray schema S0 as (TheString string);\n" +
                         "@Name('schema2') create objectarray schema S1 as (Id string);\n";
            }
            else if (rep.Value.IsAvroEvent()) {
                Console.WriteLine($"Rep = {rep.Value}");
                schema = "@Name('schema1') create avro schema S0 as (TheString string);\n" +
                         "@Name('schema2') create avro schema S1 as (Id string);\n";
            }
            else if (rep.Value.IsJsonEvent()) {
                Console.WriteLine($"Rep = {rep.Value}");
                schema = "@Name('schema1') create json schema S0 as (TheString string);\n" +
                         "@Name('schema2') create json schema S1 as (Id string);\n";
            }
            else if (rep.Value.IsJsonProvidedClassEvent()) {
                Console.WriteLine($"Rep = {rep.Value}");
                schema = "@Name('schema1') @JsonSchema(ClassName='" +
                         typeof(MyLocalJsonProvidedS0).FullName +
                         "') create json schema S0 as ();\n" +
                         "@Name('schema2') @JsonSchema(ClassName='" +
                         typeof(MyLocalJsonProvidedS1).FullName +
                         "') create json schema S1 as ();\n";
            }
            else {
                schema = null;
                Assert.Fail();
            }

            var path = new RegressionPath();
            env.CompileDeployWBusPublicType(schema, path);

            var textOne = "@Name('s1') " +
                          (bean ? "" : rep.Value.GetAnnotationTextWJsonProvided<MyLocalJsonProvidedJoin>()) +
                          "insert into event2 select * " +
                          "from S0#length(100) as S0, S1#length(5) as S1 " +
                          "where S0.TheString = S1.Id";
            env.CompileDeploy(textOne, path).AddListener("s1");

            var annoText = bean ? "" : rep.Value.GetAnnotationTextWJsonProvided<MyLocalJsonProvidedJoin>();
            var textTwo = $"@Name('s2') {annoText} select * from event2#length(10)";
            env.CompileDeploy(textTwo, path).AddListener("s2");

            // send event for joins to match on
            object eventS1;
            if (bean) {
                eventS1 = new SupportBean_A("myId");
                env.SendEventBean(eventS1, "S1");
            }
            else if (rep.Value.IsMapEvent()) {
                eventS1 = Collections.SingletonDataMap("Id", "myId");
                env.SendEventMap((IDictionary<string, object>) eventS1, "S1");
            }
            else if (rep.Value.IsObjectArrayEvent()) {
                eventS1 = new object[] {"myId"};
                env.SendEventObjectArray((object[]) eventS1, "S1");
            }
            else if (rep.Value.IsAvroEvent()) {
                var theEvent = new GenericRecord(
                    SupportAvroUtil.GetAvroSchema(
                            env.Runtime.EventTypeService.GetEventType(
                                env.DeploymentId("schema1"),
                                "S1"))
                        .AsRecordSchema());
                theEvent.Put("Id", "myId");
                eventS1 = theEvent;
                env.SendEventAvro(theEvent, "S1");
            }
            else if (rep.Value.IsJsonEvent() || rep.Value.IsJsonProvidedClassEvent()) {
                var @object = new JObject();
                @object.Add("Id", "myId");
                eventS1 = @object.ToString();
                env.SendEventJson((string) eventS1, "S1");
            }
            else {
                throw new ArgumentException();
            }

            object eventS0;
            if (bean) {
                eventS0 = new SupportBean("myId", -1);
                env.SendEventBean(eventS0, "S0");
            }
            else if (rep.Value.IsMapEvent()) {
                eventS0 = Collections.SingletonDataMap("TheString", "myId");
                env.SendEventMap((IDictionary<string, object>) eventS0, "S0");
            }
            else if (rep.Value.IsObjectArrayEvent()) {
                eventS0 = new object[] {"myId"};
                env.SendEventObjectArray((object[]) eventS0, "S0");
            }
            else if (rep.Value.IsAvroEvent()) {
                var theEvent = new GenericRecord(
                    SupportAvroUtil.GetAvroSchema(
                            env.Runtime.EventTypeService.GetEventType(
                                env.DeploymentId("schema1"),
                                "S0"))
                        .AsRecordSchema());
                theEvent.Put("TheString", "myId");
                eventS0 = theEvent;
                env.SendEventAvro(theEvent, "S0");
            }
            else if (rep.Value.IsJsonEvent() || rep.Value.IsJsonProvidedClassEvent()) {
                var @object = new JObject();
                @object.Add("TheString", "myId");
                eventS0 = @object.ToString();
                env.SendEventJson((string) eventS0, "S0");
            }
            else {
                throw new ArgumentException();
            }

            AssertJoinWildcard(rep, env.Listener("s1"), eventS0, eventS1);
            AssertJoinWildcard(rep, env.Listener("s2"), eventS0, eventS1);

            env.UndeployAll();
        }

        private static void TryAssertionRepresentationSimple(
            RegressionEnvironment env,
            EventRepresentationChoice rep,
            IDictionary<EventRepresentationChoice, Consumer<object>> assertions)
        {
            var epl = rep.GetAnnotationTextWJsonProvided<MyLocalJsonProvided>() +
                      " insert into SomeStream select TheString, IntPrimitive from SupportBean;\n" +
                      "@Name('s0') select * from SomeStream;\n";
            env.CompileDeploy(epl).AddListener("s0");

            env.SendEventBean(new SupportBean("E1", 10));
            var assertion = assertions.Get(rep);
            if (assertion == null) {
                Assert.Fail("No assertion provided for type " + rep);
            }

            assertion.Invoke(env.Listener("s0").AssertOneGetNewAndReset().Underlying);

            env.UndeployAll();
        }

        private static void TryAssertionWildcardRecast(
            RegressionEnvironment env,
            bool sourceBean,
            EventRepresentationChoice? sourceType,
            bool targetBean,
            EventRepresentationChoice? targetType)
        {
            try {
                TryAssertionWildcardRecastInternal(env, sourceBean, sourceType, targetBean, targetType);
            }
            finally {
                // cleanup
                env.UndeployAll();
            }
        }

        private static void TryAssertionWildcardRecastInternal(
            RegressionEnvironment env,
            bool sourceBean,
            EventRepresentationChoice? sourceType,
            bool targetBean,
            EventRepresentationChoice? targetType)
        {
            // declare source type
            string schemaEPL;
            if (sourceBean) {
                schemaEPL = "create schema SourceSchema as " + TypeHelper.MaskTypeName<MyP0P1EventSource>();
            }
            else {
                schemaEPL = sourceType.Value.GetAnnotationTextWJsonProvided<MyLocalJsonProvidedSourceSchema>() +
                            "create schema SourceSchema as (P0 string, P1 int)";
            }

            var path = new RegressionPath();
            env.CompileDeployWBusPublicType(schemaEPL, path);

            // declare target type
            if (targetBean) {
                var eventTargetType = TypeHelper.MaskTypeName<MyP0P1EventTarget>();
                env.CompileDeploy(
                    $"create schema TargetSchema as {eventTargetType}",
                    path);
            }
            else {
                env.CompileDeploy(
                    targetType.Value.GetAnnotationTextWJsonProvided<MyLocalJsonProvidedTargetContainedSchema>() +
                    "create schema TargetContainedSchema as (C0 int)",
                    path);
                env.CompileDeploy(
                    targetType.Value.GetAnnotationTextWJsonProvided<MyLocalJsonProvidedTargetSchema>() +
                    "create schema TargetSchema (P0 string, P1 int, C0 TargetContainedSchema)",
                    path);
            }

            // insert-into and select
            env.CompileDeploy("insert into TargetSchema select * from SourceSchema", path);
            env.CompileDeploy("@Name('s0') select * from TargetSchema", path).AddListener("s0");

            // send event
            if (sourceBean) {
                env.SendEventBean(new MyP0P1EventSource("a", 10), "SourceSchema");
            }
            else if (sourceType.Value.IsMapEvent()) {
                IDictionary<string, object> map = new Dictionary<string, object>();
                map.Put("P0", "a");
                map.Put("P1", 10);
                env.SendEventMap(map, "SourceSchema");
            }
            else if (sourceType.Value.IsObjectArrayEvent()) {
                env.SendEventObjectArray(new object[] {"a", 10}, "SourceSchema");
            }
            else if (sourceType.Value.IsAvroEvent()) {
                var schema = SchemaBuilder.Record(
                    "schema",
                    TypeBuilder.RequiredString("P0"),
                    TypeBuilder.RequiredString("P1"),
                    TypeBuilder.RequiredString("C0"));
                var record = new GenericRecord(schema);
                record.Put("P0", "a");
                record.Put("P1", 10);
                env.SendEventAvro(record, "SourceSchema");
            }
            else if (sourceType.Value.IsJsonEvent() || sourceType.Value.IsJsonProvidedClassEvent()) {
                env.SendEventJson("{\"P0\": \"a\", \"P1\": 10}", "SourceSchema");
            }
            else {
                Assert.Fail();
            }

            // assert
            var @event = env.Listener("s0").AssertOneGetNewAndReset();
            EPAssertionUtil.AssertProps(@event, "P0,P1,C0".SplitCsv(), new object[] {"a", 10, null});

            env.UndeployAll();
        }

        private static SupportMarketDataBean MakeMarketDataEvent(string symbol)
        {
            return new SupportMarketDataBean(symbol, 0, 0L, null);
        }

        internal class EPLInsertIntoEventRepresentationsSimple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                IDictionary<EventRepresentationChoice, Consumer<object>> assertions = new Dictionary<EventRepresentationChoice, Consumer<object>>();
                assertions.Put(
                    EventRepresentationChoice.OBJECTARRAY,
                    und => { EPAssertionUtil.AssertEqualsExactOrder(new object[] {"E1", 10}, (object[]) und); });
                Consumer<object> mapAssertion = und => EPAssertionUtil.AssertPropsMap(
                    und.AsStringDictionary(),
                    "TheString,IntPrimitive".SplitCsv(),
                    "E1",
                    10);
                assertions.Put(EventRepresentationChoice.MAP, mapAssertion);
                assertions.Put(EventRepresentationChoice.DEFAULT, mapAssertion);
                assertions.Put(
                    EventRepresentationChoice.AVRO,
                    und => {
                        var rec = (GenericRecord) und;
                        Assert.AreEqual("E1", rec.Get("TheString"));
                        Assert.AreEqual(10, rec.Get("IntPrimitive"));
                    });
                assertions.Put(
                    EventRepresentationChoice.JSON,
                    und => {
                        var rec = (JsonEventObject) und;
                        Assert.AreEqual("E1", rec.Get("TheString"));
                        Assert.AreEqual(10, rec.Get("IntPrimitive"));
                    });
                assertions.Put(
                    EventRepresentationChoice.JSONCLASSPROVIDED,
                    und => {
                        var rec = (MyLocalJsonProvided) und;
                        Assert.AreEqual("E1", rec.TheString);
                        Assert.AreEqual(10, rec.IntPrimitive);
                    });

                foreach (var rep in EventRepresentationChoiceExtensions.Values()) {
                    TryAssertionRepresentationSimple(env, rep, assertions);
                }
            }
        }

        internal class EPLInsertIntoRStreamOMToStmt : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var model = new EPStatementObjectModel();
                model.InsertInto = InsertIntoClause.Create("Event_1_RSOM", new string[0], StreamSelector.RSTREAM_ONLY);
                model.SelectClause = SelectClause.Create().Add("IntPrimitive", "IntBoxed");
                model.FromClause = FromClause.Create(FilterStream.Create("SupportBean"));
                model = env.CopyMayFail(model);
                model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));

                var epl = "@Name('s0') insert rstream into Event_1_RSOM " +
                          "select IntPrimitive, IntBoxed " +
                          "from SupportBean";
                Assert.AreEqual(epl, model.ToEPL());

                var modelTwo = env.EplToModel(model.ToEPL());
                model = env.CopyMayFail(modelTwo);
                Assert.AreEqual(epl, model.ToEPL());
            }
        }

        internal class EPLInsertIntoNamedColsOMToStmt : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var model = new EPStatementObjectModel();
                model.InsertInto = InsertIntoClause.Create("Event_1_OMS", "delta", "product");
                model.SelectClause = SelectClause.Create()
                    .Add(Expressions.Minus("IntPrimitive", "IntBoxed"), "deltaTag")
                    .Add(Expressions.Multiply("IntPrimitive", "IntBoxed"), "productTag");
                model.FromClause = FromClause.Create(
                    FilterStream.Create("SupportBean").AddView(View.Create("length", Expressions.Constant(100))));
                model = env.CopyMayFail(model);

                TryAssertsVariant(env, null, model, "Event_1_OMS");

                var epl = "@Name('fl') insert into Event_1_OMS(delta, product) " +
                          "select IntPrimitive-IntBoxed as deltaTag, IntPrimitive*IntBoxed as productTag " +
                          "from SupportBean#length(100)";
                Assert.AreEqual(epl, model.ToEPL());
                Assert.AreEqual(epl, env.Statement("fl").GetProperty(StatementProperty.EPL));

                env.UndeployAll();
            }
        }

        internal class EPLInsertIntoNamedColsEPLToOMStmt : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('fl') insert into Event_1_EPL(delta, product) " +
                          "select IntPrimitive-IntBoxed as deltaTag, IntPrimitive*IntBoxed as productTag " +
                          "from SupportBean#length(100)";

                var model = env.EplToModel(epl);
                model = env.CopyMayFail(model);
                Assert.AreEqual(epl, model.ToEPL());

                TryAssertsVariant(env, null, model, "Event_1_EPL");
                Assert.AreEqual(epl, env.Statement("fl").GetProperty(StatementProperty.EPL));
                env.UndeployAll();
            }
        }

        internal class EPLInsertIntoNamedColsSimple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "insert into Event_1VO (delta, product) " +
                               "select IntPrimitive - IntBoxed as deltaTag, IntPrimitive * IntBoxed as productTag " +
                               "from SupportBean#length(100)";

                TryAssertsVariant(env, stmtText, null, "Event_1VO");
                env.UndeployAll();
            }
        }

        internal class EPLInsertIntoNamedColsStateless : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtTextStateless = "insert into Event_1VOS (delta, product) " +
                                        "select IntPrimitive - IntBoxed as deltaTag, IntPrimitive * IntBoxed as productTag " +
                                        "from SupportBean";
                TryAssertsVariant(env, stmtTextStateless, null, "Event_1VOS");
                env.UndeployAll();
            }
        }

        internal class EPLInsertIntoNamedColsWildcard : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "insert into Event_1W (delta, product) " +
                               "select * from SupportBean#length(100)";
                TryInvalidCompile(env, stmtText, "Wildcard not allowed when insert-into specifies column order");

                // test insert wildcard to wildcard
                var stmtSelectText = "@Name('i0') insert into ABCStream select * from SupportBean";
                env.CompileDeploy(stmtSelectText).AddListener("i0");
                Assert.IsTrue(env.Statement("i0").EventType is BeanEventType);

                env.SendEventBean(new SupportBean("E1", 1));
                Assert.AreEqual("E1", env.Listener("i0").AssertOneGetNew().Get("TheString"));
                Assert.IsTrue(env.Listener("i0").AssertOneGetNew().Underlying is SupportBean);

                env.UndeployAll();
            }
        }

        internal class EPLInsertIntoNamedColsJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "insert into Event_1J (delta, product) " +
                               "select IntPrimitive - IntBoxed as deltaTag, IntPrimitive * IntBoxed as productTag " +
                               "from SupportBean#length(100) as S0," +
                               "SupportBean_A#length(100) as S1 " +
                               " where S0.TheString = S1.Id";

                TryAssertsVariant(env, stmtText, null, "Event_1J");
                env.UndeployAll();
            }
        }

        internal class EPLInsertIntoNamedColsJoinWildcard : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "insert into Event_1JW (delta, product) " +
                               "select * " +
                               "from SupportBean#length(100) as S0," +
                               "SupportBean_A#length(100) as S1 " +
                               " where S0.TheString = S1.Id";

                try {
                    env.CompileWCheckedEx(stmtText);
                    Assert.Fail();
                }
                catch (EPCompileException) {
                    // Expected
                }
            }
        }

        internal class EPLInsertIntoUnnamedSimple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "insert into Event_1_2 " +
                               "select IntPrimitive - IntBoxed as delta, IntPrimitive * IntBoxed as product " +
                               "from SupportBean#length(100)";

                TryAssertsVariant(env, stmtText, null, "Event_1_2");
                env.UndeployAll();
            }
        }

        internal class EPLInsertIntoUnnamedWildcard : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var stmtText = "@Name('stmt1') insert into event1 select * from SupportBean#length(100)";
                var otherText = "@Name('stmt2') select * from event1#length(10)";

                // Attach listener to feed
                env.CompileDeploy(stmtText, path).AddListener("stmt1");
                env.CompileDeploy(otherText, path).AddListener("stmt2");

                var theEvent = SendEvent(env, 10, 11);
                Assert.IsTrue(env.Listener("stmt1").GetAndClearIsInvoked());
                Assert.AreEqual(1, env.Listener("stmt1").LastNewData.Length);
                Assert.AreEqual(10, env.Listener("stmt1").LastNewData[0].Get("IntPrimitive"));
                Assert.AreEqual(11, env.Listener("stmt1").LastNewData[0].Get("IntBoxed"));
                Assert.AreEqual(22, env.Listener("stmt1").LastNewData[0].EventType.PropertyNames.Length);
                Assert.AreSame(theEvent, env.Listener("stmt1").LastNewData[0].Underlying);

                Assert.IsTrue(env.Listener("stmt2").GetAndClearIsInvoked());
                Assert.AreEqual(1, env.Listener("stmt2").LastNewData.Length);
                Assert.AreEqual(10, env.Listener("stmt2").LastNewData[0].Get("IntPrimitive"));
                Assert.AreEqual(11, env.Listener("stmt2").LastNewData[0].Get("IntBoxed"));
                Assert.AreEqual(22, env.Listener("stmt2").LastNewData[0].EventType.PropertyNames.Length);
                Assert.AreSame(theEvent, env.Listener("stmt2").LastNewData[0].Underlying);

                env.UndeployAll();
            }
        }

        internal class EPLInsertIntoUnnamedJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "insert into Event_1_2J " +
                               "select IntPrimitive - IntBoxed as delta, IntPrimitive * IntBoxed as product " +
                               "from SupportBean#length(100) as S0," +
                               "SupportBean_A#length(100) as S1 " +
                               " where S0.TheString = S1.Id";

                TryAssertsVariant(env, stmtText, null, "Event_1_2J");

                // assert type metadata
                var type = env.Statement("fl").EventType;
                Assert.AreEqual(NameAccessModifier.PUBLIC, type.Metadata.AccessModifier);
                Assert.AreEqual(EventTypeTypeClass.STREAM, type.Metadata.TypeClass);
                Assert.AreEqual(EventTypeApplicationType.MAP, type.Metadata.ApplicationType);
                Assert.AreEqual("Event_1_2J", type.Metadata.Name);
                Assert.AreEqual(EventTypeBusModifier.NONBUS, type.Metadata.BusModifier);

                env.UndeployAll();
            }
        }

        internal class EPLInsertIntoTypeMismatchInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // invalid wrapper types
                var epl = "insert into MyStream select * from pattern[a=SupportBean];\n" +
                          "insert into MyStream select * from pattern[a=SupportBean_S0];\n";
                TryInvalidCompile(
                    env,
                    epl,
                    "Event type named 'MyStream' has already been declared with differing column name or type information: Type by name 'stmt0_pat_0_0' in property 'a' expected event type 'SupportBean' but receives event type 'SupportBean_S0'");
            }
        }

        internal class EPLInsertIntoMultiBeanToMulti : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy(
                        "@Name('s0') insert into SupportObjectArrayOneDim" +
                        " select window(*) @eventbean as Arr" +
                        " from SupportBean#keepall")
                    .AddListener("s0");
                AssertStatelessStmt(env, "s0", false);

                var e1 = new SupportBean("E1", 1);
                env.SendEventBean(e1);
                var resultOne = (SupportObjectArrayOneDim) env.Listener("s0").AssertOneGetNewAndReset().Underlying;
                EPAssertionUtil.AssertEqualsExactOrder(
                    resultOne.Arr,
                    new object[] {e1});

                var e2 = new SupportBean("E2", 2);
                env.SendEventBean(e2);
                var resultTwo = (SupportObjectArrayOneDim) env.Listener("s0").AssertOneGetNewAndReset().Underlying;
                EPAssertionUtil.AssertEqualsExactOrder(
                    resultTwo.Arr,
                    new object[] {e1, e2});

                env.UndeployAll();
            }
        }

        internal class EPLInsertIntoSingleBeanToMulti : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("create schema EventOne(sbarr SupportBean[])", path);
                env.CompileDeploy(
                    "insert into EventOne select maxby(IntPrimitive) as sbarr from SupportBean as sb",
                    path);
                env.CompileDeploy("@Name('s0') select * from EventOne", path).AddListener("s0");

                var bean = new SupportBean("E1", 1);
                env.SendEventBean(bean);
                var events = (EventBean[]) env.Listener("s0").AssertOneGetNewAndReset().Get("sbarr");
                Assert.AreEqual(1, events.Length);
                Assert.AreSame(bean, events[0].Underlying);

                env.UndeployAll();
            }
        }

        internal class EPLInsertIntoAssertionWildcardRecast : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // bean to OA/Map/bean
                foreach (var rep in EventRepresentationChoiceExtensions.Values()) {
                    TryAssertionWildcardRecast(env, true, null, false, rep);
                }

                try {
                    TryAssertionWildcardRecast(env, true, null, true, null);
                    Assert.Fail();
                }
                catch (Exception ex) {
                    AssertMessage(
                        ex.InnerException.Message,
                        "Expression-returned event type 'SourceSchema' with underlying type '" +
                        typeof(MyP0P1EventSource).CleanName() +
                        "' cannot be converted to target event type 'TargetSchema' with underlying type ");
                }

                // OA
                TryAssertionWildcardRecast(
                    env,
                    false,
                    EventRepresentationChoice.OBJECTARRAY,
                    false,
                    EventRepresentationChoice.OBJECTARRAY);
                TryAssertionWildcardRecast(
                    env,
                    false,
                    EventRepresentationChoice.OBJECTARRAY,
                    false,
                    EventRepresentationChoice.MAP);
                TryAssertionWildcardRecast(
                    env,
                    false,
                    EventRepresentationChoice.OBJECTARRAY,
                    false,
                    EventRepresentationChoice.AVRO);
                TryAssertionWildcardRecast(
                    env,
                    false,
                    EventRepresentationChoice.OBJECTARRAY,
                    false,
                    EventRepresentationChoice.JSON);
                TryAssertionWildcardRecast(
                    env,
                    false,
                    EventRepresentationChoice.OBJECTARRAY,
                    true,
                    null);

                // Map
                TryAssertionWildcardRecast(
                    env,
                    false,
                    EventRepresentationChoice.MAP,
                    false,
                    EventRepresentationChoice.OBJECTARRAY);
                TryAssertionWildcardRecast(
                    env,
                    false,
                    EventRepresentationChoice.MAP,
                    false,
                    EventRepresentationChoice.MAP);
                TryAssertionWildcardRecast(
                    env,
                    false,
                    EventRepresentationChoice.MAP,
                    false,
                    EventRepresentationChoice.AVRO);
                TryAssertionWildcardRecast(
                    env,
                    false,
                    EventRepresentationChoice.MAP,
                    false,
                    EventRepresentationChoice.JSON);
                TryAssertionWildcardRecast(
                    env,
                    false,
                    EventRepresentationChoice.MAP,
                    true,
                    null);

                // Avro
                TryAssertionWildcardRecast(
                    env,
                    false,
                    EventRepresentationChoice.AVRO,
                    false,
                    EventRepresentationChoice.OBJECTARRAY);
                TryAssertionWildcardRecast(
                    env,
                    false,
                    EventRepresentationChoice.AVRO,
                    false,
                    EventRepresentationChoice.MAP);
                TryAssertionWildcardRecast(
                    env,
                    false,
                    EventRepresentationChoice.AVRO,
                    false,
                    EventRepresentationChoice.AVRO);
                TryAssertionWildcardRecast(
                    env,
                    false,
                    EventRepresentationChoice.AVRO,
                    false,
                    EventRepresentationChoice.JSON);
                TryAssertionWildcardRecast(
                    env,
                    false,
                    EventRepresentationChoice.AVRO,
                    true,
                    null);

                // Json
                TryAssertionWildcardRecast(
                    env,
                    false,
                    EventRepresentationChoice.JSON,
                    false,
                    EventRepresentationChoice.OBJECTARRAY);
                TryAssertionWildcardRecast(
                    env,
                    false,
                    EventRepresentationChoice.JSON,
                    false,
                    EventRepresentationChoice.MAP);
                TryAssertionWildcardRecast(
                    env,
                    false,
                    EventRepresentationChoice.JSON,
                    false,
                    EventRepresentationChoice.AVRO);
                TryAssertionWildcardRecast(
                    env,
                    false,
                    EventRepresentationChoice.JSON,
                    false,
                    EventRepresentationChoice.JSON);
                TryAssertionWildcardRecast(
                    env,
                    false,
                    EventRepresentationChoice.JSON,
                    true,
                    null);

                // Json-Provided-Class
                TryAssertionWildcardRecast(
                    env,
                    false,
                    EventRepresentationChoice.JSONCLASSPROVIDED,
                    false,
                    EventRepresentationChoice.OBJECTARRAY);
                TryAssertionWildcardRecast(
                    env,
                    false,
                    EventRepresentationChoice.JSONCLASSPROVIDED,
                    false,
                    EventRepresentationChoice.MAP);
                TryAssertionWildcardRecast(
                    env,
                    false,
                    EventRepresentationChoice.JSONCLASSPROVIDED,
                    false,
                    EventRepresentationChoice.AVRO);
                TryAssertionWildcardRecast(
                    env,
                    false,
                    EventRepresentationChoice.JSONCLASSPROVIDED,
                    false,
                    EventRepresentationChoice.JSONCLASSPROVIDED);
                TryAssertionWildcardRecast(
                    env,
                    false,
                    EventRepresentationChoice.JSONCLASSPROVIDED,
                    true,
                    null);
            }
        }

        internal class EPLInsertIntoJoinWildcard : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                TryAssertionJoinWildcard(env, true, null);

                foreach (var rep in EventRepresentationChoiceExtensions.Values()) {
                    TryAssertionJoinWildcard(env, false, rep);
                }
            }
        }

        internal class EPLInsertIntoProvidePartialCols : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();

                var fields = new[] {"P0", "P1"};
                var epl =
                    "insert into AStream (P0, P1) select IntPrimitive as somename, TheString from SupportBean(IntPrimitive between 0 and 10);\n" +
                    "insert into AStream (P0) select IntPrimitive as somename from SupportBean(IntPrimitive > 10);\n" +
                    "@Name('s0') select * from AStream;\n";
                env.CompileDeploy(epl, path).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 20));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {20, null});

                env.SendEventBean(new SupportBean("E2", 5));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {5, "E2"});

                env.UndeployAll();
            }
        }

        internal class EPLInsertIntoWithOutputLimitAndSort : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // NOTICE: we are inserting the RSTREAM (removed events)
                var path = new RegressionPath();
                var stmtText = "insert rstream into StockTicks(mySymbol, myPrice) " +
                               "select Symbol, Price from SupportMarketDataBean#time(60) " +
                               "output every 5 seconds " +
                               "order by Symbol asc";
                env.CompileDeploy(stmtText, path);

                stmtText = "@Name('s0') select mySymbol, sum(myPrice) as pricesum from StockTicks#length(100)";
                env.CompileDeploy(stmtText, path).AddListener("s0");

                env.AdvanceTime(0);
                SendEvent(env, "IBM", 50);
                SendEvent(env, "CSC", 10);
                SendEvent(env, "GE", 20);
                env.AdvanceTime(10 * 1000);
                SendEvent(env, "DEF", 100);
                SendEvent(env, "ABC", 11);
                env.AdvanceTime(20 * 1000);
                env.AdvanceTime(30 * 1000);
                env.AdvanceTime(40 * 1000);
                env.AdvanceTime(50 * 1000);
                env.AdvanceTime(55 * 1000);

                Assert.IsFalse(env.Listener("s0").IsInvoked);
                env.AdvanceTime(60 * 1000);

                Assert.IsTrue(env.Listener("s0").IsInvoked);
                Assert.AreEqual(3, env.Listener("s0").NewDataList.Count);
                Assert.AreEqual("CSC", env.Listener("s0").NewDataList[0][0].Get("mySymbol"));
                Assert.AreEqual(10.0, env.Listener("s0").NewDataList[0][0].Get("pricesum"));
                Assert.AreEqual("GE", env.Listener("s0").NewDataList[1][0].Get("mySymbol"));
                Assert.AreEqual(30.0, env.Listener("s0").NewDataList[1][0].Get("pricesum"));
                Assert.AreEqual("IBM", env.Listener("s0").NewDataList[2][0].Get("mySymbol"));
                Assert.AreEqual(80.0, env.Listener("s0").NewDataList[2][0].Get("pricesum"));
                env.Listener("s0").Reset();

                env.AdvanceTime(65 * 1000);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.AdvanceTime(70 * 1000);
                Assert.AreEqual("ABC", env.Listener("s0").NewDataList[0][0].Get("mySymbol"));
                Assert.AreEqual(91.0, env.Listener("s0").NewDataList[0][0].Get("pricesum"));
                Assert.AreEqual("DEF", env.Listener("s0").NewDataList[1][0].Get("mySymbol"));
                Assert.AreEqual(191.0, env.Listener("s0").NewDataList[1][0].Get("pricesum"));

                env.UndeployAll();
            }
        }

        internal class EPLInsertIntoStaggeredWithWildcard : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var statementOne =
                    "@Name('i0') insert into streamA select *" +
                    " from SupportBeanSimple#length(5)";
                var statementTwo =
                    "@Name('i1') insert into streamB select *, MyInt+MyInt as summed, MyString||MyString as concat" +
                    " from streamA#length(5)";
                var statementThree =
                    "@Name('i2') insert into streamC select *" +
                    " from streamB#length(5)";

                // try one module
                var epl = statementOne + ";\n" + statementTwo + ";\n" + statementThree + ";\n";
                env.CompileDeploy(epl);
                AssertEvents(env);
                env.UndeployAll();

                // try multiple modules
                var path = new RegressionPath();
                env.CompileDeploy(statementOne, path);
                env.CompileDeploy(statementTwo, path);
                env.CompileDeploy(statementThree, path);
                AssertEvents(env);
                env.UndeployAll();
            }

            private void AssertEvents(RegressionEnvironment env)
            {
                env.AddListener("i0").AddListener("i1").AddListener("i2");

                SendSimpleEvent(env, "one", 1);
                AssertSimple(env.Listener("i0"), "one", 1, null, 0);
                AssertSimple(env.Listener("i1"), "one", 1, "oneone", 2);
                AssertSimple(env.Listener("i2"), "one", 1, "oneone", 2);

                SendSimpleEvent(env, "two", 2);
                AssertSimple(env.Listener("i0"), "two", 2, null, 0);
                AssertSimple(env.Listener("i1"), "two", 2, "twotwo", 4);
                AssertSimple(env.Listener("i2"), "two", 2, "twotwo", 4);
            }
        }

        internal class EPLInsertIntoInsertFromPattern : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var stmtOneText = "@Name('i0') insert into streamA1 select * from pattern [every SupportBean]";
                env.CompileDeploy(stmtOneText, path).AddListener("i0");

                var stmtTwoText = "@Name('i1') insert into streamA1 select * from pattern [every SupportBean]";
                env.CompileDeploy(stmtTwoText, path).AddListener("i1");

                var eventType = env.Statement("i0").EventType;
                Assert.AreEqual(typeof(IDictionary<string, object>), eventType.UnderlyingType);

                env.UndeployAll();
            }
        }

        internal class EPLInsertIntoInsertIntoPlusPattern : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var stmtOneTxt =
                    "@Name('s1') insert into InZone " +
                    "select 111 as StatementId, Mac, LocationReportId " +
                    "from SupportRFIDEvent " +
                    "where Mac in ('1','2','3') " +
                    "and ZoneID = '10'";
                env.CompileDeploy(stmtOneTxt, path).AddListener("s1");

                var stmtTwoTxt =
                    "@Name('s2') insert into OutOfZone " +
                    "select 111 as StatementId, Mac, LocationReportId " +
                    "from SupportRFIDEvent " +
                    "where Mac in ('1','2','3') " +
                    "and ZoneID != '10'";
                env.CompileDeploy(stmtTwoTxt, path).AddListener("s2");

                var stmtThreeTxt =
                    "@Name('s3') select 111 as EventSpecId, A.LocationReportId as LocationReportId " +
                    " from pattern [every A=InZone -> (timer:interval(1 sec) and not OutOfZone(Mac=A.Mac))]";
                env.CompileDeploy(stmtThreeTxt, path).AddListener("s3");

                // try the alert case with 1 event for the mac in question
                env.AdvanceTime(0);
                env.SendEventBean(new SupportRFIDEvent("LR1", "1", "10"));
                Assert.IsFalse(env.Listener("s3").IsInvoked);
                env.AdvanceTime(1000);

                var theEvent = env.Listener("s3").AssertOneGetNewAndReset();
                Assert.AreEqual("LR1", theEvent.Get("LocationReportId"));

                env.Listener("s1").Reset();
                env.Listener("s2").Reset();

                // try the alert case with 2 events for zone 10 within 1 second for the mac in question
                env.SendEventBean(new SupportRFIDEvent("LR2", "2", "10"));
                Assert.IsFalse(env.Listener("s3").IsInvoked);
                env.AdvanceTime(1500);
                env.SendEventBean(new SupportRFIDEvent("LR3", "2", "10"));
                Assert.IsFalse(env.Listener("s3").IsInvoked);
                env.AdvanceTime(2000);

                theEvent = env.Listener("s3").AssertOneGetNewAndReset();
                Assert.AreEqual("LR2", theEvent.Get("LocationReportId"));

                env.UndeployAll();
            }
        }

        internal class EPLInsertIntoNullType : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var stmtOneTxt = "@Name('s1') insert into InZoneTwo select null as dummy from SupportBean";
                env.CompileDeploy(stmtOneTxt, path);
                AssertNullTypeForDummyField(env.Statement("s1").EventType);

                var stmtTwoTxt = "@Name('s2') select dummy from InZoneTwo";
                env.CompileDeploy(stmtTwoTxt, path).AddListener("s2");
                AssertNullTypeForDummyField(env.Statement("s2").EventType);

                env.SendEventBean(new SupportBean());
                Assert.IsNull(env.Listener("s2").AssertOneGetNewAndReset().Get("dummy"));

                env.UndeployAll();
            }

            private void AssertNullTypeForDummyField(EventType eventType)
            {
                String fieldName = "dummy";
                Assert.IsTrue(eventType.IsProperty(fieldName));
                Assert.IsNull(eventType.GetPropertyType(fieldName));
                EventPropertyDescriptor desc = eventType.GetPropertyDescriptor(fieldName);
                SupportEventPropUtil.AssertPropEquals(new SupportEventPropDesc(fieldName, TypeHelper.NullType), desc);
            }
        }

        public class EPLInsertIntoChain : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var text = "insert into S0 select irstream Symbol, 0 as val from SupportMarketDataBean";
                env.CompileDeploy(text, path);

                env.Milestone(0);

                text = "insert into S1 select irstream Symbol, 1 as val from S0";
                env.CompileDeploy(text, path);

                env.Milestone(1);

                text = "insert into S2 select irstream Symbol, 2 as val from S1";
                env.CompileDeploy(text, path);

                env.Milestone(2);

                text = "@Name('s0') insert into S3 select irstream Symbol, 3 as val from S2";
                env.CompileDeploy(text, path).AddListener("s0");

                env.Milestone(3);

                env.SendEventBean(MakeMarketDataEvent("E1"));
                env.Listener("s0")
                    .AssertNewOldData(new[] {new object[] {"Symbol", "E1"}, new object[] {"val", 3}}, null);

                env.UndeployAll();
            }
        }

        public class MyP0P1EventSource
        {
            public MyP0P1EventSource(
                string p0,
                int p1)
            {
                P0 = p0;
                P1 = p1;
            }

            [PropertyName("P0")]
            public string P0 { get; }

            [PropertyName("P1")]
            public int P1 { get; }
        }

        public class MyP0P1EventTarget
        {
            public MyP0P1EventTarget()
            {
            }

            public MyP0P1EventTarget(
                string p0,
                int p1,
                object c0)
            {
                P0 = p0;
                P1 = p1;
                C0 = c0;
            }

            public string P0 { get; set; }

            public int P1 { get; set; }

            public object C0 { get; set; }
        }

        [Serializable]
        public class MyLocalJsonProvided
        {
            public string TheString;
            public int? IntPrimitive;
        }

        [Serializable]
        public class MyLocalJsonProvidedS0
        {
            public string TheString;

            public override string ToString()
            {
                return "{\"TheString\":\"" + TheString + "\"}";
            }
        }

        [Serializable]
        public class MyLocalJsonProvidedS1
        {
            public string Id;

            public override string ToString()
            {
                return "{\"Id\":\"" + Id + "\"}";
            }
        }

        [Serializable]
        public class MyLocalJsonProvidedJoin
        {
            public MyLocalJsonProvidedS0 S0;
            public MyLocalJsonProvidedS1 S1;
        }

        [Serializable]
        public class MyLocalJsonProvidedSourceSchema
        {
            public string P0;
            public int P1;
        }

        [Serializable]
        public class MyLocalJsonProvidedTargetContainedSchema
        {
            public int C0;
        }

        [Serializable]
        public class MyLocalJsonProvidedTargetSchema
        {
            public string P0;
            public int P1;
            public MyLocalJsonProvidedTargetContainedSchema C0;
        }
    }
} // end of namespace