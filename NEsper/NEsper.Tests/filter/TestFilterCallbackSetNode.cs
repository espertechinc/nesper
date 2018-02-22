///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.threading;
using com.espertech.esper.supportunit.bean;
using com.espertech.esper.supportunit.events;
using com.espertech.esper.supportunit.filter;
using com.espertech.esper.supportunit.util;
using NUnit.Framework;

namespace com.espertech.esper.filter
{
    [TestFixture]
    public class TestFilterCallbackSetNode 
    {
        private SupportEventEvaluator _testEvaluator;
        private FilterHandleSetNode _testNode;
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();
            _testEvaluator = new SupportEventEvaluator();
            _testNode = new FilterHandleSetNode(_container.RWLockManager().CreateDefaultLock());
        }
    
        [Test]
        public void TestNodeGetSet()
        {
            FilterHandle exprOne = new SupportFilterHandle();
    
            // Check pre-conditions
            Assert.IsTrue(_testNode.NodeRWLock != null);
            Assert.IsFalse(_testNode.Contains(exprOne));
            Assert.AreEqual(0, _testNode.FilterCallbackCount);
            Assert.AreEqual(0, _testNode.Indizes.Count);
            Assert.IsTrue(_testNode.IsEmpty());
    
            _testNode.Add(exprOne);
    
            // Check after add
            Assert.IsTrue(_testNode.Contains(exprOne));
            Assert.AreEqual(1, _testNode.FilterCallbackCount);
            Assert.IsFalse(_testNode.IsEmpty());
    
            // Add an indexOne
            EventType eventType = SupportEventTypeFactory.CreateBeanType(typeof(SupportBean));
            FilterSpecLookupable lookupable = new FilterSpecLookupable("IntPrimitive", eventType.GetGetter("IntPrimitive"), eventType.GetPropertyType("IntPrimitive"), false);
            FilterParamIndexBase indexOne = new SupportFilterParamIndex(lookupable);
            _testNode.Add(indexOne);
    
            // Check after add
            Assert.AreEqual(1, _testNode.Indizes.Count);
            Assert.AreEqual(indexOne, _testNode.Indizes.FirstOrDefault());
    
            // Check removes
            Assert.IsTrue(_testNode.Remove(exprOne));
            Assert.IsFalse(_testNode.IsEmpty());
            Assert.IsFalse(_testNode.Remove(exprOne));
            Assert.IsTrue(_testNode.Remove(indexOne));
            Assert.IsFalse(_testNode.Remove(indexOne));
            Assert.IsTrue(_testNode.IsEmpty());
        }
    
        [Test]
        public void TestNodeMatching()
        {
            SupportBeanSimple eventObject = new SupportBeanSimple("DepositEvent_1", 1);
            EventBean eventBean = SupportEventBeanFactory.CreateObject(eventObject);
    
            FilterHandle expr = new SupportFilterHandle();
            _testNode.Add(expr);
    
            // Check matching without an index node
            List<FilterHandle> matches = new List<FilterHandle>();
            _testNode.MatchEvent(eventBean, matches);
            Assert.AreEqual(1, matches.Count);
            Assert.AreEqual(expr, matches[0]);
            matches.Clear();
    
            // Create, add and populate an index node
            FilterParamIndexBase index = new FilterParamIndexEquals(
                MakeLookupable("MyString", eventBean.EventType), _container.RWLockManager().CreateDefaultLock());
            _testNode.Add(index);
            index["DepositEvent_1"] = _testEvaluator;
    
            // Verify matcher instance stored in index is called
            _testNode.MatchEvent(eventBean, matches);
    
            Assert.IsTrue(_testEvaluator.GetAndResetCountInvoked() == 1);
            Assert.IsTrue(_testEvaluator.GetLastEvent() == eventBean);
            Assert.AreEqual(1, matches.Count);
            Assert.AreEqual(expr, matches[0]);
        }
    
        private FilterSpecLookupable MakeLookupable(String fieldName, EventType eventType) {
            return new FilterSpecLookupable(fieldName, eventType.GetGetter(fieldName), eventType.GetPropertyType(fieldName), false);
        }
    }
}
