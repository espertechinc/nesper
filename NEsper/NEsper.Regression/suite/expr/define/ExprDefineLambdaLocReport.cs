///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.lrreport;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.expr.define
{
    public class ExprDefineLambdaLocReport : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            // Regular algorithm to find separated luggage and new owner.
            var epl = "@name('s0') " +
                      "expression lostLuggage {" +
                      "  lr => lr.Items.where(l -> l.Type='L' and " +
                      "    lr.Items.anyof(p -> p.Type='P' and p.AssetId=l.AssetIdPassenger and LRUtil.Distance(l.Location.X, l.Location.Y, p.Location.X, p.Location.Y) > 20))" +
                      "}" +
                      "expression passengers {" +
                      "  lr => lr.Items.where(l -> l.Type='P')" +
                      "}" +
                      "" +
                      "expression nearestOwner {" +
                      "  lr => lostLuggage(lr).toMap(key -> key.AssetId, " +
                      "     value => passengers(lr).minBy(p -> LRUtil.Distance(value.Location.X, value.Location.Y, p.Location.X, p.Location.Y)))" +
                      "}" +
                      "" +
                      "select lostLuggage(lr) as val1, nearestOwner(lr) as val2 from LocationReport lr";
            env.CompileDeploy(epl).AddListener("s0");

            var bean = LocationReportFactory.MakeLarge();
            env.SendEventBean(bean);

            var val1 = env.Listener("s0").AssertOneGetNew().Get("val1").UnwrapIntoArray<Item>();
            Assert.AreEqual(3, val1.Length);
            Assert.AreEqual("L00000", val1[0].AssetId);
            Assert.AreEqual("L00007", val1[1].AssetId);
            Assert.AreEqual("L00008", val1[2].AssetId);

            var val2Event = env.Listener("s0").AssertOneGetNewAndReset();
            var val2Result = val2Event.Get("val2");
            var val2 = val2Result.AsObjectDictionary();
            Assert.AreEqual(3, val2.Count);
            Assert.AreEqual("P00008", ((Item) val2.Get("L00000")).AssetId);
            Assert.AreEqual("P00001", ((Item) val2.Get("L00007")).AssetId);
            Assert.AreEqual("P00001", ((Item) val2.Get("L00008")).AssetId);

            env.UndeployAll();
        }

        private Item[] ItemArray(ICollection<Item> it)
        {
            return it.ToArray();
        }
    }
} // end of namespace