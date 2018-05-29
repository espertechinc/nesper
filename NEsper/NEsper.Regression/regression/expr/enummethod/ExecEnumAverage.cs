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
using com.espertech.esper.supportregression.util;
using com.espertech.esper.util;

namespace com.espertech.esper.regression.expr.enummethod
{
    public class ExecEnumAverage : RegressionExecution {
    
        public override void Configure(Configuration configuration) {
            configuration.AddEventType("Bean", typeof(SupportBean_Container));
            configuration.AddEventType("SupportCollection", typeof(SupportCollection));
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionAverageEvents(epService);
            RunAssertionAverageScalar(epService);
            RunAssertionInvalid(epService);
        }
    
        private void RunAssertionAverageEvents(EPServiceProvider epService) {
    
            var fields = "val0,val1,val2,val3".Split(',');
            var eplFragment = "select " +
                    "beans.average(x => IntBoxed) as val0," +
                    "beans.average(x => DoubleBoxed) as val1," +
                    "beans.average(x => LongBoxed) as val2," +
                    "beans.average(x => decimalBoxed) as val3 " +
                    "from Bean";
            var stmtFragment = epService.EPAdministrator.CreateEPL(eplFragment);
            var listener = new SupportUpdateListener();
            stmtFragment.Events += listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, fields, new Type[]{typeof(double?), typeof(double?), typeof(double?), typeof(decimal?)});
    
            epService.EPRuntime.SendEvent(new SupportBean_Container(null));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{null, null, null, null});
    
            epService.EPRuntime.SendEvent(new SupportBean_Container(Collections.GetEmptyList<SupportBean>()));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{null, null, null, null});
    
            var list = new List<SupportBean>();
            list.Add(Make(2, 3d, 4L, 5));
            epService.EPRuntime.SendEvent(new SupportBean_Container(list));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{2d, 3d, 4d, 5.0m});
    
            list.Add(Make(4, 6d, 8L, 10));
            epService.EPRuntime.SendEvent(new SupportBean_Container(list));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{(2 + 4) / 2d, (3d + 6d) / 2d, (4L + 8L) / 2d, (decimal) ((5 + 10) / 2d)});
    
            stmtFragment.Dispose();
        }
    
        private void RunAssertionAverageScalar(EPServiceProvider epService) {
    
            var fields = "val0,val1".Split(',');
            var eplFragment = "select " +
                    "Intvals.average() as val0," +
                    "Bdvals.average() as val1 " +
                    "from SupportCollection";
            var stmtFragment = epService.EPAdministrator.CreateEPL(eplFragment);
            var listener = new SupportUpdateListener();
            stmtFragment.Events += listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, fields, new Type[]{typeof(double?), typeof(decimal?)});
    
            epService.EPRuntime.SendEvent(SupportCollection.MakeNumeric("1,2,3"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{2d, 2m});
    
            epService.EPRuntime.SendEvent(SupportCollection.MakeNumeric("1,null,3"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{2d, 2m});
    
            epService.EPRuntime.SendEvent(SupportCollection.MakeNumeric("4"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{4d, 4m});
            stmtFragment.Dispose();
    
            // test average with lambda
            epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("extractNum", typeof(ExecEnumMinMax.MyService), "ExtractNum");
            epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("extractDecimal", typeof(ExecEnumMinMax.MyService), "ExtractDecimal");
    
            var fieldsLambda = "val0,val1".Split(',');
            var eplLambda = "select " +
                    "Strvals.average(v => extractNum(v)) as val0, " +
                    "Strvals.average(v => extractDecimal(v)) as val1 " +
                    "from SupportCollection";
            var stmtLambda = epService.EPAdministrator.CreateEPL(eplLambda);
            stmtLambda.Events += listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtLambda.EventType, fieldsLambda, new Type[]{typeof(double?), typeof(decimal?)});
    
            epService.EPRuntime.SendEvent(SupportCollection.MakeString("E2,E1,E5,E4"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsLambda, new object[]{(2 + 1 + 5 + 4) / 4d, (decimal) ((2 + 1 + 5 + 4) / 4d)});
    
            epService.EPRuntime.SendEvent(SupportCollection.MakeString("E1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsLambda, new object[]{1d, 1m});
    
            epService.EPRuntime.SendEvent(SupportCollection.MakeString(null));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsLambda, new object[]{null, null});
    
            epService.EPRuntime.SendEvent(SupportCollection.MakeString(""));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsLambda, new object[]{null, null});
    
            stmtLambda.Dispose();
        }
    
        private void RunAssertionInvalid(EPServiceProvider epService) {
            string epl;
    
            epl = "select Strvals.average() from SupportCollection";
            SupportMessageAssertUtil.TryInvalid(epService, epl, "Error starting statement: Failed to validate select-clause expression 'Strvals.average()': Invalid input for built-in enumeration method 'average' and 0-parameter footprint, expecting collection of numeric values as input, received collection of String [select Strvals.average() from SupportCollection]");
    
            epl = "select Beans.average() from Bean";
            SupportMessageAssertUtil.TryInvalid(epService, epl, "Error starting statement: Failed to validate select-clause expression 'Beans.average()': Invalid input for built-in enumeration method 'average' and 0-parameter footprint, expecting collection of values (typically scalar values) as input, received collection of events of type '" + typeof(SupportBean).GetCleanName() + "'");
        }
    
        private SupportBean Make(int? intBoxed, double? doubleBoxed, long longBoxed, int decimalBoxed) {
            var bean = new SupportBean();
            bean.IntBoxed = intBoxed;
            bean.DoubleBoxed = doubleBoxed;
            bean.LongBoxed = longBoxed;
            bean.DecimalBoxed = decimalBoxed;
            return bean;
        }
    }
} // end of namespace
