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

using static com.espertech.esper.supportregression.util.SupportMessageAssertUtil;

namespace com.espertech.esper.regression.expr.enummethod
{
    public class ExecEnumMinMax : RegressionExecution {
    
        public override void Configure(Configuration configuration) {
            configuration.AddEventType("Bean", typeof(SupportBean_ST0_Container));
            configuration.AddEventType("SupportCollection", typeof(SupportCollection));
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionMinMaxScalarWithLambda(epService);
            RunAssertionMinMaxEvents(epService);
            RunAssertionMinMaxScalar(epService);
            RunAssertionInvalid(epService);
        }
    
        private void RunAssertionMinMaxScalarWithLambda(EPServiceProvider epService) {
    
            epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("extractNum", typeof(MyService), "ExtractNum");
    
            string[] fields = "val0,val1,val2,val3".Split(',');
            string eplFragment = "select " +
                    "Strvals.min(v => extractNum(v)) as val0, " +
                    "Strvals.max(v => extractNum(v)) as val1, " +
                    "Strvals.min(v => v) as val2, " +
                    "Strvals.max(v => v) as val3 " +
                    "from SupportCollection";
            EPStatement stmtFragment = epService.EPAdministrator.CreateEPL(eplFragment);
            var listener = new SupportUpdateListener();
            stmtFragment.Events += listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, fields, new Type[] {
                typeof(int?), typeof(int?), typeof(string), typeof(string)
            });
    
            epService.EPRuntime.SendEvent(SupportCollection.MakeString("E2,E1,E5,E4"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{1, 5, "E1", "E5"});
    
            epService.EPRuntime.SendEvent(SupportCollection.MakeString("E1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{1, 1, "E1", "E1"});
    
            epService.EPRuntime.SendEvent(SupportCollection.MakeString(null));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{null, null, null, null});
    
            epService.EPRuntime.SendEvent(SupportCollection.MakeString(""));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{null, null, null, null});
    
            stmtFragment.Dispose();
        }
    
        private void RunAssertionMinMaxEvents(EPServiceProvider epService) {
    
            string[] fields = "val0,val1".Split(',');
            string eplFragment = "select " +
                    "Contained.min(x => p00) as val0, " +
                    "Contained.max(x => p00) as val1 " +
                    "from Bean";
            EPStatement stmtFragment = epService.EPAdministrator.CreateEPL(eplFragment);
            var listener = new SupportUpdateListener();
            stmtFragment.Events += listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, fields, new Type[]{typeof(int?), typeof(int?)});
    
            epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value("E1,12", "E2,11", "E2,2"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{2, 12});
    
            epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value("E1,12", "E2,0", "E2,2"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{0, 12});
    
            epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value(null));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{null, null});
    
            epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value());
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{null, null});
    
            stmtFragment.Dispose();
        }
    
        private void RunAssertionMinMaxScalar(EPServiceProvider epService) {
    
            string[] fields = "val0,val1".Split(',');
            string eplFragment = "select " +
                    "Strvals.min() as val0, " +
                    "Strvals.max() as val1 " +
                    "from SupportCollection";
            EPStatement stmtFragment = epService.EPAdministrator.CreateEPL(eplFragment);
            var listener = new SupportUpdateListener();
            stmtFragment.Events += listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, fields, new Type[] {
                typeof(string), typeof(string)
            });
    
            epService.EPRuntime.SendEvent(SupportCollection.MakeString("E2,E1,E5,E4"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", "E5"});
    
            epService.EPRuntime.SendEvent(SupportCollection.MakeString("E1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", "E1"});
    
            epService.EPRuntime.SendEvent(SupportCollection.MakeString(null));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{null, null});
    
            epService.EPRuntime.SendEvent(SupportCollection.MakeString(""));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{null, null});
    
            stmtFragment.Dispose();
        }
    
        private void RunAssertionInvalid(EPServiceProvider epService) {
            string epl;
    
            epl = "select Contained.min() from Bean";
            TryInvalid(epService, epl, "Error starting statement: Failed to validate select-clause expression 'Contained.min()': Invalid input for built-in enumeration method 'min' and 0-parameter footprint, expecting collection of values (typically scalar values) as input, received collection of events of type '" + typeof(SupportBean_ST0).FullName + "' [select Contained.min() from Bean]");
        }
    
        public class MyService {
            public static int ExtractNum(string arg) {
                return int.Parse(arg.Substring(1));
            }
    
            public static decimal ExtractDecimal(string arg) {
                return decimal.Parse(arg.Substring(1));
            }
        }
    }
} // end of namespace
