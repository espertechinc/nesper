///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;
using com.espertech.esper.compat.container;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.core.service.multimatch;
using com.espertech.esper.filter;
using com.espertech.esper.supportunit.bean;
using com.espertech.esper.supportunit.events;
using com.espertech.esper.supportunit.filter;
using com.espertech.esper.supportunit.util;
using NUnit.Framework;

namespace com.espertech.esper.view.stream
{
    [TestFixture]
    public class TestStreamFactorySvcImpl 
    {
        private StreamFactoryService _streamFactoryService;
        private SupportFilterServiceImpl _supportFilterService;
    
        private FilterSpecCompiled[] _filterSpecs;
        private EventStream[] _streams;
        private EPStatementHandle _handle;
        private EPStatementAgentInstanceHandle _agentHandle;
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();
            _handle = new EPStatementHandle(1, "name", "text", StatementType.SELECT, "text", false, null, 1, false, false, new MultiMatchHandlerFactoryImpl().GetDefaultHandler());
            _agentHandle = new EPStatementAgentInstanceHandle(_handle, _container.RWLockManager().CreateDefaultLock(), -1, null, null);

            _supportFilterService = new SupportFilterServiceImpl();
            _streamFactoryService = new StreamFactorySvcImpl("default", true);
            var eventType = SupportEventTypeFactory.CreateBeanType(typeof(SupportBean));
    
            _filterSpecs = new FilterSpecCompiled[3];
            _filterSpecs[0] = SupportFilterSpecBuilder.Build(eventType, new Object[] { "string", FilterOperator.EQUAL, "a" });
            _filterSpecs[1] = SupportFilterSpecBuilder.Build(eventType, new Object[] { "string", FilterOperator.EQUAL, "a" });
            _filterSpecs[2] = SupportFilterSpecBuilder.Build(eventType, new Object[] { "string", FilterOperator.EQUAL, "b" });
        }
    
        [Test]
        public void TestInvalidJoin()
        {
            _streams = new EventStream[3];
            _streams[0] = _streamFactoryService.CreateStream(1, _filterSpecs[0], _supportFilterService, _agentHandle, true, null, false, false, null, false, 0, false).First;
    
            try
            {
                // try to reuse the same filter spec object, should fail
                _streamFactoryService.CreateStream(1, _filterSpecs[0], _supportFilterService, _agentHandle, true, null, false, false, null, false, 0, false);
                Assert.Fail();
            }
            catch (IllegalStateException)
            {
                // expected
            }
        }
    
        [Test]
        public void TestCreateJoin()
        {
            _streams = new EventStream[3];
            _streams[0] = _streamFactoryService.CreateStream(1, _filterSpecs[0], _supportFilterService, _agentHandle, true, null, false, false, null, false, 0, false).First;
            _streams[1] = _streamFactoryService.CreateStream(1, _filterSpecs[1], _supportFilterService, _agentHandle, true, null, false, false, null, false, 0, false).First;
            _streams[2] = _streamFactoryService.CreateStream(1, _filterSpecs[2], _supportFilterService, _agentHandle, true, null, false, false, null, false, 0, false).First;
    
            // Streams are reused
            Assert.AreNotSame(_streams[0], _streams[1]);
            Assert.AreNotSame(_streams[0], _streams[2]);
            Assert.AreNotSame(_streams[1], _streams[2]);
    
            // Type is ok
            Assert.AreEqual(typeof(SupportBean), _streams[0].EventType.UnderlyingType);
    
            // 2 filters are active now
            Assert.AreEqual(3, _supportFilterService.Added.Count);
        }
    
        [Test]
        public void TestDropJoin()
        {
            _streams = new EventStream[3];
            _streams[0] = _streamFactoryService.CreateStream(1, _filterSpecs[0], _supportFilterService, _agentHandle, true, null, false, false, null, false, 0, false).First;
            _streams[1] = _streamFactoryService.CreateStream(2, _filterSpecs[1], _supportFilterService, _agentHandle, true, null, false, false, null, false, 0, false).First;
            _streams[2] = _streamFactoryService.CreateStream(3, _filterSpecs[2], _supportFilterService, _agentHandle, true, null, false, false, null, false, 0, false).First;
    
            _streamFactoryService.DropStream(_filterSpecs[0], _supportFilterService, true, false, false, false);
            _streamFactoryService.DropStream(_filterSpecs[1], _supportFilterService, true, false, false, false);
            Assert.AreEqual(2, _supportFilterService.Removed.Count);
    
            // Filter removed
            _streamFactoryService.DropStream(_filterSpecs[2], _supportFilterService, true, false, false, false);
            Assert.AreEqual(3, _supportFilterService.Removed.Count);
    
            // Something already removed
            try
            {
                _streamFactoryService.DropStream(_filterSpecs[2], _supportFilterService, true, false, false, false);
                Assert.Fail();
            }
            catch (IllegalStateException)
            {
                // Expected
            }
        }
    
        [Test]
        public void TestCreateNoJoin()
        {
            var stmtHande = new EPStatementHandle(1, "id", null, StatementType.SELECT, "text", false, null, 1, false, false, new MultiMatchHandlerFactoryImpl().GetDefaultHandler());
            var stmtAgentHandle = new EPStatementAgentInstanceHandle(stmtHande, _container.RWLockManager().CreateDefaultLock(), -1, null, null);
    
            _streams = new EventStream[4];
            _streams[0] = _streamFactoryService.CreateStream(1, _filterSpecs[0], _supportFilterService, stmtAgentHandle, false, null, false, false, null, false, 0, false).First;
            _streams[1] = _streamFactoryService.CreateStream(2, _filterSpecs[0], _supportFilterService, stmtAgentHandle, false, null, false, false, null, false, 0, false).First;
            _streams[2] = _streamFactoryService.CreateStream(3, _filterSpecs[1], _supportFilterService, stmtAgentHandle, false, null, false, false, null, false, 0, false).First;
            _streams[3] = _streamFactoryService.CreateStream(4, _filterSpecs[2], _supportFilterService, stmtAgentHandle, false, null, false, false, null, false, 0, false).First;
    
            // Streams are reused
            Assert.AreSame(_streams[0], _streams[1]);
            Assert.AreSame(_streams[0], _streams[2]);
            Assert.AreNotSame(_streams[0], _streams[3]);
    
            // Type is ok
            Assert.AreEqual(typeof(SupportBean), _streams[0].EventType.UnderlyingType);
    
            // 2 filters are active now
            Assert.AreEqual(2, _supportFilterService.Added.Count);
        }
    
        [Test]
        public void TestDropNoJoin()
        {
            var stmtHande = new EPStatementHandle(1, "id", null, StatementType.SELECT, "text", false, null, 1, false, false, new MultiMatchHandlerFactoryImpl().GetDefaultHandler());
            var stmtAgentHandle = new EPStatementAgentInstanceHandle(stmtHande, _container.RWLockManager().CreateDefaultLock(), -1, null, null);
            _streams = new EventStream[4];
            _streams[0] = _streamFactoryService.CreateStream(1, _filterSpecs[0], _supportFilterService, stmtAgentHandle, false, null, false, false, null, false, 0, false).First;
            _streams[1] = _streamFactoryService.CreateStream(2, _filterSpecs[0], _supportFilterService, stmtAgentHandle, false, null, false, false, null, false, 0, false).First;
            _streams[2] = _streamFactoryService.CreateStream(3, _filterSpecs[1], _supportFilterService, stmtAgentHandle, false, null, false, false, null, false, 0, false).First;
            _streams[3] = _streamFactoryService.CreateStream(4, _filterSpecs[2], _supportFilterService, stmtAgentHandle, false, null, false, false, null, false, 0, false).First;

            _streamFactoryService.DropStream(_filterSpecs[0], _supportFilterService, false, false, false, false);
            _streamFactoryService.DropStream(_filterSpecs[1], _supportFilterService, false, false, false, false);
            Assert.AreEqual(0, _supportFilterService.Removed.Count);
    
            // Filter removed
            _streamFactoryService.DropStream(_filterSpecs[0], _supportFilterService, false, false, false, false);
            Assert.AreEqual(1, _supportFilterService.Removed.Count);
    
            _streamFactoryService.DropStream(_filterSpecs[2], _supportFilterService, false, false, false, false);
            Assert.AreEqual(2, _supportFilterService.Removed.Count);
    
            // Something already removed
            try
            {
                _streamFactoryService.DropStream(_filterSpecs[2], _supportFilterService, false, false, false, false);
                Assert.Fail();
            }
            catch (IllegalStateException)
            {
                // Expected
            }
        }
    }
}
