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
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NEsper.Avro.Core;
using NEsper.Avro.Extensions;
using NEsper.Avro.Util.Support;

using Newtonsoft.Json.Linq;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

namespace com.espertech.esper.regressionlib.suite.epl.contained
{
    public class EPLContainedEventSplitExpr
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            execs.Add(new EPLContainedScriptContextValue());
            execs.Add(new EPLContainedSplitExprReturnsEventBean());
            execs.Add(new EPLContainedSingleRowSplitAndType());
            return execs;
        }

        private static void TryAssertionSingleRowSplitAndType(
            RegressionEnvironment env,
            EventRepresentationChoice eventRepresentationEnum)
        {
            var path = new RegressionPath();
            var types = 
                eventRepresentationEnum.GetAnnotationTextWJsonProvided<MyLocalJsonSentence>() + " create schema MySentenceEvent(sentence String);\n" +
                eventRepresentationEnum.GetAnnotationTextWJsonProvided<MyLocalJsonWord>() + " create schema WordEvent(word String);\n" +
                eventRepresentationEnum.GetAnnotationTextWJsonProvided<MyLocalJsonCharacter>() + " create schema CharacterEvent(character String);\n";
            env.CompileDeployWBusPublicType(types, path);

            string stmtText;
            var fields = new[] {"word"};

            // test single-row method
            stmtText = "@name('s0') select * from MySentenceEvent[splitSentence" +
                       "_" +
                       eventRepresentationEnum.GetName() +
                       "(sentence)@type(WordEvent)]";
            env.CompileDeploy(stmtText, path).AddListener("s0");
            Assert.AreEqual("WordEvent", env.Statement("s0").EventType.Name);
            Assert.IsTrue(eventRepresentationEnum.MatchesClass(env.Statement("s0").EventType.UnderlyingType));

            SendMySentenceEvent(env, eventRepresentationEnum, "I am testing this code");
            EPAssertionUtil.AssertPropsPerRow(
                env.Listener("s0").GetAndResetLastNewData(),
                fields,
                new[] {
                    new object[] {"I"},
                    new object[] {"am"},
                    new object[] {"testing"},
                    new object[] {"this"},
                    new object[] {"code"}
                });

            SendMySentenceEvent(env, eventRepresentationEnum, "the second event");
            EPAssertionUtil.AssertPropsPerRow(
                env.Listener("s0").GetAndResetLastNewData(),
                fields,
                new[] {
                    new object[] {"the"},
                    new object[] {"second"},
                    new object[] {"event"}
                });

            env.UndeployModuleContaining("s0");

            // test SODA
            env.EplToModelCompileDeploy(stmtText, path).AddListener("s0");
            SendMySentenceEvent(env, eventRepresentationEnum, "the third event");
            EPAssertionUtil.AssertPropsPerRow(
                env.Listener("s0").GetAndResetLastNewData(),
                fields,
                new[] {
                    new object[] {"the"},
                    new object[] {"third"},
                    new object[] {"event"}
                });
            env.UndeployModuleContaining("s0");

            // test script
            if (eventRepresentationEnum.IsMapEvent()) {
                stmtText = "@name('s0') expression System.Collection js:splitSentenceJS(sentence) [" +
                           "  debug.Debug('test');" +
                           "  var listType = host.type('System.Collections.ArrayList');" +
                           "  var words = host.newObj(listType);" +
                           "  debug.Debug(words);" +
                           "  words.Add(Collections.SingletonDataMap('word', 'wordOne'));" +
                           "  words.Add(Collections.SingletonDataMap('word', 'wordTwo'));" +
                           "  return words;" +
                           "]" +
                           "select * from MySentenceEvent[splitSentenceJS(sentence)@type(WordEvent)]";

                env.CompileDeploy(stmtText, path).AddListener("s0");
                Assert.AreEqual("WordEvent", env.Statement("s0").EventType.Name);

                env.SendEventMap(Collections.EmptyDataMap, "MySentenceEvent");
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {
                        new object[] {"wordOne"},
                        new object[] {"wordTwo"}
                    });

                env.UndeployModuleContaining("s0");
            }

            // test multiple splitters
            stmtText = "@name('s0') select * from " +
                       "MySentenceEvent[splitSentence_" +
                       eventRepresentationEnum.GetName() +
                       "(sentence)@type(WordEvent)][splitWord_" +
                       eventRepresentationEnum.GetName() +
                       "(word)@type(CharacterEvent)]";
            env.CompileDeploy(stmtText, path).AddListener("s0");
            Assert.AreEqual("CharacterEvent", env.Statement("s0").EventType.Name);

            SendMySentenceEvent(env, eventRepresentationEnum, "I am");
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                env.Listener("s0").GetAndResetLastNewData(),
                new[] {"character"},
                new[] {
                    new object[] {"I"},
                    new object[] {"a"},
                    new object[] {"m"}
                });

            env.UndeployModuleContaining("s0");

            // test wildcard parameter
            stmtText = "@name('s0') select * from MySentenceEvent[splitSentenceBean_" +
                       eventRepresentationEnum.GetName() +
                       "(*)@type(WordEvent)]";
            env.CompileDeploy(stmtText, path).AddListener("s0");
            Assert.AreEqual("WordEvent", env.Statement("s0").EventType.Name);

            SendMySentenceEvent(env, eventRepresentationEnum, "another test sentence");
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                env.Listener("s0").GetAndResetLastNewData(),
                fields,
                new[] {
                    new object[] {"another"},
                    new object[] {"test"},
                    new object[] {"sentence"}
                });

            env.UndeployModuleContaining("s0");

            // test property returning untyped collection
            if (eventRepresentationEnum.IsObjectArrayEvent()) {
                stmtText = eventRepresentationEnum.GetAnnotationText() +
                           " @Name('s0') select * from SupportObjectArrayEvent[someObjectArray@type(WordEvent)]";
                env.CompileDeploy(stmtText, path).AddListener("s0");
                Assert.AreEqual("WordEvent", env.Statement("s0").EventType.Name);

                object[][] rows = {
                    new object[] {"this"},
                    new object[] {"is"},
                    new object[] {"collection"}
                };
                env.SendEventBean(new SupportObjectArrayEvent(rows));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {
                        new object[] {"this"},
                        new object[] {"is"},
                        new object[] {"collection"}
                    });
                env.UndeployAll();
            }
            else if (eventRepresentationEnum.IsMapEvent()) {
                stmtText = eventRepresentationEnum.GetAnnotationText() +
                           " @Name('s0') select * from SupportCollectionEvent[someCollection@type(WordEvent)]";
                env.CompileDeploy(stmtText, path).AddListener("s0");
                Assert.AreEqual("WordEvent", env.Statement("s0").EventType.Name);

                var coll = new List<IDictionary<string, object>>();
                coll.Add(Collections.SingletonDataMap("word", "this"));
                coll.Add(Collections.SingletonDataMap("word", "is"));
                coll.Add(Collections.SingletonDataMap("word", "collection"));

                env.SendEventBean(new SupportCollectionEvent(coll.Unwrap<object>()));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {
                        new object[] {"this"},
                        new object[] {"is"},
                        new object[] {"collection"}
                    });
                env.UndeployAll();
            }
            else if (eventRepresentationEnum.IsAvroEvent()) {
                stmtText = "@name('s0') " +
                           eventRepresentationEnum.GetAnnotationTextWJsonProvided<MyLocalJsonWord>() +
                           " select * from SupportAvroArrayEvent[someAvroArray@type(WordEvent)]";
                env.CompileDeploy(stmtText, path).AddListener("s0");
                Assert.AreEqual("WordEvent", env.Statement("s0").EventType.Name);

                var rows = new GenericRecord[3];
                var words = new[] {"this", "is", "avro"};
                for (var i = 0; i < words.Length; i++) {
                    rows[i] = new GenericRecord(
                        ((AvroEventType) env.Statement("s0").EventType).SchemaAvro.AsRecordSchema());
                    rows[i].Put("word", words[i]);
                }

                env.SendEventBean(new SupportAvroArrayEvent(rows));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {
                        new object[] {"this"},
                        new object[] {"is"},
                        new object[] {"avro"}
                    });
                env.UndeployAll();
            } else if (eventRepresentationEnum.IsJsonEvent() || eventRepresentationEnum.IsJsonProvidedClassEvent()) {
                stmtText = "@name('s0') " +
                           eventRepresentationEnum.GetAnnotationTextWJsonProvided<MyLocalJsonWord>() +
                           " select * from SupportJsonArrayEvent[someJsonArray@type(WordEvent)]";
                env.CompileDeploy(stmtText, path).AddListener("s0");
                Assert.AreEqual("WordEvent", env.Statement("s0").EventType.Name);

                var rows = new String[3];
                var words = "this,is,avro".SplitCsv();
                for (var i = 0; i < words.Length; i++) {
                    rows[i] = "{ \"word\": \"" + words[i] + "\"}";
                }

                env.SendEventBean(new SupportJsonArrayEvent(rows));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new Object[][] {
                        new[] {"this"}, new[] {"is"},
                        new[] {"avro"}
                    });
                env.UndeployAll();
            }
            else {
                throw new ArgumentException("Unrecognized enum " + eventRepresentationEnum);
            }

            // invalid: event type not found
            TryInvalidCompile(
                env,
                path,
                "select * from MySentenceEvent[splitSentence_" +
                eventRepresentationEnum.GetName() +
                "(sentence)@type(XYZ)]",
                "Event type by name 'XYZ' could not be found");

            // invalid lib-function annotation
            TryInvalidCompile(
                env,
                path,
                "select * from MySentenceEvent[splitSentence_" +
                eventRepresentationEnum.GetName() +
                "(sentence)@dummy(WordEvent)]",
                "Invalid annotation for property selection, expected 'type' but found 'dummy' in text '@dummy(WordEvent)'");

            // invalid type assignment to event type
            if (eventRepresentationEnum.IsObjectArrayEvent()) {
                TryInvalidCompile(
                    env,
                    path,
                    "select * from MySentenceEvent[invalidSentence(sentence)@type(WordEvent)]",
                    "Event type 'WordEvent' underlying type [LSystem.Object; cannot be assigned a value of type");
            }
            else if (eventRepresentationEnum.IsMapEvent()) {
                TryInvalidCompile(
                    env,
                    path,
                    "select * from MySentenceEvent[invalidSentence(sentence)@type(WordEvent)]",
                    "Event type 'WordEvent' underlying type IDictionary cannot be assigned a value of type");
            }
            else if (eventRepresentationEnum.IsAvroEvent()) {
                TryInvalidCompile(
                    env,
                    path,
                    "select * from MySentenceEvent[invalidSentence(sentence)@type(WordEvent)]",
                    "Event type 'WordEvent' underlying type " +
                    TypeHelper.AVRO_GENERIC_RECORD_CLASSNAME +
                    " cannot be assigned a value of type");
            }
            else if (eventRepresentationEnum.IsJsonEvent() || eventRepresentationEnum.IsJsonProvidedClassEvent()) {
                TryInvalidCompile(
                    env,
                    path,
                    "select * from MySentenceEvent[invalidSentence(sentence)@type(WordEvent)]",
                    "Event type 'WordEvent' requires string-type array and cannot be assigned from value of type " + typeof(SupportBean[]).CleanName());

            }
            else {
                Assert.Fail();
            }

            // invalid subquery
            TryInvalidCompile(
                env,
                path,
                "select * from MySentenceEvent[splitSentence((select * from SupportBean#keepall))@type(WordEvent)]",
                "Invalid Contained-event expression 'splitSentence(subselect_0)': Aggregation, sub-select, previous or prior functions are not supported in this context [select * from MySentenceEvent[splitSentence((select * from SupportBean#keepall))@type(WordEvent)]]");

            env.UndeployAll();
        }

        private static void SendMySentenceEvent(
            RegressionEnvironment env,
            EventRepresentationChoice eventRepresentationEnum,
            string sentence)
        {
            if (eventRepresentationEnum.IsObjectArrayEvent()) {
                env.SendEventObjectArray(new object[] {sentence}, "MySentenceEvent");
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
                events[i] = new object[] {words[i]};
            }

            return events;
        }

        public static IDictionary<string, object>[] SplitSentenceBeanMethodReturnMap(
            IDictionary<string, object> sentenceEvent)
        {
            return SplitSentenceMethodReturnMap((string) sentenceEvent.Get("sentence"));
        }

        public static object[][] SplitSentenceBeanMethodReturnObjectArray(object[] sentenceEvent)
        {
            return SplitSentenceMethodReturnObjectArray((string) sentenceEvent[0]);
        }

        public static object[][] SplitWordMethodReturnObjectArray(string word)
        {
            var count = word.Length;
            var events = new object[count][];
            for (var i = 0; i < word.Length; i++) {
                events[i] = new object[] {
                    Convert.ToString(word[i])
                };
            }

            return events;
        }

        public static IDictionary<string, object>[] SplitWordMethodReturnMap(string word)
        {
            IList<IDictionary<string, object>> maps = new List<IDictionary<string, object>>();
            for (var i = 0; i < word.Length; i++) {
                maps.Add(Collections.SingletonDataMap("character", Convert.ToString(word[i])));
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
            return SplitSentenceMethodReturnAvro((string) sentenceEvent.Get("sentence"));
        }

        internal class EPLContainedScriptContextValue : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var script = "@name('mystmt') create expression Object js:myGetScriptContext() [\n" +
                             "function myGetScriptContext() {" +
                             "  return epl;\n" +
                             "}" +
                             "return myGetScriptContext();" +
                             "]";
                env.CompileDeploy(script, path);

                env.CompileDeploy("@name('s0') select myGetScriptContext() as c0 from SupportBean", path)
                    .AddListener("s0");
                env.SendEventBean(new SupportBean());
                var context = (EPLScriptContext) env.Listener("s0").AssertOneGetNewAndReset().Get("c0");
                Assert.IsNotNull(context.EventBeanService);

                env.UndeployAll();
            }
        }

        internal class EPLContainedSplitExprReturnsEventBean : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var collections = typeof(Collections).FullName;
                var path = new RegressionPath();
                var script = "create expression EventBean[] js:mySplitScriptReturnEventBeanArray(value) [\n" +
                             "mySplitScriptReturnEventBeanArray(value);" +
                             "function mySplitScriptReturnEventBeanArray(value) {" +
                             "  var split = value.split(',');\n" +
                             "  var EventBeanArray = Java.type(\"com.espertech.esper.common.client.EventBean[]\");\n" +
                             "  var events = new EventBeanArray(split.Length);  " +
                             "  for (var i = 0; i < split.Length; i++) {\n" +
                             "    var pvalue = split[i].substring(1);\n" +
                             "    if (split[i].startsWith(\"A\")) {\n" +
                             $"      events[i] =  epl.getEventBeanService().adapterForMap({collections}.SingletonDataMap(\"p0\", pvalue), \"AEvent\");\n" +
                             "    }\n" +
                             "    else if (split[i].startsWith(\"B\")) {\n" +
                             $"      events[i] =  epl.getEventBeanService().adapterForMap({collections}.SingletonDataMap(\"p1\", pvalue), \"BEvent\");\n" +
                             "    }\n" +
                             "    else {\n" +
                             "      throw new UnsupportedOperationException(\"Unrecognized type\");\n" +
                             "    }\n" +
                             "  }\n" +
                             "  return events;\n" +
                             "}]";
                env.CompileDeploy(script, path);

                var epl = "create schema BaseEvent();\n" +
                          "create schema AEvent(p0 string) inherits BaseEvent;\n" +
                          "create schema BEvent(p1 string) inherits BaseEvent;\n" +
                          "create schema SplitEvent(value string);\n";
                var compiled = env.CompileWBusPublicType(epl);
                env.Deploy(compiled);
                path.Add(compiled);

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
                var events = env.Listener("s0").GetAndResetLastNewData();
                AssertSplitEx(events[0], "AEvent", "P0", "E1");
                AssertSplitEx(events[1], "BEvent", "P1", "E2");
                AssertSplitEx(events[2], "AEvent", "P0", "E3");

                env.UndeployModuleContaining("s0");
            }
        }

        public static String[] SplitWordMethodReturnJson(String word) {
            var strings = new List<String>();
            for (var i = 0; i < word.Length; i++) {
                var c = word[i].ToString();
                strings.Add("{ \"character\": \"" + c + "\"}");
            }
            return strings.ToArray();

        }

        public static String[] SplitSentenceMethodReturnJson(String sentence)
        {
            var words = sentence.Split(' ');
            var events = new String[words.Length];
            for (var i = 0; i < words.Length; i++) {
                events[i] = "{ \"word\": \"" + words[i] + "\"}";
            }
            return events;
        }

        public static String[] SplitSentenceBeanMethodReturnJson(Object sentenceEvent) {
            String sentence;
            if (sentenceEvent is JsonEventObject) {
                sentence = ((JsonEventObject) sentenceEvent).Get("sentence").ToString();
            } else {
                sentence = ((MyLocalJsonSentence) sentenceEvent).sentence;
            }
            return SplitSentenceMethodReturnJson(sentence);
        }

        internal class EPLContainedSingleRowSplitAndType : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                foreach (var rep in EnumHelper.GetValues<EventRepresentationChoice>()) {
                    TryAssertionSingleRowSplitAndType(env, rep);
                }
            }
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
            public String sentence;
        }

        public class MyLocalJsonWord
        {
            public String word;
        }

        public class MyLocalJsonCharacter
        {
            public String character;
        }
    }
} // end of namespace