///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using Castle.MicroKernel.ModelBuilder.Descriptors;

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.@internal.compile.compiler;
using com.espertech.esper.common.@internal.compile.stage1;
using com.espertech.esper.common.@internal.compile.stage1.specmapper;
using com.espertech.esper.common.@internal.context.compile;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.context.util;
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
using com.espertech.esper.common.@internal.statemgmtsettings;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.container;

namespace com.espertech.esper.common.@internal.compile.stage3
{
    public class StatementCompileTimeServices
    {
        public StatementCompileTimeServices(
            int statementNumber,
            ModuleCompileTimeServices services)
        {
            Services = services;
            EventTypeNameGeneratorStatement = new EventTypeNameGeneratorStatement(statementNumber);
            ClassProvidedExtension = new ClassProvidedExtensionImpl(services.ClassProvidedCompileTimeResolver);
        }

        public BeanEventTypeStemService BeanEventTypeStemService => Services.BeanEventTypeStemService;

        public BeanEventTypeFactoryPrivate BeanEventTypeFactoryPrivate => Services.BeanEventTypeFactoryPrivate;

        public Configuration Configuration => Services.Configuration;

        public ContextCompileTimeRegistry ContextCompileTimeRegistry => Services.ContextCompileTimeRegistry;

        public ContextCompileTimeResolver ContextCompileTimeResolver => Services.ContextCompileTimeResolver;

        public DatabaseConfigServiceCompileTime DatabaseConfigServiceCompileTime =>
            Services.DatabaseConfigServiceCompileTime;

        public ImportServiceCompileTime ImportServiceCompileTime => Services.ImportServiceCompileTime;

        public ClassProvidedCompileTimeRegistry ClassProvidedCompileTimeRegistry =>
            Services.ClassProvidedCompileTimeRegistry;

        public ClassProvidedCompileTimeResolver ClassProvidedCompileTimeResolver =>
            Services.ClassProvidedCompileTimeResolver;

        public EnumMethodCallStackHelperImpl EnumMethodCallStackHelper { get; } = new EnumMethodCallStackHelperImpl();

        public ExprDeclaredCompileTimeRegistry ExprDeclaredCompileTimeRegistry =>
            Services.ExprDeclaredCompileTimeRegistry;

        public ExprDeclaredCompileTimeResolver ExprDeclaredCompileTimeResolver =>
            Services.ExprDeclaredCompileTimeResolver;

        public EventTypeCompileTimeRegistry EventTypeCompileTimeRegistry => Services.EventTypeCompileTimeRegistry;

        public EventTypeRepositoryImpl EventTypeRepositoryPreconfigured => Services.EventTypeRepositoryPreconfigured;

        public IndexCompileTimeRegistry IndexCompileTimeRegistry => Services.IndexCompileTimeRegistry;

        public ModuleAccessModifierService ModuleVisibilityRules => Services.ModuleVisibilityRules;

        public NamedWindowCompileTimeResolver NamedWindowCompileTimeResolver => Services.NamedWindowCompileTimeResolver;

        public NamedWindowCompileTimeRegistry NamedWindowCompileTimeRegistry => Services.NamedWindowCompileTimeRegistry;

        public PatternObjectResolutionService PatternResolutionService => Services.PatternObjectResolutionService;

        public TableCompileTimeResolver TableCompileTimeResolver => Services.TableCompileTimeResolver;

        public TableCompileTimeRegistry TableCompileTimeRegistry => Services.TableCompileTimeRegistry;

        public VariableCompileTimeRegistry VariableCompileTimeRegistry => Services.VariableCompileTimeRegistry;

        public VariableCompileTimeResolver VariableCompileTimeResolver => Services.VariableCompileTimeResolver;

        public ViewResolutionService ViewResolutionService => Services.ViewResolutionService;

        public StatementSpecMapEnv StatementSpecMapEnv =>
            new StatementSpecMapEnv(
                Services.Container,
                Services.ImportServiceCompileTime,
                Services.VariableCompileTimeResolver,
                Services.Configuration,
                Services.ExprDeclaredCompileTimeResolver,
                Services.ContextCompileTimeResolver,
                Services.TableCompileTimeResolver,
                Services.ScriptCompileTimeResolver,
                Services.CompilerServices,
                ClassProvidedExtension);

        public ScriptCompileTimeResolver ScriptCompileTimeResolver => Services.ScriptCompileTimeResolver;

        public ScriptCompileTimeRegistry ScriptCompileTimeRegistry => Services.ScriptCompileTimeRegistry;

        public ModuleDependenciesCompileTime ModuleDependenciesCompileTime => Services.ModuleDependencies;

        public EventTypeNameGeneratorStatement EventTypeNameGeneratorStatement { get; }

        public EventTypeAvroHandler EventTypeAvroHandler => Services.EventTypeAvroHandler;

        public EventTypeXMLXSDHandler EventTypeXMLXSDHandler => Services.EventTypeXMLXSDHandler;

        public EventTypeCompileTimeResolver EventTypeCompileTimeResolver => Services.EventTypeCompileTimeResolver;

        public CompilerAbstraction CompilerAbstraction => Services.CompilerAbstraction;

        public CompilerServices CompilerServices => Services.CompilerServices;

        public ScriptCompiler ScriptCompiler => Services.ScriptCompiler;

        public DataFlowCompileTimeRegistry DataFlowCompileTimeRegistry => Services.DataFlowCompileTimeRegistry;

        public bool IsInstrumented => Services.IsInstrumented;

        public ModuleCompileTimeServices Services { get; }

        public bool IsAttachPatternText => Services.Configuration.Compiler.ByteCode.IsAttachPatternEPL;

        public string Namespace => Services.Namespace;

        public ParentTypeResolver ParentTypeResolver => Services.ParentTypeResolver;

        public SerdeEventTypeCompileTimeRegistry SerdeEventTypeRegistry => Services.SerdeEventTypeRegistry;

        public SerdeCompileTimeResolver SerdeResolver => Services.SerdeResolver;

        public XMLFragmentEventTypeFactory XmlFragmentEventTypeFactory => Services.XmlFragmentEventTypeFactory;

        public bool IsFireAndForget => Services.IsFireAndForget;

        public ClassProvidedExtension ClassProvidedExtension { get; }

        public StateMgmtSettingsProvider StateMgmtSettingsProvider => Services.StateMgmtSettingsProvider;

        public IContainer Container => Services.Container;
    }
} // end of namespace