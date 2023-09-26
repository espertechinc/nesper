///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.resultset.select.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.compat.collections;


namespace com.espertech.esper.common.@internal.context.aifactory.core
{
    public abstract class StmtForgeMethodCreateSimpleBase : StmtForgeMethod
    {
        protected readonly StatementBaseInfo _base;

        protected abstract StmtForgeMethodRegisterResult Register(StatementCompileTimeServices services);

        protected abstract StmtClassForgeable AiFactoryForgable(
            string className,
            CodegenNamespaceScope namespaceScope,
            EventType statementEventType,
            string objectName);

        public StmtForgeMethodCreateSimpleBase(StatementBaseInfo @base)
        {
            _base = @base;
        }

        public StmtForgeMethodResult Make(
            string @namespace,
            string classPostfix,
            StatementCompileTimeServices services)
        {
            var registerResult = Register(services);
            var objectName = registerResult.ObjectName;

            // define output event type
            var statementEventTypeName = services.EventTypeNameGeneratorStatement.AnonymousTypeName;
            var statementTypeMetadata = new EventTypeMetadata(
                statementEventTypeName,
                _base.ModuleName,
                EventTypeTypeClass.STATEMENTOUT,
                EventTypeApplicationType.MAP,
                NameAccessModifier.TRANSIENT,
                EventTypeBusModifier.NONBUS,
                false,
                EventTypeIdPair.Unassigned());
            EventType statementEventType = BaseNestableEventUtil.MakeMapTypeCompileTime(
                statementTypeMetadata,
                EmptyDictionary<string, object>.Instance, 
                null,
                null,
                null,
                null,
                services.BeanEventTypeFactoryPrivate,
                services.EventTypeCompileTimeResolver);
            services.EventTypeCompileTimeRegistry.NewType(statementEventType);

            var namespaceScope = new CodegenNamespaceScope(
                @namespace,
                null,
                services.IsInstrumented,
                services.Configuration.Compiler.ByteCode);

            var aiFactoryProviderClassName = CodeGenerationIDGenerator.GenerateClassNameSimple(
                typeof(StatementAIFactoryProvider),
                classPostfix);
            var aiFactoryForgeable = AiFactoryForgable(
                aiFactoryProviderClassName,
                namespaceScope,
                statementEventType,
                objectName);

            var selectSubscriberDescriptor = new SelectSubscriberDescriptor();
            var informationals = StatementInformationalsUtil.GetInformationals(
                _base,
                EmptyList<FilterSpecTracked>.Instance,
                EmptyList<ScheduleHandleTracked>.Instance,
                EmptyList<NamedWindowConsumerStreamSpec>.Instance, 
                false,
                selectSubscriberDescriptor,
                namespaceScope,
                services);
            informationals.Properties.Put(StatementProperty.CREATEOBJECTNAME, objectName);
            var statementProviderClassName =
                CodeGenerationIDGenerator.GenerateClassNameSimple(typeof(StatementProvider), classPostfix);
            var stmtProvider = new StmtClassForgeableStmtProvider(
                aiFactoryProviderClassName,
                statementProviderClassName,
                informationals,
                namespaceScope);

            IList<StmtClassForgeable> forgeables = new List<StmtClassForgeable>();
            forgeables.Add(aiFactoryForgeable);
            forgeables.Add(stmtProvider);
            return new StmtForgeMethodResult(
                forgeables,
                EmptyList<FilterSpecTracked>.Instance,
                EmptyList<ScheduleHandleTracked>.Instance,
                EmptyList<NamedWindowConsumerStreamSpec>.Instance,
                EmptyList<FilterSpecParamExprNodeForge>.Instance, 
                namespaceScope,
                registerResult.FabricCharge);
        }
    }
} // end of namespace