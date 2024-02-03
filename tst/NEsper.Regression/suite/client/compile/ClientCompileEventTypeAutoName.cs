///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.autoname.two;

namespace com.espertech.esper.regressionlib.suite.client.compile
{
    public class ClientCompileEventTypeAutoName
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
#if REGRESSION_EXECUTIONS
            WithResolve(execs);
            With(Ambiguous)(execs);
#endif
            return execs;
        }

        public static IList<RegressionExecution> WithAmbiguous(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientCompileAutoNameAmbiguous());
            return execs;
        }

        public static IList<RegressionExecution> WithResolve(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientCompileAutoNameResolve());
            return execs;
        }

        public class ClientCompileAutoNameResolve : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@public @buseventtype create schema MANE as MyAutoNameEvent;\n" +
                          "@name('s0') select P0 from MANE;\n";
                var compiled = env.Compile(epl);
                env.Deploy(compiled).AddListener("s0");

                env.SendEventBean(new MyAutoNameEvent("test"), "MANE");
                env.AssertEqualsNew("s0", "P0", "test");

                env.UndeployAll();
            }
        }

        public class ClientCompileAutoNameAmbiguous : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.TryInvalidCompile(
                    "create schema SupportAmbiguousEventType as SupportAmbiguousEventType",
                    "Failed to resolve name 'SupportAmbiguousEventType', the class was ambiguously found both in namespace 'com.espertech.esper.regressionlib.support.autoname.one' and in namespace 'com.espertech.esper.regressionlib.support.autoname.two'");
            }
        }
    }
} // end of namespace