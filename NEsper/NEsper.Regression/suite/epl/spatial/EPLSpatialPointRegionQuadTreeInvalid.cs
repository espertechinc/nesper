///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

namespace com.espertech.esper.regressionlib.suite.epl.spatial
{
    public class EPLSpatialPointRegionQuadTreeInvalid
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            execs.Add(new EPLSpatialInvalidEventIndexCreate());
            execs.Add(new EPLSpatialInvalidEventIndexRuntime());
            execs.Add(new EPLSpatialInvalidMethod());
            execs.Add(new EPLSpatialInvalidFilterIndex());
            execs.Add(new EPLSpatialDocSample());
            return execs;
        }

        internal class EPLSpatialInvalidEventIndexCreate : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("create window MyWindow#keepall as SupportSpatialPoint", path);

                // invalid number of columns
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    path,
                    "create index MyIndex on MyWindow(Px pointregionquadtree(0, 0, 100, 100))",
                    "Index of type 'pointregionquadtree' requires 2 expressions as index columns but received 1");

                // invalid column type
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    path,
                    "create index MyIndex on MyWindow((Id, Py) pointregionquadtree(0, 0, 100, 100))",
                    "Index of type 'pointregionquadtree' for column 0 that is providing X-values expecting type System.Object but received type System.String");
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    path,
                    "create index MyIndex on MyWindow((Px, Id) pointregionquadtree(0, 0, 100, 100))",
                    "Index of type 'pointregionquadtree' for column 1 that is providing Y-values expecting type System.Object but received type System.String");

                // invalid expressions for column or parameter
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    path,
                    "create index MyIndex on MyWindow((dummy, dummy2) pointregionquadtree(0, 0, 100, 100))",
                    "Failed to validate create-index index-column expression 'dummy': Property named 'dummy' is not valid in any stream");
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    path,
                    "create index MyIndex on MyWindow((Px, Py) pointregionquadtree(dummy, 0, 100, 100))",
                    "Failed to validate create-index index-parameter expression 'dummy': Property named 'dummy' is not valid in any stream");

                // invalid property use in parameter
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    path,
                    "create index MyIndex on MyWindow((Px, Py) pointregionquadtree(Px, 0, 100, 100))",
                    "Index parameters may not refer to event properties");

                // invalid number of parameters
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    path,
                    "create index MyIndex on MyWindow((Px, Py) pointregionquadtree)",
                    "Index of type 'pointregionquadtree' requires at least 4 parameters but received 0");
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    path,
                    "create index MyIndex on MyWindow((Px, Py) pointregionquadtree('a'))",
                    "Index of type 'pointregionquadtree' requires at least 4 parameters but received 1");
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    path,
                    "create index MyIndex on MyWindow((Px, Py) pointregionquadtree(0, 0, 0, 0, 0, 0, 0))",
                    "Index of type 'pointregionquadtree' requires at least 4 parameters but received 7");

                // invalid parameter type
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    path,
                    "create index MyIndex on MyWindow((Px, Py) pointregionquadtree('a', 0, 100, 100))",
                    "Index of type 'pointregionquadtree' for parameter 0 that is providing XMin-values expecting type System.Object but received type System.String");
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    path,
                    "create index MyIndex on MyWindow((Px, Py) pointregionquadtree(0, 'a', 100, 100))",
                    "Index of type 'pointregionquadtree' for parameter 1 that is providing YMin-values expecting type System.Object but received type System.String");
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    path,
                    "create index MyIndex on MyWindow((Px, Py) pointregionquadtree(0, 0, 'a', 100))",
                    "Index of type 'pointregionquadtree' for parameter 2 that is providing Width-values expecting type System.Object but received type System.String");
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    path,
                    "create index MyIndex on MyWindow((Px, Py) pointregionquadtree(0, 0, 100, 'a'))",
                    "Index of type 'pointregionquadtree' for parameter 3 that is providing Height-values expecting type System.Object but received type System.String");
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    path,
                    "create index MyIndex on MyWindow((Px, Py) pointregionquadtree(0, 0, 100, 100, 'a'))",
                    "Index of type 'pointregionquadtree' for parameter 4 that is providing LeafCapacity-values expecting type System.Nullable<System.Int32> but received type System.String");
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    path,
                    "create index MyIndex on MyWindow((Px, Py) pointregionquadtree(0, 0, 100, 100, 1, 'a'))",
                    "Index of type 'pointregionquadtree' for parameter 5 that is providing MaxTreeHeight-values expecting type System.Nullable<System.Int32> but received type System.String");

                // invalid parameter value
                SupportMessageAssertUtil.TryInvalidDeploy(
                    env,
                    path,
                    "create index MyIndex on MyWindow((Px, Py) pointregionquadtree(cast(null, double), 0, 0, 0))",
                    "Failed to deploy: Invalid value for index 'MyIndex' parameter 'XMin' received null and expected non-null");
                SupportMessageAssertUtil.TryInvalidDeploy(
                    env,
                    path,
                    "create index MyIndex on MyWindow((Py, Px) pointregionquadtree(0, 0, -100, 0))",
                    "Failed to deploy: Invalid value for index 'MyIndex' parameter 'Width' received -100.0d and expected value>0");
                SupportMessageAssertUtil.TryInvalidDeploy(
                    env,
                    path,
                    "create index MyIndex on MyWindow((Py, Px) pointregionquadtree(0, 0, 1, -200))",
                    "Failed to deploy: Invalid value for index 'MyIndex' parameter 'Height' received -200.0d and expected value>0");
                SupportMessageAssertUtil.TryInvalidDeploy(
                    env,
                    path,
                    "create index MyIndex on MyWindow((Py, Px) pointregionquadtree(0, 0, 1, 1, -1))",
                    "Failed to deploy: Invalid value for index 'MyIndex' parameter 'LeafCapacity' received -1 and expected value>=1");
                SupportMessageAssertUtil.TryInvalidDeploy(
                    env,
                    path,
                    "create index MyIndex on MyWindow((Py, Px) pointregionquadtree(0, 0, 1, 1, 10, -1))",
                    "Failed to deploy: Invalid value for index 'MyIndex' parameter 'MaxTreeHeight' received -1 and expected value>=2");

                // same index twice, by-name and by-columns
                env.CompileDeploy("create window SomeWindow#keepall as SupportSpatialPoint", path);
                env.CompileDeploy(
                    "create index SomeWindowIdx1 on SomeWindow((Px, Py) pointregionquadtree(0, 0, 1, 1))",
                    path);
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    path,
                    "create index SomeWindowIdx2 on SomeWindow((Px, Py) pointregionquadtree(0, 0, 1, 1))",
                    "An index for the same columns already exists");
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    path,
                    "create index SomeWindowIdx1 on SomeWindow((Py, Px) pointregionquadtree(0, 0, 1, 1))",
                    "An index by name 'SomeWindowIdx1' already exists");

                // non-plain column or parameter expressions
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    path,
                    "create index MyIndexInv on MyWindow((sum(Px), Py) pointregionquadtree(0, 0, 1, 1))",
                    "Invalid create-index index-column expression 'sum(Px)': Aggregation, sub-select, previous or prior functions are not supported in this context");
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    path,
                    "create index MyIndexInv on MyWindow((Px, Py) pointregionquadtree(count(*), 0, 1, 1))",
                    "Invalid create-index index-parameter expression 'count(*)': Aggregation, sub-select, previous or prior functions are not supported in this context");

                env.UndeployAll();
            }
        }

        internal class EPLSpatialInvalidEventIndexRuntime : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('mywindow') create window PointWindow#keepall as SupportSpatialPoint;\n" +
                          "insert into PointWindow select * from SupportSpatialPoint;\n" +
                          "create index MyIndex on PointWindow((Px, Py) pointregionquadtree(0, 0, 100, 100));\n";
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
                        "Unexpected exception in statement 'mywindow': Invalid value for index 'MyIndex' column '(X,Y)' received (200.0d,200.0d) and expected a value within index bounding box (range-end-non-inclusive) {MinX=0.0d, MinY=0.0d, MaxX=100.0d, MaxY=100.0d}");
                }

                env.UndeployAll();
            }
        }

        internal class EPLSpatialInvalidMethod : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "select * from SupportEventRectangleWithOffset(point('a', 0).inside(rectangle(0, 0, 0, 0)))",
                    "Failed to validate filter expression 'point(\"a\",0).inside(rectangle(0,0,0,0))': Failed to validate left-hand-side function 'point', expected a number-type result for expression parameter 0 but received System.String");
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "select * from SupportEventRectangleWithOffset(point(0).inside(rectangle(0, 0, 0, 0)))",
                    "Failed to validate filter expression 'point(0).inside(rectangle(0,0,0,0))': Failed to validate left-hand-side method 'point', expected 2 parameters but received 1 parameters");
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "select * from SupportEventRectangleWithOffset(point(0,0).inside(rectangle('a', 0, 0, 0)))",
                    "Failed to validate filter expression 'point(0,0).inside(rectangle(\"a\",0,0,0))': Failed to validate right-hand-side function 'rectangle', expected a number-type result for expression parameter 0 but received System.String");
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "select * from SupportEventRectangleWithOffset(point(0,0).inside(rectangle(0)))",
                    "Failed to validate filter expression 'point(0,0).inside(rectangle(0))': Failed to validate right-hand-side function 'rectangle', expected 4 parameters but received 1 parameters");
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "select * from SupportEventRectangleWithOffset(point(0,0).inside(0))",
                    "Failed to validate filter expression 'point(0,0).inside(0)': point.inside requires a single rectangle as parameter");
            }
        }

        internal class EPLSpatialDocSample : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "create table PointTable(pointId string primary key, Px double, Py double);\n" +
                    "create index PointIndex on PointTable((Px, Py) pointregionquadtree(0, 0, 100, 100));\n" +
                    "create schema RectangleEvent(rx double, ry double, w double, h double);\n" +
                    "on RectangleEvent select pointId from PointTable where point(Px, Py).inside(rectangle(rx, ry, w, h));" +
                    "expression myQuadtreeSettings { pointregionquadtree(0, 0, 100, 100) } \n" +
                    "select * from SupportSpatialAABB(point(0, 0, filterindex:myQuadtreeSettings).inside(rectangle(X, Y, Width, Height)));\n";
                env.CompileDeploy(epl).UndeployAll();
            }
        }

        internal class EPLSpatialInvalidFilterIndex : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // unrecognized named parameter
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "select * from SupportSpatialAABB#keepall where point(0, 0, a:1).inside(rectangle(X, Y, Width, Height))",
                    "Failed to validate expression: Failed to validate filter expression 'point(0,0,a:1).inside(rectangle(X,Y...(50 chars)': point does not accept 'a' as a named parameter");

                // not a filter
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "expression myindex {pointregionquadtree(0, 0, 100, 100)} select * from SupportSpatialAABB#keepall where point(0, 0, filterindex:myindex).inside(rectangle(X, Y, Width, Height))",
                    "Failed to validate expression: Failed to validate filter expression 'point(0,0,filterindex:myindex()).in...(68 chars)': The 'filterindex' named parameter can only be used in in filter expressions");

                // invalid index expression
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "select * from SupportSpatialAABB(point(0, 0, filterindex:1).inside(rectangle(X, Y, Width, Height)))",
                    "Failed to validate filter expression 'point(0,0,filterindex:1).inside(rec...(60 chars)': Named parameter 'filterindex' requires an expression name");
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "select * from SupportSpatialAABB(point(0, 0, filterindex:dummy).inside(rectangle(X, Y, Width, Height)))",
                    "Failed to validate filter expression 'point(0,0,filterindex:dummy).inside...(64 chars)': Named parameter 'filterindex' requires an expression name");
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "expression myindex {0} select * from SupportSpatialAABB(point(0, 0, filterindex:myindex).inside(rectangle(X, Y, Width, Height)))",
                    "Failed to validate filter expression 'point(0,0,filterindex:myindex()).in...(68 chars)': Named parameter 'filterindex' requires an index expression");
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "expression myindex {dummy(0)} select * from SupportSpatialAABB(point(0, 0, filterindex:myindex).inside(rectangle(X, Y, Width, Height)))",
                    "Failed to validate filter expression 'point(0,0,filterindex:myindex()).in...(68 chars)': Unrecognized advanced-type index 'dummy'");
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "expression myindex {pointregionquadtree(0)} select * from SupportSpatialAABB(point(0, 0, filterindex:myindex).inside(rectangle(X, Y, Width, Height)))",
                    "Failed to validate filter expression 'point(0,0,filterindex:myindex()).in...(68 chars)': Index of type 'pointregionquadtree' requires at least 4 parameters but received 1 [");
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "expression myindex {pointregionquadtree(0,0,0,0)} select * from SupportSpatialAABB(point(0, 0, filterindex:myindex).inside(rectangle(X, Y, Width, Height)))",
                    "Failed to validate filter expression 'point(0,0,filterindex:myindex()).in...(68 chars)': Invalid value for index 'myindex' parameter 'Width' received 0.0d and expected value>0");
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "expression myindex {pointregionquadtree(0,0,100,100).help()} select * from SupportSpatialAABB(point(0, 0, filterindex:myindex).inside(rectangle(X, Y, Width, Height)))",
                    "Failed to validate filter expression 'point(0,0,filterindex:myindex()).in...(68 chars)': Named parameter 'filterindex' invalid chained index expression");

                // filter-not-optimizable
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "expression myindex {pointregionquadtree(0, 0, 100, 100)} select * from SupportSpatialAABB(point(X, Y, filterindex:myindex).inside(rectangle(X, Y, Width, Height)))",
                    "Invalid filter-indexable expression 'X' in respect to index 'myindex': expected either a constant, context-builtin or property from a previous pattern match [expression myindex {pointregionquadtree(0, 0, 100, 100)} select * from SupportSpatialAABB(point(X, Y, filterindex:myindex).inside(rectangle(X, Y, Width, Height)))]");
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "expression myindex {pointregionquadtree(0, 0, 100, 100)} select * from SupportSpatialAABB(point(0, 0, filterindex:myindex).inside(rectangle(0, Y, Width, Height)))",
                    "Invalid filter-index lookup expression '0' in respect to index 'myindex': expected an event property");
            }
        }
    }
} // end of namespace