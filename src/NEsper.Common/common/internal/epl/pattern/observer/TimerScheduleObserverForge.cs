///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.util;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.time.node;
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.pattern.observer
{
    /// <summary>
    ///     Factory for ISO8601 repeating interval observers that indicate truth when a time point was reached.
    /// </summary>
    public class TimerScheduleObserverForge : ObserverForge,
        ScheduleHandleCallbackProvider
    {
        public const string NAME_OBSERVER = "Timer-schedule observer";

        private const string ISO_NAME = "iso";
        private const string REPETITIONS_NAME = "repetitions";
        private const string DATE_NAME = "date";
        private const string PERIOD_NAME = "period";
        private static readonly string[] NAMED_PARAMETERS = { ISO_NAME, REPETITIONS_NAME, DATE_NAME, PERIOD_NAME };
        private bool allConstantResult;
        private MatchedEventConvertorForge convertor;
        private int scheduleCallbackId = -1;

        private TimerScheduleSpecComputeForge scheduleComputer;

        public void SetObserverParameters(
            IList<ExprNode> parameters,
            MatchedEventConvertorForge convertor,
            ExprValidationContext validationContext)
        {
            this.convertor = convertor;

            // obtains name parameters
            IDictionary<string, ExprNamedParameterNode> namedExpressions;
            try {
                namedExpressions = ExprNodeUtilityValidate.GetNamedExpressionsHandleDups(parameters);
                ExprNodeUtilityValidate.ValidateNamed(namedExpressions, NAMED_PARAMETERS);
            }
            catch (ExprValidationException e) {
                throw new ObserverParameterException(e.Message, e);
            }

            var isoStringExpr = namedExpressions.Get(ISO_NAME);
            if (namedExpressions.Count == 1 && isoStringExpr != null) {
                try {
                    allConstantResult = ExprNodeUtilityValidate.ValidateNamedExpectType(
                        isoStringExpr,
                        new[] { typeof(string) });
                }
                catch (ExprValidationException ex) {
                    throw new ObserverParameterException(ex.Message, ex);
                }

                scheduleComputer = new TimerScheduleSpecComputeISOStringForge(isoStringExpr.ChildNodes[0]);
            }
            else if (isoStringExpr != null) {
                throw new ObserverParameterException(
                    "The '" + ISO_NAME + "' parameter is exclusive of other parameters");
            }
            else if (namedExpressions.Count == 0) {
                throw new ObserverParameterException("No parameters provided");
            }
            else {
                allConstantResult = true;
                var dateNamedNode = namedExpressions.Get(DATE_NAME);
                var repetitionsNamedNode = namedExpressions.Get(REPETITIONS_NAME);
                var periodNamedNode = namedExpressions.Get(PERIOD_NAME);
                if (dateNamedNode == null && periodNamedNode == null) {
                    throw new ObserverParameterException("Either the date or period parameter is required");
                }

                try {
                    if (dateNamedNode != null) {
                        allConstantResult = ExprNodeUtilityValidate.ValidateNamedExpectType(
                            dateNamedNode,
                            new[] {
                                typeof(string),
                                typeof(DateTimeEx),
                                typeof(DateTimeOffset),
                                typeof(DateTime),
                                typeof(long)
                            });
                    }

                    if (repetitionsNamedNode != null) {
                        allConstantResult &= ExprNodeUtilityValidate.ValidateNamedExpectType(
                            repetitionsNamedNode,
                            new[] { typeof(int), typeof(long) });
                    }

                    if (periodNamedNode != null) {
                        allConstantResult &= ExprNodeUtilityValidate.ValidateNamedExpectType(
                            periodNamedNode,
                            new[] { typeof(TimePeriod) });
                    }
                }
                catch (ExprValidationException ex) {
                    throw new ObserverParameterException(ex.Message, ex);
                }

                var dateNode = dateNamedNode?.ChildNodes[0];
                var repetitionsNode = repetitionsNamedNode?.ChildNodes[0];
                var periodNode = (ExprTimePeriod)periodNamedNode?.ChildNodes[0];
                scheduleComputer = new TimerScheduleSpecComputeFromExprForge(dateNode, repetitionsNode, periodNode);
            }

            if (allConstantResult) {
                try {
                    scheduleComputer.VerifyComputeAllConst(validationContext);
                }
                catch (ScheduleParameterException ex) {
                    throw new ObserverParameterException(ex.Message, ex);
                }
            }
        }

        public int ScheduleCallbackId {
            get => scheduleCallbackId;
            set => scheduleCallbackId = value;
        }

        public CodegenExpression MakeCodegen(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            if (scheduleCallbackId == -1) {
                throw new IllegalStateException("Unassigned schedule callback id");
            }

            var method = parent.MakeChild(
                typeof(TimerScheduleObserverFactory),
                typeof(TimerIntervalObserverForge),
                classScope);

            method.Block
                .DeclareVar<TimerScheduleObserverFactory>(
                    "factory",
                    ExprDotMethodChain(symbols.GetAddInitSvc(method))
                        .Get(EPStatementInitServicesConstants.PATTERNFACTORYSERVICE)
                        .Add("ObserverTimerSchedule"))
                .SetProperty(Ref("factory"), "ScheduleCallbackId", Constant(scheduleCallbackId))
                .SetProperty(Ref("factory"), "IsAllConstant", Constant(allConstantResult))
                .SetProperty(Ref("factory"), "ScheduleComputer", scheduleComputer.Make(method, classScope))
                .SetProperty(
                    Ref("factory"),
                    "OptionalConvertor",
                    convertor?.MakeAnonymous(method, classScope))
                .MethodReturn(Ref("factory"));
            return LocalMethod(method);
        }

        public void CollectSchedule(
            short factoryNodeId,
            Func<short, CallbackAttribution> callbackAttribution,
            IList<ScheduleHandleTracked> schedules)
        {
            schedules.Add(new ScheduleHandleTracked(callbackAttribution.Invoke(factoryNodeId), this));
        }
    }
} // end of namespace