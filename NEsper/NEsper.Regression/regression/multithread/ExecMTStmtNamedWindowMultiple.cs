///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;

using com.espertech.esper.client;
using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

namespace com.espertech.esper.regression.multithread
{
    /// <summary>
    ///     Test for multithread-safety for a simple aggregation case using count(*).
    /// </summary>
    public class ExecMTStmtNamedWindowMultiple : RegressionExecution
    {
        public override void Configure(Configuration configuration)
        {
            configuration.AddEventType("OrderEvent", typeof(OrderEvent));
            configuration.AddEventType("OrderCancelEvent", typeof(OrderCancelEvent));
        }

        public override void Run(EPServiceProvider epService)
        {
            TryCount(epService, 10, 500, 3);
        }

        public void TryCount(EPServiceProvider epService, int numUsers, int numOrders, int numThreads)
        {
            for (var i = 0; i < numUsers; i++)
            {
                epService.EPAdministrator.CreateEPL(
                    "@Name('create_" + i + "') create window MyWindow_" + i +
                    "#unique(orderId) as select * from OrderEvent");
                epService.EPAdministrator.CreateEPL(
                    "@Name('insert_" + i + "') insert into MyWindow_" + i + " select * from OrderEvent(userId = 'user" +
                    i + "')");
                epService.EPAdministrator.CreateEPL(
                    "on OrderCancelEvent as d delete from MyWindow_" + i + " w where w.orderId = d.orderId");
                epService.EPAdministrator.CreateEPL(
                    "@Name('select_" + i + "') on OrderEvent as s select sum(w.price) from MyWindow_" + i +
                    " w where w.side = s.side group by w.side");
            }

            var runnables = new RunnableOrderSim[numThreads];
            var threads = new Thread[numThreads];
            for (var i = 0; i < numThreads; i++)
            {
                runnables[i] = new RunnableOrderSim(epService, i, numUsers, numOrders);
                threads[i] = new Thread(runnables[i].Run);
            }

            for (var i = 0; i < threads.Length; i++)
            {
                threads[i].Start();
            }

            for (var i = 0; i < threads.Length; i++)
            {
                threads[i].Join();
                Assert.IsTrue(runnables[i].Status);
            }
        }

        public class RunnableOrderSim
        {
            private readonly EPServiceProvider _engine;
            private readonly Random _random = new Random();

            public RunnableOrderSim(EPServiceProvider engine, int threadId, int numUsers, int numOrders)
            {
                _engine = engine;
                ThreadId = threadId;
                NumUsers = numUsers;
                NumOrders = numOrders;
            }

            public int ThreadId { get; }

            public int NumUsers { get; }

            public int NumOrders { get; }

            public bool Status { get; private set; }

            public void Run()
            {
                var orderIds = new string[10];
                for (var i = 0; i < orderIds.Length; i++)
                {
                    orderIds[i] = "order_" + i + "_" + ThreadId;
                }

                for (var i = 0; i < NumOrders; i++)
                {
                    if (_random.Next() % 3 == 0)
                    {
                        var orderId = orderIds[_random.Next(orderIds.Length)];
                        for (var j = 0; j < NumUsers; j++)
                        {
                            var theEvent = new OrderCancelEvent("user" + j, orderId);
                            _engine.EPRuntime.SendEvent(theEvent);
                        }
                    }
                    else
                    {
                        var orderId = orderIds[_random.Next(orderIds.Length)];
                        for (var j = 0; j < NumUsers; j++)
                        {
                            var theEvent = new OrderEvent("user" + j, orderId, 1000, "B");
                            _engine.EPRuntime.SendEvent(theEvent);
                        }
                    }
                }

                Status = true;
            }
        }

        public class OrderEvent
        {
            public OrderEvent(string userId, string orderId, double price, string side)
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
            public OrderCancelEvent(string userId, string orderId)
            {
                UserId = userId;
                OrderId = orderId;
            }

            public string UserId { get; }

            public string OrderId { get; }
        }
    }
} // end of namespace