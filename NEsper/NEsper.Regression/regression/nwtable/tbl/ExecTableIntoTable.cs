///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;

namespace com.espertech.esper.regression.nwtable.tbl
{
    using Map = IDictionary<string, object>;

    public class ExecTableIntoTable : RegressionExecution
    {
        public override void Run(EPServiceProvider epService)
        {
            foreach (var clazz in new[] {typeof(SupportBean), typeof(SupportBean_S0), typeof(SupportBean_S1)})
            {
                epService.EPAdministrator.Configuration.AddEventType(clazz);
            }

            RunAssertionIntoTableWindowSortedFromJoin(epService);
            RunAssertionBoundUnbound(epService);
        }

        private void RunAssertionIntoTableWindowSortedFromJoin(EPServiceProvider epService)
        {
            epService.EPAdministrator.CreateEPL(
                "create table MyTable(" +
                "thewin window(*) @Type('SupportBean')," +
                "thesort sorted(IntPrimitive desc) @Type('SupportBean')" +
                ")");

            epService.EPAdministrator.CreateEPL(
                "into table MyTable " +
                "select window(sb.*) as thewin, sorted(sb.*) as thesort " +
                "from SupportBean_S0#lastevent, SupportBean#keepall as sb");
            epService.EPRuntime.SendEvent(new SupportBean_S0(1));

            var sb1 = new SupportBean("E1", 1);
            epService.EPRuntime.SendEvent(sb1);
            var sb2 = new SupportBean("E2", 2);
            epService.EPRuntime.SendEvent(sb2);

            var result = epService.EPRuntime.ExecuteQuery("select * from MyTable");
            EPAssertionUtil.AssertPropsPerRow(
                result.Array, "thewin,thesort".Split(','),
                new[] {new object[] {new[] {sb1, sb2}, new[] {sb2, sb1}}});

            epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionBoundUnbound(EPServiceProvider epService)
        {
            // Bound: max/min; Unbound: maxever/minever
            TryAssertionMinMax(epService, false);
            TryAssertionMinMax(epService, true);

            // Bound: sorted; Unbound: maxbyever/minbyever; Disallowed: minby, maxby declaration (must use sorted instead)
            // - requires declaring the same sort expression but can be against subtype of declared event type
            TryAssertionSortedMinMaxBy(epService, false);
            TryAssertionSortedMinMaxBy(epService, true);

            // Bound: window; Unbound: lastever/firstever; Disallowed: last, first
            TryAssertionLastFirstWindow(epService, false);
            TryAssertionLastFirstWindow(epService, true);
        }

        private void TryAssertionLastFirstWindow(EPServiceProvider epService, bool soda)
        {
            var fields = "lasteveru,firsteveru,windowb".Split(',');
            var eplDeclare = "create table varagg (" +
                             "lasteveru lastever(*) @Type('SupportBean'), " +
                             "firsteveru firstever(*) @Type('SupportBean'), " +
                             "windowb window(*) @Type('SupportBean'))";
            SupportModelHelper.CreateByCompileOrParse(epService, soda, eplDeclare);

            var eplIterate = "select varagg from SupportBean_S0#lastevent";
            var stmtIterate = SupportModelHelper.CreateByCompileOrParse(epService, soda, eplIterate);
            epService.EPRuntime.SendEvent(new SupportBean_S0(0));

            var eplBoundInto = "into table varagg select window(*) as windowb from SupportBean#length(2)";
            SupportModelHelper.CreateByCompileOrParse(epService, soda, eplBoundInto);

            var eplUnboundInto =
                "into table varagg select lastever(*) as lasteveru, firstever(*) as firsteveru from SupportBean";
            SupportModelHelper.CreateByCompileOrParse(epService, soda, eplUnboundInto);

            var b1 = MakeSendBean(epService, "E1", 20);
            var b2 = MakeSendBean(epService, "E2", 15);
            var b3 = MakeSendBean(epService, "E3", 10);
            AssertResults(stmtIterate, fields, new object[] {b3, b1, new object[] {b2, b3}});

            var b4 = MakeSendBean(epService, "E4", 5);
            AssertResults(stmtIterate, fields, new object[] {b4, b1, new object[] {b3, b4}});

            // invalid: bound aggregation into unbound max
            SupportMessageAssertUtil.TryInvalid(
                epService, "into table varagg select last(*) as lasteveru from SupportBean#length(2)",
                "Error starting statement: Failed to validate select-clause expression 'last(*)': For into-table use 'window(*)' or ''window(stream.*)' instead [");
            // invalid: unbound aggregation into bound max
            SupportMessageAssertUtil.TryInvalid(
                epService, "into table varagg select lastever(*) as windowb from SupportBean#length(2)",
                "Error starting statement: Incompatible aggregation function for table 'varagg' column 'windowb', expecting 'window(*)' and received 'lastever(*)': Not a 'window' aggregation [");

            // valid: bound with unbound variable
            var eplBoundIntoUnbound = "into table varagg select lastever(*) as lasteveru from SupportBean#length(2)";
            SupportModelHelper.CreateByCompileOrParse(epService, soda, eplBoundIntoUnbound);

            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("table_varagg__internal", false);
            epService.EPAdministrator.Configuration.RemoveEventType("table_varagg__public", false);
        }

        private void TryAssertionSortedMinMaxBy(EPServiceProvider epService, bool soda)
        {
            var fields = "maxbyeveru,minbyeveru,sortedb".Split(',');
            var eplDeclare = "create table varagg (" +
                             "maxbyeveru maxbyever(IntPrimitive) @Type('SupportBean'), " +
                             "minbyeveru minbyever(IntPrimitive) @Type('SupportBean'), " +
                             "sortedb sorted(IntPrimitive) @Type('SupportBean'))";
            SupportModelHelper.CreateByCompileOrParse(epService, soda, eplDeclare);

            var eplIterate = "select varagg from SupportBean_S0#lastevent";
            var stmtIterate = SupportModelHelper.CreateByCompileOrParse(epService, soda, eplIterate);
            epService.EPRuntime.SendEvent(new SupportBean_S0(0));

            var eplBoundInto = "into table varagg select sorted() as sortedb from SupportBean#length(2)";
            SupportModelHelper.CreateByCompileOrParse(epService, soda, eplBoundInto);

            var eplUnboundInto =
                "into table varagg select maxbyever() as maxbyeveru, minbyever() as minbyeveru from SupportBean";
            SupportModelHelper.CreateByCompileOrParse(epService, soda, eplUnboundInto);

            var b1 = MakeSendBean(epService, "E1", 20);
            var b2 = MakeSendBean(epService, "E2", 15);
            var b3 = MakeSendBean(epService, "E3", 10);
            AssertResults(stmtIterate, fields, new object[] {b1, b3, new object[] {b3, b2}});

            // invalid: bound aggregation into unbound max
            SupportMessageAssertUtil.TryInvalid(
                epService, "into table varagg select maxby(IntPrimitive) as maxbyeveru from SupportBean#length(2)",
                "Error starting statement: Failed to validate select-clause expression 'maxby(IntPrimitive)': When specifying into-table a sort expression cannot be provided [");
            // invalid: unbound aggregation into bound max
            SupportMessageAssertUtil.TryInvalid(
                epService, "into table varagg select maxbyever() as sortedb from SupportBean#length(2)",
                "Error starting statement: Incompatible aggregation function for table 'varagg' column 'sortedb', expecting 'sorted(IntPrimitive)' and received 'maxbyever()': The required aggregation function name is 'sorted' and provided is 'maxbyever' [");

            // valid: bound with unbound variable
            var eplBoundIntoUnbound = "into table varagg select " +
                                      "maxbyever() as maxbyeveru, minbyever() as minbyeveru " +
                                      "from SupportBean#length(2)";
            SupportModelHelper.CreateByCompileOrParse(epService, soda, eplBoundIntoUnbound);

            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("table_varagg__internal", false);
            epService.EPAdministrator.Configuration.RemoveEventType("table_varagg__public", false);
        }

        private void TryAssertionMinMax(EPServiceProvider epService, bool soda)
        {
            var fields = "maxb,maxu,minb,minu".Split(',');
            var eplDeclare = "create table varagg (" +
                             "maxb max(int), maxu maxever(int), minb min(int), minu minever(int))";
            SupportModelHelper.CreateByCompileOrParse(epService, soda, eplDeclare);

            var eplIterate = "select varagg from SupportBean_S0#lastevent";
            var stmtIterate = SupportModelHelper.CreateByCompileOrParse(epService, soda, eplIterate);
            epService.EPRuntime.SendEvent(new SupportBean_S0(0));

            var eplBoundInto = "into table varagg select " +
                               "max(IntPrimitive) as maxb, min(IntPrimitive) as minb " +
                               "from SupportBean#length(2)";
            SupportModelHelper.CreateByCompileOrParse(epService, soda, eplBoundInto);

            var eplUnboundInto = "into table varagg select " +
                                 "maxever(IntPrimitive) as maxu, minever(IntPrimitive) as minu " +
                                 "from SupportBean";
            SupportModelHelper.CreateByCompileOrParse(epService, soda, eplUnboundInto);

            epService.EPRuntime.SendEvent(new SupportBean("E1", 20));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 15));
            epService.EPRuntime.SendEvent(new SupportBean("E3", 10));
            AssertResults(stmtIterate, fields, new object[] {15, 20, 10, 10});

            epService.EPRuntime.SendEvent(new SupportBean("E4", 5));
            AssertResults(stmtIterate, fields, new object[] {10, 20, 5, 5});

            epService.EPRuntime.SendEvent(new SupportBean("E5", 25));
            AssertResults(stmtIterate, fields, new object[] {25, 25, 5, 5});

            // invalid: unbound aggregation into bound max
            SupportMessageAssertUtil.TryInvalid(
                epService, "into table varagg select max(IntPrimitive) as maxb from SupportBean",
                "Error starting statement: Incompatible aggregation function for table 'varagg' column 'maxb', expecting 'max(int)' and received 'max(IntPrimitive)': The aggregation declares use with data windows and provided is unbound [");

            // valid: bound with unbound variable
            var eplBoundIntoUnbound = "into table varagg select " +
                                      "maxever(IntPrimitive) as maxu, minever(IntPrimitive) as minu " +
                                      "from SupportBean#length(2)";
            SupportModelHelper.CreateByCompileOrParse(epService, soda, eplBoundIntoUnbound);

            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("table_varagg__internal", false);
            epService.EPAdministrator.Configuration.RemoveEventType("table_varagg__public", false);
        }

        private void AssertResults(EPStatement stmt, string[] fields, object[] values)
        {
            var @event = stmt.First();
            var map = (Map) @event.Get("varagg");
            EPAssertionUtil.AssertPropsMap(map, fields, values);
        }

        private SupportBean MakeSendBean(EPServiceProvider epService, string theString, int intPrimitive)
        {
            var bean = new SupportBean(theString, intPrimitive);
            epService.EPRuntime.SendEvent(bean);
            return bean;
        }
    }
} // end of namespace