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
using com.espertech.esper.common.client.util;
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
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.context.aifactory.createschema
{
	public class StmtForgeMethodCreateSchema : StmtForgeMethod {

	    private readonly StatementBaseInfo @base;

	    public StmtForgeMethodCreateSchema(StatementBaseInfo @base) {
	        this.@base = @base;
	    }

	    public StmtForgeMethodResult Make(string packageName, string classPostfix, StatementCompileTimeServices services) {
	        StatementSpecCompiled statementSpec = @base.StatementSpec;

	        CreateSchemaDesc spec = statementSpec.Raw.CreateSchemaDesc;

	        if (services.EventTypeCompileTimeResolver.GetTypeByName(spec.SchemaName) != null) {
	            throw new ExprValidationException("Event type named '" + spec.SchemaName + "' has already been declared");
	        }

	        EPLValidationUtil.ValidateTableExists(services.TableCompileTimeResolver, spec.SchemaName);
	        EventType eventType = HandleCreateSchema(spec, services);

	        CodegenPackageScope packageScope = new CodegenPackageScope(packageName, null, services.IsInstrumented);

	        string aiFactoryProviderClassName = CodeGenerationIDGenerator.GenerateClassNameSimple(typeof(StatementAIFactoryProvider), classPostfix);
	        StatementAgentInstanceFactoryCreateSchemaForge forge = new StatementAgentInstanceFactoryCreateSchemaForge(eventType);
	        StmtClassForgableAIFactoryProviderCreateSchema aiFactoryForgable = new StmtClassForgableAIFactoryProviderCreateSchema(aiFactoryProviderClassName, packageScope, forge);

	        SelectSubscriberDescriptor selectSubscriberDescriptor = new SelectSubscriberDescriptor();
	        StatementInformationalsCompileTime informationals = StatementInformationalsUtil.GetInformationals(@base, Collections.GetEmptyList<object>(), Collections.GetEmptyList<object>(), Collections.GetEmptyList<object>(), false, selectSubscriberDescriptor, packageScope, services);
	        string statementProviderClassName = CodeGenerationIDGenerator.GenerateClassNameSimple(typeof(StatementProvider), classPostfix);
	        StmtClassForgableStmtProvider stmtProvider = new StmtClassForgableStmtProvider(aiFactoryProviderClassName, statementProviderClassName, informationals, packageScope);

	        IList<StmtClassForgable> forgables = new List<StmtClassForgable>();
	        forgables.Add(aiFactoryForgable);
	        forgables.Add(stmtProvider);
	        return new StmtForgeMethodResult(forgables, Collections.GetEmptyList<object>(), Collections.GetEmptyList<object>(), Collections.GetEmptyList<object>(), Collections.GetEmptyList<object>());
	    }

	    private EventType HandleCreateSchema(CreateSchemaDesc spec, StatementCompileTimeServices services)
	            {

	        EventType eventType;

	        try {
	            if (spec.AssignedType != CreateSchemaDesc.AssignedType.VARIANT) {
	                eventType = EventTypeUtility.CreateNonVariantType(false, spec, @base, services);
	            } else {
	                eventType = HandleVariantType(spec, services);
	            }
	        }
	        catch (EPException)
	        {
	            throw;
	        }
	        catch (Exception ex)
	        {
	            throw new ExprValidationException(ex.Message, ex);
	        }

	        return eventType;
	    }

	    private EventType HandleVariantType(CreateSchemaDesc spec, StatementCompileTimeServices services) {
	        if (spec.CopyFrom != null && !spec.CopyFrom.IsEmpty()) {
	            throw new ExprValidationException("Copy-from types are not allowed with variant types");
	        }
	        string eventTypeName = spec.SchemaName;

	        // determine typing
	        bool isAny = false;
	        ISet<EventType> types = new LinkedHashSet<EventType>();
	        foreach (string typeName in spec.Types) {
	            if (typeName.Trim().Equals("*")) {
	                isAny = true;
	            } else {
	                EventType eventType = services.EventTypeCompileTimeResolver.GetTypeByName(typeName);
	                if (eventType == null) {
	                    throw new ExprValidationException("Event type by name '" + typeName + "' could not be found for use in variant stream by name '" + eventTypeName + "'");
	                }
	                types.Add(eventType);
	            }
	        }
	        EventType[] eventTypes = types.ToArray();
	        VariantSpec variantSpec = new VariantSpec(eventTypes, isAny ? ConfigurationCommonVariantStream.TypeVariance.ANY : ConfigurationCommonVariantStream.TypeVariance.PREDEFINED);

	        NameAccessModifier visibility = services.ModuleVisibilityRules.GetAccessModifierEventType(@base.StatementRawInfo, spec.SchemaName);
	        EventTypeBusModifier eventBusVisibility = services.ModuleVisibilityRules.GetBusModifierEventType(@base.StatementRawInfo, eventTypeName);
	        EventTypeUtility.ValidateModifiers(spec.SchemaName, eventBusVisibility, visibility);

	        EventTypeMetadata metadata = new EventTypeMetadata(eventTypeName, @base.ModuleName, EventTypeTypeClass.VARIANT, EventTypeApplicationType.VARIANT, visibility, eventBusVisibility, false, EventTypeIdPair.Unassigned());
	        VariantEventType variantEventType = new VariantEventType(metadata, variantSpec);
	        services.EventTypeCompileTimeRegistry.NewType(variantEventType);
	        return variantEventType;
	    }
	}
} // end of namespace