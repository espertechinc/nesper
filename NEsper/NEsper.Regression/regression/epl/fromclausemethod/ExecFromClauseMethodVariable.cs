///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.collections;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;
using com.espertech.esper.util;

namespace com.espertech.esper.regression.epl.fromclausemethod
{
    public class ExecFromClauseMethodVariable : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.AddMethodRef(typeof(MyStaticService), new ConfigurationMethodRef());
            configuration.AddImport(typeof(MyStaticService));
    
            configuration.EngineDefaults.Logging.IsEnableQueryPlan = true;
            configuration.AddEventType<SupportBean>();
            configuration.AddEventType<SupportBean_S0>();
            configuration.AddEventType(typeof(SupportBean_S1));
            configuration.AddEventType(typeof(SupportBean_S2));
        }
    
        public override void Run(EPServiceProvider epService) {
    
            epService.EPAdministrator.Configuration.AddVariable("MyConstantServiceVariable", typeof(MyConstantServiceVariable), new MyConstantServiceVariable());
            RunAssertionConstantVariable(epService);
    
            epService.EPAdministrator.Configuration.AddVariable("MyNonConstantServiceVariable", typeof(MyNonConstantServiceVariable), new MyNonConstantServiceVariable("postfix"));
            TryAssertionNonConstantVariable(epService, false);
            TryAssertionNonConstantVariable(epService, true);
    
            RunAssertionContextVariable(epService);
    
            RunAssertionVariableMapAndOA(epService);
    
            // invalid footprint
            SupportMessageAssertUtil.TryInvalid(epService, "select * from method:MyConstantServiceVariable.FetchABean() as h0",
                    "Error starting statement: Method footprint does not match the number or type of expression parameters, expecting no parameters in method: Could not find enumeration method, date-time method or instance method named 'FetchABean' in class 'com.espertech.esper.regression.epl.fromclausemethod.ExecFromClauseMethodVariable+MyConstantServiceVariable' taking no parameters (nearest match found was 'FetchABean' taking type(s) 'System.Int32') [");
    
            // null variable value and metadata is instance method
            epService.EPAdministrator.Configuration.AddVariable("MyNullMap", typeof(MyMethodHandlerMap), null);
            SupportMessageAssertUtil.TryInvalid(epService, "select field1, field2 from method:MyNullMap.GetMapData",
                    "Error starting statement: Failed to access variable method invocation metadata: The variable value is null and the metadata method is an instance method");
    
            // variable with context and metadata is instance method
            epService.EPAdministrator.CreateEPL("create context BetweenStartAndEnd start SupportBean end SupportBean");
            epService.EPAdministrator.CreateEPL("context BetweenStartAndEnd create variable " + TypeHelper.MaskTypeName<MyMethodHandlerMap>() + " themap");
            SupportMessageAssertUtil.TryInvalid(epService, "context BetweenStartAndEnd select field1, field2 from method:themap.GetMapData",
                    "Error starting statement: Failed to access variable method invocation metadata: The metadata method is an instance method however the variable is contextual, please declare the metadata method as static or remove the context declaration for the variable");
        }
    
        private void RunAssertionVariableMapAndOA(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddVariable("MyMethodHandlerMap", typeof(MyMethodHandlerMap), new MyMethodHandlerMap("a", "b"));
            epService.EPAdministrator.Configuration.AddVariable("MyMethodHandlerOA", typeof(MyMethodHandlerOA), new MyMethodHandlerOA("a", "b"));
    
            foreach (string epl in new string[]{
                    "select field1, field2 from method:MyMethodHandlerMap.GetMapData",
                    "select field1, field2 from method:MyMethodHandlerOA.GetOAData"
            }) {
                EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
                EPAssertionUtil.AssertProps(stmt.First(), "field1,field2".Split(','), new object[]{"a", "b"});
            }
        }
    
        private void RunAssertionContextVariable(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddImport(typeof(MyNonConstantServiceVariableFactory));
            epService.EPAdministrator.Configuration.AddImport(typeof(MyNonConstantServiceVariable));
    
            epService.EPAdministrator.CreateEPL("create context MyContext " +
                    "initiated by SupportBean_S0 as c_s0 " +
                    "terminated by SupportBean_S1(id=c_s0.id)");
            epService.EPAdministrator.CreateEPL("context MyContext " +
                    "create variable MyNonConstantServiceVariable var = MyNonConstantServiceVariableFactory.Make()");
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("context MyContext " +
                    "select id as c0 from SupportBean(IntPrimitive=context.c_s0.id) as sb, " +
                    "method:var.FetchABean(IntPrimitive) as h0").Events += listener.Update;
            epService.EPAdministrator.CreateEPL("context MyContext on SupportBean_S2(id = context.c_s0.id) set var.Postfix=p20");
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            epService.EPRuntime.SendEvent(new SupportBean_S0(2));
    
            SendEventAssert(epService, listener, "E1", 1, "_1_context_postfix");
            SendEventAssert(epService, listener, "E2", 2, "_2_context_postfix");
    
            epService.EPRuntime.SendEvent(new SupportBean_S2(1, "a"));
            epService.EPRuntime.SendEvent(new SupportBean_S2(2, "b"));
    
            SendEventAssert(epService, listener, "E1", 1, "_1_a");
            SendEventAssert(epService, listener, "E2", 2, "_2_b");
    
            // invalid context
            SupportMessageAssertUtil.TryInvalid(epService, "select * from method:var.FetchABean(IntPrimitive) as h0",
                    "Error starting statement: Variable by name 'var' has been declared for context 'MyContext' and can only be used within the same context");
            epService.EPAdministrator.CreateEPL("create context ABC start @now end after 1 minute");
            SupportMessageAssertUtil.TryInvalid(epService, "context ABC select * from method:var.FetchABean(IntPrimitive) as h0",
                    "Error starting statement: Variable by name 'var' has been declared for context 'MyContext' and can only be used within the same context");
        }
    
        private void RunAssertionConstantVariable(EPServiceProvider epService) {
            string epl = "select id as c0 from SupportBean as sb, " +
                    "method:MyConstantServiceVariable.FetchABean(IntPrimitive) as h0";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendEventAssert(epService, listener, "E1", 10, "_10_");
            SendEventAssert(epService, listener, "E2", 20, "_20_");
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void TryAssertionNonConstantVariable(EPServiceProvider epService, bool soda) {
            string modifyEPL = "on SupportBean_S0 set MyNonConstantServiceVariable.Postfix=p00";
            SupportModelHelper.CreateByCompileOrParse(epService, soda, modifyEPL);
    
            string epl = "select id as c0 from SupportBean as sb, " +
                    "method:MyNonConstantServiceVariable.FetchABean(IntPrimitive) as h0";
            EPStatement stmt = SupportModelHelper.CreateByCompileOrParse(epService, soda, epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendEventAssert(epService, listener, "E1", 10, "_10_postfix");
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "newpostfix"));
            SendEventAssert(epService, listener, "E1", 20, "_20_newpostfix");
    
            // return to original value
            epService.EPRuntime.SendEvent(new SupportBean_S0(2, "postfix"));
            SendEventAssert(epService, listener, "E1", 30, "_30_postfix");
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void SendEventAssert(EPServiceProvider epService, SupportUpdateListener listener, string theString, int intPrimitive, string expected) {
            string[] fields = "c0".Split(',');
            epService.EPRuntime.SendEvent(new SupportBean(theString, intPrimitive));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{expected});
        }

        private class MyConstantServiceVariable
        {
            public SupportBean_A FetchABean(int intPrimitive)
            {
                return new SupportBean_A($"_{intPrimitive}_");
            }
        }

        [Serializable]
        public class MyNonConstantServiceVariable
        {
            private string _postfix;

            public string Postfix
            {
                get => _postfix;
                set => _postfix = value;
            }

            public MyNonConstantServiceVariable(string postfix)
            {
                _postfix = postfix;
            }

            public SupportBean_A FetchABean(int intPrimitive)
            {
                return new SupportBean_A($"_{intPrimitive}_{_postfix}");
            }
        }

        private static class MyStaticService
        {
            public static SupportBean_A FetchABean(int intPrimitive)
            {
                return new SupportBean_A($"_{intPrimitive}_");
            }
        }

        private static class MyNonConstantServiceVariableFactory
        {
            public static MyNonConstantServiceVariable Make()
            {
                return new MyNonConstantServiceVariable("context_postfix");
            }
        }

        public class MyMethodHandlerMap {
            private readonly string _field1;
            private readonly string _field2;
    
            public MyMethodHandlerMap(string field1, string field2) {
                _field1 = field1;
                _field2 = field2;
            }

            public IDictionary<string, object> GetMapDataMetadata()
            {
                var fields = new Dictionary<string, Object>();
                fields.Put("field1", typeof(string));
                fields.Put("field2", typeof(string));
                return fields;
            }

            public IDictionary<string, object>[] GetMapData()
            {
                var maps = new IDictionary<string, object>[1];
                var row = new Dictionary<string, Object>();
                maps[0] = row;
                row.Put("field1", _field1);
                row.Put("field2", _field2);
                return maps;
            }
        }
    
        public class MyMethodHandlerOA {
            private readonly string _field1;
            private readonly string _field2;
    
            public MyMethodHandlerOA(string field1, string field2) {
                _field1 = field1;
                _field2 = field2;
            }

            public static IDictionary<string, object> GetOADataMetadata()
            {
                var fields = new LinkedHashMap<string, Object>();
                fields.Put("field1", typeof(string));
                fields.Put("field2", typeof(string));
                return fields;
            }

            public object[][] GetOAData()
            {
                return new object[][] {
                    new object[] { _field1, _field2 }
                };
            }
        }
    }
} // end of namespace
