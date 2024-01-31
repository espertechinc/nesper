///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Text;

using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;
using NUnit.Framework.Legacy;

// assertEquals

namespace com.espertech.esper.regressionlib.suite.epl.database
{
    public class EPLDatabaseJoinInsertInto : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            env.AdvanceTime(0);
            var path = new RegressionPath();

            var sb = new StringBuilder();
            sb.Append("@public insert into ReservationEvents(type, cid, elapsed, series) ");
            sb.Append("select istream 'type_1' as type, C.myvarchar as cid, C.myint as elapsed, C.mychar as series ");
            sb.Append("from pattern [every timer:interval(20 sec)], ");
            sb.Append("sql:MyDBWithTxnIso1WithReadOnly [' select myvarchar, myint, mychar from mytesttable '] as C ");
            env.CompileDeploy(sb.ToString(), path);

            // Reservation Events status change, aggregation, sla definition and DB cache update
            sb = new StringBuilder();
            sb.Append(
                "@name('s0') @public insert into SumOfReservations(cid, type, series, Total, insla, bordersla, outsla) ");
            sb.Append("select istream cid, type, series, ");
            sb.Append("count(*) as Total, ");
            sb.Append("sum(case when elapsed < 600000 then 1 else 0 end) as insla, ");
            sb.Append("sum(case when elapsed between 600000 and 900000 then 1 else 0 end) as bordersla, ");
            sb.Append("sum(case when elapsed > 900000 then 1 else 0 end) as outsla ");
            sb.Append("from ReservationEvents#time_batch(10 sec) ");
            sb.Append("group by cid, type, series order by series asc");

            env.CompileDeploy(sb.ToString(), path).AddListener("s0");

            env.AdvanceTime(20000);
            env.AssertListenerNotInvoked("s0");

            env.AdvanceTime(30000);
            env.AssertListener(
                "s0",
                listener => {
                    var received = listener.LastNewData;
                    ClassicAssert.AreEqual(10, received.Length);
                    listener.Reset();
                });

            env.AdvanceTime(31000);
            env.AssertListenerNotInvoked("s0");
            env.AdvanceTime(39000);
            env.AssertListenerNotInvoked("s0");

            env.AdvanceTime(40000);
            env.AssertListener(
                "s0",
                listener => {
                    var received = listener.LastNewData;
                    ClassicAssert.AreEqual(10, received.Length);
                    listener.Reset();
                });

            env.AdvanceTime(41000);
            env.AssertListenerNotInvoked("s0");

            env.UndeployAll();
        }
    }
} // end of namespace