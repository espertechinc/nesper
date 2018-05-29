///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.collections;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.bean.lambda;
using com.espertech.esper.supportregression.execution;

namespace com.espertech.esper.regression.expr.enummethod
{
    using Map = IDictionary<string, object>;

    public class ExecEnumSelectFrom : RegressionExecution
    {
        public override void Configure(Configuration configuration) {
            configuration.AddEventType("Bean", typeof(SupportBean_ST0_Container));
            configuration.AddEventType("SupportCollection", typeof(SupportCollection));
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionNew(epService);
            RunAssertionSelect(epService);
        }
    
        private void RunAssertionNew(EPServiceProvider epService) {
    
            string eplFragment = "select " +
                    "Contained.selectFrom(x => new {c0 = id||'x', c1 = key0||'y'}) as val0 " +
                    "from Bean";
            EPStatement stmtFragment = epService.EPAdministrator.CreateEPL(eplFragment);
            var listener = new SupportUpdateListener();
            stmtFragment.Events += listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, "val0".Split(','), new Type[] {
                typeof(ICollection<Map>)
            });
    
            epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make3Value("E1,12,0", "E2,11,0", "E3,2,0"));
            EPAssertionUtil.AssertPropsPerRow(ToMapArray(listener.AssertOneGetNewAndReset().Get("val0")), "c0,c1".Split(','),
                    new object[][]{new object[] {"E1x", "12y"}, new object[] {"E2x", "11y"}, new object[] {"E3x", "2y"}});
    
            epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make3Value("E4,0,1"));
            EPAssertionUtil.AssertPropsPerRow(ToMapArray(listener.AssertOneGetNewAndReset().Get("val0")), "c0,c1".Split(','),
                    new object[][]{new object[] {"E4x", "0y"}});
    
            epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make3Value(null));
            EPAssertionUtil.AssertPropsPerRow(ToMapArray(listener.AssertOneGetNewAndReset().Get("val0")), "c0,c1".Split(','), null);
    
            epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make3Value());
            EPAssertionUtil.AssertPropsPerRow(ToMapArray(listener.AssertOneGetNewAndReset().Get("val0")), "c0,c1".Split(','),
                    new Object[0][]);
    
            stmtFragment.Dispose();
        }
    
        private void RunAssertionSelect(EPServiceProvider epService) {
    
            string eplFragment = "select " +
                    "Contained.selectFrom(x => id) as val0 " +
                    "from Bean";
            EPStatement stmtFragment = epService.EPAdministrator.CreateEPL(eplFragment);
            var listener = new SupportUpdateListener();
            stmtFragment.Events += listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, "val0".Split(','), new Type[]{typeof(ICollection<string>) });
    
            epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value("E1,12", "E2,11", "E3,2"));
            LambdaAssertionUtil.AssertValuesArrayScalar(listener, "val0", "E1", "E2", "E3");
            listener.Reset();
    
            epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value(null));
            LambdaAssertionUtil.AssertValuesArrayScalar(listener, "val0", null);
            listener.Reset();
    
            epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value());
            LambdaAssertionUtil.AssertValuesArrayScalar(listener, "val0", new string[0]);
            listener.Reset();
            stmtFragment.Dispose();
    
            // test scalar-coll with lambda
            string[] fields = "val0".Split(',');
            epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("extractNum", typeof(ExecEnumMinMax.MyService), "ExtractNum");
            string eplLambda = "select " +
                    "Strvals.selectFrom(v => extractNum(v)) as val0 " +
                    "from SupportCollection";
            EPStatement stmtLambda = epService.EPAdministrator.CreateEPL(eplLambda);
            stmtLambda.Events += listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtLambda.EventType, fields, new Type[] {
                typeof(ICollection<int>),
                typeof(ICollection<int>)
            });
    
            epService.EPRuntime.SendEvent(SupportCollection.MakeString("E2,E1,E5,E4"));
            LambdaAssertionUtil.AssertValuesArrayScalar(listener, "val0", 2, 1, 5, 4);
            listener.Reset();
    
            epService.EPRuntime.SendEvent(SupportCollection.MakeString("E1"));
            LambdaAssertionUtil.AssertValuesArrayScalar(listener, "val0", 1);
            listener.Reset();
    
            epService.EPRuntime.SendEvent(SupportCollection.MakeString(null));
            LambdaAssertionUtil.AssertValuesArrayScalar(listener, "val0", null);
            listener.Reset();
    
            epService.EPRuntime.SendEvent(SupportCollection.MakeString(""));
            LambdaAssertionUtil.AssertValuesArrayScalar(listener, "val0");
    
            stmtLambda.Dispose();
        }
    
        private Map[] ToMapArray(Object result) {
            if (result == null) {
                return null;
            }

            return result.Unwrap<object>()
                .Cast<Map>()
                .ToArray();
        }
    }
} // end of namespace
