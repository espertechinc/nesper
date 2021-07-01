///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.compile;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.classprovided.core;
using com.espertech.esper.common.@internal.epl.namedwindow.path;
using com.espertech.esper.common.@internal.epl.script.core;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.epl.variable.compiletime;
using com.espertech.esper.common.@internal.@event.bean.service;
using com.espertech.esper.common.@internal.@event.path;

namespace com.espertech.esper.runtime.@internal.kernel.service
{
    public class DeployerModuleEPLObjects
    {
        public DeployerModuleEPLObjects(
            BeanEventTypeFactoryPrivate beanEventTypeFactory,
            IDictionary<string, EventType> moduleEventTypes,
            IDictionary<string, NamedWindowMetaData> moduleNamedWindows,
            IDictionary<string, TableMetaData> moduleTables,
            ISet<ModuleIndexMeta> moduleIndexes,
            IDictionary<string, ContextMetaData> moduleContexts,
            IDictionary<string, VariableMetaData> moduleVariables,
            IDictionary<string, ExpressionDeclItem> moduleExpressions,
            IDictionary<NameAndParamNum, ExpressionScriptProvided> moduleScripts,
            IDictionary<string, ClassProvided> moduleClasses,
            IList<EventTypeCollectedSerde> eventTypeSerdes,
            EventTypeResolverImpl eventTypeResolver)
        {
            BeanEventTypeFactory = beanEventTypeFactory;
            ModuleEventTypes = moduleEventTypes;
            ModuleNamedWindows = moduleNamedWindows;
            ModuleTables = moduleTables;
            ModuleIndexes = moduleIndexes;
            ModuleContexts = moduleContexts;
            ModuleVariables = moduleVariables;
            ModuleExpressions = moduleExpressions;
            ModuleScripts = moduleScripts;
            ModuleClasses = moduleClasses;
            EventTypeSerdes = eventTypeSerdes;
            EventTypeResolver = eventTypeResolver;
        }

        public BeanEventTypeFactoryPrivate BeanEventTypeFactory { get; }

        public IDictionary<string, EventType> ModuleEventTypes { get; }

        public IDictionary<string, NamedWindowMetaData> ModuleNamedWindows { get; }

        public IDictionary<string, TableMetaData> ModuleTables { get; }

        public ISet<ModuleIndexMeta> ModuleIndexes { get; }

        public IDictionary<string, ContextMetaData> ModuleContexts { get; }

        public IDictionary<string, VariableMetaData> ModuleVariables { get; }

        public IDictionary<string, ExpressionDeclItem> ModuleExpressions { get; }

        public IDictionary<NameAndParamNum, ExpressionScriptProvided> ModuleScripts { get; }

        public IDictionary<string, ClassProvided> ModuleClasses { get; }

        public IList<EventTypeCollectedSerde> EventTypeSerdes { get; }

        public ModuleIncidentals Incidentals => new ModuleIncidentals(ModuleNamedWindows, ModuleContexts, ModuleVariables, ModuleExpressions, ModuleTables);

        public EventTypeResolverImpl EventTypeResolver { get; }
    }
} // end of namespace