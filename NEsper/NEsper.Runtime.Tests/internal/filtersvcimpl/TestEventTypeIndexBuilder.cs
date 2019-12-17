///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.filtersvc;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.container;
using com.espertech.esper.runtime.@internal.support;

using NUnit.Framework;

namespace com.espertech.esper.runtime.@internal.filtersvcimpl
{
    [TestFixture]
    public class TestEventTypeIndexBuilder : AbstractRuntimeTest
    {
        private EventTypeIndex eventTypeIndex;
        private EventTypeIndexBuilder indexBuilder;

        private EventType typeOne;
        private EventType typeTwo;

        private FilterValueSetParam[][] valueSetOne;
        private FilterValueSetParam[][] valueSetTwo;

        private FilterHandle callbackOne;
        private FilterHandle callbackTwo;

        private FilterServiceGranularLockFactoryReentrant lockFactory;

        [SetUp]
        public void SetUp()
        {
            var supportEventTypeFactory = SupportEventTypeFactory.GetInstance(container);

            lockFactory = new FilterServiceGranularLockFactoryReentrant(
                container.RWLockManager());

            eventTypeIndex = new EventTypeIndex(lockFactory);
            indexBuilder = new EventTypeIndexBuilder(eventTypeIndex, true);

            typeOne = supportEventTypeFactory.CreateBeanType(typeof(SupportBean));
            typeTwo = supportEventTypeFactory.CreateBeanType(typeof(SupportBeanSimple));

            valueSetOne = SupportFilterSpecBuilder.Build(typeOne, new object[0]).GetValueSet(null, null, null, null);
            valueSetTwo = SupportFilterSpecBuilder.Build(typeTwo, new object[0]).GetValueSet(null, null, null, null);

            callbackOne = new SupportFilterHandle();
            callbackTwo = new SupportFilterHandle();
        }

        [Test, RunInApplicationDomain]
        public void TestAddRemove()
        {
            Assert.IsNull(eventTypeIndex.Get(typeOne));
            Assert.IsNull(eventTypeIndex.Get(typeTwo));

            indexBuilder.Add(typeOne, valueSetOne, callbackOne, lockFactory);
            indexBuilder.Add(typeTwo, valueSetTwo, callbackTwo, lockFactory);

            Assert.IsTrue(eventTypeIndex.Get(typeOne) != null);
            Assert.IsTrue(eventTypeIndex.Get(typeTwo) != null);

            indexBuilder.Remove(callbackOne, typeOne, valueSetOne);
            indexBuilder.Add(typeOne, valueSetOne, callbackOne, lockFactory);
            indexBuilder.Remove(callbackOne, typeOne, valueSetOne);
        }
    }
} // end of namespace
