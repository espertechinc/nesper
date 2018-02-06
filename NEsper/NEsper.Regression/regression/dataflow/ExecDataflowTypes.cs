///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.client.dataflow;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.threading;
using com.espertech.esper.dataflow.util;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.graph;
using com.espertech.esper.supportregression.util;

namespace com.espertech.esper.regression.dataflow
{
    public class ExecDataflowTypes : RegressionExecution
    {
        public override void Run(EPServiceProvider epService)
        {
            RunAssertionBeanType(epService);
            RunAssertionMapType(epService);
        }

        private void RunAssertionBeanType(EPServiceProvider epService)
        {
            epService.EPAdministrator.Configuration.AddImport<SupportBean>();
            epService.EPAdministrator.CreateEPL("create schema SupportBean SupportBean");
            epService.EPAdministrator.CreateEPL(
                "create dataflow MyDataFlowOne " +
                "DefaultSupportSourceOp -> outstream<SupportBean> {}" +
                "MySupportBeanOutputOp(outstream) {}" +
                "SupportGenericOutputOpWPort(outstream) {}");

            var source = new DefaultSupportSourceOp(new object[] {new SupportBean("E1", 1)});
            var outputOne = new MySupportBeanOutputOp();
            var outputTwo = new SupportGenericOutputOpWPort();
            var options = new EPDataFlowInstantiationOptions().OperatorProvider(
                new DefaultSupportGraphOpProvider(source, outputOne, outputTwo));
            var dfOne = epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlowOne", options);
            dfOne.Run();

            EPAssertionUtil.AssertPropsPerRow(
                epService.Container,
                outputOne.GetAndReset().ToArray(), "TheString,IntPrimitive".Split(','), new[] {new object[] {"E1", 1}});
            var received = outputTwo.GetAndReset();
            EPAssertionUtil.AssertPropsPerRow(
                epService.Container,
                received.First.ToArray(), "TheString,IntPrimitive".Split(','), new[] {new object[] {"E1", 1}});
            EPAssertionUtil.AssertEqualsExactOrder(new[] {0}, received.Second.ToArray());

            epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionMapType(EPServiceProvider epService)
        {
            epService.EPAdministrator.Configuration.AddImport(typeof(SupportBean));
            epService.EPAdministrator.CreateEPL("create map schema MyMap (p0 string, p1 int)");
            epService.EPAdministrator.CreateEPL(
                "create dataflow MyDataFlowOne " +
                "DefaultSupportSourceOp -> outstream<MyMap> {}" +
                "MyMapOutputOp(outstream) {}" +
                "DefaultSupportCaptureOp(outstream) {}");

            var source = new DefaultSupportSourceOp(new object[] {MakeMap("E1", 1)});
            var outputOne = new MyMapOutputOp();
            var outputTwo = new DefaultSupportCaptureOp(SupportContainer.Instance.LockManager());
            var options = new EPDataFlowInstantiationOptions().OperatorProvider(
                new DefaultSupportGraphOpProvider(source, outputOne, outputTwo));
            var dfOne = epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlowOne", options);
            dfOne.Run();

            EPAssertionUtil.AssertPropsPerRow(
                outputOne.GetAndReset().ToArray(), "p0,p1".Split(','), new[] {new object[] {"E1", 1}});
            EPAssertionUtil.AssertPropsPerRow(
                epService.Container,
                outputTwo.GetAndReset()[0].ToArray(), "p0,p1".Split(','), new[] {new object[] {"E1", 1}});

            epService.EPAdministrator.DestroyAllStatements();
        }

        private IDictionary<string, object> MakeMap(string p0, int p1)
        {
            var map = new Dictionary<string, object>();
            map.Put("p0", p0);
            map.Put("p1", p1);
            return map;
        }

        public class MySupportBeanOutputOp
        {
            private readonly ILockable _lock = SupportContainer.Instance.LockManager().CreateDefaultLock();
            private List<SupportBean> _received = new List<SupportBean>();

            public void OnInput(SupportBean @event)
            {
                using (_lock.Acquire())
                {
                    _received.Add(@event);
                }
            }

            public IList<SupportBean> GetAndReset()
            {
                using (_lock.Acquire())
                {
                    var result = _received;
                    _received = new List<SupportBean>();
                    return result;
                }
            }
        }

        public class MyMapOutputOp
        {
            private readonly ILockable _lock = SupportContainer.Instance.LockManager().CreateDefaultLock();
            private List<IDictionary<string, object>> _received = new List<IDictionary<string, object>>();

            public void OnInput(IDictionary<string, object> @event)
            {
                using (_lock.Acquire())
                {
                    _received.Add(@event);
                }
            }

            public IList<IDictionary<string, object>> GetAndReset()
            {
                using (_lock.Acquire())
                {
                    var result = _received;
                    _received = new List<IDictionary<string, object>>();
                    return result;
                }
            }
        }
    }
} // end of namespace