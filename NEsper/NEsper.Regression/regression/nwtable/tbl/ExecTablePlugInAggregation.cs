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
using com.espertech.esper.plugin;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable.tbl
{
    public class ExecTablePlugInAggregation : RegressionExecution {
    
        public override void Run(EPServiceProvider epService) {
            foreach (var clazz in new Type[]{typeof(SupportBean), typeof(SupportBean_S0), typeof(SupportBean_S1)}) {
                epService.EPAdministrator.Configuration.AddEventType(clazz);
            }
    
            RunAssertionPlugInAggMethod_CSVLast3Strings(epService);
            RunAssertionPlugInAccess_RefCountedMap(epService);
        }
    
        // CSV-building over a limited set of values.
        //
        // Use aggregation method single-value when the aggregation has a natural current value
        // that can be obtained without asking it a question.
        private void RunAssertionPlugInAggMethod_CSVLast3Strings(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddPlugInAggregationFunctionFactory("csvWords", typeof(SimpleWordCSVFactory));
    
            epService.EPAdministrator.CreateEPL("create table varaggPIN (csv CsvWords())");
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("select varaggPIN.csv as c0 from SupportBean_S0").Events += listener.Update;
            epService.EPAdministrator.CreateEPL("into table varaggPIN select CsvWords(TheString) as csv from SupportBean#length(3)");
    
            SendWordAssert(epService, listener, "the", "the");
            SendWordAssert(epService, listener, "fox", "the,fox");
            SendWordAssert(epService, listener, "jumps", "the,fox,jumps");
            SendWordAssert(epService, listener, "over", "fox,jumps,over");
        }
    
        // Word counting using a reference-counting-map (similar: count-min-sketch approximation, this one is more limited)
        //
        // Use aggregation access multi-value when the aggregation must be asked a specific question to return a useful value.
        private void RunAssertionPlugInAccess_RefCountedMap(EPServiceProvider epService) {
    
            var config = new ConfigurationPlugInAggregationMultiFunction(
                    "referenceCountedMap,referenceCountLookup".Split(','), typeof(ReferenceCountedMapMultiValueFactory));
            epService.EPAdministrator.Configuration.AddPlugInAggregationMultiFunction(config);
    
            epService.EPAdministrator
                .CreateEPL("create table varaggRCM (wordCount referenceCountedMap(string))");
            epService.EPAdministrator
                .CreateEPL("into table varaggRCM select referenceCountedMap(TheString) as wordCount from SupportBean#length(3)");
            var listener = new SupportUpdateListener();
            epService.EPAdministrator
                .CreateEPL("select varaggRCM.wordCount.referenceCountLookup(p00) as c0 from SupportBean_S0")
                .Events += listener.Update;
    
            string words = "the,house,is,green";
            SendWordAssert(epService, listener, "the", words, new int?[]{1, null, null, null});
            SendWordAssert(epService, listener, "house", words, new int?[]{1, 1, null, null});
            SendWordAssert(epService, listener, "the", words, new int?[]{2, 1, null, null});
            SendWordAssert(epService, listener, "green", words, new int?[]{1, 1, null, 1});
            SendWordAssert(epService, listener, "is", words, new int?[]{1, null, 1, 1});
        }
    
        private void SendWordAssert(EPServiceProvider epService, SupportUpdateListener listener, string word, string expected) {
            epService.EPRuntime.SendEvent(new SupportBean(word, 0));
            epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            Assert.AreEqual(expected, listener.AssertOneGetNewAndReset().Get("c0"));
        }
    
        private void SendWordAssert(
            EPServiceProvider epService,
            SupportUpdateListener listener, 
            string word, 
            string wordCSV,
            int?[] counts) {
            epService.EPRuntime.SendEvent(new SupportBean(word, 0));
    
            string[] words = wordCSV.Split(',');
            for (int i = 0; i < words.Length; i++) {
                epService.EPRuntime.SendEvent(new SupportBean_S0(0, words[i]));
                var count = listener.AssertOneGetNewAndReset().Get("c0").AsBoxedInt();
                Assert.AreEqual(counts[i], count, "failed for word '" + words[i] + "'");
            }
        }
    
        public class SimpleWordCSVFactory : AggregationFunctionFactory
        {
            public string FunctionName
            {
                get { return null; }
                set { }
            }

            public Type ValueType => typeof(string);

            public void Validate(AggregationValidationContext validationContext) {
            }
    
            public AggregationMethod NewAggregator() {
                return new SimpleWordCSVMethod();
            }
        }
    
        public class SimpleWordCSVMethod : AggregationMethod {
    
            private readonly IDictionary<string, int> _countPerWord = new LinkedHashMap<string, int>();
    
            public void Enter(object value) {
                string word = (string) value;
                if (_countPerWord.TryGetValue(word, out var count))
                    _countPerWord.Put(word, count + 1);
                else
                    _countPerWord.Put(word, 1);
            }
    
            public void Leave(object value) {
                string word = (string) value;
                if (_countPerWord.TryGetValue(word, out var count))
                    if (count == 1)
                        _countPerWord.Remove(word);
                    else
                        _countPerWord.Put(word, count - 1);
                else
                    _countPerWord.Put(word, 1);
            }

            public object Value
            {
                get
                {
                    var writer = new StringWriter();
                    string delimiter = "";
                    foreach (var entry in _countPerWord)
                    {
                        writer.Write(delimiter);
                        delimiter = ",";
                        writer.Write(entry.Key);
                    }

                    return writer.ToString();
                }
            }

            public void Clear() {
                _countPerWord.Clear();
            }
        }
    
        public class ReferenceCountedMapMultiValueFactory : PlugInAggregationMultiFunctionFactory {
            private static readonly AggregationStateKey SHARED_STATE_KEY = new ProxyAggregationStateKey() {
            };
    
            public void AddAggregationFunction(PlugInAggregationMultiFunctionDeclarationContext declarationContext) {
            }
    
            public PlugInAggregationMultiFunctionHandler ValidateGetHandler(PlugInAggregationMultiFunctionValidationContext validationContext)
            {
                switch (validationContext.FunctionName) {
                    case "referenceCountedMap":
                        return new ReferenceCountedMapFunctionHandler(SHARED_STATE_KEY);
                    case "referenceCountLookup":
                        ExprEvaluator eval = validationContext.ParameterExpressions[0].ExprEvaluator;
                        return new ReferenceCountLookupFunctionHandler(SHARED_STATE_KEY, eval);
                }

                throw new ArgumentException("Unexpected function name '" + validationContext.FunctionName);
            }
        }
    
        public class RefCountedMapUpdateAgent : AggregationAgent
        {
            private readonly ExprEvaluator _evaluator;
    
            public RefCountedMapUpdateAgent(ExprEvaluator evaluator) {
                this._evaluator = evaluator;
            }

            public void ApplyEnter(EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext, AggregationState aggregationState) {
                object value = _evaluator.Evaluate(new EvaluateParams(eventsPerStream, true, exprEvaluatorContext));
                RefCountedMapState themap = (RefCountedMapState) aggregationState;
                themap.Enter(value);
            }
    
            public void ApplyLeave(EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext, AggregationState aggregationState) {
                object value = _evaluator.Evaluate(new EvaluateParams(eventsPerStream, true, exprEvaluatorContext));
                RefCountedMapState themap = (RefCountedMapState) aggregationState;
                themap.Leave(value);
            }
        }
    
        public class ReferenceCountedMapFunctionHandler : PlugInAggregationMultiFunctionHandler
        {
            public ReferenceCountedMapFunctionHandler(AggregationStateKey sharedStateKey)
            {
                this.AggregationStateUniqueKey = sharedStateKey;
            }

            public AggregationAccessor Accessor => null;

            public EPType ReturnType => EPTypeHelper.NullValue;

            public AggregationStateKey AggregationStateUniqueKey { get; }

            public PlugInAggregationMultiFunctionStateFactory StateFactory
            {
                get
                {
                    return new ProxyPlugInAggregationMultiFunctionStateFactory()
                    {
                        ProcMakeAggregationState = (stateContext) => new RefCountedMapState()
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
                this._sharedStateKey = sharedStateKey;
                this._exprEvaluator = exprEvaluator;
            }

            public AggregationAccessor Accessor => new ReferenceCountLookupAccessor(_exprEvaluator);

            public EPType ReturnType => EPTypeHelper.SingleValue(typeof(int?));

            public AggregationStateKey AggregationStateUniqueKey => _sharedStateKey;

            public PlugInAggregationMultiFunctionStateFactory StateFactory => throw new IllegalStateException("Getter does not provide the state");

            public AggregationAgent GetAggregationAgent(PlugInAggregationMultiFunctionAgentContext agentContext)
            {
                return null;
            }
        }

        public class ReferenceCountLookupAccessor : AggregationAccessor {
    
            private readonly ExprEvaluator _exprEvaluator;
    
            public ReferenceCountLookupAccessor(ExprEvaluator exprEvaluator) {
                this._exprEvaluator = exprEvaluator;
            }

            public object GetValue(AggregationState state, EvaluateParams evalParams)
            {
                var mystate = (RefCountedMapState) state;
                var lookupKey = _exprEvaluator.Evaluate(evalParams);
                if (mystate.CountPerReference.TryGetValue(lookupKey, out var result)) {
                    return result;
                }

                return null;
            }

            public ICollection<EventBean> GetEnumerableEvents(AggregationState state, EvaluateParams evalParams)
            {
                return null;
            }

            public EventBean GetEnumerableEvent(AggregationState state, EvaluateParams evalParams)
            {
                return null;
            }

            public ICollection<object> GetEnumerableScalar(AggregationState state, EvaluateParams evalParams)
            {
                return null;
            }
        }

        public class RefCountedMapState : AggregationState
        {
            private readonly IDictionary<object, int> _countPerReference = new LinkedHashMap<object, int>();

            public IDictionary<object, int> CountPerReference => _countPerReference;
            public void Enter(object key)
            {
                if (_countPerReference.TryGetValue(key, out var count))
                {
                    _countPerReference.Put(key, count + 1);
                }
                else
                {
                    _countPerReference.Put(key, 1);
                }
            }

            public void Leave(object key)
            {
                if (_countPerReference.TryGetValue(key, out var count))
                {
                    if (count == 1)
                    {
                        _countPerReference.Remove(key);
                    }
                    else
                    {
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
} // end of namespace
