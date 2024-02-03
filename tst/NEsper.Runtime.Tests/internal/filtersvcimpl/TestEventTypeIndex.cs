///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.filtersvc;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.threading.locks;
using com.espertech.esper.container;
using com.espertech.esper.runtime.@internal.support;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.runtime.@internal.filtersvcimpl
{
    [TestFixture]
    public class TestEventTypeIndex : AbstractRuntimeTest
    {
        private EventTypeIndex testIndex;

        private EventBean testEventBean;
        private EventType testEventType;

        private FilterHandleSetNode handleSetNode;
        private FilterHandle filterCallback;

        [SetUp]
        public void SetUp()
        {
            SupportBean testBean = new SupportBean();
            testEventBean = SupportEventBeanFactory
                .GetInstance(Container)
                .CreateObject(testBean);
            testEventType = testEventBean.EventType;

            handleSetNode = new FilterHandleSetNode(new SlimReaderWriterLock());
            filterCallback = new SupportFilterHandle();
            handleSetNode.Add(filterCallback);

            testIndex = new EventTypeIndex(new FilterServiceGranularLockFactoryReentrant(
                Container.RWLockManager()));
            testIndex.Add(testEventType, handleSetNode);
        }

        [Test, RunInApplicationDomain]
        public void TestMatch()
        {
            IList<FilterHandle> matchesList = new List<FilterHandle>();

            // Invoke match
            testIndex.MatchEvent(testEventBean, matchesList, null);

            ClassicAssert.AreEqual(1, matchesList.Count);
            ClassicAssert.AreEqual(filterCallback, matchesList[0]);
        }

        [Test, RunInApplicationDomain]
        public void TestInvalidSecondAdd()
        {
            try
            {
                testIndex.Add(testEventType, handleSetNode);
                ClassicAssert.IsTrue(false);
            }
            catch (IllegalStateException)
            {
                // Expected
            }
        }

        [Test, RunInApplicationDomain]
        public void TestGet()
        {
            ClassicAssert.AreEqual(handleSetNode, testIndex.Get(testEventType));
        }
    }
} // end of namespace
