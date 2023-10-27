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
    public class EPLSpatialPointRegionQuadTreeInvalid
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
            execs.Add(new EPLSpatialInvalidEventIndexCreate());
            return execs;
        }

        private class EPLSpatialInvalidEventIndexCreate : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@public create window MyWindow#keepall as SupportSpatialPoint", path);

                // invalid number of columns
                env.TryInvalidCompile(
                    path,
                    "create index MyIndex on MyWindow(px pointregionquadtree(0, 0, 100, 100))",
                    "Index of type 'pointregionquadtree' requires 2 expressions as index columns but received 1");

                // invalid column type
                env.TryInvalidCompile(
                    path,
                    "create index MyIndex on MyWindow((Id, py) pointregionquadtree(0, 0, 100, 100))",
                    "Index of type 'pointregionquadtree' for column 0 that is providing x-values expecting type Number but received type String");
                env.TryInvalidCompile(
                    path,
                    "create index MyIndex on MyWindow((px, Id) pointregionquadtree(0, 0, 100, 100))",
                    "Index of type 'pointregionquadtree' for column 1 that is providing y-values expecting type Number but received type String");

                // invalid expressions for column or parameter
                env.TryInvalidCompile(
                    path,
                    "create index MyIndex on MyWindow((dummy, dummy2) pointregionquadtree(0, 0, 100, 100))",
                    "Failed to validate create-index index-column expression 'dummy': Property named 'dummy' is not valid in any stream");
                env.TryInvalidCompile(
                    path,
                    "create index MyIndex on MyWindow((px, py) pointregionquadtree(dummy, 0, 100, 100))",
                    "Failed to validate create-index index-parameter expression 'dummy': Property named 'dummy' is not valid in any stream");

                // invalid property use in parameter
                env.TryInvalidCompile(
                    path,
                    "create index MyIndex on MyWindow((px, py) pointregionquadtree(px, 0, 100, 100))",
                    "Index parameters may not refer to event properties");

                // invalid number of parameters
                env.TryInvalidCompile(
                    path,
                    "create index MyIndex on MyWindow((px, py) pointregionquadtree)",
                    "Index of type 'pointregionquadtree' requires at least 4 parameters but received 0");
                env.TryInvalidCompile(
                    path,
                    "create index MyIndex on MyWindow((px, py) pointregionquadtree('a'))",
                    "Index of type 'pointregionquadtree' requires at least 4 parameters but received 1");
                env.TryInvalidCompile(
                    path,
                    "create index MyIndex on MyWindow((px, py) pointregionquadtree(0, 0, 0, 0, 0, 0, 0))",
                    "Index of type 'pointregionquadtree' requires at least 4 parameters but received 7");

                // invalid parameter type
                env.TryInvalidCompile(
                    path,
                    "create index MyIndex on MyWindow((px, py) pointregionquadtree('a', 0, 100, 100))",
                    "Index of type 'pointregionquadtree' for parameter 0 that is providing xMin-values expecting type Number but received type String");
                env.TryInvalidCompile(
                    path,
                    "create index MyIndex on MyWindow((px, py) pointregionquadtree(0, 'a', 100, 100))",
                    "Index of type 'pointregionquadtree' for parameter 1 that is providing yMin-values expecting type Number but received type String");
                env.TryInvalidCompile(
                    path,
                    "create index MyIndex on MyWindow((px, py) pointregionquadtree(0, 0, 'a', 100))",
                    "Index of type 'pointregionquadtree' for parameter 2 that is providing width-values expecting type Number but received type String");
                env.TryInvalidCompile(
                    path,
                    "create index MyIndex on MyWindow((px, py) pointregionquadtree(0, 0, 100, 'a'))",
                    "Index of type 'pointregionquadtree' for parameter 3 that is providing height-values expecting type Number but received type String");
                env.TryInvalidCompile(
                    path,
                    "create index MyIndex on MyWindow((px, py) pointregionquadtree(0, 0, 100, 100, 'a'))",
                    "Index of type 'pointregionquadtree' for parameter 4 that is providing leafCapacity-values expecting type Integer but received type String");
                env.TryInvalidCompile(
                    path,
                    "create index MyIndex on MyWindow((px, py) pointregionquadtree(0, 0, 100, 100, 1, 'a'))",
                    "Index of type 'pointregionquadtree' for parameter 5 that is providing maxTreeHeight-values expecting type Integer but received type String");

                // invalid parameter value
                SupportMessageAssertUtil.TryInvalidDeploy(
                    env,
                    path,
                    "create index MyIndex on MyWindow((px, py) pointregionquadtree(cast(null, double), 0, 0, 0))",
                    "Failed to deploy: Invalid value for index 'MyIndex' parameter 'xMin' received null and expected non-null");
                SupportMessageAssertUtil.TryInvalidDeploy(
                    env,
                    path,
                    "create index MyIndex on MyWindow((py, px) pointregionquadtree(0, 0, -100, 0))",
                    "Failed to deploy: Invalid value for index 'MyIndex' parameter 'width' received -100.0 and expected value>0");
                SupportMessageAssertUtil.TryInvalidDeploy(
                    env,
                    path,
                    "create index MyIndex on MyWindow((py, px) pointregionquadtree(0, 0, 1, -200))",
                    "Failed to deploy: Invalid value for index 'MyIndex' parameter 'height' received -200.0 and expected value>0");
                SupportMessageAssertUtil.TryInvalidDeploy(
                    env,
                    path,
                    "create index MyIndex on MyWindow((py, px) pointregionquadtree(0, 0, 1, 1, -1))",
                    "Failed to deploy: Invalid value for index 'MyIndex' parameter 'leafCapacity' received -1 and expected value>=1");
                SupportMessageAssertUtil.TryInvalidDeploy(
                    env,
                    path,
                    "create index MyIndex on MyWindow((py, px) pointregionquadtree(0, 0, 1, 1, 10, -1))",
                    "Failed to deploy: Invalid value for index 'MyIndex' parameter 'maxTreeHeight' received -1 and expected value>=2");

                // same index twice, by-name and by-columns
                env.CompileDeploy("@public create window SomeWindow#keepall as SupportSpatialPoint", path);
                env.CompileDeploy(
                    "create index SomeWindowIdx1 on SomeWindow((px, py) pointregionquadtree(0, 0, 1, 1))",
                    path);
                env.TryInvalidCompile(
                    path,
                    "create index SomeWindowIdx2 on SomeWindow((px, py) pointregionquadtree(0, 0, 1, 1))",
                    "An index for the same columns already exists");
                env.TryInvalidCompile(
                    path,
                    "create index SomeWindowIdx1 on SomeWindow((py, px) pointregionquadtree(0, 0, 1, 1))",
                    "An index by name 'SomeWindowIdx1' already exists");

                // non-plain column or parameter expressions
                env.TryInvalidCompile(
                    path,
                    "create index MyIndexInv on MyWindow((sum(px), py) pointregionquadtree(0, 0, 1, 1))",
                    "Invalid create-index index-column expression 'sum(px)': Aggregation, sub-select, previous or prior functions are not supported in this context");
                env.TryInvalidCompile(
                    path,
                    "create index MyIndexInv on MyWindow((px, py) pointregionquadtree(count(*), 0, 1, 1))",
                    "Invalid create-index index-parameter expression 'count(*)': Aggregation, sub-select, previous or prior functions are not supported in this context");

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.INVALIDITY);
            }
        }

        private class EPLSpatialInvalidEventIndexRuntime : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('mywindow') create window PointWindow#keepall as SupportSpatialPoint;\n" +
                          "insert into PointWindow select * from SupportSpatialPoint;\n" +
                          "create index MyIndex on PointWindow((px, py) pointregionquadtree(0, 0, 100, 100));\n";
                env.CompileDeploy(epl);

                try {
                    env.SendEventBean(new SupportSpatialPoint("E1", null, null));
                }
                catch (Exception ex) {
                    SupportMessageAssertUtil.AssertMessage(
                        ex,
                        "Unexpected exception in statement 'mywindow': Invalid value for index 'MyIndex' column 'x' received null and expected non-null");
                }

                try {
                    env.SendEventBean(new SupportSpatialPoint("E1", 200d, 200d));
                }
                catch (Exception ex) {
                    SupportMessageAssertUtil.AssertMessage(
                        ex,
                        "Unexpected exception in statement 'mywindow': Invalid value for index 'MyIndex' column '(x,y)' received (200.0,200.0) and expected a value within index bounding box (range-end-non-inclusive) {minX=0.0, minY=0.0, maxX=100.0, maxY=100.0}");
                }

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.INVALIDITY);
            }
        }

        private class EPLSpatialInvalidMethod : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.TryInvalidCompile(
                    "select * from SupportEventRectangleWithOffset(point('a', 0).inside(rectangle(0, 0, 0, 0)))",
                    "Failed to validate filter expression 'point(\"a\",0).inside(rectangle(0,0,0,0))': Failed to validate left-hand-side function 'point', expected a number-type result for expression parameter 0 but received String");
                env.TryInvalidCompile(
                    "select * from SupportEventRectangleWithOffset(point(0).inside(rectangle(0, 0, 0, 0)))",
                    "Failed to validate filter expression 'point(0).inside(rectangle(0,0,0,0))': Failed to validate left-hand-side method 'point', expected 2 parameters but received 1 parameters");
                env.TryInvalidCompile(
                    "select * from SupportEventRectangleWithOffset(point(0,0).inside(rectangle('a', 0, 0, 0)))",
                    "Failed to validate filter expression 'point(0,0).inside(rectangle(\"a\",0,0,0))': Failed to validate right-hand-side function 'rectangle', expected a number-type result for expression parameter 0 but received String");
                env.TryInvalidCompile(
                    "select * from SupportEventRectangleWithOffset(point(0,0).inside(rectangle(0)))",
                    "Failed to validate filter expression 'point(0,0).inside(rectangle(0))': Failed to validate right-hand-side function 'rectangle', expected 4 parameters but received 1 parameters");
                env.TryInvalidCompile(
                    "select * from SupportEventRectangleWithOffset(point(0,0).inside(0))",
                    "Failed to validate filter expression 'point(0,0).inside(0)': point.inside requires a single rectangle as parameter");
            }
        }

        private class EPLSpatialDocSample : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "create table PointTable(pointId string primary key, px double, py double);\n" +
                    "create index PointIndex on PointTable((px, py) pointregionquadtree(0, 0, 100, 100));\n" +
                    "create schema RectangleEvent(rx double, ry double, w double, h double);\n" +
                    "on RectangleEvent select pointId from PointTable where point(px, py).inside(rectangle(rx, ry, w, h));" +
                    "expression myQuadtreeSettings { pointregionquadtree(0, 0, 100, 100) } \n" +
                    "select * from SupportSpatialAABB(point(0, 0, filterindex:myQuadtreeSettings).inside(rectangle(x, y, width, height)));\n";
                env.CompileDeploy(epl).UndeployAll();
            }
        }

        private class EPLSpatialInvalidFilterIndex : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // unrecognized named parameter
                env.TryInvalidCompile(
                    "select * from SupportSpatialAABB#keepall where point(0, 0, a:1).inside(rectangle(x, y, width, height))",
                    "Failed to validate expression: Failed to validate filter expression 'point(0,0,a:1).inside(rectangle(x,y...(50 chars)': point does not accept 'a' as a named parameter");

                // not a filter
                env.TryInvalidCompile(
                    "expression myindex {pointregionquadtree(0, 0, 100, 100)} select * from SupportSpatialAABB#keepall where point(0, 0, filterindex:myindex).inside(rectangle(x, y, width, height))",
                    "Failed to validate expression: Failed to validate filter expression 'point(0,0,filterindex:myindex()).in...(68 chars)': The 'filterindex' named parameter can only be used in in filter expressions");

                // invalid index expression
                env.TryInvalidCompile(
                    "select * from SupportSpatialAABB(point(0, 0, filterindex:1).inside(rectangle(x, y, width, height)))",
                    "Failed to validate filter expression 'point(0,0,filterindex:1).inside(rec...(60 chars)': Named parameter 'filterindex' requires an expression name");
                env.TryInvalidCompile(
                    "select * from SupportSpatialAABB(point(0, 0, filterindex:dummy).inside(rectangle(x, y, width, height)))",
                    "Failed to validate filter expression 'point(0,0,filterindex:dummy).inside...(64 chars)': Named parameter 'filterindex' requires an expression name");
                env.TryInvalidCompile(
                    "expression myindex {0} select * from SupportSpatialAABB(point(0, 0, filterindex:myindex).inside(rectangle(x, y, width, height)))",
                    "Failed to validate filter expression 'point(0,0,filterindex:myindex()).in...(68 chars)': Named parameter 'filterindex' requires an index expression");
                env.TryInvalidCompile(
                    "expression myindex {dummy(0)} select * from SupportSpatialAABB(point(0, 0, filterindex:myindex).inside(rectangle(x, y, width, height)))",
                    "Failed to validate filter expression 'point(0,0,filterindex:myindex()).in...(68 chars)': Unrecognized advanced-type index 'dummy'");
                env.TryInvalidCompile(
                    "expression myindex {pointregionquadtree(0)} select * from SupportSpatialAABB(point(0, 0, filterindex:myindex).inside(rectangle(x, y, width, height)))",
                    "Failed to validate filter expression 'point(0,0,filterindex:myindex()).in...(68 chars)': Index of type 'pointregionquadtree' requires at least 4 parameters but received 1 [");
                env.TryInvalidCompile(
                    "expression myindex {pointregionquadtree(0,0,0,0)} select * from SupportSpatialAABB(point(0, 0, filterindex:myindex).inside(rectangle(x, y, width, height)))",
                    "Failed to validate filter expression 'point(0,0,filterindex:myindex()).in...(68 chars)': Invalid value for index 'myindex' parameter 'width' received 0.0 and expected value>0");
                env.TryInvalidCompile(
                    "expression myindex {pointregionquadtree(0,0,100,100).help()} select * from SupportSpatialAABB(point(0, 0, filterindex:myindex).inside(rectangle(x, y, width, height)))",
                    "Failed to validate filter expression 'point(0,0,filterindex:myindex()).in...(68 chars)': Named parameter 'filterindex' invalid chained index expression");

                // filter-not-optimizable
                env.TryInvalidCompile(
                    "expression myindex {pointregionquadtree(0, 0, 100, 100)} select * from SupportSpatialAABB(point(x, y, filterindex:myindex).inside(rectangle(x, y, width, height)))",
                    "Invalid filter-indexable expression 'x' in respect to index 'myindex': expected either a constant, context-builtin or property from a previous pattern match [expression myindex {pointregionquadtree(0, 0, 100, 100)} select * from SupportSpatialAABB(point(x, y, filterindex:myindex).inside(rectangle(x, y, width, height)))]");
                env.TryInvalidCompile(
                    "expression myindex {pointregionquadtree(0, 0, 100, 100)} select * from SupportSpatialAABB(point(0, 0, filterindex:myindex).inside(rectangle(0, y, width, height)))",
                    "Invalid filter-index lookup expression '0' in respect to index 'myindex': expected an event property");
            }
        }
    }
} // end of namespace