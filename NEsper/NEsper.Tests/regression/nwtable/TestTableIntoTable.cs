///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable
{
    [TestFixture]
    public class TestTableIntoTable  {
        private EPServiceProvider epService;
    
        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            epService = EPServiceProviderManager.GetDefaultProvider(config);
            epService.Initialize();
            foreach (Type clazz in new Type[] {typeof(SupportBean), typeof(SupportBean_S0), typeof(SupportBean_S1)}) {
                epService.EPAdministrator.Configuration.AddEventType(clazz);
            }
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, this.GetType(), GetType().FullName);}
        }
    
        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
        }
    
        [Test]
        public void TestIntoTableWindowSortedFromJoin()
        {
            epService.EPAdministrator.CreateEPL("create table MyTable(" +
                    "thewin window(*) @type('SupportBean')," +
                    "thesort sorted(IntPrimitive desc) @type('SupportBean')" +
                    ")");
    
            epService.EPAdministrator.CreateEPL("into table MyTable " +
                    "select window(sb.*) as thewin, sorted(sb.*) as thesort " +
                    "from SupportBean_S0.std:lastevent(), SupportBean.win:keepall() as sb");
            epService.EPRuntime.SendEvent(new SupportBean_S0(1));
    
            SupportBean sb1 = new SupportBean("E1", 1);
            epService.EPRuntime.SendEvent(sb1);
            SupportBean sb2 = new SupportBean("E2", 2);
            epService.EPRuntime.SendEvent(sb2);
    
            EPOnDemandQueryResult result = epService.EPRuntime.ExecuteQuery("select * from MyTable");
            EPAssertionUtil.AssertPropsPerRow(result.Array, "thewin,thesort".Split(','),
                    new object[][] { new object[] { new SupportBean[] { sb1, sb2 }, new SupportBean[] { sb2, sb1 } } });
        }
    
        [Test]
        public void TestBoundUnbound()
        {
            // Bound: max/min; Unbound: maxever/minever
            RunAssertionMinMax(false);
            RunAssertionMinMax(true);
    
            // Bound: sorted; Unbound: maxbyever/minbyever; Disallowed: minby, maxby declaration (must use sorted instead)
            // - requires declaring the same sort expression but can be against subtype of declared event type
            RunAssertionSortedMinMaxBy(false);
            RunAssertionSortedMinMaxBy(true);
    
            // Bound: window; Unbound: lastever/firstever; Disallowed: last, first
            RunAssertionLastFirstWindow(false);
            RunAssertionLastFirstWindow(true);
        }
    
        private void RunAssertionLastFirstWindow(bool soda)
        {
            string[] fields = "lasteveru,firsteveru,windowb".Split(',');
            string eplDeclare = "create table varagg (" +
                    "lasteveru lastever(*) @type('SupportBean'), " +
                    "firsteveru firstever(*) @type('SupportBean'), " +
                    "windowb window(*) @type('SupportBean'))";
            SupportModelHelper.CreateByCompileOrParse(epService, soda, eplDeclare);
    
            string eplIterate = "select varagg from SupportBean_S0.std:lastevent()";
            EPStatement stmtIterate = SupportModelHelper.CreateByCompileOrParse(epService, soda, eplIterate);
            epService.EPRuntime.SendEvent(new SupportBean_S0(0));
    
            string eplBoundInto = "into table varagg select window(*) as windowb from SupportBean.win:length(2)";
            SupportModelHelper.CreateByCompileOrParse(epService, soda, eplBoundInto);
    
            string eplUnboundInto = "into table varagg select lastever(*) as lasteveru, firstever(*) as firsteveru from SupportBean";
            SupportModelHelper.CreateByCompileOrParse(epService, soda, eplUnboundInto);
    
            SupportBean b1 = MakeSendBean("E1", 20);
            SupportBean b2 = MakeSendBean("E2", 15);
            SupportBean b3 = MakeSendBean("E3", 10);
            AssertResults(stmtIterate, fields, new object[]{b3, b1, new object[] {b2, b3}});
    
            SupportBean b4 = MakeSendBean("E4", 5);
            AssertResults(stmtIterate, fields, new object[]{b4, b1, new object[] {b3, b4}});
    
            // invalid: bound aggregation into unbound max
            SupportMessageAssertUtil.TryInvalid(epService, "into table varagg select last(*) as lasteveru from SupportBean.win:length(2)",
                    "Error starting statement: Failed to validate select-clause expression 'last(*)': For into-table use 'window(*)' or ''window(stream.*)' instead [");
            // invalid: unbound aggregation into bound max
            SupportMessageAssertUtil.TryInvalid(epService, "into table varagg select lastever(*) as windowb from SupportBean.win:length(2)",
                    "Error starting statement: Incompatible aggregation function for table 'varagg' column 'windowb', expecting 'window(*)' and received 'lastever(*)': Not a 'window' aggregation [");
    
            // valid: bound with unbound variable
            string eplBoundIntoUnbound = "into table varagg select lastever(*) as lasteveru from SupportBean.win:length(2)";
            SupportModelHelper.CreateByCompileOrParse(epService, soda, eplBoundIntoUnbound);
    
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("table_varagg__internal", false);
            epService.EPAdministrator.Configuration.RemoveEventType("table_varagg__public", false);
        }
    
        private void RunAssertionSortedMinMaxBy(bool soda)
        {
            string[] fields = "maxbyeveru,minbyeveru,sortedb".Split(',');
            string eplDeclare = "create table varagg (" +
                    "maxbyeveru maxbyever(IntPrimitive) @type('SupportBean'), " +
                    "minbyeveru minbyever(IntPrimitive) @type('SupportBean'), " +
                    "sortedb sorted(IntPrimitive) @type('SupportBean'))";
            SupportModelHelper.CreateByCompileOrParse(epService, soda, eplDeclare);
    
            string eplIterate = "select varagg from SupportBean_S0.std:lastevent()";
            EPStatement stmtIterate = SupportModelHelper.CreateByCompileOrParse(epService, soda, eplIterate);
            epService.EPRuntime.SendEvent(new SupportBean_S0(0));
    
            string eplBoundInto = "into table varagg select sorted() as sortedb from SupportBean.win:length(2)";
            SupportModelHelper.CreateByCompileOrParse(epService, soda, eplBoundInto);
    
            string eplUnboundInto = "into table varagg select maxbyever() as maxbyeveru, minbyever() as minbyeveru from SupportBean";
            SupportModelHelper.CreateByCompileOrParse(epService, soda, eplUnboundInto);
    
            SupportBean b1 = MakeSendBean("E1", 20);
            SupportBean b2 = MakeSendBean("E2", 15);
            SupportBean b3 = MakeSendBean("E3", 10);
            AssertResults(stmtIterate, fields, new object[]{b1, b3, new object[] {b3, b2}});
    
            // invalid: bound aggregation into unbound max
            SupportMessageAssertUtil.TryInvalid(epService, "into table varagg select maxby(IntPrimitive) as maxbyeveru from SupportBean.win:length(2)",
                    "Error starting statement: Failed to validate select-clause expression 'maxby(IntPrimitive)': When specifying into-table a sort expression cannot be provided [");
            // invalid: unbound aggregation into bound max
            SupportMessageAssertUtil.TryInvalid(epService, "into table varagg select maxbyever() as sortedb from SupportBean.win:length(2)",
                    "Error starting statement: Incompatible aggregation function for table 'varagg' column 'sortedb', expecting 'sorted(IntPrimitive)' and received 'maxbyever()': The required aggregation function name is 'sorted' and provided is 'maxbyever' [");
    
            // valid: bound with unbound variable
            string eplBoundIntoUnbound = "into table varagg select " +
                    "maxbyever() as maxbyeveru, minbyever() as minbyeveru " +
                    "from SupportBean.win:length(2)";
            SupportModelHelper.CreateByCompileOrParse(epService, soda, eplBoundIntoUnbound);
    
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("table_varagg__internal", false);
            epService.EPAdministrator.Configuration.RemoveEventType("table_varagg__public", false);
        }
    
        private void RunAssertionMinMax(bool soda)
        {
            string[] fields = "maxb,maxu,minb,minu".Split(',');
            string eplDeclare = "create table varagg (" +
                    "maxb max(int), maxu maxever(int), minb min(int), minu minever(int))";
            SupportModelHelper.CreateByCompileOrParse(epService, soda, eplDeclare);
    
            string eplIterate = "select varagg from SupportBean_S0.std:lastevent()";
            EPStatement stmtIterate = SupportModelHelper.CreateByCompileOrParse(epService, soda, eplIterate);
            epService.EPRuntime.SendEvent(new SupportBean_S0(0));
    
            string eplBoundInto = "into table varagg select " +
                    "max(IntPrimitive) as maxb, min(IntPrimitive) as minb " +
                    "from SupportBean.win:length(2)";
            SupportModelHelper.CreateByCompileOrParse(epService, soda, eplBoundInto);
    
            string eplUnboundInto = "into table varagg select " +
                    "maxever(IntPrimitive) as maxu, minever(IntPrimitive) as minu " +
                    "from SupportBean";
            SupportModelHelper.CreateByCompileOrParse(epService, soda, eplUnboundInto);
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 20));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 15));
            epService.EPRuntime.SendEvent(new SupportBean("E3", 10));
            AssertResults(stmtIterate, fields, new object[]{15, 20, 10, 10});
    
            epService.EPRuntime.SendEvent(new SupportBean("E4", 5));
            AssertResults(stmtIterate, fields, new object[]{10, 20, 5, 5});
    
            epService.EPRuntime.SendEvent(new SupportBean("E5", 25));
            AssertResults(stmtIterate, fields, new object[]{25, 25, 5, 5});
    
            // invalid: unbound aggregation into bound max
            SupportMessageAssertUtil.TryInvalid(epService, "into table varagg select max(IntPrimitive) as maxb from SupportBean",
                    "Error starting statement: Incompatible aggregation function for table 'varagg' column 'maxb', expecting 'max(int)' and received 'max(IntPrimitive)': The aggregation declares use with data windows and provided is unbound [");
    
            // valid: bound with unbound variable
            string eplBoundIntoUnbound = "into table varagg select " +
                    "maxever(IntPrimitive) as maxu, minever(IntPrimitive) as minu " +
                    "from SupportBean.win:length(2)";
            SupportModelHelper.CreateByCompileOrParse(epService, soda, eplBoundIntoUnbound);
    
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("table_varagg__internal", false);
            epService.EPAdministrator.Configuration.RemoveEventType("table_varagg__public", false);
        }
    
        private void AssertResults(EPStatement stmt, string[] fields, object[] values)
        {
            var @event = stmt.First();
            var map = (IDictionary<string, object>) @event.Get("varagg");
            EPAssertionUtil.AssertPropsMap(map, fields, values);
        }
    
        private SupportBean MakeSendBean(string theString, int intPrimitive)
        {
            var bean = new SupportBean(theString, intPrimitive);
            epService.EPRuntime.SendEvent(bean);
            return bean;
        }
    }
}
