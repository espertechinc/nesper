///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.util;


using NUnit.Framework;

namespace com.espertech.esper.regression.expr.expr
{
    public class ExecExprMinMaxNonAgg : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            RunAssertionMinMaxWindowStats(epService);
            RunAssertionMinMaxWindowStats_OM(epService);
            RunAssertionMinMaxWindowStats_Compile(epService);
        }
    
        private void RunAssertionMinMaxWindowStats(EPServiceProvider epService) {
            EPStatement stmt = SetUpMinMax(epService);
            EventType type = stmt.EventType;
            Assert.AreEqual(typeof(long?), type.GetPropertyType("myMax"));
            Assert.AreEqual(typeof(long?), type.GetPropertyType("myMin"));
            Assert.AreEqual(typeof(long?), type.GetPropertyType("myMinEx"));
            Assert.AreEqual(typeof(long?), type.GetPropertyType("myMaxEx"));
    
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            TryMinMaxWindowStats(epService, listener);
    
            stmt.Dispose();
        }
    
        private void RunAssertionMinMaxWindowStats_OM(EPServiceProvider epService) {
            string epl = "select max(LongBoxed,IntBoxed) as myMax, " +
                    "max(LongBoxed,IntBoxed,ShortBoxed) as myMaxEx, " +
                    "min(LongBoxed,IntBoxed) as myMin, " +
                    "min(LongBoxed,IntBoxed,ShortBoxed) as myMinEx" +
                    " from " + typeof(SupportBean).FullName + "#length(3)";
    
            var model = new EPStatementObjectModel();
            model.SelectClause = SelectClause.Create()
                    .Add(Expressions.Max("LongBoxed", "IntBoxed"), "myMax")
                    .Add(Expressions.Max(Expressions.Property("LongBoxed"), Expressions.Property("IntBoxed"), Expressions.Property("ShortBoxed")), "myMaxEx")
                    .Add(Expressions.Min("LongBoxed", "IntBoxed"), "myMin")
                    .Add(Expressions.Min(Expressions.Property("LongBoxed"), Expressions.Property("IntBoxed"), Expressions.Property("ShortBoxed")), "myMinEx");

            model.FromClause = FromClause.Create(FilterStream.Create(typeof(SupportBean).FullName)
                .AddView("length", Expressions.Constant(3)));
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(epService.Container, model);
            Assert.AreEqual(epl, model.ToEPL());
    
            EPStatement stmt = epService.EPAdministrator.Create(model);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            TryMinMaxWindowStats(epService, listener);
    
            stmt.Dispose();
        }
    
        private void RunAssertionMinMaxWindowStats_Compile(EPServiceProvider epService) {
            string epl = "select max(LongBoxed,IntBoxed) as myMax, " +
                    "max(LongBoxed,IntBoxed,ShortBoxed) as myMaxEx, " +
                    "min(LongBoxed,IntBoxed) as myMin, " +
                    "min(LongBoxed,IntBoxed,ShortBoxed) as myMinEx" +
                    " from " + typeof(SupportBean).FullName + "#length(3)";
    
            EPStatementObjectModel model = epService.EPAdministrator.CompileEPL(epl);
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(epService.Container, model);
            Assert.AreEqual(epl, model.ToEPL());
    
            EPStatement stmt = epService.EPAdministrator.Create(model);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            TryMinMaxWindowStats(epService, listener);
    
            stmt.Dispose();
        }
    
        private void TryMinMaxWindowStats(EPServiceProvider epService, SupportUpdateListener listener) {
            SendEvent(epService, 10, 20, (short) 4);
            EventBean received = listener.GetAndResetLastNewData()[0];
            Assert.AreEqual(20L, received.Get("myMax"));
            Assert.AreEqual(10L, received.Get("myMin"));
            Assert.AreEqual(4L, received.Get("myMinEx"));
            Assert.AreEqual(20L, received.Get("myMaxEx"));
    
            SendEvent(epService, -10, -20, (short) -30);
            received = listener.GetAndResetLastNewData()[0];
            Assert.AreEqual(-10L, received.Get("myMax"));
            Assert.AreEqual(-20L, received.Get("myMin"));
            Assert.AreEqual(-30L, received.Get("myMinEx"));
            Assert.AreEqual(-10L, received.Get("myMaxEx"));
        }
    
        private EPStatement SetUpMinMax(EPServiceProvider epService) {
            string epl = "select max(LongBoxed, IntBoxed) as myMax, " +
                    "max(LongBoxed, IntBoxed, ShortBoxed) as myMaxEx," +
                    "min(LongBoxed, IntBoxed) as myMin," +
                    "min(LongBoxed, IntBoxed, ShortBoxed) as myMinEx" +
                    " from " + typeof(SupportBean).FullName + "#length(3) ";
            return epService.EPAdministrator.CreateEPL(epl);
        }
    
        private void SendEvent(EPServiceProvider epService, long longBoxed, int intBoxed, short shortBoxed) {
            SendBoxedEvent(epService, longBoxed, intBoxed, shortBoxed);
        }
    
        private void SendBoxedEvent(EPServiceProvider epService, long longBoxed, int? intBoxed, short? shortBoxed) {
            var bean = new SupportBean();
            bean.LongBoxed = longBoxed;
            bean.IntBoxed = intBoxed;
            bean.ShortBoxed = shortBoxed;
            epService.EPRuntime.SendEvent(bean);
        }
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
} // end of namespace
