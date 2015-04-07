///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading;
using com.espertech.esper.dispatch;
using NUnit.Framework;

namespace com.espertech.esper.multithread.dispatchmodel
{
    /// <summary>
    /// A model for testing multithreaded dispatches to listeners.
    /// <para/>
    /// Each thread in a loop: Next producer invoke Producer generates next integer
    /// Producer sends int[] {num, 0}
    /// </summary>
    [TestFixture]
    public class TestMTDispatch
    {
        private static void TrySend(int numThreads,
                                    int numCount,
                                    int ratioDoubleAdd,
                                    UpdateDispatchViewModel updateDispatchView,
                                    DispatchService dispatchService)
        {
            // execute
            IExecutorService threadPool = Executors.NewFixedThreadPool(numThreads);
            var future = new Future<bool>[numThreads];
            var callables = new DispatchCallable[numThreads];
            var producer = new DispatchProducer(updateDispatchView);
            for (int i = 0; i < numThreads; i++) {
                callables[i] = new DispatchCallable(producer, i, numCount, ratioDoubleAdd, updateDispatchView,
                                                    dispatchService);
                future[i] = threadPool.Submit(callables[i]);
            }

            threadPool.Shutdown();
            threadPool.AwaitTermination(new TimeSpan(0, 0, 10));

            for (int i = 0; i < numThreads; i++) {
                Assert.IsTrue(future[i].GetValueOrDefault());
            }
        }

        public class DispatchCallable : ICallable<bool>
        {
            private readonly DispatchService _dispatchService;

            private readonly int _numActions;
            private readonly int _ratioDoubleAdd;
            private readonly DispatchProducer _sharedProducer;
            private readonly int _threadNum;
            private readonly UpdateDispatchViewModel _updateDispatchView;

            public DispatchCallable(DispatchProducer sharedProducer, int threadNum, int numActions, int ratioDoubleAdd,
                                    UpdateDispatchViewModel updateDispatchView, DispatchService dispatchService)
            {
                _sharedProducer = sharedProducer;
                _threadNum = threadNum;
                _numActions = numActions;
                _ratioDoubleAdd = ratioDoubleAdd;
                _updateDispatchView = updateDispatchView;
                _dispatchService = dispatchService;
            }

            #region ICallable Members

            public bool Call()
            {
                Log.Info(".call Thread " + Thread.CurrentThread.ManagedThreadId + " starting");
                for (int i = 0; i < _numActions; i++) {
                    if (i%10000 == 1) {
                        Log.Info(".call Thread " + Thread.CurrentThread.ManagedThreadId + " at " + i);
                    }

                    int nominal = _sharedProducer.Next();
                    if (i%_ratioDoubleAdd == 1) {
                        _updateDispatchView.Add(new[] {nominal, 1});
                    }
                    _dispatchService.Dispatch();
                }
                Log.Info(".call Thread " + Thread.CurrentThread.ManagedThreadId + " done");
                return true;
            }

            #endregion

            private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        }

        [Test]
        public void TestSceneOne()
        {
            DispatchService dispatchService = new DispatchServiceImpl();
            var listener = new DispatchListenerImpl();
            //UpdateDispatchViewModel updateDispatchView = new UpdateDispatchViewNonConcurrentModel(dispatchService, listener);
            var updateDispatchView = new UpdateDispatchViewOrderEnforcingModel(dispatchService, listener);

            int numThreads = 2;
            int numActions = 10000;
            int ratioDoubleAdd = 5;
            // generates {(1,0),(2,0), (3,0)}

            TrySend(numThreads, numActions, ratioDoubleAdd, updateDispatchView, dispatchService);

            // assert size
            IList<int[][]> result = listener.Received;
            Assert.AreEqual(numActions*numThreads, result.Count);

            // analyze result
            var nominals = new int[result.Count];
            for (int i = 0; i < result.Count; i++) {
                int[][] entry = result[i];
                //Console.Out.WriteLine("entry=" + Print(entry));

                nominals[i] = entry[0][0];
                Assert.AreEqual((i + 1), nominals[i], "Order not correct: #" + i);

                // Assert last digits and nominals, i.e. (1, 0) (1, 1), (1, 2)
                for (int j = 0; j < entry.Length; j++) {
                    Assert.AreEqual(nominals[i], entry[j][0]);
                    Assert.AreEqual(j, entry[j][1]);
                }
            }
        }
    }
}
