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

using Avro.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.hook.expr;
using com.espertech.esper.common.client.json.util;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NEsper.Avro.Extensions;
using NEsper.Avro.Util.Support;

using Newtonsoft.Json.Linq;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.epl.contained
{
    public class EPLContainedEventSplitExpr
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithScriptContextValue(execs);
            WithSplitExprReturnsEventBean(execs);
            WithSingleRowSplitAndType(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithSingleRowSplitAndType(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLContainedSingleRowSplitAndType());
            return execs;
        }

        public static IList<RegressionExecution> WithSplitExprReturnsEventBean(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLContainedSplitExprReturnsEventBean());
            return execs;
        }

        public static IList<RegressionExecution> WithScriptContextValue(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLContainedScriptContextValue());
            return execs;
        }

        private class EPLContainedScriptContextValue : RegressionExecution
        {
            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED);
            }

            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var script = "@name('mystmt') @public create expression Object js:myGetScriptContext() [\n" +
                             "function myGetScriptContext() {" +
                             "  return epl;\n" +
                             "}" + 
                             "return myGetScriptContext();" +
                             "]";
                env.CompileDeploy(script, path);

                env.CompileDeploy("@name('s0') select myGetScriptContext() as c0 from SupportBean", path)
                    .AddListener("s0");
                env.SendEventBean(new SupportBean());
                env.AssertListener(
                    "s0",
                    listener => {
                        var context = (EPLScriptContext) listener.AssertOneGetNewAndReset().Get("c0");
                        Assert.IsNotNull(context.EventBeanService);
                    });

                env.UndeployAll();
            }
        }

        private class EPLContainedSplitExprReturnsEventBean : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var collections = typeof(Collections).FullName;
                var path = new RegressionPath();
                var script =
                    "@public create expression EventBean[] js:mySplitScriptReturnEventBeanArray(value) [\n" +
                    "function mySplitScriptReturnEventBeanArray(value) {" +
                    "  var split = value.split(',');\n" +
                    "  var etype = host.resolveType('com.espertech.esper.common.client.EventBean');\n" +
                    "  var events = host.newArr(etype, split.length);  " +
                    "  for (var i = 0; i < split.length; i++) {\n" +
                    "    var pvalue = split[i].substring(1);\n" +
                    "    if (split[i].startsWith(\"A\")) {\n" +
                    $"      events[i] = epl.EventBeanService.AdapterForMap(Collections.SingletonDataMap(\"P0\", pvalue), \"AEvent\");\n" +
                    "    }\n" +
                    "    else if (split[i].startsWith(\"B\")) {\n" +
                    $"      events[i] = epl.EventBeanService.AdapterForMap(Collections.SingletonDataMap(\"P1\", pvalue), \"BEvent\");\n" +
                    "    }\n" +
                    "    else {\n" +
                    "      xhost.throwException(\"UnsupportedOperationException\", \"Unrecognized type\");\n" +
                    "    }\n" +
                    "  }\n" +
                    "  return events;\n" +
                    "};" +
                    "return mySplitScriptReturnEventBeanArray(value);" +
                    "]";
                env.CompileDeploy(script, path);

                var epl = 
                    "@public create schema BaseEvent();\n" +
                    "@public create schema AEvent(P0 string) inherits BaseEvent;\n" +
                    "@public create schema BEvent(P1 string) inherits BaseEvent;\n" +
                    "@public @buseventtype create schema SplitEvent(value string);\n";
                env.CompileDeploy(epl, path);

                TryAssertionSplitExprReturnsEventBean(env, path, "mySplitUDFReturnEventBeanArray");
                TryAssertionSplitExprReturnsEventBean(env, path, "mySplitScriptReturnEventBeanArray");

                env.UndeployAll();
            }

            private void TryAssertionSplitExprReturnsEventBean(
                RegressionEnvironment env,
                RegressionPath path,
                string functionOrScript)
            {
                var epl = "@name('s0') select * from SplitEvent[" + functionOrScript + "(value) @type(BaseEvent)]";
                env.CompileDeploy(epl, path).AddListener("s0");

                env.SendEventMap(Collections.SingletonDataMap("value", "AE1,BE2,AE3"), "SplitEvent");
                env.AssertListener(
                    "s0",
                    listener => {
                        var events = listener.GetAndResetLastNewData();
                        AssertSplitEx(events[0], "AEvent", "P0", "E1");
                        AssertSplitEx(events[1], "BEvent", "P1", "E2");
                        AssertSplitEx(events[2], "AEvent", "P0", "E3");
                    });

                env.UndeployModuleContaining("s0");
            }
        }

        private class EPLContainedSingleRowSplitAndType : RegressionExecution
        {
            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED);
            }

            public void Run(RegressionEnvironment env)
            {
                foreach (var rep in EventRepresentationChoiceExtensions.Values()) {
                    TryAssertionSingleRowSplitAndType(env, rep);
                }
            }
        }

        private static void TryAssertionSingleRowSplitAndType(
            RegressionEnvironment env,
            EventRepresentationChoice eventRepresentationEnum)
        {
            var path = new RegressionPath();
            var types = eventRepresentationEnum.GetAnnotationTextWJsonProvided(typeof(MyLocalJsonSentence)) +
                        " @buseventtype @public create schema MySentenceEvent(sentence String);\n" +
                        eventRepresentationEnum.GetAnnotationTextWJsonProvided(typeof(MyLocalJsonWord)) +
                        " @public create schema WordEvent(word String);\n" +
                        eventRepresentationEnum.GetAnnotationTextWJsonProvided(typeof(MyLocalJsonCharacter)) +
                        " @public create schema CharacterEvent(character String);\n";
            env.CompileDeploy(types, path);

            string stmtText;
            var fields = "word".SplitCsv();

            // test single-row method
            stmtText = "@name('s0') select * from MySentenceEvent[splitSentence" +
                       "_" +
                       eventRepresentationEnum.GetName() +
                       "(sentence)@type(WordEvent)]";
            env.CompileDeploy(stmtText, path).AddListener("s0");
            env.AssertStatement(
                "s0",
                statement => {
                    Assert.AreEqual("WordEvent", statement.EventType.Name);
                    Assert.IsTrue(eventRepresentationEnum.MatchesClass(statement.EventType.UnderlyingType));
                });

            SendMySentenceEvent(env, eventRepresentationEnum, "I am testing this code");
            env.AssertPropsPerRowLastNew(
                "s0",
                fields,
                new object[][] {
                    new object[] { "I" }, new object[] { "am" }, new object[] { "testing" }, new object[] { "this" },
                    new object[] { "code" }
                });

            SendMySentenceEvent(env, eventRepresentationEnum, "the second event");
            env.AssertPropsPerRowLastNew(
                "s0",
                fields,
                new object[][] { new object[] { "the" }, new object[] { "second" }, new object[] { "event" } });

            env.UndeployModuleContaining("s0");

            // test SODA
            env.EplToModelCompileDeploy(stmtText, path).AddListener("s0");
            SendMySentenceEvent(env, eventRepresentationEnum, "the third event");
            env.AssertPropsPerRowLastNew(
                "s0",
                fields,
                new object[][] { new object[] { "the" }, new object[] { "third" }, new object[] { "event" } });
            env.UndeployModuleContaining("s0");

            // test script
            if (eventRepresentationEnum.IsMapEvent()) {
                stmtText = "@name('s0') expression System.Collections.IList js:SplitSentenceJS(sentence) [" +
                           "  var listType = host.resolveType('System.Collections.Generic.List<object>');" +
                           "  var words = host.newObj(listType);" +
                           "  words.Add(Collections.SingletonDataMap('word', 'wordOne'));" +
                           "  words.Add(Collections.SingletonDataMap('word', 'wordTwo'));" +
                           "  return words;" +
                           "]" +
                           "select * from MySentenceEvent[SplitSentenceJS(sentence)@type(WordEvent)]";

                env.CompileDeploy(stmtText, path).AddListener("s0");
                env.AssertStatement("s0", statement => Assert.AreEqual("WordEvent", statement.EventType.Name));

                env.SendEventMap(EmptyDictionary<string, object>.Instance, "MySentenceEvent");
                env.AssertPropsPerRowLastNewAnyOrder(
                    "s0",
                    fields,
                    new object[][] { new object[] { "wordOne" }, new object[] { "wordTwo" } });

                env.UndeployModuleContaining("s0");
            }

            // test multiple splitters
            stmtText = "@name('s0') select * from MySentenceEvent[splitSentence_" +
                       eventRepresentationEnum.GetName() +
                       "(sentence)@type(WordEvent)][splitWord_" +
                       eventRepresentationEnum.GetName() +
                       "(word)@type(CharacterEvent)]";
            env.CompileDeploy(stmtText, path).AddListener("s0");
            env.AssertStatement("s0", statement => Assert.AreEqual("CharacterEvent", statement.EventType.Name));

            SendMySentenceEvent(env, eventRepresentationEnum, "I am");
            env.AssertPropsPerRowLastNewAnyOrder(
                "s0",
                "character".SplitCsv(),
                new object[][] { new object[] { "I" }, new object[] { "a" }, new object[] { "m" } });

            env.UndeployModuleContaining("s0");

            // test wildcard parameter
            stmtText = "@name('s0') select * from MySentenceEvent[splitSentenceBean_" +
                       eventRepresentationEnum.GetName() +
                       "(*)@type(WordEvent)]";
            env.CompileDeploy(stmtText, path).AddListener("s0");
            env.AssertStatement("s0", statement => Assert.AreEqual("WordEvent", statement.EventType.Name));

            SendMySentenceEvent(env, eventRepresentationEnum, "another test sentence");
            env.AssertPropsPerRowLastNewAnyOrder(
                "s0",
                fields,
                new object[][] { new object[] { "another" }, new object[] { "test" }, new object[] { "sentence" } });

            env.UndeployModuleContaining("s0");

            // test property returning untyped collection
            if (eventRepresentationEnum.IsObjectArrayEvent()) {
                stmtText = eventRepresentationEnum.GetAnnotationText() +
                           " @Name('s0') select * from SupportObjectArrayEvent[SomeObjectArray@type(WordEvent)]";
                env.CompileDeploy(stmtText, path).AddListener("s0");
                env.AssertStatement("s0", statement => Assert.AreEqual("WordEvent", statement.EventType.Name));

                var rows = new object[][]
                    { new object[] { "this" }, new object[] { "is" }, new object[] { "collection" } };
                env.SendEventBean(new SupportObjectArrayEvent(rows));
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { "this" }, new object[] { "is" }, new object[] { "collection" } });
                env.UndeployAll();
            }
            else if (eventRepresentationEnum.IsMapEvent()) {
                stmtText = eventRepresentationEnum.GetAnnotationText() +
                           " @Name('s0') select * from SupportCollectionEvent[SomeCollection@type(WordEvent)]";
                env.CompileDeploy(stmtText, path).AddListener("s0");
                env.AssertStatement("s0", statement => Assert.AreEqual("WordEvent", statement.EventType.Name));

                var coll = new List<IDictionary<string, object>>();
                coll.Add(Collections.SingletonDataMap("word", "this"));
                coll.Add(Collections.SingletonDataMap("word", "is"));
                coll.Add(Collections.SingletonDataMap("word", "collection"));

                env.SendEventBean(new SupportCollectionEvent(coll.Unwrap<object>()));
                env.AssertPropsPerRowLastNewAnyOrder(
                    "s0",
                    fields,
                    new object[][] { new object[] { "this" }, new object[] { "is" }, new object[] { "collection" } });
                env.UndeployAll();
            }
            else if (eventRepresentationEnum.IsAvroEvent()) {
                stmtText = "@name('s0') " +
                           eventRepresentationEnum.GetAnnotationTextWJsonProvided(typeof(MyLocalJsonWord)) +
                           " select * from SupportAvroArrayEvent[SomeAvroArray@type(WordEvent)]";
                env.CompileDeploy(stmtText, path).AddListener("s0");
                env.AssertStatement("s0", statement => Assert.AreEqual("WordEvent", statement.EventType.Name));

                var rows = new GenericRecord[3];
                var words = "this,is,avro".SplitCsv();
                var schema = env.RuntimeAvroSchemaByDeployment("s0", "WordEvent").AsRecordSchema();
                for (var i = 0; i < words.Length; i++) {
                    rows[i] = new GenericRecord(schema);
                    rows[i].Put("word", words[i]);
                }

                env.SendEventBean(new SupportAvroArrayEvent(rows));
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { "this" }, new object[] { "is" }, new object[] { "avro" } });
                env.UndeployAll();
            }
            else if (eventRepresentationEnum.IsJsonEvent() || eventRepresentationEnum.IsJsonProvidedClassEvent()) {
                stmtText = "@name('s0') " +
                           eventRepresentationEnum.GetAnnotationTextWJsonProvided(typeof(MyLocalJsonWord)) +
                           " select * from SupportJsonArrayEvent[SomeJsonArray@type(WordEvent)]";
                env.CompileDeploy(stmtText, path).AddListener("s0");
                env.AssertStatement("s0", statement => Assert.AreEqual("WordEvent", statement.EventType.Name));

                var rows = new string[3];
                var words = "this,is,avro".SplitCsv();
                for (var i = 0; i < words.Length; i++) {
                    rows[i] = "{ \"word\": \"" + words[i] + "\"}";
                }

                env.SendEventBean(new SupportJsonArrayEvent(rows));
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { "this" }, new object[] { "is" }, new object[] { "avro" } });
                env.UndeployAll();
            }
            else {
                throw new ArgumentException("Unrecognized enum " + eventRepresentationEnum);
            }

            // invalid: event type not found
            env.TryInvalidCompile(
                path,
                "select * from MySentenceEvent[splitSentence_" +
                eventRepresentationEnum.GetName() +
                "(sentence)@type(XYZ)]",
                "Event type by name 'XYZ' could not be found");

            // invalid lib-function annotation
            env.TryInvalidCompile(
                path,
                "select * from MySentenceEvent[splitSentence_" +
                eventRepresentationEnum.GetName() +
                "(sentence)@dummy(WordEvent)]",
                "Invalid annotation for property selection, expected 'type' but found 'dummy' in text '@dummy(WordEvent)'");

            // invalid type assignment to event type
            if (eventRepresentationEnum.IsObjectArrayEvent()) {
                env.TryInvalidCompile(
                    path,
                    "select * from MySentenceEvent[invalidSentence(sentence)@type(WordEvent)]",
                    "Event type 'WordEvent' underlying type System.Object[] cannot be assigned a value of type");
            }
            else if (eventRepresentationEnum.IsMapEvent()) {
                env.TryInvalidCompile(
                    path,
                    "select * from MySentenceEvent[invalidSentence(sentence)@type(WordEvent)]",
                    "Event type 'WordEvent' underlying type System.Collections.Generic.IDictionary<System.String, System.Object> cannot be assigned a value of type com.espertech.esper.common.internal.support.SupportBean[] [");
            }
            else if (eventRepresentationEnum.IsAvroEvent()) {
                env.TryInvalidCompile(
                    path,
                    "select * from MySentenceEvent[invalidSentence(sentence)@type(WordEvent)]",
                    "Event type 'WordEvent' underlying type " +
                    TypeHelper.AVRO_GENERIC_RECORD_CLASSNAME +
                    " cannot be assigned a value of type");
            }
            else if (eventRepresentationEnum.IsJsonEvent() || eventRepresentationEnum.IsJsonProvidedClassEvent()) {
                env.TryInvalidCompile(
                    path,
                    "select * from MySentenceEvent[invalidSentence(sentence)@type(WordEvent)]",
                    "Event type 'WordEvent' requires string-type array and cannot be assigned from value of type " +
                    typeof(SupportBean[]).CleanName());
            }
            else {
                Assert.Fail();
            }

            // invalid subquery
            env.TryInvalidCompile(
                path,
                "select * from MySentenceEvent[splitSentence((select * from SupportBean#keepall))@type(WordEvent)]",
                "Invalid contained-event expression 'splitSentence(subselect_0)': Aggregation, sub-select, previous or prior functions are not supported in this context [select * from MySentenceEvent[splitSentence((select * from SupportBean#keepall))@type(WordEvent)]]");

            env.UndeployAll();
        }

        private static void SendMySentenceEvent(
            RegressionEnvironment env,
            EventRepresentationChoice eventRepresentationEnum,
            string sentence)
        {
            if (eventRepresentationEnum.IsObjectArrayEvent()) {
                env.SendEventObjectArray(new object[] { sentence }, "MySentenceEvent");
            }
            else if (eventRepresentationEnum.IsMapEvent()) {
                env.SendEventMap(Collections.SingletonDataMap("sentence", sentence), "MySentenceEvent");
            }
            else if (eventRepresentationEnum.IsAvroEvent()) {
                var schema = SchemaBuilder.Record("sentence", TypeBuilder.RequiredString("sentence"));
                var record = new GenericRecord(schema);
                record.Put("sentence", sentence);
                env.SendEventAvro(record, "MySentenceEvent");
            }
            else if (eventRepresentationEnum.IsJsonEvent() || eventRepresentationEnum.IsJsonProvidedClassEvent()) {
                var @object = new JObject();
                @object.Add("sentence", sentence);
                env.SendEventJson(@object.ToString(), "MySentenceEvent");
            }
            else {
                throw new IllegalStateException("Unrecognized enum " + eventRepresentationEnum);
            }
        }

        private static void AssertSplitEx(
            EventBean @event,
            string typeName,
            string propertyName,
            string propertyValue)
        {
            Assert.AreEqual(typeName, @event.EventType.Name);
            Assert.AreEqual(propertyValue, @event.Get(propertyName));
        }

        public static EventBean[] MySplitUDFReturnEventBeanArray(
            string value,
            EPLMethodInvocationContext context)
        {
            var split = value.SplitCsv();
            var events = new EventBean[split.Length];
            for (var i = 0; i < split.Length; i++) {
                var pvalue = split[i].Substring(1);
                if (split[i].StartsWith("A")) {
                    events[i] = context.EventBeanService.AdapterForMap(
                        Collections.SingletonDataMap("P0", pvalue),
                        "AEvent");
                }
                else if (split[i].StartsWith("B")) {
                    events[i] = context.EventBeanService.AdapterForMap(
                        Collections.SingletonDataMap("P1", pvalue),
                        "BEvent");
                }
                else {
                    throw new UnsupportedOperationException("Unrecognized type");
                }
            }

            return events;
        }

        public static IDictionary<string, object>[] SplitSentenceMethodReturnMap(string sentence)
        {
            var words = sentence.Split(' ');
            var events = new IDictionary<string, object>[words.Length];
            for (var i = 0; i < words.Length; i++) {
                events[i] = Collections.SingletonDataMap("word", words[i]);
            }

            return events;
        }

        public static object[][] SplitSentenceMethodReturnObjectArray(string sentence)
        {
            var words = sentence.Split(' ');
            var events = new object[words.Length][];
            for (var i = 0; i < words.Length; i++) {
                events[i] = new object[] { words[i] };
            }

            return events;
        }

        public static IDictionary<string, object>[] SplitSentenceBeanMethodReturnMap(
            IDictionary<string, object> sentenceEvent)
        {
            return SplitSentenceMethodReturnMap((string)sentenceEvent.Get("sentence"));
        }

        public static object[][] SplitSentenceBeanMethodReturnObjectArray(object[] sentenceEvent)
        {
            return SplitSentenceMethodReturnObjectArray((string)sentenceEvent[0]);
        }

        public static object[][] SplitWordMethodReturnObjectArray(string word)
        {
            var count = word.Length;
            var events = new object[count][];
            for (var i = 0; i < word.Length; i++) {
                events[i] = new object[] { word[i].ToString() };
            }

            return events;
        }

        public static IDictionary<string, object>[] SplitWordMethodReturnMap(string word)
        {
            IList<IDictionary<string, object>> maps = new List<IDictionary<string, object>>();
            for (var i = 0; i < word.Length; i++) {
                maps.Add(Collections.SingletonDataMap("character", word[i].ToString()));
            }

            return maps.ToArray();
        }

        public static SupportBean[] InvalidSentenceMethod(string sentence)
        {
            return null;
        }

        public static GenericRecord[] SplitWordMethodReturnAvro(string word)
        {
            var schema = SchemaBuilder.Record("chars", TypeBuilder.RequiredString("character"));
            var records = new GenericRecord[word.Length];
            for (var i = 0; i < word.Length; i++) {
                records[i] = new GenericRecord(schema);
                records[i].Put("character", Convert.ToString(word[i]));
            }

            return records;
        }

        public static GenericRecord[] SplitSentenceMethodReturnAvro(string sentence)
        {
            var wordSchema = SchemaBuilder.Record("word", TypeBuilder.RequiredString("word"));
            var words = sentence.Split(' ');
            var events = new GenericRecord[words.Length];
            for (var i = 0; i < words.Length; i++) {
                events[i] = new GenericRecord(wordSchema.AsRecordSchema());
                events[i].Put("word", words[i]);
            }

            return events;
        }

        public static GenericRecord[] SplitSentenceBeanMethodReturnAvro(GenericRecord sentenceEvent)
        {
            return SplitSentenceMethodReturnAvro((string)sentenceEvent.Get("sentence"));
        }

        public static string[] SplitWordMethodReturnJson(string word)
        {
            IList<string> strings = new List<string>();
            for (var i = 0; i < word.Length; i++) {
                var c = word[i].ToString();
                strings.Add("{ \"character\": \"" + c + "\"}");
            }

            return strings.ToArray();
        }

        public static string[] SplitSentenceMethodReturnJson(string sentence)
        {
            var words = sentence.Split(" ");
            var events = new string[words.Length];
            for (var i = 0; i < words.Length; i++) {
                events[i] = "{ \"word\": \"" + words[i] + "\"}";
            }

            return events;
        }

        public static string[] SplitSentenceBeanMethodReturnJson(object sentenceEvent)
        {
            string sentence;
            if (sentenceEvent is JsonEventObject jsonEventObject) {
                sentence = jsonEventObject.Get("sentence").ToString();
            }
            else {
                sentence = ((MyLocalJsonSentence)sentenceEvent).sentence;
            }

            return SplitSentenceMethodReturnJson(sentence);
        }

        public class AvroArrayEvent
        {
            public AvroArrayEvent(GenericRecord[] someAvroArray)
            {
                SomeAvroArray = someAvroArray;
            }

            public GenericRecord[] SomeAvroArray { get; }
        }

        public class MyLocalJsonSentence
        {
            public string sentence;
        }

        public class MyLocalJsonWord
        {
            public string word;
        }

        public class MyLocalJsonCharacter
        {
            public string character;
        }
    }
} // end of namespace