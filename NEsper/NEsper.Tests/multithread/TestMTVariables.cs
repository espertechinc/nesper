///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.threading;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.util;

using NUnit.Framework;

namespace com.espertech.esper.multithread
{
    /// <summary>
    /// Test for multithread-safety of setting and reading variables.
    /// <para/>
    /// Assume we have 2 statements that set 3 variables, and one statement that
    /// selects variables:
    /// <para/>
    /// <pre>on A as a set var1 = a.value, var2 = a.value, var3 = var3 + 1</pre> 
    /// <pre>on B as a set var1 = b.value, var2 = b.value, var3 = var3 + 1</pre>
    /// <pre>select var1, var2 from C(id=threadid)</pre> (one per thread)
    /// <para/>
    /// Result: If 4 threads send A and B events and assign a random value, then var1
    /// and var2 should always be the same value both when selected in the select
    /// statement. In addition, the counter var3 should not miss a single value when posted to
    /// listeners of the set-statements.
    /// <para/>
    /// Each thread sends for each loop one A, B and C event, and returns the result
    /// for all "var3" values for checking when done.
    /// </summary>
    [TestFixture]
    public class TestMTVariables 
    {
        private EPServiceProvider _epService;
        private SupportMTUpdateListener _listenerSetOne;
        private SupportMTUpdateListener _listenerSetTwo;
    
        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
    
            _epService.EPAdministrator.Configuration.AddVariable("var1", typeof(long), 0);
            _epService.EPAdministrator.Configuration.AddVariable("var2", typeof(long), 0);
            _epService.EPAdministrator.Configuration.AddVariable("var3", typeof(long), 0);
    
            _listenerSetOne = new SupportMTUpdateListener();
            _listenerSetTwo = new SupportMTUpdateListener();
    
            String stmtSetOneText = "on " + typeof(SupportBean).FullName + " set var1=LongPrimitive, var2=LongPrimitive, var3=var3+1";
            String stmtSetTwoText = "on " + typeof(SupportMarketDataBean).FullName + " set var1=Volume, var2=Volume, var3=var3+1";
            _epService.EPAdministrator.CreateEPL(stmtSetOneText).Events += _listenerSetOne.Update;
            _epService.EPAdministrator.CreateEPL(stmtSetTwoText).Events += _listenerSetTwo.Update;
        }
    
        [TearDown]
        public void TearDown()
        {
            _listenerSetOne = null;
            _listenerSetTwo = null;
        }
    
        [Test]
        public void TestMTSetAtomicity()
        {
            TrySetAndReadAtomic(4, 2000);
        }
    
        [Test]
        public void TestMTAtomicity()
        {
            TrySetAndReadAtomic(2, 10000);
        }
    
        private void TrySetAndReadAtomic(int numThreads, int numRepeats)
        {
            var threadPool = Executors.NewFixedThreadPool(numThreads);
            var future = new Future<bool>[numThreads];
            for (int i = 0; i < numThreads; i++)
            {
                var callable = new VariableReadWriteCallable(i, _epService, numRepeats);
                future[i] = threadPool.Submit(callable);
            }
    
            threadPool.Shutdown();
            threadPool.AwaitTermination(TimeSpan.FromSeconds(10));
    
            for (int i = 0; i < numThreads; i++)
            {
                Assert.IsTrue(future[i].GetValueOrDefault());
            }
    
            // Determine if we have all numbers for var3 and didn't skip one.
            // Since "var3 = var3 + 1" is executed by multiple statements and threads we need to have
            // this counter have all the values from 0 to N-1.
            var var3Values = new SortedSet<long>();
            foreach (EventBean theEvent in _listenerSetOne.GetNewDataListFlattened())
            {
                var3Values.Add(theEvent.Get("var3").AsLong());
            }
            foreach (EventBean theEvent in _listenerSetTwo.GetNewDataListFlattened())
            {
                var3Values.Add(theEvent.Get("var3").AsLong());
            }
            Assert.AreEqual(numThreads * numRepeats, var3Values.Count);
            for (int i = 1; i < numThreads * numRepeats + 1; i++)
            {
                Assert.IsTrue(var3Values.Contains((long)i));
            }
        }
    }
}
