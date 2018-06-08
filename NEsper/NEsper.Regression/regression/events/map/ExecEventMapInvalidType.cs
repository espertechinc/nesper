///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.events.map
{
    public class ExecEventMapInvalidType : RegressionExecution
    {
        public override void Run(EPServiceProvider epService) {
#if NOT_VALID_TEST
            var invalid = ExecEventMap.MakeMap(new object[][] {
                new object[] {
                    new SupportBean(), null
                }
            });
            TryInvalid(epService, invalid, typeof(SupportBean).FullName + " cannot be cast to System.String");
    
            invalid = ExecEventMap.MakeMap(new object[][]{new object[] {"abc", new SupportBean()}});
            TryInvalid(epService, invalid, "Nestable type configuration encountered an unexpected property type of 'SupportBean' for property 'abc', expected Type or DataMap or the name of a previously-declared Map or ObjectArray type");
#endif
        }
    
        private void TryInvalid(EPServiceProvider epService, IDictionary<string, Object> config, string message) {
            try {
                epService.EPAdministrator.Configuration.AddEventType("NestedMap", config);
                Assert.Fail();
            } catch (Exception ex) {
                // Comment-me-in: Log.Error(ex.Message, ex);
                Assert.IsTrue(ex.Message.Contains(message), "expected '" + message + "' but received '" + ex.Message);
            }
        }
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
} // end of namespace
