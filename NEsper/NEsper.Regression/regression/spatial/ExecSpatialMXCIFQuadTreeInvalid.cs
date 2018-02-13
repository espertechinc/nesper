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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.spatial
{
    public class ExecSpatialMXCIFQuadTreeInvalid : RegressionExecution {
    
        public override void Configure(Configuration configuration) {
            configuration.EngineDefaults.Execution.IsAllowIsolatedService = true;
            configuration.EngineDefaults.Logging.IsEnableQueryPlan = true;
        }
    
        public override void Run(EPServiceProvider epService) {
            foreach (var clazz in Collections.List(typeof(SupportSpatialAABB), typeof(SupportSpatialEventRectangle), typeof(SupportSpatialDualAABB))) {
                epService.EPAdministrator.Configuration.AddEventType(clazz);
            }
    
            // invalid-testing overlaps with pointregion-quadtree
            RunAssertionInvalidEventIndexCreate(epService);
            RunAssertionInvalidEventIndexRuntime(epService);
            RunAssertionInvalidMethod(epService);
            RunAssertionInvalidFilterIndex(epService);
    
            RunAssertionDocSample(epService);
        }
    
        private void RunAssertionInvalidFilterIndex(EPServiceProvider epService) {
            // invalid index for filter
            string epl = "expression myindex {Pointregionquadtree(0, 0, 100, 100)}" +
                    "select * from SupportSpatialEventRectangle(Rectangle(10, 20, 5, 6, filterindex:myindex).Intersects(Rectangle(x, y, width, height)))";
            SupportMessageAssertUtil.TryInvalid(epService, epl, "Failed to validate filter expression 'Rectangle(10,20,5,6,filterindex:myi...(82 chars)': Invalid index type 'pointregionquadtree', expected 'mxcifquadtree'");
        }
    
        private void RunAssertionInvalidMethod(EPServiceProvider epService) {
            SupportMessageAssertUtil.TryInvalid(epService, "select * from SupportSpatialEventRectangle(Rectangle('a', 0).Inside(Rectangle(0, 0, 0, 0)))",
                    "Failed to validate filter expression 'Rectangle(\"a\",0).Inside(Rectangle(0...(43 chars)': Failed to validate method-chain parameter expression 'Rectangle(0,0,0,0)': Unknown single-row function, expression declaration, script or aggregation function named 'rectangle' could not be resolved (did you mean 'rectangle.intersects')");
            SupportMessageAssertUtil.TryInvalid(epService, "select * from SupportSpatialEventRectangle(Rectangle(0).Intersects(Rectangle(0, 0, 0, 0)))",
                    "Failed to validate filter expression 'Rectangle(0).Intersects(Rectangle(0...(43 chars)': Error validating left-hand-side method 'rectangle', expected 4 parameters but received 1 parameters");
        }
    
        private void RunAssertionInvalidEventIndexRuntime(EPServiceProvider epService) {
            string epl = "@Name('mywindow') create window RectangleWindow#keepall as SupportSpatialEventRectangle;\n" +
                    "insert into RectangleWindow select * from SupportSpatialEventRectangle;\n" +
                    "create index MyIndex on RectangleWindow((x, y, width, height) Mxcifquadtree(0, 0, 100, 100));\n";
            epService.EPAdministrator.DeploymentAdmin.ParseDeploy(epl);
    
            try {
                epService.EPRuntime.SendEvent(new SupportSpatialEventRectangle("E1", null, null, null, null, "category"));
            } catch (Exception ex) {
                SupportMessageAssertUtil.AssertMessage(ex, "Unexpected exception in statement 'mywindow': Invalid value for index 'MyIndex' column 'x' received null and expected non-null");
            }
    
            try {
                epService.EPRuntime.SendEvent(new SupportSpatialEventRectangle("E1", 200d, 200d, 1, 1));
            } catch (Exception ex) {
                SupportMessageAssertUtil.AssertMessage(ex, "Unexpected exception in statement 'mywindow': Invalid value for index 'MyIndex' column '(x,y,width,height)' received (200.0,200.0,1.0,1.0) and expected a value intersecting index bounding box (range-end-inclusive) {minX=0.0, minY=0.0, maxX=100.0, maxY=100.0}");
            }
        }
    
        private void RunAssertionInvalidEventIndexCreate(EPServiceProvider epService) {
            // most are covered by point-region test already
            epService.EPAdministrator.CreateEPL("create window MyWindow#keepall as SupportSpatialEventRectangle");
    
            // invalid number of columns
            SupportMessageAssertUtil.TryInvalid(epService, "create index MyIndex on MyWindow(x Mxcifquadtree(0, 0, 100, 100))",
                    "Error starting statement: Index of type 'mxcifquadtree' requires 4 expressions as index columns but received 1");
    
            // same index twice, by-columns
            epService.EPAdministrator.CreateEPL("create window SomeWindow#keepall as SupportSpatialEventRectangle");
            epService.EPAdministrator.CreateEPL("create index SomeWindowIdx1 on SomeWindow((x, y, width, height) Mxcifquadtree(0, 0, 1, 1))");
            SupportMessageAssertUtil.TryInvalid(epService, "create index SomeWindowIdx2 on SomeWindow((x, y, width, height) Mxcifquadtree(0, 0, 1, 1))",
                    "Error starting statement: An index for the same columns already Exists");
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionDocSample(EPServiceProvider epService) {
            string epl = "create table RectangleTable(rectangleId string primary key, rx double, ry double, rwidth double, rheight double);\n" +
                    "create index RectangleIndex on RectangleTable((rx, ry, rwidth, rheight) Mxcifquadtree(0, 0, 100, 100));\n" +
                    "create schema OtherRectangleEvent(otherX double, otherY double, otherWidth double, otherHeight double);\n" +
                    "on OtherRectangleEvent\n" +
                    "select rectangleId from RectangleTable\n" +
                    "where Rectangle(rx, ry, rwidth, rheight).Intersects(Rectangle(otherX, otherY, otherWidth, otherHeight));" +
                    "expression myMXCIFQuadtreeSettings { Mxcifquadtree(0, 0, 100, 100) } \n" +
                    "select * from SupportSpatialAABB(Rectangle(10, 20, 5, 5, filterindex:myMXCIFQuadtreeSettings).Intersects(Rectangle(x, y, width, height)));\n";
            string deploymentId = epService.EPAdministrator.DeploymentAdmin.ParseDeploy(epl).DeploymentId;
    
            epService.EPAdministrator.DeploymentAdmin.Undeploy(deploymentId);
        }
    }
} // end of namespace
