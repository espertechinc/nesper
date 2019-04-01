///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.epl.datetime.calop;
using com.espertech.esper.common.@internal.epl.datetime.interval;
using com.espertech.esper.common.@internal.epl.datetime.reformatop;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.dot.core;
using com.espertech.esper.common.@internal.epl.expression.time.abacus;
using com.espertech.esper.common.@internal.epl.expression.time.node;
using com.espertech.esper.common.@internal.epl.@join.analyze;
using com.espertech.esper.common.@internal.epl.methodbase;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.datetime.eval
{
    public class ExprDotDTFactory
    {
        public static ExprDotDTMethodDesc ValidateMake(
            StreamTypeService streamTypeService,
            Deque<ExprChainedSpec> chainSpecStack,
            DatetimeMethodEnum dtMethod,
            string dtMethodName,
            EPType inputType,
            IList<ExprNode> parameters,
            ExprDotNodeFilterAnalyzerInput inputDesc,
            TimeAbacus timeAbacus,
            ExprEvaluatorContext exprEvaluatorContext,
            TableCompileTimeResolver tableCompileTimeResolver)
        {
            // verify input
            var message = "Date-time enumeration method '" + dtMethodName +
                          "' requires either a DateTimeEx, Date, long, DateTimeOffset or DateTime value as input or events of an event type that declares a timestamp property";
            if (inputType is EventEPType) {
                if (((EventEPType) inputType).EventType.StartTimestampPropertyName == null) {
                    throw new ExprValidationException(message);
                }
            }
            else {
                if (!(inputType is ClassEPType || inputType is NullEPType)) {
                    throw new ExprValidationException(
                        message + " but received " + inputType.ToTypeDescriptive());
                }

                if (inputType is ClassEPType) {
                    var classEPType = (ClassEPType) inputType;
                    if (!TypeHelper.IsDatetimeClass(classEPType.Clazz)) {
                        throw new ExprValidationException(
                            message + " but received " + TypeHelper.GetClassNameFullyQualPretty(classEPType.Clazz));
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
                DotMethodFPProvided footprintProvided = DotMethodUtil.GetProvidedFootprint(currentParameters);

                // validate parameters
                DotMethodUtil.ValidateParametersDetermineFootprint(
                    currentMethod.Footprints, DotMethodTypeEnum.DATETIME, currentMethodName, footprintProvided,
                    DotMethodInputTypeMatcher.DEFAULT_ALL);

                if (opFactory is CalendarForgeFactory) {
                    var calendarForge = ((CalendarForgeFactory) currentMethod.ForgeFactory).GetOp(
                        currentMethod, currentMethodName, currentParameters, forges);
                    calendarForges.Add(calendarForge);
                }
                else if (opFactory is ReformatForgeFactory) {
                    reformatForge = ((ReformatForgeFactory) opFactory).GetForge(
                        inputType, timeAbacus, currentMethod, currentMethodName, currentParameters,
                        exprEvaluatorContext);

                    // compile filter analyzer information if there are no calendar op in the chain
                    if (calendarForges.IsEmpty()) {
                        filterAnalyzerDesc = reformatForge.GetFilterDesc(
                            streamTypeService.EventTypes, currentMethod, currentParameters, inputDesc);
                    }
                    else {
                        filterAnalyzerDesc = null;
                    }
                }
                else if (opFactory is IntervalForgeFactory) {
                    intervalForge = ((IntervalForgeFactory) opFactory).GetForge(
                        streamTypeService, currentMethod, currentMethodName, currentParameters, timeAbacus,
                        tableCompileTimeResolver);

                    // compile filter analyzer information if there are no calendar op in the chain
                    if (calendarForges.IsEmpty()) {
                        filterAnalyzerDesc = intervalForge.GetFilterDesc(
                            streamTypeService.EventTypes, currentMethod, currentParameters, inputDesc);
                    }
                    else {
                        filterAnalyzerDesc = null;
                    }
                }
                else {
                    throw new IllegalStateException("Invalid op factory class " + opFactory);
                }

                // see if there is more
                if (chainSpecStack.IsEmpty() || !DatetimeMethodEnum.IsDateTimeMethod(chainSpecStack.First.Name)) {
                    break;
                }

                // pull next
                var next = chainSpecStack.RemoveFirst();
                currentMethod = DatetimeMethodEnum.FromName(next.Name);
                currentParameters = next.Parameters;
                currentMethodName = next.Name;

                if (reformatForge != null || intervalForge != null) {
                    throw new ExprValidationException("Invalid input for date-time method '" + next.Name + "'");
                }
            }

            ExprDotForge dotForge;
            EPType returnType;

            dotForge = new ExprDotDTForge(
                calendarForges, timeAbacus, reformatForge, intervalForge, inputType.GetClassSingleValued(),
                inputType.GetEventTypeSingleValued());
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
                if (innerExpr is ExprTimePeriod) {
                    var timePeriod = (ExprTimePeriod) innerExpr;
                    inputExpr[i] = new ProxyExprForge {
                        ProcExprEvaluator = () => {
                            return new ProxyExprEvaluator {
                                ProcEvaluate = (eventsPerStream, isNewData, context) =>
                                    timePeriod.EvaluateGetTimePeriod(eventsPerStream, isNewData, context)
                            };
                        },
                        ProcForgeConstantType = () => ExprForgeConstantType.NONCONST,
                        ProcEvaluateCodegen = (_, codegenMethodScope, exprSymbol, codegenClassScope) =>
                            timePeriod.EvaluateGetTimePeriodCodegen(codegenMethodScope, exprSymbol, codegenClassScope),
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