///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.bean.lambda;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.expr.enummethod
{
    public class ExecEnumFirstLastOf : RegressionExecution {
    
        public override void Configure(Configuration configuration) {
            configuration.AddEventType("Bean", typeof(SupportBean_ST0_Container));
            configuration.AddEventType("SupportCollection", typeof(SupportCollection));
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionFirstLastScalar(epService);
            RunAssertionFirstLastProperty(epService);
            RunAssertionFirstLastNoPred(epService);
            RunAssertionFirstLastPredicate(epService);
        }
    
        private void RunAssertionFirstLastScalar(EPServiceProvider epService) {
    
            string[] fields = "val0,val1,val2,val3".Split(',');
            string eplFragment = "select " +
                    "Strvals.firstOf() as val0, " +
                    "Strvals.lastOf() as val1, " +
                    "Strvals.firstOf(x => x like '%1%') as val2, " +
                    "Strvals.lastOf(x => x like '%1%') as val3 " +
                    " from SupportCollection";
            EPStatement stmtFragment = epService.EPAdministrator.CreateEPL(eplFragment);
            var listener = new SupportUpdateListener();
            stmtFragment.Events += listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, fields, new Type[]{typeof(string), typeof(string), typeof(string), typeof(string)});
    
            epService.EPRuntime.SendEvent(SupportCollection.MakeString("E1,E2,E3"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", "E3", "E1", "E1"});
    
            epService.EPRuntime.SendEvent(SupportCollection.MakeString("E1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", "E1", "E1", "E1"});
    
            epService.EPRuntime.SendEvent(SupportCollection.MakeString("E2,E3,E4"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E2", "E4", null, null});
    
            epService.EPRuntime.SendEvent(SupportCollection.MakeString(""));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{null, null, null, null});
    
            epService.EPRuntime.SendEvent(SupportCollection.MakeString(null));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{null, null, null, null});
    
            stmtFragment.Dispose();
        }
    
        private void RunAssertionFirstLastProperty(EPServiceProvider epService) {
    
            string[] fields = "val0,val1".Split(',');
            string eplFragment = "select " +
                    "Contained.firstOf().p00 as val0, " +
                    "Contained.lastOf().p00 as val1 " +
                    " from Bean";
            EPStatement stmtFragment = epService.EPAdministrator.CreateEPL(eplFragment);
            var listener = new SupportUpdateListener();
            stmtFragment.Events += listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, fields, new Type[]{typeof(int?), typeof(int?)});
    
            epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value("E1,1", "E2,9", "E3,3"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{1, 3});
    
            epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value("E1,1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{1, 1});
    
            epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value(null));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{null, null});
    
            epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value());
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{null, null});
    
            stmtFragment.Dispose();
        }
    
        private void RunAssertionFirstLastNoPred(EPServiceProvider epService) {
    
            string eplFragment = "select " +
                    "Contained.firstOf() as val0, " +
                    "Contained.lastOf() as val1 " +
                    " from Bean";
            EPStatement stmtFragment = epService.EPAdministrator.CreateEPL(eplFragment);
            var listener = new SupportUpdateListener();
            stmtFragment.Events += listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, "val0,val1".Split(','), new Type[]{typeof(SupportBean_ST0), typeof(SupportBean_ST0)});
    
            epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value("E1,1", "E3,9", "E2,9"));
            AssertId(listener, "val0", "E1");
            AssertId(listener, "val1", "E2");
            listener.Reset();
    
            epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value("E2,2"));
            AssertId(listener, "val0", "E2");
            AssertId(listener, "val1", "E2");
            listener.Reset();
    
            epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value(null));
            Assert.IsNull(listener.AssertOneGetNew().Get("val0"));
            Assert.IsNull(listener.AssertOneGetNewAndReset().Get("val1"));
    
            epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value());
            Assert.IsNull(listener.AssertOneGetNew().Get("val0"));
            Assert.IsNull(listener.AssertOneGetNewAndReset().Get("val1"));
    
            stmtFragment.Dispose();
        }
    
        private void RunAssertionFirstLastPredicate(EPServiceProvider epService) {
    
            string eplFragment = "select Contained.firstOf(x => p00 = 9) as val from Bean";
            EPStatement stmtFragment = epService.EPAdministrator.CreateEPL(eplFragment);
            var listener = new SupportUpdateListener();
            stmtFragment.Events += listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, "val".Split(','), new Type[]{typeof(SupportBean_ST0)});
    
            SupportBean_ST0_Container bean = SupportBean_ST0_Container.Make2Value("E1,1", "E2,9", "E2,9");
            epService.EPRuntime.SendEvent(bean);
            SupportBean_ST0 result = (SupportBean_ST0) listener.AssertOneGetNewAndReset().Get("val");
            Assert.AreSame(result, bean.Contained[1]);
    
            epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value(null));
            Assert.IsNull(listener.AssertOneGetNewAndReset().Get("val"));
    
            epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value());
            Assert.IsNull(listener.AssertOneGetNewAndReset().Get("val"));
    
            epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value("E1,1", "E2,1", "E2,1"));
            Assert.IsNull(listener.AssertOneGetNewAndReset().Get("val"));
    
            stmtFragment.Dispose();
        }
    
        private void AssertId(SupportUpdateListener listener, string property, string id) {
            SupportBean_ST0 result = (SupportBean_ST0) listener.AssertOneGetNew().Get(property);
            Assert.AreEqual(id, result.Id);
        }
    }
} // end of namespace
