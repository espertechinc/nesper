///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

namespace com.espertech.esper.regressionlib.suite.epl.spatial
{
    public class EPLSpatialMXCIFQuadTreeInvalid
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithInvalidEventIndexCreate(execs);
            WithInvalidEventIndexRuntime(execs);
            WithInvalidMethod(execs);
            WithInvalidFilterIndex(execs);
            WithDocSample(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithDocSample(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSpatialDocSample());
            return execs;
        }

        public static IList<RegressionExecution> WithInvalidFilterIndex(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSpatialInvalidFilterIndex());
            return execs;
        }

        public static IList<RegressionExecution> WithInvalidMethod(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSpatialInvalidMethod());
            return execs;
        }

        public static IList<RegressionExecution> WithInvalidEventIndexRuntime(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSpatialInvalidEventIndexRuntime());
            return execs;
        }

        public static IList<RegressionExecution> WithInvalidEventIndexCreate(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            // invalid-testing overlaps with pointregion-quadtree
            execs.Add(new EPLSpatialInvalidEventIndexCreate());
            return execs;
        }

        private class EPLSpatialInvalidFilterIndex : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // invalid index for filter
                var epl = "expression myindex {pointregionquadtree(0, 0, 100, 100)}" +
                          "select * from SupportSpatialEventRectangle(rectangle(10, 20, 5, 6, filterindex:myindex).intersects(rectangle(x, y, width, height)))";
                env.TryInvalidCompile(
                    epl,
                    "Failed to validate filter expression 'rectangle(10,20,5,6,filterindex:myi...(82 chars)': Invalid index type 'pointregionquadtree', expected 'mxcifquadtree'");
            }
        }

        private class EPLSpatialInvalidMethod : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.TryInvalidCompile(
                    "select * from SupportSpatialEventRectangle(rectangle('a', 0).inside(rectangle(0, 0, 0, 0)))",
                    "Failed to validate filter expression 'rectangle(\"a\",0).inside(rectangle(0...(43 chars)': Failed to validate method-chain parameter expression 'rectangle(0,0,0,0)': Unknown single-row function, expression declaration, script or aggregation function named 'rectangle' could not be resolved (did you mean 'rectangle.intersects')");
                env.TryInvalidCompile(
                    "select * from SupportSpatialEventRectangle(rectangle(0).intersects(rectangle(0, 0, 0, 0)))",
                    "Failed to validate filter expression 'rectangle(0).intersects(rectangle(0...(43 chars)': Failed to validate left-hand-side method 'rectangle', expected 4 parameters but received 1 parameters");
            }
        }

        private class EPLSpatialInvalidEventIndexRuntime : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('mywindow') create window RectangleWindow#keepall as SupportSpatialEventRectangle;\n" +
                          "insert into RectangleWindow select * from SupportSpatialEventRectangle;\n" +
                          "create index MyIndex on RectangleWindow((x, y, width, height) mxcifquadtree(0, 0, 100, 100));\n";
                env.CompileDeploy(epl);

                try {
                    env.SendEventBean(new SupportSpatialEventRectangle("E1", null, null, null, null, "category"));
                }
                catch (Exception ex) {
                    SupportMessageAssertUtil.AssertMessage(
                        ex,
                        "Unexpected exception in statement 'mywindow': Invalid value for index 'MyIndex' column 'x' received null and expected non-null");
                }

                try {
                    env.SendEventBean(new SupportSpatialEventRectangle("E1", 200d, 200d, 1, 1));
                }
                catch (Exception ex) {
                    SupportMessageAssertUtil.AssertMessage(
                        ex,
                        "Unexpected exception in statement 'mywindow': Invalid value for index 'MyIndex' column '(x,y,width,height)' received (200.0,200.0,1.0,1.0) and expected a value intersecting index bounding box (range-end-inclusive) {minX=0.0, minY=0.0, maxX=100.0, maxY=100.0}");
                }

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.INVALIDITY);
            }
        }

        private class EPLSpatialInvalidEventIndexCreate : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // most are covered by point-region test already
                var path = new RegressionPath();
                env.CompileDeploy("@public create window MyWindow#keepall as SupportSpatialEventRectangle", path);

                // invalid number of columns
                env.TryInvalidCompile(
                    path,
                    "create index MyIndex on MyWindow(x mxcifquadtree(0, 0, 100, 100))",
                    "Index of type 'mxcifquadtree' requires 4 expressions as index columns but received 1");

                // same index twice, by-columns
                env.CompileDeploy("@public create window SomeWindow#keepall as SupportSpatialEventRectangle", path);
                env.CompileDeploy(
                    "create index SomeWindowIdx1 on SomeWindow((x, y, width, height) mxcifquadtree(0, 0, 1, 1))",
                    path);
                env.TryInvalidCompile(
                    path,
                    "create index SomeWindowIdx2 on SomeWindow((x, y, width, height) mxcifquadtree(0, 0, 1, 1))",
                    "An index for the same columns already exists");

                env.UndeployAll();
            }
        }

        private class EPLSpatialDocSample : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "create table RectangleTable(rectangleId string primary key, rx double, ry double, rwidth double, rheight double);\n" +
                    "create index RectangleIndex on RectangleTable((rx, ry, rwidth, rheight) mxcifquadtree(0, 0, 100, 100));\n" +
                    "create schema OtherRectangleEvent(otherX double, otherY double, otherWidth double, otherHeight double);\n" +
                    "on OtherRectangleEvent\n" +
                    "select rectangleId from RectangleTable\n" +
                    "where rectangle(rx, ry, rwidth, rheight).intersects(rectangle(otherX, otherY, otherWidth, otherHeight));" +
                    "expression myMXCIFQuadtreeSettings { mxcifquadtree(0, 0, 100, 100) } \n" +
                    "select * from SupportSpatialAABB(rectangle(10, 20, 5, 5, filterindex:myMXCIFQuadtreeSettings).intersects(rectangle(x, y, width, height)));\n";
                env.CompileDeploy(epl).UndeployAll();
            }
        }
    }
} // end of namespace