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
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.threading;
using com.espertech.esper.core.support;
using com.espertech.esper.events.bean;
using com.espertech.esper.events.map;
using com.espertech.esper.supportunit.bean;
using com.espertech.esper.supportunit.util;
using NUnit.Framework;

namespace com.espertech.esper.events
{
    /// <summary>Test for multithread-safety for manageing statements, i.e. creating and stopping statements </summary>
    [TestFixture]
    public class TestEventAdapterSvcMT 
    {
        private EventAdapterService _service;
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();
            _service = new EventAdapterServiceImpl(
                _container,
                new EventTypeIdGeneratorImpl(), 5, null,
                SupportEngineImportServiceFactory.Make(_container));
        }
    
        [Test]
        public void TestAddBeanTypeClass()
        {
            var types = new HashSet<EventType>();
    
            var callables = new Callable[2];
            for (var i = 0; i < callables.Length; i++)
            {
                callables[i] = () =>
                {
                    var type = _service.AddBeanType("a", typeof(SupportMarketDataBean), true, true, true);
                    types.Add(type);
    
                    type = _service.AddBeanType("b", typeof(SupportMarketDataBean), true, true, true);
                    types.Add(type);
                    return true;
                };
            }
            
            var result = TryMT(callables);
            EPAssertionUtil.AssertAllBooleanTrue(result);
            Assert.AreEqual(1, types.Count);
        }
    
        [Test]
        public void TestAddMapType()
        {
            var typeOne = new Dictionary<String, Object>();
            typeOne.Put("f1", typeof(int));
            var typeTwo = new Dictionary<String, Object>();
            typeTwo.Put("f2", typeof(int));
    
            var callables = new Callable[2];
            for (var i = 0; i < callables.Length; i++)
            {
                var index = i;
                callables[i] = () =>
                {
                    try
                    {
                        if (index == 0)
                        {
                            return _service.AddNestableMapType("A", typeOne, null, true, true, true, false, false);
                        }
                        else
                        {
                            return _service.AddNestableMapType("A", typeTwo, null, true, true, true, false, false);
                        }
                    }
                    catch (EventAdapterException ex)
                    {
                        return ex;
                    }
                };
            }
    
            // the result should be one exception and one type
            var results = TryMT(callables);
            EPAssertionUtil.AssertTypeEqualsAnyOrder(new Type[]{typeof(EventAdapterException), typeof(MapEventType)}, results);
        }
    
        [Test]
        public void TestAddBeanType()
        {
            var typeOne = new Dictionary<String, Type>();
            typeOne.Put("f1", typeof(int));
    
            var callables = new Callable[2];
            for (var i = 0; i < callables.Length; i++)
            {
                var index = i;
                callables[i] = () =>
                {
                    try
                    {
                        if (index == 0)
                        {
                            return _service.AddBeanType("X", typeof(SupportBean_S1), true, true, true);
                        }
                        else
                        {
                            return _service.AddBeanType("X", typeof(SupportBean_S0), true, true, true);
                        }
                    }
                    catch (EventAdapterException ex)
                    {
                        return ex;
                    }
                };
            }
    
            // the result should be one exception and one type
            var results = TryMT(callables);
            EPAssertionUtil.AssertTypeEqualsAnyOrder(
                new Type[] { typeof(EventAdapterException),  typeof(BeanEventType) },
                results);
        }
    
        private Object[] TryMT(Callable[] callables)
        {
            var threadPool = Executors.NewFixedThreadPool(callables.Length);
    
            var futures = new Future<object>[callables.Length];
            for (var i = 0; i < callables.Length; i++)
            {
                var callable = callables[i];
                futures[i] = threadPool.Submit(() => callable());
            }

            threadPool.Shutdown();
            threadPool.AwaitTermination(TimeSpan.FromSeconds(10));

            return futures.Select(f => f.GetValueOrDefault()).ToArray();
        }
    
        private interface CallableFactory
        {
            Callable MakeCallable(int threadNum);
        }
    }
}
