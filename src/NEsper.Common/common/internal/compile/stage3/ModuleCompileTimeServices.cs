///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Threading;

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.@internal.compile.compiler;
using com.espertech.esper.common.@internal.compile.stage1;
using com.espertech.esper.common.@internal.context.compile;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.classprovided.compiletime;
using com.espertech.esper.common.@internal.epl.dataflow.core;
using com.espertech.esper.common.@internal.epl.expression.declared.compiletime;
using com.espertech.esper.common.@internal.epl.historical.database.connection;
using com.espertech.esper.common.@internal.epl.index.compile;
using com.espertech.esper.common.@internal.epl.namedwindow.compile;
using com.espertech.esper.common.@internal.epl.namedwindow.path;
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.epl.script.compiletime;
using com.espertech.esper.common.@internal.epl.script.core;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.epl.variable.compiletime;
using com.espertech.esper.common.@internal.@event.avro;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.@event.bean.service;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.eventtyperepo;
using com.espertech.esper.common.@internal.@event.xml;
using com.espertech.esper.common.@internal.serde.compiletime.eventtype;
using com.espertech.esper.common.@internal.serde.compiletime.resolve;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.statemgmtsettings;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.container;


namespace com.espertech.esper.common.@internal.compile.stage3
{
    public class ModuleCompileTimeServices
    {
        private static long _generation = 0L;

        public ModuleCompileTimeServices(
            IContainer container,
            CompilerAbstraction compilerAbstraction,
            CompilerServices compilerServices,
            Configuration configuration,
            ClassProvidedCompileTimeRegistry classProvidedCompileTimeRegistry,
            ClassProvidedCompileTimeResolver classProvidedCompileTimeResolver,
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
            EventTypeXMLXSDHandler eventTypeXMLXSDHandler,
            bool fireAndForget,
            IndexCompileTimeRegistry indexCompileTimeRegistry,
            ModuleDependenciesCompileTime moduleDependencies,
            ModuleAccessModifierService moduleVisibilityRules,
            NamedWindowCompileTimeResolver namedWindowCompileTimeResolver,
            NamedWindowCompileTimeRegistry namedWindowCompileTimeRegistry,
            StateMgmtSettingsProvider stateMgmtSettingsProvider,
            ParentTypeResolver parentTypeResolver,
            PatternObjectResolutionService patternObjectResolutionService,
            ScriptCompileTimeRegistry scriptCompileTimeRegistry,
            ScriptCompileTimeResolver scriptCompileTimeResolver,
            ScriptCompiler scriptCompiler,
            SerdeEventTypeCompileTimeRegistry serdeEventTypeRegistry,
            SerdeCompileTimeResolver serdeResolver,
            TableCompileTimeRegistry tableCompileTimeRegistry,
            TableCompileTimeResolver tableCompileTimeResolver,
            VariableCompileTimeRegistry variableCompileTimeRegistry,
            VariableCompileTimeResolver variableCompileTimeResolver,
            ViewResolutionService viewResolutionService,
            XMLFragmentEventTypeFactory xmlFragmentEventTypeFactory)
        {
            var generation = Interlocked.Increment(ref _generation);

            Namespace = $"generation_{generation}";

            Container = container;
            CompilerAbstraction = compilerAbstraction;
            ParentTypeResolver = parentTypeResolver;
            StateMgmtSettingsProvider = stateMgmtSettingsProvider;
            CompilerServices = compilerServices;
            Configuration = configuration;
            ClassProvidedCompileTimeRegistry = classProvidedCompileTimeRegistry;
            ClassProvidedCompileTimeResolver = classProvidedCompileTimeResolver;
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
            EventTypeXMLXSDHandler = eventTypeXMLXSDHandler;
            IsFireAndForget = fireAndForget;
            IndexCompileTimeRegistry = indexCompileTimeRegistry;
            ModuleDependencies = moduleDependencies;
            ModuleVisibilityRules = moduleVisibilityRules;
            NamedWindowCompileTimeResolver = namedWindowCompileTimeResolver;
            NamedWindowCompileTimeRegistry = namedWindowCompileTimeRegistry;
            PatternObjectResolutionService = patternObjectResolutionService;
            ScriptCompileTimeRegistry = scriptCompileTimeRegistry;
            ScriptCompileTimeResolver = scriptCompileTimeResolver;
            ScriptCompiler = scriptCompiler;
            SerdeEventTypeRegistry = serdeEventTypeRegistry;
            SerdeResolver = serdeResolver;
            TableCompileTimeRegistry = tableCompileTimeRegistry;
            TableCompileTimeResolver = tableCompileTimeResolver;
            VariableCompileTimeRegistry = variableCompileTimeRegistry;
            VariableCompileTimeResolver = variableCompileTimeResolver;
            ViewResolutionService = viewResolutionService;
            XmlFragmentEventTypeFactory = xmlFragmentEventTypeFactory;
        }

        public ModuleCompileTimeServices(IContainer container)
        {
            Container = container;
            CompilerAbstraction = null;
            ClassProvidedCompileTimeRegistry = null;
            ClassProvidedCompileTimeResolver = null;
            ParentTypeResolver = null;
            StateMgmtSettingsProvider = null;
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
            EventTypeXMLXSDHandler = null;
            IsFireAndForget = false;
            IndexCompileTimeRegistry = null;
            SerdeEventTypeRegistry = null;
            SerdeResolver = null;
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

        public IContainer Container { get; }

        public BeanEventTypeStemService BeanEventTypeStemService { get; }

        public BeanEventTypeFactoryPrivate BeanEventTypeFactoryPrivate { get; }

        public ClassProvidedCompileTimeResolver ClassProvidedCompileTimeResolver { get; }

        public CompilerAbstraction CompilerAbstraction { get; }

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

        public EventTypeXMLXSDHandler EventTypeXMLXSDHandler { get; }

        public bool IsFireAndForget { get; }

        public IndexCompileTimeRegistry IndexCompileTimeRegistry { get; }

        public ModuleDependenciesCompileTime ModuleDependencies { get; }

        public ModuleAccessModifierService ModuleVisibilityRules { get; }

        public NamedWindowCompileTimeResolver NamedWindowCompileTimeResolver { get; }

        public NamedWindowCompileTimeRegistry NamedWindowCompileTimeRegistry { get; }

        public string Namespace { get; }

        public ParentTypeResolver ParentTypeResolver { get; }

        public PatternObjectResolutionService PatternObjectResolutionService { get; }

        public ScriptCompileTimeRegistry ScriptCompileTimeRegistry { get; }

        public ScriptCompileTimeResolver ScriptCompileTimeResolver { get; }

        public ScriptCompiler ScriptCompiler { get; }

        public SerdeEventTypeCompileTimeRegistry SerdeEventTypeRegistry { get; }

        public SerdeCompileTimeResolver SerdeResolver { get; }

        public TableCompileTimeRegistry TableCompileTimeRegistry { get; }

        public TableCompileTimeResolver TableCompileTimeResolver { get; }

        public VariableCompileTimeRegistry VariableCompileTimeRegistry { get; }

        public VariableCompileTimeResolver VariableCompileTimeResolver { get; }

        public ViewResolutionService ViewResolutionService { get; }

        public XMLFragmentEventTypeFactory XmlFragmentEventTypeFactory { get; }

        public EventTypeCompileTimeResolver EventTypeCompileTimeResolver { get; }

        public DataFlowCompileTimeRegistry DataFlowCompileTimeRegistry { get; } = new DataFlowCompileTimeRegistry();

        public bool IsInstrumented => Configuration.Compiler.ByteCode.IsInstrumented;

        public ClassProvidedCompileTimeRegistry ClassProvidedCompileTimeRegistry { get; }

        public StateMgmtSettingsProvider StateMgmtSettingsProvider { get; }
    }
} // end of namespace