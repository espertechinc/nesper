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
using com.espertech.esper.compiler.client;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

namespace com.espertech.esper.regressionlib.suite.client.compile
{
    public class ClientCompileSyntaxValidate
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            execs.Add(new ClientCompileOptionsValidateOnly());
            execs.Add(new ClientCompileSyntaxMgs());
            return execs;
        }

        internal class ClientCompileSyntaxMgs : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                TryInvalidCompile(
                    env,
                    "insert into 7event select * from SupportBeanReservedKeyword",
                    "Incorrect syntax near '7' at line 1 column 12");

                TryInvalidCompile(
                    env,
                    "select foo, create from SupportBeanReservedKeyword",
                    "Incorrect syntax near 'create' (a reserved keyword) at line 1 column 12, please check the select clause");

                TryInvalidCompile(
                    env,
                    "select * from pattern [",
                    "Unexpected end-of-input at line 1 column 23, please check the pattern expression within the from clause");

                TryInvalidCompile(
                    env,
                    "select * from A, into",
                    "Incorrect syntax near 'into' (a reserved keyword) at line 1 column 17, please check the from clause");

                TryInvalidCompile(
                    env,
                    "select * from pattern[A => B - C]",
                    "Incorrect syntax near '-' expecting a right angle bracket ']' but found a minus '-' at line 1 column 29, please check the from clause");

                TryInvalidCompile(
                    env,
                    "insert into A (a",
                    "Unexpected end-of-input at line 1 column 16 [insert into A (a]");

                TryInvalidCompile(
                    env,
                    "select case when 1>2 from A",
                    "Incorrect syntax near 'from' (a reserved keyword) expecting 'then' but found 'from' at line 1 column 21, please check the case expression within the select clause [select case when 1>2 from A]");

                TryInvalidCompile(
                    env,
                    "select * from A full outer join B on A.field < B.field",
                    "Incorrect syntax near '<' expecting an equals '=' but found a lesser then '<' at line 1 column 45, please check the outer join within the from clause [select * from A full outer join B on A.field < B.field]");

                TryInvalidCompile(
                    env,
                    "select a.b('aa\") from A",
                    "Failed to parse: Unexpected exception recognizing module text, recognition failed for LexerNoViableAltException(''')");

                TryInvalidCompile(
                    env,
                    "select * from A, sql:mydb [\"",
                    "Failed to parse: Unexpected exception recognizing module text, recognition failed for LexerNoViableAltException('\"')");

                TryInvalidCompile(
                    env,
                    "select * google",
                    "Incorrect syntax near 'google' at line 1 column 9 [");

                TryInvalidCompile(
                    env,
                    "insert into into",
                    "Incorrect syntax near 'into' (a reserved keyword) at line 1 column 12 [insert into into]");

                TryInvalidCompile(
                    env,
                    "on SupportBean select 1",
                    "Required insert-into clause is not provIded, the clause is required for split-stream syntax");
            }
        }

        internal class ClientCompileOptionsValidateOnly : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var module = new Module();
                module.Items.Add(new ModuleItem("select * from NoSuchEvent"));
                try {
                    EPCompilerProvider.Compiler.SyntaxValidate(module, null);
                }
                catch (EPCompileException e) {
                    throw new EPException(e);
                }

                module = new Module();
                module.Items.Add(new ModuleItem("xxx"));
                try {
                    EPCompilerProvider.Compiler.SyntaxValidate(module, null);
                    Assert.Fail();
                }
                catch (EPCompileException ex) {
                    AssertMessage(ex, "Incorrect syntax near 'xxx'");
                }

                module = new Module();
                var model = new EPStatementObjectModel();
                module.Items.Add(new ModuleItem(model));
                try {
                    EPCompilerProvider.Compiler.SyntaxValidate(module, null);
                    Assert.Fail();
                }
                catch (EPCompileException ex) {
                    AssertMessage(ex, "Select-clause has not been defined");
                }
            }
        }
    }
} // end of namespace