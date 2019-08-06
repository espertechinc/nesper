///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

namespace com.espertech.esper.regressionlib.suite.infra.nwtable
{
    public class InfraNWTableCreateIndexAdvancedSyntax : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            AssertCompileSODA(env, "create index MyIndex on MyWindow((x,y) dummy_name(\"a\",10101))");
            AssertCompileSODA(env, "create index MyIndex on MyWindow(x dummy_name)");
            AssertCompileSODA(env, "create index MyIndex on MyWindow((x,y,z) dummy_name)");
            AssertCompileSODA(
                env,
                "create index MyIndex on MyWindow(x dummy_name, (y,z) dummy_name_2(\"a\"), p dummyname3)");

            var path = new RegressionPath();
            env.CompileDeploy("create window MyWindow#keepall as SupportSpatialPoint", path);

            TryInvalidCompile(
                env,
                path,
                "create index MyIndex on MyWindow(())",
                "Invalid empty list of index expressions");

            TryInvalidCompile(
                env,
                path,
                "create index MyIndex on MyWindow(IntPrimitive+1)",
                "Invalid index expression 'IntPrimitive+1'");

            TryInvalidCompile(
                env,
                path,
                "create index MyIndex on MyWindow((x, y))",
                "Invalid multiple index expressions");

            TryInvalidCompile(
                env,
                path,
                "create index MyIndex on MyWindow(x.y)",
                "Invalid index expression 'x.y'");

            TryInvalidCompile(
                env,
                path,
                "create index MyIndex on MyWindow(Id xxxx)",
                "Unrecognized advanced-type index 'xxxx'");

            env.UndeployAll();
        }

        private static void AssertCompileSODA(
            RegressionEnvironment env,
            string epl)
        {
            var model = env.EplToModel(epl);
            Assert.AreEqual(epl, model.ToEPL());
        }
    }
} // end of namespace