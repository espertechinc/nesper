///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.collections;

using NUnit.Framework;

namespace com.espertech.esper.util
{
    [TestFixture]
    public class TestGraphUtil
    {
        [Test]
        public void TestMerge()
        {
            IDictionary<String, Object> mapOne = MakeMap(new Object[][]
            {
                new Object[]
                {
                    "base1", 1
                },
                new Object[]
                {
                    "base3", MakeMap(new Object[][]
                    {
                        new Object[]
                        {
                            "n1", 9
                        }
                    })
                },
                new Object[]
                {
                    "base4", null
                },
            });

            IDictionary<String, Object> mapTwo = MakeMap(new Object[][]
            {
                new Object[]
                {
                    "base1", null
                },
                new Object[]
                {
                    "base2", 5
                },
                new Object[]
                {
                    "base5", null
                },
                new Object[]
                {
                    "base3", MakeMap(new Object[][]
                    {
                        new Object[]
                        {
                            "n1", 7
                        },
                        new Object[]
                        {
                            "n2", 10
                        }
                    })
                }
            });

            IDictionary<String, Object> merged = GraphUtil.MergeNestableMap(mapOne, mapTwo);

            Assert.AreEqual(1, merged.Get("base1"));
            Assert.AreEqual(5, merged.Get("base2"));
            Assert.AreEqual(null, merged.Get("base4"));
            Assert.AreEqual(null, merged.Get("base5"));
            Assert.AreEqual(5, merged.Count);
            IDictionary<String, Object> nested = (IDictionary<String, Object>) merged.Get("base3");

            Assert.AreEqual(2, nested.Count);
            Assert.AreEqual(9, nested.Get("n1"));
            Assert.AreEqual(10, nested.Get("n2"));
        }

        [Test]
        public void TestSimpleTopDownOrder()
        {
            IDictionary<String, ICollection<String>> graph = new LinkedHashMap<String, ICollection<String>>();

            Assert.AreEqual(0, GraphUtil.GetTopDownOrder(graph).Count);

            Add(graph, "1_1", "1");
            EPAssertionUtil.AssertEqualsExactOrder(
                GraphUtil.GetTopDownOrder(graph).ToArray(), "1,1_1".Split(','));

            Add(graph, "1_1_1", "1_1");
            EPAssertionUtil.AssertEqualsExactOrder(
                GraphUtil.GetTopDownOrder(graph).ToArray(),
                "1,1_1,1_1_1".Split(','));

            Add(graph, "0_1", "0");
            EPAssertionUtil.AssertEqualsExactOrder(
                GraphUtil.GetTopDownOrder(graph).ToArray(),
                "0,0_1,1,1_1,1_1_1".Split(','));

            Add(graph, "1_2", "1");
            EPAssertionUtil.AssertEqualsExactOrder(
                GraphUtil.GetTopDownOrder(graph).ToArray(),
                "0,0_1,1,1_1,1_1_1,1_2".Split(','));

            Add(graph, "1_1_2", "1_1");
            EPAssertionUtil.AssertEqualsExactOrder(
                GraphUtil.GetTopDownOrder(graph).ToArray(),
                "0,0_1,1,1_1,1_1_1,1_1_2,1_2".Split(','));

            Add(graph, "1_2_1", "1_2");
            EPAssertionUtil.AssertEqualsExactOrder(
                GraphUtil.GetTopDownOrder(graph).ToArray(),
                "0,0_1,1,1_1,1_1_1,1_1_2,1_2,1_2_1".Split(','));

            Add(graph, "0", "R");
            EPAssertionUtil.AssertEqualsExactOrder(GraphUtil.GetTopDownOrder(graph).ToArray(), "1,1_1,1_1_1,1_1_2,1_2,1_2_1,R,0,0_1".Split(','));

            Add(graph, "1", "R");
            EPAssertionUtil.AssertEqualsExactOrder(
                GraphUtil.GetTopDownOrder(graph).ToArray(),
                "R,0,0_1,1,1_1,1_1_1,1_1_2,1_2,1_2_1".Split(','));
        }

        [Test]
        public void TestAcyclicTopDownOrder()
        {
            IDictionary<String, ICollection<String>> graph = new LinkedHashMap<String, ICollection<String>>();
    
            Add(graph, "1_1", "R2");
            Add(graph, "A", "R1");
            Add(graph, "A", "R2");
            EPAssertionUtil.AssertEqualsExactOrder("R1,R2,1_1,A".Split(','),
                    GraphUtil.GetTopDownOrder(graph).ToArray());
    
            Add(graph, "R1", "R2");
            EPAssertionUtil.AssertEqualsExactOrder("R2,1_1,R1,A".Split(','),
                    GraphUtil.GetTopDownOrder(graph).ToArray());
    
            Add(graph, "1_1", "A");
            EPAssertionUtil.AssertEqualsExactOrder("R2,R1,A,1_1".Split(','),
                    GraphUtil.GetTopDownOrder(graph).ToArray());
    
            Add(graph, "0", "1_1");
            EPAssertionUtil.AssertEqualsExactOrder("R2,R1,A,1_1,0".Split(','),
                    GraphUtil.GetTopDownOrder(graph).ToArray());
    
            Add(graph, "R1", "0");
            TryInvalid(graph, "Circular dependency detected between [0, R1, A, 1_1]");
        }

        [Test]
        public void TestInvalidTopDownOrder()
        {
            IDictionary<String, ICollection<String>> graph = new LinkedHashMap<String, ICollection<String>>();

            Add(graph, "1_1", "1");
            Add(graph, "1", "1_1");
            TryInvalid(graph, "Circular dependency detected between [1, 1_1]");

            graph = new LinkedHashMap<String, ICollection<String>>();
            Add(graph, "1", "2");
            Add(graph, "2", "3");
            Add(graph, "3", "1");
            TryInvalid(graph, "Circular dependency detected between [3, 2, 1]");
        }

        private void TryInvalid(IDictionary<String, ICollection<String>> graph, String msg)
        {
            try
            {
                GraphUtil.GetTopDownOrder(graph);
                Assert.Fail();
            }
            catch (GraphCircularDependencyException ex)
            {
                // expected
                Assert.AreEqual(msg, ex.Message);
            }
        }

        private void Add(IDictionary<String, ICollection<String>> graph, String child, String parent)
        {
            ICollection<String> parents = graph.Get(child);

            if (parents == null)
            {
                parents = new HashSet<String>();
                graph.Put(child, parents);
            }
            parents.Add(parent);
        }

        private IDictionary<String, Object> MakeMap(Object[][] entries)
        {
            var result = new Dictionary<String, Object>();

            if (entries == null)
            {
                return result;
            }
            for (int i = 0; i < entries.Length; i++)
            {
                result.Put((string) entries[i][0], entries[i][1]);
            }
            return result;
        }
    }
}
