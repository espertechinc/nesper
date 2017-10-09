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
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.regression.script;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.util;

using NEsper.Avro.Core;
using NEsper.Avro.Extensions;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl
{
    using Map = IDictionary<string, object>;

    [TestFixture]
    public class TestContainedEventSplitExpr
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;

        [SetUp]
        public void SetUp()
        {
            _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _listener = new SupportUpdateListener();
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listener = null;
        }

        [Test]
        public void TestScriptContextValue()
        {
            _epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBean));

            var script = "@Name('mystmt') create expression Object jscript:MyGetScriptContext() [\n" +
                            "function MyGetScriptContext() {" +
                            "  return epl;\n" +
                            "}" +
                            "return MyGetScriptContext();" +
                            "]";
            _epService.EPAdministrator.CreateEPL(script);

            _epService.EPAdministrator.CreateEPL("select MyGetScriptContext() as c0 from SupportBean").AddListener(_listener);
            _epService.EPRuntime.SendEvent(new SupportBean());
            var context = (EPLScriptContext)_listener.AssertOneGetNewAndReset().Get("c0");
            Assert.IsNotNull(context.EventBeanService);
        }

        [Test]
        public void TestSplitExprReturnsEventBean()
        {
            _epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("mySplitUDFReturnEventBeanArray", GetType().FullName, "MySplitUDFReturnEventBeanArray");

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
            _epService.EPAdministrator.CreateEPL(script);

            var epl = "create schema BaseEvent();\n" +
                         "create schema AEvent(p0 string) inherits BaseEvent;\n" +
                         "create schema BEvent(p1 string) inherits BaseEvent;\n" +
                         "create schema SplitEvent(value string);\n";
            _epService.EPAdministrator.DeploymentAdmin.ParseDeploy(epl);

            RunAssertionSplitExprReturnsEventBean("mySplitUDFReturnEventBeanArray");
            RunAssertionSplitExprReturnsEventBean("mySplitScriptReturnEventBeanArray");
        }

        private void RunAssertionSplitExprReturnsEventBean(string functionOrScript)
        {
            var epl = "@Name('s0') select * from SplitEvent[" + functionOrScript + "(value) @Type(BaseEvent)]";
            var statement = _epService.EPAdministrator.CreateEPL(epl);
            statement.AddListener(_listener);

            _epService.EPRuntime.SendEvent(Collections.SingletonDataMap("value", "AE1,BE2,AE3"), "SplitEvent");
            var events = _listener.GetAndResetLastNewData();
            AssertSplitEx(events[0], "AEvent", "p0", "E1");
            AssertSplitEx(events[1], "BEvent", "p1", "E2");
            AssertSplitEx(events[2], "AEvent", "p0", "E3");

            statement.Dispose();
        }

        [Test]
        public void TestSingleRowSplitAndType()
        {
            EnumHelper.ForEach<EventRepresentationChoice>(rep => RunAssertionSingleRowSplitAndType(rep));
        }

        private void RunAssertionSingleRowSplitAndType(EventRepresentationChoice eventRepresentationEnum)
        {
            string[] methods;
            if (eventRepresentationEnum.IsObjectArrayEvent())
            {
                methods = "SplitSentenceMethodReturnObjectArray,SplitSentenceBeanMethodReturnObjectArray,SplitWordMethodReturnObjectArray".Split(',');
            }
            else if (eventRepresentationEnum.IsMapEvent())
            {
                methods = "SplitSentenceMethodReturnMap,SplitSentenceBeanMethodReturnMap,SplitWordMethodReturnMap".Split(',');
            }
            else if (eventRepresentationEnum.IsAvroEvent())
            {
                methods = "SplitSentenceMethodReturnAvro,SplitSentenceBeanMethodReturnAvro,SplitWordMethodReturnAvro".Split(',');
            }
            else
            {
                throw new IllegalStateException("Unrecognized enum " + eventRepresentationEnum);
            }
            var funcs = "SplitSentence,SplitSentenceBean,SplitWord".Split(',');
            for (var i = 0; i < funcs.Length; i++)
            {
                _epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction(funcs[i], GetType().FullName, methods[i]);
            }
            _epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("invalidSentence", GetType().FullName, "InvalidSentenceMethod");

            _epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema SentenceEvent(sentence string)");
            _epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema WordEvent(word string)");
            _epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema CharacterEvent(char string)");

            string stmtText;
            EPStatement stmt;
            var fields = "word".Split(',');

            // test single-row method
            stmtText = "select * from SentenceEvent[SplitSentence(sentence)@Type(WordEvent)]";
            stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.AddListener(_listener);
            Assert.AreEqual("WordEvent", stmt.EventType.Name);
            Assert.IsTrue(eventRepresentationEnum.MatchesClass(stmt.EventType.UnderlyingType));

            SendSentenceEvent(eventRepresentationEnum, "I am testing this code");
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, new Object[][]
            {
                new object[] { "I" },
                new object[] { "am" },
                new object[] { "testing" },
                new object[] { "this" },
                new object[] { "code" }
            });

            SendSentenceEvent(eventRepresentationEnum, "the second event");
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, new Object[][]
            {
                new object[] { "the" },
                new object[] { "second" },
                new object[] { "event" }
            });

            stmt.Dispose();

            // test SODA
            var model = _epService.EPAdministrator.CompileEPL(stmtText);
            Assert.AreEqual(stmtText, model.ToEPL());
            stmt = _epService.EPAdministrator.Create(model);
            Assert.AreEqual(stmtText, stmt.Text);
            stmt.AddListener(_listener);

            SendSentenceEvent(eventRepresentationEnum, "the third event");
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, new Object[][]
            {
                new object[] { "the" },
                new object[] { "third" },
                new object[] { "event" }
            });

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

                stmt = _epService.EPAdministrator.CreateEPL(stmtText);
                stmt.AddListener(_listener);
                Assert.AreEqual("WordEvent", stmt.EventType.Name);

                _epService.EPRuntime.SendEvent(Collections.EmptyDataMap, "SentenceEvent");
                EPAssertionUtil.AssertPropsPerRowAnyOrder(_listener.GetAndResetLastNewData(), fields, new Object[][]
                {
                    new object[] { "wordOne" },
                    new object[] { "wordTwo" }
                });

                stmt.Dispose();
            }

            // test multiple splitters
            stmtText = "select * from SentenceEvent[SplitSentence(sentence)@Type(WordEvent)][SplitWord(word)@Type(CharacterEvent)]";
            stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.AddListener(_listener);
            Assert.AreEqual("CharacterEvent", stmt.EventType.Name);

            SendSentenceEvent(eventRepresentationEnum, "I am");
            EPAssertionUtil.AssertPropsPerRowAnyOrder(_listener.GetAndResetLastNewData(), "char".Split(','), new Object[][]
            {
                new object[] { "I" },
                new object[] { "a" },
                new object[] { "m" }
            });

            stmt.Dispose();

            // test wildcard parameter
            stmtText = "select * from SentenceEvent[SplitSentenceBean(*)@Type(WordEvent)]";
            stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.AddListener(_listener);
            Assert.AreEqual("WordEvent", stmt.EventType.Name);

            SendSentenceEvent(eventRepresentationEnum, "another test sentence");
            EPAssertionUtil.AssertPropsPerRowAnyOrder(_listener.GetAndResetLastNewData(), fields, new Object[][]
            {
                new object[] { "another" },
                new object[] { "test" },
                new object[] { "sentence" }
            });

            stmt.Dispose();

            // test property returning untyped collection
            if (eventRepresentationEnum.IsObjectArrayEvent())
            {
                _epService.EPAdministrator.Configuration.AddEventType(typeof(ObjectArrayEvent));
                stmtText = eventRepresentationEnum.GetAnnotationText() + " select * from ObjectArrayEvent[someObjectArray@Type(WordEvent)]";
                stmt = _epService.EPAdministrator.CreateEPL(stmtText);
                stmt.AddListener(_listener);
                Assert.AreEqual("WordEvent", stmt.EventType.Name);

                var rows = new Object[][]
                {
                    new object[] { "this" },
                    new object[] { "is" },
                    new object[] { "collection" }
                };
                _epService.EPRuntime.SendEvent(new ObjectArrayEvent(rows));
                EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, new Object[][]
                {
                    new object[] { "this" },
                    new object[] { "is" },
                    new object[] { "collection" }
                });
                stmt.Dispose();
            }
            else if (eventRepresentationEnum.IsMapEvent())
            {
                _epService.EPAdministrator.Configuration.AddEventType(typeof(MyCollectionEvent));
                stmtText = eventRepresentationEnum.GetAnnotationText() + " select * from MyCollectionEvent[someCollection@Type(WordEvent)]";
                stmt = _epService.EPAdministrator.CreateEPL(stmtText);
                stmt.AddListener(_listener);
                Assert.AreEqual("WordEvent", stmt.EventType.Name);

                var coll = new List<Map>();
                coll.Add(Collections.SingletonDataMap("word", "this"));
                coll.Add(Collections.SingletonDataMap("word", "is"));
                coll.Add(Collections.SingletonDataMap("word", "collection"));

                _epService.EPRuntime.SendEvent(new MyCollectionEvent(coll));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(_listener.GetAndResetLastNewData(), fields, new Object[][]
                {
                    new object[] { "this" },
                    new object[] { "is" },
                    new object[] { "collection" }
                });
                stmt.Dispose();
            }
            else if (eventRepresentationEnum.IsAvroEvent())
            {
                _epService.EPAdministrator.Configuration.AddEventType(typeof(AvroArrayEvent));
                stmtText = eventRepresentationEnum.GetAnnotationText() + " select * from AvroArrayEvent[someAvroArray@Type(WordEvent)]";
                stmt = _epService.EPAdministrator.CreateEPL(stmtText);
                stmt.AddListener(_listener);
                Assert.AreEqual("WordEvent", stmt.EventType.Name);

                var rows = new GenericRecord[3];
                var words = "this,is,avro".Split(',');
                for (var i = 0; i < words.Length; i++)
                {
                    rows[i] = new GenericRecord(((AvroEventType)stmt.EventType).SchemaAvro);
                    rows[i].Put("word", words[i]);
                }
                _epService.EPRuntime.SendEvent(new AvroArrayEvent(rows));
                EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, new Object[][]
                {
                    new object[] { "this" },
                    new object[] { "is" },
                    new object[] { "avro" }
                });
                stmt.Dispose();
            }
            else
            {
                throw new ArgumentException("Unrecognized enum " + eventRepresentationEnum);
            }

            // invalid: event type not found
            TryInvalid("select * from SentenceEvent[SplitSentence(sentence)@type(XYZ)]",
                "Event type by name 'XYZ' could not be found [select * from SentenceEvent[SplitSentence(sentence)@type(XYZ)]]");

            // invalid lib-function annotation
            TryInvalid("select * from SentenceEvent[SplitSentence(sentence)@dummy(WordEvent)]",
                "Invalid annotation for property selection, expected 'type' but found 'dummy' in text '@dummy(WordEvent)'");

            // invalid type assignment to event type
            if (eventRepresentationEnum.IsObjectArrayEvent())
            {
                TryInvalid("select * from SentenceEvent[InvalidSentence(sentence)@type(WordEvent)]",
                    "Event type 'WordEvent' underlying type System.Object[] cannot be assigned a value of type");
            }
            else if (eventRepresentationEnum.IsMapEvent())
            {
                TryInvalid("select * from SentenceEvent[InvalidSentence(sentence)@type(WordEvent)]",
                    "Event type 'WordEvent' underlying type " + Name.Of<IDictionary<string, object>>() + " cannot be assigned a value of type");
            }
            else if (eventRepresentationEnum.IsAvroEvent())
            {
                TryInvalid("select * from SentenceEvent[InvalidSentence(sentence)@Type(WordEvent)]",
                    "Event type 'WordEvent' underlying type " + AvroConstantsNoDep.GENERIC_RECORD_CLASSNAME + " cannot be assigned a value of type");
            }
            else
            {
                Assert.Fail();
            }

            // invalid subquery
            TryInvalid("select * from SentenceEvent[SplitSentence((select * from SupportBean#keepall))@type(WordEvent)]",
                "Invalid contained-event expression 'SplitSentence(subselect_0)': Aggregation, sub-select, previous or prior functions are not supported in this context [select * from SentenceEvent[SplitSentence((select * from SupportBean#keepall))@type(WordEvent)]]");

            _epService.Initialize();
        }

        private void SendSentenceEvent(EventRepresentationChoice eventRepresentationEnum, string sentence)
        {
            if (eventRepresentationEnum.IsObjectArrayEvent())
            {
                _epService.EPRuntime.SendEvent(new Object[] { sentence }, "SentenceEvent");
            }
            else if (eventRepresentationEnum.IsMapEvent())
            {
                _epService.EPRuntime.SendEvent(Collections.SingletonDataMap("sentence", sentence), "SentenceEvent");
            }
            else if (eventRepresentationEnum.IsAvroEvent())
            {
                var schema = SchemaBuilder.Record("sentence", TypeBuilder.RequiredString("sentence"));
                var record = new GenericRecord(schema);
                record.Put("sentence", sentence);
                _epService.EPRuntime.SendEventAvro(record, "SentenceEvent");
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
                    events[i] = context.EventBeanService.AdapterForMap(Collections.SingletonDataMap("p0", pvalue), "AEvent");
                }
                else if (split[i].StartsWith("B"))
                {
                    events[i] = context.EventBeanService.AdapterForMap(Collections.SingletonDataMap("p1", pvalue), "BEvent");
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

        public static Object[][] SplitSentenceMethodReturnObjectArray(string sentence)
        {
            var words = sentence.Split(' ');
            var events = new Object[words.Length][];
            for (var i = 0; i < words.Length; i++)
            {
                events[i] = new Object[] { words[i] };
            }
            return events;
        }

        public static Map[] SplitSentenceBeanMethodReturnMap(Map sentenceEvent)
        {
            return SplitSentenceMethodReturnMap((string)sentenceEvent.Get("sentence"));
        }

        public static GenericRecord[] SplitSentenceBeanMethodReturnAvro(GenericRecord sentenceEvent)
        {
            return SplitSentenceMethodReturnAvro((string)sentenceEvent.Get("sentence"));
        }

        public static Object[][] SplitSentenceBeanMethodReturnObjectArray(Object[] sentenceEvent)
        {
            return SplitSentenceMethodReturnObjectArray((string)sentenceEvent[0]);
        }

        public static Object[][] SplitWordMethodReturnObjectArray(string word)
        {
            int count = word.Length;
            var events = new Object[count][];
            for (var i = 0; i < word.Length; i++)
            {
                events[i] = new Object[] { word[i].ToString() };
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

        private void TryInvalid(string epl, string expected)
        {
            try
            {
                _epService.EPAdministrator.CreateEPL(epl);
                Assert.Fail();
            }
            catch (EPStatementException ex)
            {
                Assert.IsFalse(String.IsNullOrEmpty(expected));
                Assert.IsTrue(ex.Message.StartsWith(expected), "Received message: " + ex.Message);
            }
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
            public ObjectArrayEvent(Object[][] someObjectArray)
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
