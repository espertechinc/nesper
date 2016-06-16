///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.factory;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.util;
using com.espertech.esper.events;
using com.espertech.esper.view;

namespace com.espertech.esper.core.start
{
    /// <summary>
    /// Starts and provides the stop method for EPL statements.
    /// </summary>
    public class EPStatementStartMethodCreateSchema : EPStatementStartMethodBase
    {
        public EPStatementStartMethodCreateSchema(StatementSpecCompiled statementSpec)
            : base(statementSpec)
        {
        }

        public override EPStatementStartResult StartInternal(
            EPServicesContext services,
            StatementContext statementContext,
            bool isNewStatement,
            bool isRecoveringStatement,
            bool isRecoveringResilient)
        {
            CreateSchemaDesc spec = StatementSpec.CreateSchemaDesc;

            EPLValidationUtil.ValidateTableExists(services.TableService, spec.SchemaName);
            EventType eventType = HandleCreateSchema(services, statementContext, spec);

            // enter a reference
            services.StatementEventTypeRefService.AddReferences(
                statementContext.StatementName, new String[]
                {
                    spec.SchemaName
                });

            EventType allocatedEventType = eventType;
            EPStatementStopMethod stopMethod = new ProxyEPStatementStopMethod(() =>
            {
                services.StatementEventTypeRefService.RemoveReferencesStatement(statementContext.StatementName);
                if (services.StatementEventTypeRefService.GetStatementNamesForType(spec.SchemaName).IsEmpty())
                {
                    services.EventAdapterService.RemoveType(allocatedEventType.Name);
                    services.FilterService.RemoveType(allocatedEventType);
                }
            });

            Viewable viewable = new ViewableDefaultImpl(eventType);

            // assign agent instance factory (an empty op)
            statementContext.StatementAgentInstanceFactory = new StatementAgentInstanceFactoryNoAgentInstance(viewable);

            return new EPStatementStartResult(viewable, stopMethod, null);
        }

        private EventType HandleCreateSchema(
            EPServicesContext services,
            StatementContext statementContext,
            CreateSchemaDesc spec)
        {
            EventType eventType;

            try
            {
                if (spec.AssignedType != AssignedType.VARIANT)
                {
                    eventType = EventTypeUtility.CreateNonVariantType(
                        false, spec, statementContext.Annotations, services.ConfigSnapshot, services.EventAdapterService,
                        services.EngineImportService);
                }
                else
                {
                    if (spec.CopyFrom != null && !spec.CopyFrom.IsEmpty())
                    {
                        throw new ExprValidationException("Copy-from types are not allowed with variant types");
                    }

                    var isAny = false;
                    var config = new ConfigurationVariantStream();
                    foreach (var typeName in spec.Types)
                    {
                        if (typeName.Trim().Equals("*"))
                        {
                            isAny = true;
                            break;
                        }
                        config.AddEventTypeName(typeName);
                    }
                    if (!isAny)
                    {
                        config.TypeVariance = TypeVarianceEnum.PREDEFINED;
                    }
                    else
                    {
                        config.TypeVariance = TypeVarianceEnum.ANY;
                    }
                    services.ValueAddEventService.AddVariantStream(
                        spec.SchemaName, config, services.EventAdapterService, services.EventTypeIdGenerator);
                    eventType = services.ValueAddEventService.GetValueAddProcessor(spec.SchemaName).ValueAddEventType;
                }
            }
            catch (Exception ex)
            {
                throw new ExprValidationException(ex.Message, ex);
            }

            return eventType;
        }
    }
}
