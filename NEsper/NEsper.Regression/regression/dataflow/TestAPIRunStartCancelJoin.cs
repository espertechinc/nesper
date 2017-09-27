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
using com.espertech.esper.compat.threading;
using com.espertech.esper.dataflow.util;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.dataflow
{
    [TestFixture]
    public class TestAPIRunStartCancelJoin
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _epService.EPAdministrator.CreateEPL("create schema SomeType ()");
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }

        #endregion

        private EPServiceProvider _epService;

        private static void RunAssertionAfterExec(EPDataFlowInstance df)
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

        [Test]
        public void TestBlockingCancel()
        {
            // declare
            _epService.EPAdministrator.CreateEPL(
                "create dataflow MyDataFlowOne " +
                "SourceOne -> outstream<SomeType> {}" +
                "OutputOp(outstream) {}");

            // instantiate
            var latchOne = new CountDownLatch(1);
            IDictionary<String, Object> ops = new Dictionary<String, Object>();
            ops.Put(
                "SourceOne", new DefaultSupportSourceOp(
                                 new Object[]
                                 {
                                     latchOne, new Object[]
                                     {
                                         1
                                     }
                                 }));
            var output = new DefaultSupportCaptureOp();
            ops["OutputOp"] = output;

            EPDataFlowInstantiationOptions options =
                new EPDataFlowInstantiationOptions().OperatorProvider(new DefaultSupportGraphOpProviderByOpName(ops));
            EPDataFlowInstance dfOne = _epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlowOne", options);

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
        }

        [Test]
        public void TestBlockingException()
        {
            _epService.EPAdministrator.CreateEPL(
                "create dataflow MyDataFlowOne " +
                "DefaultSupportSourceOp -> outstream<SomeType> {}" +
                "DefaultSupportCaptureOp(outstream) {}");

            var src = new DefaultSupportSourceOp(
                new Object[]
                {
                    new MyRuntimeException("TestException")
                });
            var output = new DefaultSupportCaptureOp();
            var options = new EPDataFlowInstantiationOptions().OperatorProvider(new DefaultSupportGraphOpProvider(src, output));
            var dfOne = _epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlowOne", options);

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
        }

        [Test]
        public void TestBlockingMultipleRunnable()
        {
            // declare
            _epService.EPAdministrator.CreateEPL(
                "create dataflow MyDataFlowOne " +
                "SourceOne -> outstream<SomeType> {}" +
                "SourceTwo -> outstream<SomeType> {}" +
                "Future(outstream) {}");

            // instantiate
            var latchOne = new CountDownLatch(1);
            var latchTwo = new CountDownLatch(1);
            IDictionary<String, Object> ops = new Dictionary<String, Object>();
            ops.Put(
                "SourceOne", new DefaultSupportSourceOp(
                                 new Object[]
                                 {
                                     latchOne, new Object[]
                                     {
                                         1
                                     }
                                 }));
            ops.Put(
                "SourceTwo", new DefaultSupportSourceOp(
                                 new Object[]
                                 {
                                     latchTwo, new Object[]
                                     {
                                         1
                                     }
                                 }));
            var future = new DefaultSupportCaptureOp(2);
            ops["Future"] = future;

            EPDataFlowInstantiationOptions options =
                new EPDataFlowInstantiationOptions().OperatorProvider(new DefaultSupportGraphOpProviderByOpName(ops));
            EPDataFlowInstance dfOne = _epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlowOne", options);

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
        }

        [Test]
        public void TestBlockingRunJoin()
        {
            // declare
            _epService.EPAdministrator.CreateEPL(
                "create dataflow MyDataFlowOne " +
                "DefaultSupportSourceOp -> s<SomeType> {}" +
                "DefaultSupportCaptureOp(s) {}");

            // instantiate
            var latch = new CountDownLatch(1);
            var source = new DefaultSupportSourceOp(
                new Object[]
                {
                    latch, new Object[]
                    {
                        1
                    }
                });
            var future = new DefaultSupportCaptureOp(1);
            EPDataFlowInstantiationOptions options =
                new EPDataFlowInstantiationOptions().OperatorProvider(new DefaultSupportGraphOpProvider(source, future));
            EPDataFlowInstance dfOne = _epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlowOne", options);
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
            long deltaJoin = joiningRunnable.End - joiningRunnable.Start;
            Assert.That(deltaJoin, Is.GreaterThanOrEqualTo(500));
        }

        [Test]
        public void TestFastCompleteBlocking()
        {
            // declare
            _epService.EPAdministrator.CreateEPL(
                "create dataflow MyDataFlowOne " +
                "BeaconSource -> BeaconStream {iterations : 1}" +
                "DefaultSupportCaptureOp(BeaconStream) {}");

            // instantiate
            var future = new DefaultSupportCaptureOp(1);
            EPDataFlowInstantiationOptions options =
                new EPDataFlowInstantiationOptions().OperatorProvider(new DefaultSupportGraphOpProvider(future));
            EPDataFlowInstance dfOne = _epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlowOne", options);
            Assert.AreEqual("MyDataFlowOne", dfOne.DataFlowName);
            Assert.AreEqual(EPDataFlowState.INSTANTIATED, dfOne.State);

            // has not run
            Thread.Sleep(1000);
            Assert.IsFalse(future.IsDone());

            // blocking run
            dfOne.Run();
            Assert.AreEqual(EPDataFlowState.COMPLETE, dfOne.State);
            Assert.AreEqual(1, future.GetValue(TimeSpan.MaxValue).Length);

            // assert past-exec
            RunAssertionAfterExec(dfOne);
        }

        [Test]
        public void TestFastCompleteNonBlocking()
        {
            // declare
            _epService.EPAdministrator.CreateEPL(
                "create dataflow MyDataFlowOne " +
                "BeaconSource -> BeaconStream {iterations : 1}" +
                "DefaultSupportCaptureOp(BeaconStream) {}");

            // instantiate
            var future = new DefaultSupportCaptureOp(1);
            EPDataFlowInstantiationOptions options =
                new EPDataFlowInstantiationOptions().OperatorProvider(new DefaultSupportGraphOpProvider(future));
            EPDataFlowInstance dfOne = _epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlowOne", options);
            Assert.AreEqual("MyDataFlowOne", dfOne.DataFlowName);
            Assert.AreEqual(EPDataFlowState.INSTANTIATED, dfOne.State);
            Assert.IsFalse(future.IsDone());

            // non-blocking run, spinning wait
            dfOne.Start();
            long start = Environment.TickCount;
            while (dfOne.State != EPDataFlowState.COMPLETE)
            {
                if (Environment.TickCount - start > 1000)
                {
                    Assert.Fail();
                }
            }
            Assert.AreEqual(1, future.GetValue(TimeSpan.MaxValue).Length);

            // assert past-exec
            RunAssertionAfterExec(dfOne);
        }

        [Test]
        public void TestInvalidJoinRun()
        {
            _epService.EPAdministrator.CreateEPL(
                "create dataflow MyDataFlowOne " +
                "BeaconSource -> BeaconStream {iterations : 1}");

            var source = new DefaultSupportSourceOp(
                new Object[]
                {
                    5000
                });
            EPDataFlowInstantiationOptions options =
                new EPDataFlowInstantiationOptions().OperatorProvider(new DefaultSupportGraphOpProvider(source));
            EPDataFlowInstance dfOne = _epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlowOne", options);

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
        }

        [Test]
        public void TestNonBlockingCancel()
        {
            // declare
            _epService.EPAdministrator.CreateEPL(
                "create dataflow MyDataFlowOne " +
                "SourceOne -> outstream<SomeType> {}" +
                "OutputOp(outstream) {}");

            // instantiate
            var latchOne = new CountDownLatch(1);
            IDictionary<String, Object> ops = new Dictionary<String, Object>();
            ops.Put(
                "SourceOne", new DefaultSupportSourceOp(
                                 new Object[]
                                 {
                                     latchOne, new Object[]
                                     {
                                         1
                                     }
                                 }));
            var output = new DefaultSupportCaptureOp();
            ops["OutputOp"] = output;

            EPDataFlowInstantiationOptions options =
                new EPDataFlowInstantiationOptions().OperatorProvider(new DefaultSupportGraphOpProviderByOpName(ops));
            EPDataFlowInstance dfOne = _epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlowOne", options);

            dfOne.Start();
            Assert.AreEqual(EPDataFlowState.RUNNING, dfOne.State);

            dfOne.Cancel();

            latchOne.CountDown();
            Thread.Sleep(100);
            Assert.AreEqual(EPDataFlowState.CANCELLED, dfOne.State);
            Assert.AreEqual(0, output.GetAndReset().Count);
        }

        [Test]
        public void TestNonBlockingException()
        {
            _epService.EPAdministrator.CreateEPL(
                "create dataflow MyDataFlowOne " +
                "DefaultSupportSourceOp -> outstream<SomeType> {}" +
                "DefaultSupportCaptureOp(outstream) {}");

            var src = new DefaultSupportSourceOp(
                new Object[]
                {
                    new MyRuntimeException("TestException")
                });
            var output = new DefaultSupportCaptureOp();
            EPDataFlowInstantiationOptions options =
                new EPDataFlowInstantiationOptions().OperatorProvider(new DefaultSupportGraphOpProvider(src, output));
            EPDataFlowInstance dfOne = _epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlowOne", options);

            dfOne.Start();
            Thread.Sleep(200);
            Assert.AreEqual(EPDataFlowState.COMPLETE, dfOne.State);
            Assert.AreEqual(0, output.GetAndReset().Count);
        }

        [Test]
        public void TestNonBlockingJoinCancel()
        {
            _epService.EPAdministrator.CreateEPL(
                "create dataflow MyDataFlowOne " +
                "DefaultSupportSourceOp -> outstream<SomeType> {}" +
                "DefaultSupportCaptureOp(outstream) {}");

            var latchOne = new CountDownLatch(1);
            var src = new DefaultSupportSourceOp(
                new Object[]
                {
                    latchOne
                });
            var output = new DefaultSupportCaptureOp();
            EPDataFlowInstantiationOptions options =
                new EPDataFlowInstantiationOptions().OperatorProvider(new DefaultSupportGraphOpProvider(src, output));
            EPDataFlowInstance dfOne = _epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlowOne", options);

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
        }

        [Test]
        public void TestNonBlockingJoinException()
        {
            _epService.EPAdministrator.CreateEPL(
                "create dataflow MyDataFlowOne " +
                "DefaultSupportSourceOp -> outstream<SomeType> {}" +
                "DefaultSupportCaptureOp(outstream) {}");

            var latchOne = new CountDownLatch(1);
            var src = new DefaultSupportSourceOp(
                new Object[]
                {
                    latchOne, new MyRuntimeException("TestException")
                });
            var output = new DefaultSupportCaptureOp();
            EPDataFlowInstantiationOptions options =
                new EPDataFlowInstantiationOptions().OperatorProvider(new DefaultSupportGraphOpProvider(src, output));
            EPDataFlowInstance dfOne = _epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlowOne", options);

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
        }

        [Test]
        public void TestNonBlockingJoinMultipleRunnable()
        {
            // declare
            _epService.EPAdministrator.CreateEPL(
                "create dataflow MyDataFlowOne " +
                "SourceOne -> outstream<SomeType> {}" +
                "SourceTwo -> outstream<SomeType> {}" +
                "Future(outstream) {}");

            // instantiate
            var latchOne = new CountDownLatch(1);
            var latchTwo = new CountDownLatch(1);
            IDictionary<String, Object> ops = new Dictionary<String, Object>();
            ops.Put(
                "SourceOne", new DefaultSupportSourceOp(
                                 new Object[]
                                 {
                                     latchOne, new Object[]
                                     {
                                         1
                                     }
                                 }));
            ops.Put(
                "SourceTwo", new DefaultSupportSourceOp(
                                 new Object[]
                                 {
                                     latchTwo, new Object[]
                                     {
                                         1
                                     }
                                 }));
            var future = new DefaultSupportCaptureOp(2);
            ops["Future"] = future;

            EPDataFlowInstantiationOptions options =
                new EPDataFlowInstantiationOptions().OperatorProvider(new DefaultSupportGraphOpProviderByOpName(ops));
            EPDataFlowInstance dfOne = _epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlowOne", options);

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
        }

        [Test]
        public void TestNonBlockingJoinSingleRunnable()
        {
            // declare
            _epService.EPAdministrator.CreateEPL(
                "create dataflow MyDataFlowOne " +
                "DefaultSupportSourceOp -> outstream<SomeType> {}" +
                "DefaultSupportCaptureOp(outstream) {}");

            // instantiate
            var latch = new CountDownLatch(1);
            var source = new DefaultSupportSourceOp(
                new Object[]
                {
                    latch, new Object[]
                    {
                        1
                    }
                });
            var future = new DefaultSupportCaptureOp(1);
            EPDataFlowInstantiationOptions options =
                new EPDataFlowInstantiationOptions().OperatorProvider(new DefaultSupportGraphOpProvider(source, future));
            EPDataFlowInstance dfOne = _epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlowOne", options);
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
        }

        [Test]
        public void TestRunBlocking()
        {
            // declare
            _epService.EPAdministrator.CreateEPL(
                "create dataflow MyDataFlowOne " +
                "DefaultSupportSourceOp -> s<SomeType> {}" +
                "DefaultSupportCaptureOp(s) {}");

            // instantiate
            var latch = new CountDownLatch(1);
            var source = new DefaultSupportSourceOp(
                new Object[]
                {
                    latch, new Object[]
                    {
                        1
                    }
                });
            var future = new DefaultSupportCaptureOp(1);
            EPDataFlowInstantiationOptions options =
                new EPDataFlowInstantiationOptions().OperatorProvider(new DefaultSupportGraphOpProvider(future, source));
            EPDataFlowInstance dfOne = _epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlowOne", options);
            Assert.AreEqual("MyDataFlowOne", dfOne.DataFlowName);
            Assert.AreEqual(EPDataFlowState.INSTANTIATED, dfOne.State);

            var unlatchingThread = new Thread(
                () =>
                {
                    try
                    {
                        while (dfOne.State != EPDataFlowState.RUNNING)
                        {
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
        }

        public class MyJoiningRunnable : IRunnable
        {
            private readonly EPDataFlowInstance _instance;

            public MyJoiningRunnable(EPDataFlowInstance instance)
            {
                _instance = instance;
            }

            public long Start { get; private set; }

            public long End { get; private set; }

            #region IRunnable Members

            public void Run()
            {
                while (_instance.State != EPDataFlowState.RUNNING)
                {
                }

                Start = Environment.TickCount;
                _instance.Join();
                End = Environment.TickCount;
            }

            #endregion
        }

        public class MyRuntimeException : Exception
        {
            public MyRuntimeException(String message)
                : base(message)
            {
            }
        }
    }
}
