///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;

using static com.espertech.esper.supportregression.util.SupportMessageAssertUtil;

using NUnit.Framework;

namespace com.espertech.esper.regression.expr.datetime
{
    public class ExecDTInvalid : RegressionExecution {
    
        public override void Configure(Configuration configuration) {
            configuration.AddEventType<SupportBean>();
            configuration.AddEventType("SupportBean_ST0_Container", typeof(SupportBean_ST0_Container));
            configuration.AddEventType("SupportDateTime", typeof(SupportDateTime));
            configuration.AddImport(typeof(SupportBean_ST0_Container));
            configuration.AddPlugInSingleRowFunction("makeTest", typeof(SupportBean_ST0_Container).Name, "makeTest");
        }
    
        public override void Run(EPServiceProvider epService) {
            string epl;
    
            // invalid incompatible params
            epl = "select Contained.Set('hour', 1) from SupportBean_ST0_Container";
            TryInvalid(epService, epl, "Error starting statement: Failed to validate select-clause expression 'contained.Set(\"hour\",1)': Date-time enumeration method 'set' requires either a Calendar, Date, long, LocalDateTime or ZonedDateTime value as input or events of an event type that declares a timestamp property but received collection of events of type '" + typeof(SupportBean_ST0).Name + "'");
    
            // invalid incompatible params
            epl = "select window(*).Set('hour', 1) from SupportBean#keepall";
            TryInvalid(epService, epl, "Error starting statement: Failed to validate select-clause expression 'window(*).Set(\"hour\",1)': Date-time enumeration method 'set' requires either a Calendar, Date, long, LocalDateTime or ZonedDateTime value as input or events of an event type that declares a timestamp property but received collection of events of type 'SupportBean'");
    
            // invalid incompatible params
            epl = "select Utildate.Set('invalid') from SupportDateTime";
            TryInvalid(epService, epl, "Error starting statement: Failed to validate select-clause expression 'utildate.Set(\"invalid\")': Parameters mismatch for date-time method 'set', the method requires an expression providing a string-type calendar field name and an expression providing an integer-type value");
    
            // invalid lambda parameter
            epl = "select Utildate.Set(x => true) from SupportDateTime";
            TryInvalid(epService, epl, "Error starting statement: Failed to validate select-clause expression 'utildate.Set()': Parameters mismatch for date-time method 'set', the method requires an expression providing a string-type calendar field name and an expression providing an integer-type value");
    
            // invalid no parameter
            epl = "select Utildate.Set() from SupportDateTime";
            TryInvalid(epService, epl, "Error starting statement: Failed to validate select-clause expression 'utildate.Set()': Parameters mismatch for date-time method 'set', the method requires an expression providing a string-type calendar field name and an expression providing an integer-type value");
    
            // invalid wrong parameter
            epl = "select Utildate.Set(1) from SupportDateTime";
            TryInvalid(epService, epl, "Error starting statement: Failed to validate select-clause expression 'utildate.Set(1)': Parameters mismatch for date-time method 'set', the method requires an expression providing a string-type calendar field name and an expression providing an integer-type value");
    
            // invalid wrong parameter
            epl = "select Utildate.Between('a', 'b') from SupportDateTime";
            TryInvalid(epService, epl, "Error starting statement: Failed to validate select-clause expression 'utildate.Between(\"a\",\"b\")': Error validating date-time method 'between', expected a long-typed, Date-typed or Calendar-typed result for expression parameter 0 but received java.lang.string");
    
            // invalid wrong parameter
            epl = "select Utildate.Between(utildate, utildate, 1, true) from SupportDateTime";
            TryInvalid(epService, epl, "Error starting statement: Failed to validate select-clause expression 'utildate.Between(utildate,utildate,...(42 chars)': Error validating date-time method 'between', expected a bool-type result for expression parameter 2 but received java.lang.int?");
    
            // mispatch parameter to input
            epl = "select Utildate.Format(java.time.format.DateTimeFormatter.ISO_ORDINAL_DATE) from SupportDateTime";
            TryInvalid(epService, epl, "Error starting statement: Failed to validate select-clause expression 'utildate.Format(ParseCaseSensitive(...(114 chars)': Date-time enumeration method 'format' invalid format, expected string-format or DateFormat but received java.time.format.DateTimeFormatter");
            epl = "select Zoneddate.Format(SimpleDateFormat.Instance) from SupportDateTime";
            TryInvalid(epService, epl, "Error starting statement: Failed to validate select-clause expression 'zoneddate.Format(SimpleDateFormat.g...(48 chars)': Date-time enumeration method 'format' invalid format, expected string-format or DateTimeFormatter but received java.text.SimpleDateFormat");
    
            // invalid date format null
            epl = "select Utildate.Format(null) from SupportDateTime";
            TryInvalid(epService, epl, "Error starting statement: Failed to validate select-clause expression 'utildate.Format(null)': Error validating date-time method 'format', expected any of [string, DateFormat, DateTimeFormatter]-type result for expression parameter 0 but received null");
        }
    }
} // end of namespace
