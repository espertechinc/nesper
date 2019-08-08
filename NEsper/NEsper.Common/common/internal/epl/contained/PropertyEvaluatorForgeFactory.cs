///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.resultset.@select.core;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.contained
{
    /// <summary>
    ///     Factory for property evaluators.
    /// </summary>
    public class PropertyEvaluatorForgeFactory
    {
        public static PropertyEvaluatorForge MakeEvaluator(
            PropertyEvalSpec spec,
            EventType sourceEventType,
            string optionalSourceStreamName,
            StatementRawInfo rawInfo,
            StatementCompileTimeServices services)
        {
            var length = spec.Atoms.Count;
            var containedEventForges = new ContainedEventEvalForge[length];
            var fragmentEventTypes = new FragmentEventType[length];
            var currentEventType = sourceEventType;
            var whereClauses = new ExprForge[length];

            IList<EventType> streamEventTypes = new List<EventType>();
            IList<string> streamNames = new List<string>();
            IDictionary<string, int> streamNameAndNumber = new Dictionary<string, int>().WithNullKeySupport();
            IList<string> expressionTexts = new List<string>();

            streamEventTypes.Add(sourceEventType);
            streamNames.Add(optionalSourceStreamName);
            streamNameAndNumber.Put(optionalSourceStreamName, 0);
            expressionTexts.Add(sourceEventType.Name);

            IList<SelectClauseElementCompiled> cumulativeSelectClause = new List<SelectClauseElementCompiled>();
            for (var i = 0; i < length; i++) {
                var atom = spec.Atoms[i];
                ContainedEventEvalForge containedEventEval = null;
                string expressionText = null;
                EventType streamEventType = null;
                FragmentEventType fragmentEventType = null;

                // Resolve directly as fragment event type if possible
                if (atom.SplitterExpression is ExprIdentNode) {
                    var propertyName = ((ExprIdentNode) atom.SplitterExpression).FullUnresolvedName;
                    fragmentEventType = currentEventType.GetFragmentType(propertyName);
                    if (fragmentEventType != null) {
                        var getter = ((EventTypeSPI) currentEventType).GetGetterSPI(propertyName);
                        if (getter != null) {
                            containedEventEval = new ContainedEventEvalGetterForge(getter);
                            expressionText = propertyName;
                            streamEventType = fragmentEventType.FragmentType;
                        }
                    }
                }

                // evaluate splitter expression
                if (containedEventEval == null) {
                    ExprNodeUtilityValidate.ValidatePlainExpression(
                        ExprNodeOrigin.CONTAINEDEVENT,
                        atom.SplitterExpression);

                    var availableTypes = streamEventTypes.ToArray();
                    var availableStreamNames = streamNames.ToArray();
                    var isIStreamOnly = new bool[streamNames.Count];
                    isIStreamOnly.Fill(true);
                    StreamTypeService streamTypeService = new StreamTypeServiceImpl(
                        availableTypes,
                        availableStreamNames,
                        isIStreamOnly,
                        false,
                        false);
                    var validationContext = new ExprValidationContextBuilder(streamTypeService, rawInfo, services)
                        .WithAllowBindingConsumption(true)
                        .Build();
                    var validatedExprNode = ExprNodeUtilityValidate.GetValidatedSubtree(
                        ExprNodeOrigin.CONTAINEDEVENT,
                        atom.SplitterExpression,
                        validationContext);

                    // determine result type
                    if (atom.OptionalResultEventType == null) {
                        throw new ExprValidationException(
                            "Missing @type(name) declaration providing the event type name of the return type for expression '" +
                            ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(atom.SplitterExpression) +
                            "'");
                    }

                    streamEventType = services.EventTypeCompileTimeResolver.GetTypeByName(atom.OptionalResultEventType);
                    if (streamEventType == null) {
                        throw new ExprValidationException(
                            "Event type by name '" + atom.OptionalResultEventType + "' could not be found");
                    }

                    var returnType = validatedExprNode.Forge.EvaluationType;

                    // when the expression returns an array, allow array values to become the column of the single-column event type
                    if (returnType.IsArray &&
                        streamEventType.PropertyNames.Length == 1 &&
                        TypeHelper.IsSubclassOrImplementsInterface(
                            returnType.GetElementType().GetBoxedType(),
                            streamEventType.GetPropertyType(streamEventType.PropertyNames[0]).GetBoxedType())) {
                        var writables = EventTypeUtility.GetWriteableProperties(streamEventType, false);
                        if (writables != null && !writables.IsEmpty()) {
                            try {
                                var manufacturer = EventTypeUtility.GetManufacturer(
                                    streamEventType,
                                    new[] {writables.First()},
                                    services.ImportServiceCompileTime,
                                    false,
                                    services.EventTypeAvroHandler);
                                containedEventEval = new ContainedEventEvalArrayToEventForge(
                                    validatedExprNode.Forge,
                                    manufacturer);
                            }
                            catch (EventBeanManufactureException e) {
                                throw new ExprValidationException(
                                    "Event type '" + streamEventType.Name + "' cannot be populated: " + e.Message,
                                    e);
                            }
                        }
                        else {
                            throw new ExprValidationException(
                                "Event type '" + streamEventType.Name + "' cannot be written to");
                        }
                    }
                    else if (returnType.IsArray &&
                             returnType.GetElementType() == typeof(EventBean)) {
                        containedEventEval = new ContainedEventEvalEventBeanArrayForge(validatedExprNode.Forge);
                    }
                    else {
                        // check expression result type against eventtype expected underlying type
                        if (returnType.IsArray) {
                            if (!TypeHelper.IsSubclassOrImplementsInterface(
                                returnType.GetElementType(),
                                streamEventType.UnderlyingType)) {
                                throw new ExprValidationException(
                                    "Event type '" +
                                    streamEventType.Name +
                                    "' underlying type " +
                                    streamEventType.UnderlyingType.Name +
                                    " cannot be assigned a value of type " +
                                    TypeHelper.GetCleanName(returnType));
                            }
                        }
                        else if (returnType.IsGenericEnumerable()) {
                            // fine, assumed to return the right type
                        }
                        else {
                            throw new ExprValidationException(
                                "Return type of expression '" +
                                ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(atom.SplitterExpression) +
                                "' is '" +
                                returnType.Name +
                                "', expected an Iterable or array result");
                        }

                        containedEventEval = new ContainedEventEvalExprNodeForge(
                            validatedExprNode.Forge,
                            streamEventType);
                    }

                    expressionText = ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(validatedExprNode);
                    fragmentEventType = new FragmentEventType(streamEventType, true, false);
                }

                // validate where clause, if any
                streamEventTypes.Add(streamEventType);
                streamNames.Add(atom.OptionalAsName);
                streamNameAndNumber.Put(atom.OptionalAsName, i + 1);
                expressionTexts.Add(expressionText);

                if (atom.OptionalWhereClause != null) {
                    var whereTypes = streamEventTypes.ToArray();
                    var whereStreamNames = streamNames.ToArray();
                    var isIStreamOnly = new bool[streamNames.Count];
                    isIStreamOnly.Fill(true);
                    StreamTypeService streamTypeService = new StreamTypeServiceImpl(
                        whereTypes,
                        whereStreamNames,
                        isIStreamOnly,
                        false,
                        false);
                    var validationContext = new ExprValidationContextBuilder(streamTypeService, rawInfo, services)
                        .WithAllowBindingConsumption(true)
                        .Build();
                    var whereClause = ExprNodeUtilityValidate.GetValidatedSubtree(
                        ExprNodeOrigin.CONTAINEDEVENT,
                        atom.OptionalWhereClause,
                        validationContext);
                    whereClauses[i] = whereClause.Forge;
                }

                // validate select clause
                if (atom.OptionalSelectClause != null && !atom.OptionalSelectClause.SelectExprList.IsEmpty()) {
                    var whereTypes = streamEventTypes.ToArray();
                    var whereStreamNames = streamNames.ToArray();
                    var isIStreamOnly = new bool[streamNames.Count];
                    isIStreamOnly.Fill(true);
                    StreamTypeService streamTypeService = new StreamTypeServiceImpl(
                        whereTypes,
                        whereStreamNames,
                        isIStreamOnly,
                        false,
                        false);
                    var validationContext = new ExprValidationContextBuilder(streamTypeService, rawInfo, services)
                        .WithAllowBindingConsumption(true)
                        .Build();

                    foreach (var raw in atom.OptionalSelectClause.SelectExprList) {
                        if (raw is SelectClauseStreamRawSpec) {
                            var rawStreamSpec = (SelectClauseStreamRawSpec) raw;
                            if (!streamNames.Contains(rawStreamSpec.StreamName)) {
                                throw new ExprValidationException(
                                    "Property rename '" + rawStreamSpec.StreamName + "' not found in path");
                            }

                            var streamSpec = new SelectClauseStreamCompiledSpec(
                                rawStreamSpec.StreamName,
                                rawStreamSpec.OptionalAsName);
                            var streamNumber = streamNameAndNumber.Get(rawStreamSpec.StreamName);
                            streamSpec.StreamNumber = streamNumber;
                            cumulativeSelectClause.Add(streamSpec);
                        }
                        else if (raw is SelectClauseExprRawSpec) {
                            var exprSpec = (SelectClauseExprRawSpec) raw;
                            var exprCompiled = ExprNodeUtilityValidate.GetValidatedSubtree(
                                ExprNodeOrigin.CONTAINEDEVENT,
                                exprSpec.SelectExpression,
                                validationContext);
                            var resultName = exprSpec.OptionalAsName;
                            if (resultName == null) {
                                resultName = ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(exprCompiled);
                            }

                            cumulativeSelectClause.Add(
                                new SelectClauseExprCompiledSpec(
                                    exprCompiled,
                                    resultName,
                                    exprSpec.OptionalAsName,
                                    exprSpec.IsEvents));

                            var isMinimal = ExprNodeUtilityValidate.IsMinimalExpression(exprCompiled);
                            if (isMinimal != null) {
                                throw new ExprValidationException(
                                    "Expression in a property-selection may not utilize " + isMinimal);
                            }
                        }
                        else if (raw is SelectClauseElementWildcard) {
                            // wildcards are stream selects: we assign a stream name (any) and add a stream wildcard select
                            var streamNameAtom = atom.OptionalAsName;
                            if (streamNameAtom == null) {
                                streamNameAtom = UuidGenerator.Generate();
                            }

                            var streamSpec = new SelectClauseStreamCompiledSpec(streamNameAtom, atom.OptionalAsName);
                            var streamNumber = i + 1;
                            streamSpec.StreamNumber = streamNumber;
                            cumulativeSelectClause.Add(streamSpec);
                        }
                        else {
                            throw new IllegalStateException("Unknown select clause item:" + raw);
                        }
                    }
                }

                currentEventType = fragmentEventType.FragmentType;
                fragmentEventTypes[i] = fragmentEventType;
                containedEventForges[i] = containedEventEval;
            }

            if (cumulativeSelectClause.IsEmpty()) {
                if (length == 1) {
                    return new PropertyEvaluatorSimpleForge(
                        containedEventForges[0],
                        fragmentEventTypes[0],
                        whereClauses[0],
                        expressionTexts[0]);
                }

                return new PropertyEvaluatorNestedForge(
                    containedEventForges,
                    fragmentEventTypes,
                    whereClauses,
                    expressionTexts.ToArray());
            }

            {
                var fragmentEventTypeIsIndexed = new bool[fragmentEventTypes.Length];
                for (var i = 0; i < fragmentEventTypes.Length; i++) {
                    fragmentEventTypeIsIndexed[i] = fragmentEventTypes[i].IsIndexed;
                }

                var accumulative = new PropertyEvaluatorAccumulativeForge(
                    containedEventForges,
                    fragmentEventTypeIsIndexed,
                    whereClauses,
                    expressionTexts);

                var whereTypes = streamEventTypes.ToArray();
                var whereStreamNames = streamNames.ToArray();
                var isIStreamOnly = new bool[streamNames.Count];
                isIStreamOnly.Fill(true);
                StreamTypeService streamTypeService = new StreamTypeServiceImpl(
                    whereTypes,
                    whereStreamNames,
                    isIStreamOnly,
                    false,
                    false);

                var cumulativeSelectArr = cumulativeSelectClause.ToArray();
                var args = new SelectProcessorArgs(
                    cumulativeSelectArr,
                    null,
                    false,
                    null,
                    null,
                    streamTypeService,
                    null,
                    false,
                    rawInfo.Annotations,
                    rawInfo,
                    services);
                var selectExprDesc = SelectExprProcessorFactory.GetProcessor(args, null, false);

                return new PropertyEvaluatorSelectForge(selectExprDesc, accumulative);
            }
        }
    }
} // end of namespace