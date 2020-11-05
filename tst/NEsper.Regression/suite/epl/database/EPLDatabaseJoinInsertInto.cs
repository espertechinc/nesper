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

namespace com.espertech.esper.regressionlib.suite.epl.database
{
    public class EPLDatabaseJoinInsertInto : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            env.AdvanceTime(0);
            var path = new RegressionPath();

            var sb = new StringBuilder();
            sb.Append("insert into ReservationEvents(type, cId, elapsed, series) ");
            sb.Append("select istream 'type_1' as type, C.myvarchar as cId, C.myint as elapsed, C.mychar as series ");
            sb.Append("from pattern [every timer:interval(20 sec)], ");
            sb.Append("sql:MyDBWithTxnIso1WithReadOnly [' select myvarchar, myint, mychar from mytesttable '] as C ");
            env.CompileDeploy(sb.ToString(), path);

            // Reservation Events status change, aggregation, sla definition and DB cache update
            sb = new StringBuilder();
            sb.Append("@Name('s0') insert into SumOfReservations(cId, type, series, total, insla, bordersla, outsla) ");
            sb.Append("select istream cId, type, series, ");
            sb.Append("count(*) as total, ");
            sb.Append("sum(case when elapsed < 600000 then 1 else 0 end) as insla, ");
            sb.Append("sum(case when elapsed between 600000 and 900000 then 1 else 0 end) as bordersla, ");
            sb.Append("sum(case when elapsed > 900000 then 1 else 0 end) as outsla ");
            sb.Append("from ReservationEvents#time_batch(10 sec) ");
            sb.Append("group by cId, type, series order by series asc");

            env.CompileDeploy(sb.ToString(), path).AddListener("s0");

            env.AdvanceTime(20000);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.AdvanceTime(30000);
            var received = env.Listener("s0").LastNewData;
            Assert.AreEqual(10, received.Length);
            env.Listener("s0").Reset();

            env.AdvanceTime(31000);
            Assert.IsFalse(env.Listener("s0").IsInvoked);
            env.AdvanceTime(39000);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.AdvanceTime(40000);
            received = env.Listener("s0").LastNewData;
            Assert.AreEqual(10, received.Length);
            env.Listener("s0").Reset();

            env.AdvanceTime(41000);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.UndeployAll();
        }
    }
} // end of namespace