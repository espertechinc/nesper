///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.property;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.events;
using com.espertech.esper.filter;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.spec
{
    /// <summary>
    /// Unvalided filter-based stream specification.
    /// </summary>
    [Serializable]
    public class FilterStreamSpecRaw
        : StreamSpecBase
        , StreamSpecRaw
        , MetaDefItem
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly FilterSpecRaw _rawFilterSpec;

        /// <summary>Ctor. </summary>
        /// <param name="rawFilterSpec">is unvalidated filter specification</param>
        /// <param name="viewSpecs">is the view definition</param>
        /// <param name="optionalStreamName">is the stream name if supplied, or null if not supplied</param>
        /// <param name="streamSpecOptions">additional options, such as unidirectional stream in a join</param>
        public FilterStreamSpecRaw(FilterSpecRaw rawFilterSpec, ViewSpec[] viewSpecs, String optionalStreamName, StreamSpecOptions streamSpecOptions)
            : base(optionalStreamName, viewSpecs, streamSpecOptions)
        {
            _rawFilterSpec = rawFilterSpec;
        }

        /// <summary>Default ctor. </summary>
        public FilterStreamSpecRaw()
        {
        }

        /// <summary>Returns the unvalided filter spec. </summary>
        /// <value>filter def</value>
        public FilterSpecRaw RawFilterSpec
        {
            get { return _rawFilterSpec; }
        }

        public StreamSpecCompiled Compile(StatementContext context, ICollection<String> eventTypeReferences, bool isInsertInto, ICollection<int> assignedTypeNumberStack, bool isJoin, bool isContextDeclaration, bool isOnTrigger, String optionalStreamName)
        {
            StreamTypeService streamTypeService;

            // Determine the event type
            var eventName = _rawFilterSpec.EventTypeName;

            if (context.TableService != null && context.TableService.GetTableMetadata(eventName) != null)
            {
                if (ViewSpecs != null && ViewSpecs.Length > 0)
                {
                    throw new ExprValidationException("Views are not supported with tables");
                }
                if (RawFilterSpec.OptionalPropertyEvalSpec != null)
                {
                    throw new ExprValidationException("Contained-event expressions are not supported with tables");
                }
                var tableMetadata = context.TableService.GetTableMetadata(eventName);
                var streamTypeServiceX = new StreamTypeServiceImpl(new EventType[] { tableMetadata.InternalEventType }, new String[] { optionalStreamName }, new bool[] { true }, context.EngineURI, false);
                var validatedNodes = FilterSpecCompiler.ValidateAllowSubquery(ExprNodeOrigin.FILTER, _rawFilterSpec.FilterExpressions, streamTypeServiceX, context, null, null);
                return new TableQueryStreamSpec(OptionalStreamName, ViewSpecs, Options, eventName, validatedNodes);
            }

            // Could be a named window
            if (context.NamedWindowMgmtService.IsNamedWindow(eventName))
            {
                var namedWindowType = context.NamedWindowMgmtService.GetProcessor(eventName).TailView.EventType;
                streamTypeService = new StreamTypeServiceImpl(new EventType[] { namedWindowType }, new String[] { optionalStreamName }, new bool[] { true }, context.EngineURI, false);

                var validatedNodes = FilterSpecCompiler.ValidateAllowSubquery(
                    ExprNodeOrigin.FILTER, _rawFilterSpec.FilterExpressions, streamTypeService, context, null, null);

                PropertyEvaluator optionalPropertyEvaluator = null;
                if (_rawFilterSpec.OptionalPropertyEvalSpec != null)
                {
                    optionalPropertyEvaluator =
                        PropertyEvaluatorFactory.MakeEvaluator(
                            context.Container,
                            _rawFilterSpec.OptionalPropertyEvalSpec, namedWindowType, OptionalStreamName,
                            context.EventAdapterService,
                            context.EngineImportService,
                            context.TimeProvider,
                            context.VariableService,
                            context.ScriptingService,
                            context.TableService,
                            context.EngineURI,
                            context.StatementId,
                            context.StatementName,
                            context.Annotations, assignedTypeNumberStack,
                            context.ConfigSnapshot,
                            context.NamedWindowMgmtService,
                            context.StatementExtensionServicesContext);
                }
                eventTypeReferences.Add(((EventTypeSPI)namedWindowType).Metadata.PrimaryName);
                return new NamedWindowConsumerStreamSpec(eventName, OptionalStreamName, ViewSpecs, validatedNodes, Options, optionalPropertyEvaluator);
            }

            EventType eventType = null;

            if (context.ValueAddEventService.IsRevisionTypeName(eventName))
            {
                eventType = context.ValueAddEventService.GetValueAddUnderlyingType(eventName);
                eventTypeReferences.Add(((EventTypeSPI)eventType).Metadata.PrimaryName);
            }

            if (eventType == null)
            {
                eventType = ResolveType(context.EngineURI, eventName, context.EventAdapterService, context.PlugInTypeResolutionURIs);
                if (eventType is EventTypeSPI)
                {
                    eventTypeReferences.Add(((EventTypeSPI)eventType).Metadata.PrimaryName);
                }
            }

            // Validate all nodes, make sure each returns a bool and types are good;
            // Also decompose all AND super nodes into individual expressions
            streamTypeService = new StreamTypeServiceImpl(new EventType[] { eventType }, new String[] { base.OptionalStreamName }, new bool[] { true }, context.EngineURI, false);

            var spec = FilterSpecCompiler.MakeFilterSpec(eventType, eventName, _rawFilterSpec.FilterExpressions, _rawFilterSpec.OptionalPropertyEvalSpec,
                    null, null,  // no tags
                    streamTypeService, OptionalStreamName, context, assignedTypeNumberStack);

            return new FilterStreamSpecCompiled(spec, ViewSpecs, OptionalStreamName, Options);
        }

        /// <summary>Resolves a given event name to an event type. </summary>
        /// <param name="eventName">is the name to resolve</param>
        /// <param name="eventAdapterService">for resolving event types</param>
        /// <param name="engineURI">the provider URI</param>
        /// <param name="optionalResolutionURIs">is URIs for resolving the event name against plug-inn event representations, if any</param>
        /// <returns>event type</returns>
        /// <throws>ExprValidationException if the info cannot be resolved</throws>
        public static EventType ResolveType(String engineURI, String eventName, EventAdapterService eventAdapterService, IList<Uri> optionalResolutionURIs)
        {
            var eventType = eventAdapterService.GetEventTypeByName(eventName);

            // may already be known
            if (eventType != null)
            {
                return eventType;
            }

            var engineURIQualifier = engineURI;
            if (engineURI == null || EPServiceProviderConstants.DEFAULT_ENGINE_URI.Equals(engineURI))
            {
                engineURIQualifier = EPServiceProviderConstants.DEFAULT_ENGINE_URI_QUALIFIER;
            }

            // The event name can be prefixed by the engine URI, i.e. "select * from default.MyEvent"
            if (eventName.StartsWith(engineURIQualifier))
            {
                var indexDot = eventName.IndexOf('.');
                if (indexDot > 0)
                {
                    var eventNameURI = eventName.Substring(0, indexDot);
                    var eventNameRemainder = eventName.Substring(indexDot + 1);

                    if (engineURIQualifier.Equals(eventNameURI))
                    {
                        eventType = eventAdapterService.GetEventTypeByName(eventNameRemainder);
                    }
                }
            }

            // may now be known
            if (eventType != null)
            {
                return eventType;
            }

            // The type is not known yet, attempt to add as an object type with the same name
            String message = null;
            try
            {
                eventType = eventAdapterService.AddBeanType(eventName, eventName, true, false, false, false);
            }
            catch (EventAdapterException ex)
            {
                Log.Debug(".resolveType Event type named '" + eventName + "' not resolved as Type event");
                message = "Failed to resolve event type: " + ex.Message;
            }

            // Attempt to use plug-in event types
            try
            {
                eventType = eventAdapterService.AddPlugInEventType(eventName, optionalResolutionURIs, null);
            }
            catch (EventAdapterException)
            {
                Log.Debug(".resolveType Event type named '" + eventName + "' not resolved by plug-in event representations");
                // remains unresolved
            }

            if (eventType == null)
            {
                throw new ExprValidationException(message);
            }
            return eventType;
        }
    }
}
