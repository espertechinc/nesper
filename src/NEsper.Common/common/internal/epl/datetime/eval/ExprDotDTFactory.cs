///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.hook.datetimemethod;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.epl.datetime.calop;
using com.espertech.esper.common.@internal.epl.datetime.interval;
using com.espertech.esper.common.@internal.epl.datetime.plugin;
using com.espertech.esper.common.@internal.epl.datetime.reformatop;
using com.espertech.esper.common.@internal.epl.expression.chain;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.dot.core;
using com.espertech.esper.common.@internal.epl.expression.time.abacus;
using com.espertech.esper.common.@internal.epl.expression.time.node;
using com.espertech.esper.common.@internal.epl.join.analyze;
using com.espertech.esper.common.@internal.epl.methodbase;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;


namespace com.espertech.esper.common.@internal.epl.datetime.eval
{
    public class ExprDotDTFactory
    {
        public static ExprDotDTMethodDesc ValidateMake(
            StreamTypeService streamTypeService,
            Deque<Chainable> chainSpecStack,
            DatetimeMethodDesc dtMethod,
            string dtMethodName,
            EPChainableType inputType,
            IList<ExprNode> parameters,
            ExprDotNodeFilterAnalyzerInput inputDesc,
            TimeAbacus timeAbacus,
            TableCompileTimeResolver tableCompileTimeResolver,
            ImportServiceCompileTime importService,
            StatementRawInfo statementRawInfo)
        {
            // verify input
            var message = $"Date-time enumeration method '{dtMethodName}' requires either a DateTimeEx, DateTimeOffset, DateTime, or long value as input or events of an event type that declares a timestamp property";
            if (inputType is EPChainableTypeEventSingle single) {
                if (single.EventType.StartTimestampPropertyName == null) {
                    throw new ExprValidationException(message);
                }
            }
            else {
                if (!(inputType is EPChainableTypeClass || inputType is EPChainableTypeNull)) {
                    throw new ExprValidationException(
                        message + " but received " + inputType.ToTypeDescriptive());
                }

                if (inputType is EPChainableTypeClass classEpType) {
                    if (!TypeHelper.IsDateTime(classEpType.Clazz)) {
                        throw new ExprValidationException(
                            $"{message} but received {classEpType.Clazz.CleanName()}");
                    }
                }
            }

            IList<CalendarForge> calendarForges = new List<CalendarForge>();
            ReformatForge reformatForge = null;
            IntervalForge intervalForge = null;
            var currentMethod = dtMethod;
            var currentParameters = parameters;
            var currentMethodName = dtMethodName;

            // drain all calendar op
            FilterExprAnalyzerAffector filterAnalyzerDesc = null;
            while (true) {
                // handle the first one only if its a calendar op
                var forges = GetForges(currentParameters);
                var opFactory = currentMethod.ForgeFactory;

                // compile parameter abstract for validation against available footprints
                var footprintProvided = DotMethodUtil.GetProvidedFootprint(currentParameters);

                // validate parameters
                var footprintFound = DotMethodUtil.ValidateParametersDetermineFootprint(
                    currentMethod.Footprints,
                    DotMethodTypeEnum.DATETIME,
                    currentMethodName,
                    footprintProvided,
                    DotMethodInputTypeMatcher.DEFAULT_ALL);

                if (opFactory is CalendarForgeFactory) {
                    var calendarForge = ((CalendarForgeFactory)currentMethod.ForgeFactory).GetOp(
                        currentMethod,
                        currentMethodName,
                        currentParameters,
                        forges);
                    calendarForges.Add(calendarForge);
                }
                else if (opFactory is ReformatForgeFactory factory) {
                    reformatForge = factory.GetForge(
                        inputType,
                        timeAbacus,
                        currentMethod,
                        currentMethodName,
                        currentParameters);

                    // compile filter analyzer information if there are no calendar op in the chain
                    if (calendarForges.IsEmpty()) {
                        filterAnalyzerDesc = reformatForge.GetFilterDesc(
                            streamTypeService.EventTypes,
                            currentMethod,
                            currentParameters,
                            inputDesc);
                    }
                    else {
                        filterAnalyzerDesc = null;
                    }
                }
                else if (opFactory is IntervalForgeFactory forgeFactory) {
                    intervalForge = forgeFactory.GetForge(
                        streamTypeService,
                        currentMethod,
                        currentMethodName,
                        currentParameters,
                        timeAbacus,
                        tableCompileTimeResolver);

                    // compile filter analyzer information if there are no calendar op in the chain
                    if (calendarForges.IsEmpty()) {
                        filterAnalyzerDesc = intervalForge.GetFilterDesc(
                            streamTypeService.EventTypes,
                            currentMethod,
                            currentParameters,
                            inputDesc);
                    }
                    else {
                        filterAnalyzerDesc = null;
                    }
                }
                else if (opFactory is DTMPluginForgeFactory plugIn) {
                    var usageDesc = new DateTimeMethodValidateContext(
                        footprintFound,
                        streamTypeService,
                        currentMethod,
                        currentParameters,
                        statementRawInfo);
                    var ops = plugIn.Validate(usageDesc);
                    if (ops == null) {
                        throw new ExprValidationException(
                            "Plug-in datetime method provider " +
                            plugIn.GetType() +
                            " returned a null-value for the operations");
                    }

                    var input = EPChainableTypeClass.FromInputOrNull(inputType);
                    if (ops is DateTimeMethodOpsModify modify) {
                        calendarForges.Add(new DTMPluginValueChangeForge(input, modify, usageDesc.CurrentParameters));
                    }
                    else if (ops is DateTimeMethodOpsReformat reformat) {
                        reformatForge = new DTMPluginReformatForge(input, reformat, usageDesc.CurrentParameters);
                    }
                    else {
                        throw new ExprValidationException(
                            "Plug-in datetime method ops " + ops.GetType() + " is not recognized");
                    }
                    // no action
                }
                else {
                    throw new IllegalStateException("Invalid op factory class " + opFactory);
                }

                // see if there is more
                if (chainSpecStack.IsEmpty() ||
                    !DatetimeMethodResolver.IsDateTimeMethod(
                        chainSpecStack.First.RootNameOrEmptyString,
                        importService)) {
                    break;
                }

                // pull next
                var next = chainSpecStack.RemoveFirst();
                currentMethodName = next.RootNameOrEmptyString;
                currentMethod = DatetimeMethodResolver.FromName(currentMethodName, importService);
                currentParameters = next.ParametersOrEmpty;

                if (reformatForge != null || intervalForge != null) {
                    throw new ExprValidationException("Invalid input for date-time method '" + currentMethodName + "'");
                }
            }

            ExprDotForge dotForge;
            EPChainableType returnType;

            var inputTypeClass = EPChainableTypeClass.FromInputOrNull(inputType);
            var inputEventType = EPChainableTypeEventSingle.FromInputOrNull(inputType);
            dotForge = new ExprDotDTForge(
                calendarForges,
                timeAbacus,
                reformatForge,
                intervalForge,
                inputTypeClass,
                inputEventType);
            returnType = dotForge.TypeInfo;
            return new ExprDotDTMethodDesc(dotForge, returnType, filterAnalyzerDesc);
        }

        private static ExprForge[] GetForges(IList<ExprNode> parameters)
        {
            var inputExpr = new ExprForge[parameters.Count];
            for (var i = 0; i < parameters.Count; i++) {
                var innerExpr = parameters[i];
                var inner = innerExpr.Forge;

                // Time periods get special attention
                if (innerExpr is ExprTimePeriod timePeriod) {
                    inputExpr[i] = new ProxyExprForge() {
                        ProcExprEvaluator = () => {
                            return new ProxyExprEvaluator() {
                                ProcEvaluate = (
                                    eventsPerStream,
                                    isNewData,
                                    context) => timePeriod.EvaluateGetTimePeriod(eventsPerStream, isNewData, context)
                            };
                        },

                        ProcForgeConstantType = () => ExprForgeConstantType.NONCONST,
                        ProcEvaluateCodegen = (
                            requiredType,
                            codegenMethodScope,
                            exprSymbol,
                            codegenClassScope) => timePeriod.EvaluateGetTimePeriodCodegen(
                            codegenMethodScope,
                            exprSymbol,
                            codegenClassScope),

                        ProcEvaluationType = () => typeof(TimePeriod),
                        ProcForgeRenderable = () => timePeriod
                    };
                }
                else {
                    inputExpr[i] = inner;
                }
            }

            return inputExpr;
        }
    }
} // end of namespace