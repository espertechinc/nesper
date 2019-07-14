///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.@internal.epl.expression.time.node;
using com.espertech.esper.compiler.client;
using com.espertech.esper.compiler.@internal.util;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.client.compile
{
    public class ClientCompileSPI
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            execs.Add(new ClientCompileSPIExpression());
            return execs;
        }

        private static void CompileEvaluate(
            string expression,
            object expected,
            EPCompilerSPIExpression expressionCompiler)
        {
            object actual = null;
            try {
                actual = expressionCompiler.CompileValidate(expression).Forge.ExprEvaluator.Evaluate(null, true, null);
            }
            catch (EPCompileException e) {
                Assert.Fail(e.Message);
            }

            Assert.AreEqual(expected, actual);
        }

        internal class ClientCompileSPIExpression : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var compiler = (EPCompilerSPI) EPCompilerProvider.Compiler;

                EPCompilerSPIExpression expressionCompiler = null;
                try {
                    expressionCompiler = compiler.ExpressionCompiler(new Configuration());
                }
                catch (EPCompileException e) {
                    Assert.Fail(e.Message);
                }

                CompileEvaluate("1*1", 1, expressionCompiler);
                CompileEvaluate("'a' || 'y'", "ay", expressionCompiler);

                try {
                    var timePeriod = (ExprTimePeriod) expressionCompiler.CompileValidate("5 seconds");
                    Assert.AreEqual(5d, timePeriod.EvaluateAsSeconds(null, true, null), 0.0001);
                }
                catch (EPCompileException e) {
                    Assert.Fail(e.Message);
                }
            }
        }
    }
} // end of namespace