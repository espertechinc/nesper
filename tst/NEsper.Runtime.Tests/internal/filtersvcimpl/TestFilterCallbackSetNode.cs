///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.filtersvc;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.threading.locks;
using com.espertech.esper.runtime.@internal.support;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.runtime.@internal.filtersvcimpl
{
    [TestFixture]
    public class TestFilterCallbackSetNode : AbstractRuntimeTest
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
            SupportExprEventEvaluator eval = new SupportExprEventEvaluator(eventType.GetGetter(fieldName));
            return new ExprFilterSpecLookupable(fieldName, eval, null, eventType.GetPropertyType(fieldName), false, null);
        }

        [Test, RunInApplicationDomain]
        public void TestNodeGetSet()
        {
            FilterHandle exprOne = new SupportFilterHandle();

            // Check pre-conditions
            ClassicAssert.IsTrue(testNode.NodeRWLock != null);
            ClassicAssert.IsFalse(testNode.Contains(exprOne));
            ClassicAssert.AreEqual(0, testNode.FilterCallbackCount);
            ClassicAssert.AreEqual(0, testNode.Indizes.Count);
            ClassicAssert.IsTrue(testNode.IsEmpty());

            testNode.Add(exprOne);

            // Check after add
            ClassicAssert.IsTrue(testNode.Contains(exprOne));
            ClassicAssert.AreEqual(1, testNode.FilterCallbackCount);
            ClassicAssert.IsFalse(testNode.IsEmpty());

            // Add an indexOne
            EventType eventType = SupportEventTypeFactory
                .GetInstance(Container)
                .CreateBeanType(typeof(SupportBean));
            SupportExprEventEvaluator eval = new SupportExprEventEvaluator(eventType.GetGetter("IntPrimitive"));
            ExprFilterSpecLookupable lookupable = new ExprFilterSpecLookupable(
                "IntPrimitive",
                eval,
                null,
                eventType.GetPropertyType("IntPrimitive"),
                false,
                null);
            FilterParamIndexBase indexOne = new SupportFilterParamIndex(lookupable);
            testNode.Add(indexOne);

            // Check after add
            ClassicAssert.AreEqual(1, testNode.Indizes.Count);
            ClassicAssert.AreEqual(indexOne, testNode.Indizes[0]);

            // Check removes
            ClassicAssert.IsTrue(testNode.Remove(exprOne));
            ClassicAssert.IsFalse(testNode.IsEmpty());
            ClassicAssert.IsFalse(testNode.Remove(exprOne));
            ClassicAssert.IsTrue(testNode.Remove(indexOne));
            ClassicAssert.IsFalse(testNode.Remove(indexOne));
            ClassicAssert.IsTrue(testNode.IsEmpty());
        }

        [Test, RunInApplicationDomain]
        public void TestNodeMatching()
        {
            var eventObject = new SupportBeanSimple("DepositEvent_1", 1);
            var eventBean = SupportEventBeanFactory
                .GetInstance(Container)
                .CreateObject(eventObject);

            FilterHandle expr = new SupportFilterHandle();
            testNode.Add(expr);

            // Check matching without an index node
            IList<FilterHandle> matches = new List<FilterHandle>();
            testNode.MatchEvent(eventBean, matches, null);
            ClassicAssert.AreEqual(1, matches.Count);
            ClassicAssert.AreEqual(expr, matches[0]);
            matches.Clear();

            // Create, add and populate an index node
            FilterParamIndexBase index = new FilterParamIndexEquals(
                MakeLookupable("MyString", eventBean.EventType),
                new SlimReaderWriterLock());
            testNode.Add(index);
            index.Put("DepositEvent_1", testEvaluator);

            // Verify matcher instance stored in index is called
            testNode.MatchEvent(eventBean, matches, null);

            ClassicAssert.IsTrue(testEvaluator.GetAndResetCountInvoked() == 1);
            ClassicAssert.IsTrue(testEvaluator.LastEvent == eventBean);
            ClassicAssert.AreEqual(1, matches.Count);
            ClassicAssert.AreEqual(expr, matches[0]);
        }
    }
} // end of namespace
