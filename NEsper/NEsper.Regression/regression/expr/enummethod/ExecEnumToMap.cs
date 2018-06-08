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


using NUnit.Framework;

namespace com.espertech.esper.regression.expr.enummethod
{
    using Map = IDictionary<string, object>;

    public class ExecEnumToMap : RegressionExecution {
    
        public override void Configure(Configuration configuration) {
            configuration.AddEventType("Bean", typeof(SupportBean_ST0_Container));
            configuration.AddEventType("SupportCollection", typeof(SupportCollection));
        }
    
        public override void Run(EPServiceProvider epService) {
            // - duplicate value allowed, latest value wins
            // - null key & value allowed
    
            string eplFragment = "select Contained.ToMap(c => id, c=> p00) as val from Bean";
            EPStatement stmtFragment = epService.EPAdministrator.CreateEPL(eplFragment);
            var listener = new SupportUpdateListener();
            stmtFragment.Events += listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, "val".Split(','), new Type[] {
                typeof(IDictionary<object, object>)
            });
    
            epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value("E1,1", "E3,12", "E2,5"));
            EPAssertionUtil.AssertPropsMap(listener.AssertOneGetNewAndReset().Get("val").UnwrapStringDictionary(), "E1,E2,E3".Split(','), new object[]{1, 5, 12});
    
            epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value("E1,1", "E3,12", "E2,12", "E1,2"));
            EPAssertionUtil.AssertPropsMap(listener.AssertOneGetNewAndReset().Get("val").UnwrapStringDictionary(), "E1,E2,E3".Split(','), new object[]{2, 12, 12});
    
            epService.EPRuntime.SendEvent(new SupportBean_ST0_Container(Collections.SingletonList(new SupportBean_ST0(null, null))));
            EPAssertionUtil.AssertPropsMap(listener.AssertOneGetNewAndReset().Get("val").UnwrapStringDictionary(), "E1,E2,E3".Split(','), new object[]{null, null, null});
            stmtFragment.Dispose();
    
            // test scalar-coll with lambda
            string[] fields = "val0".Split(',');
            epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("extractNum", typeof(ExecEnumMinMax.MyService), "ExtractNum");
            string eplLambda = "select " +
                    "Strvals.ToMap(c => c, c => extractNum(c)) as val0 " +
                    "from SupportCollection";
            EPStatement stmtLambda = epService.EPAdministrator.CreateEPL(eplLambda);
            stmtLambda.Events += listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtLambda.EventType, fields, new Type[] {
                typeof(IDictionary<object, object>)
            });
    
            epService.EPRuntime.SendEvent(SupportCollection.MakeString("E2,E1,E3"));
            EPAssertionUtil.AssertPropsMap(
                listener.AssertOneGetNewAndReset().Get("val0").UnwrapDictionary(),
                "E1,E2,E3".Split(','), new object[]{1, 2, 3});
    
            epService.EPRuntime.SendEvent(SupportCollection.MakeString("E1"));
            EPAssertionUtil.AssertPropsMap(
                listener.AssertOneGetNewAndReset().Get("val0").UnwrapDictionary(),
                "E1".Split(','), new object[]{1});
    
            epService.EPRuntime.SendEvent(SupportCollection.MakeString(null));
            Assert.IsNull(listener.AssertOneGetNewAndReset().Get("val0"));
    
            epService.EPRuntime.SendEvent(SupportCollection.MakeString(""));
            Assert.AreEqual(0, listener.AssertOneGetNewAndReset().Get("val0").Unwrap<object>().Count);
        }
    }
} // end of namespace
