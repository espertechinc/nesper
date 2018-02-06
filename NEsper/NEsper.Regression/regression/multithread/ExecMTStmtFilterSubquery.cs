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
using com.espertech.esper.supportregression.execution;

namespace com.espertech.esper.regression.multithread
{
    /// <summary>
    /// Test for multithread-safety for a simple aggregation case using count(*).
    /// </summary>
    public class ExecMTStmtFilterSubquery : RegressionExecution {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.Configuration.AddEventType<SupportBean_S0>();
    
            TryNamedWindowFilterSubquery(epService);
            TryStreamFilterSubquery(epService);
        }
    
        private void TryNamedWindowFilterSubquery(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create window MyWindow#keepall as SupportBean_S0");
            epService.EPAdministrator.CreateEPL("insert into MyWindow select * from SupportBean_S0");
    
            string epl = "select * from pattern[SupportBean_S0 -> SupportBean(not exists (select * from MyWindow mw where mw.p00 = 'E'))]";
            epService.EPAdministrator.CreateEPL(epl);
            epService.EPRuntime.SendEvent(new SupportBean_S0(1));
    
            var insertThread = new Thread(new InsertRunnable(epService, 1000).Run);
            var filterThread = new Thread(new FilterRunnable(epService, 1000).Run);
    
            Log.Info("Starting threads");
            insertThread.Start();
            filterThread.Start();
    
            Log.Info("Waiting for join");
            insertThread.Join();
            filterThread.Join();
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void TryStreamFilterSubquery(EPServiceProvider engine) {
            string epl = "select * from SupportBean(not exists (select * from SupportBean_S0#keepall mw where mw.p00 = 'E'))";
            engine.EPAdministrator.CreateEPL(epl);
    
            var insertThread = new Thread(new InsertRunnable(engine, 1000).Run);
            var filterThread = new Thread(new FilterRunnable(engine, 1000).Run);
    
            Log.Info("Starting threads");
            insertThread.Start();
            filterThread.Start();
    
            Log.Info("Waiting for join");
            insertThread.Join();
            filterThread.Join();
    
            engine.EPAdministrator.DestroyAllStatements();
        }
    
        public class InsertRunnable
        {
            private readonly EPServiceProvider engine;
            private readonly int numInserts;

            public int NumInserts => numInserts;

            public InsertRunnable(EPServiceProvider engine, int numInserts) {
                this.engine = engine;
                this.numInserts = numInserts;
            }
    
            public void Run() {
                Log.Info("Starting insert thread");
                for (int i = 0; i < numInserts; i++) {
                    engine.EPRuntime.SendEvent(new SupportBean_S0(i, "E"));
                }
                Log.Info("Completed insert thread, " + numInserts + " inserted");
            }
        }
    
        public class FilterRunnable
        {
            private readonly EPServiceProvider engine;
            private readonly int numEvents;

            public int NumEvents => numEvents;

            public FilterRunnable(EPServiceProvider engine, int numEvents) {
                this.engine = engine;
                this.numEvents = numEvents;
            }
    
            public void Run() {
                Log.Info("Starting filter thread");
                for (int i = 0; i < numEvents; i++) {
                    engine.EPRuntime.SendEvent(new SupportBean("G" + i, i));
                }
                Log.Info("Completed filter thread, " + numEvents + " completed");
            }
        }
    }
} // end of namespace
