///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.compat.collections;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.common.@internal.util
{
    [TestFixture]
    public class TestGraphUtil : AbstractCommonTest
    {
        private void TryInvalid(
            IDictionary<string, ICollection<string>> graph,
            string msg)
        {
            try
            {
                GraphUtil.GetTopDownOrder(graph);
                Assert.Fail();
            }
            catch (GraphCircularDependencyException ex)
            {
                // expected
                ClassicAssert.AreEqual(msg, ex.Message);
            }
        }

        private void Add(
            IDictionary<string, ICollection<string>> graph,
            string child,
            string parent)
        {
            var parents = graph.Get(child);
            if (parents == null)
            {
                parents = new HashSet<string>();
                graph.Put(child, parents);
            }

            parents.Add(parent);
        }

        private IDictionary<string, object> MakeMap(object[][] entries)
        {
            var result = new Dictionary<string, object>();
            if (entries == null)
            {
                return result;
            }

            for (var i = 0; i < entries.Length; i++)
            {
                result.Put((string) entries[i][0], entries[i][1]);
            }

            return result;
        }

        [Test]
        public void TestAcyclicTopDownOrder()
        {
            IDictionary<string, ICollection<string>> graph = new LinkedHashMap<string, ICollection<string>>();

            Add(graph, "1_1", "R2");
            Add(graph, "A", "R1");
            Add(graph, "A", "R2");
            EPAssertionUtil.AssertEqualsExactOrder(new [] { "R1","R2","1_1","A" }, GraphUtil.GetTopDownOrder(graph).ToArray());

            Add(graph, "R1", "R2");
            EPAssertionUtil.AssertEqualsExactOrder(new [] { "R2","1_1","R1","A" }, GraphUtil.GetTopDownOrder(graph).ToArray());

            Add(graph, "1_1", "A");
            EPAssertionUtil.AssertEqualsExactOrder(new [] { "R2","R1","A","1_1" }, GraphUtil.GetTopDownOrder(graph).ToArray());

            Add(graph, "0", "1_1");
            EPAssertionUtil.AssertEqualsExactOrder(new [] { "R2","R1","A","1_1","0" }, GraphUtil.GetTopDownOrder(graph).ToArray());

            Add(graph, "R1", "0");
            TryInvalid(graph, "Circular dependency detected between [\"0\", \"R1\", \"A\", \"1_1\"]");
        }

        [Test]
        public void TestInvalidTopDownOder()
        {
            IDictionary<string, ICollection<string>> graph = new LinkedHashMap<string, ICollection<string>>();
            Add(graph, "1_1", "1");
            Add(graph, "1", "1_1");
            TryInvalid(graph, "Circular dependency detected between [\"1\", \"1_1\"]");

            graph = new LinkedHashMap<string, ICollection<string>>();
            Add(graph, "1", "2");
            Add(graph, "2", "3");
            Add(graph, "3", "1");
            TryInvalid(graph, "Circular dependency detected between [\"3\", \"2\", \"1\"]");
        }

        [Test]
        public void TestMerge()
        {
            var mapOne = MakeMap(
                new[] {
                    new object[] {"base1", 1},
                    new object[] {
                        "base3", MakeMap(
                            new[] {
                                new object[] {"n1", 9}
                            })
                    },
                    new object[] {"base4", null}
                });

            var mapTwo = MakeMap(
                new[] {
                    new object[] {"base1", null},
                    new object[] {"base2", 5},
                    new object[] {"base5", null},
                    new object[] {
                        "base3", MakeMap(
                            new[] {
                                new object[] {"n1", 7},
                                new object[] {"n2", 10}
                            })
                    }
                });

            var merged = GraphUtil.MergeNestableMap(mapOne, mapTwo);
            ClassicAssert.AreEqual(1, merged.Get("base1"));
            ClassicAssert.AreEqual(5, merged.Get("base2"));
            ClassicAssert.AreEqual(null, merged.Get("base4"));
            ClassicAssert.AreEqual(null, merged.Get("base5"));
            ClassicAssert.AreEqual(5, merged.Count);
            var nested = (IDictionary<string, object>) merged.Get("base3");
            ClassicAssert.AreEqual(2, nested.Count);
            ClassicAssert.AreEqual(9, nested.Get("n1"));
            ClassicAssert.AreEqual(10, nested.Get("n2"));
        }

        [Test]
        public void TestSimpleTopDownOrder()
        {
            IDictionary<string, ICollection<string>> graph = new LinkedHashMap<string, ICollection<string>>();
            ClassicAssert.AreEqual(0, GraphUtil.GetTopDownOrder(graph).Count);

            Add(graph, "1_1", "1");
            EPAssertionUtil.AssertEqualsExactOrder(GraphUtil.GetTopDownOrder(graph).ToArray(), new [] { "1","1_1" });

            Add(graph, "1_1_1", "1_1");
            EPAssertionUtil.AssertEqualsExactOrder(
                GraphUtil.GetTopDownOrder(graph).ToArray(),
                new [] { "1","1_1","1_1_1" });

            Add(graph, "0_1", "0");
            EPAssertionUtil.AssertEqualsExactOrder(
                GraphUtil.GetTopDownOrder(graph).ToArray(),
                new [] { "0","0_1","1","1_1","1_1_1" });

            Add(graph, "1_2", "1");
            EPAssertionUtil.AssertEqualsExactOrder(
                GraphUtil.GetTopDownOrder(graph).ToArray(),
                new [] { "0","0_1","1","1_1","1_1_1","1_2" });

            Add(graph, "1_1_2", "1_1");
            EPAssertionUtil.AssertEqualsExactOrder(
                GraphUtil.GetTopDownOrder(graph).ToArray(),
                new [] { "0","0_1","1","1_1","1_1_1","1_1_2","1_2" });

            Add(graph, "1_2_1", "1_2");
            EPAssertionUtil.AssertEqualsExactOrder(
                GraphUtil.GetTopDownOrder(graph).ToArray(),
                new [] { "0","0_1","1","1_1","1_1_1","1_1_2","1_2","1_2_1" });

            Add(graph, "0", "R");
            EPAssertionUtil.AssertEqualsExactOrder(
                GraphUtil.GetTopDownOrder(graph).ToArray(),
                new [] { "1","1_1","1_1_1","1_1_2","1_2","1_2_1","R","0","0_1" });

            Add(graph, "1", "R");
            EPAssertionUtil.AssertEqualsExactOrder(
                GraphUtil.GetTopDownOrder(graph).ToArray(),
                new [] { "R","0","0_1","1","1_1","1_1_1","1_1_2","1_2","1_2_1" });
        }
    }
} // end of namespace