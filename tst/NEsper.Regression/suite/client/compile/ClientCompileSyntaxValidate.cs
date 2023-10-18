///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.module;
using com.espertech.esper.common.client.soda;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compiler.client;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework; // fail

namespace com.espertech.esper.regressionlib.suite.client.compile
{
	public class ClientCompileSyntaxValidate {
	    public static IList<RegressionExecution> Executions() {
	        IList<RegressionExecution> execs = new List<RegressionExecution>();
	        execs.Add(new ClientCompileOptionsValidateOnly());
	        execs.Add(new ClientCompileSyntaxMgs());
	        return execs;
	    }

	    private class ClientCompileSyntaxMgs : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            env.TryInvalidCompile("insert into 7event select * from SupportBeanReservedKeyword",
	                "Incorrect syntax near '7' at line 1 column 12");

	            env.TryInvalidCompile("select foo, create from SupportBeanReservedKeyword",
	                "Incorrect syntax near 'create' (a reserved keyword) at line 1 column 12, please check the select clause");

	            env.TryInvalidCompile("select * from pattern [",
	                "Unexpected end-of-input at line 1 column 23, please check the pattern expression within the from clause");

	            env.TryInvalidCompile("select * from A, into",
	                "Incorrect syntax near 'into' (a reserved keyword) at line 1 column 17, please check the from clause");

	            env.TryInvalidCompile("select * from pattern[A -> B - C]",
	                "Incorrect syntax near '-' expecting a right angle bracket ']' but found a minus '-' at line 1 column 29, please check the from clause");

	            env.TryInvalidCompile("insert into A (a",
	                "Unexpected end-of-input at line 1 column 16 [insert into A (a]");

	            env.TryInvalidCompile("select case when 1>2 from A",
	                "Incorrect syntax near 'from' (a reserved keyword) expecting 'then' but found 'from' at line 1 column 21, please check the case expression within the select clause [select case when 1>2 from A]");

	            env.TryInvalidCompile("select * from A full outer join B on A.field < B.field",
	                "Incorrect syntax near '<' expecting an equals '=' but found a lesser then '<' at line 1 column 45, please check the outer join within the from clause [select * from A full outer join B on A.field < B.field]");

	            env.TryInvalidCompile("select a.b('aa\") from A",
	                "Failed to parse: Unexpected exception recognizing module text, recognition failed for LexerNoViableAltException(''')");

	            env.TryInvalidCompile("select * from A, sql:mydb [\"",
	                "Failed to parse: Unexpected exception recognizing module text, recognition failed for LexerNoViableAltException('\"')");

	            env.TryInvalidCompile("select * google",
	                "Incorrect syntax near 'google' at line 1 column 9 [");

	            env.TryInvalidCompile("insert into into",
	                "Incorrect syntax near 'into' (a reserved keyword) at line 1 column 12 [insert into into]");

	            env.TryInvalidCompile("on SupportBean select 1",
	                "Required insert-into clause is not provided, the clause is required for split-stream syntax");
	        }

	        public ISet<RegressionFlag> Flags() {
	            return Collections.Set(RegressionFlag.COMPILEROPS, RegressionFlag.INVALIDITY);
	        }
	    }

	    private class ClientCompileOptionsValidateOnly : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var module = new Module();
	            module.Items.Add(new ModuleItem("select * from NoSuchEvent"));
	            try {
	                env.Compiler.SyntaxValidate(module, null);
	            } catch (EPCompileException e) {
	                throw new EPRuntimeException(e);
	            }

	            module = new Module();
	            module.Items.Add(new ModuleItem("xxx"));
	            try {
	                env.Compiler.SyntaxValidate(module, null);
	                Assert.Fail();
	            } catch (EPCompileException ex) {
	                SupportMessageAssertUtil.AssertMessage(ex, "Incorrect syntax near 'xxx'");
	            }

	            module = new Module();
	            var model = new EPStatementObjectModel();
	            module.Items.Add(new ModuleItem(model));
	            try {
	                env.Compiler.SyntaxValidate(module, null);
	                Assert.Fail();
	            } catch (EPCompileException ex) {
	                SupportMessageAssertUtil.AssertMessage(ex, "Select-clause has not been defined");
	            }
	        }

	        public ISet<RegressionFlag> Flags() {
	            return Collections.Set(RegressionFlag.COMPILEROPS);
	        }
	    }
	}
} // end of namespace
