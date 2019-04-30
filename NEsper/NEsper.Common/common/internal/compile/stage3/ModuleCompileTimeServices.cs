///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.@internal.compile.stage1;
using com.espertech.esper.common.@internal.context.compile;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.dataflow.core;
using com.espertech.esper.common.@internal.epl.expression.declared.compiletime;
using com.espertech.esper.common.@internal.epl.historical.database.connection;
using com.espertech.esper.common.@internal.epl.index.compile;
using com.espertech.esper.common.@internal.epl.namedwindow.compile;
using com.espertech.esper.common.@internal.epl.namedwindow.path;
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.epl.script.compiletime;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.epl.variable.compiletime;
using com.espertech.esper.common.@internal.@event.avro;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.@event.bean.service;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.eventtyperepo;
using com.espertech.esper.common.@internal.@event.xml;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.view.core;

namespace com.espertech.esper.common.@internal.compile.stage3
{
    public class ModuleCompileTimeServices
    {
        public ModuleCompileTimeServices(
            CompilerServices compilerServices,
            Configuration configuration,
            ContextCompileTimeRegistry contextCompileTimeRegistry,
            ContextCompileTimeResolver contextCompileTimeResolver,
            BeanEventTypeStemService beanEventTypeStemService,
            BeanEventTypeFactoryPrivate beanEventTypeFactoryPrivate,
            DatabaseConfigServiceCompileTime databaseConfigServiceCompileTime,
            ImportServiceCompileTime importService,
            ExprDeclaredCompileTimeRegistry exprDeclaredCompileTimeRegistry,
            ExprDeclaredCompileTimeResolver exprDeclaredCompileTimeResolver,
            EventTypeAvroHandler eventTypeAvroHandler,
            EventTypeCompileTimeRegistry eventTypeCompileTimeRegistry,
            EventTypeCompileTimeResolver eventTypeCompileTimeResolver,
            EventTypeRepositoryImpl eventTypeRepositoryPreconfigured,
            IndexCompileTimeRegistry indexCompileTimeRegistry,
            ModuleDependenciesCompileTime moduleDependencies,
            ModuleAccessModifierService moduleVisibilityRules,
            NamedWindowCompileTimeResolver namedWindowCompileTimeResolver,
            NamedWindowCompileTimeRegistry namedWindowCompileTimeRegistry,
            PatternObjectResolutionService patternObjectResolutionService,
            ScriptCompileTimeRegistry scriptCompileTimeRegistry,
            ScriptCompileTimeResolver scriptCompileTimeResolver,
            TableCompileTimeRegistry tableCompileTimeRegistry,
            TableCompileTimeResolver tableCompileTimeResolver,
            VariableCompileTimeRegistry variableCompileTimeRegistry,
            VariableCompileTimeResolver variableCompileTimeResolver,
            ViewResolutionService viewResolutionService,
            XMLFragmentEventTypeFactory xmlFragmentEventTypeFactory)
        {
            CompilerServices = compilerServices;
            Configuration = configuration;
            ContextCompileTimeRegistry = contextCompileTimeRegistry;
            ContextCompileTimeResolver = contextCompileTimeResolver;
            BeanEventTypeStemService = beanEventTypeStemService;
            BeanEventTypeFactoryPrivate = beanEventTypeFactoryPrivate;
            DatabaseConfigServiceCompileTime = databaseConfigServiceCompileTime;
            ImportServiceCompileTime = importService;
            ExprDeclaredCompileTimeRegistry = exprDeclaredCompileTimeRegistry;
            ExprDeclaredCompileTimeResolver = exprDeclaredCompileTimeResolver;
            EventTypeAvroHandler = eventTypeAvroHandler;
            EventTypeCompileTimeRegistry = eventTypeCompileTimeRegistry;
            EventTypeCompileTimeResolver = eventTypeCompileTimeResolver;
            EventTypeRepositoryPreconfigured = eventTypeRepositoryPreconfigured;
            IndexCompileTimeRegistry = indexCompileTimeRegistry;
            ModuleDependencies = moduleDependencies;
            ModuleVisibilityRules = moduleVisibilityRules;
            NamedWindowCompileTimeResolver = namedWindowCompileTimeResolver;
            NamedWindowCompileTimeRegistry = namedWindowCompileTimeRegistry;
            PatternObjectResolutionService = patternObjectResolutionService;
            ScriptCompileTimeRegistry = scriptCompileTimeRegistry;
            ScriptCompileTimeResolver = scriptCompileTimeResolver;
            TableCompileTimeRegistry = tableCompileTimeRegistry;
            TableCompileTimeResolver = tableCompileTimeResolver;
            VariableCompileTimeRegistry = variableCompileTimeRegistry;
            VariableCompileTimeResolver = variableCompileTimeResolver;
            ViewResolutionService = viewResolutionService;
            XmlFragmentEventTypeFactory = xmlFragmentEventTypeFactory;
        }

        public ModuleCompileTimeServices()
        {
            CompilerServices = null;
            Configuration = null;
            ContextCompileTimeRegistry = null;
            ContextCompileTimeResolver = null;
            BeanEventTypeStemService = null;
            BeanEventTypeFactoryPrivate = null;
            DatabaseConfigServiceCompileTime = null;
            ImportServiceCompileTime = null;
            ExprDeclaredCompileTimeRegistry = null;
            ExprDeclaredCompileTimeResolver = null;
            EventTypeAvroHandler = null;
            EventTypeCompileTimeRegistry = null;
            EventTypeCompileTimeResolver = null;
            EventTypeRepositoryPreconfigured = null;
            IndexCompileTimeRegistry = null;
            ModuleDependencies = null;
            ModuleVisibilityRules = null;
            NamedWindowCompileTimeResolver = null;
            NamedWindowCompileTimeRegistry = null;
            PatternObjectResolutionService = null;
            ScriptCompileTimeRegistry = null;
            ScriptCompileTimeResolver = null;
            TableCompileTimeRegistry = null;
            TableCompileTimeResolver = null;
            VariableCompileTimeRegistry = null;
            VariableCompileTimeResolver = null;
            ViewResolutionService = null;
            XmlFragmentEventTypeFactory = null;
        }

        public BeanEventTypeStemService BeanEventTypeStemService { get; }

        public BeanEventTypeFactoryPrivate BeanEventTypeFactoryPrivate { get; }

        public CompilerServices CompilerServices { get; }

        public Configuration Configuration { get; set; }

        public ContextCompileTimeRegistry ContextCompileTimeRegistry { get; }

        public ContextCompileTimeResolver ContextCompileTimeResolver { get; }

        public DatabaseConfigServiceCompileTime DatabaseConfigServiceCompileTime { get; }

        public ImportServiceCompileTime ImportServiceCompileTime { get; set; }

        public ExprDeclaredCompileTimeRegistry ExprDeclaredCompileTimeRegistry { get; }

        public ExprDeclaredCompileTimeResolver ExprDeclaredCompileTimeResolver { get; }

        public EventTypeAvroHandler EventTypeAvroHandler { get; }

        public EventTypeCompileTimeRegistry EventTypeCompileTimeRegistry { get; }

        public EventTypeRepositoryImpl EventTypeRepositoryPreconfigured { get; }

        public IndexCompileTimeRegistry IndexCompileTimeRegistry { get; }

        public ModuleDependenciesCompileTime ModuleDependencies { get; }

        public ModuleAccessModifierService ModuleVisibilityRules { get; }

        public NamedWindowCompileTimeResolver NamedWindowCompileTimeResolver { get; }

        public NamedWindowCompileTimeRegistry NamedWindowCompileTimeRegistry { get; }

        public PatternObjectResolutionService PatternObjectResolutionService { get; }

        public ScriptCompileTimeRegistry ScriptCompileTimeRegistry { get; }

        public ScriptCompileTimeResolver ScriptCompileTimeResolver { get; }

        public TableCompileTimeRegistry TableCompileTimeRegistry { get; }

        public TableCompileTimeResolver TableCompileTimeResolver { get; }

        public VariableCompileTimeRegistry VariableCompileTimeRegistry { get; }

        public VariableCompileTimeResolver VariableCompileTimeResolver { get; }

        public ViewResolutionService ViewResolutionService { get; }

        public XMLFragmentEventTypeFactory XmlFragmentEventTypeFactory { get; }

        public EventTypeCompileTimeResolver EventTypeCompileTimeResolver { get; }

        public DataFlowCompileTimeRegistry DataFlowCompileTimeRegistry { get; } = new DataFlowCompileTimeRegistry();

        public bool IsInstrumented()
        {
            return Configuration.Compiler.ByteCode.IsInstrumented;
        }
    }
} // end of namespace