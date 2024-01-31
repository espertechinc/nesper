///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat.collections;
using com.espertech.esper.compiler.client;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;
using NUnit.Framework.Legacy;
using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

namespace com.espertech.esper.regressionlib.suite.client.compile
{
    public class ClientCompileExceptionItems
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithceptionTwoItems(execs);
            WithceptionMultiLineMultiItem(execs);
            WitheptionEPLWNewline(execs);
            return execs;
        }

        public static IList<RegressionExecution> WitheptionEPLWNewline(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientCompileExeptionEPLWNewline());
            return execs;
        }

        public static IList<RegressionExecution> WithceptionMultiLineMultiItem(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientCompileExceptionMultiLineMultiItem());
            return execs;
        }

        public static IList<RegressionExecution> WithceptionTwoItems(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientCompileExceptionTwoItems());
            return execs;
        }

        public class ClientCompileExceptionMultiLineMultiItem : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create schema\n" +
                          "MySchemaOne\n" +
                          "(\n" +
                          "  col1 Wrong\n" +
                          ");\n" +
                          "create schema\n" +
                          "MySchemaTwo\n" +
                          "(\n" +
                          "  col1 WrongTwo\n" +
                          ");\n";
                try {
                    env.Compiler.Compile(epl, new CompilerArguments());
                    Assert.Fail();
                }
                catch (EPCompileException ex) {
                    AssertMessage(
                        ex,
                        "Nestable type configuration encountered an unexpected property type name 'Wrong' for property 'col1'");
                    ClassicAssert.AreEqual(2, ex.Items.Count);
                    AssertItem(
                        ex.Items[0],
                        "create schema MySchemaOne (   col1 Wrong )",
                        1,
                        "Nestable type configuration encountered an unexpected property type name 'Wrong' for property 'col1'");
                    AssertItem(
                        ex.Items[1],
                        "create schema MySchemaTwo (   col1 WrongTwo )",
                        6,
                        "Nestable type configuration encountered an unexpected property type name 'WrongTwo' for property 'col1'");
                }
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.COMPILEROPS, RegressionFlag.INVALIDITY);
            }
        }

        public class ClientCompileExceptionTwoItems : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create schema MySchemaOne (col1 Wrong);\n" +
                          "create schema MySchemaTwo (col1 WrongTwo);\n";
                try {
                    env.Compiler.Compile(epl, new CompilerArguments());
                    Assert.Fail();
                }
                catch (EPCompileException ex) {
                    AssertMessage(
                        ex,
                        "Nestable type configuration encountered an unexpected property type name 'Wrong' for property 'col1'");
                    ClassicAssert.AreEqual(2, ex.Items.Count);
                    AssertItem(
                        ex.Items[0],
                        "create schema MySchemaOne (col1 Wrong)",
                        1,
                        "Nestable type configuration encountered an unexpected property type name 'Wrong' for property 'col1'");
                    AssertItem(
                        ex.Items[1],
                        "create schema MySchemaTwo (col1 WrongTwo)",
                        2,
                        "Nestable type configuration encountered an unexpected property type name 'WrongTwo' for property 'col1'");
                }
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.COMPILEROPS, RegressionFlag.INVALIDITY);
            }
        }

        public class ClientCompileExeptionEPLWNewline : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                try {
                    env.Compiler.Compile("XX\nX", new CompilerArguments());
                    Assert.Fail();
                }
                catch (EPCompileException ex) {
                    AssertMessage(ex, "Incorrect syntax near 'XX' [XX X]");
                    ClassicAssert.AreEqual(1, ex.Items.Count);
                    AssertItem(ex.Items[0], "XX X", 1, "Incorrect syntax near 'XX'");
                }
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.COMPILEROPS, RegressionFlag.INVALIDITY);
            }
        }

        private static void AssertItem(
            EPCompileExceptionItem item,
            string expression,
            int lineNumber,
            string expectedMsg)
        {
            ClassicAssert.AreEqual(expression, item.Expression);
            ClassicAssert.AreEqual(lineNumber, item.LineNumber);
            AssertMessage(item.InnerException.Message, expectedMsg);
        }
    }
} // end of namespace