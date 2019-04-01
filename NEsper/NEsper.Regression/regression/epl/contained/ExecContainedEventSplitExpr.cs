///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using Avro.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.hook;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.events.avro;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.util;

using NEsper.Avro.Core;
using NEsper.Avro.Extensions;

using NUnit.Framework;

using static com.espertech.esper.supportregression.util.SupportMessageAssertUtil;

// using static org.apache.avro.SchemaBuilder.record;

namespace com.espertech.esper.regression.epl.contained
{
    using Map = IDictionary<string, object>;

    public class ExecContainedEventSplitExpr : RegressionExecution
    {
        public override void Run(EPServiceProvider epService)
        {
            epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction(
                "invalidSentence", GetType(), "InvalidSentenceMethod");

            RunAssertionScriptContextValue(epService);
            RunAssertionSplitExprReturnsEventBean(epService);
            RunAssertionSingleRowSplitAndType(epService);
        }

        private void RunAssertionScriptContextValue(EPServiceProvider epService)
        {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();

            var script = "@Name('mystmt') create expression Object jscript:myGetScriptContext() [\n" +
                         "function myGetScriptContext() {" +
                         "  return epl;\n" +
                         "}" +
                         "return myGetScriptContext();" +
                         "]";
            epService.EPAdministrator.CreateEPL(script);

            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("select myGetScriptContext() as c0 from SupportBean")
                .Events += listener.Update;
            epService.EPRuntime.SendEvent(new SupportBean());
            var context = (EPLScriptContext) listener.AssertOneGetNewAndReset().Get("c0");
            Assert.IsNotNull(context.EventBeanService);

            epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionSplitExprReturnsEventBean(EPServiceProvider epService)
        {
            epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction(
                "mySplitUDFReturnEventBeanArray", GetType(), "MySplitUDFReturnEventBeanArray");

            var script = string.Join(
                "\n",
                "create expression EventBean[] jscript:mySplitScriptReturnEventBeanArray(value) [",
                "function mySplitScriptReturnEventBeanArray(value) {",
                "  var split = value.split(',');",
                "  var etype = host.resolveType('com.espertech.esper.client.EventBean');",
                "  var events = host.newArr(etype, split.length);",
                "  for (var i = 0; i < split.length; i++) {",
                "    var pvalue = split[i].substring(1);",
                "    if (split[i].startsWith(\"A\")) {",
                "      events[i] = epl.EventBeanService.AdapterForMap(Collections.SingletonDataMap(\"p0\", pvalue), \"AEvent\");",
                "    }",
                "    else if (split[i].startsWith(\"B\")) {",
                "      events[i] = epl.EventBeanService.AdapterForMap(Collections.SingletonDataMap(\"p1\", pvalue), \"BEvent\");",
                "    }",
                "    else {",
                "      xhost.throwException('UnsupportedOperationException', 'Unrecognized type');",
                "    }",
                "  }",
                "  return events;",
                "}",
                "return mySplitScriptReturnEventBeanArray(value);",
                "]"
            );

            epService.EPAdministrator.CreateEPL(script);

            var epl = "create schema BaseEvent();\n" +
                      "create schema AEvent(p0 string) inherits BaseEvent;\n" +
                      "create schema BEvent(p1 string) inherits BaseEvent;\n" +
                      "create schema SplitEvent(value string);\n";
            var dep = epService.EPAdministrator.DeploymentAdmin.ParseDeploy(epl);

            TryAssertionSplitExprReturnsEventBean(epService, "mySplitUDFReturnEventBeanArray");
            TryAssertionSplitExprReturnsEventBean(epService, "mySplitScriptReturnEventBeanArray");

            epService.EPAdministrator.DeploymentAdmin.UndeployRemove(dep.DeploymentId);
        }

        private void TryAssertionSplitExprReturnsEventBean(EPServiceProvider epService, string functionOrScript)
        {
            var epl = "@Name('s0') select * from SplitEvent[" + functionOrScript + "(value) @Type(BaseEvent)]";
            var statement = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;

            epService.EPRuntime.SendEvent(Collections.SingletonDataMap("value", "AE1,BE2,AE3"), "SplitEvent");
            var events = listener.GetAndResetLastNewData();
            AssertSplitEx(events[0], "AEvent", "p0", "E1");
            AssertSplitEx(events[1], "BEvent", "p1", "E2");
            AssertSplitEx(events[2], "AEvent", "p0", "E3");

            statement.Dispose();
        }

        private void RunAssertionSingleRowSplitAndType(EPServiceProvider epService)
        {
            foreach (var rep in EnumHelper.GetValues<EventRepresentationChoice>())
            {
                TryAssertionSingleRowSplitAndType(epService, rep);
            }
        }

        private void TryAssertionSingleRowSplitAndType(
            EPServiceProvider epService, EventRepresentationChoice eventRepresentationEnum)
        {
            string[] methods;
            if (eventRepresentationEnum.IsObjectArrayEvent()) {
                methods = new[] {
                    "SplitSentenceMethodReturnObjectArray",
                    "SplitSentenceBeanMethodReturnObjectArray",
                    "SplitWordMethodReturnObjectArray"
                };
            }
            else if (eventRepresentationEnum.IsMapEvent()) {
                methods = new[] {
                    "SplitSentenceMethodReturnMap",
                    "SplitSentenceBeanMethodReturnMap",
                    "SplitWordMethodReturnMap"
                };
            }
            else if (eventRepresentationEnum.IsAvroEvent()) {
                methods = new[] {
                    "SplitSentenceMethodReturnAvro",
                    "SplitSentenceBeanMethodReturnAvro",
                    "SplitWordMethodReturnAvro"
                };
            }
            else
            {
                throw new IllegalStateException("Unrecognized enum " + eventRepresentationEnum);
            }

            var funcs = new[] {
                "SplitSentence",
                "SplitSentenceBean",
                "SplitWord"
            };
            for (var i = 0; i < funcs.Length; i++)
            {
                epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction(
                    funcs[i] + "_" + eventRepresentationEnum.GetName(), GetType(), methods[i]);
            }

            epService.EPAdministrator.CreateEPL(
                eventRepresentationEnum.GetAnnotationText() + " create schema SentenceEvent(sentence string)");
            epService.EPAdministrator.CreateEPL(
                eventRepresentationEnum.GetAnnotationText() + " create schema WordEvent(word string)");
            epService.EPAdministrator.CreateEPL(
                eventRepresentationEnum.GetAnnotationText() + " create schema CharacterEvent(char string)");

            var fields = "word".Split(',');

            // test single-row method
            var stmtText = "select * from SentenceEvent[SplitSentence" + "_" + eventRepresentationEnum.GetName() + "(sentence)@Type(WordEvent)]";
            var stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            Assert.AreEqual("WordEvent", stmt.EventType.Name);
            Assert.IsTrue(eventRepresentationEnum.MatchesClass(stmt.EventType.UnderlyingType));

            SendSentenceEvent(epService, eventRepresentationEnum, "I am testing this code");
            EPAssertionUtil.AssertPropsPerRow(
                listener.GetAndResetLastNewData(), fields,
                new[]
                {
                    new object[] {"I"},
                    new object[] {"am"},
                    new object[] {"testing"},
                    new object[] {"this"},
                    new object[] {"code"}
                });

            SendSentenceEvent(epService, eventRepresentationEnum, "the second event");
            EPAssertionUtil.AssertPropsPerRow(
                listener.GetAndResetLastNewData(), fields,
                new[] {
                    new object[] {"the"},
                    new object[] {"second"},
                    new object[] {"event"}
                });

            stmt.Dispose();

            // test SODA
            var model = epService.EPAdministrator.CompileEPL(stmtText);
            Assert.AreEqual(stmtText, model.ToEPL());
            stmt = epService.EPAdministrator.Create(model);
            Assert.AreEqual(stmtText, stmt.Text);
            stmt.Events += listener.Update;

            SendSentenceEvent(epService, eventRepresentationEnum, "the third event");
            EPAssertionUtil.AssertPropsPerRow(
                listener.GetAndResetLastNewData(), fields,
                new[] {new object[] {"the"}, new object[] {"third"}, new object[] {"event"}});

            stmt.Dispose();

            // test script
            if (eventRepresentationEnum.IsMapEvent())
            {
                stmtText = "expression System.Collections.IList jscript:SplitSentenceJS(sentence) [" +
                           "  debug.Debug('test');" +
                           "  var listType = host.type('System.Collections.ArrayList');" +
                           "  var words = host.newObj(listType);" +
                           "  debug.Debug(words);" +
                           "  words.Add(Collections.SingletonDataMap('word', 'wordOne'));" +
                           "  words.Add(Collections.SingletonDataMap('word', 'wordTwo'));" +
                           "  return words;" +
                           "]" +
                           "select * from SentenceEvent[SplitSentenceJS(sentence)@Type(WordEvent)]";

                stmt = epService.EPAdministrator.CreateEPL(stmtText);
                stmt.Events += listener.Update;
                Assert.AreEqual("WordEvent", stmt.EventType.Name);

                epService.EPRuntime.SendEvent(Collections.EmptyDataMap, "SentenceEvent");
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    listener.GetAndResetLastNewData(), fields, new[]
                    {
                        new object[] {"wordOne"},
                        new object[] {"wordTwo"}
                    });

                stmt.Dispose();
            }

            // test multiple splitters
            stmtText = "select * from SentenceEvent[SplitSentence_" + eventRepresentationEnum.GetName() +
                       "(sentence)@Type(WordEvent)][SplitWord_" + eventRepresentationEnum.GetName() +
                       "(word)@Type(CharacterEvent)]";
            stmt = epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += listener.Update;
            Assert.AreEqual("CharacterEvent", stmt.EventType.Name);

            SendSentenceEvent(epService, eventRepresentationEnum, "I am");
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                listener.GetAndResetLastNewData(), "char".Split(','),
                new[] {new object[] {"I"}, new object[] {"a"}, new object[] {"m"}});

            stmt.Dispose();

            // test wildcard parameter
            stmtText = "select * from SentenceEvent[SplitSentenceBean_" + eventRepresentationEnum.GetName() +
                       "(*)@Type(WordEvent)]";
            stmt = epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += listener.Update;
            Assert.AreEqual("WordEvent", stmt.EventType.Name);

            SendSentenceEvent(epService, eventRepresentationEnum, "another test sentence");
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                listener.GetAndResetLastNewData(), fields, new[]
                {
                    new object[] {"another"},
                    new object[] {"test"},
                    new object[] {"sentence"}
                });

            stmt.Dispose();

            // test property returning untyped collection
            if (eventRepresentationEnum.IsObjectArrayEvent())
            {
                epService.EPAdministrator.Configuration.AddEventType(typeof(ObjectArrayEvent));
                stmtText = eventRepresentationEnum.GetAnnotationText() +
                           " select * from ObjectArrayEvent[someObjectArray@Type(WordEvent)]";
                stmt = epService.EPAdministrator.CreateEPL(stmtText);
                stmt.Events += listener.Update;
                Assert.AreEqual("WordEvent", stmt.EventType.Name);

                var rows = new[]
                {
                    new object[] {"this"},
                    new object[] {"is"},
                    new object[] {"collection"}
                };
                epService.EPRuntime.SendEvent(new ObjectArrayEvent(rows));
                EPAssertionUtil.AssertPropsPerRow(
                    listener.GetAndResetLastNewData(), fields,
                    new[]
                    {
                        new object[] {"this"},
                        new object[] {"is"},
                        new object[] {"collection"}
                    });
                stmt.Dispose();
            }
            else if (eventRepresentationEnum.IsMapEvent())
            {
                epService.EPAdministrator.Configuration.AddEventType(typeof(MyCollectionEvent));
                stmtText = eventRepresentationEnum.GetAnnotationText() +
                           " select * from MyCollectionEvent[someCollection@Type(WordEvent)]";
                stmt = epService.EPAdministrator.CreateEPL(stmtText);
                stmt.Events += listener.Update;
                Assert.AreEqual("WordEvent", stmt.EventType.Name);

                var coll = new List<Map>();
                coll.Add(Collections.SingletonDataMap("word", "this"));
                coll.Add(Collections.SingletonDataMap("word", "is"));
                coll.Add(Collections.SingletonDataMap("word", "collection"));

                epService.EPRuntime.SendEvent(new MyCollectionEvent(coll));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    listener.GetAndResetLastNewData(), fields,
                    new[]
                    {
                        new object[] {"this"},
                        new object[] {"is"},
                        new object[] {"collection"}
                    });
                stmt.Dispose();
            }
            else if (eventRepresentationEnum.IsAvroEvent())
            {
                epService.EPAdministrator.Configuration.AddEventType(typeof(AvroArrayEvent));
                stmtText = eventRepresentationEnum.GetAnnotationText() +
                           " select * from AvroArrayEvent[someAvroArray@Type(WordEvent)]";
                stmt = epService.EPAdministrator.CreateEPL(stmtText);
                stmt.Events += listener.Update;
                Assert.AreEqual("WordEvent", stmt.EventType.Name);

                var rows = new GenericRecord[3];
                var words = "this,is,avro".Split(',');
                for (var i = 0; i < words.Length; i++)
                {
                    rows[i] = new GenericRecord(((AvroEventType) stmt.EventType).SchemaAvro);
                    rows[i].Put("word", words[i]);
                }

                epService.EPRuntime.SendEvent(new AvroArrayEvent(rows));
                EPAssertionUtil.AssertPropsPerRow(
                    listener.GetAndResetLastNewData(), fields,
                    new[]
                    {
                        new object[] {"this"},
                        new object[] {"is"},
                        new object[] {"avro"}
                    });
                stmt.Dispose();
            }
            else
            {
                throw new ArgumentException("Unrecognized enum " + eventRepresentationEnum);
            }

            // invalid: event type not found
            TryInvalid(
                epService, 
                "select * from SentenceEvent[SplitSentence_" + eventRepresentationEnum.GetName() + "(sentence)@type(XYZ)]",
                "Event type by name 'XYZ' could not be found");

            // invalid lib-function annotation
            TryInvalid(
                epService, 
                "select * from SentenceEvent[splitSentence_" + eventRepresentationEnum.GetName() + "(sentence)@dummy(WordEvent)]",
                "Invalid annotation for property selection, expected 'type' but found 'dummy' in text '@dummy(WordEvent)'");

            // invalid type assignment to event type
            if (eventRepresentationEnum.IsObjectArrayEvent())
            {
                TryInvalid(
                    epService,
                    "select * from SentenceEvent[InvalidSentence(sentence)@type(WordEvent)]",
                    "Event type 'WordEvent' underlying type System.Object[] cannot be assigned a value of type");
            }
            else if (eventRepresentationEnum.IsMapEvent())
            {
                TryInvalid(
                    epService,
                    "select * from SentenceEvent[InvalidSentence(sentence)@type(WordEvent)]",
                    "Event type 'WordEvent' underlying type " + Name.Clean<IDictionary<string, object>>() +
                    " cannot be assigned a value of type");
            }
            else if (eventRepresentationEnum.IsAvroEvent())
            {
                TryInvalid(
                    epService,
                    "select * from SentenceEvent[InvalidSentence(sentence)@Type(WordEvent)]",
                    "Event type 'WordEvent' underlying type " + AvroConstantsNoDep.GENERIC_RECORD_CLASSNAME +
                    " cannot be assigned a value of type");
            }
            else
            {
                Assert.Fail();
            }

            // invalid subquery
            TryInvalid(
                epService,
                "select * from SentenceEvent[SplitSentence((select * from SupportBean#keepall))@type(WordEvent)]",
                "Invalid contained-event expression 'SplitSentence(subselect_0)': Aggregation, sub-select, previous or prior functions are not supported in this context [select * from SentenceEvent[SplitSentence((select * from SupportBean#keepall))@type(WordEvent)]]");

            epService.EPAdministrator.DestroyAllStatements();
            foreach (var name in "SentenceEvent,WordEvent,CharacterEvent".Split(','))
            {
                epService.EPAdministrator.Configuration.RemoveEventType(name, true);
            }
        }

        private void SendSentenceEvent(
            EPServiceProvider epService, EventRepresentationChoice eventRepresentationEnum, string sentence)
        {
            if (eventRepresentationEnum.IsObjectArrayEvent())
            {
                epService.EPRuntime.SendEvent(new object[] {sentence}, "SentenceEvent");
            }
            else if (eventRepresentationEnum.IsMapEvent())
            {
                epService.EPRuntime.SendEvent(Collections.SingletonDataMap("sentence", sentence), "SentenceEvent");
            }
            else if (eventRepresentationEnum.IsAvroEvent())
            {
                var schema = SchemaBuilder.Record("sentence", TypeBuilder.RequiredString("sentence"));
                var record = new GenericRecord(schema);
                record.Put("sentence", sentence);
                epService.EPRuntime.SendEventAvro(record, "SentenceEvent");
            }
            else
            {
                throw new IllegalStateException("Unrecognized enum " + eventRepresentationEnum);
            }
        }

        private void AssertSplitEx(EventBean @event, string typeName, string propertyName, string propertyValue)
        {
            Assert.AreEqual(typeName, @event.EventType.Name);
            Assert.AreEqual(propertyValue, @event.Get(propertyName));
        }

        public static EventBean[] MySplitUDFReturnEventBeanArray(string value, EPLMethodInvocationContext context)
        {
            var split = value.Split(',');
            var events = new EventBean[split.Length];
            for (var i = 0; i < split.Length; i++)
            {
                var pvalue = split[i].Substring(1);
                if (split[i].StartsWith("A"))
                {
                    events[i] = context.EventBeanService.AdapterForMap(
                        Collections.SingletonDataMap("p0", pvalue), "AEvent");
                }
                else if (split[i].StartsWith("B"))
                {
                    events[i] = context.EventBeanService.AdapterForMap(
                        Collections.SingletonDataMap("p1", pvalue), "BEvent");
                }
                else
                {
                    throw new UnsupportedOperationException("Unrecognized type");
                }
            }

            return events;
        }

        public static Map[] SplitSentenceMethodReturnMap(string sentence)
        {
            var words = sentence.Split(' ');
            var events = new Map[words.Length];
            for (var i = 0; i < words.Length; i++)
            {
                events[i] = Collections.SingletonDataMap("word", words[i]);
            }

            return events;
        }

        public static GenericRecord[] SplitSentenceMethodReturnAvro(string sentence)
        {
            var wordSchema = SchemaBuilder.Record("word", TypeBuilder.RequiredString("word"));
            var words = sentence.Split(' ');
            var events = new GenericRecord[words.Length];
            for (var i = 0; i < words.Length; i++)
            {
                events[i] = new GenericRecord(wordSchema);
                events[i].Put("word", words[i]);
            }

            return events;
        }

        public static object[][] SplitSentenceMethodReturnObjectArray(string sentence)
        {
            var words = sentence.Split(' ');
            var events = new object[words.Length][];
            for (var i = 0; i < words.Length; i++)
            {
                events[i] = new object[] {words[i]};
            }

            return events;
        }

        public static Map[] SplitSentenceBeanMethodReturnMap(Map sentenceEvent)
        {
            return SplitSentenceMethodReturnMap((string) sentenceEvent.Get("sentence"));
        }

        public static GenericRecord[] SplitSentenceBeanMethodReturnAvro(GenericRecord sentenceEvent)
        {
            return SplitSentenceMethodReturnAvro((string) sentenceEvent.Get("sentence"));
        }

        public static object[][] SplitSentenceBeanMethodReturnObjectArray(object[] sentenceEvent)
        {
            return SplitSentenceMethodReturnObjectArray((string) sentenceEvent[0]);
        }

        public static object[][] SplitWordMethodReturnObjectArray(string word)
        {
            var count = word.Length;
            var events = new object[count][];
            for (var i = 0; i < word.Length; i++)
            {
                events[i] = new object[] {word[i].ToString()};
            }

            return events;
        }

        public static Map[] SplitWordMethodReturnMap(string word)
        {
            var maps = new List<Map>();
            for (var i = 0; i < word.Length; i++)
            {
                maps.Add(Collections.SingletonDataMap("char", word[i].ToString()));
            }

            return maps.ToArray();
        }

        public static GenericRecord[] SplitWordMethodReturnAvro(string word)
        {
            var schema = SchemaBuilder.Record("chars", TypeBuilder.RequiredString("char"));
            var records = new GenericRecord[word.Length];
            for (var i = 0; i < word.Length; i++)
            {
                records[i] = new GenericRecord(schema);
                records[i].Put("char", word[i].ToString());
            }

            return records;
        }

        public static SupportBean[] InvalidSentenceMethod(string sentence)
        {
            return null;
        }

        public class CollectionEvent<T>
        {
            public CollectionEvent(ICollection<T> someCollection)
            {
                SomeCollection = someCollection;
            }

            public ICollection<T> SomeCollection { get; set; }
        }

        public class MyCollectionEvent : CollectionEvent<Map>
        {
            public MyCollectionEvent(ICollection<Map> someCollection) : base(someCollection)
            {
            }
        }

        public class ObjectArrayEvent
        {
            public ObjectArrayEvent(object[][] someObjectArray)
            {
                SomeObjectArray = someObjectArray;
            }

            public object[][] SomeObjectArray { get; }
        }

        public class AvroArrayEvent
        {
            public AvroArrayEvent(GenericRecord[] someAvroArray)
            {
                SomeAvroArray = someAvroArray;
            }

            public GenericRecord[] SomeAvroArray { get; }
        }
    }
} // end of namespace