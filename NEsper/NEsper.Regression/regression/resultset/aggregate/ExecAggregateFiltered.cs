///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Numerics;
using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.core.service;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;

using static com.espertech.esper.supportregression.util.SupportMessageAssertUtil;

using NUnit.Framework;

namespace com.espertech.esper.regression.resultset.aggregate
{
    public class ExecAggregateFiltered : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType(typeof(BlackWhiteEvent));
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBeanNumeric));
    
            RunAssertionBlackWhitePercent(epService);
            RunAssertionCountVariations(epService);
            RunAssertionAllAggFunctions(epService);
            RunAssertionFirstLastEver(epService);
            RunAssertionInvalid(epService);
        }
    
        private void RunAssertionBlackWhitePercent(EPServiceProvider epService) {
            string[] fields = "cb,cnb,c,pct".Split(',');
            string epl = "select count(*,IsBlack) as cb, count(*,not IsBlack) as cnb, count(*) as c, count(*,IsBlack)/count(*) as pct from BlackWhiteEvent#length(3)";
            EPStatementSPI stmt = (EPStatementSPI) epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            Assert.IsFalse(stmt.StatementContext.IsStatelessSelect);
    
            epService.EPRuntime.SendEvent(new BlackWhiteEvent(true));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{1L, 0L, 1L, 1d});
    
            epService.EPRuntime.SendEvent(new BlackWhiteEvent(false));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{1L, 1L, 2L, 0.5d});
    
            epService.EPRuntime.SendEvent(new BlackWhiteEvent(false));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{1L, 2L, 3L, 1 / 3d});
    
            epService.EPRuntime.SendEvent(new BlackWhiteEvent(false));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{0L, 3L, 3L, 0d});
    
            SupportModelHelper.CompileCreate(epService, epl);
            SupportModelHelper.CompileCreate(epService, "select count(distinct IsBlack,not IsBlack), count(IsBlack,IsBlack) from BlackWhiteEvent");
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionCountVariations(EPServiceProvider epService) {
            string[] fields = "c1,c2".Split(',');
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select " +
                    "count(IntBoxed, BoolPrimitive) as c1," +
                    "count(distinct IntBoxed, BoolPrimitive) as c2 " +
                    "from SupportBean#length(3)");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(MakeBean(100, true));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{1L, 1L});
    
            epService.EPRuntime.SendEvent(MakeBean(100, true));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{2L, 1L});
    
            epService.EPRuntime.SendEvent(MakeBean(101, false));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{2L, 1L});
    
            epService.EPRuntime.SendEvent(MakeBean(102, true));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{2L, 2L});
    
            epService.EPRuntime.SendEvent(MakeBean(103, false));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{1L, 1L});
    
            epService.EPRuntime.SendEvent(MakeBean(104, false));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{1L, 1L});
    
            epService.EPRuntime.SendEvent(MakeBean(105, false));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{0L, 0L});
    
            stmt.Dispose();
        }
    
        private void RunAssertionAllAggFunctions(EPServiceProvider epService) {
    
            string[] fields = "cavedev,cavg,cmax,cmedian,cmin,cstddev,csum,cfmaxever,cfminever".Split(',');
            EPStatementSPI stmt = (EPStatementSPI) epService.EPAdministrator.CreateEPL("select " +
                    "avedev(IntBoxed, BoolPrimitive) as cavedev," +
                    "avg(IntBoxed, BoolPrimitive) as cavg, " +
                    "fmax(IntBoxed, BoolPrimitive) as cmax, " +
                    "median(IntBoxed, BoolPrimitive) as cmedian, " +
                    "fmin(IntBoxed, BoolPrimitive) as cmin, " +
                    "stddev(IntBoxed, BoolPrimitive) as cstddev, " +
                    "sum(IntBoxed, BoolPrimitive) as csum," +
                    "fmaxever(IntBoxed, BoolPrimitive) as cfmaxever, " +
                    "fminever(IntBoxed, BoolPrimitive) as cfminever " +
                    "from SupportBean#length(3)");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(MakeBean(100, false));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{null, null, null, null, null, null, null, null, null});
    
            epService.EPRuntime.SendEvent(MakeBean(10, true));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{0.0d, 10.0, 10, 10.0, 10, null, 10, 10, 10});
    
            epService.EPRuntime.SendEvent(MakeBean(11, false));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{0.0d, 10.0, 10, 10.0, 10, null, 10, 10, 10});
    
            epService.EPRuntime.SendEvent(MakeBean(20, true));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{5.0d, 15.0, 20, 15.0, 10, 7.0710678118654755, 30, 20, 10});
    
            epService.EPRuntime.SendEvent(MakeBean(30, true));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{5.0d, 25.0, 30, 25.0, 20, 7.0710678118654755, 50, 30, 10});
    
            // Test all remaining types of "sum"
            stmt.Dispose();
            fields = "c1,c2,c3,c4".Split(',');
            stmt = (EPStatementSPI) epService.EPAdministrator.CreateEPL("select " +
                    "sum(FloatPrimitive, BoolPrimitive) as c1," +
                    "sum(DoublePrimitive, BoolPrimitive) as c2, " +
                    "sum(LongPrimitive, BoolPrimitive) as c3, " +
                    "sum(ShortPrimitive, BoolPrimitive) as c4 " +
                    "from SupportBean#length(2)");
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(MakeBean(2f, 3d, 4L, (short) 5, false));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{null, null, null, null});
    
            epService.EPRuntime.SendEvent(MakeBean(3f, 4d, 5L, (short) 6, true));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{3f, 4d, 5L, 6});
    
            epService.EPRuntime.SendEvent(MakeBean(4f, 5d, 6L, (short) 7, true));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{7f, 9d, 11L, 13});
    
            epService.EPRuntime.SendEvent(MakeBean(1f, 1d, 1L, (short) 1, true));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{5f, 6d, 7L, 8});
    
            // Test min/max-ever
            stmt.Dispose();
            fields = "c1,c2".Split(',');
            stmt = (EPStatementSPI) epService.EPAdministrator.CreateEPL("select " +
                    "fmax(IntBoxed, BoolPrimitive) as c1," +
                    "fmin(IntBoxed, BoolPrimitive) as c2 " +
                    "from SupportBean");
            stmt.Events += listener.Update;
            Assert.IsFalse(stmt.StatementContext.IsStatelessSelect);
    
            epService.EPRuntime.SendEvent(MakeBean(10, true));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{10, 10});
    
            epService.EPRuntime.SendEvent(MakeBean(20, true));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{20, 10});
    
            epService.EPRuntime.SendEvent(MakeBean(8, false));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{20, 10});
    
            epService.EPRuntime.SendEvent(MakeBean(7, true));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{20, 7});
    
            epService.EPRuntime.SendEvent(MakeBean(30, false));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{20, 7});
    
            epService.EPRuntime.SendEvent(MakeBean(40, true));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{40, 7});
    
            // test big decimal big integer
            stmt.Dispose();
            fields = "c1,c2,c3".Split(',');
            stmt = (EPStatementSPI) epService.EPAdministrator.CreateEPL("select " +
                    "avg(DecimalOne, BigInt < 100) as c1," +
                    "sum(DecimalOne, BigInt < 100) as c2, " +
                    "sum(BigInt, BigInt < 100) as c3 " +
                    "from SupportBeanNumeric#length(2)");
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBeanNumeric(new BigInteger(10), new decimal(20)));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{new decimal(20), new decimal(20), new BigInteger(10)});
    
            epService.EPRuntime.SendEvent(new SupportBeanNumeric(new BigInteger(101), new decimal(101)));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{new decimal(20), new decimal(20), new BigInteger(10)});
    
            epService.EPRuntime.SendEvent(new SupportBeanNumeric(new BigInteger(20), new decimal(40)));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{new decimal(40), new decimal(40), new BigInteger(20)});
    
            epService.EPRuntime.SendEvent(new SupportBeanNumeric(new BigInteger(30), new decimal(50)));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{new decimal(45), new decimal(90), new BigInteger(50)});
    
            stmt.Dispose();
            string epl = "select " +
                    "avedev(distinct IntBoxed,BoolPrimitive) as cavedev, " +
                    "avg(distinct IntBoxed,BoolPrimitive) as cavg, " +
                    "fmax(distinct IntBoxed,BoolPrimitive) as cmax, " +
                    "median(distinct IntBoxed,BoolPrimitive) as cmedian, " +
                    "fmin(distinct IntBoxed,BoolPrimitive) as cmin, " +
                    "stddev(distinct IntBoxed,BoolPrimitive) as cstddev, " +
                    "sum(distinct IntBoxed,BoolPrimitive) as csum " +
                    "from SupportBean#length(3)";
            stmt = (EPStatementSPI) epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += listener.Update;
    
            TryAssertionDistinct(epService, listener);
    
            // test SODA
            stmt.Dispose();
            EPStatementObjectModel model = epService.EPAdministrator.CompileEPL(epl);
            Assert.AreEqual(epl, model.ToEPL());
            stmt = (EPStatementSPI) epService.EPAdministrator.Create(model);
            stmt.Events += listener.Update;
            Assert.AreEqual(epl, stmt.Text);
    
            TryAssertionDistinct(epService, listener);
    
            stmt.Dispose();
        }
    
        private void TryAssertionDistinct(EPServiceProvider epService, SupportUpdateListener listener) {
    
            string[] fields = "cavedev,cavg,cmax,cmedian,cmin,cstddev,csum".Split(',');
            epService.EPRuntime.SendEvent(MakeBean(100, true));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{0d, 100d, 100, 100d, 100, null, 100});
    
            epService.EPRuntime.SendEvent(MakeBean(100, true));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{0d, 100d, 100, 100d, 100, null, 100});
    
            epService.EPRuntime.SendEvent(MakeBean(200, true));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{50d, 150d, 200, 150d, 100, 70.71067811865476, 300});
    
            epService.EPRuntime.SendEvent(MakeBean(200, true));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{50d, 150d, 200, 150d, 100, 70.71067811865476, 300});
    
            epService.EPRuntime.SendEvent(MakeBean(200, true));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{0d, 200d, 200, 200d, 200, null, 200});
        }
    
        private void RunAssertionFirstLastEver(EPServiceProvider epService) {
            TryAssertionFirstLastEver(epService, true);
            TryAssertionFirstLastEver(epService, false);
        }
    
        private void TryAssertionFirstLastEver(EPServiceProvider epService, bool soda) {
            string[] fields = "c1,c2,c3".Split(',');
            string epl = "select " +
                    "firstever(IntBoxed,BoolPrimitive) as c1, " +
                    "lastever(IntBoxed,BoolPrimitive) as c2, " +
                    "countever(*,BoolPrimitive) as c3 " +
                    "from SupportBean#length(3)";
            EPStatement stmt = SupportModelHelper.CreateByCompileOrParse(epService, soda, epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(MakeBean(100, false));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{null, null, 0L});
    
            epService.EPRuntime.SendEvent(MakeBean(100, true));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{100, 100, 1L});
    
            epService.EPRuntime.SendEvent(MakeBean(200, true));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{100, 200, 2L});
    
            epService.EPRuntime.SendEvent(MakeBean(201, false));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{100, 200, 2L});
    
            stmt.Dispose();
        }
    
        private void RunAssertionInvalid(EPServiceProvider epService) {
            TryInvalid(epService, "select count(*, IntPrimitive) from SupportBean",
                    "Error starting statement: Failed to validate select-clause expression 'count(*,IntPrimitive)': Invalid filter expression parameter to the aggregation function 'count' is expected to return a bool value but returns System.Int32 [select count(*, IntPrimitive) from SupportBean]");
    
            TryInvalid(epService, "select fmin(IntPrimitive) from SupportBean",
                    "Error starting statement: Failed to validate select-clause expression 'min(IntPrimitive)': MIN-filtered aggregation function must have a filter expression as a second parameter [select fmin(IntPrimitive) from SupportBean]");
        }
    
        private SupportBean MakeBean(float floatPrimitive, double doublePrimitive, long longPrimitive, short shortPrimitive, bool boolPrimitive) {
            var sb = new SupportBean();
            sb.FloatPrimitive = floatPrimitive;
            sb.DoublePrimitive = doublePrimitive;
            sb.LongPrimitive = longPrimitive;
            sb.ShortPrimitive = shortPrimitive;
            sb.BoolPrimitive = boolPrimitive;
            return sb;
        }
    
        private SupportBean MakeBean(int? intBoxed, bool boolPrimitive) {
            var sb = new SupportBean();
            sb.IntBoxed = intBoxed;
            sb.BoolPrimitive = boolPrimitive;
            return sb;
        }
    
        public class BlackWhiteEvent {
            public bool IsBlack { get; }
            public BlackWhiteEvent(bool black) {
                IsBlack = black;
            }
        }
    }
} // end of namespace
