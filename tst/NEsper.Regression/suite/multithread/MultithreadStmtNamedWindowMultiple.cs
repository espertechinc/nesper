///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Threading;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.client;
using com.espertech.esper.runtime.client;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.multithread
{
    /// <summary>
    ///     Test for multithread-safety for a simple aggregation case using count(*).
    /// </summary>
    public class MultithreadStmtNamedWindowMultiple : RegressionExecution
    {
        public ISet<RegressionFlag> Flags()
        {
            return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.MULTITHREADED);
        }

        public void Run(RegressionEnvironment env)
        {
            TryCount(env, 10, 500, 3);
        }

        public void TryCount(
            RegressionEnvironment env,
            int numUsers,
            int numOrders,
            int numThreads)
        {
            var path = new RegressionPath();
            for (var i = 0; i < numUsers; i++) {
                env.CompileDeploy(
                    $"@name('create_{i}') @public create window MyWindow_{i}#unique(OrderId) as select * from OrderEvent",
                    path);
                env.CompileDeploy(
                    $"@name('insert_{i}') insert into MyWindow_{i} select * from OrderEvent(UserId = 'user{i}')",
                    path);
                env.CompileDeploy(
                    $"on OrderCancelEvent as d delete from MyWindow_{i} w where w.OrderId = d.OrderId",
                    path);
                env.CompileDeploy(
                    $"@name('select_{i}') on OrderEvent as s select sum(w.Price) from MyWindow_{i} w where w.Side = s.Side group by w.Side",
                    path);
            }

            var runnables = new RunnableOrderSim[numThreads];
            var threads = new Thread[numThreads];
            for (var i = 0; i < numThreads; i++) {
                runnables[i] = new RunnableOrderSim(env.Runtime, i, numUsers, numOrders);
                threads[i] = new Thread(runnables[i].Run) {
                    Name = nameof(MultithreadStmtNamedWindowMultiple)
                };
            }

            for (var i = 0; i < threads.Length; i++) {
                threads[i].Start();
            }

            for (var i = 0; i < threads.Length; i++) {
                SupportCompileDeployUtil.ThreadJoin(threads[i]);
                ClassicAssert.IsTrue(runnables[i].Status);
            }

            env.UndeployAll();
        }

        public class RunnableOrderSim : IRunnable
        {
            private readonly int numOrders;
            private readonly int numUsers;
            private readonly Random random = new Random();
            private readonly EPRuntime runtime;
            private readonly int threadId;

            public RunnableOrderSim(
                EPRuntime runtime,
                int threadId,
                int numUsers,
                int numOrders)
            {
                this.runtime = runtime;
                this.threadId = threadId;
                this.numUsers = numUsers;
                this.numOrders = numOrders;
            }

            public bool Status { get; private set; }

            public void Run()
            {
                var orderIds = new string[10];
                for (var i = 0; i < orderIds.Length; i++) {
                    orderIds[i] = $"order_{i}_{threadId}";
                }

                for (var i = 0; i < numOrders; i++) {
                    if (random.Next() % 3 == 0) {
                        var orderId = orderIds[random.Next(orderIds.Length)];
                        for (var j = 0; j < numUsers; j++) {
                            var theEvent = new OrderCancelEvent($"user{j}", orderId);
                            runtime.EventService.SendEventBean(theEvent, theEvent.GetType().Name);
                        }
                    }
                    else {
                        var orderId = orderIds[random.Next(orderIds.Length)];
                        for (var j = 0; j < numUsers; j++) {
                            var theEvent = new OrderEvent($"user{j}", orderId, 1000, "B");
                            runtime.EventService.SendEventBean(theEvent, theEvent.GetType().Name);
                        }
                    }
                }

                Status = true;
            }
        }

        public class OrderEvent
        {
            public OrderEvent(
                string userId,
                string orderId,
                double price,
                string side)
            {
                UserId = userId;
                OrderId = orderId;
                Price = price;
                Side = side;
            }

            public string UserId { get; }

            public string OrderId { get; }

            public double Price { get; }

            public string Side { get; }
        }

        public class OrderCancelEvent
        {
            public OrderCancelEvent(
                string userId,
                string orderId)
            {
                UserId = userId;
                OrderId = orderId;
            }

            public string UserId { get; }

            public string OrderId { get; }
        }
    }
} // end of namespace