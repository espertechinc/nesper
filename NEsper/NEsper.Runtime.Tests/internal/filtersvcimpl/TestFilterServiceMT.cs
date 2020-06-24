///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common;
using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.filtersvc;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.concurrency;
using com.espertech.esper.container;
using com.espertech.esper.runtime.@internal.support;

using NUnit.Framework;

namespace com.espertech.esper.runtime.@internal.filtersvcimpl
{
    /// <summary>
    /// Test for multithread-safety for managing statements, i.e. creating and stopping statements
    /// </summary>
    [TestFixture]
    public class TestFilterServiceMT : AbstractRuntimeTest
    {
        [Test, RunInApplicationDomain]
        public void TestFilterService()
        {
            var rwLockManager = container.RWLockManager();
            RunAssertionAddRemoveFilter(new FilterServiceLockCoarse(rwLockManager, -1));
            RunAssertionAddRemoveFilter(new FilterServiceLockFine(rwLockManager, -1));
        }

        private void RunAssertionAddRemoveFilter(FilterService service)
        {
            EventType eventType = supportEventTypeFactory.CreateBeanType(typeof(SupportBean));
            var spec = SupportFilterSpecBuilder.Build(eventType, new object[] { "string", FilterOperator.EQUAL, "HELLO" });
            var filterValues = spec.GetValueSet(null, null, null, null);

            var callables = new Func<bool>[5];
            for (var ii = 0; ii < callables.Length; ii++)
            {
                callables[ii] = () => {
                    var handle = new SupportFilterHandle();
                    for (var jj = 0; jj < 10000; jj++)
                    {
                        service.Add(eventType, filterValues, handle);
                        service.Remove(handle, eventType, filterValues);
                    }

                    return true;
                };
            }

            var result = TryMT(callables);
            EPAssertionUtil.AssertAllBooleanTrue(result);
        }

        private object[] TryMT(Func<bool>[] callables)
        {
            var threadPool = Executors.DefaultExecutor(); // NewFixedThreadPool(callables.Length);

            var futures = new IFuture<bool>[callables.Length];
            for (var i = 0; i < callables.Length; i++)
            {
                futures[i] = threadPool.Submit(callables[i]);
            }

            threadPool.Shutdown();
            threadPool.AwaitTermination(10, TimeUnit.SECONDS);

            var results = new object[futures.Length];
            for (var i = 0; i < futures.Length; i++)
            {
                results[i] = futures[i].GetValueOrDefault();
            }
            return results;
        }
    }
} // end of namespace
