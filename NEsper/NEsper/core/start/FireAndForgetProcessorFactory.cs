///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.named;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.table.mgmt;

namespace com.espertech.esper.core.start
{
    public class FireAndForgetProcessorFactory
    {
        public static FireAndForgetProcessor ValidateResolveProcessor(StreamSpecCompiled streamSpec, EPServicesContext services)
        {
            // resolve processor
            string processorName;
            if (streamSpec is NamedWindowConsumerStreamSpec) {
                var namedSpec = (NamedWindowConsumerStreamSpec) streamSpec;
                processorName = namedSpec.WindowName;
            }
            else {
                var tableSpec = (TableQueryStreamSpec) streamSpec;
                processorName = tableSpec.TableName;
            }
    
            // get processor instance
            var tableMetadata = services.TableService.GetTableMetadata(processorName);
            if (tableMetadata != null) {
                return new FireAndForgetProcessorTable(services.TableService, tableMetadata);
            }
            else {
                var nwprocessor = services.NamedWindowMgmtService.GetProcessor(processorName);
                if (nwprocessor == null) {
                    throw new ExprValidationException("A table or named window by name '" + processorName + "' does not exist");
                }
                return new FireAndForgetProcessorNamedWindow(nwprocessor);
            }
        }
    }
}
