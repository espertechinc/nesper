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
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.ops;
using com.espertech.esper.epl.parse;
using com.espertech.esper.filter;
using com.espertech.esper.supportunit.bean;
using com.espertech.esper.supportunit.epl.parse;
using com.espertech.esper.supportunit.util;
using com.espertech.esper.supportunit.view;

using NUnit.Framework;

namespace com.espertech.esper.epl.spec
{
    [TestFixture]
    public class TestFilterStreamSpecRaw 
    {
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();
        }

        [Test]
        public void TestNoExpr()
        {
            var raw = MakeSpec("select * from " + typeof(SupportBean).FullName);
            var spec = Compile(raw);
            Assert.AreEqual(typeof(SupportBean), spec.FilterForEventType.UnderlyingType);
            Assert.AreEqual(0, spec.Parameters.Length);
        }
    
        [Test]
        public void TestMultipleExpr()
        {
            var raw = MakeSpec("select * from " + typeof(SupportBean).FullName +
                    "(IntPrimitive-1>2 and IntBoxed-5>3)");
            var spec = Compile(raw);
            Assert.AreEqual(typeof(SupportBean), spec.FilterForEventType.UnderlyingType);
            Assert.AreEqual(1, spec.Parameters.Length);
            // expecting unoptimized expressions to condense to a single bool expression, more efficient this way

            var exprNode = (FilterSpecParamExprNode) spec.Parameters[0][0];
            Assert.AreEqual(FilterSpecCompiler.PROPERTY_NAME_BOOLEAN_EXPRESSION, exprNode.Lookupable.Expression);
            Assert.AreEqual(FilterOperator.BOOLEAN_EXPRESSION, exprNode.FilterOperator);
            Assert.IsTrue(exprNode.ExprNode is ExprAndNode);
        }
    
        [Test]
        public void TestInvalid()
        {
            TryInvalid("select * from " + typeof(SupportBean).FullName + "(IntPrimitive=5L)");
            TryInvalid("select * from " + typeof(SupportBean).FullName + "(5d = ByteBoxed)");
            TryInvalid("select * from " + typeof(SupportBean).FullName + "(5d > LongBoxed)");
            TryInvalid("select * from " + typeof(SupportBean).FullName + "(LongBoxed in (5d, 1.1d))");
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
        public void TestEquals()
        {
            var raw = MakeSpec("select * from " + typeof(SupportBean).FullName + "(IntPrimitive=5)");
            var spec = Compile(raw);
            Assert.AreEqual(1, spec.Parameters.Length);
            Assert.AreEqual("IntPrimitive", spec.Parameters[0][0].Lookupable.Expression);
            Assert.AreEqual(FilterOperator.EQUAL, spec.Parameters[0][0].FilterOperator);
            Assert.AreEqual(5, GetConstant(spec.Parameters[0][0]));
        }
    
        [Test]
        public void TestEqualsAndLess()
        {
            var raw = MakeSpec("select * from " + typeof(SupportBean).FullName + "(TheString='a' and IntPrimitive<9)");
            var spec = Compile(raw);
            Assert.AreEqual(2, spec.Parameters[0].Length);
            var parameters = MapParameters(spec.Parameters[0]);
    
            Assert.AreEqual(FilterOperator.EQUAL, parameters.Get("TheString").FilterOperator);
            Assert.AreEqual("a", GetConstant(parameters.Get("TheString")));
    
            Assert.AreEqual(FilterOperator.LESS, parameters.Get("IntPrimitive").FilterOperator);
            Assert.AreEqual(9, GetConstant(parameters.Get("IntPrimitive")));
        }
    
        private IDictionary<String, FilterSpecParam> MapParameters(FilterSpecParam[] parameters)
        {
            IDictionary<String, FilterSpecParam> map = new Dictionary<String, FilterSpecParam>();
            foreach (var param in parameters)
            {
                map.Put(param.Lookupable.Expression, param);
            }
            return map;
        }
    
        [Test]
        public void TestCommaAndCompare()
        {
            var raw = MakeSpec("select * from " + typeof(SupportBean).FullName +
                    "(DoubleBoxed>1.11, DoublePrimitive>=9.11 and IntPrimitive<=9, TheString || 'a' = 'sa')");
            var spec = Compile(raw);
            Assert.AreEqual(4, spec.Parameters[0].Length);
            var parameters = MapParameters(spec.Parameters[0]);
    
            Assert.AreEqual(FilterOperator.GREATER, parameters.Get("DoubleBoxed").FilterOperator);
            Assert.AreEqual(1.11, GetConstant(parameters.Get("DoubleBoxed")));
    
            Assert.AreEqual(FilterOperator.GREATER_OR_EQUAL, parameters.Get("DoublePrimitive").FilterOperator);
            Assert.AreEqual(9.11, GetConstant(parameters.Get("DoublePrimitive")));
    
            Assert.AreEqual(FilterOperator.LESS_OR_EQUAL, parameters.Get("IntPrimitive").FilterOperator);
            Assert.AreEqual(9, GetConstant(parameters.Get("IntPrimitive")));
    
            Assert.AreEqual(FilterOperator.BOOLEAN_EXPRESSION, parameters.Get(FilterSpecCompiler.PROPERTY_NAME_BOOLEAN_EXPRESSION).FilterOperator);
            Assert.IsTrue(parameters.Get(FilterSpecCompiler.PROPERTY_NAME_BOOLEAN_EXPRESSION) is FilterSpecParamExprNode);
        }
    
        [Test]
        public void TestNestedAnd()
        {
            var raw = MakeSpec("select * from " + typeof(SupportBean).FullName +
                    "((DoubleBoxed=1 and DoublePrimitive=2) and (IntPrimitive=3 and (TheString like '%_a' and TheString = 'a')))");
            var spec = Compile(raw);
            Assert.AreEqual(5, spec.Parameters[0].Length);
            var parameters = MapParameters(spec.Parameters[0]);
    
            Assert.AreEqual(FilterOperator.EQUAL, parameters.Get("DoubleBoxed").FilterOperator);
            Assert.AreEqual(1.0, GetConstant(parameters.Get("DoubleBoxed")));
    
            Assert.AreEqual(FilterOperator.EQUAL, parameters.Get("DoublePrimitive").FilterOperator);
            Assert.AreEqual(2.0, GetConstant(parameters.Get("DoublePrimitive")));
    
            Assert.AreEqual(FilterOperator.EQUAL, parameters.Get("IntPrimitive").FilterOperator);
            Assert.AreEqual(3, GetConstant(parameters.Get("IntPrimitive")));
    
            Assert.AreEqual(FilterOperator.EQUAL, parameters.Get("TheString").FilterOperator);
            Assert.AreEqual("a", GetConstant(parameters.Get("TheString")));
    
            Assert.AreEqual(FilterOperator.BOOLEAN_EXPRESSION, parameters.Get(FilterSpecCompiler.PROPERTY_NAME_BOOLEAN_EXPRESSION).FilterOperator);
            Assert.IsTrue(parameters.Get(FilterSpecCompiler.PROPERTY_NAME_BOOLEAN_EXPRESSION) is FilterSpecParamExprNode);
        }
    
        [Test]
        public void TestIn()
        {
            var raw = MakeSpec("select * from " + typeof(SupportBean).FullName + "(DoubleBoxed in (1, 2, 3))");
            var spec = Compile(raw);
            Assert.AreEqual(1, spec.Parameters[0].Length);

            Assert.AreEqual("DoubleBoxed", spec.Parameters[0][0].Lookupable.Expression);
            Assert.AreEqual(FilterOperator.IN_LIST_OF_VALUES, spec.Parameters[0][0].FilterOperator);
            var inParam = (FilterSpecParamIn)spec.Parameters[0][0];
            Assert.AreEqual(3, inParam.ListOfValues.Count);
            Assert.AreEqual(1.0, GetConstant(inParam.ListOfValues[0]));
            Assert.AreEqual(2.0, GetConstant(inParam.ListOfValues[1]));
            Assert.AreEqual(3.0, GetConstant(inParam.ListOfValues[2]));
        }
    
        [Test]
        public void TestNotIn()
        {
            var raw = MakeSpec("select * from " + typeof(SupportBean).FullName + "(TheString not in (\"a\"))");
            var spec = Compile(raw);
            Assert.AreEqual(1, spec.Parameters[0].Length);

            Assert.AreEqual("TheString", spec.Parameters[0][0].Lookupable.Expression);
            Assert.AreEqual(FilterOperator.NOT_IN_LIST_OF_VALUES, spec.Parameters[0][0].FilterOperator);
            var inParam = (FilterSpecParamIn)spec.Parameters[0][0];
            Assert.AreEqual(1, inParam.ListOfValues.Count);
            Assert.AreEqual("a", GetConstant(inParam.ListOfValues[0]));
        }
    
        [Test]
        public void TestRanges()
        {
            var raw = MakeSpec("select * from " + typeof(SupportBean).FullName +
                    "(IntBoxed in [1:5] and DoubleBoxed in (2:6) and FloatBoxed in (3:7] and ByteBoxed in [0:1))");
            var spec = Compile(raw);
            Assert.AreEqual(4, spec.Parameters[0].Length);
            var parameters = MapParameters(spec.Parameters[0]);
    
            Assert.AreEqual(FilterOperator.RANGE_CLOSED, parameters.Get("IntBoxed").FilterOperator);
            var rangeParam = (FilterSpecParamRange) parameters.Get("IntBoxed");
            Assert.AreEqual(1.0, GetConstant(rangeParam.Min));
            Assert.AreEqual(5.0, GetConstant(rangeParam.Max));
    
            Assert.AreEqual(FilterOperator.RANGE_OPEN, parameters.Get("DoubleBoxed").FilterOperator);
            rangeParam = (FilterSpecParamRange) parameters.Get("DoubleBoxed");
            Assert.AreEqual(2.0, GetConstant(rangeParam.Min));
            Assert.AreEqual(6.0, GetConstant(rangeParam.Max));
    
            Assert.AreEqual(FilterOperator.RANGE_HALF_CLOSED, parameters.Get("FloatBoxed").FilterOperator);
            rangeParam = (FilterSpecParamRange) parameters.Get("FloatBoxed");
            Assert.AreEqual(3.0, GetConstant(rangeParam.Min));
            Assert.AreEqual(7.0, GetConstant(rangeParam.Max));
    
            Assert.AreEqual(FilterOperator.RANGE_HALF_OPEN, parameters.Get("ByteBoxed").FilterOperator);
            rangeParam = (FilterSpecParamRange) parameters.Get("ByteBoxed");
            Assert.AreEqual(0.0, GetConstant(rangeParam.Min));
            Assert.AreEqual(1.0, GetConstant(rangeParam.Max));
        }
    
        [Test]
        public void TestRangesNot()
        {
            var raw = MakeSpec("select * from " + typeof(SupportBean).FullName +
                    "(IntBoxed not in [1:5] and DoubleBoxed not in (2:6) and FloatBoxed not in (3:7] and ByteBoxed not in [0:1))");
            var spec = Compile(raw);
            Assert.AreEqual(4, spec.Parameters[0].Length);
            var parameters = MapParameters(spec.Parameters[0]);
    
            Assert.AreEqual(FilterOperator.NOT_RANGE_CLOSED, parameters.Get("IntBoxed").FilterOperator);
            var rangeParam = (FilterSpecParamRange) parameters.Get("IntBoxed");
            Assert.AreEqual(1.0, GetConstant(rangeParam.Min));
            Assert.AreEqual(5.0, GetConstant(rangeParam.Max));
    
            Assert.AreEqual(FilterOperator.NOT_RANGE_OPEN, parameters.Get("DoubleBoxed").FilterOperator);
            rangeParam = (FilterSpecParamRange) parameters.Get("DoubleBoxed");
            Assert.AreEqual(2.0, GetConstant(rangeParam.Min));
            Assert.AreEqual(6.0, GetConstant(rangeParam.Max));
    
            Assert.AreEqual(FilterOperator.NOT_RANGE_HALF_CLOSED, parameters.Get("FloatBoxed").FilterOperator);
            rangeParam = (FilterSpecParamRange) parameters.Get("FloatBoxed");
            Assert.AreEqual(3.0, GetConstant(rangeParam.Min));
            Assert.AreEqual(7.0, GetConstant(rangeParam.Max));
    
            Assert.AreEqual(FilterOperator.NOT_RANGE_HALF_OPEN, parameters.Get("ByteBoxed").FilterOperator);
            rangeParam = (FilterSpecParamRange) parameters.Get("ByteBoxed");
            Assert.AreEqual(0.0, GetConstant(rangeParam.Min));
            Assert.AreEqual(1.0, GetConstant(rangeParam.Max));
        }
    
        private double GetConstant(FilterSpecParamFilterForEval param)
        {
            var constant = (FilterForEvalConstantDouble) param;
            return constant.DoubleValue;
        }
    
        private Object GetConstant(FilterSpecParamInValue param)
        {
            var constant = (FilterForEvalConstantAnyType)param;
            return constant.Constant;
        }
    
        private Object GetConstant(FilterSpecParam param)
        {
            var constant = (FilterSpecParamConstant) param;
            return constant.FilterConstant;
        }
    
        private FilterSpecCompiled Compile(FilterStreamSpecRaw raw)
        {
            var compiled = (FilterStreamSpecCompiled) raw.Compile(SupportStatementContextFactory.MakeContext(_container), new HashSet<String>(), false, Collections.GetEmptyList<int>(), false, false, false, null);
            return compiled.FilterSpec;
        }
    
        private static FilterStreamSpecRaw MakeSpec(String expression)
        {
            EPLTreeWalkerListener walker = SupportParserHelper.ParseAndWalkEPL(expression);
            return (FilterStreamSpecRaw)walker.StatementSpec.StreamSpecs[0];
        }
    }
}
