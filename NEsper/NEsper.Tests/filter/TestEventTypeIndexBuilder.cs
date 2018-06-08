///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.container;
using com.espertech.esper.supportunit.bean;
using com.espertech.esper.supportunit.events;
using com.espertech.esper.supportunit.filter;
using com.espertech.esper.supportunit.util;
using NUnit.Framework;

namespace com.espertech.esper.filter
{
    [TestFixture]
    public class TestEventTypeIndexBuilder 
    {
        private EventTypeIndex _eventTypeIndex;
        private EventTypeIndexBuilder _indexBuilder;
    
        private EventType _typeOne;
        private EventType _typeTwo;
    
        private FilterValueSet _valueSetOne;
        private FilterValueSet _valueSetTwo;
    
        private FilterHandle _callbackOne;
        private FilterHandle _callbackTwo;

        private FilterServiceGranularLockFactoryReentrant _lockFactory;

        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();

            _lockFactory = new FilterServiceGranularLockFactoryReentrant(_container.RWLockManager());

            _eventTypeIndex = new EventTypeIndex(_lockFactory);
            _indexBuilder = new EventTypeIndexBuilder(_container.LockManager(), _eventTypeIndex, true);
    
            _typeOne = SupportEventTypeFactory.CreateBeanType(typeof(SupportBean));
            _typeTwo = SupportEventTypeFactory.CreateBeanType(typeof(SupportBeanSimple));
    
            _valueSetOne = SupportFilterSpecBuilder.Build(_typeOne, new Object[0]).GetValueSet(null, null, null);
            _valueSetTwo = SupportFilterSpecBuilder.Build(_typeTwo, new Object[0]).GetValueSet(null, null, null);
    
            _callbackOne = new SupportFilterHandle();
            _callbackTwo = new SupportFilterHandle();
        }
    
        [Test]
        public void TestAddRemove()
        {
            Assert.IsNull(_eventTypeIndex.Get(_typeOne));
            Assert.IsNull(_eventTypeIndex.Get(_typeTwo));
    
            var entryOne = _indexBuilder.Add(_valueSetOne, _callbackOne, _lockFactory);
            _indexBuilder.Add(_valueSetTwo, _callbackTwo, _lockFactory);
    
            Assert.IsTrue(_eventTypeIndex.Get(_typeOne) != null);
            Assert.IsTrue(_eventTypeIndex.Get(_typeTwo) != null);
    
            _indexBuilder.Remove(_callbackOne, entryOne);
            _indexBuilder.Add(_valueSetOne, _callbackOne, _lockFactory);
            _indexBuilder.Remove(_callbackOne, entryOne);
        }
    }
}
