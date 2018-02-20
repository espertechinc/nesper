///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.core.start;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.baseagg;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.subquery;
using com.espertech.esper.epl.expression.visitor;
using com.espertech.esper.epl.named;
using com.espertech.esper.epl.property;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.epl.util;
using com.espertech.esper.epl.variable;
using com.espertech.esper.events;
using com.espertech.esper.schedule;
using com.espertech.esper.script;
using com.espertech.esper.view;

namespace com.espertech.esper.filter
{
    /// <summary>
    /// Helper to compile (validate and optimize) filter expressions as used in pattern and filter-based streams.
    /// </summary>
    public class FilterSpecCompiler
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Assigned for filter parameters that are based on boolean expression and not on
        /// any particular property name.
        /// <para />Keeping this artificial property name is a simplification as optimized filter parameters
        /// generally keep a property name.
        /// </summary>
        public const string PROPERTY_NAME_BOOLEAN_EXPRESSION = ".boolean_expression";

        /// <summary>
        /// Factory method for compiling filter expressions into a filter specification
        /// for use with filter service.
        /// </summary>
        /// <param name="eventType">is the filtered-out event type</param>
        /// <param name="eventTypeName">is the name of the event type</param>
        /// <param name="filterExpessions">is a list of filter expressions</param>
        /// <param name="optionalPropertyEvalSpec">specification for evaluating properties</param>
        /// <param name="taggedEventTypes">is a map of stream names (tags) and event types available</param>
        /// <param name="arrayEventTypes">is a map of name tags and event type per tag for repeat-expressions that generate an array of events</param>
        /// <param name="streamTypeService">is used to set rules for resolving properties</param>
        /// <param name="optionalStreamName">the stream name, if provided</param>
        /// <param name="statementContext">context for statement</param>
        /// <param name="assignedTypeNumberStack">The assigned type number stack.</param>
        /// <returns>
        /// compiled filter specification
        /// </returns>
        /// <throws>ExprValidationException if the expression or type validations failed</throws>
        public static FilterSpecCompiled MakeFilterSpec(
            EventType eventType,
            string eventTypeName,
            IList<ExprNode> filterExpessions,
            PropertyEvalSpec optionalPropertyEvalSpec,
            IDictionary<string, Pair<EventType, string>> taggedEventTypes,
            IDictionary<string, Pair<EventType, string>> arrayEventTypes,
            StreamTypeService streamTypeService,
            string optionalStreamName,
            StatementContext statementContext,
            ICollection<int> assignedTypeNumberStack)
        {
            // Validate all nodes, make sure each returns a boolean and types are good;
            // Also decompose all AND super nodes into individual expressions
            var validatedNodes = ValidateAllowSubquery(ExprNodeOrigin.FILTER, filterExpessions, streamTypeService, statementContext, taggedEventTypes, arrayEventTypes);
            return Build(validatedNodes, eventType, eventTypeName, optionalPropertyEvalSpec, taggedEventTypes, arrayEventTypes, streamTypeService, optionalStreamName, statementContext, assignedTypeNumberStack);
        }

        public static FilterSpecCompiled Build(
            IList<ExprNode> validatedNodes,
            EventType eventType,
            string eventTypeName,
            PropertyEvalSpec optionalPropertyEvalSpec,
            IDictionary<string, Pair<EventType, string>> taggedEventTypes,
            IDictionary<string, Pair<EventType, string>> arrayEventTypes,
            StreamTypeService streamTypeService,
            string optionalStreamName,
            StatementContext stmtContext,
            ICollection<int> assignedTypeNumberStack)
        {
            var evaluatorContextStmt = new ExprEvaluatorContextStatement(stmtContext, false);

            return BuildNoStmtCtx(
                stmtContext.Container,
                validatedNodes,
                eventType,
                eventTypeName,
                optionalPropertyEvalSpec,
                taggedEventTypes,
                arrayEventTypes,
                streamTypeService,
                optionalStreamName, assignedTypeNumberStack,
                evaluatorContextStmt,
                stmtContext.StatementId,
                stmtContext.StatementName,
                stmtContext.Annotations,
                stmtContext.ContextDescriptor,
                stmtContext.EngineImportService,
                stmtContext.EventAdapterService,
                stmtContext.FilterBooleanExpressionFactory,
                stmtContext.TimeProvider,
                stmtContext.VariableService,
                stmtContext.ScriptingService,
                stmtContext.TableService,
                stmtContext.ConfigSnapshot,
                stmtContext.NamedWindowMgmtService,
                stmtContext.StatementExtensionServicesContext);
        }

        public static FilterSpecCompiled BuildNoStmtCtx(
            IContainer container,
            IList<ExprNode> validatedFilterNodes,
            EventType eventType,
            string eventTypeName,
            PropertyEvalSpec optionalPropertyEvalSpec,
            IDictionary<string, Pair<EventType, string>> taggedEventTypes,
            IDictionary<string, Pair<EventType, string>> arrayEventTypes,
            StreamTypeService streamTypeService,
            string optionalStreamName,
            ICollection<int> assignedTypeNumberStack,
            ExprEvaluatorContext exprEvaluatorContext,
            int statementId,
            string statementName,
            Attribute[] annotations,
            ContextDescriptor contextDescriptor,
            EngineImportService engineImportService,
            EventAdapterService eventAdapterService,
            FilterBooleanExpressionFactory filterBooleanExpressionFactory,
            TimeProvider timeProvider,
            VariableService variableService,
            ScriptingService scriptingService,
            TableService tableService,
            ConfigurationInformation configurationInformation,
            NamedWindowMgmtService namedWindowMgmtService,
            StatementExtensionSvcContext statementExtensionSvcContext)
        {
            var args = new FilterSpecCompilerArgs(
                container,
                taggedEventTypes,
                arrayEventTypes,
                exprEvaluatorContext,
                statementName,
                statementId,
                streamTypeService,
                engineImportService,
                timeProvider,
                variableService,
                tableService,
                eventAdapterService,
                filterBooleanExpressionFactory,
                scriptingService,
                annotations,
                contextDescriptor,
                configurationInformation,
                statementExtensionSvcContext);
            var parameters = FilterSpecCompilerPlanner.PlanFilterParameters(validatedFilterNodes, args);

            PropertyEvaluator optionalPropertyEvaluator = null;
            if (optionalPropertyEvalSpec != null)
            {
                optionalPropertyEvaluator = PropertyEvaluatorFactory.MakeEvaluator(
                    container,
                    optionalPropertyEvalSpec,
                    eventType,
                    optionalStreamName,
                    eventAdapterService,
                    engineImportService,
                    timeProvider,
                    variableService,
                    scriptingService,
                    tableService,
                    streamTypeService.EngineURIQualifier,
                    statementId,
                    statementName,
                    annotations,
                    assignedTypeNumberStack,
                    configurationInformation,
                    namedWindowMgmtService,
                    statementExtensionSvcContext);
            }

            var spec = new FilterSpecCompiled(eventType, eventTypeName, parameters, optionalPropertyEvaluator);

            if (Log.IsDebugEnabled)
            {
                Log.Debug(".makeFilterSpec spec=" + spec);
            }

            return spec;
        }

        /// <summary>
        /// Validates expression nodes and returns a list of validated nodes.
        /// </summary>
        /// <param name="exprNodeOrigin">The expr node origin.</param>
        /// <param name="exprNodes">is the nodes to validate</param>
        /// <param name="streamTypeService">is provding type information for each stream</param>
        /// <param name="statementContext">context</param>
        /// <param name="taggedEventTypes">pattern tagged types</param>
        /// <param name="arrayEventTypes">@return list of validated expression nodes</param>
        /// <returns>
        /// expr nodes
        /// </returns>
        /// <exception cref="ExprValidationException">
        /// Failed to validate  + EPStatementStartMethodHelperSubselect.GetSubqueryInfoText(count, subselect) + :  + ex.Message
        /// or
        /// Filter expression not returning a boolean value: ' + ExprNodeUtility.ToExpressionStringMinPrecedenceSafe(validated) + '
        /// </exception>
        /// <throws>ExprValidationException for validation errors</throws>
        public static IList<ExprNode> ValidateAllowSubquery(
            ExprNodeOrigin exprNodeOrigin,
            IList<ExprNode> exprNodes,
            StreamTypeService streamTypeService,
            StatementContext statementContext,
            IDictionary<string, Pair<EventType, string>> taggedEventTypes,
            IDictionary<string, Pair<EventType, string>> arrayEventTypes)
        {
            IList<ExprNode> validatedNodes = new List<ExprNode>();

            var evaluatorContextStmt = new ExprEvaluatorContextStatement(statementContext, false);
            var validationContext = new ExprValidationContext(
                statementContext.Container,
                streamTypeService,
                statementContext.EngineImportService,
                statementContext.StatementExtensionServicesContext, null,
                statementContext.TimeProvider,
                statementContext.VariableService,
                statementContext.TableService,
                evaluatorContextStmt,
                statementContext.EventAdapterService,
                statementContext.StatementName,
                statementContext.StatementId,
                statementContext.Annotations,
                statementContext.ContextDescriptor,
                statementContext.ScriptingService,
                false, false, true, false, null, true);
            foreach (var node in exprNodes)
            {
                // Determine subselects
                var visitor = new ExprNodeSubselectDeclaredDotVisitor();
                node.Accept(visitor);

                // Compile subselects
                if (!visitor.Subselects.IsEmpty())
                {

                    // The outer event type is the filtered-type itself
                    var subselectStreamNumber = 2048;
                    var count = -1;
                    foreach (var subselect in visitor.Subselects)
                    {
                        count++;
                        subselectStreamNumber++;
                        try
                        {
                            HandleSubselectSelectClauses(subselectStreamNumber, statementContext, subselect,
                                streamTypeService.EventTypes[0], streamTypeService.StreamNames[0], streamTypeService.StreamNames[0],
                                taggedEventTypes, arrayEventTypes);
                        }
                        catch (ExprValidationException ex)
                        {
                            throw new ExprValidationException("Failed to validate " + EPStatementStartMethodHelperSubselect.GetSubqueryInfoText(count, subselect) + ": " + ex.Message, ex);
                        }
                    }
                }

                var validated = ExprNodeUtility.GetValidatedSubtree(exprNodeOrigin, node, validationContext);
                validatedNodes.Add(validated);

                if ((validated.ExprEvaluator.ReturnType != typeof(bool?)) && ((validated.ExprEvaluator.ReturnType != typeof(bool))))
                {
                    throw new ExprValidationException("Filter expression not returning a boolean value: '" + ExprNodeUtility.ToExpressionStringMinPrecedenceSafe(validated) + "'");
                }
            }

            return validatedNodes;
        }

        private static void HandleSubselectSelectClauses(
            int subselectStreamNumber,
            StatementContext statementContext,
            ExprSubselectNode subselect,
            EventType outerEventType,
            string outerEventTypeName,
            string outerStreamName,
            IDictionary<string, Pair<EventType, string>> taggedEventTypes,
            IDictionary<string, Pair<EventType, string>> arrayEventTypes)
        {
            var statementSpec = subselect.StatementSpecCompiled;
            var filterStreamSpec = statementSpec.StreamSpecs[0];

            ViewFactoryChain viewFactoryChain;
            string subselecteventTypeName = null;

            // construct view factory chain
            try
            {
                if (statementSpec.StreamSpecs[0] is FilterStreamSpecCompiled)
                {
                    var filterStreamSpecCompiled = (FilterStreamSpecCompiled)statementSpec.StreamSpecs[0];
                    subselecteventTypeName = filterStreamSpecCompiled.FilterSpec.FilterForEventTypeName;

                    // A child view is required to limit the stream
                    if (filterStreamSpec.ViewSpecs.Length == 0)
                    {
                        throw new ExprValidationException("Subqueries require one or more views to limit the stream, consider declaring a length or time window");
                    }

                    // Register filter, create view factories
                    viewFactoryChain = statementContext.ViewService.CreateFactories(subselectStreamNumber, filterStreamSpecCompiled.FilterSpec.ResultEventType, filterStreamSpec.ViewSpecs, filterStreamSpec.Options, statementContext, true, subselect.SubselectNumber);
                    subselect.RawEventType = viewFactoryChain.EventType;
                }
                else
                {
                    var namedSpec = (NamedWindowConsumerStreamSpec)statementSpec.StreamSpecs[0];
                    var processor = statementContext.NamedWindowMgmtService.GetProcessor(namedSpec.WindowName);
                    viewFactoryChain = statementContext.ViewService.CreateFactories(0, processor.NamedWindowType, namedSpec.ViewSpecs, namedSpec.Options, statementContext, true, subselect.SubselectNumber);
                    subselecteventTypeName = namedSpec.WindowName;
                    EPLValidationUtil.ValidateContextName(false, processor.NamedWindowName, processor.ContextName, statementContext.ContextName, true);
                    subselect.RawEventType = processor.NamedWindowType;
                }
            }
            catch (ViewProcessingException ex)
            {
                throw new ExprValidationException("Error validating subexpression: " + ex.Message, ex);
            }

            // the final event type
            var eventType = viewFactoryChain.EventType;

            // determine a stream name unless one was supplied
            var subexpressionStreamName = filterStreamSpec.OptionalStreamName;
            if (subexpressionStreamName == null)
            {
                subexpressionStreamName = "$subselect_" + subselectStreamNumber;
            }

            // Named windows don't allow data views
            if (filterStreamSpec is NamedWindowConsumerStreamSpec)
            {
                EPStatementStartMethodHelperValidate.ValidateNoDataWindowOnNamedWindow(viewFactoryChain.FactoryChain);
            }

            // Streams event types are the original stream types with the stream zero the subselect stream
            var namesAndTypes = new LinkedHashMap<string, Pair<EventType, string>>();
            namesAndTypes.Put(subexpressionStreamName, new Pair<EventType, string>(eventType, subselecteventTypeName));
            namesAndTypes.Put(outerStreamName, new Pair<EventType, string>(outerEventType, outerEventTypeName));
            if (taggedEventTypes != null)
            {
                foreach (KeyValuePair<string, Pair<EventType, string>> entry in taggedEventTypes)
                {
                    namesAndTypes.Put(entry.Key, new Pair<EventType, string>(entry.Value.First, entry.Value.Second));
                }
            }
            if (arrayEventTypes != null)
            {
                foreach (KeyValuePair<string, Pair<EventType, string>> entry in arrayEventTypes)
                {
                    namesAndTypes.Put(entry.Key, new Pair<EventType, string>(entry.Value.First, entry.Value.Second));
                }
            }
            StreamTypeService subselectTypeService = new StreamTypeServiceImpl(namesAndTypes, statementContext.EngineURI, true, true);
            var viewResourceDelegateSubselect = new ViewResourceDelegateUnverified();
            subselect.FilterSubqueryStreamTypes = subselectTypeService;

            // Validate select expression
            var selectClauseSpec = subselect.StatementSpecCompiled.SelectClauseSpec;
            if (selectClauseSpec.SelectExprList.Length > 0)
            {
                if (selectClauseSpec.SelectExprList.Length > 1)
                {
                    throw new ExprValidationException("Subquery multi-column select is not allowed in this context.");
                }

                var element = selectClauseSpec.SelectExprList[0];
                if (element is SelectClauseExprCompiledSpec)
                {
                    // validate
                    var compiled = (SelectClauseExprCompiledSpec)element;
                    var selectExpression = compiled.SelectExpression;
                    var evaluatorContextStmt = new ExprEvaluatorContextStatement(statementContext, false);
                    var validationContext = new ExprValidationContext(
                        statementContext.Container,
                        subselectTypeService,
                        statementContext.EngineImportService,
                        statementContext.StatementExtensionServicesContext,
                        viewResourceDelegateSubselect,
                        statementContext.SchedulingService,
                        statementContext.VariableService,
                        statementContext.TableService,
                        evaluatorContextStmt,
                        statementContext.EventAdapterService,
                        statementContext.StatementName,
                        statementContext.StatementId,
                        statementContext.Annotations,
                        statementContext.ContextDescriptor,
                        statementContext.ScriptingService,
                        false, false, true, false, null, false);
                    selectExpression = ExprNodeUtility.GetValidatedSubtree(ExprNodeOrigin.SUBQUERYSELECT, selectExpression, validationContext);
                    subselect.SelectClause = new ExprNode[] { selectExpression };
                    subselect.SelectAsNames = new string[] { compiled.AssignedName };

                    // handle aggregation
                    var aggExprNodes = new List<ExprAggregateNode>();
                    ExprAggregateNodeUtil.GetAggregatesBottomUp(selectExpression, aggExprNodes);
                    if (aggExprNodes.Count > 0)
                    {
                        // Other stream properties, if there is aggregation, cannot be under aggregation.
                        foreach (var aggNode in aggExprNodes)
                        {
                            var propertiesNodesAggregated = ExprNodeUtility.GetExpressionProperties(aggNode, true);
                            foreach (var pair in propertiesNodesAggregated)
                            {
                                if (pair.First != 0)
                                {
                                    throw new ExprValidationException("Subselect aggregation function cannot aggregate across correlated properties");
                                }
                            }
                        }

                        // This stream (stream 0) properties must either all be under aggregation, or all not be.
                        var propertiesNotAggregated = ExprNodeUtility.GetExpressionProperties(selectExpression, false);
                        foreach (var pair in propertiesNotAggregated)
                        {
                            if (pair.First == 0)
                            {
                                throw new ExprValidationException("Subselect properties must all be within aggregation functions");
                            }
                        }
                    }
                }
            }
        }
    }
} // end of namespace
