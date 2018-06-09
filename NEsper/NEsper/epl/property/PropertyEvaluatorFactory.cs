///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.threading;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.named;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.epl.variable;
using com.espertech.esper.events;
using com.espertech.esper.schedule;
using com.espertech.esper.script;
using com.espertech.esper.util;

using static com.espertech.esper.util.TypeHelper;

namespace com.espertech.esper.epl.property
{
    /// <summary>
    /// Factory for property evaluators.
    /// </summary>
    public class PropertyEvaluatorFactory
    {
        /// <summary>
        /// Makes the property evaluator.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="spec">is the property specification</param>
        /// <param name="sourceEventType">the event type</param>
        /// <param name="optionalSourceStreamName">the source stream name</param>
        /// <param name="eventAdapterService">for event instances</param>
        /// <param name="engineImportService">The engine import service.</param>
        /// <param name="timeProvider">provides time</param>
        /// <param name="variableService">for resolving variables</param>
        /// <param name="scriptingService">The scripting service.</param>
        /// <param name="tableService">The table service.</param>
        /// <param name="engineURI">engine URI</param>
        /// <param name="statementId">The statement identifier.</param>
        /// <param name="statementName">Name of the statement.</param>
        /// <param name="annotations">The annotations.</param>
        /// <param name="assignedTypeNumberStack">The assigned type number stack.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="namedWindowMgmtService">The named window service.</param>
        /// <param name="statementExtensionSvcContext">The statement extension SVC context.</param>
        /// <returns>
        /// property evaluator
        /// </returns>
        public static PropertyEvaluator MakeEvaluator(
            IContainer container,
            PropertyEvalSpec spec,
            EventType sourceEventType,
            string optionalSourceStreamName,
            EventAdapterService eventAdapterService,
            EngineImportService engineImportService,
            TimeProvider timeProvider,
            VariableService variableService,
            ScriptingService scriptingService,
            TableService tableService,
            string engineURI,
            int statementId,
            string statementName,
            Attribute[] annotations,
            ICollection<int> assignedTypeNumberStack,
            ConfigurationInformation configuration,
            NamedWindowMgmtService namedWindowMgmtService,
            StatementExtensionSvcContext statementExtensionSvcContext)
        {
            var length = spec.Atoms.Count;
            var containedEventEvals = new ContainedEventEval[length];
            var fragmentEventTypes = new FragmentEventType[length];
            var currentEventType = sourceEventType;
            var whereClauses = new ExprEvaluator[length];

            var streamEventTypes = new List<EventType>();
            var streamNames = new List<string>();
            var streamNameAndNumber = new Dictionary<string, int>().WithNullSupport();
            var expressionTexts = new List<string>();
            var validateContext = new ExprEvaluatorContextTimeOnly(container, timeProvider);

            streamEventTypes.Add(sourceEventType);
            streamNames.Add(optionalSourceStreamName);
            streamNameAndNumber.Put(optionalSourceStreamName, 0);
            expressionTexts.Add(sourceEventType.Name);

            IList<SelectClauseElementCompiled> cumulativeSelectClause = new List<SelectClauseElementCompiled>();
            for (var i = 0; i < length; i++)
            {
                var atom = spec.Atoms[i];
                ContainedEventEval containedEventEval = null;
                string expressionText = null;
                EventType streamEventType = null;
                FragmentEventType fragmentEventType = null;

                // Resolve directly as fragment event type if possible
                if (atom.SplitterExpression is ExprIdentNode)
                {
                    var propertyName = ((ExprIdentNode)atom.SplitterExpression).FullUnresolvedName;
                    fragmentEventType = currentEventType.GetFragmentType(propertyName);
                    if (fragmentEventType != null)
                    {
                        var getter = currentEventType.GetGetter(propertyName);
                        if (getter != null)
                        {
                            containedEventEval = new ContainedEventEvalGetter(getter);
                            expressionText = propertyName;
                            streamEventType = fragmentEventType.FragmentType;
                        }
                    }
                }

                // evaluate splitter expression
                if (containedEventEval == null)
                {
                    ExprNodeUtility.ValidatePlainExpression(ExprNodeOrigin.CONTAINEDEVENT, atom.SplitterExpression);

                    var availableTypes = streamEventTypes.ToArray();
                    var availableStreamNames = streamNames.ToArray();
                    var isIStreamOnly = new bool[streamNames.Count];
                    isIStreamOnly.Fill(true);
                    StreamTypeService streamTypeService = new StreamTypeServiceImpl(
                        availableTypes, availableStreamNames, isIStreamOnly, engineURI, false);
                    var validationContext = new ExprValidationContext(
                        container,
                        streamTypeService,
                        engineImportService,
                        statementExtensionSvcContext, null,
                        timeProvider, 
                        variableService,
                        tableService,
                        validateContext, 
                        eventAdapterService, 
                        statementName, 
                        statementId, 
                        annotations, null, 
                        scriptingService,
                        false, false, true, false, null, false);
                    var validatedExprNode = ExprNodeUtility.GetValidatedSubtree(
                        ExprNodeOrigin.CONTAINEDEVENT, atom.SplitterExpression, validationContext);
                    var evaluator = validatedExprNode.ExprEvaluator;

                    // determine result type
                    if (atom.OptionalResultEventType == null)
                    {
                        throw new ExprValidationException(
                            "Missing @type(name) declaration providing the event type name of the return type for expression '" +
                            ExprNodeUtility.ToExpressionStringMinPrecedenceSafe(atom.SplitterExpression) + "'");
                    }
                    streamEventType = eventAdapterService.GetEventTypeByName(atom.OptionalResultEventType);
                    if (streamEventType == null)
                    {
                        throw new ExprValidationException(
                            "Event type by name '" + atom.OptionalResultEventType + "' could not be found");
                    }

                    var returnType = evaluator.ReturnType;

                    // when the expression returns an array, allow array values to become the column of the single-column event type
                    if (returnType.IsArray &&
                        streamEventType.PropertyNames.Length == 1 &&
                        TypeHelper.IsSubclassOrImplementsInterface(
                            TypeHelper.GetBoxedType(returnType.GetElementType()),
                            TypeHelper.GetBoxedType(streamEventType.GetPropertyType(streamEventType.PropertyNames[0]))))
                    {
                        var writables = eventAdapterService.GetWriteableProperties(streamEventType, false);
                        if (!writables.IsEmpty())
                        {
                            try
                            {
                                EventBeanManufacturer manufacturer = EventAdapterServiceHelper.GetManufacturer(
                                        eventAdapterService, streamEventType, 
                                        new WriteablePropertyDescriptor[] { writables.First() }, 
                                        engineImportService, false, 
                                        eventAdapterService.EventAdapterAvroHandler);
                                containedEventEval = new ContainedEventEvalArrayToEvent(evaluator, manufacturer);
                            }
                            catch (EventBeanManufactureException e)
                            {
                                throw new ExprValidationException(
                                    "Event type '" + streamEventType.Name + "' cannot be populated: " + e.Message, e);
                            }
                        }
                        else
                        {
                            throw new ExprValidationException("Event type '" + streamEventType.Name + "' cannot be written to");
                        }
                    } 
                    else if (returnType.IsArray() && returnType.GetElementType() == typeof(EventBean))
                    {
                        containedEventEval = new ContainedEventEvalEventBeanArray(evaluator);
                    }
                    else
                    {
                        EventBeanFactory eventBeanFactory = EventAdapterServiceHelper.GetFactoryForType(
                            streamEventType, eventAdapterService);
                        // check expression result type against eventtype expected underlying type
                        if (returnType.IsArray())
                        {
                            if (!TypeHelper.IsSubclassOrImplementsInterface(returnType.GetElementType(), streamEventType.UnderlyingType))
                            {
                                throw new ExprValidationException(
                                    "Event type '" + streamEventType.Name + "' underlying type " +
                                    streamEventType.UnderlyingType.GetCleanName() +
                                    " cannot be assigned a value of type " + returnType.GetCleanName());
                            }
                        }
                        else if (GenericExtensions.IsGenericEnumerable(returnType) || returnType.IsImplementsInterface<IEnumerable>())
                        {
                            // fine, assumed to return the right type
                        }
                        else
                        {
                            throw new ExprValidationException(
                                "Return type of expression '" +
                                ExprNodeUtility.ToExpressionStringMinPrecedenceSafe(atom.SplitterExpression) + "' is '" +
                                returnType.GetCleanName() + "', expected an Iterable or array result");
                        }
                        containedEventEval = new ContainedEventEvalExprNode(evaluator, eventBeanFactory);
                    }
                    expressionText = ExprNodeUtility.ToExpressionStringMinPrecedenceSafe(validatedExprNode);
                    fragmentEventType = new FragmentEventType(streamEventType, true, false);
                }

                // validate where clause, if any
                streamEventTypes.Add(streamEventType);
                streamNames.Add(atom.OptionalAsName);
                streamNameAndNumber.Put(atom.OptionalAsName, i + 1);
                expressionTexts.Add(expressionText);

                if (atom.OptionalWhereClause != null)
                {
                    var whereTypes = streamEventTypes.ToArray();
                    var whereStreamNames = streamNames.ToArray();
                    var isIStreamOnly = new bool[streamNames.Count];
                    isIStreamOnly.Fill(true);
                    StreamTypeService streamTypeService = new StreamTypeServiceImpl(
                        whereTypes, whereStreamNames, isIStreamOnly, engineURI, false);
                    var validationContext = new ExprValidationContext(
                        container,
                        streamTypeService, 
                        engineImportService, 
                        statementExtensionSvcContext, null, 
                        timeProvider, 
                        variableService,
                        tableService,
                        validateContext,
                        eventAdapterService, 
                        statementName, 
                        statementId, 
                        annotations, null, 
                        scriptingService,
                        false, false, true, false, null, false);
                    whereClauses[i] =
                        ExprNodeUtility.GetValidatedSubtree(
                            ExprNodeOrigin.CONTAINEDEVENT, atom.OptionalWhereClause, validationContext).ExprEvaluator;
                }

                // validate select clause
                if (atom.OptionalSelectClause != null)
                {
                    var whereTypes = streamEventTypes.ToArray();
                    var whereStreamNames = streamNames.ToArray();
                    var isIStreamOnly = new bool[streamNames.Count];
                    isIStreamOnly.Fill(true);
                    StreamTypeService streamTypeService = new StreamTypeServiceImpl(
                        whereTypes, whereStreamNames, isIStreamOnly, engineURI, false);
                    var validationContext = new ExprValidationContext(
                        container,
                        streamTypeService,
                        engineImportService, 
                        statementExtensionSvcContext, null,
                        timeProvider,
                        variableService, 
                        tableService,
                        validateContext, 
                        eventAdapterService, 
                        statementName, 
                        statementId, 
                        annotations, null,
                        scriptingService,
                        false, false, true, false, null, false);

                    foreach (var raw in atom.OptionalSelectClause.SelectExprList)
                    {
                        if (raw is SelectClauseStreamRawSpec)
                        {
                            var rawStreamSpec = (SelectClauseStreamRawSpec)raw;
                            if (!streamNames.Contains(rawStreamSpec.StreamName))
                            {
                                throw new ExprValidationException(
                                    "Property rename '" + rawStreamSpec.StreamName + "' not found in path");
                            }
                            var streamSpec = new SelectClauseStreamCompiledSpec(
                                rawStreamSpec.StreamName, rawStreamSpec.OptionalAsName);
                            int streamNumber = streamNameAndNumber.Get(rawStreamSpec.StreamName);
                            streamSpec.StreamNumber = streamNumber;
                            cumulativeSelectClause.Add(streamSpec);
                        }
                        else if (raw is SelectClauseExprRawSpec)
                        {
                            var exprSpec = (SelectClauseExprRawSpec)raw;
                            var exprCompiled = ExprNodeUtility.GetValidatedSubtree(
                                ExprNodeOrigin.CONTAINEDEVENT, exprSpec.SelectExpression, validationContext);
                            var resultName = exprSpec.OptionalAsName;
                            if (resultName == null)
                            {
                                resultName = ExprNodeUtility.ToExpressionStringMinPrecedenceSafe(exprCompiled);
                            }
                            cumulativeSelectClause.Add(
                                new SelectClauseExprCompiledSpec(
                                    exprCompiled, resultName, exprSpec.OptionalAsName, exprSpec.IsEvents));

                            var isMinimal = ExprNodeUtility.IsMinimalExpression(exprCompiled);
                            if (isMinimal != null)
                            {
                                throw new ExprValidationException(
                                    "Expression in a property-selection may not utilize " + isMinimal);
                            }
                        }
                        else if (raw is SelectClauseElementWildcard)
                        {
                            // wildcards are stream selects: we assign a stream name (any) and add a stream wildcard select
                            var streamNameAtom = atom.OptionalAsName;
                            if (streamNameAtom == null)
                            {
                                streamNameAtom = UuidGenerator.Generate();
                            }

                            var streamSpec = new SelectClauseStreamCompiledSpec(streamNameAtom, atom.OptionalAsName);
                            var streamNumber = i + 1;
                            streamSpec.StreamNumber = streamNumber;
                            cumulativeSelectClause.Add(streamSpec);
                        }
                        else
                        {
                            throw new IllegalStateException("Unknown select clause item:" + raw);
                        }
                    }
                }

                currentEventType = fragmentEventType.FragmentType;
                fragmentEventTypes[i] = fragmentEventType;
                containedEventEvals[i] = containedEventEval;
            }

            if (cumulativeSelectClause.IsEmpty())
            {
                if (length == 1)
                {
                    return new PropertyEvaluatorSimple(
                        containedEventEvals[0], fragmentEventTypes[0], whereClauses[0], expressionTexts[0]);
                }
                else
                {
                    return new PropertyEvaluatorNested(containedEventEvals, fragmentEventTypes, whereClauses, expressionTexts);
                }
            }
            else
            {
                var accumulative = new PropertyEvaluatorAccumulative(
                    containedEventEvals, fragmentEventTypes, whereClauses, expressionTexts);

                var whereTypes = streamEventTypes.ToArray();
                var whereStreamNames = streamNames.ToArray();
                var isIStreamOnly = new bool[streamNames.Count];
                isIStreamOnly.Fill(true);
                StreamTypeService streamTypeService = new StreamTypeServiceImpl(
                    whereTypes, whereStreamNames, isIStreamOnly, engineURI, false);

                var cumulativeSelectArr = cumulativeSelectClause.ToArray();
                var selectExpr = SelectExprProcessorFactory.GetProcessor(
                    container,
                    assignedTypeNumberStack, 
                    cumulativeSelectArr, false, null, null, null, streamTypeService,
                    eventAdapterService, null, null, null, engineImportService, validateContext, variableService,
                    scriptingService,
                    tableService, timeProvider, engineURI, statementId, statementName, annotations, null, configuration, null,
                    namedWindowMgmtService, null, null, statementExtensionSvcContext);
                return new PropertyEvaluatorSelect(selectExpr, accumulative);
            }
        }
    }
} // end of namespace
