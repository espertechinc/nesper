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
using com.espertech.esper.compat.container;
using com.espertech.esper.core.context.mgr;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.core.support;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.spec;
using com.espertech.esper.supportunit.epl;
using com.espertech.esper.supportunit.util;
using com.espertech.esper.supportunit.view;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.epl.core
{
    [TestFixture]
    public class TestResultSetProcessorFactory 
    {
        private StreamTypeService _typeService1Stream;
        private StreamTypeService _typeService3Stream;
        private IList<ExprNode> _groupByList;
        private IList<OrderByItem> _orderByList;
        private AgentInstanceContext _agentInstanceContext;
        private StatementContext _stmtContext;
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();
            _typeService1Stream = new SupportStreamTypeSvc1Stream();
            _typeService3Stream = new SupportStreamTypeSvc3Stream();
            _groupByList = new List<ExprNode>();
            _orderByList = new List<OrderByItem>();
            _agentInstanceContext = SupportStatementContextFactory.MakeAgentInstanceContext(_container);
            _stmtContext = _agentInstanceContext.StatementContext;
        }
    
        [Test]
        public void TestGetProcessorNoProcessorRequired()
        {
            // single stream, empty group-by and wildcard select, no having clause, no need for any output processing
            var wildcardSelect = new SelectClauseElementCompiled[] {new SelectClauseElementWildcard()};
            var spec = MakeSpec(new SelectClauseSpecCompiled(wildcardSelect, false), null, _groupByList, null, null, _orderByList);
            var processor = ResultSetProcessorFactoryFactory.GetProcessorPrototype(
                spec, _stmtContext, _typeService1Stream,
                null, new bool[0], true, ContextPropertyRegistryImpl.EMPTY_REGISTRY, 
                null, new Configuration(_container), null, false, false);
            Assert.IsTrue(processor.ResultSetProcessorFactory is ResultSetProcessorHandThroughFactory);
        }
    
        [Test]
        public void TestGetProcessorSimpleSelect()
        {
            // empty group-by and no event properties aggregated in select clause (wildcard), no having clause
            var wildcardSelect = new SelectClauseElementCompiled[] {new SelectClauseElementWildcard()};
            var spec = MakeSpec(new SelectClauseSpecCompiled(wildcardSelect, false), null, _groupByList, null, null, _orderByList);
            var processor = ResultSetProcessorFactoryFactory.GetProcessorPrototype(
                spec, _stmtContext, _typeService3Stream,  
                null, new bool[0], true, ContextPropertyRegistryImpl.EMPTY_REGISTRY, 
                null, new Configuration(_container), null, false, false);
            Assert.IsTrue(processor.ResultSetProcessorFactory is ResultSetProcessorHandThroughFactory);
    
            // empty group-by with select clause elements
            var selectList = SupportSelectExprFactory.MakeNoAggregateSelectListUnnamed();
            spec = MakeSpec(new SelectClauseSpecCompiled(selectList, false), null, _groupByList, null, null, _orderByList);
            processor = ResultSetProcessorFactoryFactory.GetProcessorPrototype(
                spec, _stmtContext, _typeService1Stream, 
                null, new bool[0], true, ContextPropertyRegistryImpl.EMPTY_REGISTRY, 
                null, new Configuration(_container), null, false, false);
            Assert.IsTrue(processor.ResultSetProcessorFactory is ResultSetProcessorHandThroughFactory);
    
            // non-empty group-by and wildcard select, group by ignored
            _groupByList.Add(SupportExprNodeFactory.MakeIdentNode("DoubleBoxed", "s0"));
            spec = MakeSpec(new SelectClauseSpecCompiled(wildcardSelect, false), null, _groupByList, null, null, _orderByList);
            processor = ResultSetProcessorFactoryFactory.GetProcessorPrototype(
                spec, _stmtContext, _typeService1Stream, 
                null, new bool[0], true, ContextPropertyRegistryImpl.EMPTY_REGISTRY, 
                null, new Configuration(_container), null, false, false);
            Assert.IsTrue(processor.ResultSetProcessorFactory is ResultSetProcessorSimpleFactory);
        }
    
        [Test]
        public void TestGetProcessorAggregatingAll()
        {
            // empty group-by but aggragating event properties in select clause (output per event), no having clause
            // and one or more properties in the select clause is not aggregated
            var selectList = SupportSelectExprFactory.MakeAggregateMixed();
            var spec = MakeSpec(new SelectClauseSpecCompiled(selectList, false), null, _groupByList, null, null, _orderByList);
            var processor = ResultSetProcessorFactoryFactory.GetProcessorPrototype(
                spec, _stmtContext, _typeService1Stream, 
                null, new bool[0], true, ContextPropertyRegistryImpl.EMPTY_REGISTRY, 
                null, new Configuration(_container), null, false, false);
            Assert.IsTrue(processor.ResultSetProcessorFactory is ResultSetProcessorAggregateAllFactory);
    
            // test a case where a property is both aggregated and non-aggregated: select volume, Sum(volume)
            selectList = SupportSelectExprFactory.MakeAggregatePlusNoAggregate();
            spec = MakeSpec(new SelectClauseSpecCompiled(selectList, false), null, _groupByList, null, null, _orderByList);
            processor = ResultSetProcessorFactoryFactory.GetProcessorPrototype(
                spec, _stmtContext, _typeService1Stream, 
                null, new bool[0], true, ContextPropertyRegistryImpl.EMPTY_REGISTRY, 
                null, new Configuration(_container), null, false, false);
            Assert.IsTrue(processor.ResultSetProcessorFactory is ResultSetProcessorAggregateAllFactory);
        }
    
        [Test]
        public void TestGetProcessorRowForAll()
        {
            // empty group-by but aggragating event properties in select clause (output per event), no having clause
            // and all properties in the select clause are aggregated
            var selectList = SupportSelectExprFactory.MakeAggregateSelectListWithProps();
            var spec = MakeSpec(new SelectClauseSpecCompiled(selectList, false), null, _groupByList, null, null, _orderByList);
            var processor = ResultSetProcessorFactoryFactory.GetProcessorPrototype(
                spec, _stmtContext, _typeService1Stream, 
                null, new bool[0], true, ContextPropertyRegistryImpl.EMPTY_REGISTRY, 
                null, new Configuration(_container), null, false, false);
            Assert.IsTrue(processor.ResultSetProcessorFactory is ResultSetProcessorRowForAllFactory);
        }
    
        [Test]
        public void TestGetProcessorRowPerGroup()
        {
            // with group-by and the non-aggregated event properties are all listed in the group by (output per group)
            // no having clause
            var selectList = SupportSelectExprFactory.MakeAggregateMixed();
            _groupByList.Add(SupportExprNodeFactory.MakeIdentNode("DoubleBoxed", "s0"));
            var spec = MakeSpec(new SelectClauseSpecCompiled(selectList, false), null, _groupByList, null, null, _orderByList);
            var processor = ResultSetProcessorFactoryFactory.GetProcessorPrototype(
                spec, _stmtContext, _typeService1Stream, 
                null, new bool[0], true, ContextPropertyRegistryImpl.EMPTY_REGISTRY, 
                null, new Configuration(_container), null, false, false);
            Assert.IsTrue(processor.ResultSetProcessorFactory is ResultSetProcessorRowPerGroupFactory);
        }
    
        [Test]
        public void TestGetProcessorAggregatingGrouped()
        {
            // with group-by but either
            //      wildcard
            //      or one or more non-aggregated event properties are not in the group by (output per event)
            var selectList = SupportSelectExprFactory.MakeAggregateMixed();
            var identNode = SupportExprNodeFactory.MakeIdentNode("TheString", "s0");
            selectList = (SelectClauseElementCompiled[]) CollectionUtil.ArrayExpandAddSingle(selectList, new SelectClauseExprCompiledSpec(identNode, null, null, false));
    
            _groupByList.Add(SupportExprNodeFactory.MakeIdentNode("DoubleBoxed", "s0"));
            var spec = MakeSpec(new SelectClauseSpecCompiled(selectList, false), null, _groupByList, null, null, _orderByList);
            var processor = ResultSetProcessorFactoryFactory.GetProcessorPrototype(
                spec, _stmtContext, _typeService1Stream,
                null, new bool[0], true, ContextPropertyRegistryImpl.EMPTY_REGISTRY, 
                null, new Configuration(_container), null, false, false);
            Assert.IsTrue(processor.ResultSetProcessorFactory is ResultSetProcessorAggregateGroupedFactory);
        }
    
        [Test]
        public void TestGetProcessorInvalid()
        {
            var spec = MakeSpec(new SelectClauseSpecCompiled(SupportSelectExprFactory.MakeInvalidSelectList(), false), null, _groupByList, null, null, _orderByList);
            // invalid select clause
            try
            {
                ResultSetProcessorFactoryFactory.GetProcessorPrototype(
                    spec, _stmtContext, _typeService3Stream, 
                    null, new bool[0],  true, ContextPropertyRegistryImpl.EMPTY_REGISTRY, 
                    null, new Configuration(_container), null, false, false);
                Assert.Fail();
            }
            catch (ExprValidationException)
            {
                // expected
            }
    
            // invalid group-by
            _groupByList.Add(new ExprIdentNodeImpl("xxxx", "s0"));
            try
            {
                spec = MakeSpec(new SelectClauseSpecCompiled(SupportSelectExprFactory.MakeNoAggregateSelectListUnnamed(), false), null, _groupByList, null, null, _orderByList);
                ResultSetProcessorFactoryFactory.GetProcessorPrototype(
                    spec, _stmtContext, _typeService3Stream, 
                    null, new bool[0], true, ContextPropertyRegistryImpl.EMPTY_REGISTRY, 
                    null, new Configuration(_container), null, false, false);
                Assert.Fail();
            }
            catch (ExprValidationException)
            {
                // expected
            }
    
            // Test group by having properties that are aggregated in select clause, should fail
            _groupByList.Clear();
            _groupByList.Add(SupportExprNodeFactory.MakeSumAggregateNode());
    
            var selectList = new SelectClauseElementCompiled[] {
                    new SelectClauseExprCompiledSpec(SupportExprNodeFactory.MakeSumAggregateNode(), null, null, false)
            };
    
            try
            {
                spec = MakeSpec(new SelectClauseSpecCompiled(selectList, false), null, _groupByList, null, null, _orderByList);
                ResultSetProcessorFactoryFactory.GetProcessorPrototype(
                    spec, _stmtContext, _typeService3Stream, 
                    null, new bool[0], true, ContextPropertyRegistryImpl.EMPTY_REGISTRY, 
                    null, new Configuration(_container), null, false, false);
                Assert.Fail();
            }
            catch (ExprValidationException)
            {
                // expected
            }
        }
    
        private StatementSpecCompiled MakeSpec(SelectClauseSpecCompiled selectClauseSpec,
                                                      InsertIntoDesc insertIntoDesc,
                                                   	  IList<ExprNode> groupByNodes,
                                                   	  ExprNode optionalHavingNode,
                                                   	  OutputLimitSpec outputLimitSpec,
                                                   	  IList<OrderByItem> orderByList)
        {
            return new StatementSpecCompiled(null, // on trigger
                    null,  // create win
                    null,  // create index
                    null,  // create var
                    null,  // create agg var
                    null,  // create schema
                    insertIntoDesc,
                    SelectClauseStreamSelectorEnum.ISTREAM_ONLY,
                    selectClauseSpec,
                    new StreamSpecCompiled[0],  // stream specs
                    null,  // outer join
                    null,
                    optionalHavingNode,
                    outputLimitSpec,
                    OrderByItem.ToArray(orderByList),
                    null,
                    null,
                    null,
                    null,
                    null,
                    CollectionUtil.EMPTY_STRING_ARRAY,
                    new Attribute[0],
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    new GroupByClauseExpressions(ExprNodeUtility.ToArray(groupByNodes)),
                    null,
                    null
                    );
        }
    }
}
