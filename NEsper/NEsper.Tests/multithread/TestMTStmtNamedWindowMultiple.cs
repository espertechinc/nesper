///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;

using com.espertech.esper.client;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.multithread
{
    /// <summary>
    /// Test for multithread-safety for a simple aggregation case using count(*).
    /// </summary>
    [TestFixture]
    public class TestMTStmtNamedWindowMultiple 
    {
        private EPServiceProvider _engine;
    
        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddEventType("OrderEvent", typeof(OrderEvent).FullName);
            config.AddEventType("OrderCancelEvent", typeof(OrderCancelEvent).FullName);
            _engine = EPServiceProviderManager.GetProvider("TestMTStmtNamedWindowMultiple", config);
        }
    
        [TearDown]
        public void TearDown()
        {
            _engine.Dispose();
        }
    
        [Test]
        public void TestInsertDeleteSelect()
        {
            TryCount(10, 500, 3);
        }
    
        public void TryCount(int numUsers, int numOrders, int numThreads)
        {
            for (int i = 0; i < numUsers; i++)
            {
                _engine.EPAdministrator.CreateEPL("@Name('create_" + i + "') create window MyWindow_" + i + ".std:unique(OrderId) as select * from OrderEvent");
                _engine.EPAdministrator.CreateEPL("@Name('insert_" + i + "') insert into MyWindow_" + i + " select * from OrderEvent(UserId = 'user" + i + "')");
                _engine.EPAdministrator.CreateEPL("on OrderCancelEvent as d delete from MyWindow_" + i + " w where w.OrderId = d.OrderId");
                _engine.EPAdministrator.CreateEPL("@Name('select_" + i + "') on OrderEvent as s select sum(w.Price) from MyWindow_" + i + " w where w.Side = s.Side group by w.Side");
            }
    
            RunnableOrderSim[] runnables = new RunnableOrderSim[numThreads];
            Thread[] threads = new Thread[numThreads];
            for (int i = 0; i < numThreads; i++)
            {
                runnables[i] = new RunnableOrderSim(_engine, i, numUsers, numOrders);
                threads[i] = new Thread(runnables[i].Run);
            }
    
            for (int i = 0; i < threads.Length; i++)
            {
                threads[i].Start();
            }
    
            for (int i = 0; i < threads.Length; i++)
            {
                threads[i].Join();
                Assert.IsTrue(runnables[i].Status);
            }
        }
    
        public class RunnableOrderSim
        {
            private readonly EPServiceProvider _engine;
            private readonly int _threadId;
            private readonly int _numUsers;
            private readonly int _numOrders;
            private readonly Random _random = new Random();

            public RunnableOrderSim(EPServiceProvider engine, int threadId, int numUsers, int numOrders)
            {
                _engine = engine;
                _threadId = threadId;
                _numUsers = numUsers;
                _numOrders = numOrders;
            }
    
            public void Run()
            {
                String[] orderIds = new String[10];
                for (int i = 0; i < orderIds.Length; i++)
                {
                    orderIds[i] = "order_" + i + "_" + _threadId;
                }
    
                for (int i = 0; i < _numOrders; i++)
                {
                    if (_random.Next(0, 4) % 3 == 0)
                    {
                        String orderId = orderIds[_random.Next(0, orderIds.Length)];
                        for (int j = 0; j < _numUsers; j++)
                        {
                            OrderCancelEvent theEvent = new OrderCancelEvent("user" + j, orderId);
                            _engine.EPRuntime.SendEvent(theEvent);
                        }
                    }
                    else
                    {
                        String orderId = orderIds[_random.Next(0, orderIds.Length)];
                        for (int j = 0; j < _numUsers; j++)
                        {
                            OrderEvent theEvent = new OrderEvent("user" + j, orderId, 1000, "B");
                            _engine.EPRuntime.SendEvent(theEvent);
                        }
                    }
                }
    
                Status = true;
            }

            public bool Status { get; private set; }
        }
    
        public class OrderEvent
        {
            public OrderEvent(String userId, String orderId, double price, String side)
            {
                UserId = userId;
                OrderId = orderId;
                Price = price;
                Side = side;
            }

            public string UserId { get; private set; }
            public string OrderId { get; private set; }
            public double Price { get; private set; }
            public string Side { get; private set; }
        }
    
        public class OrderCancelEvent
        {
            public OrderCancelEvent(String userId, String orderId)
            {
                UserId = userId;
                OrderId = orderId;
            }

            public string UserId { get; private set; }
            public string OrderId { get; private set; }
        }
    }
}
