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
using com.espertech.esper.filter;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;

using static com.espertech.esper.supportregression.util.SupportSpatialUtil;

using NUnit.Framework;

namespace com.espertech.esper.regression.spatial
{
    public class ExecSpatialMXCIFQuadTreeFilterIndex : RegressionExecution {
    
        public override void Configure(Configuration configuration) {
            configuration.EngineDefaults.Execution.IsAllowIsolatedService = true;
            configuration.EngineDefaults.Logging.IsEnableQueryPlan = true;
        }
    
        public override void Run(EPServiceProvider epService)
        {
            var clazzes = Collections.List(
                typeof(SupportSpatialAABB), 
                typeof(SupportSpatialEventRectangle), 
                typeof(SupportSpatialDualAABB));

            foreach (var clazz in clazzes) {
                epService.EPAdministrator.Configuration.AddEventType(clazz);
            }
    
            RunAssertionFilterIndexPerfPattern(epService);
            RunAssertionFilterIndexTypeAssertion(epService);
        }
    
        private void RunAssertionFilterIndexTypeAssertion(EPServiceProvider epService) {
            string eplNoIndex = "select * from SupportSpatialEventRectangle(rectangle(0, 0, 1, 1).intersects(rectangle(x, y, width, height)))";
            SupportFilterHelper.AssertFilterMulti(epService, eplNoIndex, "SupportSpatialEventRectangle", new SupportFilterItem[][] {
                new []{SupportFilterItem.BoolExprFilterItem}
            });
    
            string eplIndexed = "expression myindex {mxcifquadtree(0, 0, 100, 100)}" +
                    "select * from SupportSpatialEventRectangle(rectangle(10, 20, 5, 6, filterindex:myindex).intersects(rectangle(x, y, width, height)))";
            EPStatement statement = SupportFilterHelper.AssertFilterMulti(epService, eplIndexed, "SupportSpatialEventRectangle", new SupportFilterItem[][] {
                new []{new SupportFilterItem("x,y,width,height/myindex/mxcifquadtree/0.0,0.0,100.0,100.0,4.0,20.0", FilterOperator.ADVANCED_INDEX)}
            });
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            SendAssertEventRectangle(epService, listener, 10, 20, 0, 0, true);
            SendAssertEventRectangle(epService, listener, 9, 19, 0.9999, 0.9999, false);
            SendAssertEventRectangle(epService, listener, 9, 19, 1, 1, true);
            SendAssertEventRectangle(epService, listener, 15, 26, 0, 0, true);
            SendAssertEventRectangle(epService, listener, 15.001, 26.001, 0, 0, false);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionFilterIndexPerfPattern(EPServiceProvider epService) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL("expression myindex {mxcifquadtree(0, 0, 100, 100)}" +
                    "select * from pattern [every p=SupportSpatialEventRectangle -> SupportSpatialAABB(rectangle(p.x, p.y, p.width, p.height, filterindex:myindex).intersects(rectangle(x, y, width, height)))]");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendSpatialEventRectangles(epService, 100, 50);
            SendAssertSpatialAABB(epService, listener, 100, 50, 1000);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        internal static void SendAssertEventRectangle(EPServiceProvider epService, SupportUpdateListener listener, double x, double y, double width, double height, bool expected) {
            epService.EPRuntime.SendEvent(new SupportSpatialEventRectangle(null, x, y, width, height));
            Assert.AreEqual(expected, listener.IsInvokedAndReset());
        }
    
        internal static void SendSpatialEventRectangles(EPServiceProvider epService, int numX, int numY) {
            for (int x = 0; x < numX; x++) {
                for (int y = 0; y < numY; y++) {
                    epService.EPRuntime.SendEvent(new SupportSpatialEventRectangle(Convert.ToString(x) + "_" + Convert.ToString(y), (double) x, (double) y, 0.1, 0.2));
                }
            }
        }
    }
} // end of namespace
