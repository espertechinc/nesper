///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.common.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
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
            WithDifferentTypes(execs);
            WithResolveContextInfo(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithResolveContextInfo(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientCompileUserObjectResolveContextInfo());
            return execs;
        }

        public static IList<RegressionExecution> WithDifferentTypes(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientCompileUserObjectDifferentTypes());
            return execs;
        }

        private class ClientCompileUserObjectResolveContextInfo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                MyUserObjectResolver.Contexts.Clear();
                var args = new CompilerArguments(env.Configuration);
                args.Options.StatementUserObject = (new MyUserObjectResolver()).GetValue;
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

        private class ClientCompileUserObjectDifferentTypes : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                AssertUserObject(env, "ABC");
                AssertUserObject(env, new int[] {1, 2, 3});
                AssertUserObject(env, null);
                AssertUserObject(env, new MyUserObject("hello"));
            }
        }

        private static void AssertUserObject(
            RegressionEnvironment env,
            object userObject)
        {
            var args = new CompilerArguments(env.Configuration);
            args.Options.SetStatementUserObject(_ => userObject);
            var compiled = env.Compile("@Name('s0') select * from SupportBean", args);
            env.Deploy(compiled);
            var received = env.Statement("s0").UserObjectCompileTime;
            if (received == null) {
                Assert.IsNull(userObject);
            }
            else if (received.GetType() == typeof(int[])) {
                Assert.IsTrue(Arrays.AreEqual((int[]) received, (int[]) userObject));
            }
            else {
                Assert.AreEqual(userObject, env.Statement("s0").UserObjectCompileTime);
            }

            env.UndeployAll();
        }

        [Serializable]
        private class MyUserObject
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

            protected bool Equals(MyUserObject other)
            {
                return id == other.id;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) {
                    return false;
                }

                if (ReferenceEquals(this, obj)) {
                    return true;
                }

                if (obj.GetType() != this.GetType()) {
                    return false;
                }

                return Equals((MyUserObject) obj);
            }

            public override int GetHashCode()
            {
                return (id != null ? id.GetHashCode() : 0);
            }
        }

        private class MyUserObjectResolver
        {
            private static IList<StatementUserObjectContext> contexts = new List<StatementUserObjectContext>();

            public static IList<StatementUserObjectContext> Contexts => contexts;

            public object GetValue(StatementUserObjectContext env)
            {
                contexts.Add(env);
                return null;
            }
        }
    }
} // end of namespace