///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;
using static com.espertech.esper.regressionlib.support.stage.SupportStageUtil;

namespace com.espertech.esper.regressionlib.suite.client.stage
{
	public class ClientStageObjectResolution
	{
		private const string EPL_NAMED_WINDOW = "@public create window MyWindow#keepall as SupportBean;\n";
		private const string EPL_CONTEXT = "@public create context MyContext initiated by SupportBean_S0;\n";
		private const string EPL_VARIABLE = "@public create variable int MyVariable;\n";
		private const string EPL_EVENT_TYPE = "@public create schema MyEvent();\n";
		private const string EPL_TABLE = "@public create table MyTable(k string);\n";
		private const string EPL_EXPRESSION = "@public create expression MyExpression {1};\n";
		private const string EPL_SCRIPT = "@public create expression MyScript(params)[ ];\n";

		private const string EPL_OBJECTS = "@Name('eplobjects') " +
		                                   EPL_NAMED_WINDOW +
		                                   EPL_CONTEXT +
		                                   EPL_VARIABLE +
		                                   EPL_EVENT_TYPE +
		                                   EPL_TABLE +
		                                   EPL_EXPRESSION +
		                                   EPL_SCRIPT;

		public static IList<RegressionExecution> Executions()
		{
			IList<RegressionExecution> execs = new List<RegressionExecution>();
			execs.Add(new ClientStageObjectResolutionAfterStaging());
			execs.Add(new ClientStageObjectAlreadyExists());
			return execs;
		}

		private class ClientStageObjectAlreadyExists : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var compiled = env.Compile(EPL_OBJECTS);
				env.Deploy(compiled);

				var idCreate = env.DeploymentId("eplobjects");
				env.StageService.GetStage("S1");
				StageIt(env, "S1", idCreate);

				TryInvalidDeploy(
					env,
					env.Compile(EPL_NAMED_WINDOW),
					"A precondition is not satisfied: named window by name 'MyWindow' is already defined by stage 'S1'");
				TryInvalidDeploy(
					env,
					env.Compile(EPL_CONTEXT),
					"A precondition is not satisfied: context by name 'MyContext' is already defined by stage 'S1'");
				TryInvalidDeploy(
					env,
					env.Compile(EPL_VARIABLE),
					"A precondition is not satisfied: variable by name 'MyVariable' is already defined by stage 'S1'");
				TryInvalidDeploy(
					env,
					env.Compile(EPL_EVENT_TYPE),
					"A precondition is not satisfied: event type by name 'MyEvent' is already defined by stage 'S1'");
				TryInvalidDeploy(
					env,
					env.Compile(EPL_TABLE),
					"A precondition is not satisfied: event type by name 'table_internal_MyTable' is already defined by stage 'S1'");
				TryInvalidDeploy(
					env,
					env.Compile(EPL_EXPRESSION),
					"A precondition is not satisfied: expression by name 'MyExpression' is already defined by stage 'S1'");
				TryInvalidDeploy(
					env,
					env.Compile(EPL_SCRIPT),
					"A precondition is not satisfied: script by name 'MyScript (1 parameters)' is already defined by stage 'S1'");

				UnstageIt(env, "S1", idCreate);

				env.UndeployAll();
			}
		}

		private class ClientStageObjectResolutionAfterStaging : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var path = new RegressionPath();
				env.CompileDeploy(EPL_OBJECTS, path);

				var idCreate = env.DeploymentId("eplobjects");
				env.StageService.GetStage("S1");
				StageIt(env, "S1", idCreate);

				var eplNamedWindow = "select * from MyWindow";
				TryInvalidDeploy(env, path, eplNamedWindow, "A precondition is not satisfied: Required dependency named window 'MyWindow' cannot be found");
				TryInvalidFAF(env, path, eplNamedWindow, "Failed to resolve path named window 'MyWindow'");

				var eplContext = "context MyContext select count(*) from SupportBean";
				TryInvalidDeploy(env, path, eplContext, "A precondition is not satisfied: Required dependency context 'MyContext' cannot be found");

				var eplVariable = "select MyVariable from SupportBean";
				TryInvalidDeploy(env, path, eplVariable, "A precondition is not satisfied: Required dependency variable 'MyVariable' cannot be found");

				var eplEventType = "select * from MyEvent";
				TryInvalidDeploy(env, path, eplEventType, "A precondition is not satisfied: Required dependency event type 'MyEvent' cannot be found");

				var eplTable = "select MyTable from SupportBean";
				TryInvalidDeploy(env, path, eplTable, "A precondition is not satisfied: Required dependency table 'MyTable' cannot be found");

				var eplExpression = "select MyExpression from SupportBean";
				TryInvalidDeploy(
					env,
					path,
					eplExpression,
					"A precondition is not satisfied: Required dependency declared-expression 'MyExpression' cannot be found");

				var eplScript = "select MyScript(TheString) from SupportBean";
				TryInvalidDeploy(env, path, eplScript, "A precondition is not satisfied: Required dependency script 'MyScript' cannot be found");

				UnstageIt(env, "S1", idCreate);

				env.CompileDeploy(eplNamedWindow, path);
				env.CompileExecuteFAF(eplNamedWindow, path);
				env.CompileDeploy(eplContext, path);

				env.UndeployAll();
			}

		}

		private static void TryInvalidFAF(
			RegressionEnvironment env,
			RegressionPath path,
			string query,
			string expected)
		{
			var compiled = env.CompileFAF(query, path);
			try {
				env.Runtime.FireAndForgetService.ExecuteQuery(compiled);
				Assert.Fail();
			}
			catch (EPException ex) {
				SupportMessageAssertUtil.AssertMessage(ex.Message, expected);
			}
		}
	}
} // end of namespace
