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
    public class ExecEnumOrderBy : RegressionExecution {
    
        public override void Configure(Configuration configuration) {
            configuration.AddEventType("Bean", typeof(SupportBean_ST0_Container));
            configuration.AddEventType("SupportCollection", typeof(SupportCollection));
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionOrderByEvents(epService);
            RunAssertionOrderByScalar(epService);
            RunAssertionInvalid(epService);
        }
    
        private void RunAssertionOrderByEvents(EPServiceProvider epService) {
    
            string[] fields = "val0,val1,val2,val3,val4,val5".Split(',');
            string eplFragment = "select " +
                    "contained.OrderBy(x => p00) as val0," +
                    "contained.OrderBy(x => 10 - p00) as val1," +
                    "contained.OrderBy(x => 0) as val2," +
                    "contained.OrderByDesc(x => p00) as val3," +
                    "contained.OrderByDesc(x => 10 - p00) as val4," +
                    "contained.OrderByDesc(x => 0) as val5" +
                    " from Bean";
            EPStatement stmtFragment = epService.EPAdministrator.CreateEPL(eplFragment);
            var listener = new SupportUpdateListener();
            stmtFragment.AddListener(listener);
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, fields, new Type[]{typeof(Collection), typeof(Collection), typeof(Collection), typeof(Collection), typeof(Collection), typeof(Collection)});
    
            epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value("E1,1", "E2,2"));
            LambdaAssertionUtil.AssertST0Id(listener, "val0", "E1,E2");
            LambdaAssertionUtil.AssertST0Id(listener, "val1", "E2,E1");
            LambdaAssertionUtil.AssertST0Id(listener, "val2", "E1,E2");
            LambdaAssertionUtil.AssertST0Id(listener, "val3", "E2,E1");
            LambdaAssertionUtil.AssertST0Id(listener, "val4", "E1,E2");
            LambdaAssertionUtil.AssertST0Id(listener, "val5", "E1,E2");
            listener.Reset();
    
            epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value("E3,1", "E2,2", "E4,1", "E1,2"));
            LambdaAssertionUtil.AssertST0Id(listener, "val0", "E3,E4,E2,E1");
            LambdaAssertionUtil.AssertST0Id(listener, "val1", "E2,E1,E3,E4");
            LambdaAssertionUtil.AssertST0Id(listener, "val2", "E3,E2,E4,E1");
            LambdaAssertionUtil.AssertST0Id(listener, "val3", "E2,E1,E3,E4");
            LambdaAssertionUtil.AssertST0Id(listener, "val4", "E3,E4,E2,E1");
            LambdaAssertionUtil.AssertST0Id(listener, "val5", "E3,E2,E4,E1");
            listener.Reset();
    
            epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value(null));
            foreach (string field in fields) {
                LambdaAssertionUtil.AssertST0Id(listener, field, null);
            }
            listener.Reset();
    
            epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value());
            foreach (string field in fields) {
                LambdaAssertionUtil.AssertST0Id(listener, field, "");
            }
            listener.Reset();
    
            stmtFragment.Dispose();
        }
    
        private void RunAssertionOrderByScalar(EPServiceProvider epService) {
    
            string[] fields = "val0,val1".Split(',');
            string eplFragment = "select " +
                    "strvals.OrderBy() as val0, " +
                    "strvals.OrderByDesc() as val1 " +
                    "from SupportCollection";
            EPStatement stmtFragment = epService.EPAdministrator.CreateEPL(eplFragment);
            var listener = new SupportUpdateListener();
            stmtFragment.AddListener(listener);
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, fields, new Type[]{typeof(Collection), typeof(Collection)});
    
            epService.EPRuntime.SendEvent(SupportCollection.MakeString("E2,E1,E5,E4"));
            LambdaAssertionUtil.AssertValuesArrayScalar(listener, "val0", "E1", "E2", "E4", "E5");
            LambdaAssertionUtil.AssertValuesArrayScalar(listener, "val1", "E5", "E4", "E2", "E1");
            listener.Reset();
    
            LambdaAssertionUtil.AssertSingleAndEmptySupportColl(epService, listener, fields);
            stmtFragment.Dispose();
    
            // test scalar-coll with lambda
            epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("extractNum", typeof(ExecEnumMinMax.MyService).Name, "extractNum");
            string eplLambda = "select " +
                    "strvals.OrderBy(v => ExtractNum(v)) as val0, " +
                    "strvals.OrderByDesc(v => ExtractNum(v)) as val1 " +
                    "from SupportCollection";
            EPStatement stmtLambda = epService.EPAdministrator.CreateEPL(eplLambda);
            stmtLambda.AddListener(listener);
            LambdaAssertionUtil.AssertTypes(stmtLambda.EventType, fields, new Type[]{typeof(Collection), typeof(Collection)});
    
            epService.EPRuntime.SendEvent(SupportCollection.MakeString("E2,E1,E5,E4"));
            LambdaAssertionUtil.AssertValuesArrayScalar(listener, "val0", "E1", "E2", "E4", "E5");
            LambdaAssertionUtil.AssertValuesArrayScalar(listener, "val1", "E5", "E4", "E2", "E1");
            listener.Reset();
    
            LambdaAssertionUtil.AssertSingleAndEmptySupportColl(epService, listener, fields);
    
            stmtLambda.Dispose();
        }
    
        private void RunAssertionInvalid(EPServiceProvider epService) {
            string epl;
    
            epl = "select Contained.OrderBy() from Bean";
            TryInvalid(epService, epl, "Error starting statement: Failed to validate select-clause expression 'contained.OrderBy()': Invalid input for built-in enumeration method 'orderBy' and 0-parameter footprint, expecting collection of values (typically scalar values) as input, received collection of events of type '" + typeof(SupportBean_ST0).Name + "' [select Contained.OrderBy() from Bean]");
        }
    }
} // end of namespace
