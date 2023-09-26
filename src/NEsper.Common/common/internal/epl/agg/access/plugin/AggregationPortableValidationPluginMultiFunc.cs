///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.configuration.compiler;
using com.espertech.esper.common.client.hook.aggmultifunc;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.common.@internal.util;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.agg.access.plugin
{
    public class AggregationPortableValidationPluginMultiFunc : AggregationPortableValidation
    {
        public string AggregationFunctionName { get; set; }
        public ConfigurationCompilerPlugInAggregationMultiFunction Config { get; set; }
        public AggregationMultiFunctionHandler Handler { get; set; }
        public bool ParametersValidated { get; set; }

        public void ValidateIntoTableCompatible(
            string tableExpression,
            AggregationPortableValidation intoTableAgg,
            string intoExpression,
            AggregationForgeFactory factory)
        {
            AggregationValidationUtil.ValidateAggregationType(this, tableExpression, intoTableAgg, intoExpression);
        }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            ModuleTableInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(AggregationPortableValidationPluginMultiFunc), GetType(), classScope);
            method.Block
                .DeclareVar<AggregationPortableValidationPluginMultiFunc>(
                    "portable",
                    NewInstance(typeof(AggregationPortableValidationPluginMultiFunc)))
                .SetProperty(Ref("portable"), "AggregationFunctionName", Constant(AggregationFunctionName))
                .SetProperty(Ref("portable"), "Config", Config == null ? ConstantNull() : Config.ToExpression())
                .MethodReturn(Ref("portable"));
            return LocalMethod(method);
        }


        public bool IsAggregationMethod(
            string name,
            ExprNode[] parameters,
            ExprValidationContext validationContext)
        {
            // always obtain a new handler since the name may have changes
            var configPair = validationContext.ImportService.ResolveAggregationMultiFunction(
                AggregationFunctionName,
                validationContext.ClassProvidedExtension);
            if (configPair == null) {
                return false;
            }

            AggregationMultiFunctionForge forge;
            if (configPair.Second != null) {
                forge = TypeHelper.Instantiate<AggregationMultiFunctionForge>(configPair.Second);
            }
            else {
                forge = TypeHelper.Instantiate<AggregationMultiFunctionForge>(
                    configPair.First.MultiFunctionForgeClassName,
                    validationContext.ImportService.TypeResolver);
            }

            ValidateParamsUnless(validationContext, parameters);

            var ctx = new AggregationMultiFunctionValidationContext(
                name,
                validationContext.StreamTypeService.EventTypes,
                parameters,
                validationContext.StatementName,
                validationContext,
                Config,
                parameters,
                null);

            Handler = forge.ValidateGetHandler(ctx);
            return Handler.GetAggregationMethodMode(
                       new AggregationMultiFunctionAggregationMethodContext(name, parameters, validationContext)) !=
                   null;
        }

        public AggregationMultiFunctionMethodDesc ValidateAggregationMethod(
            ExprValidationContext validationContext,
            string aggMethodName,
            ExprNode[] @params)
        {
            ValidateParamsUnless(validationContext, @params);

            // set of reader
            var epType = Handler.ReturnType;
            var returnType = epType.GetNormalizedType();
            if (returnType == null) {
                throw new ExprValidationException(
                    "Null-type value returned by aggregation function '" + aggMethodName + "' is not allowed");
            }

            var forge = new AggregationMethodForgePlugIn(
                returnType,
                (AggregationMultiFunctionAggregationMethodModeManaged)Handler.GetAggregationMethodMode(
                    new AggregationMultiFunctionAggregationMethodContext(aggMethodName, @params, validationContext)));
            var eventTypeCollection = epType.OptionalIsEventTypeColl();
            var eventTypeSingle = epType.OptionalIsEventTypeSingle();
            var componentTypeCollection = EPChainableTypeHelper.GetCollectionOrArrayComponentTypeOrNull(epType);
            //var componentTypeCollection = epType.OptionalIsComponentTypeColl();

            return new AggregationMultiFunctionMethodDesc(
                forge,
                eventTypeCollection,
                componentTypeCollection,
                eventTypeSingle);
        }


        private void ValidateParamsUnless(
            ExprValidationContext validationContext,
            ExprNode[] parameters)
        {
            if (ParametersValidated) {
                return;
            }

            ExprNodeUtilityValidate.GetValidatedSubtree(ExprNodeOrigin.AGGPARAM, parameters, validationContext);
            ParametersValidated = true;
        }
    }
} // end of namespace