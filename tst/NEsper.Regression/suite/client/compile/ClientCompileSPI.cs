///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.epl.expression.time.node;
using com.espertech.esper.compat.collections;
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
            WithSPIExpression(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithSPIExpression(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientCompileSPIExpression());
            return execs;
        }

        private static void CompileEvaluate(
            string expression,
            object expected,
            EPCompilerSPIExpression expressionCompiler)
        {
            var actual = CompileEvaluate(expression, expressionCompiler);
            Assert.AreEqual(expected, actual);
        }

        private static object CompileEvaluate(
            string expression,
            EPCompilerSPIExpression expressionCompiler)
        {
            return expressionCompiler.CompileValidate(expression).Forge.ExprEvaluator.Evaluate(null, true, null);
        }

        private class ClientCompileSPIExpression : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var compiler = (EPCompilerSPI) env.Compiler;

                var expressionCompiler = compiler.ExpressionCompiler(new Configuration());

                CompileEvaluate("1*1", 1, expressionCompiler);
                CompileEvaluate("'a' || 'y'", "ay", expressionCompiler);

                var arrays = typeof(Arrays).FullName;

                var list = (ICollection<object>) CompileEvaluate($"{arrays}.AsList({{\"a\"}})", expressionCompiler);
                EPAssertionUtil.AssertEqualsExactOrder(list.ToArray(), new object[] {"a"});

                CompileEvaluate($"{arrays}.AsList({{'a', 'b'}}).firstOf()", "a", expressionCompiler);

                var timePeriod = (ExprTimePeriod) expressionCompiler.CompileValidate("5 seconds");
                Assert.AreEqual(5d, timePeriod.EvaluateAsSeconds(null, true, null), 0.0001);
            }
        }
    }
} // end of namespace