///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;

using static com.espertech.esper.supportregression.util.SupportMessageAssertUtil;

namespace com.espertech.esper.regression.expr.datetime
{
    public class ExecDTInvalid : RegressionExecution {
    
        public override void Configure(Configuration configuration) {
            configuration.AddEventType<SupportBean>();
            configuration.AddEventType("SupportBean_ST0_Container", typeof(SupportBean_ST0_Container));
            configuration.AddEventType("SupportDateTime", typeof(SupportDateTime));
            configuration.AddImport(typeof(SupportBean_ST0_Container));
            configuration.AddPlugInSingleRowFunction("makeTest", typeof(SupportBean_ST0_Container), "MakeTest");
        }
    
        public override void Run(EPServiceProvider epService) {
            string epl;

#if false
            // invalid incompatible params
            epl = "select Contained.set('hour', 1) from SupportBean_ST0_Container";
            TryInvalid(epService, epl, "Error starting statement: Failed to validate select-clause expression 'Contained.set(\"hour\",1)': Date-time enumeration method 'set' requires either a DateTime, DateTimeEx or long value as input or events of an event type that declares a timestamp property but received collection of events of type '" + typeof(SupportBean_ST0).FullName + "'");
    
            // invalid incompatible params
            epl = "select window(*).set('hour', 1) from SupportBean#keepall";
            TryInvalid(epService, epl, "Error starting statement: Failed to validate select-clause expression 'window(*).set(\"hour\",1)': Date-time enumeration method 'set' requires either a DateTime, DateTimeEx or long value as input or events of an event type that declares a timestamp property but received collection of events of type 'SupportBean'");
    
            // invalid incompatible params
            epl = "select Utildate.set('invalid') from SupportDateTime";
            TryInvalid(epService, epl, "Error starting statement: Failed to validate select-clause expression 'Utildate.set(\"invalid\")': Parameters mismatch for date-time method 'set', the method requires an expression providing a string-type calendar field name and an expression providing an integer-type value");
    
            // invalid lambda parameter
            epl = "select Utildate.set(x => true) from SupportDateTime";
            TryInvalid(epService, epl, "Error starting statement: Failed to validate select-clause expression 'Utildate.set()': Parameters mismatch for date-time method 'set', the method requires an expression providing a string-type calendar field name and an expression providing an integer-type value");
    
            // invalid no parameter
            epl = "select Utildate.set() from SupportDateTime";
            TryInvalid(epService, epl, "Error starting statement: Failed to validate select-clause expression 'Utildate.set()': Parameters mismatch for date-time method 'set', the method requires an expression providing a string-type calendar field name and an expression providing an integer-type value");
    
            // invalid wrong parameter
            epl = "select Utildate.set(1) from SupportDateTime";
            TryInvalid(epService, epl, "Error starting statement: Failed to validate select-clause expression 'Utildate.set(1)': Parameters mismatch for date-time method 'set', the method requires an expression providing a string-type calendar field name and an expression providing an integer-type value");
    
            // invalid wrong parameter
            epl = "select Utildate.between('a', 'b') from SupportDateTime";
            TryInvalid(epService, epl, "Error starting statement: Failed to validate select-clause expression 'Utildate.between(\"a\",\"b\")': Error validating date-time method 'between', expected a long-typed, Date-typed or Calendar-typed result for expression parameter 0 but received System.String");
    
            // invalid wrong parameter
            epl = "select Utildate.between(Utildate, Utildate, 1, true) from SupportDateTime";
            TryInvalid(epService, epl, "Error starting statement: Failed to validate select-clause expression 'Utildate.between(Utildate,Utildate,...(42 chars)': Error validating date-time method 'between', expected a bool-type result for expression parameter 2 but received " + Name.Clean<int>() + "");
#endif

            // invalid date format null
            epl = "select Utildate.format(null) from SupportDateTime";
            TryInvalid(epService, epl, "Error starting statement: Failed to validate select-clause expression 'Utildate.format(null)': Error validating date-time method 'format', expected a System.String-type result for expression parameter 0 but received null");
        }
    }
} // end of namespace
