///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.container;
using com.espertech.esper.core.support;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.parse;
using com.espertech.esper.filter;
using com.espertech.esper.pattern;
using com.espertech.esper.supportunit.bean;
using com.espertech.esper.supportunit.epl.parse;
using com.espertech.esper.supportunit.util;
using NUnit.Framework;

namespace com.espertech.esper.epl.spec
{
    [TestFixture]
    public class TestPatternStreamSpecRaw 
    {
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();
        }

        [Test]
        public void TestPatternEquals()
        {
            var text = "select * from pattern [" +
                    "s=" + typeof(SupportBean).FullName + "(IntPrimitive=5) -> " +
                    "t=" + typeof(SupportBean).FullName + "(IntPrimitive=s.IntBoxed)" +
                    "]";
            TryPatternEquals(text);
    
            text = "select * from pattern [" +
                    "s=" + typeof(SupportBean).FullName + "(5=IntPrimitive) -> " +
                    "t=" + typeof(SupportBean).FullName + "(s.IntBoxed=IntPrimitive)" +
                    "]";
            TryPatternEquals(text);
        }
    
        [Test]
        public void TestInvalid()
        {
            var text = "select * from pattern [" +
                    "s=" + typeof(SupportBean).FullName + " -> " +
                    "t=" + typeof(SupportBean).FullName + "(IntPrimitive=s.DoubleBoxed)" +
                    "]";
            TryInvalid(text);
    
            text = "select * from pattern [" +
                    "s=" + typeof(SupportBean).FullName + " -> " +
                    "t=" + typeof(SupportBean).FullName + "(IntPrimitive in (s.DoubleBoxed))" +
                    "]";
            TryInvalid(text);
        }
    
        private void TryInvalid(String text)
        {
            try
            {
                var raw = MakeSpec(text);
                Compile(raw);
                Assert.Fail();
            }
            catch (ExprValidationException)
            {
                // expected
            }
        }
    
        [Test]
        public void TestPatternExpressions()
        {
            var text = "select * from pattern [" +
                    "s=" + typeof(SupportBean).FullName + "(IntPrimitive in (s.IntBoxed + 1, 0), IntBoxed+1=IntPrimitive-1)" +
                    "]";
    
            var raw = MakeSpec(text);
            var spec = Compile(raw);
            Assert.AreEqual(1, spec.TaggedEventTypes.Count);
            Assert.AreEqual(typeof(SupportBean), spec.TaggedEventTypes.Get("s").First.UnderlyingType);
    
            var evalNodeAnalysisResult = EvalNodeUtil.RecursiveAnalyzeChildNodes(spec.EvalFactoryNode);
            var filters = evalNodeAnalysisResult.FilterNodes;
            Assert.AreEqual(1, filters.Count);
    
            // node 0
            var filterNode = filters[0];
            Assert.AreEqual(typeof(SupportBean), filterNode.FilterSpec.FilterForEventType.UnderlyingType);
            Assert.AreEqual(1, filterNode.FilterSpec.Parameters.Length);
            var exprParam = (FilterSpecParamExprNode) filterNode.FilterSpec.Parameters[0][0];
        }
    
        [Test]
        public void TestPatternInSetOfVal()
        {
            var text = "select * from pattern [" +
                    "s=" + typeof(SupportBean).FullName + " -> " +
                           typeof(SupportBean).FullName + "(IntPrimitive in (s.IntBoxed, 0))" +
                    "]";
    
            var raw = MakeSpec(text);
            var spec = Compile(raw);
            Assert.AreEqual(1, spec.TaggedEventTypes.Count);
            Assert.AreEqual(typeof(SupportBean), spec.TaggedEventTypes.Get("s").First.UnderlyingType);
    
            var evalNodeAnalysisResult = EvalNodeUtil.RecursiveAnalyzeChildNodes(spec.EvalFactoryNode);
            var filters = evalNodeAnalysisResult.FilterNodes;
            Assert.AreEqual(2, filters.Count);
    
            // node 0
            var filterNode = filters[0];
            Assert.AreEqual(typeof(SupportBean), filterNode.FilterSpec.FilterForEventType.UnderlyingType);
            Assert.AreEqual(0, filterNode.FilterSpec.Parameters.Length);
    
            // node 1
            filterNode = filters[1];
            Assert.AreEqual(typeof(SupportBean), filterNode.FilterSpec.FilterForEventType.UnderlyingType);
            Assert.AreEqual(1, filterNode.FilterSpec.Parameters.Length);

            var inlist = (FilterSpecParamIn)filterNode.FilterSpec.Parameters[0][0];
            Assert.AreEqual(FilterOperator.IN_LIST_OF_VALUES, inlist.FilterOperator);
            Assert.AreEqual(2, inlist.ListOfValues.Count);

            // in-value 1
            var prop = (FilterForEvalEventPropMayCoerce) inlist.ListOfValues[0];
            Assert.AreEqual("s", prop.ResultEventAsName);
            Assert.AreEqual("IntBoxed", prop.ResultEventProperty);

            // in-value 1
            var constant = (FilterForEvalConstantAnyType) inlist.ListOfValues[1];
            Assert.AreEqual(0, constant.Constant);
        }
    
        [Test]
        public void TestRange()
        {
            var text = "select * from pattern [" +
                    "s=" + typeof(SupportBean).FullName + " -> " +
                           typeof(SupportBean).FullName + "(IntPrimitive between s.IntBoxed and 100)" +
                    "]";
    
            var raw = MakeSpec(text);
            var spec = Compile(raw);
            Assert.AreEqual(1, spec.TaggedEventTypes.Count);
            Assert.AreEqual(typeof(SupportBean), spec.TaggedEventTypes.Get("s").First.UnderlyingType);
    
            var evalNodeAnalysisResult = EvalNodeUtil.RecursiveAnalyzeChildNodes(spec.EvalFactoryNode);
            var filters = evalNodeAnalysisResult.FilterNodes;
            Assert.AreEqual(2, filters.Count);
    
            // node 0
            var filterNode = filters[0];
            Assert.AreEqual(typeof(SupportBean), filterNode.FilterSpec.FilterForEventType.UnderlyingType);
            Assert.AreEqual(0, filterNode.FilterSpec.Parameters.Length);
    
            // node 1
            filterNode = filters[1];
            Assert.AreEqual(typeof(SupportBean), filterNode.FilterSpec.FilterForEventType.UnderlyingType);
            Assert.AreEqual(1, filterNode.FilterSpec.Parameters.Length);

            var range = (FilterSpecParamRange)filterNode.FilterSpec.Parameters[0][0];
            Assert.AreEqual(FilterOperator.RANGE_CLOSED, range.FilterOperator);

            // min-value
            var prop = (FilterForEvalEventPropDouble) range.Min;
            Assert.AreEqual("s", prop.ResultEventAsName);
            Assert.AreEqual("IntBoxed", prop.ResultEventProperty);

            // max-value
            var constant = (FilterForEvalConstantDouble) range.Max;
            Assert.AreEqual(100d, constant.DoubleValue);
        }
    
        private void TryPatternEquals(String text)
        {
            var raw = MakeSpec(text);
            var spec = Compile(raw);
            Assert.AreEqual(2, spec.TaggedEventTypes.Count);
            Assert.AreEqual(typeof(SupportBean), spec.TaggedEventTypes.Get("s").First.UnderlyingType);
            Assert.AreEqual(typeof(SupportBean), spec.TaggedEventTypes.Get("t").First.UnderlyingType);
    
            var evalNodeAnalysisResult = EvalNodeUtil.RecursiveAnalyzeChildNodes(spec.EvalFactoryNode);
            var filters = evalNodeAnalysisResult.FilterNodes;
            Assert.AreEqual(2, filters.Count);
    
            // node 0
            var filterNode = filters[0];
            Assert.AreEqual(typeof(SupportBean), filterNode.FilterSpec.FilterForEventType.UnderlyingType);
            Assert.AreEqual(1, filterNode.FilterSpec.Parameters.Length);

            var constant = (FilterSpecParamConstant)filterNode.FilterSpec.Parameters[0][0];
            Assert.AreEqual(FilterOperator.EQUAL, constant.FilterOperator);
            Assert.AreEqual("IntPrimitive", constant.Lookupable.Expression);
            Assert.AreEqual(5, constant.FilterConstant);
    
            // node 1
            filterNode = filters[1];
            Assert.AreEqual(typeof(SupportBean), filterNode.FilterSpec.FilterForEventType.UnderlyingType);
            Assert.AreEqual(1, filterNode.FilterSpec.Parameters.Length);

            var eventprop = (FilterSpecParamEventProp)filterNode.FilterSpec.Parameters[0][0];
            Assert.AreEqual(FilterOperator.EQUAL, constant.FilterOperator);
            Assert.AreEqual("IntPrimitive", constant.Lookupable.Expression);
            Assert.AreEqual("s", eventprop.ResultEventAsName);
            Assert.AreEqual("IntBoxed", eventprop.ResultEventProperty);
        }
    
        private PatternStreamSpecCompiled Compile(PatternStreamSpecRaw raw)
        {
            return raw.Compile(SupportStatementContextFactory.MakeContext(_container), new HashSet<String>(), false, Collections.GetEmptyList<int>(), false, false, false, null)
                    as PatternStreamSpecCompiled;
        }
    
        private static PatternStreamSpecRaw MakeSpec(String expression)
        {
            EPLTreeWalkerListener walker = SupportParserHelper.ParseAndWalkEPL(expression);
            return (PatternStreamSpecRaw)walker.StatementSpec.StreamSpecs[0];
        }
    }
}
