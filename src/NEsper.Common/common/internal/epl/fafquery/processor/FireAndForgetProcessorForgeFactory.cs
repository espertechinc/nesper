///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;

namespace com.espertech.esper.common.@internal.epl.fafquery.processor
{
    public class FireAndForgetProcessorForgeFactory
    {
        public static FireAndForgetProcessorForge ValidateResolveProcessor(
            StreamSpecCompiled streamSpec,
            StatementSpecCompiled statementSpec,
            StatementRawInfo raw,
            StatementCompileTimeServices services)
        {
            if (streamSpec is NamedWindowConsumerStreamSpec namedWindowConsumerStreamSpec) {
                return new FireAndForgetProcessorNamedWindowForge(namedWindowConsumerStreamSpec.NamedWindow);
            }

            if (streamSpec is DBStatementStreamSpec dbStatementStreamSpec) {
                return new FireAndForgetProcessorDBForge(dbStatementStreamSpec, statementSpec, raw, services);
            }

            var tableSpec = (TableQueryStreamSpec)streamSpec;
            return new FireAndForgetProcessorTableForge(tableSpec.Table);
        }
    }
} // end of namespace