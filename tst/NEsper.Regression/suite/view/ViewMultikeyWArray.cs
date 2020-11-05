///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.dataflow.core;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.epl.dataflow.util;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.container;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.view
{
    public partial class ViewMultikeyWArray
    {
        public static ICollection<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithLastUniqueTwoKey(execs);
            WithFirstUnique(execs);
            WithGroupWin(execs);
            WithRank(execs);
            WithLastUniqueThreeKey(execs);
            WithLastUniqueOneKeyArrayOfLongPrimitive(execs);
            WithLastUniqueOneKeyArrayOfObjectArray(execs);
            WithLastUniqueOneKey2DimArray(execs);
            WithLastUniqueTwoKeyAllArrayOfPrimitive(execs);
            WithLastUniqueTwoKeyAllArrayOfObject(execs);
            WithLastUniqueArrayKeyIntersection(execs);
            WithLastUniqueArrayKeyUnion(execs);
            WithLastUniqueArrayKeySubquery(execs);
            WithLastUniqueArrayKeyNamedWindow(execs);
            WithLastUniqueArrayKeySubqueryInFilter(execs);
            WithLastUniqueArrayKeyDataflow(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithLastUniqueArrayKeyDataflow(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewMultiKeyLastUniqueArrayKeyDataflow());
            return execs;
        }

        public static IList<RegressionExecution> WithLastUniqueArrayKeySubqueryInFilter(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewMultiKeyLastUniqueArrayKeySubqueryInFilter());
            return execs;
        }

        public static IList<RegressionExecution> WithLastUniqueArrayKeyNamedWindow(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewMultiKeyLastUniqueArrayKeyNamedWindow());
            return execs;
        }

        public static IList<RegressionExecution> WithLastUniqueArrayKeySubquery(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewMultiKeyLastUniqueArrayKeySubquery());
            return execs;
        }

        public static IList<RegressionExecution> WithLastUniqueArrayKeyUnion(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewMultiKeyLastUniqueArrayKeyUnion());
            return execs;
        }

        public static IList<RegressionExecution> WithLastUniqueArrayKeyIntersection(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewMultiKeyLastUniqueArrayKeyIntersection());
            return execs;
        }

        public static IList<RegressionExecution> WithLastUniqueTwoKeyAllArrayOfObject(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewMultiKeyLastUniqueTwoKeyAllArrayOfObject());
            return execs;
        }

        public static IList<RegressionExecution> WithLastUniqueTwoKeyAllArrayOfPrimitive(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewMultiKeyLastUniqueTwoKeyAllArrayOfPrimitive());
            return execs;
        }

        public static IList<RegressionExecution> WithLastUniqueOneKey2DimArray(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewMultiKeyLastUniqueOneKey2DimArray());
            return execs;
        }

        public static IList<RegressionExecution> WithLastUniqueOneKeyArrayOfObjectArray(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewMultiKeyLastUniqueOneKeyArrayOfObjectArray());
            return execs;
        }

        public static IList<RegressionExecution> WithLastUniqueOneKeyArrayOfLongPrimitive(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewMultiKeyLastUniqueOneKeyArrayOfLongPrimitive());
            return execs;
        }

        public static IList<RegressionExecution> WithLastUniqueThreeKey(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewMultiKeyLastUniqueThreeKey());
            return execs;
        }

        public static IList<RegressionExecution> WithRank(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewMultiKeyRank());
            return execs;
        }

        public static IList<RegressionExecution> WithGroupWin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewMultiKeyGroupWin());
            return execs;
        }

        public static IList<RegressionExecution> WithFirstUnique(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewMultiKeyFirstUnique());
            return execs;
        }

        public static IList<RegressionExecution> WithLastUniqueTwoKey(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewMultiKeyLastUniqueTwoKey());
            return execs;
        }

        private class ViewMultiKeyRank : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select window(Id) as ids from SupportEventWithLongArray#rank(Coll, 10, Id)";
                env.CompileDeploy(epl).AddListener("s0");

                SendAssertLongArrayIdWindow(env, "E1", new long[] {1, 2}, "E1");
                SendAssertLongArrayIdWindow(env, "E2", new long[] {1}, "E1,E2");
                SendAssertLongArrayIdWindow(env, "E3", new long[] { }, "E1,E2,E3");
                SendAssertLongArrayIdWindow(env, "E4", null, "E1,E2,E3,E4");

                env.Milestone(0);

                SendAssertLongArrayIdWindow(env, "E10", new long[] {1}, "E1,E3,E4,E10");
                SendAssertLongArrayIdWindow(env, "E11", new long[] { }, "E1,E4,E10,E11");
                SendAssertLongArrayIdWindow(env, "E12", new long[] {1, 2}, "E4,E10,E11,E12");
                SendAssertLongArrayIdWindow(env, "E13", null, "E10,E11,E12,E13");

                env.UndeployAll();
            }
        }

        private class ViewMultiKeyGroupWin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select window(Id) as ids from SupportEventWithLongArray#groupwin(Coll)#lastevent";
                env.CompileDeploy(epl).AddListener("s0");

                SendAssertLongArrayIdWindow(env, "E1", new long[] {1, 2}, "E1");
                SendAssertLongArrayIdWindow(env, "E2", new long[] {1}, "E1,E2");
                SendAssertLongArrayIdWindow(env, "E3", new long[] { }, "E1,E2,E3");
                SendAssertLongArrayIdWindow(env, "E4", null, "E1,E2,E3,E4");

                env.Milestone(0);

                SendAssertLongArrayIdWindow(env, "E10", new long[] {1}, "E1,E3,E4,E10");
                SendAssertLongArrayIdWindow(env, "E11", new long[] { }, "E1,E4,E10,E11");
                SendAssertLongArrayIdWindow(env, "E12", new long[] {1, 2}, "E4,E10,E11,E12");
                SendAssertLongArrayIdWindow(env, "E13", null, "E10,E11,E12,E13");

                env.UndeployAll();
            }
        }

        private class ViewMultiKeyFirstUnique : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select irstream * from SupportEventWithLongArray#firstunique(Coll)";
                env.CompileDeploy(epl).AddListener("s0");

                SendAssertLongArrayIStream(env, true, "E1", false, 1, 2);
                SendAssertLongArrayIStream(env, true, "E2", false, 1);

                env.Milestone(0);

                SendAssertLongArrayIStream(env, false, "E10", false, 1, 2);
                SendAssertLongArrayIStream(env, false, "E11", false, 1);
                SendAssertLongArrayIStream(env, true, "E12", false, 2, 2);
                SendAssertLongArrayIStream(env, true, "E13", true);

                env.Milestone(1);

                SendAssertLongArrayIStream(env, false, "E20", true);

                env.UndeployAll();
            }
        }

        private class ViewMultiKeyLastUniqueArrayKeyDataflow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                if (env.IsHA) {
                    return;
                }

                var graph = "@Name('flow') create dataflow MySelect\n" +
                            "Emitter -> instream_s0<SupportEventWithLongArray>{name: 'emitterS0'}\n" +
                            "Select(instream_s0) -> outstream {\n" +
                            "  select: (select window(Id) as ids from instream_s0#unique(Coll))\n" +
                            "}\n" +
                            "DefaultSupportCaptureOp(outstream) {}\n";
                env.CompileDeploy(graph);

                var capture = new DefaultSupportCaptureOp(env.Container.LockManager());
                var operators = CollectionUtil.PopulateNameValueMap("DefaultSupportCaptureOp", capture);

                var options = new EPDataFlowInstantiationOptions()
                    .WithOperatorProvider(new DefaultSupportGraphOpProviderByOpName(operators));
                var instance = env.Runtime.DataFlowService.Instantiate(env.DeploymentId("flow"), "MySelect", options);
                var captive = instance.StartCaptive();

                AssertDataflowIds(captive, "E1", new long[] {1, 2}, capture, "E1");
                AssertDataflowIds(captive, "E2", new long[] {1, 2}, capture, "E2");
                AssertDataflowIds(captive, "E3", new long[] {1}, capture, "E2,E3");
                AssertDataflowIds(captive, "E4", new long[] {1}, capture, "E2,E4");
                AssertDataflowIds(captive, "E5", new long[] {1, 2}, capture, "E4,E5");

                instance.Cancel();
                env.UndeployAll();
            }
        }

        public class ViewMultiKeyLastUniqueArrayKeySubqueryInFilter : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select * from SupportBean(2 = (select count(*) from SupportEventWithLongArray#unique(Coll)))";
                env.CompileDeploy(epl).AddListener("s0");

                SendSBAssertFilter(env, false);

                env.SendEventBean(new SupportEventWithLongArray("E0", new long[] {1, 2}));
                env.SendEventBean(new SupportEventWithLongArray("E1", new long[] {1}));

                env.Milestone(0);

                SendSBAssertFilter(env, true);

                env.SendEventBean(new SupportEventWithLongArray("E2", new long[] {1, 2}));
                env.SendEventBean(new SupportEventWithLongArray("E3", new long[] {1}));

                SendSBAssertFilter(env, true);

                env.SendEventBean(new SupportEventWithLongArray("E4", new long[] {3}));

                SendSBAssertFilter(env, false);

                env.UndeployAll();
            }
        }

        public class ViewMultiKeyLastUniqueArrayKeyNamedWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create window MyWindow#unique(Coll) as SupportEventWithLongArray;\n" +
                          "insert into MyWindow select * from SupportEventWithLongArray;\n" +
                          "@Name('s0') select irstream * from MyWindow;\n";
                env.CompileDeploy(epl).AddListener("s0");

                RunAssertionLongArray(env);

                env.UndeployAll();
            }
        }

        public class ViewMultiKeyLastUniqueArrayKeySubquery : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select irstream (select window(Id) from SupportEventWithLongArray#unique(Coll)) as c0 from SupportBean";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportEventWithLongArray("E0", new long[] {1, 2}));
                env.SendEventBean(new SupportEventWithLongArray("E1", null));
                env.SendEventBean(new SupportEventWithLongArray("E2", new long[] {1}));
                env.SendEventBean(new SupportEventWithLongArray("E3", new long[] { }));
                env.SendEventBean(new SupportEventWithLongArray("E4", new long[] {1}));

                env.Milestone(0);

                env.SendEventBean(new SupportBean());
                EPAssertionUtil.AssertEqualsAnyOrder((string[]) env.Listener("s0").AssertOneGetNewAndReset().Get("c0"), "E0,E1,E3,E4".SplitCsv());

                env.SendEventBean(new SupportEventWithLongArray("E10", new long[] {1, 2}));
                env.SendEventBean(new SupportEventWithLongArray("E13", new long[] { }));
                env.SendEventBean(new SupportEventWithLongArray("E14", new long[] {1}));

                env.SendEventBean(new SupportBean());
                EPAssertionUtil.AssertEqualsAnyOrder((string[]) env.Listener("s0").AssertOneGetNewAndReset().Get("c0"), "E10,E1,E13,E14".SplitCsv());

                env.UndeployAll();
            }
        }

        public class ViewMultiKeyLastUniqueArrayKeyUnion : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@public @buseventtype create schema EventTwoArrayOfPrimitive as " +
                    typeof(EventTwoArrayOfPrimitive).MaskTypeName() +
                    ";\n" +
                    "@Name('s0') select irstream * from EventTwoArrayOfPrimitive#unique(One)#unique(Two) retain-union";
                env.CompileDeploy(epl).AddListener("s0");

                SendAssertTwoArrayIterate(env, "E0", new[] {1, 2}, new[] {3, 4}, "E0");

                env.Milestone(0);

                SendAssertTwoArrayIterate(env, "E1", new[] {1, 2}, new[] {3, 4}, "E1");
                SendAssertTwoArrayIterate(env, "E2", new[] {10, 20}, new[] {30}, "E1,E2");

                env.Milestone(1);

                SendAssertTwoArrayIterate(env, "E3", new[] {1, 2}, new[] {40}, "E1,E2,E3");
                SendAssertTwoArrayIterate(env, "E4", new[] {30}, new[] {30}, "E1,E2,E3,E4");
                SendAssertTwoArrayIterate(env, "E5", new[] {1, 2}, new[] {3, 4}, "E2,E3,E4,E5");

                env.UndeployAll();
            }
        }

        public class ViewMultiKeyLastUniqueArrayKeyIntersection : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@public @buseventtype create schema EventTwoArrayOfPrimitive as " +
                          typeof(EventTwoArrayOfPrimitive).MaskTypeName() +
                          ";\n" +
                          "@Name('s0') select irstream * from EventTwoArrayOfPrimitive#unique(One)#unique(Two)";
                env.CompileDeploy(epl).AddListener("s0");

                SendAssertTwoArrayIterate(env, "E0", new[] {1, 2}, new[] {3, 4}, "E0");

                env.Milestone(0);

                SendAssertTwoArrayIterate(env, "E1", new[] {1, 2}, new[] {3, 4}, "E1");
                SendAssertTwoArrayIterate(env, "E2", new[] {10, 20}, new[] {30}, "E1,E2");

                env.Milestone(1);

                SendAssertTwoArrayIterate(env, "E3", new[] {1, 2}, new[] {40}, "E3,E2");
                SendAssertTwoArrayIterate(env, "E4", new[] {30}, new[] {30}, "E3,E4");
                SendAssertTwoArrayIterate(env, "E5", new[] {1, 3}, new[] {50}, "E3,E4,E5");

                env.UndeployAll();
            }
        }

        public class ViewMultiKeyLastUniqueTwoKeyAllArrayOfPrimitive : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@public @buseventtype create schema EventTwoArrayOfPrimitive as " +
                          typeof(EventTwoArrayOfPrimitive).MaskTypeName() +
                          ";\n" +
                          "@Name('s0') select irstream * from EventTwoArrayOfPrimitive#unique(One, Two)";
                env.CompileDeploy(epl).AddListener("s0");

                var b0 = SendAssertTwoArray(env, null, "E0", new[] {1, 2}, new[] {3, 4});

                env.Milestone(0);

                var b1 = SendAssertTwoArray(env, b0, "E1", new[] {1, 2}, new[] {3, 4});
                var b2 = SendAssertTwoArray(env, null, "E2", new int[] { }, new[] {3, 4});
                var b3 = SendAssertTwoArray(env, null, "E3", new[] {1, 2}, new int[] { });
                var b4 = SendAssertTwoArray(env, null, "E4", new[] {1}, new[] {3, 4});

                env.Milestone(1);

                SendAssertTwoArray(env, b1, "E20", new[] {1, 2}, new[] {3, 4});
                SendAssertTwoArray(env, b3, "E21", new[] {1, 2}, new int[] { });
                SendAssertTwoArray(env, b2, "E22", new int[] { }, new[] {3, 4});
                SendAssertTwoArray(env, b4, "E23", new[] {1}, new[] {3, 4});

                env.UndeployAll();
            }
        }

        public class ViewMultiKeyLastUniqueTwoKeyAllArrayOfObject : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var eventTwoArrayOfObject = typeof(EventTwoArrayOfObject).MaskTypeName();
                var epl =
                    $"@public @buseventtype create schema EventTwoArrayOfObject as {eventTwoArrayOfObject};\n" +
                    $"@Name('s0') select irstream * from EventTwoArrayOfObject#unique(One, Two)";
                env.CompileDeploy(epl).AddListener("s0");

                var b0 = SendAssertTwoArray(env, null, "E0", new object[] {1, 2}, new object[] {new[] {"a", "b"}});

                env.Milestone(0);

                var b1 = SendAssertTwoArray(env, b0, "E1", new object[] {1, 2}, new object[] {new[] {"a", "b"}});
                var b2 = SendAssertTwoArray(env, null, "E2", new object[] {0}, new object[] {new[] {"a", "b"}});
                var b3 = SendAssertTwoArray(env, null, "E3", new object[] {1, 2}, new object[] {new[] {"a"}});
                var b4 = SendAssertTwoArray(env, null, "E4", new object[] { }, new object[] {new string[] { }});

                env.Milestone(1);

                SendAssertTwoArray(env, b1, "E20", new object[] {1, 2}, new object[] {new[] {"a", "b"}});
                SendAssertTwoArray(env, b3, "E21", new object[] {1, 2}, new object[] {new[] {"a"}});
                SendAssertTwoArray(env, b2, "E22", new object[] {0}, new object[] {new[] {"a", "b"}});
                SendAssertTwoArray(env, b4, "E23", new object[] { }, new object[] {new string[] { }});

                env.UndeployAll();
            }
        }

        public class ViewMultiKeyLastUniqueOneKey2DimArray : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@public @buseventtype create schema EventTwoDimArray as " +
                    typeof(EventTwoDimArray).MaskTypeName() +
                    ";\n" +
                    "@Name('s0') select irstream * from EventTwoDimArray#unique(Array)";
                env.CompileDeploy(epl).AddListener("s0");

                var b0 = SendAssertInt2DimArray(
                    env,
                    null,
                    "E0",
                    new[] {
                        new[] {1},
                        new[] {2}
                    });

                env.Milestone(0);

                var b1 = SendAssertInt2DimArray(
                    env,
                    b0,
                    "E1",
                    new[] {
                        new[] {1},
                        new[] {2}
                    });
                var b2 = SendAssertInt2DimArray(
                    env,
                    null,
                    "E2",
                    new[] {
                        new[] {2},
                        new[] {1}
                    });
                var b3 = SendAssertInt2DimArray(
                    env,
                    null,
                    "E3",
                    new[] {
                        new[] {1, 2}
                    });
                var b4 = SendAssertInt2DimArray(
                    env,
                    null,
                    "E4",
                    new[] {
                        new int[] { },
                        new[] {1, 2}
                    });

                env.Milestone(1);

                SendAssertInt2DimArray(
                    env,
                    b1,
                    "E20",
                    new[] {
                        new[] {1},
                        new[] {2}
                    });
                SendAssertInt2DimArray(
                    env,
                    b3,
                    "E21",
                    new[] {
                        new[] {1, 2}
                    });
                SendAssertInt2DimArray(
                    env,
                    b2,
                    "E22",
                    new[] {
                        new[] {2},
                        new[] {1}
                    });
                SendAssertInt2DimArray(
                    env,
                    b4,
                    "E23",
                    new[] {
                        new int[] { },
                        new[] {1, 2}
                    });

                env.UndeployAll();
            }
        }

        public class ViewMultiKeyLastUniqueOneKeyArrayOfObjectArray : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select irstream * from SupportObjectArrayOneDim#unique(Arr)";
                env.CompileDeploy(epl).AddListener("s0");

                var b0 = SendAssertObjectArray(env, null, "E0", false, 1, ArrayOf("A", "B"));

                env.Milestone(0);

                var b1 = SendAssertObjectArray(env, b0, "E1", false, 1, ArrayOf("A", "B"));
                var b2 = SendAssertObjectArray(env, null, "E2", false, 2, ArrayOf("A", "B"));
                var b3 = SendAssertObjectArray(env, null, "E3", false, 1, ArrayOf("A", "A"));
                var b4 = SendAssertObjectArray(env, null, "E4", false, 1, ArrayOf("B", "B"));
                var b5 = SendAssertObjectArray(env, null, "E5", false);
                var b6 = SendAssertObjectArray(env, null, "E6", false, 1);
                var b7 = SendAssertObjectArray(env, null, "E7", false, 1, 2);
                //var b8 = SendAssertObjectArray(env, null, "E8", true);

                env.Milestone(1);

                SendAssertObjectArray(env, b1, "E20", false, 1, ArrayOf("A", "B"));
                SendAssertObjectArray(env, b3, "E21", false, 1, ArrayOf("A", "A"));
                SendAssertObjectArray(env, b2, "E22", false, 2, ArrayOf("A", "B"));
                SendAssertObjectArray(env, b4, "E23", false, 1, ArrayOf("B", "B"));
                SendAssertObjectArray(env, b5, "E24", false);
                SendAssertObjectArray(env, b6, "E25", false, 1);
                SendAssertObjectArray(env, b7, "E26", false, 1, 2);
                //SendAssertObjectArray(env, b8, "E27", true);

                env.UndeployAll();
            }
        }

        public class ViewMultiKeyLastUniqueOneKeyArrayOfLongPrimitive : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select irstream * from SupportEventWithLongArray#unique(Coll)";
                env.CompileDeploy(epl).AddListener("s0");

                RunAssertionLongArray(env);

                env.UndeployAll();
            }
        }

        public class ViewMultiKeyLastUniqueTwoKey : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select irstream * from SupportBean#unique(IntBoxed, LongBoxed)";
                env.CompileDeploy(epl).AddListener("s0");

                var b0 = SendAssertSB(env, "E0", 1, 10L, null);

                env.Milestone(0);

                var b1 = SendAssertSB(env, "E1", 1, 10L, b0);
                var b2 = SendAssertSB(env, "E2", 1, 20L, null);
                var b3 = SendAssertSB(env, "E3", 2, 10L, null);
                var b4 = SendAssertSB(env, "E4", null, null, null);
                var b5 = SendAssertSB(env, "E5", 3, null, null);
                var b6 = SendAssertSB(env, "E6", null, 3L, null);

                env.Milestone(1);

                SendAssertSB(env, "E10", 1, 10L, b1);
                SendAssertSB(env, "E11", 2, 10L, b3);
                SendAssertSB(env, "E12", 1, 20L, b2);
                SendAssertSB(env, "E13", null, null, b4);
                SendAssertSB(env, "E14", 3, null, b5);
                SendAssertSB(env, "E15", null, 3L, b6);

                env.UndeployAll();
            }
        }

        public class ViewMultiKeyLastUniqueThreeKey : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select irstream * from SupportBean#unique(IntBoxed, LongBoxed, DoubleBoxed)";
                env.CompileDeploy(epl).AddListener("s0");

                var b0 = SendAssertSB(env, "E0", 1, 10L, 100d, null);

                env.Milestone(0);

                var b1 = SendAssertSB(env, "E1", 1, 10L, 100d, b0);
                var b2 = SendAssertSB(env, "E2", 1, 20L, 30d, null);
                var b3 = SendAssertSB(env, "E3", 2, 10L, 20d, null);
                var b4 = SendAssertSB(env, "E4", null, null, null, null);
                var b5 = SendAssertSB(env, "E5", 3, null, null, null);
                var b6 = SendAssertSB(env, "E6", null, 3L, null, null);
                var b7 = SendAssertSB(env, "E6", null, null, 3d, null);

                env.Milestone(1);

                SendAssertSB(env, "E10", 1, 10L, 100d, b1);
                SendAssertSB(env, "E11", 2, 10L, 20d, b3);
                SendAssertSB(env, "E12", 1, 20L, 30d, b2);
                SendAssertSB(env, "E13", null, null, null, b4);
                SendAssertSB(env, "E14", 3, null, null, b5);
                SendAssertSB(env, "E15", null, 3L, null, b6);
                SendAssertSB(env, "E15", null, null, 3d, b7);

                env.UndeployAll();
            }
        }

        private static SupportEventWithLongArray SendAssertLongArray(
            RegressionEnvironment env,
            SupportEventWithLongArray expectedRemove,
            string id,
            bool isNull,
            params long[] array)
        {
            var @event = new SupportEventWithLongArray(id, isNull ? null : array);
            env.SendEventBean(@event);
            AssertExpectedRemove(env, expectedRemove);
            return @event;
        }

        private static SupportEventWithLongArray SendAssertLongArrayIStream(
            RegressionEnvironment env,
            bool expected,
            string id,
            bool isNull,
            params long[] array)
        {
            var @event = new SupportEventWithLongArray(id, isNull ? null : array);
            Assert.That(env.Listener("s0").IsInvoked, Is.False);
            env.SendEventBean(@event);
            Assert.That(env.Listener("s0").GetAndClearIsInvoked(), Is.EqualTo(expected));
            Assert.That(env.Listener("s0").IsInvoked, Is.False);
            return @event;
        }

        private static void SendAssertLongArrayIdWindow(
            RegressionEnvironment env,
            string id,
            long[] array,
            string expectedCSV)
        {
            var @event = new SupportEventWithLongArray(id, array);
            env.SendEventBean(@event);
            var ids = (string[]) env.Listener("s0").AssertOneGetNewAndReset().Get("ids");
            EPAssertionUtil.AssertEqualsAnyOrder(expectedCSV.SplitCsv(), ids);
        }

        private static SupportObjectArrayOneDim SendAssertObjectArray(
            RegressionEnvironment env,
            SupportObjectArrayOneDim expectedRemove,
            string id,
            bool isNull,
            params object[] array)
        {
            var @event = new SupportObjectArrayOneDim(id, isNull ? null : array);
            env.SendEventBean(@event);
            AssertExpectedRemove(env, expectedRemove);
            return @event;
        }

        private static SupportBean SendAssertSB(
            RegressionEnvironment env,
            string theString,
            int? intBoxed,
            long? longBoxed,
            SupportBean expectedRemove)
        {
            var sb = new SupportBean(theString, -1);
            sb.IntBoxed = intBoxed;
            sb.LongBoxed = longBoxed;
            env.SendEventBean(sb);
            AssertExpectedRemove(env, expectedRemove);
            return sb;
        }

        private static SupportBean SendAssertSB(
            RegressionEnvironment env,
            string theString,
            int? intBoxed,
            long? longBoxed,
            double? doubleBoxed,
            SupportBean expectedRemove)
        {
            var sb = new SupportBean(theString, -1);
            sb.IntBoxed = intBoxed;
            sb.LongBoxed = longBoxed;
            sb.DoubleBoxed = doubleBoxed;
            env.SendEventBean(sb);
            AssertExpectedRemove(env, expectedRemove);
            return sb;
        }

        private static EventTwoDimArray SendAssertInt2DimArray(
            RegressionEnvironment env,
            EventTwoDimArray expectedRemove,
            string id,
            int[][] ints)
        {
            var @event = new EventTwoDimArray(id, ints);
            env.SendEventBean(@event);
            AssertExpectedRemove(env, expectedRemove);
            return @event;
        }

        private static EventTwoArrayOfPrimitive SendAssertTwoArray(
            RegressionEnvironment env,
            EventTwoArrayOfPrimitive expectedRemove,
            string id,
            int[] one,
            int[] two)
        {
            var @event = new EventTwoArrayOfPrimitive(id, one, two);
            env.SendEventBean(@event);
            AssertExpectedRemove(env, expectedRemove);
            return @event;
        }

        private static EventTwoArrayOfObject SendAssertTwoArray(
            RegressionEnvironment env,
            EventTwoArrayOfObject expectedRemove,
            string id,
            object[] one,
            object[] two)
        {
            var @event = new EventTwoArrayOfObject(id, one, two);
            env.SendEventBean(@event);
            AssertExpectedRemove(env, expectedRemove);
            return @event;
        }

        private static void SendAssertTwoArrayIterate(
            RegressionEnvironment env,
            string id,
            int[] one,
            int[] two,
            string iterateCSV)
        {
            var @event = new EventTwoArrayOfPrimitive(id, one, two);
            env.SendEventBean(@event);
            var ids = EPAssertionUtil.EnumeratorToObjectArr(env.GetEnumerator("s0"), "Id");
            EPAssertionUtil.AssertEqualsAnyOrder(iterateCSV.SplitCsv(), ids);
        }

        private static void SendSBAssertFilter(
            RegressionEnvironment env,
            bool received)
        {
            env.SendEventBean(new SupportBean());
            Assert.AreEqual(received, env.Listener("s0").GetAndClearIsInvoked());
        }

        private static void AssertExpectedRemove(
            RegressionEnvironment env,
            object expectedRemove)
        {
            var old = env.Listener("s0").LastOldData;
            if (expectedRemove != null) {
                Assert.AreEqual(1, old.Length);
                Assert.AreEqual(expectedRemove, old[0].Underlying);
            }
            else {
                Assert.IsNull(old);
            }
        }

        private static void RunAssertionLongArray(RegressionEnvironment env)
        {
            var b0 = SendAssertLongArray(env, null, "E0", false, 1, 2);

            env.Milestone(0);

            var b1 = SendAssertLongArray(env, b0, "E1", false, 1, 2);
            var b2 = SendAssertLongArray(env, null, "E2", false, 2, 1);
            var b3 = SendAssertLongArray(env, null, "E3", false, 2, 2);
            var b4 = SendAssertLongArray(env, null, "E4", false, 2, 2, 2);
            var b5 = SendAssertLongArray(env, null, "E5", false);
            var b6 = SendAssertLongArray(env, null, "E6", false, 1);
            var b7 = SendAssertLongArray(env, null, "E7", true);

            env.Milestone(1);

            SendAssertLongArray(env, b1, "E10", false, 1, 2);
            SendAssertLongArray(env, b3, "E11", false, 2, 2);
            SendAssertLongArray(env, b2, "E12", false, 2, 1);
            SendAssertLongArray(env, b4, "E13", false, 2, 2, 2);
            SendAssertLongArray(env, b5, "E14", false);
            SendAssertLongArray(env, b6, "E15", false, 1);
            SendAssertLongArray(env, b7, "E16", true);
        }

        private static void AssertDataflowIds(
            EPDataFlowInstanceCaptive captive,
            string id,
            long[] longs,
            DefaultSupportCaptureOp<object> capture,
            string csv)
        {
            captive.Emitters.Get("emitterS0").Submit(new SupportEventWithLongArray(id, longs));
            var received = (string[]) ((object[]) capture.GetCurrentAndReset()[0])[0];
            EPAssertionUtil.AssertEqualsAnyOrder(csv.SplitCsv(), received);
        }

        private static string[] ArrayOf(params string[] strings)
        {
            return strings;
        }
    }
} // end of namespace