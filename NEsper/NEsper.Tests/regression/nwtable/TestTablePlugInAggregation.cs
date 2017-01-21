///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.client;
using com.espertech.esper.client.hook;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.agg.aggregator;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.rettype;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.plugin;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable
{
    [TestFixture]
    public class TestTablePlugInAggregation
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp()
        {
            _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            _epService.Initialize();
            foreach (var clazz in new Type[] {typeof(SupportBean), typeof(SupportBean_S0), typeof(SupportBean_S1)}) {
                _epService.EPAdministrator.Configuration.AddEventType(clazz);
            }
            _listener = new SupportUpdateListener();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), GetType().FullName);}
        }
    
        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
            _listener = null;
        }
    
        // CSV-building over a limited set of values.
        //
        // Use aggregation method single-value when the aggregation has a natural current value
        // that can be obtained without asking it a question.
        [Test]
        public void TestPlugInAggMethod_CSVLast3Strings()
        {
            _epService.EPAdministrator.Configuration.AddPlugInAggregationFunctionFactory("csvWords", typeof(SimpleWordCSVFactory).FullName);
    
            _epService.EPAdministrator.CreateEPL("create table varagg (csv csvWords())");
            _epService.EPAdministrator.CreateEPL("select varagg.csv as c0 from SupportBean_S0").AddListener(_listener);
            _epService.EPAdministrator.CreateEPL("into table varagg select csvWords(TheString) as csv from SupportBean.win:length(3)");
    
            SendWordAssert("the", "the");
            SendWordAssert("fox", "the,fox");
            SendWordAssert("jumps", "the,fox,jumps");
            SendWordAssert("over", "fox,jumps,over");
        }
    
        // Word counting using a reference-counting-map (similar: count-min-sketch approximation, this one is more limited)
        //
        // Use aggregation access multi-value when the aggregation must be asked a specific question to return a useful value.
        [Test]
        public void TestPlugInAccess_RefCountedMap()
        {
            var config = new ConfigurationPlugInAggregationMultiFunction(
                    "referenceCountedMap,referenceCountLookup".Split(','), typeof(ReferenceCountedMapMultiValueFactory).FullName);
            _epService.EPAdministrator.Configuration.AddPlugInAggregationMultiFunction(config);
    
            _epService.EPAdministrator.CreateEPL("create table varagg (wordCount referenceCountedMap(string))");
            _epService.EPAdministrator.CreateEPL("into table varagg select referenceCountedMap(TheString) as wordCount from SupportBean.win:length(3)");
            _epService.EPAdministrator.CreateEPL("select varagg.wordCount.referenceCountLookup(p00) as c0 from SupportBean_S0").AddListener(_listener);
    
            var words = "the,house,is,green";
            SendWordAssert("the", words, new int?[]{1, null, null, null});
            SendWordAssert("house", words, new int?[]{1, 1, null, null});
            SendWordAssert("the", words, new int?[]{2, 1, null, null});
            SendWordAssert("green", words, new int?[]{1, 1, null, 1});
            SendWordAssert("is", words, new int?[]{1, null, 1, 1});
        }
    
        private void SendWordAssert(string word, string expected)
        {
            _epService.EPRuntime.SendEvent(new SupportBean(word, 0));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            Assert.AreEqual(expected, _listener.AssertOneGetNewAndReset().Get("c0"));
        }
    
        private void SendWordAssert(string word, string wordCSV, int?[] counts)
        {
            _epService.EPRuntime.SendEvent(new SupportBean(word, 0));
    
            var words = wordCSV.Split(',');
            for (var i = 0; i < words.Length; i++) {
                _epService.EPRuntime.SendEvent(new SupportBean_S0(0, words[i]));
                var count = (int?) _listener.AssertOneGetNewAndReset().Get("c0");
                Assert.AreEqual(counts[i], count, "failed for word '" + words[i] + "'");
            }
        }
    
        public class SimpleWordCSVFactory : AggregationFunctionFactory
        {
            public virtual string FunctionName
            {
                set { }
            }

            public virtual void Validate(AggregationValidationContext validationContext)
            {
            }

            public virtual AggregationMethod NewAggregator()
            {
                return new SimpleWordCSVMethod();
            }

            public virtual Type ValueType
            {
                get { return typeof (string); }
            }
        }
    
        public class SimpleWordCSVMethod : AggregationMethod
        {
            private readonly IDictionary<String, int?> _countPerWord = new LinkedHashMap<string, int?>();
    
            public void Enter(object value)
            {
                var word = (string) value;
                var count = _countPerWord.Get(word);
                if (count == null) {
                    _countPerWord.Put(word, 1);
                }
                else {
                    _countPerWord.Put(word, count + 1);
                }
            }
    
            public void Leave(object value)
            {
                var word = (string) value;
                var count = _countPerWord.Get(word);
                if (count == null) {
                    _countPerWord.Put(word, 1);
                }
                else if (count == 1) {
                    _countPerWord.Remove(word);
                }
                else {
                    _countPerWord.Put(word, count - 1);
                }
            }

            public object Value
            {
                get
                {
                    var writer = new StringWriter();
                    var delimiter = "";
                    foreach (var entry in _countPerWord)
                    {
                        writer.Write(delimiter);
                        delimiter = ",";
                        writer.Write(entry.Key);
                    }
                    return writer.ToString();
                }
            }

            public void Clear()
            {
                _countPerWord.Clear();
            }
        }
    
        public class ReferenceCountedMapMultiValueFactory : PlugInAggregationMultiFunctionFactory
        {
            private readonly static AggregationStateKey SharedStateKey = new ProxyAggregationStateKey {};
    
            public void AddAggregationFunction(PlugInAggregationMultiFunctionDeclarationContext declarationContext)
            {
            }
    
            public PlugInAggregationMultiFunctionHandler ValidateGetHandler(PlugInAggregationMultiFunctionValidationContext validationContext)
            {
                if (validationContext.FunctionName.Equals("referenceCountedMap"))
                {
                    return new ReferenceCountedMapFunctionHandler(SharedStateKey);
                }
                if (validationContext.FunctionName.Equals("referenceCountLookup"))
                {
                    var eval = validationContext.ParameterExpressions[0].ExprEvaluator;
                    return new ReferenceCountLookupFunctionHandler(SharedStateKey, eval);
                }
                throw new ArgumentException("Unexpected function name '" + validationContext.FunctionName);
            }
        }
    
        public class RefCountedMapUpdateAgent : AggregationAgent
        {
            private readonly ExprEvaluator _evaluator;
    
            public RefCountedMapUpdateAgent(ExprEvaluator evaluator)
            {
                this._evaluator = evaluator;
            }
    
            public void ApplyEnter(EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext, AggregationState aggregationState)
            {
                var value = _evaluator.Evaluate(new EvaluateParams(eventsPerStream, true, exprEvaluatorContext));
                var themap = (RefCountedMapState) aggregationState;
                themap.Enter(value);
            }
    
            public void ApplyLeave(EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext, AggregationState aggregationState)
            {
                var value = _evaluator.Evaluate(new EvaluateParams(eventsPerStream, true, exprEvaluatorContext));
                var themap = (RefCountedMapState) aggregationState;
                themap.Leave(value);
            }
        }
    
        public class ReferenceCountedMapFunctionHandler : PlugInAggregationMultiFunctionHandler
        {
            private readonly AggregationStateKey _sharedStateKey;
    
            public ReferenceCountedMapFunctionHandler(AggregationStateKey sharedStateKey)
            {
                this._sharedStateKey = sharedStateKey;
            }

            public AggregationAccessor Accessor
            {
                get { return null; }
            }

            public EPType ReturnType
            {
                get { return EPTypeHelper.NullValue; }
            }

            public AggregationStateKey AggregationStateUniqueKey
            {
                get { return _sharedStateKey; }
            }

            public PlugInAggregationMultiFunctionStateFactory StateFactory
            {
                get
                {
                    return new ProxyPlugInAggregationMultiFunctionStateFactory()
                    {
                        ProcMakeAggregationState = (stateContext) => { return new RefCountedMapState(); },
                    };
                }
            }

            public AggregationAgent GetAggregationAgent(PlugInAggregationMultiFunctionAgentContext agentContext)
            {
                return new RefCountedMapUpdateAgent(agentContext.ChildNodes[0].ExprEvaluator);
            }
        }
    
        public class ReferenceCountLookupFunctionHandler : PlugInAggregationMultiFunctionHandler
        {
            private readonly AggregationStateKey _sharedStateKey;
            private readonly ExprEvaluator _exprEvaluator;
    
            public ReferenceCountLookupFunctionHandler(AggregationStateKey sharedStateKey, ExprEvaluator exprEvaluator)
            {
                _sharedStateKey = sharedStateKey;
                _exprEvaluator = exprEvaluator;
            }

            public AggregationAccessor Accessor
            {
                get { return new ReferenceCountLookupAccessor(_exprEvaluator); }
            }

            public EPType ReturnType
            {
                get
                {
                    return EPTypeHelper.SingleValue(typeof (int?));
                }
            }

            public AggregationStateKey AggregationStateUniqueKey
            {
                get { return _sharedStateKey; }
            }

            public PlugInAggregationMultiFunctionStateFactory StateFactory
            {
                get { throw new IllegalStateException("Getter does not provide the state"); }
            }

            public AggregationAgent GetAggregationAgent(PlugInAggregationMultiFunctionAgentContext agentContext)
            {
                return null;
            }
        }
    
        public class ReferenceCountLookupAccessor : AggregationAccessor
        {
            private readonly ExprEvaluator _exprEvaluator;
    
            public ReferenceCountLookupAccessor(ExprEvaluator exprEvaluator)
            {
                this._exprEvaluator = exprEvaluator;
            }
    
            public object GetValue(AggregationState state, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
            {
                var mystate = (RefCountedMapState) state;
                var lookupKey = _exprEvaluator.Evaluate(new EvaluateParams(eventsPerStream, isNewData, exprEvaluatorContext));
                return mystate.CountPerReference.Get(lookupKey);
            }
    
            public ICollection<EventBean> GetEnumerableEvents(AggregationState state, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
            {
                return null;
            }
    
            public EventBean GetEnumerableEvent(AggregationState state, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
            {
                return null;
            }
    
            public ICollection<object> GetEnumerableScalar(AggregationState state, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
            {
                return null;
            }
        }
    
        public class RefCountedMapState : AggregationState
        {
            private readonly IDictionary<Object, int?> _countPerReference = new LinkedHashMap<object, int?>();

            public IDictionary<object, int?> CountPerReference
            {
                get { return _countPerReference; }
            }

            public void Enter(object key)
            {
                var count = _countPerReference.Get(key);
                if (count == null) {
                    _countPerReference.Put(key, 1);
                }
                else {
                    _countPerReference.Put(key, count + 1);
                }
            }
    
            public void Leave(object key)
            {
                var count = _countPerReference.Get(key);
                if (count != null) {
                    if (count == 1) {
                        _countPerReference.Remove(key);
                    }
                    else {
                        _countPerReference.Put(key, count - 1);
                    }
                }
            }
    
            public void ApplyEnter(EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext)
            {
                // no need to implement, we mutate using enter and leave instead
                throw new UnsupportedOperationException("Use enter instead");
            }
    
            public void ApplyLeave(EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext)
            {
                // no need to implement, we mutate using enter and leave instead
                throw new UnsupportedOperationException("Use leave instead");
            }
    
            public void Clear()
            {
                _countPerReference.Clear();
            }
        }
    }
    
}
