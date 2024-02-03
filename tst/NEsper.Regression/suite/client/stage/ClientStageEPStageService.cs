///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.client.stage
{
    public class ClientStageEPStageService
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            Withe(execs);
            return execs;
        }

        public static IList<RegressionExecution> Withe(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientStageEPStageServiceGetStage());
            return execs;
        }

        private class ClientStageEPStageServiceGetStage : ClientStageRegressionExecution
        {
            public override void Run(RegressionEnvironment env)
            {
                var stageAOne = env.StageService.GetStage("A");
                var stageBTwo = env.StageService.GetStage("A");

                ClassicAssert.AreSame(stageAOne, stageBTwo);
                ClassicAssert.AreEqual("A", stageAOne.URI);
                CollectionAssert.AreEquivalent("A".SplitCsv(), env.StageService.StageURIs);
                ClassicAssert.IsNull(env.StageService.GetExistingStage("B"));
                ClassicAssert.AreEqual("A", env.StageService.GetStage("A").URI);

                env.Milestone(0);

                var stageB = env.StageService.GetStage("B");
                ClassicAssert.AreNotSame(stageB, stageAOne);
                CollectionAssert.AreEquivalent("A,B".SplitCsv(), env.StageService.StageURIs);
                ClassicAssert.IsNull(env.StageService.GetExistingStage("C"));
                ClassicAssert.AreEqual("A", env.StageService.GetExistingStage("A").URI);
                ClassicAssert.AreEqual("B", env.StageService.GetExistingStage("B").URI);

                env.Milestone(1);

                var stageC = env.StageService.GetStage("C");
                ClassicAssert.AreNotSame(stageB, stageC);
                CollectionAssert.AreEquivalent("A,B,C".SplitCsv(), env.StageService.StageURIs);
                ClassicAssert.AreEqual("A", env.StageService.GetExistingStage("A").URI);
                ClassicAssert.AreEqual("B", env.StageService.GetExistingStage("B").URI);
                ClassicAssert.AreEqual("C", env.StageService.GetExistingStage("C").URI);

                env.Milestone(2);

                CollectionAssert.AreEquivalent("A,B,C".SplitCsv(), env.StageService.StageURIs);
                env.StageService.GetStage("A").Destroy();
                CollectionAssert.AreEquivalent("B,C".SplitCsv(), env.StageService.StageURIs);
                ClassicAssert.IsNull(env.StageService.GetExistingStage("A"));
                ClassicAssert.AreEqual("B", env.StageService.GetExistingStage("B").URI);
                ClassicAssert.AreEqual("C", env.StageService.GetExistingStage("C").URI);

                env.Milestone(3);

                ClassicAssert.IsNull(env.StageService.GetExistingStage("A"));
                ClassicAssert.AreEqual("B", env.StageService.GetExistingStage("B").URI);
                ClassicAssert.AreEqual("C", env.StageService.GetExistingStage("C").URI);
                env.StageService.GetStage("B").Destroy();
                CollectionAssert.AreEquivalent("C".SplitCsv(), env.StageService.StageURIs);
                ClassicAssert.IsNull(env.StageService.GetExistingStage("A"));
                ClassicAssert.IsNull(env.StageService.GetExistingStage("B"));
                ClassicAssert.AreEqual("C", env.StageService.GetExistingStage("C").URI);

                env.Milestone(4);

                ClassicAssert.AreEqual("C", stageC.URI);
                stageC = env.StageService.GetStage("C");
                stageC.Destroy();
                stageC.Destroy();
                ClassicAssert.AreEqual(0, env.StageService.StageURIs.Length);
                ClassicAssert.IsNull(env.StageService.GetExistingStage("A"));
                ClassicAssert.IsNull(env.StageService.GetExistingStage("B"));
                ClassicAssert.IsNull(env.StageService.GetExistingStage("C"));

                try {
                    env.StageService.GetStage(null);
                    Assert.Fail();
                }
                catch (ArgumentException) {
                    // expected
                }

                try {
                    env.StageService.GetExistingStage(null);
                    Assert.Fail();
                }
                catch (ArgumentException) {
                    // expected
                }

                env.UndeployAll();
            }
        }
    }
} // end of namespace