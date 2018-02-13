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
    public class ExecSpatialPointRegionQuadTreeInvalid : RegressionExecution {
    
        public override void Configure(Configuration configuration) {
            configuration.EngineDefaults.Execution.IsAllowIsolatedService = true;
            configuration.EngineDefaults.Logging.IsEnableQueryPlan = true;
        }
    
        public override void Run(EPServiceProvider epService) {
            foreach (var clazz in Collections.List(typeof(SupportSpatialPoint), typeof(SupportSpatialAABB), typeof(SupportSpatialDualPoint), typeof(ExecSpatialPointRegionQuadTreeEventIndex.MyEventRectangleWithOffset))) {
                epService.EPAdministrator.Configuration.AddEventType(clazz);
            }
    
            RunAssertionInvalidEventIndexCreate(epService);
            RunAssertionInvalidEventIndexRuntime(epService);
            RunAssertionInvalidMethod(epService);
            RunAssertionInvalidFilterIndex(epService);
    
            RunAssertionDocSample(epService);
        }
    
        private void RunAssertionInvalidEventIndexCreate(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create window MyWindow#keepall as SupportSpatialPoint");
    
            // invalid number of columns
            SupportMessageAssertUtil.TryInvalid(epService, "create index MyIndex on MyWindow(px Pointregionquadtree(0, 0, 100, 100))",
                    "Error starting statement: Index of type 'pointregionquadtree' requires 2 expressions as index columns but received 1");
    
            // invalid column type
            SupportMessageAssertUtil.TryInvalid(epService, "create index MyIndex on MyWindow((id, py) Pointregionquadtree(0, 0, 100, 100))",
                    "Error starting statement: Index of type 'pointregionquadtree' for column 0 that is providing x-values expecting type java.lang.Number but received type java.lang.string");
            SupportMessageAssertUtil.TryInvalid(epService, "create index MyIndex on MyWindow((px, id) Pointregionquadtree(0, 0, 100, 100))",
                    "Error starting statement: Index of type 'pointregionquadtree' for column 1 that is providing y-values expecting type java.lang.Number but received type java.lang.string");
    
            // invalid expressions for column or parameter
            SupportMessageAssertUtil.TryInvalid(epService, "create index MyIndex on MyWindow((dummy, dummy2) Pointregionquadtree(0, 0, 100, 100))",
                    "Error starting statement: Failed to validate create-index index-column expression 'dummy': Property named 'dummy' is not valid in any stream");
            SupportMessageAssertUtil.TryInvalid(epService, "create index MyIndex on MyWindow((px, py) Pointregionquadtree(dummy, 0, 100, 100))",
                    "Error starting statement: Failed to validate create-index index-parameter expression 'dummy': Property named 'dummy' is not valid in any stream");
    
            // invalid property use in parameter
            SupportMessageAssertUtil.TryInvalid(epService, "create index MyIndex on MyWindow((px, py) Pointregionquadtree(px, 0, 100, 100))",
                    "Error starting statement: Index parameters may not refer to event properties");
    
            // invalid number of parameters
            SupportMessageAssertUtil.TryInvalid(epService, "create index MyIndex on MyWindow((px, py) pointregionquadtree)",
                    "Error starting statement: Index of type 'pointregionquadtree' requires at least 4 parameters but received 0");
            SupportMessageAssertUtil.TryInvalid(epService, "create index MyIndex on MyWindow((px, py) Pointregionquadtree('a'))",
                    "Error starting statement: Index of type 'pointregionquadtree' requires at least 4 parameters but received 1");
            SupportMessageAssertUtil.TryInvalid(epService, "create index MyIndex on MyWindow((px, py) Pointregionquadtree(0, 0, 0, 0, 0, 0, 0))",
                    "Error starting statement: Index of type 'pointregionquadtree' requires at least 4 parameters but received 7");
    
            // invalid parameter type
            SupportMessageAssertUtil.TryInvalid(epService, "create index MyIndex on MyWindow((px, py) Pointregionquadtree('a', 0, 100, 100))",
                    "Error starting statement: Index of type 'pointregionquadtree' for parameter 0 that is providing xMin-values expecting type java.lang.Number but received type java.lang.string");
            SupportMessageAssertUtil.TryInvalid(epService, "create index MyIndex on MyWindow((px, py) Pointregionquadtree(0, 'a', 100, 100))",
                    "Error starting statement: Index of type 'pointregionquadtree' for parameter 1 that is providing yMin-values expecting type java.lang.Number but received type java.lang.string");
            SupportMessageAssertUtil.TryInvalid(epService, "create index MyIndex on MyWindow((px, py) Pointregionquadtree(0, 0, 'a', 100))",
                    "Error starting statement: Index of type 'pointregionquadtree' for parameter 2 that is providing width-values expecting type java.lang.Number but received type java.lang.string");
            SupportMessageAssertUtil.TryInvalid(epService, "create index MyIndex on MyWindow((px, py) Pointregionquadtree(0, 0, 100, 'a'))",
                    "Error starting statement: Index of type 'pointregionquadtree' for parameter 3 that is providing height-values expecting type java.lang.Number but received type java.lang.string");
            SupportMessageAssertUtil.TryInvalid(epService, "create index MyIndex on MyWindow((px, py) Pointregionquadtree(0, 0, 100, 100, 'a'))",
                    "Error starting statement: Index of type 'pointregionquadtree' for parameter 4 that is providing leafCapacity-values expecting type java.lang.int? but received type java.lang.string");
            SupportMessageAssertUtil.TryInvalid(epService, "create index MyIndex on MyWindow((px, py) Pointregionquadtree(0, 0, 100, 100, 1, 'a'))",
                    "Error starting statement: Index of type 'pointregionquadtree' for parameter 5 that is providing maxTreeHeight-values expecting type java.lang.int? but received type java.lang.string");
    
            // invalid parameter value
            SupportMessageAssertUtil.TryInvalid(epService, "create index MyIndex on MyWindow((px, py) Pointregionquadtree(Cast(null, double), 0, 0, 0))",
                    "Unexpected exception starting statement: Invalid value for index 'MyIndex' parameter 'xMin' received null and expected non-null");
            SupportMessageAssertUtil.TryInvalid(epService, "create index MyIndex on MyWindow((py, px) Pointregionquadtree(0, 0, -100, 0))",
                    "Unexpected exception starting statement: Invalid value for index 'MyIndex' parameter 'width' received -100.0 and expected value>0");
            SupportMessageAssertUtil.TryInvalid(epService, "create index MyIndex on MyWindow((py, px) Pointregionquadtree(0, 0, 1, -200))",
                    "Unexpected exception starting statement: Invalid value for index 'MyIndex' parameter 'height' received -200.0 and expected value>0");
            SupportMessageAssertUtil.TryInvalid(epService, "create index MyIndex on MyWindow((py, px) Pointregionquadtree(0, 0, 1, 1, -1))",
                    "Unexpected exception starting statement: Invalid value for index 'MyIndex' parameter 'leafCapacity' received -1 and expected value>=1");
            SupportMessageAssertUtil.TryInvalid(epService, "create index MyIndex on MyWindow((py, px) Pointregionquadtree(0, 0, 1, 1, 10, -1))",
                    "Unexpected exception starting statement: Invalid value for index 'MyIndex' parameter 'maxTreeHeight' received -1 and expected value>=2");
    
            // same index twice, by-name and by-columns
            epService.EPAdministrator.CreateEPL("create window SomeWindow#keepall as SupportSpatialPoint");
            epService.EPAdministrator.CreateEPL("create index SomeWindowIdx1 on SomeWindow((px, py) Pointregionquadtree(0, 0, 1, 1))");
            SupportMessageAssertUtil.TryInvalid(epService, "create index SomeWindowIdx2 on SomeWindow((px, py) Pointregionquadtree(0, 0, 1, 1))",
                    "Error starting statement: An index for the same columns already Exists");
            SupportMessageAssertUtil.TryInvalid(epService, "create index SomeWindowIdx1 on SomeWindow((py, px) Pointregionquadtree(0, 0, 1, 1))",
                    "Error starting statement: An index by name 'SomeWindowIdx1' already Exists");
    
            // non-plain column or parameter expressions
            SupportMessageAssertUtil.TryInvalid(epService, "create index MyIndexInv on MyWindow((sum(px), py) Pointregionquadtree(0, 0, 1, 1))",
                    "Error starting statement: Invalid create-index index-column expression 'sum(px)': Aggregation, sub-select, previous or prior functions are not supported in this context");
            SupportMessageAssertUtil.TryInvalid(epService, "create index MyIndexInv on MyWindow((px, py) Pointregionquadtree(count(*), 0, 1, 1))",
                    "Error starting statement: Invalid create-index index-parameter expression 'count(*)': Aggregation, sub-select, previous or prior functions are not supported in this context");
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionInvalidEventIndexRuntime(EPServiceProvider epService) {
            string epl = "@Name('mywindow') create window PointWindow#keepall as SupportSpatialPoint;\n" +
                    "insert into PointWindow select * from SupportSpatialPoint;\n" +
                    "create index MyIndex on PointWindow((px, py) Pointregionquadtree(0, 0, 100, 100));\n";
            epService.EPAdministrator.DeploymentAdmin.ParseDeploy(epl);
    
            try {
                epService.EPRuntime.SendEvent(new SupportSpatialPoint("E1", null, null));
            } catch (Exception ex) {
                SupportMessageAssertUtil.AssertMessage(ex, "Unexpected exception in statement 'mywindow': Invalid value for index 'MyIndex' column 'x' received null and expected non-null");
            }
    
            try {
                epService.EPRuntime.SendEvent(new SupportSpatialPoint("E1", 200d, 200d));
            } catch (Exception ex) {
                SupportMessageAssertUtil.AssertMessage(ex, "Unexpected exception in statement 'mywindow': Invalid value for index 'MyIndex' column '(x,y)' received (200.0,200.0) and expected a value within index bounding box (range-end-non-inclusive) {minX=0.0, minY=0.0, maxX=100.0, maxY=100.0}");
            }
        }
    
        private void RunAssertionInvalidMethod(EPServiceProvider epService) {
            SupportMessageAssertUtil.TryInvalid(epService, "select * from MyEventRectangleWithOffset(Point('a', 0).Inside(Rectangle(0, 0, 0, 0)))",
                    "Failed to validate filter expression 'Point(\"a\",0).Inside(Rectangle(0,0,0,0))': Error validating left-hand-side function 'point', expected a number-type result for expression parameter 0 but received java.lang.string");
            SupportMessageAssertUtil.TryInvalid(epService, "select * from MyEventRectangleWithOffset(Point(0).Inside(Rectangle(0, 0, 0, 0)))",
                    "Failed to validate filter expression 'Point(0).Inside(Rectangle(0,0,0,0))': Error validating left-hand-side method 'point', expected 2 parameters but received 1 parameters");
            SupportMessageAssertUtil.TryInvalid(epService, "select * from MyEventRectangleWithOffset(Point(0,0).Inside(Rectangle('a', 0, 0, 0)))",
                    "Failed to validate filter expression 'Point(0,0).Inside(Rectangle(\"a\",0,0,0))': Error validating right-hand-side function 'rectangle', expected a number-type result for expression parameter 0 but received java.lang.string");
            SupportMessageAssertUtil.TryInvalid(epService, "select * from MyEventRectangleWithOffset(Point(0,0).Inside(Rectangle(0)))",
                    "Failed to validate filter expression 'Point(0,0).Inside(Rectangle(0))': Error validating right-hand-side function 'rectangle', expected 4 parameters but received 1 parameters");
            SupportMessageAssertUtil.TryInvalid(epService, "select * from MyEventRectangleWithOffset(Point(0,0).Inside(0))",
                    "Failed to validate filter expression 'Point(0,0).Inside(0)': point.inside requires a single rectangle as parameter");
        }
    
        private void RunAssertionDocSample(EPServiceProvider epService) {
            string epl =
                    "create table PointTable(pointId string primary key, px double, py double);\n" +
                            "create index PointIndex on PointTable((px, py) Pointregionquadtree(0, 0, 100, 100));\n" +
                            "create schema RectangleEvent(rx double, ry double, w double, h double);\n" +
                            "on RectangleEvent select pointId from PointTable where Point(px, py).Inside(Rectangle(rx, ry, w, h));" +
                            "expression myQuadtreeSettings { Pointregionquadtree(0, 0, 100, 100) } \n" +
                            "select * from SupportSpatialAABB(Point(0, 0, filterindex:myQuadtreeSettings).Inside(Rectangle(x, y, width, height)));\n";
            string deploymentId = epService.EPAdministrator.DeploymentAdmin.ParseDeploy(epl).DeploymentId;
    
            epService.EPAdministrator.DeploymentAdmin.Undeploy(deploymentId);
        }
    
        private void RunAssertionInvalidFilterIndex(EPServiceProvider epService) {
            // unrecognized named parameter
            SupportMessageAssertUtil.TryInvalid(epService, "select * from SupportSpatialAABB#keepall where Point(0, 0, a:1).Inside(Rectangle(x, y, width, height))",
                    "Error validating expression: Failed to validate filter expression 'Point(0,0,a:1).Inside(Rectangle(x,y...(50 chars)': point does not accept 'a' as a named parameter");
    
            // not a filter
            SupportMessageAssertUtil.TryInvalid(epService, "expression myindex {Pointregionquadtree(0, 0, 100, 100)} select * from SupportSpatialAABB#keepall where Point(0, 0, filterindex:myindex).Inside(Rectangle(x, y, width, height))",
                    "Error validating expression: Failed to validate filter expression 'Point(0,0,filterindex:Myindex()).in...(68 chars)': The 'filterindex' named parameter can only be used in in filter expressions");
    
            // invalid index expression
            SupportMessageAssertUtil.TryInvalid(epService, "select * from SupportSpatialAABB(Point(0, 0, filterindex:1).Inside(Rectangle(x, y, width, height)))",
                    "Failed to validate filter expression 'Point(0,0,filterindex:1).Inside(rec...(60 chars)': Named parameter 'filterindex' requires an expression name");
            SupportMessageAssertUtil.TryInvalid(epService, "select * from SupportSpatialAABB(Point(0, 0, filterindex:dummy).Inside(Rectangle(x, y, width, height)))",
                    "Failed to validate filter expression 'Point(0,0,filterindex:dummy).inside...(64 chars)': Named parameter 'filterindex' requires an expression name");
            SupportMessageAssertUtil.TryInvalid(epService, "expression myindex {0} select * from SupportSpatialAABB(Point(0, 0, filterindex:myindex).Inside(Rectangle(x, y, width, height)))",
                    "Failed to validate filter expression 'Point(0,0,filterindex:Myindex()).in...(68 chars)': Named parameter 'filterindex' requires an index expression");
            SupportMessageAssertUtil.TryInvalid(epService, "expression myindex {Dummy(0)} select * from SupportSpatialAABB(Point(0, 0, filterindex:myindex).Inside(Rectangle(x, y, width, height)))",
                    "Failed to validate filter expression 'Point(0,0,filterindex:Myindex()).in...(68 chars)': Unrecognized advanced-type index 'dummy'");
            SupportMessageAssertUtil.TryInvalid(epService, "expression myindex {Pointregionquadtree(0)} select * from SupportSpatialAABB(Point(0, 0, filterindex:myindex).Inside(Rectangle(x, y, width, height)))",
                    "Failed to validate filter expression 'Point(0,0,filterindex:Myindex()).in...(68 chars)': Index of type 'pointregionquadtree' requires at least 4 parameters but received 1 [");
            SupportMessageAssertUtil.TryInvalid(epService, "expression myindex {Pointregionquadtree(0,0,0,0)} select * from SupportSpatialAABB(Point(0, 0, filterindex:myindex).Inside(Rectangle(x, y, width, height)))",
                    "Failed to validate filter expression 'Point(0,0,filterindex:Myindex()).in...(68 chars)': Invalid value for index 'myindex' parameter 'width' received 0.0 and expected value>0");
            SupportMessageAssertUtil.TryInvalid(epService, "expression myindex {Pointregionquadtree(0,0,100,100).Help()} select * from SupportSpatialAABB(Point(0, 0, filterindex:myindex).Inside(Rectangle(x, y, width, height)))",
                    "Failed to validate filter expression 'Point(0,0,filterindex:Myindex()).in...(68 chars)': Named parameter 'filterindex' invalid chained index expression");
    
            // filter-not-optimizable
            SupportMessageAssertUtil.TryInvalid(epService, "expression myindex {Pointregionquadtree(0, 0, 100, 100)} select * from SupportSpatialAABB(Point(x, y, filterindex:myindex).Inside(Rectangle(x, y, width, height)))",
                    "Invalid filter-indexable expression 'x' in respect to index 'myindex': expected either a constant, context-builtin or property from a previous pattern match [expression myindex {Pointregionquadtree(0, 0, 100, 100)} select * from SupportSpatialAABB(Point(x, y, filterindex:myindex).Inside(Rectangle(x, y, width, height)))]");
            SupportMessageAssertUtil.TryInvalid(epService, "expression myindex {Pointregionquadtree(0, 0, 100, 100)} select * from SupportSpatialAABB(Point(0, 0, filterindex:myindex).Inside(Rectangle(0, y, width, height)))",
                    "Invalid filter-index lookup expression '0' in respect to index 'myindex': expected an event property");
        }
    }
} // end of namespace
