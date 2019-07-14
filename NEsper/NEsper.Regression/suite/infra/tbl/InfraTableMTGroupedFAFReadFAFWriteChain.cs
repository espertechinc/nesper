///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.fireandforget;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.infra.tbl
{
    /// <summary>
    ///     NOTE: More table-related tests in "nwtable"
    /// </summary>
    public class InfraTableMTGroupedFAFReadFAFWriteChain : RegressionExecution
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        ///     Tests fire-and-forget lock cleanup:
        ///     create table MyTable(key int primary key, p0 int)   (5 props)
        ///     <para />
        ///     The following threads are in a chain communicating by queue holding key values:
        ///     - Insert: populates MyTable={key=N, p0=N}, last row indicated by -1
        ///     - Select-Table-Access: select MyTable[N].p0 from SupportBean
        /// </summary>
        public void Run(RegressionEnvironment env)
        {
            try {
                TryMT(env, 1000);
            }
            catch (ThreadInterruptedException e) {
                throw new EPException(e);
            }
        }

        private static void TryMT(
            RegressionEnvironment env,
            int numInserted)
        {
            var path = new RegressionPath();
            var epl = "create table MyTable (key int primary key, p0 int);";
            env.CompileDeploy(epl, path);

            IList<BaseRunnable> runnables = new List<BaseRunnable>();

            var insertOutQ = new LinkedBlockingQueue<int>();
            var insert = new InsertRunnable(env, path, numInserted, insertOutQ);
            runnables.Add(insert);

            var selectOutQ = new LinkedBlockingQueue<int>();
            var select = new SelectRunnable(env, path, insertOutQ, selectOutQ);
            runnables.Add(select);

            var updateOutQ = new LinkedBlockingQueue<int>();
            var update = new UpdateRunnable(env, path, selectOutQ, updateOutQ);
            runnables.Add(update);

            IBlockingQueue<int> deleteOutQ = new LinkedBlockingQueue<int>();
            var delete = new DeleteRunnable(env, path, updateOutQ, deleteOutQ);
            runnables.Add(delete);

            // start
            var threads = new Thread[runnables.Count];
            for (var i = 0; i < runnables.Count; i++) {
                threads[i] = new Thread(runnables[i].Run);
                threads[i].Name = typeof(InfraTableMTGroupedFAFReadFAFWriteChain).Name + "-" + i;
                threads[i].Start();
            }

            // join
            foreach (var t in threads) {
                t.Join();
            }

            env.UndeployAll();

            // assert
            foreach (var runnable in runnables) {
                Assert.IsNull(runnable.Exception);
                Assert.AreEqual(
                    numInserted + 1,
                    runnable.NumberOfOperations,
                    "failed for " + runnable); // account for -1 indicator
            }
        }

        public abstract class BaseRunnable
        {
            protected readonly RegressionEnvironment env;
            protected readonly RegressionPath path;
            protected readonly string workName;
            protected int numberOfOperations;

            protected BaseRunnable(
                RegressionEnvironment env,
                RegressionPath path,
                string workName)
            {
                this.env = env;
                this.path = path;
                this.workName = workName;
            }

            public Exception Exception { get; private set; }

            public int NumberOfOperations => numberOfOperations;

            public abstract void RunWork();

            public void Run()
            {
                log.Info("Starting " + workName);
                try {
                    RunWork();
                }
                catch (Exception ex) {
                    log.Error("Exception encountered: " + ex.Message, ex);
                    Exception = ex;
                }

                log.Info("Completed " + workName);
            }
        }

        public class InsertRunnable : BaseRunnable
        {
            private readonly int numInserted;
            private readonly IBlockingQueue<int> stageOutput;

            public InsertRunnable(
                RegressionEnvironment env,
                RegressionPath path,
                int numInserted,
                IBlockingQueue<int> stageOutput) : base(env, path, "Insert")
            {
                this.numInserted = numInserted;
                this.stageOutput = stageOutput;
            }

            public override void RunWork()
            {
                var compiled = env.CompileFAF(
                    "insert into MyTable (key, p0) values (cast(?, int), cast(?, int))",
                    path);
                var q = env.Runtime.FireAndForgetService.PrepareQueryWithParameters(compiled);
                for (var i = 0; i < numInserted; i++) {
                    Process(q, i);
                }

                Process(q, -1);
            }

            private void Process(
                EPFireAndForgetPreparedQueryParameterized q,
                int id)
            {
                q.SetObject(1, id);
                q.SetObject(2, id);
                env.Runtime.FireAndForgetService.ExecuteQuery(q);
                stageOutput.Push(id);
                numberOfOperations++;
            }
        }

        public class SelectRunnable : BaseRunnable
        {
            private readonly IBlockingQueue<int> stageInput;
            private readonly IBlockingQueue<int> stageOutput;

            public SelectRunnable(
                RegressionEnvironment env,
                RegressionPath path,
                IBlockingQueue<int> stageInput,
                IBlockingQueue<int> stageOutput) : base(env, path, "Select")
            {
                this.stageInput = stageInput;
                this.stageOutput = stageOutput;
            }

            public override void RunWork()
            {
                var epl = "select p0 from MyTable where key = cast(?, int)";
                var compiled = env.CompileFAF(epl, path);
                var q = env.Runtime.FireAndForgetService.PrepareQueryWithParameters(compiled);
                while (true) {
                    var id = stageInput.Pop();
                    Process(q, id);
                    if (id == -1) {
                        break;
                    }
                }
            }

            private void Process(
                EPFireAndForgetPreparedQueryParameterized q,
                int id)
            {
                q.SetObject(1, id);
                var result = env.Runtime.FireAndForgetService.ExecuteQuery(q);
                Assert.AreEqual(1, result.Array.Length, "failed for id " + id);
                Assert.AreEqual(id, result.Array[0].Get("p0"));
                stageOutput.Push(id);
                numberOfOperations++;
            }
        }

        public class UpdateRunnable : BaseRunnable
        {
            private readonly IBlockingQueue<int> stageInput;
            private readonly IBlockingQueue<int> stageOutput;

            public UpdateRunnable(
                RegressionEnvironment env,
                RegressionPath path,
                IBlockingQueue<int> stageInput,
                IBlockingQueue<int> stageOutput) : base(env, path, "Update")
            {
                this.stageInput = stageInput;
                this.stageOutput = stageOutput;
            }

            public override void RunWork()
            {
                var epl = "update MyTable set p0 = 99999999 where key = cast(?, int)";
                var compiled = env.CompileFAF(epl, path);
                var q = env.Runtime.FireAndForgetService.PrepareQueryWithParameters(compiled);
                while (true) {
                    var id = stageInput.Pop();
                    Process(q, id);
                    if (id == -1) {
                        break;
                    }
                }
            }

            private void Process(
                EPFireAndForgetPreparedQueryParameterized q,
                int id)
            {
                q.SetObject(1, id);
                env.Runtime.FireAndForgetService.ExecuteQuery(q);
                stageOutput.Push(id);
                numberOfOperations++;
            }
        }

        public class DeleteRunnable : BaseRunnable
        {
            private readonly IBlockingQueue<int> stageInput;
            private readonly IBlockingQueue<int> stageOutput;

            public DeleteRunnable(
                RegressionEnvironment env,
                RegressionPath path,
                IBlockingQueue<int> stageInput,
                IBlockingQueue<int> stageOutput) : base(env, path, "Delete")
            {
                this.stageInput = stageInput;
                this.stageOutput = stageOutput;
            }

            public override void RunWork()
            {
                var epl = "delete from MyTable where key = cast(?, int)";
                var compiled = env.CompileFAF(epl, path);
                var q = env.Runtime.FireAndForgetService.PrepareQueryWithParameters(compiled);
                while (true) {
                    var id = stageInput.Pop();
                    Process(q, id);
                    if (id == -1) {
                        break;
                    }
                }
            }

            private void Process(
                EPFireAndForgetPreparedQueryParameterized q,
                int id)
            {
                q.SetObject(1, id);
                env.Runtime.FireAndForgetService.ExecuteQuery(q);
                stageOutput.Push(id);
                numberOfOperations++;
            }
        }
    }
} // end of namespace