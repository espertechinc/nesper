///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Threading;

using com.espertech.esper.client;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.multithread
{
    /// <summary>Test for multithread-safety for a simple aggregation case using count(*). </summary>
    [TestFixture]
    public class TestMTStmtFilterSubquery 
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        private EPServiceProvider _engine;
    
        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            _engine = EPServiceProviderManager.GetProvider(GetType().FullName, config);
            _engine.EPAdministrator.Configuration.AddEventType<SupportBean>();
            _engine.EPAdministrator.Configuration.AddEventType(typeof(SupportBean_S0));
        }
    
        [TearDown]
        public void TearDown()
        {
            _engine.Dispose();
        }
    
        [Test]
        public void TestCount()
        {
            TryNamedWindowFilterSubquery();
            TryStreamFilterSubquery();
        }
    
        public void TryNamedWindowFilterSubquery()
        {
            _engine.EPAdministrator.CreateEPL("create window MyWindow#keepall as SupportBean_S0");
            _engine.EPAdministrator.CreateEPL("insert into MyWindow select * from SupportBean_S0");
    
            const string epl = "select * from pattern[SupportBean_S0 -> SupportBean(not exists (select * from MyWindow mw where mw.P00 = 'E'))]";
            _engine.EPAdministrator.CreateEPL(epl);
            _engine.EPRuntime.SendEvent(new SupportBean_S0(1));
    
            var insertThread = new Thread(() => new InsertRunnable(_engine, 1000).Run());
            var filterThread = new Thread(() => new FilterRunnable(_engine, 1000).Run());
    
            Log.Info("Starting threads");
            insertThread.Start();
            filterThread.Start();
    
            Log.Info("Waiting for join");
            insertThread.Join();
            filterThread.Join();
    
            _engine.EPAdministrator.DestroyAllStatements();
        }
    
        public void TryStreamFilterSubquery()
        {
            const string epl = "select * from SupportBean(not exists (select * from SupportBean_S0#keepall mw where mw.P00 = 'E'))";
            _engine.EPAdministrator.CreateEPL(epl);
    
            var insertThread = new Thread(new InsertRunnable(_engine, 1000).Run);
            var filterThread = new Thread(new FilterRunnable(_engine, 1000).Run);
    
            Log.Info("Starting threads");
            insertThread.Start();
            filterThread.Start();
    
            Log.Info("Waiting for join");
            insertThread.Join();
            filterThread.Join();
    
            _engine.EPAdministrator.DestroyAllStatements();
        }

        public class InsertRunnable
        {
            private readonly EPServiceProvider _engine;
            private readonly int _numInserts;

            public InsertRunnable(EPServiceProvider engine, int numInserts)
            {
                _engine = engine;
                _numInserts = numInserts;
            }

            public void Run()
            {
                Log.Info("Starting insert thread");
                for (int i = 0; i < _numInserts; i++)
                {
                    _engine.EPRuntime.SendEvent(new SupportBean_S0(i, "E"));
                }
                Log.Info("Completed insert thread, " + _numInserts + " inserted");
            }
        }

        public class FilterRunnable
        {
            private readonly EPServiceProvider _engine;
            private readonly int _numEvents;

            public FilterRunnable(EPServiceProvider engine, int numEvents)
            {
                _engine = engine;
                _numEvents = numEvents;
            }

            public void Run()
            {
                Log.Info("Starting filter thread");
                for (int i = 0; i < _numEvents; i++)
                {
                    _engine.EPRuntime.SendEvent(new SupportBean("G" + i, i));
                }
                Log.Info("Completed filter thread, " + _numEvents + " completed");
            }
        }
    }
}
