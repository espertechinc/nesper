///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.spec;
using com.espertech.esper.util;
using com.espertech.esper.view;

namespace com.espertech.esper.core.start
{
    /// <summary>
    /// Starts and provides the stop method for EPL statements.
    /// </summary>
    public class EPStatementStartMethodCreateContext : EPStatementStartMethodBase
    {
        public EPStatementStartMethodCreateContext(StatementSpecCompiled statementSpec)
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
            if (_statementSpec.OptionalContextName != null)
            {
                throw new ExprValidationException("A create-context statement cannot itself be associated to a context, please declare a nested context instead");
            }
            var context = _statementSpec.ContextDesc;
            var agentInstanceContext = GetDefaultAgentInstanceContext(statementContext);

            // compile filter specs, if any
            ISet<string> eventTypesReferenced = new HashSet<string>();
            ValidateContextDetail(services, statementContext, eventTypesReferenced, context.ContextDetail, agentInstanceContext);
            services.StatementEventTypeRefService.AddReferences(statementContext.StatementName, CollectionUtil.ToArray(eventTypesReferenced));

            // define output event type
            var typeName = "EventType_Context_" + context.ContextName;
            var statementResultEventType = services.EventAdapterService.CreateAnonymousMapType(typeName, Collections.GetEmptyMap<string, object>(), true);

            // add context - does not activate that context
            services.ContextManagementService.AddContextSpec(services, agentInstanceContext, context, isRecoveringResilient, statementResultEventType);

            EPStatementStopMethod stopMethod = new ProxyEPStatementStopMethod(() =>
            {
                // no action
            });

            EPStatementDestroyMethod destroyMethod = new ProxyEPStatementDestroyMethod(() => services.ContextManagementService.DestroyedContext(context.ContextName));
            return new EPStatementStartResult(new ZeroDepthStreamNoIterate(statementResultEventType), stopMethod, destroyMethod);
        }

        private void ValidateContextDetail(
            EPServicesContext servicesContext,
            StatementContext statementContext,
            ISet<string> eventTypesReferenced,
            ContextDetail contextDetail,
            AgentInstanceContext agentInstanceContext)
        {
            if (contextDetail is ContextDetailPartitioned)
            {
                var segmented = (ContextDetailPartitioned)contextDetail;
                foreach (var partition in segmented.Items)
                {
                    ValidateNotTable(servicesContext, partition.FilterSpecRaw.EventTypeName);
                    var raw = new FilterStreamSpecRaw(partition.FilterSpecRaw, ViewSpec.EMPTY_VIEWSPEC_ARRAY, null, StreamSpecOptions.DEFAULT);
                    var compiled = raw.Compile(statementContext, eventTypesReferenced, false, Collections.GetEmptyList<int>(), false, true, false, null);
                    if (!(compiled is FilterStreamSpecCompiled))
                    {
                        throw new ExprValidationException("Partition criteria may not include named windows");
                    }
                    var result = (FilterStreamSpecCompiled)compiled;
                    partition.FilterSpecCompiled = result.FilterSpec;
                }
            }
            else if (contextDetail is ContextDetailCategory)
            {

                // compile filter
                var category = (ContextDetailCategory)contextDetail;
                ValidateNotTable(servicesContext, category.FilterSpecRaw.EventTypeName);
                var raw = new FilterStreamSpecRaw(category.FilterSpecRaw, ViewSpec.EMPTY_VIEWSPEC_ARRAY, null, StreamSpecOptions.DEFAULT);
                var result = (FilterStreamSpecCompiled)raw.Compile(statementContext, eventTypesReferenced, false, Collections.GetEmptyList<int>(), false, true, false, null);
                category.FilterSpecCompiled = result.FilterSpec;
                servicesContext.StatementEventTypeRefService.AddReferences(statementContext.StatementName, CollectionUtil.ToArray(eventTypesReferenced));

                // compile expressions
                foreach (var item in category.Items)
                {
                    ValidateNotTable(servicesContext, category.FilterSpecRaw.EventTypeName);
                    var filterSpecRaw = new FilterSpecRaw(category.FilterSpecRaw.EventTypeName, Collections.SingletonList(item.Expression), null);
                    var rawExpr = new FilterStreamSpecRaw(filterSpecRaw, ViewSpec.EMPTY_VIEWSPEC_ARRAY, null, StreamSpecOptions.DEFAULT);
                    var compiled = (FilterStreamSpecCompiled)rawExpr.Compile(statementContext, eventTypesReferenced, false, Collections.GetEmptyList<int>(), false, true, false, null);
                    item.SetCompiledFilter(compiled.FilterSpec, agentInstanceContext);
                }
            }
            else if (contextDetail is ContextDetailHash)
            {
                var hashed = (ContextDetailHash)contextDetail;
                foreach (var hashItem in hashed.Items)
                {
                    var raw = new FilterStreamSpecRaw(hashItem.FilterSpecRaw, ViewSpec.EMPTY_VIEWSPEC_ARRAY, null, StreamSpecOptions.DEFAULT);
                    ValidateNotTable(servicesContext, hashItem.FilterSpecRaw.EventTypeName);
                    var result = (FilterStreamSpecCompiled)raw.Compile(statementContext, eventTypesReferenced, false, Collections.GetEmptyList<int>(), false, true, false, null);
                    hashItem.FilterSpecCompiled = result.FilterSpec;

                    // validate parameters
                    var streamTypes = new StreamTypeServiceImpl(result.FilterSpec.FilterForEventType, null, true, statementContext.EngineURI);
                    var validationContext = new ExprValidationContext(
                        statementContext.Container,
                        streamTypes,
                        statementContext.EngineImportService,
                        statementContext.StatementExtensionServicesContext, null,
                        statementContext.SchedulingService,
                        statementContext.VariableService, statementContext.TableService,
                        GetDefaultAgentInstanceContext(statementContext),
                        statementContext.EventAdapterService,
                        statementContext.StatementName,
                        statementContext.StatementId,
                        statementContext.Annotations,
                        statementContext.ContextDescriptor, 
                        statementContext.ScriptingService,
                        false, false, false, false,
                        null, false);
                    ExprNodeUtility.Validate(ExprNodeOrigin.CONTEXT, Collections.SingletonList(hashItem.Function), validationContext);
                }
            }
            else if (contextDetail is ContextDetailInitiatedTerminated)
            {
                var def = (ContextDetailInitiatedTerminated)contextDetail;
                var startCondition = ValidateRewriteContextCondition(servicesContext, statementContext, def.Start, eventTypesReferenced, new MatchEventSpec(), new LinkedHashSet<string>());
                var endCondition = ValidateRewriteContextCondition(servicesContext, statementContext, def.End, eventTypesReferenced, startCondition.Matches, startCondition.AllTags);
                def.Start = startCondition.Condition;
                def.End = endCondition.Condition;

                if (def.DistinctExpressions != null)
                {
                    if (!(startCondition.Condition is ContextDetailConditionFilter))
                    {
                        throw new ExprValidationException("Distinct-expressions require a stream as the initiated-by condition");
                    }
                    var distinctExpressions = def.DistinctExpressions;
                    if (distinctExpressions.Length == 0)
                    {
                        throw new ExprValidationException("Distinct-expressions have not been provided");
                    }
                    var filter = (ContextDetailConditionFilter)startCondition.Condition;
                    if (filter.OptionalFilterAsName == null)
                    {
                        throw new ExprValidationException("Distinct-expressions require that a stream name is assigned to the stream using 'as'");
                    }
                    var types = new StreamTypeServiceImpl(filter.FilterSpecCompiled.FilterForEventType, filter.OptionalFilterAsName, true, servicesContext.EngineURI);
                    var validationContext = new ExprValidationContext(
                        statementContext.Container,
                        types,
                        statementContext.EngineImportService,
                        statementContext.StatementExtensionServicesContext, null,
                        statementContext.SchedulingService,
                        statementContext.VariableService,
                        statementContext.TableService,
                        GetDefaultAgentInstanceContext(statementContext), 
                        statementContext.EventAdapterService,
                        statementContext.StatementName, 
                        statementContext.StatementId,
                        statementContext.Annotations,
                        statementContext.ContextDescriptor,
                        statementContext.ScriptingService,
                        false, false, true, false, null, false
                        );
                    for (var i = 0; i < distinctExpressions.Length; i++)
                    {
                        ExprNodeUtility.ValidatePlainExpression(ExprNodeOrigin.CONTEXTDISTINCT, distinctExpressions[i]);
                        distinctExpressions[i] = ExprNodeUtility.GetValidatedSubtree(ExprNodeOrigin.CONTEXTDISTINCT, distinctExpressions[i], validationContext);
                    }
                }
            }
            else if (contextDetail is ContextDetailNested)
            {
                var nested = (ContextDetailNested)contextDetail;
                foreach (var nestedContext in nested.Contexts)
                {
                    ValidateContextDetail(servicesContext, statementContext, eventTypesReferenced, nestedContext.ContextDetail, agentInstanceContext);
                }
            }
            else
            {
                throw new IllegalStateException("Unrecognized context detail " + contextDetail);
            }
        }

        private void ValidateNotTable(EPServicesContext servicesContext, string eventTypeName)
        {
            if (servicesContext.TableService.GetTableMetadata(eventTypeName) != null)
            {
                throw new ExprValidationException("Tables cannot be used in a context declaration");
            }
        }

        private ContextDetailMatchPair ValidateRewriteContextCondition(
            EPServicesContext servicesContext,
            StatementContext statementContext,
            ContextDetailCondition endpoint,
            ISet<string> eventTypesReferenced,
            MatchEventSpec priorMatches,
            ISet<string> priorAllTags)
        {
            if (endpoint is ContextDetailConditionCrontab)
            {
                var crontab = (ContextDetailConditionCrontab)endpoint;
                var scheduleSpecEvaluators = ExprNodeUtility.CrontabScheduleValidate(ExprNodeOrigin.CONTEXTCONDITION, crontab.Crontab, statementContext, false);
                var schedule = ExprNodeUtility.CrontabScheduleBuild(scheduleSpecEvaluators, new ExprEvaluatorContextStatement(statementContext, false));
                crontab.Schedule = schedule;
                return new ContextDetailMatchPair(crontab, new MatchEventSpec(), new LinkedHashSet<string>());
            }

            if (endpoint is ContextDetailConditionTimePeriod)
            {
                var timePeriod = (ContextDetailConditionTimePeriod)endpoint;
                var validationContext =
                    new ExprValidationContext(
                        statementContext.Container,
                        new StreamTypeServiceImpl(servicesContext.EngineURI, false),
                        statementContext.EngineImportService,
                        statementContext.StatementExtensionServicesContext, null,
                        statementContext.SchedulingService,
                        statementContext.VariableService,
                        statementContext.TableService,
                        GetDefaultAgentInstanceContext(statementContext),
                        statementContext.EventAdapterService,
                        statementContext.StatementName, statementContext.StatementId,
                        statementContext.Annotations,
                        statementContext.ContextDescriptor,
                        statementContext.ScriptingService,
                        false, false, false, false,
                        null, false);
                ExprNodeUtility.GetValidatedSubtree(ExprNodeOrigin.CONTEXTCONDITION, timePeriod.TimePeriod, validationContext);
                if (timePeriod.TimePeriod.IsConstantResult)
                {
                    if (timePeriod.TimePeriod.EvaluateAsSeconds(null, true, null) < 0)
                    {
                        throw new ExprValidationException("Invalid negative time period expression '" + ExprNodeUtility.ToExpressionStringMinPrecedenceSafe(timePeriod.TimePeriod) + "'");
                    }
                }
                return new ContextDetailMatchPair(timePeriod, new MatchEventSpec(), new LinkedHashSet<string>());
            }

            if (endpoint is ContextDetailConditionPattern)
            {
                var pattern = (ContextDetailConditionPattern)endpoint;
                var matches = ValidatePatternContextConditionPattern(statementContext, pattern, eventTypesReferenced, priorMatches, priorAllTags);
                return new ContextDetailMatchPair(pattern, matches.First, matches.Second);
            }

            if (endpoint is ContextDetailConditionFilter)
            {
                var filter = (ContextDetailConditionFilter)endpoint;
                ValidateNotTable(servicesContext, filter.FilterSpecRaw.EventTypeName);

                // compile as filter if there are no prior match to consider
                if (priorMatches == null || (priorMatches.ArrayEventTypes.IsEmpty() && priorMatches.TaggedEventTypes.IsEmpty()))
                {
                    var rawExpr = new FilterStreamSpecRaw(filter.FilterSpecRaw, ViewSpec.EMPTY_VIEWSPEC_ARRAY, null, StreamSpecOptions.DEFAULT);
                    var compiled = (FilterStreamSpecCompiled)rawExpr.Compile(statementContext, eventTypesReferenced, false, Collections.GetEmptyList<int>(), false, true, false, filter.OptionalFilterAsName);
                    filter.FilterSpecCompiled = compiled.FilterSpec;
                    var matchEventSpec = new MatchEventSpec();
                    var filterForType = compiled.FilterSpec.FilterForEventType;
                    var allTags = new LinkedHashSet<string>();
                    if (filter.OptionalFilterAsName != null)
                    {
                        matchEventSpec.TaggedEventTypes.Put(filter.OptionalFilterAsName, new Pair<EventType, string>(filterForType, rawExpr.RawFilterSpec.EventTypeName));
                        allTags.Add(filter.OptionalFilterAsName);
                    }
                    return new ContextDetailMatchPair(filter, matchEventSpec, allTags);
                }

                // compile as pattern if there are prior matches to consider, since this is a type of followed-by relationship
                var factoryNode = servicesContext.PatternNodeFactory.MakeFilterNode(filter.FilterSpecRaw, filter.OptionalFilterAsName, 0);
                var pattern = new ContextDetailConditionPattern(factoryNode, true, false);
                var matches = ValidatePatternContextConditionPattern(statementContext, pattern, eventTypesReferenced, priorMatches, priorAllTags);
                return new ContextDetailMatchPair(pattern, matches.First, matches.Second);
            }
            else if (endpoint is ContextDetailConditionImmediate || endpoint is ContextDetailConditionNever)
            {
                return new ContextDetailMatchPair(endpoint, new MatchEventSpec(), new LinkedHashSet<string>());
            }
            else
            {
                throw new IllegalStateException("Unrecognized endpoint type " + endpoint);
            }
        }

        private Pair<MatchEventSpec, ISet<string>> ValidatePatternContextConditionPattern(
            StatementContext statementContext,
            ContextDetailConditionPattern pattern,
            ISet<string> eventTypesReferenced,
            MatchEventSpec priorMatches,
            ISet<string> priorAllTags)
        {
            var raw = new PatternStreamSpecRaw(pattern.PatternRaw, ViewSpec.EMPTY_VIEWSPEC_ARRAY, null, StreamSpecOptions.DEFAULT, false, false);
            var compiled = raw.Compile(statementContext, eventTypesReferenced, false, Collections.GetEmptyList<int>(), priorMatches, priorAllTags, false, true, false);
            pattern.PatternCompiled = compiled;
            return new Pair<MatchEventSpec, ISet<string>>(new MatchEventSpec(compiled.TaggedEventTypes, compiled.ArrayEventTypes), compiled.AllTags);
        }

        internal class ContextDetailMatchPair
        {
            internal ContextDetailMatchPair(ContextDetailCondition condition, MatchEventSpec matches, ISet<string> allTags)
            {
                Condition = condition;
                Matches = matches;
                AllTags = allTags;
            }

            public ContextDetailCondition Condition { get; private set; }

            public MatchEventSpec Matches { get; private set; }

            public ISet<string> AllTags { get; private set; }
        }
    }
} // end of namespace
