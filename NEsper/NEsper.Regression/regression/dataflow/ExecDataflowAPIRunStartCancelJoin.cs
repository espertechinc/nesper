///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Threading;

using com.espertech.esper.client;
using com.espertech.esper.client.dataflow;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.threading;
using com.espertech.esper.dataflow.util;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;
using NUnit.Framework;

namespace com.espertech.esper.regression.dataflow
{
    public class ExecDataflowAPIRunStartCancelJoin : RegressionExecution
    {
        public override void Run(EPServiceProvider epService)
        {
            RunAssertionNonBlockingJoinCancel(epService);
            RunAssertionNonBlockingJoinException(epService);
            RunAssertionNonBlockingException(epService);
            RunAssertionBlockingException(epService);
            RunAssertionBlockingCancel(epService);
            RunAssertionNonBlockingCancel(epService);
            RunAssertionInvalidJoinRun(epService);
            RunAssertionNonBlockingJoinMultipleRunnable(epService);
            RunAssertionBlockingMultipleRunnable(epService);
            RunAssertionNonBlockingJoinSingleRunnable(epService);
            RunAssertionBlockingRunJoin(epService);
            RunAssertionFastCompleteBlocking(epService);
            RunAssertionRunBlocking(epService);
            RunAssertionFastCompleteNonBlocking(epService);
        }

        private void RunAssertionNonBlockingJoinCancel(EPServiceProvider epService)
        {
            epService.EPAdministrator.CreateEPL("create schema SomeType ()");
            epService.EPAdministrator.CreateEPL(
                "create dataflow MyDataFlowOne " +
                "DefaultSupportSourceOp -> outstream<SomeType> {}" +
                "DefaultSupportCaptureOp(outstream) {}");

            var latchOne = new CountDownLatch(1);
            var src = new DefaultSupportSourceOp(new object[] {latchOne});
            var output = new DefaultSupportCaptureOp(SupportContainer.Instance.LockManager());
            var options =
                new EPDataFlowInstantiationOptions().OperatorProvider(new DefaultSupportGraphOpProvider(src, output));
            var dfOne = epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlowOne", options);

            dfOne.Start();

            var cancellingThread = new Thread(
                () =>
                {
                    try
                    {
                        Thread.Sleep(300);
                        dfOne.Cancel();
                    }
                    catch (Exception e)
                    {
                        Console.Error.WriteLine(e.StackTrace);
                    }
                });
            cancellingThread.Start();
            dfOne.Join();

            Assert.AreEqual(EPDataFlowState.CANCELLED, dfOne.State);
            Assert.AreEqual(0, output.GetAndReset().Count);

            epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionNonBlockingJoinException(EPServiceProvider epService)
        {
            epService.EPAdministrator.CreateEPL("create schema SomeType ()");
            epService.EPAdministrator.CreateEPL(
                "create dataflow MyDataFlowOne " +
                "DefaultSupportSourceOp -> outstream<SomeType> {}" +
                "DefaultSupportCaptureOp(outstream) {}");

            var latchOne = new CountDownLatch(1);
            var src = new DefaultSupportSourceOp(new object[] {latchOne, new MyRuntimeException("TestException")});
            var output = new DefaultSupportCaptureOp(SupportContainer.Instance.LockManager());
            var options =
                new EPDataFlowInstantiationOptions().OperatorProvider(new DefaultSupportGraphOpProvider(src, output));
            var dfOne = epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlowOne", options);

            dfOne.Start();

            var unlatchingThread = new Thread(
                () =>
                {
                    try
                    {
                        Thread.Sleep(300);
                        latchOne.CountDown();
                    }
                    catch (Exception e)
                    {
                        Console.Error.WriteLine(e.StackTrace);
                    }
                });
            unlatchingThread.Start();
            dfOne.Join();

            Assert.AreEqual(EPDataFlowState.COMPLETE, dfOne.State);
            Assert.AreEqual(0, output.GetAndReset().Count);
            epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionNonBlockingException(EPServiceProvider epService)
        {
            epService.EPAdministrator.CreateEPL("create schema SomeType ()");
            epService.EPAdministrator.CreateEPL(
                "create dataflow MyDataFlowOne " +
                "DefaultSupportSourceOp -> outstream<SomeType> {}" +
                "DefaultSupportCaptureOp(outstream) {}");

            var src = new DefaultSupportSourceOp(new object[] {new MyRuntimeException("TestException")});
            var output = new DefaultSupportCaptureOp(SupportContainer.Instance.LockManager());
            var options =
                new EPDataFlowInstantiationOptions().OperatorProvider(new DefaultSupportGraphOpProvider(src, output));
            var dfOne = epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlowOne", options);

            dfOne.Start();
            Thread.Sleep(200);
            Assert.AreEqual(EPDataFlowState.COMPLETE, dfOne.State);
            Assert.AreEqual(0, output.GetAndReset().Count);
            epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionBlockingException(EPServiceProvider epService)
        {
            epService.EPAdministrator.CreateEPL("create schema SomeType ()");
            epService.EPAdministrator.CreateEPL(
                "create dataflow MyDataFlowOne " +
                "DefaultSupportSourceOp -> outstream<SomeType> {}" +
                "DefaultSupportCaptureOp(outstream) {}");

            var src = new DefaultSupportSourceOp(new object[] {new MyRuntimeException("TestException")});
            var output = new DefaultSupportCaptureOp(SupportContainer.Instance.LockManager());
            var options =
                new EPDataFlowInstantiationOptions().OperatorProvider(new DefaultSupportGraphOpProvider(src, output));
            var dfOne = epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlowOne", options);

            try
            {
                dfOne.Run();
                Assert.Fail();
            }
            catch (EPDataFlowExecutionException ex)
            {
                Assert.IsTrue(ex.InnerException.InnerException is MyRuntimeException);
                Assert.AreEqual("Support-graph-source generated exception: TestException", ex.InnerException.Message);
            }

            Assert.AreEqual(EPDataFlowState.COMPLETE, dfOne.State);
            Assert.AreEqual(0, output.GetAndReset().Count);
            epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionBlockingCancel(EPServiceProvider epService)
        {
            // declare
            epService.EPAdministrator.CreateEPL("create schema SomeType ()");
            epService.EPAdministrator.CreateEPL(
                "create dataflow MyDataFlowOne " +
                "SourceOne -> outstream<SomeType> {}" +
                "OutputOp(outstream) {}");

            // instantiate
            var latchOne = new CountDownLatch(1);
            var ops = new Dictionary<string, object>();
            ops.Put("SourceOne", new DefaultSupportSourceOp(new object[] {latchOne, new object[] {1}}));
            var output = new DefaultSupportCaptureOp(SupportContainer.Instance.LockManager());
            ops.Put("OutputOp", output);

            var options =
                new EPDataFlowInstantiationOptions().OperatorProvider(new DefaultSupportGraphOpProviderByOpName(ops));
            var dfOne = epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlowOne", options);

            var cancellingThread = new Thread(
                () =>
                {
                    try
                    {
                        Thread.Sleep(300);
                        dfOne.Cancel();
                    }
                    catch (Exception e)
                    {
                        Console.Error.WriteLine(e.StackTrace);
                    }
                });
            cancellingThread.Start();

            try
            {
                dfOne.Run();
                Assert.Fail();
            }
            catch (EPDataFlowCancellationException ex)
            {
                Assert.AreEqual("Data flow 'MyDataFlowOne' execution was cancelled", ex.Message);
            }

            Assert.AreEqual(EPDataFlowState.CANCELLED, dfOne.State);
            Assert.AreEqual(0, output.GetAndReset().Count);
            epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionNonBlockingCancel(EPServiceProvider epService)
        {
            // declare
            epService.EPAdministrator.CreateEPL("create schema SomeType ()");
            epService.EPAdministrator.CreateEPL(
                "create dataflow MyDataFlowOne " +
                "SourceOne -> outstream<SomeType> {}" +
                "OutputOp(outstream) {}");

            // instantiate
            var latchOne = new CountDownLatch(1);
            var ops = new Dictionary<string, object>();
            ops.Put("SourceOne", new DefaultSupportSourceOp(new object[] {latchOne, new object[] {1}}));
            var output = new DefaultSupportCaptureOp(SupportContainer.Instance.LockManager());
            ops.Put("OutputOp", output);

            var options =
                new EPDataFlowInstantiationOptions().OperatorProvider(new DefaultSupportGraphOpProviderByOpName(ops));
            var dfOne = epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlowOne", options);

            dfOne.Start();
            Assert.AreEqual(EPDataFlowState.RUNNING, dfOne.State);

            dfOne.Cancel();

            latchOne.CountDown();
            Thread.Sleep(100);
            Assert.AreEqual(EPDataFlowState.CANCELLED, dfOne.State);
            Assert.AreEqual(0, output.GetAndReset().Count);
            epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionInvalidJoinRun(EPServiceProvider epService)
        {
            epService.EPAdministrator.CreateEPL(
                "create dataflow MyDataFlowOne " +
                "BeaconSource -> BeaconStream {iterations : 1}");

            var source = new DefaultSupportSourceOp(new object[] {5000});
            var options =
                new EPDataFlowInstantiationOptions().OperatorProvider(new DefaultSupportGraphOpProvider(source));
            var dfOne = epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlowOne", options);

            // invalid join
            try
            {
                dfOne.Join();
                Assert.Fail();
            }
            catch (IllegalStateException ex)
            {
                Assert.AreEqual(
                    "Data flow 'MyDataFlowOne' instance has not been executed, please use join after start or run",
                    ex.Message);
            }

            // cancel
            dfOne.Cancel();

            // invalid run and start
            try
            {
                dfOne.Run();
                Assert.Fail();
            }
            catch (IllegalStateException ex)
            {
                Assert.AreEqual(
                    "Data flow 'MyDataFlowOne' instance has been cancelled and cannot be run or started", ex.Message);
            }

            try
            {
                dfOne.Start();
                Assert.Fail();
            }
            catch (IllegalStateException ex)
            {
                Assert.AreEqual(
                    "Data flow 'MyDataFlowOne' instance has been cancelled and cannot be run or started", ex.Message);
            }

            // cancel again
            dfOne.Cancel();
            epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionNonBlockingJoinMultipleRunnable(EPServiceProvider epService)
        {
            // declare
            epService.EPAdministrator.CreateEPL("create schema SomeType ()");
            epService.EPAdministrator.CreateEPL(
                "create dataflow MyDataFlowOne " +
                "SourceOne -> outstream<SomeType> {}" +
                "SourceTwo -> outstream<SomeType> {}" +
                "Future(outstream) {}");

            // instantiate
            var latchOne = new CountDownLatch(1);
            var latchTwo = new CountDownLatch(1);
            var ops = new Dictionary<string, object>();
            ops.Put("SourceOne", new DefaultSupportSourceOp(new object[] {latchOne, new object[] {1}}));
            ops.Put("SourceTwo", new DefaultSupportSourceOp(new object[] {latchTwo, new object[] {1}}));
            var future = new DefaultSupportCaptureOp(2, SupportContainer.Instance.LockManager());
            ops.Put("Future", future);

            var options =
                new EPDataFlowInstantiationOptions().OperatorProvider(new DefaultSupportGraphOpProviderByOpName(ops));
            var dfOne = epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlowOne", options);

            dfOne.Start();
            Thread.Sleep(50);
            Assert.AreEqual(EPDataFlowState.RUNNING, dfOne.State);

            latchOne.CountDown();
            Thread.Sleep(200);
            Assert.AreEqual(EPDataFlowState.RUNNING, dfOne.State);

            latchTwo.CountDown();
            dfOne.Join();
            Assert.AreEqual(EPDataFlowState.COMPLETE, dfOne.State);
            Assert.AreEqual(2, future.GetAndReset().Count);
            epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionBlockingMultipleRunnable(EPServiceProvider epService)
        {
            // declare
            epService.EPAdministrator.CreateEPL("create schema SomeType ()");
            epService.EPAdministrator.CreateEPL(
                "create dataflow MyDataFlowOne " +
                "SourceOne -> outstream<SomeType> {}" +
                "SourceTwo -> outstream<SomeType> {}" +
                "Future(outstream) {}");

            // instantiate
            var latchOne = new CountDownLatch(1);
            var latchTwo = new CountDownLatch(1);
            var ops = new Dictionary<string, object>();
            ops.Put("SourceOne", new DefaultSupportSourceOp(new object[] {latchOne, new object[] {1}}));
            ops.Put("SourceTwo", new DefaultSupportSourceOp(new object[] {latchTwo, new object[] {1}}));
            var future = new DefaultSupportCaptureOp(2, SupportContainer.Instance.LockManager());
            ops.Put("Future", future);

            var options =
                new EPDataFlowInstantiationOptions().OperatorProvider(new DefaultSupportGraphOpProviderByOpName(ops));
            var dfOne = epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlowOne", options);

            try
            {
                dfOne.Run();
                Assert.Fail();
            }
            catch (UnsupportedOperationException ex)
            {
                Assert.AreEqual(
                    "The data flow 'MyDataFlowOne' has zero or multiple sources and requires the use of the start method instead",
                    ex.Message);
            }

            latchTwo.CountDown();
            dfOne.Start();
            latchOne.CountDown();
            dfOne.Join();

            Assert.AreEqual(EPDataFlowState.COMPLETE, dfOne.State);
            Assert.AreEqual(2, future.GetAndReset().Count);
            epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionNonBlockingJoinSingleRunnable(EPServiceProvider epService)
        {
            // declare
            epService.EPAdministrator.CreateEPL("create schema SomeType ()");
            epService.EPAdministrator.CreateEPL(
                "create dataflow MyDataFlowOne " +
                "DefaultSupportSourceOp -> outstream<SomeType> {}" +
                "DefaultSupportCaptureOp(outstream) {}");

            // instantiate
            var latch = new CountDownLatch(1);
            var source = new DefaultSupportSourceOp(new object[] {latch, new object[] {1}});
            var future = new DefaultSupportCaptureOp(1, SupportContainer.Instance.LockManager());
            var options =
                new EPDataFlowInstantiationOptions().OperatorProvider(
                    new DefaultSupportGraphOpProvider(source, future));
            var dfOne = epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlowOne", options);
            Assert.AreEqual("MyDataFlowOne", dfOne.DataFlowName);
            Assert.AreEqual(EPDataFlowState.INSTANTIATED, dfOne.State);

            dfOne.Start();
            Thread.Sleep(100);
            Assert.AreEqual(EPDataFlowState.RUNNING, dfOne.State);

            latch.CountDown();
            dfOne.Join();
            Assert.AreEqual(EPDataFlowState.COMPLETE, dfOne.State);
            Assert.AreEqual(1, future.GetAndReset()[0].Count);
            Assert.AreEqual(2, source.GetCurrentCount());

            dfOne.Cancel();
            Assert.AreEqual(EPDataFlowState.COMPLETE, dfOne.State);
            epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionBlockingRunJoin(EPServiceProvider epService)
        {
            // declare
            epService.EPAdministrator.CreateEPL("create schema SomeType ()");
            epService.EPAdministrator.CreateEPL(
                "create dataflow MyDataFlowOne " +
                "DefaultSupportSourceOp -> s<SomeType> {}" +
                "DefaultSupportCaptureOp(s) {}");

            // instantiate
            var latch = new CountDownLatch(1);
            var source = new DefaultSupportSourceOp(new object[] {latch, new object[] {1}});
            var future = new DefaultSupportCaptureOp(1, SupportContainer.Instance.LockManager());
            var options =
                new EPDataFlowInstantiationOptions().OperatorProvider(
                    new DefaultSupportGraphOpProvider(source, future));
            var dfOne = epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlowOne", options);
            Assert.AreEqual("MyDataFlowOne", dfOne.DataFlowName);
            Assert.AreEqual(EPDataFlowState.INSTANTIATED, dfOne.State);

            var joiningRunnable = new MyJoiningRunnable(dfOne);
            var joiningThread = new Thread(joiningRunnable.Run);

            var unlatchingThread = new Thread(
                () =>
                {
                    try
                    {
                        while (dfOne.State != EPDataFlowState.RUNNING)
                        {
                            Thread.Sleep(10);
                        }

                        Thread.Sleep(1000);
                        latch.CountDown();
                    }
                    catch (Exception e)
                    {
                        Console.Error.WriteLine(e.StackTrace);
                    }
                });

            joiningThread.Start();
            unlatchingThread.Start();
            dfOne.Run();

            Assert.AreEqual(EPDataFlowState.COMPLETE, dfOne.State);
            Assert.AreEqual(1, future.GetAndReset()[0].Count);
            Assert.AreEqual(2, source.GetCurrentCount());

            joiningThread.Join();
            unlatchingThread.Join();
            var deltaJoin = joiningRunnable.End - joiningRunnable.Start;
            Assert.IsTrue(deltaJoin >= 500, "deltaJoin=" + deltaJoin);
            epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionFastCompleteBlocking(EPServiceProvider epService)
        {
            // declare
            epService.EPAdministrator.CreateEPL(
                "create dataflow MyDataFlowOne " +
                "BeaconSource -> BeaconStream {iterations : 1}" +
                "DefaultSupportCaptureOp(BeaconStream) {}");

            // instantiate
            var future = new DefaultSupportCaptureOp(1, SupportContainer.Instance.LockManager());
            var options =
                new EPDataFlowInstantiationOptions().OperatorProvider(new DefaultSupportGraphOpProvider(future));
            var dfOne = epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlowOne", options);
            Assert.AreEqual("MyDataFlowOne", dfOne.DataFlowName);
            Assert.AreEqual(EPDataFlowState.INSTANTIATED, dfOne.State);

            // has not run
            Thread.Sleep(1000);
            Assert.IsFalse(future.IsDone());

            // blocking run
            dfOne.Run();
            Assert.AreEqual(EPDataFlowState.COMPLETE, dfOne.State);
            Assert.AreEqual(1, future.GetValue(TimeSpan.Zero).Length);

            // assert past-exec
            TryAssertionAfterExec(dfOne);

            epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionRunBlocking(EPServiceProvider epService)
        {
            // declare
            epService.EPAdministrator.CreateEPL("create schema SomeType ()");
            epService.EPAdministrator.CreateEPL(
                "create dataflow MyDataFlowOne " +
                "DefaultSupportSourceOp -> s<SomeType> {}" +
                "DefaultSupportCaptureOp(s) {}");

            // instantiate
            var latch = new CountDownLatch(1);
            var source = new DefaultSupportSourceOp(new object[] {latch, new object[] {1}});
            var future = new DefaultSupportCaptureOp(1, SupportContainer.Instance.LockManager());
            var options =
                new EPDataFlowInstantiationOptions().OperatorProvider(
                    new DefaultSupportGraphOpProvider(future, source));
            var dfOne = epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlowOne", options);
            Assert.AreEqual("MyDataFlowOne", dfOne.DataFlowName);
            Assert.AreEqual(EPDataFlowState.INSTANTIATED, dfOne.State);

            var unlatchingThread = new Thread(
                () =>
                {
                    try
                    {
                        while (dfOne.State != EPDataFlowState.RUNNING)
                        {
                            Thread.Sleep(0);
                        }

                        Thread.Sleep(100);
                        latch.CountDown();
                    }
                    catch (Exception e)
                    {
                        Console.Error.WriteLine(e.StackTrace);
                    }
                });

            // blocking run
            unlatchingThread.Start();
            dfOne.Run();
            Assert.AreEqual(EPDataFlowState.COMPLETE, dfOne.State);
            Assert.AreEqual(1, future.GetAndReset()[0].Count);
            Assert.AreEqual(2, source.GetCurrentCount());
            unlatchingThread.Join();
            epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionFastCompleteNonBlocking(EPServiceProvider epService)
        {
            // declare
            epService.EPAdministrator.CreateEPL(
                "create dataflow MyDataFlowOne " +
                "BeaconSource -> BeaconStream {iterations : 1}" +
                "DefaultSupportCaptureOp(BeaconStream) {}");

            // instantiate
            var future = new DefaultSupportCaptureOp(1, SupportContainer.Instance.LockManager());
            var options =
                new EPDataFlowInstantiationOptions().OperatorProvider(new DefaultSupportGraphOpProvider(future));
            var dfOne = epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlowOne", options);
            Assert.AreEqual("MyDataFlowOne", dfOne.DataFlowName);
            Assert.AreEqual(EPDataFlowState.INSTANTIATED, dfOne.State);
            Assert.IsFalse(future.IsDone());

            // non-blocking run, spinning wait
            dfOne.Start();
            var start = PerformanceObserver.MilliTime;
            while (dfOne.State != EPDataFlowState.COMPLETE)
            {
                if (PerformanceObserver.MilliTime - start > 1000)
                {
                    Assert.Fail();
                }
            }

            Assert.AreEqual(1, future.GetValue(TimeSpan.Zero).Length);

            // assert past-exec
            TryAssertionAfterExec(dfOne);
            epService.EPAdministrator.DestroyAllStatements();
        }

        private void TryAssertionAfterExec(EPDataFlowInstance df)
        {
            // cancel and join ignored
            df.Join();

            // can't start or run again
            try
            {
                df.Run();
                Assert.Fail();
            }
            catch (IllegalStateException ex)
            {
                Assert.AreEqual(
                    "Data flow 'MyDataFlowOne' instance has already completed, please use instantiate to run the data flow again",
                    ex.Message);
            }

            try
            {
                df.Start();
                Assert.Fail();
            }
            catch (IllegalStateException ex)
            {
                Assert.AreEqual(
                    "Data flow 'MyDataFlowOne' instance has already completed, please use instantiate to run the data flow again",
                    ex.Message);
            }

            df.Cancel();
            df.Join();
        }

        public class MyJoiningRunnable
        {
            private readonly EPDataFlowInstance _instance;

            public MyJoiningRunnable(EPDataFlowInstance instance)
            {
                _instance = instance;
            }

            public long Start { get; private set; }

            public long End { get; private set; }

            public void Run()
            {
                try
                {
                    while (_instance.State != EPDataFlowState.RUNNING)
                    {
                        Thread.Sleep(0);
                    }

                    Start = DateTimeHelper.CurrentTimeMillis;
                    _instance.Join();
                    End = DateTimeHelper.CurrentTimeMillis;
                }
                catch (ThreadInterruptedException e)
                {
                    Console.Error.WriteLine(e.StackTrace);
                }
            }
        }

        public class MyRuntimeException : EPRuntimeException
        {
            public MyRuntimeException(string message) : base(message)
            {
            }
        }
    }
} // end of namespace