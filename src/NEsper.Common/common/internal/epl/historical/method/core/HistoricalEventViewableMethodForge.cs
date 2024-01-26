///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.multikey;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.visitor;
using com.espertech.esper.common.@internal.epl.historical.common;
using com.espertech.esper.common.@internal.epl.historical.method.poll;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.historical.method.core
{
    public class HistoricalEventViewableMethodForge : HistoricalEventViewableForgeBase
    {
        private readonly MethodStreamSpec _methodStreamSpec;
        private readonly MethodPollingViewableMeta _metadata;

        private MethodTargetStrategyForge _target;
        private MethodConversionStrategyForge _conversion;

        public HistoricalEventViewableMethodForge(
            int streamNum,
            EventType eventType,
            MethodStreamSpec methodStreamSpec,
            MethodPollingViewableMeta metadata) : base(streamNum, eventType)
        {
            _methodStreamSpec = methodStreamSpec;
            _metadata = metadata;
        }

        public override IList<StmtClassForgeableFactory> Validate(
            StreamTypeService typeService,
            IDictionary<int, IList<ExprNode>> sqlParameters,
            StatementRawInfo rawInfo,
            StatementCompileTimeServices services)
        {
            // validate and visit
            var validationContext = new ExprValidationContextBuilder(typeService, rawInfo, services)
                .WithAllowBindingConsumption(true)
                .Build();

            var visitor = new ExprNodeIdentifierAndStreamRefVisitor(true);
            var validatedInputParameters = new List<ExprNode>();
            foreach (var exprNode in _methodStreamSpec.Expressions) {
                var validated = ExprNodeUtilityValidate.GetValidatedSubtree(
                    ExprNodeOrigin.METHODINVJOIN,
                    exprNode,
                    validationContext);
                validatedInputParameters.Add(validated);
                validated.Accept(visitor);
            }

            // determine required streams
            foreach (var @ref in visitor.Refs) {
                SubordinateStreams.Add(@ref.StreamNum);
            }

            // class-based evaluation
            MethodInfo targetMethod = null;
            if (_metadata.MethodProviderClass != null) {
                // resolve actual method to use
                ExprNodeUtilResolveExceptionHandler handler = new ProxyExprNodeUtilResolveExceptionHandler() {
                    ProcHandle = (e) => {
                        if (_methodStreamSpec.Expressions.Count == 0) {
                            return new ExprValidationException(
                                "Method footprint does not match the number or type of expression parameters, expecting no parameters in method: " +
                                e.Message);
                        }

                        var resultTypes = ExprNodeUtilityQuery.GetExprResultTypes(validatedInputParameters);
                        return new ExprValidationException(
                            "Method footprint does not match the number or type of expression parameters, expecting a method where parameters are typed '" +
                            TypeHelper.GetParameterAsString(resultTypes) +
                            "': " +
                            e.Message);
                    }
                };
                var desc = ExprNodeUtilityResolve.ResolveMethodAllowWildcardAndStream(
                    _metadata.MethodProviderClass.FullName,
                    _metadata.IsStaticMethod
                        ? null
                        : _metadata.MethodProviderClass,
                    _methodStreamSpec.MethodName,
                    validatedInputParameters,
                    false,
                    null,
                    handler,
                    _methodStreamSpec.MethodName,
                    rawInfo,
                    services);
                InputParamEvaluators = desc.ChildForges;
                targetMethod = desc.ReflectionMethod;
            }
            else {
                // script-based evaluation
                InputParamEvaluators = ExprNodeUtilityQuery.GetForges(ExprNodeUtilityQuery.ToArray(validatedInputParameters));
            }

            // plan multikey
            var multiKeyPlan = MultiKeyPlanner.PlanMultiKey(
                InputParamEvaluators,
                false,
                rawInfo,
                services.SerdeResolver);
            MultiKeyClassRef = multiKeyPlan.ClassRef;

            var strategies = PollExecStrategyPlanner.Plan(_metadata, targetMethod, EventType);
            _target = strategies.First;
            _conversion = strategies.Second;

            return multiKeyPlan.MultiKeyForgeables;
        }

        public override Type TypeOfImplementation()
        {
            return typeof(HistoricalEventViewableMethodFactory);
        }

        public override void CodegenSetter(
            CodegenExpressionRef @ref,
            CodegenMethod method,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var configName = _metadata.GetConfigurationName(_methodStreamSpec);
            method.Block
                .SetProperty(@ref, "ConfigurationName", Constant(configName))
                .SetProperty(@ref, "TargetStrategy", _target.Make(method, symbols, classScope))
                .SetProperty(@ref, "ConversionStrategy", _conversion.Make(method, symbols, classScope));
        }
    }
} // end of namespace