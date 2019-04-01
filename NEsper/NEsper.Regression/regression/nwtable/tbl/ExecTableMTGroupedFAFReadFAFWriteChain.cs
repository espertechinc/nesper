///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Threading;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable.tbl
{
    public class ExecTableMTGroupedFAFReadFAFWriteChain : RegressionExecution
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        /// <summary>
        /// Tests fire-and-forget lock cleanup:
        /// create table MyTable(key int primary key, p0 int)   (5 props)
        /// <para>
        /// The following threads are in a chain communicating by queue holding key values:
        /// - Insert: populates MyTable={key=N, p0=N}, last row indicated by -1
        /// - Select-Table-Access: select MyTable[N].p0 from SupportBean
        /// </para>
        /// </summary>
        public override void Run(EPServiceProvider epService) {
            TryMT(epService, 1000);
        }
    
        private void TryMT(EPServiceProvider epService, int numInserted)
        {
            var epl = "create table MyTable (key int primary key, p0 int);";
            epService.EPAdministrator.DeploymentAdmin.ParseDeploy(epl);
    
            var runnables = new List<BaseRunnable>();
    
            var insertOutQ = new LinkedBlockingQueue<int>();
            var insert = new InsertRunnable(epService, numInserted, insertOutQ);
            runnables.Add(insert);
    
            var selectOutQ = new LinkedBlockingQueue<int>();
            var select = new SelectRunnable(epService, insertOutQ, selectOutQ);
            runnables.Add(select);
    
            var updateOutQ = new LinkedBlockingQueue<int>();
            var update = new UpdateRunnable(epService, selectOutQ, updateOutQ);
            runnables.Add(update);
    
            var deleteOutQ = new LinkedBlockingQueue<int>();
            var delete = new DeleteRunnable(epService, updateOutQ, deleteOutQ);
            runnables.Add(delete);
    
            // start
            var threads = new Thread[runnables.Count];
            for (var i = 0; i < runnables.Count; i++) {
                threads[i] = new Thread(runnables[i].Run);
                threads[i].Start();
            }
    
            // join
            foreach (var t in threads) {
                t.Join();
            }
    
            // assert
            foreach (var runnable in runnables) {
                Assert.IsNull(runnable.Exception);
                Assert.AreEqual(numInserted + 1, runnable.NumberOfOperations, "failed for " + runnable);    // account for -1 indicator
            }
        }
    
        public abstract class BaseRunnable
        {
            protected readonly EPServiceProvider epService;
            protected readonly string workName;
            protected int numberOfOperations;
            private Exception _exception;
    
            protected BaseRunnable(EPServiceProvider epService, string workName) {
                this.epService = epService;
                this.workName = workName;
            }
    
            public abstract void RunWork() ;
    
            public void Run() {
                Log.Info("Starting " + workName);
                try {
                    RunWork();
                } catch (Exception ex) {
                    Log.Error("Exception encountered: " + ex.Message, ex);
                    _exception = ex;
                }
                Log.Info("Completed " + workName);
            }

            public Exception Exception => _exception;

            public int NumberOfOperations => numberOfOperations;
        }
    
        public class InsertRunnable : BaseRunnable
        {
            private readonly int _numInserted;
            private readonly IBlockingQueue<int> _stageOutput;
    
            public InsertRunnable(EPServiceProvider epService, int numInserted, IBlockingQueue<int> stageOutput)
                : base(epService, "Insert")
            {
                this._numInserted = numInserted;
                this._stageOutput = stageOutput;
            }
    
            public override void RunWork() {
                var q = epService.EPRuntime.PrepareQueryWithParameters("insert into MyTable (key, p0) values (?, ?)");
                for (var i = 0; i < _numInserted; i++) {
                    Process(q, i);
                }
                Process(q, -1);
            }
    
            private void Process(EPOnDemandPreparedQueryParameterized q, int id) {
                q.SetObject(1, id);
                q.SetObject(2, id);
                epService.EPRuntime.ExecuteQuery(q);
                _stageOutput.Push(id);
                numberOfOperations++;
            }
        }
    
        public class SelectRunnable : BaseRunnable
        {
            private readonly IBlockingQueue<int> _stageInput;
            private readonly IBlockingQueue<int> _stageOutput;
    
            public SelectRunnable(EPServiceProvider epService, IBlockingQueue<int> stageInput, IBlockingQueue<int> stageOutput)
                : base(epService, "Select")
            {
                this._stageInput = stageInput;
                this._stageOutput = stageOutput;
            }
    
            public override void RunWork() {
                var epl = "select p0 from MyTable where key = ?";
                var q = epService.EPRuntime.PrepareQueryWithParameters(epl);
                while (true) {
                    var id = _stageInput.Pop();
                    Process(q, id);
                    if (id == -1) {
                        break;
                    }
                }
            }
    
            private void Process(EPOnDemandPreparedQueryParameterized q, int id) {
                q.SetObject(1, id);
                var result = epService.EPRuntime.ExecuteQuery(q);
                Assert.AreEqual(1, result.Array.Length, "failed for id " + id);
                Assert.AreEqual(id, result.Array[0].Get("p0"));
                _stageOutput.Push(id);
                numberOfOperations++;
            }
        }
    
        public class UpdateRunnable : BaseRunnable
        {
            private readonly IBlockingQueue<int> _stageInput;
            private readonly IBlockingQueue<int> _stageOutput;
    
            public UpdateRunnable(EPServiceProvider epService, IBlockingQueue<int> stageInput, IBlockingQueue<int> stageOutput)
                : base(epService, "Update")
            {
                this._stageInput = stageInput;
                this._stageOutput = stageOutput;
            }
    
            public override void RunWork() {
                var epl = "update MyTable set p0 = 99999999 where key = ?";
                var q = epService.EPRuntime.PrepareQueryWithParameters(epl);
                while (true) {
                    var id = _stageInput.Pop();
                    Process(q, id);
                    if (id == -1) {
                        break;
                    }
                }
            }
    
            private void Process(EPOnDemandPreparedQueryParameterized q, int id) {
                q.SetObject(1, id);
                epService.EPRuntime.ExecuteQuery(q);
                _stageOutput.Push(id);
                numberOfOperations++;
            }
        }
    
        public class DeleteRunnable : BaseRunnable{
            private readonly IBlockingQueue<int> _stageInput;
            private readonly IBlockingQueue<int> _stageOutput;
    
            public DeleteRunnable(EPServiceProvider epService, IBlockingQueue<int> stageInput, IBlockingQueue<int> stageOutput)
            : base(epService, "Delete")
            {
                this._stageInput = stageInput;
                this._stageOutput = stageOutput;
            }
    
            public override void RunWork() {
                var epl = "delete from MyTable where key = ?";
                var q = epService.EPRuntime.PrepareQueryWithParameters(epl);
                while (true) {
                    var id = _stageInput.Pop();
                    Process(q, id);
                    if (id == -1) {
                        break;
                    }
                }
            }
    
            private void Process(EPOnDemandPreparedQueryParameterized q, int id) {
                q.SetObject(1, id);
                epService.EPRuntime.ExecuteQuery(q);
                _stageOutput.Push(id);
                numberOfOperations++;
            }
        }
    }
} // end of namespace
