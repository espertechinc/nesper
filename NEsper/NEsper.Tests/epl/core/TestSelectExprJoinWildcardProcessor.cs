///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.container;
using com.espertech.esper.core.service;
using com.espertech.esper.core.support;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.events;
using com.espertech.esper.supportunit.bean;
using com.espertech.esper.supportunit.epl;
using com.espertech.esper.supportunit.events;
using com.espertech.esper.supportunit.util;
using NUnit.Framework;

namespace com.espertech.esper.epl.core
{
    [TestFixture]
    public class TestSelectExprJoinWildcardProcessor 
    {
        private SelectExprProcessor _processor;
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();

            var selectExprEventTypeRegistry = new SelectExprEventTypeRegistry(
                "abc", new StatementEventTypeRefImpl(_container.RWLockManager()));
            var supportTypes = new SupportStreamTypeSvc3Stream();

            _processor = SelectExprJoinWildcardProcessorFactory.Create(
                Collections.GetEmptyList<int>(), 1, "stmtname", 
                supportTypes.StreamNames, 
                supportTypes.EventTypes,
                _container.Resolve<EventAdapterService>(), null, 
                selectExprEventTypeRegistry, null, null, 
                new Configuration(_container), 
                new TableServiceImpl(_container),
                "default");
        }
    
        [Test]
        public void TestProcess()
        {
            EventBean[] testEvents = SupportStreamTypeSvc3Stream.SampleEvents;
    
            EventBean result = _processor.Process(testEvents, true, false, null);
            Assert.AreEqual(testEvents[0].Underlying, result.Get("s0"));
            Assert.AreEqual(testEvents[1].Underlying, result.Get("s1"));
    
            // Test null events, such as in an outer join
            testEvents[1] = null;
            result = _processor.Process(testEvents, true, false, null);
            Assert.AreEqual(testEvents[0].Underlying, result.Get("s0"));
            Assert.IsNull(result.Get("s1"));
        }
    
        [Test]
        public void TestType()
        {
            Assert.AreEqual(typeof(SupportBean), _processor.ResultEventType.GetPropertyType("s0"));
            Assert.AreEqual(typeof(SupportBean), _processor.ResultEventType.GetPropertyType("s1"));
        }
    }
}
