///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.threading;
using com.espertech.esper.supportunit.bean;
using com.espertech.esper.supportunit.events;
using com.espertech.esper.supportunit.filter;

using NUnit.Framework;

namespace com.espertech.esper.filter
{
    [TestFixture]
    public class TestEventTypeIndex 
    {
        private EventTypeIndex _testIndex;
    
        private EventBean _testEventBean;
        private EventType _testEventType;
    
        private FilterHandleSetNode _handleSetNode;
        private FilterHandle _filterCallback;
    
        [SetUp]
        public void SetUp()
        {
            SupportBean testBean = new SupportBean();
            _testEventBean = SupportEventBeanFactory.CreateObject(testBean);
            _testEventType = _testEventBean.EventType;
    
            _handleSetNode = new FilterHandleSetNode(ReaderWriterLockManager.CreateDefaultLock());
            _filterCallback = new SupportFilterHandle();
            _handleSetNode.Add(_filterCallback);
    
            _testIndex = new EventTypeIndex(new FilterServiceGranularLockFactoryReentrant());
            _testIndex.Add(_testEventType, _handleSetNode);
        }
    
        [Test]
        public void TestMatch()
        {
            List<FilterHandle> matchesList = new List<FilterHandle>();
    
            // Invoke match
            _testIndex.MatchEvent(_testEventBean, matchesList);
    
            Assert.AreEqual(1, matchesList.Count);
            Assert.AreEqual(_filterCallback, matchesList[0]);
        }
    
        [Test]
        public void TestInvalidSecondAdd()
        {
            try
            {
                _testIndex.Add(_testEventType, _handleSetNode);
                Assert.IsTrue(false);
            }
            catch (IllegalStateException ex)
            {
                // Expected
            }
        }
    
        [Test]
        public void TestGet()
        {
            Assert.AreEqual(_handleSetNode, _testIndex.Get(_testEventType));
        }
    
        [Test]
        public void TestSuperclassMatch()
        {
            _testEventBean = SupportEventBeanFactory.CreateObject(new ISupportAImplSuperGImplPlus());
            _testEventType = SupportEventTypeFactory.CreateBeanType(typeof(ISupportA));

            _testIndex = new EventTypeIndex(new FilterServiceGranularLockFactoryReentrant());
            _testIndex.Add(_testEventType, _handleSetNode);
    
            List<FilterHandle> matchesList = new List<FilterHandle>();
            _testIndex.MatchEvent(_testEventBean, matchesList);
    
            Assert.AreEqual(1, matchesList.Count);
            Assert.AreEqual(_filterCallback, matchesList[0]);
        }
    
        [Test]
        public void TestInterfaceMatch()
        {
            _testEventBean = SupportEventBeanFactory.CreateObject(new ISupportABCImpl("a", "b", "ab", "c"));
            _testEventType = SupportEventTypeFactory.CreateBeanType(typeof(ISupportBaseAB));

            _testIndex = new EventTypeIndex(new FilterServiceGranularLockFactoryReentrant());
            _testIndex.Add(_testEventType, _handleSetNode);
    
            List<FilterHandle> matchesList = new List<FilterHandle>();
            _testIndex.MatchEvent(_testEventBean, matchesList);
    
            Assert.AreEqual(1, matchesList.Count);
            Assert.AreEqual(_filterCallback, matchesList[0]);
        }
    }
}
