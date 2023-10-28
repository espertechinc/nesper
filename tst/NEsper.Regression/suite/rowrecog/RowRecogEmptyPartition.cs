///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.rowrecog;

namespace com.espertech.esper.regressionlib.suite.rowrecog
{
    public class RowRecogEmptyPartition : RegressionExecution
    {
        public ISet<RegressionFlag> Flags()
        {
            return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED);
        }

        public void Run(RegressionEnvironment env)
        {
            var fields = "value".SplitCsv();
            var text = "@name('s0') select * from SupportRecogBean#length(10) " +
                       "match_recognize (" +
                       "  partition by Value" +
                       "  measures E1.Value as Value" +
                       "  pattern (E1 E2 | E2 E1 ) " +
                       "  define " +
                       "    E1 as E1.TheString = 'A', " +
                       "    E2 as E2.TheString = 'B' " +
                       ")";

            env.CompileDeploy(text).AddListener("s0");

            env.SendEventBean(new SupportRecogBean("A", 1));

            env.Milestone(0);

            env.SendEventBean(new SupportRecogBean("B", 1));
            env.AssertPropsNew("s0", fields, new object[] { 1 });

            env.Milestone(1);

            env.SendEventBean(new SupportRecogBean("B", 2));

            env.Milestone(2);

            env.SendEventBean(new SupportRecogBean("A", 2));
            env.AssertPropsNew("s0", fields, new object[] { 2 });

            env.Milestone(3);

            env.SendEventBean(new SupportRecogBean("B", 3));
            env.SendEventBean(new SupportRecogBean("A", 4));
            env.SendEventBean(new SupportRecogBean("A", 3));
            env.AssertPropsNew("s0", fields, new object[] { 3 });

            env.Milestone(4);

            env.SendEventBean(new SupportRecogBean("B", 4));
            env.AssertPropsNew("s0", fields, new object[] { 4 });

            env.Milestone(5);

            env.SendEventBean(new SupportRecogBean("A", 6));
            env.SendEventBean(new SupportRecogBean("B", 7));
            env.SendEventBean(new SupportRecogBean("B", 8));
            env.SendEventBean(new SupportRecogBean("A", 7));
            env.AssertPropsNew("s0", fields, new object[] { 7 });

            /// <summary>
            /// Comment-in for testing partition removal.
            /// </summary>
            for (var i = 0; i < 10000; i++) {
                env.SendEventBean(new SupportRecogBean("A", i));
                //Console.WriteLine(i);
                //env.sendEventBean(new SupportRecogBean("B", i));
                //env.assertPropsListenerNew("s0", fields, new Object[] {i});
            }

            env.UndeployAll();
        }
    }
} // end of namespace