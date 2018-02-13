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

using static com.espertech.esper.supportregression.util.SupportMessageAssertUtil;

using NUnit.Framework;

namespace com.espertech.esper.regression.expr.enummethod
{
    public class ExecEnumSequenceEqual : RegressionExecution {
    
        public override void Configure(Configuration configuration) {
            configuration.AddEventType("SupportBean_ST0_Container", typeof(SupportBean_ST0_Container));
            configuration.AddEventType("SupportCollection", typeof(SupportCollection));
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionSelectFrom(epService);
            RunAssertionTwoProperties(epService);
            RunAssertionInvalid(epService);
        }
    
        private void RunAssertionSelectFrom(EPServiceProvider epService) {
            string[] fields = "val0".Split(',');
            string eplFragment = "select Contained.SelectFrom(x => key0).SequenceEqual(contained.SelectFrom(y => id)) as val0 " +
                    "from SupportBean_ST0_Container";
            EPStatement stmtFragment = epService.EPAdministrator.CreateEPL(eplFragment);
            var listener = new SupportUpdateListener();
            stmtFragment.AddListener(listener);
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, "val0".Split(','), new Type[]{typeof(bool?)});
    
            epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make3Value("I1,E1,0", "I2,E2,0"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{false});
    
            epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make3Value("I3,I3,0", "X4,X4,0"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{true});
    
            epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make3Value("I3,I3,0", "X4,Y4,0"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{false});
    
            epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make3Value("I3,I3,0", "Y4,X4,0"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{false});
    
            stmtFragment.Dispose();
        }
    
        private void RunAssertionTwoProperties(EPServiceProvider epService) {
    
            string[] fields = "val0".Split(',');
            string eplFragment = "select " +
                    "strvals.SequenceEqual(strvalstwo) as val0 " +
                    "from SupportCollection";
            EPStatement stmtFragment = epService.EPAdministrator.CreateEPL(eplFragment);
            var listener = new SupportUpdateListener();
            stmtFragment.AddListener(listener);
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, "val0".Split(','), new Type[]{typeof(bool?)});
    
            epService.EPRuntime.SendEvent(SupportCollection.MakeString("E1,E2,E3", "E1,E2,E3"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{true});
    
            epService.EPRuntime.SendEvent(SupportCollection.MakeString("E1,E3", "E1,E2,E3"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{false});
    
            epService.EPRuntime.SendEvent(SupportCollection.MakeString("E1,E3", "E1,E3"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{true});
    
            epService.EPRuntime.SendEvent(SupportCollection.MakeString("E1,E2,E3", "E1,E3"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{false});
    
            epService.EPRuntime.SendEvent(SupportCollection.MakeString("E1,E2,null,E3", "E1,E2,null,E3"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{true});
    
            epService.EPRuntime.SendEvent(SupportCollection.MakeString("E1,E2,E3", "E1,E2,null"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{false});
    
            epService.EPRuntime.SendEvent(SupportCollection.MakeString("E1,E2,null", "E1,E2,E3"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{false});
    
            epService.EPRuntime.SendEvent(SupportCollection.MakeString("E1", ""));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{false});
    
            epService.EPRuntime.SendEvent(SupportCollection.MakeString("", "E1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{false});
    
            epService.EPRuntime.SendEvent(SupportCollection.MakeString("E1", "E1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{true});
    
            epService.EPRuntime.SendEvent(SupportCollection.MakeString("", ""));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{true});
    
            epService.EPRuntime.SendEvent(SupportCollection.MakeString(null, ""));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{null});
    
            epService.EPRuntime.SendEvent(SupportCollection.MakeString("", null));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{false});
    
            epService.EPRuntime.SendEvent(SupportCollection.MakeString(null, null));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{null});
    
            stmtFragment.Dispose();
        }
    
        private void RunAssertionInvalid(EPServiceProvider epService) {
            string epl;
    
            epl = "select window(*).SequenceEqual(strvals) from SupportCollection#lastevent";
            TryInvalid(epService, epl, "Error starting statement: Failed to validate select-clause expression 'window(*).SequenceEqual(strvals)': Invalid input for built-in enumeration method 'sequenceEqual' and 1-parameter footprint, expecting collection of values (typically scalar values) as input, received collection of events of type 'SupportCollection' [select window(*).SequenceEqual(strvals) from SupportCollection#lastevent]");
        }
    }
} // end of namespace
