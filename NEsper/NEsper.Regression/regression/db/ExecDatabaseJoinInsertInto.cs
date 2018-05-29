///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Data;
using System.Text;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.time;
using com.espertech.esper.supportregression.epl;
using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

namespace com.espertech.esper.regression.db
{
    public class ExecDatabaseJoinInsertInto : RegressionExecution {
        public override void Configure(Configuration configuration) {
            var configDB = SupportDatabaseService.CreateDefaultConfig();
            configDB.ConnectionLifecycle = ConnectionLifecycleEnum.RETAIN;
            configDB.ConnectionCatalog = "test";
            //configDB.ConnectionReadOnly = true;
            configDB.ConnectionTransactionIsolation = IsolationLevel.Serializable;
            configDB.ConnectionAutoCommit = true;

            configuration.AddDatabaseReference("MyDB", configDB);
    
            configuration.EngineDefaults.Logging.IsEnableADO = true;
        }
    
        public override void Run(EPServiceProvider epService) {
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
    
            var sb = new StringBuilder();
            sb.Append("insert into ReservationEvents(type, cid, elapsed, series) ");
            sb.Append("select istream 'type_1' as type, C.myvarchar as cid, C.myint as elapsed, C.mychar as series ");
            sb.Append("from pattern [every timer:interval(20 sec)], ");
            sb.Append("sql:MyDB [' select myvarchar, myint, mychar from mytesttable '] as C ");
            epService.EPAdministrator.CreateEPL(sb.ToString());
    
            // Reservation Events status change, aggregation, sla definition and DB cache update
            sb = new StringBuilder();
            sb.Append("insert into SumOfReservations(cid, type, series, total, insla, bordersla, outsla) ");
            sb.Append("select istream cid, type, series, ");
            sb.Append("count(*) as total, ");
            sb.Append("sum(case when elapsed < 600000 then 1 else 0 end) as insla, ");
            sb.Append("sum(case when elapsed between 600000 and 900000 then 1 else 0 end) as bordersla, ");
            sb.Append("sum(case when elapsed > 900000 then 1 else 0 end) as outsla ");
            sb.Append("from ReservationEvents#time_batch(10 sec) ");
            sb.Append("group by cid, type, series order by series asc");
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(sb.ToString());
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(20000));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(30000));
            EventBean[] received = listener.LastNewData;
            Assert.AreEqual(10, received.Length);
            listener.Reset();
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(31000));
            Assert.IsFalse(listener.IsInvoked);
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(39000));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(40000));
            received = listener.LastNewData;
            Assert.AreEqual(10, received.Length);
            listener.Reset();
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(41000));
            Assert.IsFalse(listener.IsInvoked);
        }
    }
} // end of namespace
