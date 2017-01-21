///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable
{
    [TestFixture]
    public class TestTableMTGroupedFAFReadFAFWriteChain 
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    
        private EPServiceProvider _epService;
    
        [SetUp]
        public void SetUp()
        {
            var config = SupportConfigFactory.GetConfiguration();
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
        }
    
        /// <summary>
        /// Tests fire-and-forget lock cleanup:
        /// create table MyTable(key int primary key, p0 int)   (5 props)
        /// The following threads are in a chain communicating by queue holding key values:
        /// - Insert: populates MyTable={key=Count, p0=Count}, last row indicated by -1
        /// - Select-Table-Access: select MyTable[Count].p0 from SupportBean
        /// </summary>
        [Test]
        public void TestMT() 
        {
            TryMT(1000);
        }
    
        private void TryMT(int numInserted)
        {
            var epl = "create table MyTable (key int primary key, p0 int);";
            _epService.EPAdministrator.DeploymentAdmin.ParseDeploy(epl);
    
            var runnables = new List<BaseRunnable>();

            var insertOutQ = new LinkedBlockingQueue<int>();
            var insert = new InsertRunnable(_epService, numInserted, insertOutQ);
            runnables.Add(insert);

            var selectOutQ = new LinkedBlockingQueue<int>();
            var select = new SelectRunnable(_epService, insertOutQ, selectOutQ);
            runnables.Add(select);

            var updateOutQ = new LinkedBlockingQueue<int>();
            var update = new UpdateRunnable(_epService, selectOutQ, updateOutQ);
            runnables.Add(update);

            var deleteOutQ = new LinkedBlockingQueue<int>();
            var delete = new DeleteRunnable(_epService, updateOutQ, deleteOutQ);
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
            protected readonly EPServiceProvider EPService;
            protected readonly string WorkName;
            internal int NumberOfOperations;
            internal Exception Exception;
    
            protected BaseRunnable(EPServiceProvider epService, string workName)
            {
                EPService = epService;
                WorkName = workName;
            }

            public abstract void RunWork();
    
            public virtual void Run() {
                Log.Info("Starting " + WorkName);
                try {
                    RunWork();
                }
                catch (Exception ex) {
                    Log.Error("Exception encountered: " + ex.Message, ex);
                    Exception = ex;
                }
                Log.Info("Completed " + WorkName);
            }
        }
    
        public class InsertRunnable : BaseRunnable
        {
            private readonly int _numInserted;
            private readonly IBlockingQueue<int> _stageOutput;

            public InsertRunnable(EPServiceProvider epService, int numInserted, IBlockingQueue<int> stageOutput)
                : base(epService, "Insert")
            {
                _numInserted = numInserted;
                _stageOutput = stageOutput;
            }
    
            public override void RunWork()
            {
                var q = EPService.EPRuntime.PrepareQueryWithParameters("insert into MyTable (key, p0) values (?, ?)");
                for (var i = 0; i < _numInserted; i++) {
                    Process(q, i);
                }
                Process(q, -1);
            }
    
            private void Process(EPOnDemandPreparedQueryParameterized q, int id) {
                q.SetObject(1, id);
                q.SetObject(2, id);
                EPService.EPRuntime.ExecuteQuery(q);
                _stageOutput.Push(id);
                NumberOfOperations++;
            }
        }
    
        public class SelectRunnable : BaseRunnable
        {
            private readonly IBlockingQueue<int> _stageInput;
            private readonly IBlockingQueue<int> _stageOutput;

            public SelectRunnable(EPServiceProvider epService, IBlockingQueue<int> stageInput, IBlockingQueue<int> stageOutput)
                : base(epService, "Select")
            {
                _stageInput = stageInput;
                _stageOutput = stageOutput;
            }
    
            public override void RunWork()
            {
                var epl = "select p0 from MyTable where key = ?";
                var q = EPService.EPRuntime.PrepareQueryWithParameters(epl);
                while (true) {
                    int id = _stageInput.Pop();
                    Process(q, id);
                    if (id == -1) {
                        break;
                    }
                }
            }
    
            private void Process(EPOnDemandPreparedQueryParameterized q, int id)
            {
                q.SetObject(1, id);
                var result = EPService.EPRuntime.ExecuteQuery(q);
                Assert.AreEqual(1, result.Array.Length, "failed for id " + id);
                Assert.AreEqual(id, result.Array[0].Get("p0"));
                _stageOutput.Push(id);
                NumberOfOperations++;
            }
        }
    
        public class UpdateRunnable : BaseRunnable
        {
            private readonly IBlockingQueue<int> _stageInput;
            private readonly IBlockingQueue<int> _stageOutput;

            public UpdateRunnable(EPServiceProvider epService, IBlockingQueue<int> stageInput, IBlockingQueue<int> stageOutput)
                : base(epService, "Update")
            {
                _stageInput = stageInput;
                _stageOutput = stageOutput;
            }
    
            public override void RunWork()
            {
                var epl = "update MyTable set p0 = 99999999 where key = ?";
                var q = EPService.EPRuntime.PrepareQueryWithParameters(epl);
                while (true) {
                    int id = _stageInput.Pop();
                    Process(q, id);
                    if (id == -1) {
                        break;
                    }
                }
            }
    
            private void Process(EPOnDemandPreparedQueryParameterized q, int id)
            {
                q.SetObject(1, id);
                EPService.EPRuntime.ExecuteQuery(q);
                _stageOutput.Push(id);
                NumberOfOperations++;
            }
        }
    
        public class DeleteRunnable : BaseRunnable
        {
            private readonly IBlockingQueue<int> _stageInput;
            private readonly IBlockingQueue<int> _stageOutput;

            public DeleteRunnable(EPServiceProvider epService, IBlockingQueue<int> stageInput, IBlockingQueue<int> stageOutput)
                : base(epService, "Delete")
            {
                _stageInput = stageInput;
                _stageOutput = stageOutput;
            }
    
            public override void RunWork()
            {
                var epl = "delete from MyTable where key = ?";
                var q = EPService.EPRuntime.PrepareQueryWithParameters(epl);
                while (true) {
                    var id = _stageInput.Pop();
                    Process(q, id);
                    if (id == -1) {
                        break;
                    }
                }
            }
    
            private void Process(EPOnDemandPreparedQueryParameterized q, int id)
            {
                q.SetObject(1, id);
                EPService.EPRuntime.ExecuteQuery(q);
                _stageOutput.Push(id);
                NumberOfOperations++;
            }
        }
    }
}
