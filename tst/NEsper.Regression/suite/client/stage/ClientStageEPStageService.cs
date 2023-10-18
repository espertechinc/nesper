///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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

namespace com.espertech.esper.regressionlib.suite.client.stage
{
	public class ClientStageEPStageService {

	    public static IList<RegressionExecution> Executions() {
	        IList<RegressionExecution> execs = new List<RegressionExecution>();
	        execs.Add(new ClientStageEPStageServiceGetStage());
	        return execs;
	    }

	    private class ClientStageEPStageServiceGetStage : ClientStageRegressionExecution {
	        public override void Run(RegressionEnvironment env) {
	            var stageAOne = env.StageService.GetStage("A");
	            var stageBTwo = env.StageService.GetStage("A");

	            Assert.AreSame(stageAOne, stageBTwo);
	            Assert.AreEqual("A", stageAOne.URI);
	            CollectionAssert.AreEquivalent("A".SplitCsv(), env.StageService.StageURIs);
	            Assert.IsNull(env.StageService.GetExistingStage("B"));
	            Assert.AreEqual("A", env.StageService.GetStage("A").URI);

	            env.Milestone(0);

	            var stageB = env.StageService.GetStage("B");
	            Assert.AreNotSame(stageB, stageAOne);
	            CollectionAssert.AreEquivalent("A,B".SplitCsv(), env.StageService.StageURIs);
	            Assert.IsNull(env.StageService.GetExistingStage("C"));
	            Assert.AreEqual("A", env.StageService.GetExistingStage("A").URI);
	            Assert.AreEqual("B", env.StageService.GetExistingStage("B").URI);

	            env.Milestone(1);

	            var stageC = env.StageService.GetStage("C");
	            Assert.AreNotSame(stageB, stageC);
	            CollectionAssert.AreEquivalent("A,B,C".SplitCsv(), env.StageService.StageURIs);
	            Assert.AreEqual("A", env.StageService.GetExistingStage("A").URI);
	            Assert.AreEqual("B", env.StageService.GetExistingStage("B").URI);
	            Assert.AreEqual("C", env.StageService.GetExistingStage("C").URI);

	            env.Milestone(2);

	            CollectionAssert.AreEquivalent("A,B,C".SplitCsv(), env.StageService.StageURIs);
	            env.StageService.GetStage("A").Destroy();
	            CollectionAssert.AreEquivalent("B,C".SplitCsv(), env.StageService.StageURIs);
	            Assert.IsNull(env.StageService.GetExistingStage("A"));
	            Assert.AreEqual("B", env.StageService.GetExistingStage("B").URI);
	            Assert.AreEqual("C", env.StageService.GetExistingStage("C").URI);

	            env.Milestone(3);

	            Assert.IsNull(env.StageService.GetExistingStage("A"));
	            Assert.AreEqual("B", env.StageService.GetExistingStage("B").URI);
	            Assert.AreEqual("C", env.StageService.GetExistingStage("C").URI);
	            env.StageService.GetStage("B").Destroy();
	            CollectionAssert.AreEquivalent("C".SplitCsv(), env.StageService.StageURIs);
	            Assert.IsNull(env.StageService.GetExistingStage("A"));
	            Assert.IsNull(env.StageService.GetExistingStage("B"));
	            Assert.AreEqual("C", env.StageService.GetExistingStage("C").URI);

	            env.Milestone(4);

	            Assert.AreEqual("C", stageC.URI);
	            stageC = env.StageService.GetStage("C");
	            stageC.Destroy();
	            stageC.Destroy();
	            Assert.AreEqual(0, env.StageService.StageURIs.Length);
	            Assert.IsNull(env.StageService.GetExistingStage("A"));
	            Assert.IsNull(env.StageService.GetExistingStage("B"));
	            Assert.IsNull(env.StageService.GetExistingStage("C"));

	            try {
	                env.StageService.GetStage(null);
	                Assert.Fail();
	            } catch (ArgumentException) {
	                // expected
	            }

	            try {
	                env.StageService.GetExistingStage(null);
	                Assert.Fail();
	            } catch (ArgumentException) {
	                // expected
	            }

	            env.UndeployAll();
	        }
	    }
	}
} // end of namespace
