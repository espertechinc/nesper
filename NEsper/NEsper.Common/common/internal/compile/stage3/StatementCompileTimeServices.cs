///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.@internal.compile.stage1;
using com.espertech.esper.common.@internal.compile.stage1.specmapper;
using com.espertech.esper.common.@internal.context.compile;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.dataflow.core;
using com.espertech.esper.common.@internal.epl.enummethod.compile;
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
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.container;

namespace com.espertech.esper.common.@internal.compile.stage3
{
    public class StatementCompileTimeServices
    {
        private readonly ModuleCompileTimeServices services;

        public StatementCompileTimeServices(
            int statementNumber,
            ModuleCompileTimeServices services)
        {
            this.services = services;
            EventTypeNameGeneratorStatement = new EventTypeNameGeneratorStatement(statementNumber);
        }

        public BeanEventTypeStemService BeanEventTypeStemService => services.BeanEventTypeStemService;

        public BeanEventTypeFactoryPrivate BeanEventTypeFactoryPrivate => services.BeanEventTypeFactoryPrivate;

        public Configuration Configuration => services.Configuration;

        public ContextCompileTimeRegistry ContextCompileTimeRegistry => services.ContextCompileTimeRegistry;

        public ContextCompileTimeResolver ContextCompileTimeResolver => services.ContextCompileTimeResolver;

        public DatabaseConfigServiceCompileTime DatabaseConfigServiceCompileTime =>
            services.DatabaseConfigServiceCompileTime;

        public ImportServiceCompileTime ImportServiceCompileTime =>
            services.ImportServiceCompileTime;

        public EnumMethodCallStackHelperImpl EnumMethodCallStackHelper { get; } = new EnumMethodCallStackHelperImpl();

        public ExprDeclaredCompileTimeRegistry ExprDeclaredCompileTimeRegistry =>
            services.ExprDeclaredCompileTimeRegistry;

        public ExprDeclaredCompileTimeResolver ExprDeclaredCompileTimeResolver =>
            services.ExprDeclaredCompileTimeResolver;

        public EventTypeCompileTimeRegistry EventTypeCompileTimeRegistry => services.EventTypeCompileTimeRegistry;

        public EventTypeRepositoryImpl EventTypeRepositoryPreconfigured => services.EventTypeRepositoryPreconfigured;

        public IndexCompileTimeRegistry IndexCompileTimeRegistry => services.IndexCompileTimeRegistry;

        public ModuleAccessModifierService ModuleVisibilityRules => services.ModuleVisibilityRules;

        public NamedWindowCompileTimeResolver NamedWindowCompileTimeResolver => services.NamedWindowCompileTimeResolver;

        public NamedWindowCompileTimeRegistry NamedWindowCompileTimeRegistry => services.NamedWindowCompileTimeRegistry;

        public PatternObjectResolutionService PatternResolutionService => services.PatternObjectResolutionService;

        public TableCompileTimeResolver TableCompileTimeResolver => services.TableCompileTimeResolver;

        public TableCompileTimeRegistry TableCompileTimeRegistry => services.TableCompileTimeRegistry;

        public VariableCompileTimeRegistry VariableCompileTimeRegistry => services.VariableCompileTimeRegistry;

        public VariableCompileTimeResolver VariableCompileTimeResolver => services.VariableCompileTimeResolver;

        public ViewResolutionService ViewResolutionService => services.ViewResolutionService;

        public ScriptCompileTimeResolver ScriptCompileTimeResolver => services.ScriptCompileTimeResolver;

        public ScriptCompileTimeRegistry ScriptCompileTimeRegistry => services.ScriptCompileTimeRegistry;

        public ModuleDependenciesCompileTime ModuleDependenciesCompileTime => services.ModuleDependencies;

        public EventTypeNameGeneratorStatement EventTypeNameGeneratorStatement { get; }

        public EventTypeAvroHandler EventTypeAvroHandler => services.EventTypeAvroHandler;

        public EventTypeCompileTimeResolver EventTypeCompileTimeResolver => services.EventTypeCompileTimeResolver;

        public CompilerServices CompilerServices => services.CompilerServices;

        public DataFlowCompileTimeRegistry DataFlowCompileTimeRegistry => services.DataFlowCompileTimeRegistry;

        public StatementSpecMapEnv StatementSpecMapEnv => new StatementSpecMapEnv(
            services.ImportServiceCompileTime,
            services.VariableCompileTimeResolver,
            services.Configuration,
            services.ExprDeclaredCompileTimeResolver,
            services.ContextCompileTimeResolver,
            services.TableCompileTimeResolver,
            services.ScriptCompileTimeResolver,
            services.CompilerServices);

        public bool IsInstrumented => services.IsInstrumented();

        public IContainer Container => services.Container;
    }
} // end of namespace