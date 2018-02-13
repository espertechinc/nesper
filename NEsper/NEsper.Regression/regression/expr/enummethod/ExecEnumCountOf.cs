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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.bean.lambda;
using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

namespace com.espertech.esper.regression.expr.enummethod
{
    public class ExecEnumCountOf : RegressionExecution {
    
        public override void Configure(Configuration configuration) {
            configuration.AddEventType("Bean", typeof(SupportBean_ST0_Container));
            configuration.AddEventType("SupportCollection", typeof(SupportCollection));
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionCountOfEvents(epService);
            RunAssertionCountOfScalar(epService);
        }
    
        private void RunAssertionCountOfEvents(EPServiceProvider epService) {
    
            var fields = new string[]{"val0", "val1"};
            string eplFragment = "select " +
                    "contained.Countof(x=> x.p00 = 9) as val0, " +
                    "contained.Countof() as val1 " +
                    " from Bean";
            EPStatement stmtFragment = epService.EPAdministrator.CreateEPL(eplFragment);
            var listener = new SupportUpdateListener();
            stmtFragment.AddListener(listener);
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, fields, new Type[]{typeof(int?), typeof(int?)});
    
            epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value("E1,1", "E2,9", "E2,9"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields,
                    new Object[]{2, 3});
    
            epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value(null));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields,
                    new Object[]{null, null});
    
            epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value(new string[0]));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields,
                    new Object[]{0, 0});
    
            epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value("E1,9"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields,
                    new Object[]{1, 1});
    
            epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value("E1,1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields,
                    new Object[]{0, 1});
    
            stmtFragment.Dispose();
        }
    
        private void RunAssertionCountOfScalar(EPServiceProvider epService) {
    
            var fields = new string[]{"val0", "val1"};
            string eplFragment = "select " +
                    "strvals.Countof() as val0, " +
                    "strvals.Countof(x => x = 'E1') as val1 " +
                    " from SupportCollection";
            EPStatement stmtFragment = epService.EPAdministrator.CreateEPL(eplFragment);
            var listener = new SupportUpdateListener();
            stmtFragment.AddListener(listener);
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, fields, new Type[]{typeof(int?), typeof(int?)});
    
            epService.EPRuntime.SendEvent(SupportCollection.MakeString("E1,E2"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{2, 1});
    
            epService.EPRuntime.SendEvent(SupportCollection.MakeString("E1,E2,E1,E3"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{4, 2});
    
            stmtFragment.Dispose();
        }
    }
} // end of namespace
