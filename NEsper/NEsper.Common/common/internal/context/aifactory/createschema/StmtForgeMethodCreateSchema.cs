///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.resultset.select.core;
using com.espertech.esper.common.@internal.epl.util;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.variant;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.context.aifactory.createschema
{
    public class StmtForgeMethodCreateSchema : StmtForgeMethod
    {
        private readonly StatementBaseInfo @base;

        public StmtForgeMethodCreateSchema(StatementBaseInfo @base)
        {
            this.@base = @base;
        }

        public StmtForgeMethodResult Make(
            string @namespace,
            string classPostfix,
            StatementCompileTimeServices services)
        {
            var statementSpec = @base.StatementSpec;

            var spec = statementSpec.Raw.CreateSchemaDesc;

            if (services.EventTypeCompileTimeResolver.GetTypeByName(spec.SchemaName) != null) {
                throw new ExprValidationException(
                    "Event type named '" + spec.SchemaName + "' has already been declared");
            }

            EPLValidationUtil.ValidateTableExists(services.TableCompileTimeResolver, spec.SchemaName);
            var eventType = HandleCreateSchema(spec, services);

            var statementFieldsClassName = CodeGenerationIDGenerator.GenerateClassNameSimple(
                typeof(StatementFields), classPostfix);
            var namespaceScope = new CodegenNamespaceScope(
                @namespace, statementFieldsClassName, services.IsInstrumented);
            var fieldsForgable = new StmtClassForgableStmtFields(statementFieldsClassName, namespaceScope, 0);

            var aiFactoryProviderClassName = CodeGenerationIDGenerator.GenerateClassNameSimple(
                typeof(StatementAIFactoryProvider),
                classPostfix);
            var forge = new StatementAgentInstanceFactoryCreateSchemaForge(eventType);
            var aiFactoryForgable = new StmtClassForgableAIFactoryProviderCreateSchema(
                aiFactoryProviderClassName,
                namespaceScope,
                forge);

            var selectSubscriberDescriptor = new SelectSubscriberDescriptor();
            var informationals = StatementInformationalsUtil.GetInformationals(
                @base,
                new EmptyList<FilterSpecCompiled>(),
                new EmptyList<ScheduleHandleCallbackProvider>(),
                new EmptyList<NamedWindowConsumerStreamSpec>(),
                false,
                selectSubscriberDescriptor,
                namespaceScope,
                services);
            var statementProviderClassName =
                CodeGenerationIDGenerator.GenerateClassNameSimple(typeof(StatementProvider), classPostfix);
            var stmtProvider = new StmtClassForgableStmtProvider(
                aiFactoryProviderClassName,
                statementProviderClassName,
                informationals,
                namespaceScope);

            IList<StmtClassForgable> forgables = new List<StmtClassForgable>();
            forgables.Add(fieldsForgable);
            forgables.Add(aiFactoryForgable);
            forgables.Add(stmtProvider);
            return new StmtForgeMethodResult(
                forgables,
                new EmptyList<FilterSpecCompiled>(),
                new EmptyList<ScheduleHandleCallbackProvider>(),
                new EmptyList<NamedWindowConsumerStreamSpec>(),
                new EmptyList<FilterSpecParamExprNodeForge>());
        }

        private EventType HandleCreateSchema(
            CreateSchemaDesc spec,
            StatementCompileTimeServices services)
        {
            EventType eventType;

            try {
                if (spec.AssignedType != AssignedType.VARIANT) {
                    eventType = EventTypeUtility.CreateNonVariantType(false, spec, @base, services);
                }
                else {
                    eventType = HandleVariantType(spec, services);
                }
            }
            catch (EPException) {
                throw;
            }
            catch (Exception ex) {
                throw new ExprValidationException(ex.Message, ex);
            }

            return eventType;
        }

        private EventType HandleVariantType(
            CreateSchemaDesc spec,
            StatementCompileTimeServices services)
        {
            if (spec.CopyFrom != null && !spec.CopyFrom.IsEmpty()) {
                throw new ExprValidationException("Copy-from types are not allowed with variant types");
            }

            var eventTypeName = spec.SchemaName;

            // determine typing
            var isAny = false;
            ISet<EventType> types = new LinkedHashSet<EventType>();
            foreach (var typeName in spec.Types) {
                if (typeName.Trim().Equals("*")) {
                    isAny = true;
                }
                else {
                    var eventType = services.EventTypeCompileTimeResolver.GetTypeByName(typeName);
                    if (eventType == null) {
                        throw new ExprValidationException(
                            "Event type by name '" +
                            typeName +
                            "' could not be found for use in variant stream by name '" +
                            eventTypeName +
                            "'");
                    }

                    types.Add(eventType);
                }
            }

            var eventTypes = types.ToArray();
            var variantSpec = new VariantSpec(eventTypes, isAny ? TypeVariance.ANY : TypeVariance.PREDEFINED);

            var visibility =
                services.ModuleVisibilityRules.GetAccessModifierEventType(@base.StatementRawInfo, spec.SchemaName);
            var eventBusVisibility =
                services.ModuleVisibilityRules.GetBusModifierEventType(@base.StatementRawInfo, eventTypeName);
            EventTypeUtility.ValidateModifiers(spec.SchemaName, eventBusVisibility, visibility);

            var metadata = new EventTypeMetadata(
                eventTypeName,
                @base.ModuleName,
                EventTypeTypeClass.VARIANT,
                EventTypeApplicationType.VARIANT,
                visibility,
                eventBusVisibility,
                false,
                EventTypeIdPair.Unassigned());
            var variantEventType = new VariantEventType(metadata, variantSpec);
            services.EventTypeCompileTimeRegistry.NewType(variantEventType);
            return variantEventType;
        }
    }
} // end of namespace