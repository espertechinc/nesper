///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.@internal.compile.stage1;
using com.espertech.esper.common.@internal.compile.stage1.specmapper;
using com.espertech.esper.common.@internal.context.compile;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.classprovided.compiletime;
using com.espertech.esper.common.@internal.epl.dataflow.core;
using com.espertech.esper.common.@internal.epl.enummethod.compile;
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
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat;
using com.espertech.esper.container;

namespace com.espertech.esper.common.@internal.compile.stage3
{
    public class StatementCompileTimeServices
    {
        private readonly ModuleCompileTimeServices _services;
        private readonly ClassProvidedExtension _classProvidedExtension;
        private readonly EventTypeNameGeneratorStatement _eventTypeNameGeneratorStatement;

        public StatementCompileTimeServices(
            int statementNumber,
            ModuleCompileTimeServices services)
        {    
            _services = services;
            _eventTypeNameGeneratorStatement = new EventTypeNameGeneratorStatement(statementNumber);
            _classProvidedExtension = new ClassProvidedExtensionImpl(services.ClassProvidedCompileTimeResolver);
        }

        public BeanEventTypeStemService BeanEventTypeStemService => _services.BeanEventTypeStemService;

        public BeanEventTypeFactoryPrivate BeanEventTypeFactoryPrivate => _services.BeanEventTypeFactoryPrivate;

        public Configuration Configuration => _services.Configuration;

        public ContextCompileTimeRegistry ContextCompileTimeRegistry => _services.ContextCompileTimeRegistry;

        public ContextCompileTimeResolver ContextCompileTimeResolver => _services.ContextCompileTimeResolver;
        
        public ClassProvidedCompileTimeRegistry ClassProvidedCompileTimeRegistry => _services.ClassProvidedCompileTimeRegistry;

        public ClassProvidedCompileTimeResolver ClassProvidedCompileTimeResolver => _services.ClassProvidedCompileTimeResolver;

        public DatabaseConfigServiceCompileTime DatabaseConfigServiceCompileTime => _services.DatabaseConfigServiceCompileTime;

        public ImportServiceCompileTime ImportServiceCompileTime => _services.ImportServiceCompileTime;

        public EnumMethodCallStackHelperImpl EnumMethodCallStackHelper { get; } = new EnumMethodCallStackHelperImpl();

        public ExprDeclaredCompileTimeRegistry ExprDeclaredCompileTimeRegistry =>
            _services.ExprDeclaredCompileTimeRegistry;

        public ExprDeclaredCompileTimeResolver ExprDeclaredCompileTimeResolver =>
            _services.ExprDeclaredCompileTimeResolver;

        public EventTypeCompileTimeRegistry EventTypeCompileTimeRegistry => _services.EventTypeCompileTimeRegistry;

        public EventTypeRepositoryImpl EventTypeRepositoryPreconfigured => _services.EventTypeRepositoryPreconfigured;

        public IndexCompileTimeRegistry IndexCompileTimeRegistry => _services.IndexCompileTimeRegistry;

        public ModuleAccessModifierService ModuleVisibilityRules => _services.ModuleVisibilityRules;

        public NamedWindowCompileTimeResolver NamedWindowCompileTimeResolver => _services.NamedWindowCompileTimeResolver;

        public NamedWindowCompileTimeRegistry NamedWindowCompileTimeRegistry => _services.NamedWindowCompileTimeRegistry;

        public PatternObjectResolutionService PatternResolutionService => _services.PatternObjectResolutionService;

        public TableCompileTimeResolver TableCompileTimeResolver => _services.TableCompileTimeResolver;

        public TableCompileTimeRegistry TableCompileTimeRegistry => _services.TableCompileTimeRegistry;

        public VariableCompileTimeRegistry VariableCompileTimeRegistry => _services.VariableCompileTimeRegistry;

        public VariableCompileTimeResolver VariableCompileTimeResolver => _services.VariableCompileTimeResolver;

        public ViewResolutionService ViewResolutionService => _services.ViewResolutionService;

        public ScriptCompileTimeResolver ScriptCompileTimeResolver => _services.ScriptCompileTimeResolver;

        public ScriptCompileTimeRegistry ScriptCompileTimeRegistry => _services.ScriptCompileTimeRegistry;

        public ModuleDependenciesCompileTime ModuleDependenciesCompileTime => _services.ModuleDependencies;

        public EventTypeNameGeneratorStatement EventTypeNameGeneratorStatement => _eventTypeNameGeneratorStatement;

        public EventTypeAvroHandler EventTypeAvroHandler => _services.EventTypeAvroHandler;

        public EventTypeCompileTimeResolver EventTypeCompileTimeResolver => _services.EventTypeCompileTimeResolver;

        public CompilerServices CompilerServices => _services.CompilerServices;

        public ScriptCompiler ScriptCompiler => _services.ScriptCompiler;

        public DataFlowCompileTimeRegistry DataFlowCompileTimeRegistry => _services.DataFlowCompileTimeRegistry;

        public StatementSpecMapEnv StatementSpecMapEnv => new StatementSpecMapEnv(
            _services.ImportServiceCompileTime,
            _services.VariableCompileTimeResolver,
            _services.Configuration,
            _services.ExprDeclaredCompileTimeResolver,
            _services.ContextCompileTimeResolver,
            _services.TableCompileTimeResolver,
            _services.ScriptCompileTimeResolver,
            _services.CompilerServices,
            _classProvidedExtension);

        public bool IsInstrumented => _services.IsInstrumented();

        public IContainer Container => _services.Container;

        public ModuleCompileTimeServices Services => _services;

        public bool IsAttachPatternText => _services.Configuration.Compiler.ByteCode.IsAttachPatternEPL;
        
        public string Namespace => _services.Namespace;

        public ClassLoader ParentClassLoader => _services.ParentClassLoader;

        public SerdeEventTypeCompileTimeRegistry SerdeEventTypeRegistry => _services.SerdeEventTypeRegistry;

        public SerdeCompileTimeResolver SerdeResolver => _services.SerdeResolver;

        public XMLFragmentEventTypeFactory XmlFragmentEventTypeFactory => _services.XmlFragmentEventTypeFactory;

        public bool IsFireAndForget => _services.IsFireAndForget;

        public ClassProvidedExtension ClassProvidedExtension => _classProvidedExtension;
    }
} // end of namespace