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
    public class ExecSpatialPointRegionQuadTreeFilterIndex : RegressionExecution {
    
        public override void Configure(Configuration configuration) {
            configuration.EngineDefaults.Execution.IsAllowIsolatedService = true;
            configuration.EngineDefaults.Logging.IsEnableQueryPlan = true;
        }
    
        public override void Run(EPServiceProvider epService) {
            foreach (var clazz in Collections.List(typeof(SupportSpatialPoint), typeof(SupportSpatialAABB), typeof(SupportSpatialDualPoint))) {
                epService.EPAdministrator.Configuration.AddEventType(clazz);
            }
    
            RunAssertionFilterIndexPerfStatement(epService);
            RunAssertionFilterIndexPerfContextPartition(epService);
            RunAssertionFilterIndexPerfPattern(epService);
            RunAssertionFilterIndexUnoptimized(epService);
            RunAssertionFilterIndexTypeAssertion(epService);
        }
    
        private void RunAssertionFilterIndexTypeAssertion(EPServiceProvider epService) {
            string eplNoIndex = "select * from SupportSpatialAABB(Point(0, 0).Inside(Rectangle(x, y, width, height)))";
            SupportFilterHelper.AssertFilterMulti(epService, eplNoIndex, "SupportSpatialAABB", new SupportFilterItem[][] {
                new []{SupportFilterItem.GetBoolExprFilterItem()}
            });
    
            string eplIndexed = "expression myindex {Pointregionquadtree(0, 0, 100, 100)}" +
                    "select * from SupportSpatialAABB(Point(0, 0, filterindex:myindex).Inside(Rectangle(x, y, width, height)))";
            SupportFilterHelper.AssertFilterMulti(epService, eplIndexed, "SupportSpatialAABB", new SupportFilterItem[][] {
                new []{new SupportFilterItem("x,y,width,height/myindex/pointregionquadtree/0.0,0.0,100.0,100.0,4.0,20.0", FilterOperator.ADVANCED_INDEX)}
            });
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
    
        private void RunAssertionFilterIndexUnoptimized(EPServiceProvider epService) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select * from SupportSpatialAABB(Point(5, 10).Inside(Rectangle(x, y, width, height)))");
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
    
            SendRectangle(epService, "R1", 0, 0, 5, 10);
            SendRectangle(epService, "R2", 4, 3, 2, 20);
            Assert.AreEqual("R2", listener.AssertOneGetNewAndReset().Get("id"));
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionFilterIndexPerfStatement(EPServiceProvider epService) {
            EPPreparedStatement prepared = epService.EPAdministrator.PrepareEPL("expression myindex {Pointregionquadtree(0, 0, 100, 100)}" +
                    "select * from SupportSpatialAABB(Point(?, ?, filterindex:myindex).Inside(Rectangle(x, y, width, height)))");
            var listener = new SupportUpdateListener();
    
            for (int x = 0; x < 100; x++) {
                for (int y = 0; y < 20; y++) {
                    prepared.SetObject(1, x);
                    prepared.SetObject(2, y);
                    epService.EPAdministrator.Create(prepared).AddListener(listener);
                }
            }
            SendAssertSpatialAABB(epService, listener, 100, 20, 1000);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionFilterIndexPerfPattern(EPServiceProvider epService) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL("expression myindex {Pointregionquadtree(0, 0, 100, 100)}" +
                    "select * from pattern [every p=SupportSpatialPoint -> SupportSpatialAABB(Point(p.px, p.py, filterindex:myindex).Inside(Rectangle(x, y, width, height)))]");
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
    
            SendSpatialPoints(epService, 100, 100);
            SendAssertSpatialAABB(epService, listener, 100, 100, 1000);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionFilterIndexPerfContextPartition(EPServiceProvider epService) {
    
            epService.EPAdministrator.CreateEPL("create context PerPointCtx initiated by SupportSpatialPoint ssp");
            EPStatement stmt = epService.EPAdministrator.CreateEPL("expression myindex {Pointregionquadtree(0, 0, 100, 100)}" +
                    "context PerPointCtx select count(*) from SupportSpatialAABB(Point(context.ssp.px, context.ssp.py, filterindex:myindex).Inside(Rectangle(x, y, width, height)))");
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
    
            SendSpatialPoints(epService, 100, 100);
            SendAssertSpatialAABB(epService, listener, 100, 100, 1000);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    }
} // end of namespace
