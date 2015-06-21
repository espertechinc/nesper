///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
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
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName);}
	        _listener = new SupportUpdateListener();
	    }

        [TearDown]
	    public void TearDown() {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
	        _listener = null;
	    }

        [Test]
	    public void TestSingleRowSplitAndType() {
	        RunAssertionSingleRowSplitAndType(EventRepresentationEnum.OBJECTARRAY);
	        RunAssertionSingleRowSplitAndType(EventRepresentationEnum.MAP);
	        RunAssertionSingleRowSplitAndType(EventRepresentationEnum.DEFAULT);
	    }

	    private void RunAssertionSingleRowSplitAndType(EventRepresentationEnum eventRepresentationEnum) {
	        if (eventRepresentationEnum.IsObjectArrayEvent()) {
	            _epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("splitSentence", GetType().FullName, "SplitSentenceMethodReturnObjectArray");
                _epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("splitSentenceBean", GetType().FullName, "SplitSentenceBeanMethodReturnObjectArray");
                _epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("splitWord", GetType().FullName, "SplitWordMethodReturnObjectArray");
	        }
	        else {
                _epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("splitSentence", GetType().FullName, "SplitSentenceMethodReturnMap");
                _epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("splitSentenceBean", GetType().FullName, "SplitSentenceBeanMethodReturnMap");
                _epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("splitWord", GetType().FullName, "SplitWordMethodReturnMap");
	        }
            _epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("invalidSentence", GetType().FullName, "InvalidSentenceMethod");

	        _epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema SentenceEvent(sentence String)");
            _epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema WordEvent(word String)");
            _epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema CharacterEvent(char String)");

	        string stmtText;
	        EPStatement stmt;
	        var fields = "word".Split(',');

	        // test single-row method
	        stmtText = "select * from SentenceEvent[splitSentence(sentence)@type(WordEvent)]";
	        stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(_listener);
	        Assert.AreEqual("WordEvent", stmt.EventType.Name);
	        Assert.AreEqual(eventRepresentationEnum.GetOutputClass(), stmt.EventType.UnderlyingType);

	        SendSentenceEvent(eventRepresentationEnum, "I am testing this code");
            EPAssertionUtil.AssertPropsPerRow(
                _listener.GetAndResetLastNewData(), fields,
                new object[][]
                {
                    new object[] { "I" }, 
                    new object[] { "am" }, 
                    new object[] { "testing" }, 
                    new object[] { "this" }, 
                    new object[] { "code" }
                });

	        SendSentenceEvent(eventRepresentationEnum, "the second event");
            EPAssertionUtil.AssertPropsPerRow(
                _listener.GetAndResetLastNewData(), fields,
                new object[][]
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
            EPAssertionUtil.AssertPropsPerRow(
                _listener.GetAndResetLastNewData(), fields, 
                new object[][]
                {
                    new object[] { "the" }, 
                    new object[] { "third" }, 
                    new object[] { "event" }
                });

	        stmt.Dispose();

	        // test script
	        if (!eventRepresentationEnum.IsObjectArrayEvent()) {
                stmtText = "expression com.espertech.esper.support.collections.ISupportDataMapCollection js:splitSentenceJS(sentence) [" +
                        "  var words = clr.New('com.espertech.esper.support.collections.SupportDataMapList',[]);" +
                        "  var factory = clr.New('com.espertech.esper.support.collections.SupportDataMapFactory',[]);" +
                        "  words.Add(factory.Create('word', 'wordOne'));" +
	                    "  words.Add(factory.Create('word', 'wordTwo'));" +
	                    "  words;" +
	                    "]" +
	                    "select * from SentenceEvent[splitSentenceJS(sentence)@type(WordEvent)]";
	            stmt = _epService.EPAdministrator.CreateEPL(stmtText).AddListener(_listener);
	            Assert.AreEqual("WordEvent", stmt.EventType.Name);

	            _epService.EPRuntime.SendEvent(Collections.EmptyDataMap, "SentenceEvent");
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    _listener.GetAndResetLastNewData(), fields,
                    new object[][]
                    {
                        new object[] { "wordOne" },
                        new object[] { "wordTwo" }
                    });

	            stmt.Dispose();
	        }

	        // test multiple splitters
	        stmtText = "select * from SentenceEvent[splitSentence(sentence)@type(WordEvent)][splitWord(word)@type(CharacterEvent)]";
	        stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(_listener);
	        Assert.AreEqual("CharacterEvent", stmt.EventType.Name);

	        SendSentenceEvent(eventRepresentationEnum, "I am");
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                _listener.GetAndResetLastNewData(), "char".Split(','),
                new object[][]
                {
                    new object[] { "I" }, 
                    new object[] { "a" }, 
                    new object[] { "m" }
                });

	        stmt.Dispose();

	        // test wildcard parameter
	        stmtText = "select * from SentenceEvent[splitSentenceBean(*)@type(WordEvent)]";
	        stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(_listener);
	        Assert.AreEqual("WordEvent", stmt.EventType.Name);

	        SendSentenceEvent(eventRepresentationEnum, "another test sentence");
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                _listener.GetAndResetLastNewData(), fields, 
                new object[][]
                {
                    new object[] { "another" }, 
                    new object[] { "test" },
                    new object[] { "sentence" }
                });

	        stmt.Dispose();

	        // test property returning untyped collection
	        if (eventRepresentationEnum.IsObjectArrayEvent()) {
	            _epService.EPAdministrator.Configuration.AddEventType(typeof(ObjectArrayEvent));
	            stmtText = eventRepresentationEnum.GetAnnotationText() + " select * from ObjectArrayEvent[someObjectArray@type(WordEvent)]";
	            stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	            stmt.AddListener(_listener);
	            Assert.AreEqual("WordEvent", stmt.EventType.Name);

                var rows = new object[][] { new object[] { "this" }, new object[] { "is" }, new object[] { "collection" } };
	            _epService.EPRuntime.SendEvent(new ObjectArrayEvent(rows));
                EPAssertionUtil.AssertPropsPerRow(
                    _listener.GetAndResetLastNewData(), fields,
                    new object[][]
                    {
                        new object[] { "this" }, 
                        new object[] { "is" }, 
                        new object[] { "collection" }
                    });
	        }
	        else
            {
                _epService.EPAdministrator.Configuration.AddEventType<CollectionEvent<IDictionary<string, object>>>("CollectionEvent");
	            stmtText = eventRepresentationEnum.GetAnnotationText() + " select * from CollectionEvent[someCollection@type(WordEvent)]";
	            stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	            stmt.AddListener(_listener);
	            Assert.AreEqual("WordEvent", stmt.EventType.Name);

	            var coll = new List<IDictionary<string, object>>();
	            coll.Add(Collections.SingletonDataMap("word", "this"));
	            coll.Add(Collections.SingletonDataMap("word", "is"));
	            coll.Add(Collections.SingletonDataMap("word", "collection"));

	            _epService.EPRuntime.SendEvent(new CollectionEvent<IDictionary<string, object>>(coll));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    _listener.GetAndResetLastNewData(), fields, 
                    new object[][]
                    {
                        new object[] { "this" }, 
                        new object[] { "is" }, 
                        new object[] { "collection" }
                    });
	        }

	        // invalid: event type not found
	        TryInvalid("select * from SentenceEvent[splitSentence(sentence)@type(XYZ)]",
	                   "Event type by name 'XYZ' could not be found [select * from SentenceEvent[splitSentence(sentence)@type(XYZ)]]");

	        // invalid lib-function annotation
	        TryInvalid("select * from SentenceEvent[splitSentence(sentence)@dummy(WordEvent)]",
	                   "Invalid annotation for property selection, expected 'type' but found 'dummy' in text '[splitSentence(sentence)@dummy(WordEvent)]' [select * from SentenceEvent[splitSentence(sentence)@dummy(WordEvent)]]");

	        // invalid type assignment to event type
	        if (eventRepresentationEnum.IsObjectArrayEvent()) {
	            TryInvalid("select * from SentenceEvent[invalidSentence(sentence)@type(WordEvent)]",
	                       "Event type 'WordEvent' underlying type System.Object[] cannot be assigned a value of type");
	        }
	        else {
	            TryInvalid("select * from SentenceEvent[invalidSentence(sentence)@type(WordEvent)]",
	                       "Event type 'WordEvent' underlying type " + Name.Of<IDictionary<string, object>>() + " cannot be assigned a value of type");
	        }

	        // invalid subquery
	        TryInvalid("select * from SentenceEvent[splitSentence((select * from SupportBean.win:keepall()))@type(WordEvent)]",
	                   "Invalid contained-event expression 'splitSentence(subselect_0)': Aggregation, sub-select, previous or prior functions are not supported in this context [select * from SentenceEvent[splitSentence((select * from SupportBean.win:keepall()))@type(WordEvent)]]");

	        _epService.Initialize();
	    }

	    private void SendSentenceEvent(EventRepresentationEnum eventRepresentationEnum, string sentence) {
	        if (eventRepresentationEnum.IsObjectArrayEvent()) {
	            _epService.EPRuntime.SendEvent(new object[] {sentence}, "SentenceEvent");
	        }
	        else {
	            _epService.EPRuntime.SendEvent(
                    Collections.SingletonDataMap("sentence", sentence), "SentenceEvent");
	        }
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

	    public static object[][] SplitSentenceMethodReturnObjectArray(string sentence) {
	        var words = sentence.Split(' ');
	        var events = new object[words.Length][];
	        for (var i = 0; i < words.Length; i++) {
	            events[i] = new object[] {words[i]};
	        }
	        return events;
	    }

        public static IDictionary<string, object>[] SplitSentenceBeanMethodReturnMap(IDictionary<string, object> sentenceEvent)
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
	            events[i] = new object[] { word[i].ToString() };
	        }
	        return events;
	    }

        public static IDictionary<string, object>[] SplitWordMethodReturnMap(string word)
        {
            var maps = new List<IDictionary<string, object>>();
	        for (var i = 0; i < word.Length; i++)
	        {
	            maps.Add(Collections.SingletonDataMap("char", word[i].ToString()));
	        }
	        return maps.ToArray();
	    }

	    public static SupportBean[] InvalidSentenceMethod(string sentence) {
	        return null;
	    }

	    private void TryInvalid(string epl, string expected) {
	        try {
	            _epService.EPAdministrator.CreateEPL(epl);
	            Assert.Fail();
	        }
	        catch (EPStatementException ex)
	        {
	            Assert.IsFalse(string.IsNullOrEmpty(expected));
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

	    public class ObjectArrayEvent
        {
	        public ObjectArrayEvent(object[][] someObjectArray)
            {
	            SomeObjectArray = someObjectArray;
	        }

	        public object[][] SomeObjectArray { get; private set; }
        }
	}
} // end of namespace
