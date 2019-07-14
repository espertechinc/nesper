///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compiler.client;
using com.espertech.esper.compiler.client.option;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.client.compile
{
    public class ClientCompileUserObject
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            execs.Add(new ClientCompileUserObjectDifferentTypes());
            execs.Add(new ClientCompileUserObjectResolveContextInfo());
            return execs;
        }

        private static void AssertUserObject(
            RegressionEnvironment env,
            object userObject)
        {
            var args = new CompilerArguments(env.Configuration);
            args.Options.StatementUserObject = _ => userObject;
            var compiled = env.Compile("@Name('s0') select * from SupportBean", args);
            env.Deploy(compiled);
            var received = env.Statement("s0").UserObjectCompileTime;
            if (received == null) {
                Assert.IsNull(userObject);
            }
            else if (received.GetType() == typeof(int[])) {
                Assert.IsTrue(Equals((int[]) received, (int[]) userObject));
            }
            else {
                Assert.AreEqual(userObject, env.Statement("s0").UserObjectCompileTime);
            }

            env.UndeployAll();
        }

        internal class ClientCompileUserObjectResolveContextInfo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                MyUserObjectResolver.Contexts.Clear();
                var args = new CompilerArguments(env.Configuration);
                args.Options.StatementUserObject = new MyUserObjectResolver().GetValue;
                var epl = "@Name('s0') select * from SupportBean";
                env.Compile(epl, args);

                var ctx = MyUserObjectResolver.Contexts[0];
                Assert.AreEqual(epl, ctx.EplSupplier.Invoke());
                Assert.AreEqual("s0", ctx.StatementName);
                Assert.AreEqual(null, ctx.ModuleName);
                Assert.AreEqual(1, ctx.Annotations.Length);
                Assert.AreEqual(0, ctx.StatementNumber);
            }
        }

        internal class ClientCompileUserObjectDifferentTypes : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                AssertUserObject(env, "ABC");
                AssertUserObject(env, new[] {1, 2, 3});
                AssertUserObject(env, null);
                AssertUserObject(env, new MyUserObject("hello"));
            }
        }

        [Serializable]
        internal class MyUserObject
        {
            private string id;

            public MyUserObject(string id)
            {
                this.id = id;
            }

            public MyUserObject()
            {
            }

            public string Id {
                get => id;
                set => id = value;
            }

            public override bool Equals(object o)
            {
                if (this == o) {
                    return true;
                }

                if (o == null || GetType() != o.GetType()) {
                    return false;
                }

                var that = (MyUserObject) o;

                return id.Equals(that.id);
            }

            public override int GetHashCode()
            {
                return id.GetHashCode();
            }
        }

        internal class MyUserObjectResolver
        {
            public static IList<StatementUserObjectContext> Contexts { get; } = new List<StatementUserObjectContext>();

            public object GetValue(StatementUserObjectContext env)
            {
                Contexts.Add(env);
                return null;
            }
        }
    }
} // end of namespace