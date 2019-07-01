///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.filtersvc;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.threading.locks;
using com.espertech.esper.runtime.@internal.support;

using NUnit.Framework;

namespace com.espertech.esper.runtime.@internal.filtersvcimpl
{
    [TestFixture]
    public class TestFilterCallbackSetNode : AbstractTestBase
    {
        [SetUp]
        public void SetUp()
        {
            testEvaluator = new SupportEventEvaluator();
            testNode = new FilterHandleSetNode(new SlimReaderWriterLock());
        }

        private SupportEventEvaluator testEvaluator;
        private FilterHandleSetNode testNode;

        private ExprFilterSpecLookupable MakeLookupable(
            string fieldName,
            EventType eventType)
        {
            return new ExprFilterSpecLookupable(fieldName, eventType.GetGetter(fieldName), eventType.GetPropertyType(fieldName), false);
        }

        [Test]
        public void TestNodeGetSet()
        {
            FilterHandle exprOne = new SupportFilterHandle();

            // Check pre-conditions
            Assert.IsTrue(testNode.NodeRWLock != null);
            Assert.IsFalse(testNode.Contains(exprOne));
            Assert.AreEqual(0, testNode.FilterCallbackCount);
            Assert.AreEqual(0, testNode.Indizes.Count);
            Assert.IsTrue(testNode.IsEmpty());

            testNode.Add(exprOne);

            // Check after add
            Assert.IsTrue(testNode.Contains(exprOne));
            Assert.AreEqual(1, testNode.FilterCallbackCount);
            Assert.IsFalse(testNode.IsEmpty());

            // Add an indexOne
            EventType eventType = SupportEventTypeFactory
                .GetInstance(container)
                .CreateBeanType(typeof(SupportBean));

            var lookupable = new ExprFilterSpecLookupable(
                "intPrimitive",
                eventType.GetGetter("intPrimitive"),
                eventType.GetPropertyType("intPrimitive"),
                false);
            FilterParamIndexBase indexOne = new SupportFilterParamIndex(lookupable);
            testNode.Add(indexOne);

            // Check after add
            Assert.AreEqual(1, testNode.Indizes.Count);
            Assert.AreEqual(indexOne, testNode.Indizes[0]);

            // Check removes
            Assert.IsTrue(testNode.Remove(exprOne));
            Assert.IsFalse(testNode.IsEmpty());
            Assert.IsFalse(testNode.Remove(exprOne));
            Assert.IsTrue(testNode.Remove(indexOne));
            Assert.IsFalse(testNode.Remove(indexOne));
            Assert.IsTrue(testNode.IsEmpty());
        }

        [Test]
        public void TestNodeMatching()
        {
            var eventObject = new SupportBeanSimple("DepositEvent_1", 1);
            var eventBean = SupportEventBeanFactory
                .GetInstance(container)
                .CreateObject(eventObject);

            FilterHandle expr = new SupportFilterHandle();
            testNode.Add(expr);

            // Check matching without an index node
            IList<FilterHandle> matches = new List<FilterHandle>();
            testNode.MatchEvent(eventBean, matches);
            Assert.AreEqual(1, matches.Count);
            Assert.AreEqual(expr, matches[0]);
            matches.Clear();

            // Create, add and populate an index node
            FilterParamIndexBase index = new FilterParamIndexEquals(
                MakeLookupable("myString", eventBean.EventType),
                new SlimReaderWriterLock());
            testNode.Add(index);
            index.Put("DepositEvent_1", testEvaluator);

            // Verify matcher instance stored in index is called
            testNode.MatchEvent(eventBean, matches);

            Assert.IsTrue(testEvaluator.GetAndResetCountInvoked() == 1);
            Assert.IsTrue(testEvaluator.LastEvent == eventBean);
            Assert.AreEqual(1, matches.Count);
            Assert.AreEqual(expr, matches[0]);
        }
    }
} // end of namespace