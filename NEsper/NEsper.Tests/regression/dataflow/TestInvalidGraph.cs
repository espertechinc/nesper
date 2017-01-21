///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.dataflow.annotations;
using com.espertech.esper.dataflow.interfaces;
using com.espertech.esper.dataflow.util;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.dataflow
{
    [TestFixture]
    public class TestInvalidGraph
    {
        private EPServiceProvider _epService;
    
        [SetUp]
        public void SetUp() {
            _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            _epService.Initialize();
        }
    
        [Test]
        public void TestInvalidSyntax() {
            SupportDataFlowAssertionUtil.TryInvalidCreate(_epService, "create dataflow MyGraph MySource -> select",
                    "Incorrect syntax near 'select' (a reserved keyword) at line 1 column 36 [");
    
            SupportDataFlowAssertionUtil.TryInvalidCreate(_epService, "create dataflow MyGraph MySource -> myout",
                    "Incorrect syntax near end-of-input expecting a left curly bracket '{' but found end-of-input at line 1 column 41 [");
        }
    
        [Test]
        public void TestWithInvalidGraph() {
            _epService.EPAdministrator.Configuration.AddImport(typeof(DefaultSupportSourceOp));
            _epService.EPAdministrator.Configuration.AddImport(typeof(DefaultSupportCaptureOp<object>));
            _epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBean_A));
            _epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBean_B));
            _epService.EPAdministrator.Configuration.AddImport(typeof(MyInvalidOpFactory));
            _epService.EPAdministrator.Configuration.AddImport(typeof(MyTestOp));
            _epService.EPAdministrator.Configuration.AddImport(typeof(MySBInputOp));
            String epl;
    
            // type not found
            SupportDataFlowAssertionUtil.TryInvalidInstantiate(_epService, "MyGraph", "create dataflow MyGraph DefaultSupportSourceOp -> outstream<ABC> {}",
                    "Failed to instantiate data flow 'MyGraph': Failed to find event type 'ABC'");
    
            // invalid schema (need not test all variants, same as create-schema)
            SupportDataFlowAssertionUtil.TryInvalidInstantiate(_epService, "MyGraph", "create dataflow MyGraph create schema DUMMY com.mycompany.DUMMY, " +
                    "DefaultSupportSourceOp -> outstream<?> {}",
                    "Failed to instantiate data flow 'MyGraph': Failed to resolve class 'com.mycompany.DUMMY': Could not load class by name 'com.mycompany.DUMMY', please check imports");
            
            // can't find op
            SupportDataFlowAssertionUtil.TryInvalidInstantiate(_epService, "MyGraph", "create dataflow MyGraph DummyOp {}",
                    "Failed to instantiate data flow 'MyGraph': Failed to resolve operator 'DummyOp': Could not load class by name 'DummyOp', please check imports");
    
            // op is some other class
            SupportDataFlowAssertionUtil.TryInvalidInstantiate(_epService, "MyGraph", "create dataflow MyGraph Random {}",
                    "Failed to instantiate data flow 'MyGraph': Failed to resolve operator 'Random', operator class System.Random does not declare the DataFlowOperatorAttribute annotation or implement the DataFlowSourceOperator interface");
    
            // input stream not found
            SupportDataFlowAssertionUtil.TryInvalidInstantiate(_epService, "MyGraph", "create dataflow MyGraph DefaultSupportCaptureOp(nostream) {}",
                    "Failed to instantiate data flow 'MyGraph': Input stream 'nostream' consumed by operator 'DefaultSupportCaptureOp' could not be found");
    
            // failed op factory
            SupportDataFlowAssertionUtil.TryInvalidInstantiate(_epService, "MyGraph", "create dataflow MyGraph MyInvalidOp {}",
                    "Failed to instantiate data flow 'MyGraph': Failed to obtain operator 'MyInvalidOp', encountered an exception raised by factory class MyInvalidOpFactory: Failed-Here");
    
            // inject properties: property not found
            SupportDataFlowAssertionUtil.TryInvalidInstantiate(_epService, "MyGraph", "create dataflow MyGraph DefaultSupportCaptureOp {dummy: 1}",
                    "Failed to instantiate data flow 'MyGraph': Failed to find writable property 'dummy' for class");
    
            // inject properties: property invalid type
            SupportDataFlowAssertionUtil.TryInvalidInstantiate(_epService, "MyGraph", "create dataflow MyGraph MyTestOp {TheString: 1}",
                    "Failed to instantiate data flow 'MyGraph': Property 'TheString' of class com.espertech.esper.regression.dataflow.TestInvalidGraph+MyTestOp expects an System.String but receives a value of type " + typeof(int).FullName);
    
            // two incompatible input streams: different types
            epl = "create dataflow MyGraph " +
                    "DefaultSupportSourceOp -> out1<SupportBean_A> {}\n" +
                    "DefaultSupportSourceOp -> out2<SupportBean_B> {}\n" +
                    "MyTestOp((out1, out2) as ABC) {}";
            SupportDataFlowAssertionUtil.TryInvalidInstantiate(_epService, "MyGraph", epl,
                    "Failed to instantiate data flow 'MyGraph': For operator 'MyTestOp' stream 'out1' typed 'SupportBean_A' is not the same type as stream 'out2' typed 'SupportBean_B'");
    
            // two incompatible input streams: one is wildcard
            epl = "create dataflow MyGraph " +
                    "DefaultSupportSourceOp -> out1<?> {}\n" +
                    "DefaultSupportSourceOp -> out2<SupportBean_B> {}\n" +
                    "MyTestOp((out1, out2) as ABC) {}";
            SupportDataFlowAssertionUtil.TryInvalidInstantiate(_epService, "MyGraph", epl,
                    "Failed to instantiate data flow 'MyGraph': For operator 'MyTestOp' streams 'out1' and 'out2' have differing wildcard type information");
    
            // two incompatible input streams: underlying versus eventbean
            epl = "create dataflow MyGraph " +
                    "DefaultSupportSourceOp -> out1<Eventbean<SupportBean_B>> {}\n" +
                    "DefaultSupportSourceOp -> out2<SupportBean_B> {}\n" +
                    "MyTestOp((out1, out2) as ABC) {}";
            SupportDataFlowAssertionUtil.TryInvalidInstantiate(_epService, "MyGraph", epl,
                    "Failed to instantiate data flow 'MyGraph': For operator 'MyTestOp' streams 'out1' and 'out2' have differing underlying information");
    
            // output stream multiple type parameters
            epl = "create dataflow MyGraph " +
                    "DefaultSupportSourceOp -> out1<SupportBean_A, SupportBean_B> {}";
            SupportDataFlowAssertionUtil.TryInvalidCreate(_epService, epl,
                    "Error starting statement: Failed to validate operator 'DefaultSupportSourceOp': Multiple output types for a single stream 'out1' are not supported [");
    
            // same output stream declared twice
            epl = "create dataflow MyGraph " +
                    "DefaultSupportSourceOp -> out1<SupportBean_A>, out1<SupportBean_B> {}";
            SupportDataFlowAssertionUtil.TryInvalidInstantiate(_epService, "MyGraph", epl,
                    "Failed to instantiate data flow 'MyGraph': For operator 'DefaultSupportSourceOp' stream 'out1' typed 'SupportBean_A' is not the same type as stream 'out1' typed 'SupportBean_B'");
    
            epl = "create dataflow MyGraph " +
                    "DefaultSupportSourceOp -> out1<Eventbean<SupportBean_A>>, out1<Eventbean<SupportBean_B>> {}";
            SupportDataFlowAssertionUtil.TryInvalidInstantiate(_epService, "MyGraph", epl,
                    "Failed to instantiate data flow 'MyGraph': For operator 'DefaultSupportSourceOp' stream 'out1' typed 'SupportBean_A' is not the same type as stream 'out1' typed 'SupportBean_B'");
    
            // two incompatible output streams: underlying versus eventbean
            epl = "create dataflow MyGraph " +
                    "DefaultSupportSourceOp -> out1<SupportBean_A> {}\n" +
                    "DefaultSupportSourceOp -> out1<SupportBean_B> {}\n" +
                    "MyTestOp(out1) {}";
            SupportDataFlowAssertionUtil.TryInvalidInstantiate(_epService, "MyGraph", epl,
                    "Failed to instantiate data flow 'MyGraph': For operator 'MyTestOp' stream 'out1' typed 'SupportBean_A' is not the same type as stream 'out1' typed 'SupportBean_B'");
    
            // incompatible on-input method
            epl = "create dataflow MyGraph " +
                    "DefaultSupportSourceOp -> out1<SupportBean_A> {}\n" +
                    "MySBInputOp(out1) {}";
            SupportDataFlowAssertionUtil.TryInvalidInstantiate(_epService, "MyGraph", epl,
                    "Failed to instantiate data flow 'MyGraph': Failed to find OnInput method on for operator 'MySBInputOp#1(out1)' class com.espertech.esper.regression.dataflow.TestInvalidGraph+MySBInputOp, expected an OnInput method that takes any of {Object, Object[");
    
            // same schema defined twice
            epl = "create dataflow MyGraph " +
                    "create schema ABC (c0 string), create schema ABC (c1 string), " +
                    "DefaultSupportSourceOp -> out1<SupportBean_A> {}";
            SupportDataFlowAssertionUtil.TryInvalidCreate(_epService, epl,
                    "Error starting statement: Schema name 'ABC' is declared more then once [");
        }
    
        public class MyInvalidOpFactory : DataFlowOperatorFactory {
            public Object Create() {
                throw new ApplicationException("Failed-Here");
            }
        }

        [DataFlowOperator]
        public class MyTestOp
        {
            [DataFlowOpParameter] private String theString;

            public string TheString
            {
                get { return theString; }
                set { theString = value; }
            }

            public void OnInput(Object o) { }
        }

        [DataFlowOperator]
        public class MySBInputOp
        {
            public void OnInput(SupportBean_B b) { }
        }
    }
}
