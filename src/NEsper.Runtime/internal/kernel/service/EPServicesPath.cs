///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.context.compile;
using com.espertech.esper.common.@internal.epl.classprovided.core;
using com.espertech.esper.common.@internal.epl.namedwindow.path;
using com.espertech.esper.common.@internal.epl.script.core;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.epl.variable.compiletime;

namespace com.espertech.esper.runtime.@internal.kernel.service
{
    public interface EPServicesPath
    {
        PathRegistry<string, NamedWindowMetaData> NamedWindowPathRegistry { get; }
        PathRegistry<string, ContextMetaData> ContextPathRegistry { get; }
        PathRegistry<string, ExpressionDeclItem> ExprDeclaredPathRegistry { get; }
        PathRegistry<string, EventType> EventTypePathRegistry { get; }
        PathRegistry<NameAndParamNum, ExpressionScriptProvided> ScriptPathRegistry { get; }
        PathRegistry<string, TableMetaData> TablePathRegistry { get; }
        PathRegistry<string, VariableMetaData> VariablePathRegistry { get; }
        PathRegistry<string, ClassProvided> ClassProvidedPathRegistry { get; }
    }
} // end of namespace