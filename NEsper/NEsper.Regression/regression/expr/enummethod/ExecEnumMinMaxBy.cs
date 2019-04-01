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

namespace com.espertech.esper.regression.expr.enummethod
{
    public class ExecEnumMinMaxBy : RegressionExecution {
    
        public override void Configure(Configuration configuration) {
            configuration.AddEventType("Bean", typeof(SupportBean_ST0_Container));
            configuration.AddEventType("SupportCollection", typeof(SupportCollection));
        }
    
        public override void Run(EPServiceProvider epService) {
            string[] fields = "val0,val1,val2,val3".Split(',');
            string eplFragment = "select " +
                    "Contained.minBy(x => p00) as val0," +
                    "Contained.MaxBy(x => p00) as val1," +
                    "Contained.minBy(x => p00).id as val2," +
                    "Contained.MaxBy(x => p00).p00 as val3 " +
                    "from Bean";
            EPStatement stmtFragment = epService.EPAdministrator.CreateEPL(eplFragment);
            var listener = new SupportUpdateListener();
            stmtFragment.Events += listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, fields, new Type[]{typeof(SupportBean_ST0), typeof(SupportBean_ST0), typeof(string), typeof(int?)});
    
            SupportBean_ST0_Container bean = SupportBean_ST0_Container.Make2Value("E1,12", "E2,11", "E2,2");
            epService.EPRuntime.SendEvent(bean);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields,
                    new object[]{bean.Contained[2], bean.Contained[0], "E2", 12});
    
            bean = SupportBean_ST0_Container.Make2Value("E1,12");
            epService.EPRuntime.SendEvent(bean);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields,
                    new object[]{bean.Contained[0], bean.Contained[0], "E1", 12});
    
            epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value(null));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields,
                    new object[]{null, null, null, null});
    
            epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value());
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields,
                    new object[]{null, null, null, null});
            stmtFragment.Dispose();
    
            // test scalar-coll with lambda
            string[] fieldsLambda = "val0,val1".Split(',');
            epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("extractNum", typeof(ExecEnumMinMax.MyService), "ExtractNum");
            string eplLambda = "select " +
                    "Strvals.minBy(v => extractNum(v)) as val0, " +
                    "Strvals.MaxBy(v => extractNum(v)) as val1 " +
                    "from SupportCollection";
            EPStatement stmtLambda = epService.EPAdministrator.CreateEPL(eplLambda);
            stmtLambda.Events += listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtLambda.EventType, fieldsLambda, new Type[]{typeof(string), typeof(string)});
    
            epService.EPRuntime.SendEvent(SupportCollection.MakeString("E2,E1,E5,E4"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsLambda, new object[]{"E1", "E5"});
    
            epService.EPRuntime.SendEvent(SupportCollection.MakeString("E1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsLambda, new object[]{"E1", "E1"});
            listener.Reset();
    
            epService.EPRuntime.SendEvent(SupportCollection.MakeString(null));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsLambda, new object[]{null, null});
            listener.Reset();
    
            epService.EPRuntime.SendEvent(SupportCollection.MakeString(""));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsLambda, new object[]{null, null});
        }
    }
} // end of namespace
