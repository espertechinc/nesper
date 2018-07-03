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
using com.espertech.esper.client.soda;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.core.eval;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.dot;
using com.espertech.esper.epl.named;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.epl.variable;
using com.espertech.esper.events;
using com.espertech.esper.events.vaevent;
using com.espertech.esper.schedule;
using com.espertech.esper.script;

using ContextDescriptor = com.espertech.esper.core.context.util.ContextDescriptor;

namespace com.espertech.esper.epl.core
{
    /// <summary>
    /// Factory for select expression processors.
    /// </summary>
    public class SelectExprProcessorFactory
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SelectExprProcessorFactory));

        /// <summary>
        /// Returns the processor to use for a given select-clause.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="assignedTypeNumberStack">The assigned type number stack.</param>
        /// <param name="selectionList">the list of select clause elements/items, which are expected to have been validated</param>
        /// <param name="isUsingWildcard">true if the wildcard (*) occurs in the select clause</param>
        /// <param name="insertIntoDesc">contains column names for the optional insert-into clause (if supplied)</param>
        /// <param name="optionalInsertIntoEventType">Type of the optional insert into event.</param>
        /// <param name="forClauseSpec">For clause spec.</param>
        /// <param name="typeService">serves stream type information</param>
        /// <param name="eventAdapterService">for generating wrapper instances for events</param>
        /// <param name="statementResultService">handles listeners/subscriptions awareness to reduce output result generation</param>
        /// <param name="valueAddEventService">service that handles update events and variant events</param>
        /// <param name="selectExprEventTypeRegistry">registry for event type to statements</param>
        /// <param name="engineImportService">The engine import service.</param>
        /// <param name="exprEvaluatorContext">context for expression evalauation</param>
        /// <param name="variableService">The variable service.</param>
        /// <param name="scriptingService">The scripting service.</param>
        /// <param name="tableService">The table service.</param>
        /// <param name="timeProvider">The time provider.</param>
        /// <param name="engineURI">The engine URI.</param>
        /// <param name="statementId">The statement identifier.</param>
        /// <param name="statementName">Name of the statement.</param>
        /// <param name="annotations">The annotations.</param>
        /// <param name="contextDescriptor">The context descriptor.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="selectExprProcessorCallback">The select expr processor callback.</param>
        /// <param name="namedWindowMgmtService">The named window service.</param>
        /// <param name="intoTableClause">The into table clause.</param>
        /// <param name="groupByRollupInfo">The group by rollup information.</param>
        /// <param name="statementExtensionSvcContext">The statement extension SVC context.</param>
        /// <returns>
        /// select-clause expression processor
        /// </returns>
        /// <exception cref="ExprValidationException">Expected any of the  + Arrays.ToString(ForClauseKeyword.Values()).ToLowerCase() +  for-clause keywords after reserved keyword 'for'
        /// or
        /// The for-clause with the  + ForClauseKeyword.GROUPED_DELIVERY.Name +  keyword requires one or more grouping expressions
        /// or
        /// The for-clause with the  + ForClauseKeyword.DISCRETE_DELIVERY.Name +  keyword does not allow grouping expressions
        /// or
        /// The for-clause with delivery keywords may only occur once in a statement
        /// or
        /// Expected any of the  + Arrays.ToString(ForClauseKeyword.Values()).ToLowerCase() +  for-clause keywords after reserved keyword 'for'</exception>
        /// <throws>ExprValidationException to indicate the select expression cannot be validated</throws>
        public static SelectExprProcessor GetProcessor(
            IContainer container,
            ICollection<int> assignedTypeNumberStack,
            SelectClauseElementCompiled[] selectionList,
            bool isUsingWildcard,
            InsertIntoDesc insertIntoDesc,
            EventType optionalInsertIntoEventType,
            ForClauseSpec forClauseSpec,
            StreamTypeService typeService,
            EventAdapterService eventAdapterService,
            StatementResultService statementResultService,
            ValueAddEventService valueAddEventService,
            SelectExprEventTypeRegistry selectExprEventTypeRegistry,
            EngineImportService engineImportService,
            ExprEvaluatorContext exprEvaluatorContext,
            VariableService variableService,
            ScriptingService scriptingService,
            TableService tableService,
            TimeProvider timeProvider,
            string engineURI,
            int statementId,
            string statementName,
            Attribute[] annotations,
            ContextDescriptor contextDescriptor,
            ConfigurationInformation configuration,
            SelectExprProcessorDeliveryCallback selectExprProcessorCallback,
            NamedWindowMgmtService namedWindowMgmtService,
            IntoTableSpec intoTableClause,
            GroupByRollupInfo groupByRollupInfo,
            StatementExtensionSvcContext statementExtensionSvcContext)
        {
            if (selectExprProcessorCallback != null)
            {
                var bindProcessor = new BindProcessor(selectionList, typeService.EventTypes, typeService.StreamNames, tableService);
                IDictionary<string, object> properties = new LinkedHashMap<string, object>();
                for (var i = 0; i < bindProcessor.ColumnNamesAssigned.Length; i++)
                {
                    properties.Put(bindProcessor.ColumnNamesAssigned[i], bindProcessor.ExpressionTypes[i]);
                }
                var eventType = eventAdapterService.CreateAnonymousObjectArrayType("Output_" + statementName, properties);
                return new SelectExprProcessorWDeliveryCallback(eventType, bindProcessor, selectExprProcessorCallback);
            }

            var synthetic = GetProcessorInternal(
                assignedTypeNumberStack, selectionList, isUsingWildcard, insertIntoDesc, optionalInsertIntoEventType,
                typeService, eventAdapterService, valueAddEventService, selectExprEventTypeRegistry,
                engineImportService, statementId, statementName, annotations, configuration, namedWindowMgmtService, tableService,
                groupByRollupInfo);

            // Handle table as an optional service
            if (statementResultService != null)
            {
                // Handle for-clause delivery contract checking
                ExprNode[] groupedDeliveryExpr = null;
                var forDelivery = false;
                if (forClauseSpec != null)
                {
                    foreach (var item in forClauseSpec.Clauses)
                    {
                        if (item.Keyword == null)
                        {
                            throw new ExprValidationException("Expected any of the " + EnumHelper.GetValues<ForClauseKeyword>().Render().ToLower() + " for-clause keywords after reserved keyword 'for'");
                        }
                        try
                        {
                            ForClauseKeyword keyword = EnumHelper.Parse<ForClauseKeyword>(item.Keyword);
                            if ((keyword == ForClauseKeyword.GROUPED_DELIVERY) && (item.Expressions.IsEmpty()))
                            {
                                throw new ExprValidationException(
                                    "The for-clause with the " + ForClauseKeyword.GROUPED_DELIVERY.GetName() +
                                    " keyword requires one or more grouping expressions");
                            }
                            if ((keyword == ForClauseKeyword.DISCRETE_DELIVERY) && (!item.Expressions.IsEmpty()))
                            {
                                throw new ExprValidationException(
                                    "The for-clause with the " + ForClauseKeyword.DISCRETE_DELIVERY.GetName() +
                                    " keyword does not allow grouping expressions");
                            }
                            if (forDelivery)
                            {
                                throw new ExprValidationException(
                                    "The for-clause with delivery keywords may only occur once in a statement");
                            }
                        }
                        catch (ExprValidationException)
                        {
                            throw;
                        }
                        catch (EPException)
                        {
                            throw;
                        }
                        catch (Exception ex)
                        {
                            throw new ExprValidationException("Expected any of the " + EnumHelper.GetValues<ForClauseKeyword>().Render().ToLower() + " for-clause keywords after reserved keyword 'for'", ex);
                        }

                        StreamTypeService type = new StreamTypeServiceImpl(synthetic.ResultEventType, null, false, engineURI);
                        groupedDeliveryExpr = new ExprNode[item.Expressions.Count];
                        var validationContext = new ExprValidationContext(
                            container,
                            type, 
                            engineImportService, 
                            statementExtensionSvcContext, 
                            null, 
                            timeProvider, 
                            variableService, 
                            tableService, 
                            exprEvaluatorContext,
                            eventAdapterService,
                            statementName, 
                            statementId, annotations, null,
                            scriptingService,
                            false, false, true, false, 
                            intoTableClause == null ? null : intoTableClause.Name, 
                            false);  // no context descriptor available
                        for (var i = 0; i < item.Expressions.Count; i++)
                        {
                            groupedDeliveryExpr[i] = ExprNodeUtility.GetValidatedSubtree(ExprNodeOrigin.FORCLAUSE, item.Expressions[i], validationContext);
                        }
                        forDelivery = true;
                    }
                }

                var bindProcessor = new BindProcessor(selectionList, typeService.EventTypes, typeService.StreamNames, tableService);
                statementResultService.SetSelectClause(bindProcessor.ExpressionTypes, bindProcessor.ColumnNamesAssigned, forDelivery, ExprNodeUtility.GetEvaluators(groupedDeliveryExpr), exprEvaluatorContext);
                return new SelectExprResultProcessor(statementResultService, synthetic, bindProcessor);
            }

            return synthetic;
        }

        private static SelectExprProcessor GetProcessorInternal(
            ICollection<int> assignedTypeNumberStack,
            SelectClauseElementCompiled[] selectionList,
            bool isUsingWildcard,
            InsertIntoDesc insertIntoDesc,
            EventType optionalInsertIntoEventType,
            StreamTypeService typeService,
            EventAdapterService eventAdapterService,
            ValueAddEventService valueAddEventService,
            SelectExprEventTypeRegistry selectExprEventTypeRegistry,
            EngineImportService engineImportService,
            int statementId,
            string statementName,
            Attribute[] annotations,
            ConfigurationInformation configuration,
            NamedWindowMgmtService namedWindowMgmtService,
            TableService tableService,
            GroupByRollupInfo groupByRollupInfo)
        {
            // Wildcard not allowed when insert into specifies column order
            if (isUsingWildcard && insertIntoDesc != null && !insertIntoDesc.ColumnNames.IsEmpty())
            {
                throw new ExprValidationException("Wildcard not allowed when insert-into specifies column order");
            }

            // Determine wildcard processor (select *)
            if (IsWildcardsOnly(selectionList))
            {
                // For joins
                if (typeService.StreamNames.Length > 1)
                {
                    Log.Debug(".getProcessor Using SelectExprJoinWildcardProcessor");
                    return SelectExprJoinWildcardProcessorFactory.Create(
                        assignedTypeNumberStack, statementId, statementName,
                        typeService.StreamNames, typeService.EventTypes,
                        eventAdapterService, insertIntoDesc, selectExprEventTypeRegistry, engineImportService,
                        annotations, configuration, tableService, typeService.EngineURIQualifier);
                }
                // Single-table selects with no insert-into
                // don't need extra processing
                else if (insertIntoDesc == null)
                {
                    Log.Debug(".getProcessor Using wildcard processor");
                    if (typeService.HasTableTypes)
                    {
                        var tableName = TableServiceUtil.GetTableNameFromEventType(typeService.EventTypes[0]);
                        return new SelectExprWildcardTableProcessor(tableName, tableService);
                    }
                    return new SelectExprWildcardProcessor(typeService.EventTypes[0]);
                }
            }

            // Verify the assigned or name used is unique
            if (insertIntoDesc == null)
            {
                VerifyNameUniqueness(selectionList);
            }

            // Construct processor
            var buckets = GetSelectExpressionBuckets(selectionList);

            var factory = new SelectExprProcessorHelper(
                assignedTypeNumberStack, buckets.Expressions, buckets.SelectedStreams, insertIntoDesc,
                optionalInsertIntoEventType, isUsingWildcard, typeService, eventAdapterService, valueAddEventService,
                selectExprEventTypeRegistry, engineImportService, statementId, statementName, annotations, configuration,
                namedWindowMgmtService, tableService, groupByRollupInfo);
            SelectExprProcessor processor = factory.GetEvaluator();

            // add reference to the type obtained
            var type = (EventTypeSPI)processor.ResultEventType;
            if (!typeService.IsOnDemandStreams && type.Metadata.TypeClass != TypeClass.ANONYMOUS)
            {
                selectExprEventTypeRegistry.Add(processor.ResultEventType);
            }
            return processor;
        }

        /// <summary>
        /// Verify that each given name occurs exactly one.
        /// </summary>
        /// <param name="selectionList">is the list of select items to verify names</param>
        /// <throws>com.espertech.esper.epl.expression.core.ExprValidationException thrown if a name occured more then once</throws>
        internal static void VerifyNameUniqueness(SelectClauseElementCompiled[] selectionList)
        {
            ISet<string> names = new HashSet<string>();
            foreach (var element in selectionList)
            {
                if (element is SelectClauseExprCompiledSpec)
                {
                    var expr = (SelectClauseExprCompiledSpec)element;
                    if (names.Contains(expr.AssignedName))
                    {
                        throw new ExprValidationException("Column name '" + expr.AssignedName + "' appears more then once in select clause");
                    }
                    names.Add(expr.AssignedName);
                }
                else if (element is SelectClauseStreamCompiledSpec)
                {
                    var stream = (SelectClauseStreamCompiledSpec)element;
                    if (stream.OptionalName == null)
                    {
                        continue; // ignore no-name stream selectors
                    }
                    if (names.Contains(stream.OptionalName))
                    {
                        throw new ExprValidationException("Column name '" + stream.OptionalName + "' appears more then once in select clause");
                    }
                    names.Add(stream.OptionalName);
                }
            }
        }

        private static bool IsWildcardsOnly(IEnumerable<SelectClauseElementCompiled> elements)
        {
            foreach (var element in elements)
            {
                if (!(element is SelectClauseElementWildcard))
                {
                    return false;
                }
            }
            return true;
        }

        private static SelectExprBuckets GetSelectExpressionBuckets(IEnumerable<SelectClauseElementCompiled> elements)
        {
            var expressions = new List<SelectClauseExprCompiledSpec>();
            var selectedStreams = new List<SelectExprStreamDesc>();

            foreach (var element in elements)
            {
                if (element is SelectClauseExprCompiledSpec)
                {
                    var expr = (SelectClauseExprCompiledSpec)element;
                    if (!IsTransposingFunction(expr.SelectExpression))
                    {
                        expressions.Add(expr);
                    }
                    else
                    {
                        selectedStreams.Add(new SelectExprStreamDesc(expr));
                    }
                }
                else if (element is SelectClauseStreamCompiledSpec)
                {
                    selectedStreams.Add(new SelectExprStreamDesc((SelectClauseStreamCompiledSpec)element));
                }
            }
            return new SelectExprBuckets(expressions, selectedStreams);
        }

        private static bool IsTransposingFunction(ExprNode selectExpression)
        {
            if (!(selectExpression is ExprDotNode))
            {
                return false;
            }
            var dotNode = (ExprDotNode)selectExpression;
            if (dotNode.ChainSpec[0].Name.ToLower() == EngineImportServiceConstants.EXT_SINGLEROW_FUNCTION_TRANSPOSE)
            {
                return true;
            }
            return false;
        }

        public class SelectExprBuckets
        {
            public SelectExprBuckets(IList<SelectClauseExprCompiledSpec> expressions, IList<SelectExprStreamDesc> selectedStreams)
            {
                Expressions = expressions;
                SelectedStreams = selectedStreams;
            }

            public IList<SelectExprStreamDesc> SelectedStreams { get; private set; }

            public IList<SelectClauseExprCompiledSpec> Expressions { get; private set; }
        }
    }
} // end of namespace
