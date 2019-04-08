///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.collection;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.compile;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.compile.stage2
{
    public class FilterSpecCompilerArgs
    {
        public readonly IDictionary<string, Pair<EventType, string>> arrayEventTypes;
        public readonly StatementCompileTimeServices compileTimeServices;
        public readonly ContextCompileTimeDescriptor contextDescriptor;
        public readonly StatementRawInfo statementRawInfo;
        public readonly StreamTypeService streamTypeService;

        public readonly IDictionary<string, Pair<EventType, string>> taggedEventTypes;

        public FilterSpecCompilerArgs(
            IDictionary<string, Pair<EventType, string>> taggedEventTypes,
            IDictionary<string, Pair<EventType, string>> arrayEventTypes, StreamTypeService streamTypeService,
            ContextCompileTimeDescriptor contextDescriptor, StatementRawInfo statementRawInfo,
            StatementCompileTimeServices compileTimeServices)
        {
            this.taggedEventTypes = taggedEventTypes;
            this.arrayEventTypes = arrayEventTypes;
            this.streamTypeService = streamTypeService;
            this.contextDescriptor = contextDescriptor;
            this.statementRawInfo = statementRawInfo;
            this.compileTimeServices = compileTimeServices;
        }
    }
} // end of namespace