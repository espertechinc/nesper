///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.collections;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.bean.lambda;
using com.espertech.esper.supportregression.execution;

using static com.espertech.esper.supportregression.util.SupportMessageAssertUtil;

namespace com.espertech.esper.regression.expr.enummethod
{
    public class ExecEnumSumOf : RegressionExecution {
    
        public override void Configure(Configuration configuration) {
            configuration.AddEventType("Bean", typeof(SupportBean_Container));
            configuration.AddEventType("SupportCollection", typeof(SupportCollection));
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionSumEvents(epService);
            RunAssertionSumOfScalar(epService);
            RunAssertionInvalid(epService);
        }
    
        private void RunAssertionSumEvents(EPServiceProvider epService) {
    
            string[] fields = "val0,val1,val2,val3".Split(',');
            string eplFragment = "select " +
                    "beans.sumOf(x => IntBoxed) as val0," +
                    "beans.sumOf(x => DoubleBoxed) as val1," +
                    "beans.sumOf(x => LongBoxed) as val2," +
                    "beans.sumOf(x => decimalBoxed) as val3 " +
                    "from Bean";
            EPStatement stmtFragment = epService.EPAdministrator.CreateEPL(eplFragment);
            var listener = new SupportUpdateListener();
            stmtFragment.Events += listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, fields, new Type[] {
                typeof(int?), typeof(double?),
                typeof(long?), typeof(decimal?)
            });
    
            epService.EPRuntime.SendEvent(new SupportBean_Container(null));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{null, null, null, null});
    
            epService.EPRuntime.SendEvent(new SupportBean_Container(Collections.GetEmptyList<SupportBean>()));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{null, null, null, null});
    
            var list = new List<SupportBean>();
            list.Add(Make(2, 3d, 4L, 5));
            epService.EPRuntime.SendEvent(new SupportBean_Container(list));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{2, 3d, 4L, 5m});
    
            list.Add(Make(4, 6d, 8L, 10));
            epService.EPRuntime.SendEvent(new SupportBean_Container(list));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{2 + 4, 3d + 6d, 4L + 8L, 5m + 10m});
    
            stmtFragment.Dispose();
        }
    
        private void RunAssertionSumOfScalar(EPServiceProvider epService) {
    
            string[] fields = "val0,val1".Split(',');
            string eplFragment = "select " +
                    "Intvals.sumOf() as val0, " +
                    "Bdvals.sumOf() as val1 " +
                    "from SupportCollection";
            EPStatement stmtFragment = epService.EPAdministrator.CreateEPL(eplFragment);
            var listener = new SupportUpdateListener();
            stmtFragment.Events += listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, fields, new Type[] {
                typeof(int?), typeof(decimal?)
            });
    
            epService.EPRuntime.SendEvent(SupportCollection.MakeNumeric("1,4,5"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{1 + 4 + 5, 1m + 4m + 5m});
    
            epService.EPRuntime.SendEvent(SupportCollection.MakeNumeric("3,4"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{3 + 4, 3m + 4m});
    
            epService.EPRuntime.SendEvent(SupportCollection.MakeNumeric("3"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{3, 3m});
    
            epService.EPRuntime.SendEvent(SupportCollection.MakeNumeric(""));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{null, null});
    
            epService.EPRuntime.SendEvent(SupportCollection.MakeNumeric(null));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{null, null});
    
            stmtFragment.Dispose();
    
            // test average with lambda
            epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("extractNum", typeof(ExecEnumMinMax.MyService), "ExtractNum");
            epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("extractDecimal", typeof(ExecEnumMinMax.MyService), "ExtractDecimal");
    
            // lambda with string-array input
            string[] fieldsLambda = "val0,val1".Split(',');
            string eplLambda = "select " +
                    "Strvals.sumOf(v => extractNum(v)) as val0, " +
                    "Strvals.sumOf(v => extractDecimal(v)) as val1 " +
                    "from SupportCollection";
            EPStatement stmtLambda = epService.EPAdministrator.CreateEPL(eplLambda);
            stmtLambda.Events += listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtLambda.EventType, fieldsLambda, new Type[]{typeof(int?), typeof(decimal?)});
    
            epService.EPRuntime.SendEvent(SupportCollection.MakeString("E2,E1,E5,E4"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsLambda, new object[]{2 + 1 + 5 + 4, 2m + 1m + 5m + 4m});
    
            epService.EPRuntime.SendEvent(SupportCollection.MakeString("E1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsLambda, new object[]{1, 1m});
    
            epService.EPRuntime.SendEvent(SupportCollection.MakeString(null));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsLambda, new object[]{null, null});
    
            epService.EPRuntime.SendEvent(SupportCollection.MakeString(""));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsLambda, new object[]{null, null});
    
            stmtLambda.Dispose();
        }
    
        private SupportBean Make(int? intBoxed, double? doubleBoxed, long longBoxed, int decimalBoxed) {
            var bean = new SupportBean();
            bean.IntBoxed = intBoxed;
            bean.DoubleBoxed = doubleBoxed;
            bean.LongBoxed = longBoxed;
            bean.DecimalBoxed = decimalBoxed;
            return bean;
        }
    
        private void RunAssertionInvalid(EPServiceProvider epService) {
            string epl;
    
            epl = "select Beans.sumof() from Bean";
            TryInvalid(epService, epl, "Error starting statement: Failed to validate select-clause expression 'Beans.sumof()': Invalid input for built-in enumeration method 'sumof' and 0-parameter footprint, expecting collection of values (typically scalar values) as input, received collection of events of type '" + typeof(SupportBean).FullName + "' [select Beans.sumof() from Bean]");
        }
    }
} // end of namespace
