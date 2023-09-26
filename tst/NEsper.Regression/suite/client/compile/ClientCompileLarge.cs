///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Text;

using com.espertech.esper.regressionlib.framework;

namespace com.espertech.esper.regressionlib.suite.client.compile
{
    public class ClientCompileLarge
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
#if REGRESSION_EXECUTIONS
            With(LargeConstantPoolDueToMethods)(execs);
#endif
            return execs;
        }

        public static IList<RegressionExecution> WithLargeConstantPoolDueToMethods(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientCompileLargeConstantPoolDueToMethods());
            return execs;
        }

        public class ClientCompileLargeConstantPoolDueToMethods : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var buf = new StringBuilder();
                buf.Append("select ");
                var delimiter = "";
                for (var i = 0; i < 1000; i++) {
                    buf.Append(delimiter);
                    buf.Append(
                        "((((((((((((((((((((((((1+1)+1)+1)+1)+1)+1)+1)+1)+1)+1)+1)+1)+1)+1)+1)+1)+1)+1)+1)+1)+1)+1)+1)+1)+1");
                    buf.Append(" as z" + i);
                    delimiter = ",";
                }

                buf.Append(" from SupportBean");

                env.Compile(buf.ToString());
            }
        }
    }
} // end of namespace