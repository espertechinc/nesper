///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.util;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.pattern.observer
{
    /// <summary>
    ///     Factory for 'crontab' observers that indicate truth when a time point was reached.
    /// </summary>
    public class TimerAtObserverForge : ObserverForge,
        ScheduleHandleCallbackProvider
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        
        private MatchedEventConvertorForge convertor;
        private IList<ExprNode> parameters;
        private int scheduleCallbackId = -1;
        private ScheduleSpec spec;

        public void SetObserverParameters(
            IList<ExprNode> parameters,
            MatchedEventConvertorForge convertor,
            ExprValidationContext validationContext)
        {
            ObserverParameterUtil.ValidateNoNamedParameters("timer:at", parameters);
            if (Log.IsDebugEnabled) {
                Log.Debug(".setObserverParameters " + parameters);
            }

            if (parameters.Count < 5 || parameters.Count > 9) {
                throw new ObserverParameterException("Invalid number of parameters for timer:at");
            }

            this.parameters = parameters;
            this.convertor = convertor;

            // if all parameters are constants, lets try to evaluate and build a schedule for early validation
            var allConstantResult = true;
            foreach (var param in parameters) {
                if (!(param is ExprWildcard) && !param.Forge.ForgeConstantType.IsCompileTimeConstant) {
                    allConstantResult = false;
                }
            }

            if (allConstantResult) {
                try {
                    var observerParameters = EvaluateCompileTime(parameters);
                    spec = ScheduleSpecUtil.ComputeValues(observerParameters.ToArray());
                }
                catch (ScheduleParameterException e) {
                    throw new ObserverParameterException(
                        "Error computing crontab schedule specification: " + e.Message,
                        e);
                }
            }
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
                typeof(TimerAtObserverFactory),
                typeof(TimerIntervalObserverForge),
                classScope);

            CodegenExpression parametersExpr;
            CodegenExpression optionalConvertorExpr;
            CodegenExpression specExpr;
            if (spec != null) { // handle all-constant specification
                parametersExpr = ConstantNull();
                optionalConvertorExpr = ConstantNull();
                specExpr = spec.Make(method, classScope);
            }
            else {
                specExpr = ConstantNull();
                optionalConvertorExpr = convertor.MakeAnonymous(method, classScope);
                parametersExpr = ExprNodeUtilityCodegen.CodegenEvaluators(
                    ExprNodeUtilityQuery.ToArray(parameters),
                    method,
                    GetType(),
                    classScope);
            }

            method.Block
                .DeclareVar<TimerAtObserverFactory>(
                    "factory",
                    ExprDotMethodChain(symbols.GetAddInitSvc(method))
                        .Get(EPStatementInitServicesConstants.PATTERNFACTORYSERVICE)
                        .Add("ObserverTimerAt"))
                .SetProperty(Ref("factory"), "ScheduleCallbackId", Constant(scheduleCallbackId))
                .SetProperty(Ref("factory"), "Parameters", parametersExpr)
                .SetProperty(Ref("factory"), "OptionalConvertor", optionalConvertorExpr)
                .SetProperty(Ref("factory"), "Spec", specExpr)
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

        public int ScheduleCallbackId {
            get => scheduleCallbackId;
            set => scheduleCallbackId = value;
        }

        private static IList<object> EvaluateCompileTime(IList<ExprNode> parameters)
        {
            IList<object> results = new List<object>();
            var count = 0;
            foreach (var expr in parameters) {
                try {
                    var result = expr.Forge.ExprEvaluator.Evaluate(null, true, null);
                    results.Add(result);
                    count++;
                }
                catch (EPException) {
                    throw;
                }
                catch (Exception ex) {
                    var message = "Tmer-at observer invalid parameter in expression " + count;
                    if (ex.Message != null) {
                        message += ": " + ex.Message;
                    }

                    Log.Error(message, ex);
                    throw new EPException(message);
                }
            }

            return results;
        }
    }
} // end of namespace