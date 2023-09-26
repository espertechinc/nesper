///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.common.@internal.epl.variable.core;
using com.espertech.esper.common.@internal.@event.eventtyperepo;

namespace com.espertech.esper.common.@internal.epl.util
{
    public class EPCompilerPathableImpl : EPCompilerPathable
    {
        public EPCompilerPathableImpl(string optionalModuleName)
        {
            VariablePathRegistry = new PathRegistry<string, VariableMetaData>(PathRegistryObjectType.VARIABLE);
            EventTypePathRegistry = new PathRegistry<string, EventType>(PathRegistryObjectType.EVENTTYPE);
            ExprDeclaredPathRegistry = new PathRegistry<string, ExpressionDeclItem>(PathRegistryObjectType.EXPRDECL);
            NamedWindowPathRegistry = new PathRegistry<string, NamedWindowMetaData>(PathRegistryObjectType.NAMEDWINDOW);
            TablePathRegistry = new PathRegistry<string, TableMetaData>(PathRegistryObjectType.TABLE);
            ContextPathRegistry = new PathRegistry<string, ContextMetaData>(PathRegistryObjectType.CONTEXT);
            ScriptPathRegistry =
                new PathRegistry<NameAndParamNum, ExpressionScriptProvided>(PathRegistryObjectType.SCRIPT);
            ClassProvidedPathRegistry = new PathRegistry<string, ClassProvided>(PathRegistryObjectType.CLASSPROVIDED);
            EventTypePreconfigured = new EventTypeRepositoryImpl(true);
            VariablePreconfigured = new VariableRepositoryPreconfigured();
            OptionalModuleName = optionalModuleName;
        }

        public EPCompilerPathableImpl(
            PathRegistry<string, VariableMetaData> variablePathRegistry,
            PathRegistry<string, EventType> eventTypePathRegistry,
            PathRegistry<string, ExpressionDeclItem> exprDeclaredPathRegistry,
            PathRegistry<string, NamedWindowMetaData> namedWindowPathRegistry,
            PathRegistry<string, TableMetaData> tablePathRegistry,
            PathRegistry<string, ContextMetaData> contextPathRegistry,
            PathRegistry<NameAndParamNum, ExpressionScriptProvided> scriptPathRegistry,
            PathRegistry<string, ClassProvided> classProvidedPathRegistry,
            EventTypeRepositoryImpl eventTypePreconfigured,
            VariableRepositoryPreconfigured variablePreconfigured)
        {
            VariablePathRegistry = variablePathRegistry;
            EventTypePathRegistry = eventTypePathRegistry;
            ExprDeclaredPathRegistry = exprDeclaredPathRegistry;
            NamedWindowPathRegistry = namedWindowPathRegistry;
            TablePathRegistry = tablePathRegistry;
            ContextPathRegistry = contextPathRegistry;
            ScriptPathRegistry = scriptPathRegistry;
            ClassProvidedPathRegistry = classProvidedPathRegistry;
            EventTypePreconfigured = eventTypePreconfigured;
            VariablePreconfigured = variablePreconfigured;
            OptionalModuleName = null;
        }

        public PathRegistry<string, VariableMetaData> VariablePathRegistry { get; }

        public PathRegistry<string, EventType> EventTypePathRegistry { get; }

        public PathRegistry<string, ExpressionDeclItem> ExprDeclaredPathRegistry { get; }

        public PathRegistry<string, NamedWindowMetaData> NamedWindowPathRegistry { get; }

        public PathRegistry<string, TableMetaData> TablePathRegistry { get; }

        public PathRegistry<string, ContextMetaData> ContextPathRegistry { get; }

        public PathRegistry<NameAndParamNum, ExpressionScriptProvided> ScriptPathRegistry { get; }

        public PathRegistry<string, ClassProvided> ClassProvidedPathRegistry { get; }

        public EventTypeRepositoryImpl EventTypePreconfigured { get; }

        public VariableRepositoryPreconfigured VariablePreconfigured { get; }

        public string OptionalModuleName { get; }
    }
} // end of namespace