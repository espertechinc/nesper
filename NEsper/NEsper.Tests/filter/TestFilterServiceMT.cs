///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.threading;
using com.espertech.esper.supportunit.bean;
using com.espertech.esper.supportunit.events;
using com.espertech.esper.supportunit.filter;
using com.espertech.esper.supportunit.util;
using NUnit.Framework;

namespace com.espertech.esper.filter
{
    /// <summary>Test for multithread-safety for manageing statements, i.e. creating and stopping statements </summary>
    [TestFixture]
    public class TestFilterServiceMT 
    {
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();
        }

        [Test]
        public void TestFilterService() 
        {
            RunAssertionAddRemoveFilter(new FilterServiceLockCoarse(
                _container.LockManager(), _container.RWLockManager(), false));
            RunAssertionAddRemoveFilter(new FilterServiceLockFine(
                _container.LockManager(), _container.RWLockManager(), false));
        }

        private void RunAssertionAddRemoveFilter(FilterService service)
        {
            var eventType = SupportEventTypeFactory.CreateBeanType(typeof(SupportBean));
            var spec = SupportFilterSpecBuilder.Build(eventType, new Object[] {"TheString", FilterOperator.EQUAL, "HELLO"});
            var filterValues = spec.GetValueSet(null, null, null);

            var callables = new Func<bool>[5];
            for (int ii = 0; ii < callables.Length; ii++)
            {
                callables[ii] =
                    () =>
                    {
                        var handle = new SupportFilterHandle();
                        for (int jj = 0; jj < 10000; jj++)
                        {
                            var entry = service.Add(filterValues, handle);
                            service.Remove(handle, entry);
                        }
                        return true;
                    };
            }
    
            Object[] result = TryMT(callables);
            EPAssertionUtil.AssertAllBooleanTrue(result);
        }
    
        private static Object[] TryMT(Func<bool>[] callables)
        {
            var threadPool = Executors.NewFixedThreadPool(callables.Length);

            var futures = new Future<bool>[callables.Length];
            for (int i = 0; i < callables.Length; i++)
            {
                futures[i] = threadPool.Submit(callables[i]);
            }
    
            threadPool.Shutdown();
            threadPool.AwaitTermination(TimeSpan.FromSeconds(10));
    
            var results = new Object[futures.Length];
            for (int i = 0; i < futures.Length; i++)
            {
                results[i] = futures[i].GetValueOrDefault();
            }
            return results;
        }
    }
}
