///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.deploy;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable.infra
{
    public class ExecNWTableInfraComparative : RegressionExecution {
    
        public override void Run(EPServiceProvider epService) {
            foreach (var clazz in new Type[]{typeof(SupportBean), typeof(SupportBean_S0), typeof(SupportBean_S1)}) {
                epService.EPAdministrator.Configuration.AddEventType(clazz);
            }
    
            string eplNamedWindow =
                    "create window TotalsWindow#unique(TheString) as (TheString string, total int);" +
                            "insert into TotalsWindow select TheString, sum(IntPrimitive) as total from SupportBean group by TheString;" +
                            "@Name('Listen') select p00 as c0, " +
                            "    (select total from TotalsWindow tw where tw.TheString = s0.p00) as c1 from SupportBean_S0 as s0;";
            TryAssertionComparativeGroupByTopLevelSingleAgg(epService, "named window", 1000, eplNamedWindow, 1);
    
            string eplTable =
                    "create table varTotal (key string primary key, total sum(int));\n" +
                            "into table varTotal select TheString, sum(IntPrimitive) as total from SupportBean group by TheString;\n" +
                            "@Name('Listen') select p00 as c0, varTotal[p00].total as c1 from SupportBean_S0;\n";
            TryAssertionComparativeGroupByTopLevelSingleAgg(epService, "table", 1000, eplTable, 1);
        }
    
        private void TryAssertionComparativeGroupByTopLevelSingleAgg(EPServiceProvider epService, string caseName, int numEvents, string epl, int numSets) {
            string[] fields = "c0,c1".Split(',');
            DeploymentResult deployed = epService.EPAdministrator.DeploymentAdmin.ParseDeploy(epl);
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.GetStatement("Listen").Events += listener.Update;
    
            long startLoad = PerformanceObserver.NanoTime;
            for (int i = 0; i < numEvents; i++) {
                epService.EPRuntime.SendEvent(new SupportBean("E" + i, i));
            }
            long deltaLoad = PerformanceObserver.NanoTime - startLoad;
    
            long startQuery = PerformanceObserver.NanoTime;
            for (int j = 0; j < numSets; j++) {
                for (int i = 0; i < numEvents; i++) {
                    string key = "E" + i;
                    epService.EPRuntime.SendEvent(new SupportBean_S0(0, key));
                    EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{key, i});
                }
            }
            long deltaQuery = PerformanceObserver.NanoTime - startQuery;
    
            /** Comment-me-inn:
             Log.Info(caseName + ": Load " + deltaLoad/1000000d +
             " Query " + deltaQuery / 1000000d +
             " Total " + (deltaQuery+deltaLoad) / 1000000d );
             */
            epService.EPAdministrator.DeploymentAdmin.Undeploy(deployed.DeploymentId);
        }
    }
} // end of namespace
