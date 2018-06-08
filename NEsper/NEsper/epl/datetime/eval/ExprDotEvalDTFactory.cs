///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.datetime.calop;
using com.espertech.esper.epl.datetime.interval;
using com.espertech.esper.epl.datetime.reformatop;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.dot;
using com.espertech.esper.epl.expression.time;
using com.espertech.esper.epl.@join.plan;
using com.espertech.esper.epl.methodbase;
using com.espertech.esper.epl.rettype;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.datetime.eval
{
    public class ExprDotEvalDTFactory
    {
        public static ExprDotEvalDTMethodDesc ValidateMake(
            StreamTypeService streamTypeService,
            Deque<ExprChainedSpec> chainSpecStack,
            DatetimeMethodEnum dtMethod,
            String dtMethodName,
            EPType inputType,
            IList<ExprNode> parameters,
            ExprDotNodeFilterAnalyzerInput inputDesc,
            TimeZoneInfo timeZone,
            TimeAbacus timeAbacus)
        {
            // verify input
            String message = "Date-time enumeration method '" + dtMethodName +
                             "' requires either a DateTime, DateTimeEx or long value as input or events of an event type that declares a timestamp property";
            if (inputType is EventEPType)
            {
                if (((EventEPType)inputType).EventType.StartTimestampPropertyName == null)
                {
                    throw new ExprValidationException(message);
                }
            }
            else
            {
                if (!(inputType is ClassEPType || inputType is NullEPType))
                {
                    throw new ExprValidationException(message + " but received " + inputType.ToTypeDescriptive());
                }
                if (inputType is ClassEPType)
                {
                    ClassEPType classEPType = (ClassEPType)inputType;
                    if (!TypeHelper.IsDateTime(classEPType.Clazz))
                    {
                        throw new ExprValidationException(
                            message + " but received " + classEPType.Clazz.GetCleanName());
                    }
                }
            }

            IList<CalendarOp> calendarOps = new List<CalendarOp>();
            ReformatOp reformatOp = null;
            IntervalOp intervalOp = null;
            DatetimeMethodEnum currentMethod = dtMethod;
            IList<ExprNode> currentParameters = parameters;
            String currentMethodName = dtMethodName;

            // drain all calendar ops
            FilterExprAnalyzerAffector filterAnalyzerDesc = null;
            while (true)
            {

                // handle the first one only if its a calendar op
                var evaluators = GetEvaluators(currentParameters);
                var opFactory = currentMethod.MetaData().OpFactory;

                // compile parameter abstract for validation against available footprints
                var footprintProvided = DotMethodUtil.GetProvidedFootprint(currentParameters);

                // validate parameters
                DotMethodUtil.ValidateParametersDetermineFootprint(
                    currentMethod.Footprints(),
                    DotMethodTypeEnum.DATETIME,
                    currentMethodName, footprintProvided,
                    DotMethodInputTypeMatcherImpl.DEFAULT_ALL);

                if (opFactory is CalendarOpFactory)
                {
                    CalendarOp calendarOp = ((CalendarOpFactory)opFactory).GetOp(currentMethod, currentMethodName, currentParameters, evaluators);
                    calendarOps.Add(calendarOp);
                }
                else if (opFactory is ReformatOpFactory)
                {
                    reformatOp = ((ReformatOpFactory)opFactory).GetOp(timeZone, timeAbacus, currentMethod, currentMethodName, currentParameters);

                    // compile filter analyzer information if there are no calendar ops in the chain
                    if (calendarOps.IsEmpty())
                    {
                        filterAnalyzerDesc = reformatOp.GetFilterDesc(streamTypeService.EventTypes, currentMethod, currentParameters, inputDesc);
                    }
                    else
                    {
                        filterAnalyzerDesc = null;
                    }
                }
                else if (opFactory is IntervalOpFactory)
                {
                    intervalOp = ((IntervalOpFactory)opFactory).GetOp(streamTypeService, currentMethod, currentMethodName, currentParameters, timeZone, timeAbacus);

                    // compile filter analyzer information if there are no calendar ops in the chain
                    if (calendarOps.IsEmpty())
                    {
                        filterAnalyzerDesc = intervalOp.GetFilterDesc(streamTypeService.EventTypes, currentMethod, currentParameters, inputDesc);
                    }
                    else
                    {
                        filterAnalyzerDesc = null;
                    }
                }
                else
                {
                    throw new IllegalStateException("Invalid op factory class " + opFactory);
                }

                // see if there is more
                if (chainSpecStack.IsEmpty() || !DatetimeMethodEnumExtensions.IsDateTimeMethod(chainSpecStack.First.Name))
                {
                    break;
                }

                // pull next
                var next = chainSpecStack.RemoveFirst();
                currentMethod = DatetimeMethodEnumExtensions.FromName(next.Name);
                currentParameters = next.Parameters;
                currentMethodName = next.Name;

                if ((reformatOp != null || intervalOp != null))
                {
                    throw new ExprValidationException("Invalid input for date-time method '" + next.Name + "'");
                }
            }

            ExprDotEval dotEval;
            EPType returnType;

            dotEval = new ExprDotEvalDT(
                calendarOps, timeZone, timeAbacus, reformatOp, intervalOp, 
                EPTypeHelper.GetClassSingleValued(inputType),
                EPTypeHelper.GetEventTypeSingleValued(inputType));
            returnType = dotEval.TypeInfo;
            return new ExprDotEvalDTMethodDesc(dotEval, returnType, filterAnalyzerDesc);
        }

        private static ExprEvaluator[] GetEvaluators(IList<ExprNode> parameters)
        {
            var inputExpr = new ExprEvaluator[parameters.Count];
            for (int i = 0; i < parameters.Count; i++)
            {
                ExprNode innerExpr = parameters[i];
                ExprEvaluator inner = innerExpr.ExprEvaluator;

                // Time periods get special attention
                if (innerExpr is ExprTimePeriod)
                {

                    var timePeriod = (ExprTimePeriod)innerExpr;
                    inputExpr[i] = new ProxyExprEvaluator
                    {
                        ProcEvaluate = evaluateParams => timePeriod.EvaluateGetTimePeriod(evaluateParams),
                        ReturnType = typeof(TimePeriod),
                    };
                }
                else
                {
                    inputExpr[i] = inner;
                }
            }
            return inputExpr;
        }
    }
}
