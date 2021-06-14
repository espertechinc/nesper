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

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.dataflow.core;
using com.espertech.esper.common.@internal.epl.dataflow.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading;
using com.espertech.esper.container;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.support.epl.SupportStaticMethodLib;

namespace com.espertech.esper.regressionlib.suite.epl.dataflow
{
    public class EPLDataflowAPIRunStartCancelJoin
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
WithNonBlockingJoinCancel(execs);
WithNonBlockingJoinException(execs);
WithNonBlockingException(execs);
WithBlockingException(execs);
WithBlockingCancel(execs);
WithNonBlockingCancel(execs);
WithInvalidJoinRun(execs);
WithNonBlockingJoinMultipleRunnable(execs);
WithBlockingMultipleRunnable(execs);
WithNonBlockingJoinSingleRunnable(execs);
WithFastCompleteBlocking(execs);
WithRunBlocking(execs);
WithFastCompleteNonBlocking(execs);
WithBlockingRunJoin(execs);
            return execs;
        }
public static IList<RegressionExecution> WithBlockingRunJoin(IList<RegressionExecution> execs = null)
{
    execs = execs ?? new List<RegressionExecution>();
    execs.Add(new EPLDataflowBlockingRunJoin());
    return execs;
}public static IList<RegressionExecution> WithFastCompleteNonBlocking(IList<RegressionExecution> execs = null)
{
    execs = execs ?? new List<RegressionExecution>();
    execs.Add(new EPLDataflowFastCompleteNonBlocking());
    return execs;
}public static IList<RegressionExecution> WithRunBlocking(IList<RegressionExecution> execs = null)
{
    execs = execs ?? new List<RegressionExecution>();
    execs.Add(new EPLDataflowRunBlocking());
    return execs;
}public static IList<RegressionExecution> WithFastCompleteBlocking(IList<RegressionExecution> execs = null)
{
    execs = execs ?? new List<RegressionExecution>();
    execs.Add(new EPLDataflowFastCompleteBlocking());
    return execs;
}public static IList<RegressionExecution> WithNonBlockingJoinSingleRunnable(IList<RegressionExecution> execs = null)
{
    execs = execs ?? new List<RegressionExecution>();
    execs.Add(new EPLDataflowNonBlockingJoinSingleRunnable());
    return execs;
}public static IList<RegressionExecution> WithBlockingMultipleRunnable(IList<RegressionExecution> execs = null)
{
    execs = execs ?? new List<RegressionExecution>();
    execs.Add(new EPLDataflowBlockingMultipleRunnable());
    return execs;
}public static IList<RegressionExecution> WithNonBlockingJoinMultipleRunnable(IList<RegressionExecution> execs = null)
{
    execs = execs ?? new List<RegressionExecution>();
    execs.Add(new EPLDataflowNonBlockingJoinMultipleRunnable());
    return execs;
}public static IList<RegressionExecution> WithInvalidJoinRun(IList<RegressionExecution> execs = null)
{
    execs = execs ?? new List<RegressionExecution>();
    execs.Add(new EPLDataflowInvalidJoinRun());
    return execs;
}public static IList<RegressionExecution> WithNonBlockingCancel(IList<RegressionExecution> execs = null)
{
    execs = execs ?? new List<RegressionExecution>();
    execs.Add(new EPLDataflowNonBlockingCancel());
    return execs;
}public static IList<RegressionExecution> WithBlockingCancel(IList<RegressionExecution> execs = null)
{
    execs = execs ?? new List<RegressionExecution>();
    execs.Add(new EPLDataflowBlockingCancel());
    return execs;
}public static IList<RegressionExecution> WithBlockingException(IList<RegressionExecution> execs = null)
{
    execs = execs ?? new List<RegressionExecution>();
    execs.Add(new EPLDataflowBlockingException());
    return execs;
}public static IList<RegressionExecution> WithNonBlockingException(IList<RegressionExecution> execs = null)
{
    execs = execs ?? new List<RegressionExecution>();
    execs.Add(new EPLDataflowNonBlockingException());
    return execs;
}public static IList<RegressionExecution> WithNonBlockingJoinException(IList<RegressionExecution> execs = null)
{
    execs = execs ?? new List<RegressionExecution>();
    execs.Add(new EPLDataflowNonBlockingJoinException());
    return execs;
}public static IList<RegressionExecution> WithNonBlockingJoinCancel(IList<RegressionExecution> execs = null)
{
    execs = execs ?? new List<RegressionExecution>();
    execs.Add(new EPLDataflowNonBlockingJoinCancel());
    return execs;
}
        private static void TryAssertionAfterExec(EPDataFlowInstance df)
        {
            // cancel and join ignored
            try {
                df.Join();
            }
            catch (ThreadInterruptedException e) {
                throw new EPException(e);
            }

            // can't start or run again
            try {
                df.Run();
                Assert.Fail();
            }
            catch (IllegalStateException ex) {
                Assert.AreEqual(
                    "Data flow 'MyDataFlowOne' instance has already completed, please use instantiate to run the data flow again",
                    ex.Message);
            }

            try {
                df.Start();
                Assert.Fail();
            }
            catch (IllegalStateException ex) {
                Assert.AreEqual(
                    "Data flow 'MyDataFlowOne' instance has already completed, please use instantiate to run the data flow again",
                    ex.Message);
            }

            df.Cancel();
            try {
                df.Join();
            }
            catch (ThreadInterruptedException e) {
                throw new EPException(e);
            }
        }

        internal class EPLDataflowNonBlockingJoinCancel : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("create schema SomeType ()", path);
                env.CompileDeploy(
                    "@Name('flow') create dataflow MyDataFlowOne " +
                    "DefaultSupportSourceOp -> outstream<SomeType> {}" +
                    "DefaultSupportCaptureOp(outstream) {}",
                    path);

                var latchOne = new CountDownLatch(1);
                var src = new DefaultSupportSourceOp(new object[] {latchOne});
                var output = new DefaultSupportCaptureOp();
                var options =
                    new EPDataFlowInstantiationOptions().WithOperatorProvider(
                        new DefaultSupportGraphOpProvider(src, output));
                var dfOne = env.Runtime.DataFlowService.Instantiate(env.DeploymentId("flow"), "MyDataFlowOne", options);

                dfOne.Start();

                var cancellingThread = new Thread(
                    () => {
                        try {
                            Thread.Sleep(300);
                            dfOne.Cancel();
                        }
                        catch (Exception e) {
                            log.Error("Unexpected exception", e);
                        }
                    });
                cancellingThread.Name = GetType().Name + "-cancelling";
                cancellingThread.Start();
                try {
                    dfOne.Join();
                }
                catch (ThreadInterruptedException e) {
                    throw new EPException(e);
                }

                Assert.AreEqual(EPDataFlowState.CANCELLED, dfOne.State);
                Assert.AreEqual(0, output.GetAndReset().Count);

                env.UndeployAll();
            }
        }

        internal class EPLDataflowNonBlockingJoinException : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("create schema SomeType ()", path);
                env.CompileDeploy(
                    "@Name('flow') create dataflow MyDataFlowOne " +
                    "DefaultSupportSourceOp -> outstream<SomeType> {}" +
                    "DefaultSupportCaptureOp(outstream) {}",
                    path);

                var latchOne = new CountDownLatch(1);
                var src = new DefaultSupportSourceOp(new object[] {latchOne, new MyException("TestException")});
                var output = new DefaultSupportCaptureOp();
                var options =
                    new EPDataFlowInstantiationOptions().WithOperatorProvider(
                        new DefaultSupportGraphOpProvider(src, output));
                var dfOne = env.Runtime.DataFlowService.Instantiate(env.DeploymentId("flow"), "MyDataFlowOne", options);

                dfOne.Start();

                var unlatchingThread = new Thread(
                    () => {
                        try {
                            Thread.Sleep(300);
                            latchOne.CountDown();
                        }
                        catch (Exception e) {
                            log.Error("Unexpected exception", e);
                        }
                    });
                unlatchingThread.Name = GetType().Name + "-unlatching";
                unlatchingThread.Start();

                try {
                    dfOne.Join();
                }
                catch (ThreadInterruptedException e) {
                    throw new EPException(e);
                }

                Assert.AreEqual(EPDataFlowState.COMPLETE, dfOne.State);
                Assert.AreEqual(0, output.GetAndReset().Count);
                env.UndeployAll();
            }
        }

        internal class EPLDataflowNonBlockingException : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("create schema SomeType ()", path);
                env.CompileDeploy(
                    "@Name('flow') create dataflow MyDataFlowOne " +
                    "DefaultSupportSourceOp -> outstream<SomeType> {}" +
                    "DefaultSupportCaptureOp(outstream) {}",
                    path);

                var src = new DefaultSupportSourceOp(new object[] {new MyException("TestException")});
                var output = new DefaultSupportCaptureOp();
                var options =
                    new EPDataFlowInstantiationOptions().WithOperatorProvider(
                        new DefaultSupportGraphOpProvider(src, output));
                var dfOne = env.Runtime.DataFlowService.Instantiate(env.DeploymentId("flow"), "MyDataFlowOne", options);

                dfOne.Start();
                Sleep(200);
                Assert.AreEqual(EPDataFlowState.COMPLETE, dfOne.State);
                Assert.AreEqual(0, output.GetAndReset().Count);
                env.UndeployAll();
            }
        }

        internal class EPLDataflowBlockingException : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("create schema SomeType ()", path);
                env.CompileDeploy(
                    "@Name('flow') create dataflow MyDataFlowOne " +
                    "DefaultSupportSourceOp -> outstream<SomeType> {}" +
                    "DefaultSupportCaptureOp(outstream) {}",
                    path);

                var src = new DefaultSupportSourceOp(new object[] {new MyException("TestException")});
                var output = new DefaultSupportCaptureOp();
                var options =
                    new EPDataFlowInstantiationOptions().WithOperatorProvider(
                        new DefaultSupportGraphOpProvider(src, output));
                var dfOne = env.Runtime.DataFlowService.Instantiate(env.DeploymentId("flow"), "MyDataFlowOne", options);

                try {
                    dfOne.Run();
                    Assert.Fail();
                }
                catch (EPDataFlowExecutionException ex) {
                    Assert.IsTrue(ex.InnerException.InnerException is MyException);
                    Assert.AreEqual(
                        "Support-graph-source generated exception: TestException",
                        ex.InnerException.Message);
                }

                Assert.AreEqual(EPDataFlowState.COMPLETE, dfOne.State);
                Assert.AreEqual(0, output.GetAndReset().Count);
                env.UndeployAll();
            }
        }

        internal class EPLDataflowBlockingCancel : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // declare
                var path = new RegressionPath();
                env.CompileDeploy("create schema SomeType ()", path);
                env.CompileDeploy(
                    "@Name('flow') create dataflow MyDataFlowOne " +
                    "DefaultSupportSourceOp -> outstream<SomeType> {}" +
                    "DefaultSupportCaptureOp(outstream) {}",
                    path);

                // instantiate
                var latchOne = new CountDownLatch(1);
                IDictionary<string, object> ops = new Dictionary<string, object>();
                ops.Put(
                    "DefaultSupportSourceOp",
                    new DefaultSupportSourceOp(
                        new object[] {
                            latchOne,
                            new object[] {1}
                        }));
                var output = new DefaultSupportCaptureOp();
                ops.Put("DefaultSupportCaptureOp", output);

                var options =
                    new EPDataFlowInstantiationOptions().WithOperatorProvider(
                        new DefaultSupportGraphOpProviderByOpName(ops));
                var dfOne = env.Runtime.DataFlowService.Instantiate(env.DeploymentId("flow"), "MyDataFlowOne", options);

                var cancellingThread = new Thread(
                    () => {
                        try {
                            Thread.Sleep(300);
                            dfOne.Cancel();
                        }
                        catch (Exception e) {
                            log.Error("Unexpected exception", e);
                        }
                    });
                cancellingThread.Name = GetType().Name + "-cancelling";
                cancellingThread.Start();

                try {
                    dfOne.Run();
                    Assert.Fail();
                }
                catch (EPDataFlowCancellationException ex) {
                    Assert.AreEqual("Data flow 'MyDataFlowOne' execution was cancelled", ex.Message);
                }

                Assert.AreEqual(EPDataFlowState.CANCELLED, dfOne.State);
                Assert.AreEqual(0, output.GetAndReset().Count);
                env.UndeployAll();
            }
        }

        internal class EPLDataflowNonBlockingCancel : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // declare
                var path = new RegressionPath();
                env.CompileDeploy("create schema SomeType ()", path);
                env.CompileDeploy(
                    "@Name('flow') create dataflow MyDataFlowOne " +
                    "DefaultSupportSourceOp -> outstream<SomeType> {}" +
                    "DefaultSupportCaptureOp(outstream) {}",
                    path);

                // instantiate
                var latchOne = new CountDownLatch(1);
                IDictionary<string, object> ops = new Dictionary<string, object>();
                ops.Put(
                    "DefaultSupportSourceOp",
                    new DefaultSupportSourceOp(
                        new object[] {
                            latchOne,
                            new object[] {1}
                        }));
                var output = new DefaultSupportCaptureOp();
                ops.Put("DefaultSupportCaptureOp", output);

                var options =
                    new EPDataFlowInstantiationOptions().WithOperatorProvider(
                        new DefaultSupportGraphOpProviderByOpName(ops));
                var dfOne = env.Runtime.DataFlowService.Instantiate(env.DeploymentId("flow"), "MyDataFlowOne", options);

                dfOne.Start();
                Assert.AreEqual(EPDataFlowState.RUNNING, dfOne.State);

                dfOne.Cancel();

                latchOne.CountDown();
                Sleep(100);
                Assert.AreEqual(EPDataFlowState.CANCELLED, dfOne.State);
                Assert.AreEqual(0, output.GetAndReset().Count);
                env.UndeployAll();
            }
        }

        internal class EPLDataflowInvalidJoinRun : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy(
                    "@Name('flow') create dataflow MyDataFlowOne " +
                    "BeaconSource -> BeaconStream {iterations : 1}");

                var source = new DefaultSupportSourceOp(new object[] {5000});
                var options =
                    new EPDataFlowInstantiationOptions().WithOperatorProvider(
                        new DefaultSupportGraphOpProvider(source));
                var dfOne = env.Runtime.DataFlowService.Instantiate(env.DeploymentId("flow"), "MyDataFlowOne", options);

                // invalid join
                try {
                    dfOne.Join();
                    Assert.Fail();
                }
                catch (IllegalStateException ex) {
                    Assert.AreEqual(
                        "Data flow 'MyDataFlowOne' instance has not been executed, please use join after start or run",
                        ex.Message);
                }
                catch (ThreadInterruptedException ex) {
                    throw new EPException(ex);
                }

                // cancel
                dfOne.Cancel();

                // invalid run and start
                try {
                    dfOne.Run();
                    Assert.Fail();
                }
                catch (IllegalStateException ex) {
                    Assert.AreEqual(
                        "Data flow 'MyDataFlowOne' instance has been cancelled and cannot be run or started",
                        ex.Message);
                }

                try {
                    dfOne.Start();
                    Assert.Fail();
                }
                catch (IllegalStateException ex) {
                    Assert.AreEqual(
                        "Data flow 'MyDataFlowOne' instance has been cancelled and cannot be run or started",
                        ex.Message);
                }

                // cancel again
                dfOne.Cancel();
                env.UndeployAll();
            }
        }

        internal class EPLDataflowNonBlockingJoinMultipleRunnable : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // declare
                var path = new RegressionPath();
                env.CompileDeploy("create schema SomeType ()", path);
                env.CompileDeploy(
                    "@Name('flow') create dataflow MyDataFlowOne " +
                    "DefaultSupportSourceOp -> outstream<SomeType> { name: 'SourceOne' }" +
                    "DefaultSupportSourceOp -> outstream<SomeType> { name: 'SourceTwo' }" +
                    "DefaultSupportCaptureOp(outstream) {}",
                    path);

                // instantiate
                var latchOne = new CountDownLatch(1);
                var latchTwo = new CountDownLatch(1);
                IDictionary<string, object> ops = new Dictionary<string, object>();
                ops.Put(
                    "SourceOne",
                    new DefaultSupportSourceOp(
                        new object[] {
                            latchOne,
                            new object[] {1}
                        }));
                ops.Put(
                    "SourceTwo",
                    new DefaultSupportSourceOp(
                        new object[] {
                            latchTwo,
                            new object[] {1}
                        }));
                var future = new DefaultSupportCaptureOp(2, env.Container.LockManager());
                ops.Put("DefaultSupportCaptureOp", future);

                var options =
                    new EPDataFlowInstantiationOptions().WithOperatorProvider(
                        new DefaultSupportGraphOpProviderByOpName(ops));
                var dfOne = env.Runtime.DataFlowService.Instantiate(env.DeploymentId("flow"), "MyDataFlowOne", options);

                dfOne.Start();
                Sleep(50);
                Assert.AreEqual(EPDataFlowState.RUNNING, dfOne.State);

                latchOne.CountDown();
                Sleep(200);
                Assert.AreEqual(EPDataFlowState.RUNNING, dfOne.State);

                latchTwo.CountDown();
                try {
                    dfOne.Join();
                }
                catch (ThreadInterruptedException e) {
                    throw new EPException(e);
                }

                Assert.AreEqual(EPDataFlowState.COMPLETE, dfOne.State);
                Assert.AreEqual(2, future.GetAndReset().Count);
                env.UndeployAll();
            }
        }

        internal class EPLDataflowBlockingMultipleRunnable : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // declare
                var path = new RegressionPath();
                env.CompileDeploy("create schema SomeType ()", path);
                env.CompileDeploy(
                    "@Name('flow') create dataflow MyDataFlowOne " +
                    "DefaultSupportSourceOp -> outstream<SomeType> {name: 'SourceOne'}" +
                    "DefaultSupportSourceOp -> outstream<SomeType> {name: 'SourceTwo'}" +
                    "DefaultSupportCaptureOp(outstream) {}",
                    path);

                // instantiate
                var latchOne = new CountDownLatch(1);
                var latchTwo = new CountDownLatch(1);
                IDictionary<string, object> ops = new Dictionary<string, object>();
                ops.Put(
                    "SourceOne",
                    new DefaultSupportSourceOp(
                        new object[] {
                            latchOne,
                            new object[] {1}
                        }));
                ops.Put(
                    "SourceTwo",
                    new DefaultSupportSourceOp(
                        new object[] {
                            latchTwo,
                            new object[] {1}
                        }));
                var future = new DefaultSupportCaptureOp(2, env.Container.LockManager());
                ops.Put("DefaultSupportCaptureOp", future);

                var options =
                    new EPDataFlowInstantiationOptions().WithOperatorProvider(
                        new DefaultSupportGraphOpProviderByOpName(ops));
                var dfOne = env.Runtime.DataFlowService.Instantiate(env.DeploymentId("flow"), "MyDataFlowOne", options);

                try {
                    dfOne.Run();
                    Assert.Fail();
                }
                catch (UnsupportedOperationException ex) {
                    Assert.AreEqual(
                        "The data flow 'MyDataFlowOne' has zero or multiple sources and requires the use of the start method instead",
                        ex.Message);
                }

                latchTwo.CountDown();
                dfOne.Start();
                latchOne.CountDown();
                try {
                    dfOne.Join();
                }
                catch (ThreadInterruptedException e) {
                    throw new EPException(e);
                }

                Assert.AreEqual(EPDataFlowState.COMPLETE, dfOne.State);
                Assert.AreEqual(2, future.GetAndReset().Count);
                env.UndeployAll();
            }
        }

        internal class EPLDataflowNonBlockingJoinSingleRunnable : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // declare
                var path = new RegressionPath();
                env.CompileDeploy("create schema SomeType ()", path);
                env.CompileDeploy(
                    "@Name('flow') create dataflow MyDataFlowOne " +
                    "DefaultSupportSourceOp -> outstream<SomeType> {}" +
                    "DefaultSupportCaptureOp(outstream) {}",
                    path);

                // instantiate
                var latch = new CountDownLatch(1);
                var source = new DefaultSupportSourceOp(
                    new object[] {
                        latch,
                        new object[] {1}
                    });
                var future = new DefaultSupportCaptureOp(1, env.Container.LockManager());
                var options =
                    new EPDataFlowInstantiationOptions().WithOperatorProvider(
                        new DefaultSupportGraphOpProvider(source, future));
                var dfOne = env.Runtime.DataFlowService.Instantiate(env.DeploymentId("flow"), "MyDataFlowOne", options);
                Assert.AreEqual("MyDataFlowOne", dfOne.DataFlowName);
                Assert.AreEqual(EPDataFlowState.INSTANTIATED, dfOne.State);

                dfOne.Start();
                Sleep(100);
                Assert.AreEqual(EPDataFlowState.RUNNING, dfOne.State);

                latch.CountDown();
                try {
                    dfOne.Join();
                }
                catch (ThreadInterruptedException e) {
                    throw new EPException(e);
                }

                Assert.AreEqual(EPDataFlowState.COMPLETE, dfOne.State);
                Assert.AreEqual(1, future.GetAndReset()[0].Count);
                Assert.AreEqual(2, source.CurrentCount);

                dfOne.Cancel();
                Assert.AreEqual(EPDataFlowState.COMPLETE, dfOne.State);
                env.UndeployAll();
            }
        }

        internal class EPLDataflowBlockingRunJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // declare
                var path = new RegressionPath();
                env.CompileDeploy("create schema SomeType ()", path);
                env.CompileDeploy(
                    "@Name('flow') create dataflow MyDataFlowOne " +
                    "DefaultSupportSourceOp -> s<SomeType> {}" +
                    "DefaultSupportCaptureOp(s) {}",
                    path);

                // instantiate
                var latch = new CountDownLatch(1);
                var source = new DefaultSupportSourceOp(
                    new object[] {
                        latch,
                        new object[] {1}
                    });
                var future = new DefaultSupportCaptureOp(1, env.Container.LockManager());
                var options =
                    new EPDataFlowInstantiationOptions().WithOperatorProvider(
                        new DefaultSupportGraphOpProvider(source, future));
                var dfOne = env.Runtime.DataFlowService.Instantiate(env.DeploymentId("flow"), "MyDataFlowOne", options);
                Assert.AreEqual("MyDataFlowOne", dfOne.DataFlowName);
                Assert.AreEqual(EPDataFlowState.INSTANTIATED, dfOne.State);

                var joiningRunnable = new MyJoiningRunnable(dfOne);
                var joiningThread = new Thread(joiningRunnable.Run);
                joiningThread.Name = GetType().Name + "-joining";

                var unlatchingThread = new Thread(
                    () => {
                        try {
                            while (dfOne.State != EPDataFlowState.RUNNING) {
                                Thread.Sleep(10);
                            }

                            Thread.Sleep(1000);
                            latch.CountDown();
                        }
                        catch (Exception e) {
                            log.Error("Unexpected exception", e);
                        }
                    });
                unlatchingThread.Name = GetType().Name + "-unlatching";

                joiningThread.Start();
                unlatchingThread.Start();
                dfOne.Run();

                Assert.AreEqual(EPDataFlowState.COMPLETE, dfOne.State);
                Assert.AreEqual(1, future.GetAndReset()[0].Count);
                Assert.AreEqual(2, source.CurrentCount);

                try {
                    joiningThread.Join();
                    unlatchingThread.Join();
                }
                catch (ThreadInterruptedException e) {
                    throw new EPException(e);
                }

                var deltaJoin = joiningRunnable.End - joiningRunnable.Start;
                Assert.IsTrue(deltaJoin >= 500, "deltaJoin=" + deltaJoin);
                env.UndeployAll();
            }
        }

        internal class EPLDataflowFastCompleteBlocking : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // declare
                env.CompileDeploy(
                    "@Name('flow') create dataflow MyDataFlowOne " +
                    "BeaconSource -> BeaconStream {iterations : 1}" +
                    "DefaultSupportCaptureOp(BeaconStream) {}");

                // instantiate
                var future = new DefaultSupportCaptureOp(1, env.Container.LockManager());
                var options =
                    new EPDataFlowInstantiationOptions().WithOperatorProvider(
                        new DefaultSupportGraphOpProvider(future));
                var dfOne = env.Runtime.DataFlowService.Instantiate(env.DeploymentId("flow"), "MyDataFlowOne", options);
                Assert.AreEqual("MyDataFlowOne", dfOne.DataFlowName);
                Assert.AreEqual(EPDataFlowState.INSTANTIATED, dfOne.State);

                // has not run
                Sleep(1000);
                Assert.IsFalse(future.IsDone());

                // blocking run
                dfOne.Run();
                Assert.AreEqual(EPDataFlowState.COMPLETE, dfOne.State);
                try {
                    Assert.AreEqual(1, future.GetValueOrDefault().Length);
                }
                catch (Exception t) {
                    throw new EPException(t);
                }

                // assert past-exec
                TryAssertionAfterExec(dfOne);

                env.UndeployAll();
            }
        }

        internal class EPLDataflowRunBlocking : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // declare
                var path = new RegressionPath();
                env.CompileDeploy("create schema SomeType ()", path);
                env.CompileDeploy(
                    "@Name('flow') create dataflow MyDataFlowOne " +
                    "DefaultSupportSourceOp -> s<SomeType> {}" +
                    "DefaultSupportCaptureOp(s) {}",
                    path);

                // instantiate
                var latch = new CountDownLatch(1);
                var source = new DefaultSupportSourceOp(
                    new object[] {
                        latch,
                        new object[] {1}
                    });
                var future = new DefaultSupportCaptureOp(1, env.Container.LockManager());
                var options =
                    new EPDataFlowInstantiationOptions().WithOperatorProvider(
                        new DefaultSupportGraphOpProvider(future, source));
                var dfOne = env.Runtime.DataFlowService.Instantiate(env.DeploymentId("flow"), "MyDataFlowOne", options);
                Assert.AreEqual("MyDataFlowOne", dfOne.DataFlowName);
                Assert.AreEqual(EPDataFlowState.INSTANTIATED, dfOne.State);

                var unlatchingThread = new Thread(
                    () => {
                        try {
                            while (dfOne.State != EPDataFlowState.RUNNING) {
                                Thread.Sleep(0);
                            }

                            Thread.Sleep(100);
                            latch.CountDown();
                        }
                        catch (Exception e) {
                            log.Error("Unexpected exception", e);
                        }
                    });
                unlatchingThread.Name = GetType().Name + "-unlatching";

                // blocking run
                unlatchingThread.Start();
                dfOne.Run();
                Assert.AreEqual(EPDataFlowState.COMPLETE, dfOne.State);
                Assert.AreEqual(1, future.GetAndReset()[0].Count);
                Assert.AreEqual(2, source.CurrentCount);
                try {
                    unlatchingThread.Join();
                }
                catch (ThreadInterruptedException e) {
                    throw new EPException(e);
                }

                env.UndeployAll();
            }
        }

        internal class EPLDataflowFastCompleteNonBlocking : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // declare
                env.CompileDeploy(
                    "@Name('flow') create dataflow MyDataFlowOne " +
                    "BeaconSource -> BeaconStream {iterations : 1}" +
                    "DefaultSupportCaptureOp(BeaconStream) {}");

                // instantiate
                var future = new DefaultSupportCaptureOp(1, env.Container.LockManager());
                var options =
                    new EPDataFlowInstantiationOptions().WithOperatorProvider(
                        new DefaultSupportGraphOpProvider(future));
                var dfOne = env.Runtime.DataFlowService.Instantiate(env.DeploymentId("flow"), "MyDataFlowOne", options);
                Assert.AreEqual("MyDataFlowOne", dfOne.DataFlowName);
                Assert.AreEqual(EPDataFlowState.INSTANTIATED, dfOne.State);
                Assert.IsFalse(future.IsDone());

                // non-blocking run, spinning wait
                dfOne.Start();
                var start = PerformanceObserver.MilliTime;
                while (dfOne.State != EPDataFlowState.COMPLETE) {
                    if (PerformanceObserver.MilliTime - start > 1000) {
                        Assert.Fail();
                    }
                }

                try {
                    Assert.AreEqual(1, future.GetValueOrDefault().Length);
                }
                catch (Exception t) {
                    throw new EPException(t);
                }

                // assert past-exec
                TryAssertionAfterExec(dfOne);
                env.UndeployAll();
            }
        }

        public class MyJoiningRunnable : IRunnable
        {
            private readonly EPDataFlowInstance instance;

            public MyJoiningRunnable(EPDataFlowInstance instance)
            {
                this.instance = instance;
            }

            public long Start { get; private set; }

            public long End { get; private set; }

            public void Run()
            {
                try {
                    while (instance.State != EPDataFlowState.RUNNING) {
                        Thread.Sleep(0);
                    }

                    Start = PerformanceObserver.MilliTime;
                    instance.Join();
                    End = PerformanceObserver.MilliTime;
                }
                catch (ThreadInterruptedException e) {
                    log.Error("Unexpected exception", e);
                }
            }
        }

        public class MyException : EPRuntimeException
        {
            public MyException(string message) : base(message)
            {
            }
        }
    }
} // end of namespace