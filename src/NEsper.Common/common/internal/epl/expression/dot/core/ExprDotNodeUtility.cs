///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.datetime.eval;
using com.espertech.esper.common.@internal.epl.enummethod.dot;
using com.espertech.esper.common.@internal.epl.expression.chain;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.join.analyze;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.metrics.instrumentation.InstrumentationCode;

namespace com.espertech.esper.common.@internal.epl.expression.dot.core
{
    public class ExprDotNodeUtility
    {
        public static ObjectArrayEventType MakeTransientOAType(
            string enumMethod,
            string propertyName,
            Type type,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services)
        {
            IDictionary<string, object> propsResult = new Dictionary<string, object>();
            propsResult.Put(propertyName, type.GetBoxedType());
            return MakeTransientOATypeInternal(enumMethod, propsResult, propertyName, statementRawInfo, services);
        }

        public static ObjectArrayEventType MakeTransientOAType(
            string enumMethod,
            IDictionary<string, object> boxedPropertyTypes,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services)
        {
            return MakeTransientOATypeInternal(
                enumMethod,
                boxedPropertyTypes,
                CodeGenerationIDGenerator.GenerateClassNameUUID(),
                statementRawInfo,
                services);
        }

        private static ObjectArrayEventType MakeTransientOATypeInternal(
            string enumMethod,
            IDictionary<string, object> boxedPropertyTypes,
            string eventTypeNameUUid,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services)
        {
            var eventTypeName =
                services.EventTypeNameGeneratorStatement.GetAnonymousTypeNameEnumMethod(enumMethod, eventTypeNameUUid);
            var metadata = new EventTypeMetadata(
                eventTypeName,
                statementRawInfo.ModuleName,
                EventTypeTypeClass.ENUMDERIVED,
                EventTypeApplicationType.OBJECTARR,
                NameAccessModifier.TRANSIENT,
                EventTypeBusModifier.NONBUS,
                false,
                EventTypeIdPair.Unassigned());
            var oatype = BaseNestableEventUtil.MakeOATypeCompileTime(
                metadata,
                boxedPropertyTypes,
                null,
                null,
                null,
                null,
                services.BeanEventTypeFactoryPrivate,
                services.EventTypeCompileTimeResolver);
            services.EventTypeCompileTimeRegistry.NewType(oatype);
            return oatype;
        }

        public static bool IsDatetimeOrEnumMethod(
            string name,
            ImportServiceCompileTime importService)
        {
            return EnumMethodResolver.IsEnumerationMethod(name, importService) ||
                   DatetimeMethodResolver.IsDateTimeMethod(name, importService);
        }

        public static ExprDotEnumerationSourceForge GetEnumerationSource(
            ExprNode inputExpression,
            StreamTypeService streamTypeService,
            bool hasEnumerationMethod,
            bool disablePropertyExpressionEventCollCache,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices compileTimeServices)
        {
            var rootNodeForge = inputExpression.Forge;
            ExprEnumerationForge rootLambdaForge = null;
            EPChainableType info = null;

            if (rootNodeForge is ExprEnumerationForge forge) {
                rootLambdaForge = forge;
                var eventTypeCollection =
                    rootLambdaForge.GetEventTypeCollection(statementRawInfo, compileTimeServices);
                if (eventTypeCollection != null) {
                    info = EPChainableTypeHelper.CollectionOfEvents(eventTypeCollection);
                }

                if (info == null) {
                    var eventTypeSingle =
                        rootLambdaForge.GetEventTypeSingle(statementRawInfo, compileTimeServices);
                    if (eventTypeSingle != null) {
                        info = EPChainableTypeHelper.SingleEvent(eventTypeSingle);
                    }
                }

                if (info == null) {
                    var componentType = rootLambdaForge.ComponentTypeCollection == null
                        ? null
                        : rootLambdaForge.ComponentTypeCollection;
                    if (componentType != null) {
                        info = EPChainableTypeHelper.CollectionOfSingleValue(rootLambdaForge.ComponentTypeCollection);
                    }
                }

                if (info == null) {
                    rootLambdaForge = null; // not a lambda evaluator
                }
            }
            else if (inputExpression is ExprIdentNode identNode) {
                var streamId = identNode.StreamId;
                var streamType = streamTypeService.EventTypes[streamId];
                return GetPropertyEnumerationSource(
                    identNode.ResolvedPropertyName,
                    streamId,
                    streamType,
                    hasEnumerationMethod,
                    disablePropertyExpressionEventCollCache);
            }

            return new ExprDotEnumerationSourceForge(info, null, rootLambdaForge);
        }

        public static ExprDotEnumerationSourceForgeForProps GetPropertyEnumerationSource(
            string propertyName,
            int streamId,
            EventType streamType,
            bool allowEnumType,
            bool disablePropertyExpressionEventCollCache)
        {
            var propertyType = streamType.GetPropertyType(propertyName);
            var typeInfo = EPChainableTypeHelper.SingleValue(propertyType); // assume scalar for now

            // no enumeration methods, no need to expose as an enumeration
            if (!allowEnumType) {
                return new ExprDotEnumerationSourceForgeForProps(null, typeInfo, streamId, null);
            }

            var fragmentEventType = streamType.GetFragmentType(propertyName);
            var getter = ((EventTypeSPI)streamType).GetGetterSPI(propertyName);

            ExprEnumerationForge enumEvaluator = null;
            if (getter != null && fragmentEventType != null) {
                if (fragmentEventType.IsIndexed) {
                    enumEvaluator = new PropertyDotEventCollectionForge(
                        propertyName,
                        streamId,
                        fragmentEventType.FragmentType,
                        getter,
                        disablePropertyExpressionEventCollCache);
                    typeInfo = EPChainableTypeHelper.CollectionOfEvents(fragmentEventType.FragmentType);
                }
                else { // we don't want native to require an eventbean instance
                    enumEvaluator = new PropertyDotEventSingleForge(streamId, fragmentEventType.FragmentType, getter);
                    typeInfo = EPChainableTypeHelper.SingleEvent(fragmentEventType.FragmentType);
                }
            }
            else {
                var desc = EventTypeUtility.GetNestablePropertyDescriptor(streamType, propertyName);
                if (desc != null && desc.IsIndexed && !desc.IsRequiresIndex && desc.PropertyComponentType != null) {
                    if (propertyType == typeof(string)) {
                        enumEvaluator = new PropertyDotScalarStringForge(
                            propertyName,
                            streamId,
                            getter);
                    }
                    else if (propertyType.IsArray) {
                        enumEvaluator = new PropertyDotScalarArrayForge(
                            propertyName,
                            streamId,
                            getter,
                            desc.PropertyComponentType,
                            desc.PropertyType);
                    }
					else if (propertyType.IsGenericCollection()) {
                        enumEvaluator = new PropertyDotScalarCollection(
                            propertyName,
                            streamId,
                            getter,
                            desc.PropertyComponentType);
                    }
					else if (propertyType.IsGenericEnumerable()) {
                        enumEvaluator = new PropertyDotScalarIterable(
                            propertyName,
                            streamId,
                            getter,
                            desc.PropertyComponentType,
                            propertyType);
                    }
                    else {
                        throw new IllegalStateException(
                            "Property indicated indexed-type but failed to find proper collection adapter for use with enumeration methods");
                    }

                    typeInfo = EPChainableTypeHelper.CollectionOfSingleValue(desc.PropertyComponentType);
                }
            }

            return new ExprDotEnumerationSourceForgeForProps(
                enumEvaluator,
                typeInfo,
                streamId,
                (ExprEnumerationGivenEventForge)enumEvaluator);
        }

        public static ExprDotEval[] GetEvaluators(ExprDotForge[] forges)
        {
            var evals = new ExprDotEval[forges.Length];
            for (var i = 0; i < forges.Length; i++) {
                evals[i] = forges[i].DotEvaluator;
            }

            return evals;
        }

        public static object EvaluateChain(
            ExprDotForge[] forges,
            ExprDotEval[] evaluators,
            object inner,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            foreach (var methodEval in evaluators) {
                inner = methodEval.Evaluate(inner, eventsPerStream, isNewData, context);
                if (inner == null) {
                    break;
                }
            }

            return inner;
        }

        public static ExprDotNodeRealizedChain GetChainEvaluators(
            int? streamOfProviderIfApplicable,
            EPChainableType inputType,
            IList<Chainable> chainSpec,
            ExprValidationContext validationContext,
            bool isDuckTyping,
            ExprDotNodeFilterAnalyzerInput inputDesc)
        {
            var methodForges = new List<ExprDotForge>();
            var currentInputType = inputType;
            EnumMethodDesc lastLambdaFunc = null;
            var lastElement = chainSpec.IsEmpty() ? null : chainSpec[^1];
            FilterExprAnalyzerAffector filterAnalyzerDesc = null;

            Deque<Chainable> chainSpecStack = new ArrayDeque<Chainable>(chainSpec);
            while (!chainSpecStack.IsEmpty()) {
                var chainElement = chainSpecStack.RemoveFirst();
                var parameters = chainElement.ParametersOrEmpty;
                var chainElementName = chainElement.RootNameOrEmptyString;
                var last = chainSpecStack.IsEmpty();
                lastLambdaFunc = null; // reset

                // compile parameters for chain element
                var paramForges = new ExprForge[parameters.Count];
                var paramTypes = new Type[parameters.Count];
                for (var i = 0; i < parameters.Count; i++) {
                    paramForges[i] = parameters[i].Forge;
                    paramTypes[i] = paramForges[i].EvaluationType;
                }

                // check if special 'size' method
                if (currentInputType is EPChainableTypeClass chainableTypeClass) {
                    // is this an array
                    var isArray = chainableTypeClass.Clazz.IsArray;
                    // is this a generic collection
                    var isCollection = chainableTypeClass.Clazz.IsGenericCollection();

                    if ((isArray || isCollection) &&
                        chainElementName.Equals("size", StringComparison.InvariantCultureIgnoreCase) &&
                        paramTypes.Length == 0 &&
                        lastElement == chainElement) {
                        ExprDotForge size;
                        if (isArray) {
                            size = new ExprDotForgeSizeArray();
                        }
                        else {
                            size = new ExprDotForgeSizeCollection();
                        }

                        methodForges.Add(size);
                        currentInputType = size.TypeInfo;
                        continue;
                    }

                    if ((isArray || isCollection) &&
                        chainElementName.Equals("get", StringComparison.InvariantCultureIgnoreCase) &&
                        paramTypes.Length == 1 &&
                        paramTypes[0].IsTypeInteger()) {
                        ExprDotForge get;
                        var component = chainableTypeClass.Clazz.GetComponentType();
                        var componentBoxed = component.GetBoxedType();
                        if (isArray) {
                            get = new ExprDotForgeGetArray(paramForges[0], componentBoxed);
                        }
                        else {
                            get = new ExprDotForgeGetCollection(paramForges[0], componentBoxed);
                        }

                        methodForges.Add(get);
                        currentInputType = get.TypeInfo;
                        continue;
                    }

                    if (chainElement is ChainableArray chainableArray && isArray) {
                        var typeInfo = currentInputType;
                        var indexExpr = ChainableArray.ValidateSingleIndexExpr(
                            chainableArray.Indexes,
                            () => $"operation on type {typeInfo.ToTypeDescriptive()}");
                        var componentType = chainableTypeClass.Clazz.GetComponentType().GetBoxedType();
                        var get = new ExprDotForgeGetArray(indexExpr.Forge, componentType);
                        methodForges.Add(get);
                        currentInputType = get.TypeInfo;
                        continue;
                    }
                }

                // determine if there is a matching method
                var matchingMethod = false;
                var optionalMethodTarget = GetMethodTarget(currentInputType);
                if (optionalMethodTarget != null && !(chainElement is ChainableArray)) {
                    try {
                        GetValidateMethodDescriptor(
                            optionalMethodTarget,
                            chainElementName,
                            parameters,
                            validationContext);
                        matchingMethod = true;
                    }
                    catch (ExprValidationException) {
                        // expected
                    }
                }

                if (EnumMethodResolver.IsEnumerationMethod(chainElementName, validationContext.ImportService) &&
                    (!matchingMethod ||
                     (optionalMethodTarget != null &&
                      (optionalMethodTarget.IsArray ||
                       optionalMethodTarget.IsGenericCollection())))) {
                    var enumerationMethod = EnumMethodResolver.FromName(
                        chainElementName,
                        validationContext.ImportService);
                    var eval = enumerationMethod.Factory.Invoke(chainElement.ParametersOrEmpty.Count);
                    eval.Init(
                        streamOfProviderIfApplicable,
                        enumerationMethod,
                        chainElementName,
                        currentInputType,
                        parameters,
                        validationContext);
                    currentInputType = eval.TypeInfo;
                    if (currentInputType == null) {
                        throw new IllegalStateException(
                            "Enumeration method '" + chainElementName + "' has not returned type information");
                    }

                    methodForges.Add(eval);
                    lastLambdaFunc = enumerationMethod;
                    continue;
                }

                // resolve datetime
                if (DatetimeMethodResolver.IsDateTimeMethod(chainElementName, validationContext.ImportService) &&
                    (!matchingMethod ||
                     (optionalMethodTarget != null &&
                      (optionalMethodTarget == typeof(DateTimeEx) ||
                       optionalMethodTarget == typeof(DateTimeOffset) ||
                       optionalMethodTarget == typeof(DateTime))))) {
                    var datetimeMethod = DatetimeMethodResolver.FromName(
                        chainElementName,
                        validationContext.ImportService);
                    try {
                        var datetimeImpl = ExprDotDTFactory.ValidateMake(
                            validationContext.StreamTypeService,
                            chainSpecStack,
                            datetimeMethod,
                            chainElementName,
                            currentInputType,
                            parameters,
                            inputDesc,
                            validationContext.ImportService.TimeAbacus,
                            validationContext.TableCompileTimeResolver,
                            validationContext.ImportService,
                            validationContext.StatementRawInfo);
                        currentInputType = datetimeImpl.ReturnType;
                        if (currentInputType == null) {
                            throw new IllegalStateException(
                                "Date-time method '" + chainElementName + "' has not returned type information");
                        }

                        methodForges.Add(datetimeImpl.Forge);
                        filterAnalyzerDesc = datetimeImpl.IntervalFilterDesc;
                        continue;
                    }
                    catch (ExprValidationException) {
                        if (!chainElementName.Equals("get", StringComparison.InvariantCultureIgnoreCase)) {
                            throw;
                        }
                    }
                }

                // try to resolve as property if the last method returned a type
                if (currentInputType is EPChainableTypeEventSingle single) {
                    if (chainElement is ChainableArray) {
                        throw new ExprValidationException(
                            "Could not perform array operation on type " +
                            single.ToTypeDescriptive());
                    }

                    var inputEventType = (EventTypeSPI)single.EventType;
                    var type = inputEventType.GetPropertyType(chainElementName);
                    var getter = inputEventType.GetGetterSPI(chainElementName);
                    var fragmentType = inputEventType.GetFragmentType(chainElementName);
                    ExprDotForge forge;
                    if (type != null && getter != null) {
                        if (fragmentType == null || last) {
                            forge = new ExprDotForgeProperty(
                                getter,
                                EPChainableTypeHelper.SingleValue(type.GetBoxedType()));
                            currentInputType = forge.TypeInfo;
                        }
                        else {
                            if (!fragmentType.IsIndexed) {
                                currentInputType = EPChainableTypeHelper.SingleEvent(fragmentType.FragmentType);
                            }
                            else {
                                currentInputType = EPChainableTypeHelper.ArrayOfEvents(fragmentType.FragmentType);
                            }

                            forge = new ExprDotForgePropertyFragment(getter, currentInputType);
                        }

                        methodForges.Add(forge);
                        continue;
                    }
                }

                if (currentInputType is EPChainableTypeEventMulti multi && chainElement is ChainableArray element) {
                    var inputEventType = (EventTypeSPI)multi.Component;
                    var typeInfo = currentInputType;
                    var indexExpr =
                        ChainableArray.ValidateSingleIndexExpr(
                            element.Indexes,
                            () => "operation on type " +
                                  typeInfo.ToTypeDescriptive());
                    currentInputType = EPChainableTypeHelper.SingleEvent(inputEventType);
                    var forge = new ExprDotForgeEventArrayAtIndex(
                        currentInputType,
                        indexExpr);
                    methodForges.Add(forge);
                    continue;
                }

                // Finally try to resolve the method
                if (optionalMethodTarget != null && !(chainElement is ChainableArray)) {
                    try {
                        // find descriptor again, allow for duck typing
                        var desc = GetValidateMethodDescriptor(
                            optionalMethodTarget,
                            chainElementName,
                            parameters,
                            validationContext);
                        paramForges = desc.ChildForges;
                        var forge = GetDotChainMethodCallForge(
                            currentInputType,
                            validationContext,
                            chainSpecStack,
                            desc,
                            paramForges);
                        methodForges.Add(forge);
                        currentInputType = forge.TypeInfo;
                    }
                    catch (Exception e) {
                        if (!isDuckTyping) {
                            if (chainElement is ChainableName) {
                                // try "something.property" -> getProperty()
                                try {
                                    var methodName = "Get" + chainElementName.Capitalize();
                                    var desc = GetValidateMethodDescriptor(
                                        optionalMethodTarget,
                                        methodName,
                                        parameters,
                                        validationContext);
                                    var forge = GetDotChainMethodCallForge(
                                        currentInputType,
                                        validationContext,
                                        chainSpecStack,
                                        desc,
                                        paramForges);
                                    methodForges.Add(forge);
                                    currentInputType = forge.TypeInfo;
                                    continue;
                                }
                                catch (Exception) {
                                    throw new ExprValidationException(e.Message, e);
                                }
                            }

                            throw new ExprValidationException(e.Message, e);
                        }
                        else {
                            var duck = new ExprDotMethodForgeDuck(
                                validationContext.StatementName,
                                validationContext.ImportService,
                                chainElementName,
                                paramTypes,
                                paramForges);
                            methodForges.Add(duck);
                            currentInputType = duck.TypeInfo;
                        }
                    }

                    continue;
                }

                string message;
                if (!(chainElement is ChainableArray)) {
                    message = "Could not find event property or method named '" +
                              chainElementName +
                              "' in " +
                              currentInputType.ToTypeDescriptive();
                }
                else {
                    message = "Could not perform array operation on type " +
                              currentInputType.ToTypeDescriptive();
                }

                throw new ExprValidationException(message);
            }

            var intermediateEvals = methodForges.ToArray();

            if (lastLambdaFunc != null) {
                ExprDotForge finalEval = null;
                if (currentInputType is EPChainableTypeEventMulti mvType) {
                    var tableMetadata =
                        validationContext.TableCompileTimeResolver.ResolveTableFromEventType(mvType.Component);
                    if (tableMetadata != null) {
                        finalEval = new ExprDotForgeUnpackCollEventBeanTable(mvType.Component, tableMetadata);
                    }
                    else {
                        finalEval = new ExprDotForgeUnpackCollEventBean(mvType.Component);
                    }
                }
                else if (currentInputType is EPChainableTypeEventSingle epType) {
                    var tableMetadata =
                        validationContext.TableCompileTimeResolver.ResolveTableFromEventType(epType.EventType);
                    if (tableMetadata != null) {
                        finalEval = new ExprDotForgeUnpackBeanTable(epType.EventType, tableMetadata);
                    }
                    else {
                        finalEval = new ExprDotForgeUnpackBean(epType.EventType);
                    }
                }

                if (finalEval != null) {
                    methodForges.Add(finalEval);
                }
            }

            var unpackingForges = methodForges.ToArray();
            return new ExprDotNodeRealizedChain(intermediateEvals, unpackingForges, filterAnalyzerDesc);
        }

        private static ExprDotForge GetDotChainMethodCallForge(
            EPChainableType currentInputType,
            ExprValidationContext validationContext,
            Deque<Chainable> chainSpecStack,
            ExprNodeUtilMethodDesc desc,
            ExprForge[] paramForges)
        {
            if (currentInputType is EPChainableTypeClass) {
                // if followed by an enumeration method, convert array to collection
                if (desc.ReflectionMethod.ReturnType.IsArray &&
                    !chainSpecStack.IsEmpty() &&
                    EnumMethodResolver.IsEnumerationMethod(
                        chainSpecStack.First.RootNameOrEmptyString,
                        validationContext.ImportService)) {
                    return new ExprDotMethodForgeNoDuck(
                        validationContext.StatementName,
                        desc.ReflectionMethod,
                        desc.MethodTargetType,
                        paramForges,
                        ExprDotMethodForgeNoDuck.WrapType.WRAPARRAY);
                }
                else {
                    return new ExprDotMethodForgeNoDuck(
                        validationContext.StatementName,
                        desc.ReflectionMethod,
                        desc.MethodTargetType,
                        paramForges,
                        ExprDotMethodForgeNoDuck.WrapType.PLAIN);
                }
            }
            else {
                return new ExprDotMethodForgeNoDuck(
                    validationContext.StatementName,
                    desc.ReflectionMethod,
                    desc.MethodTargetType,
                    paramForges,
                    ExprDotMethodForgeNoDuck.WrapType.UNDERLYING);
            }
        }

        private static Type GetMethodTarget(EPChainableType currentInputType)
        {
            if (currentInputType is EPChainableTypeClass @class) {
                return @class.Clazz;
            }
            else if (currentInputType is EPChainableTypeEventSingle single) {
                return single.EventType.UnderlyingType;
            }

            return null;
        }

        public static object EvaluateChainWithWrap(
            ExprDotStaticMethodWrap resultWrapLambda,
            object result,
            EventType optionalResultSingleEventType,
            Type resultType,
            ExprDotEval[] chainEval,
            ExprDotForge[] chainForges,
            EventBean[] eventsPerStream,
            bool newData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            if (result == null) {
                return null;
            }

            if (resultWrapLambda != null) {
                result = resultWrapLambda.ConvertNonNull(result);
            }

            foreach (var aChainEval in chainEval) {
                result = aChainEval.Evaluate(result, eventsPerStream, newData, exprEvaluatorContext);
                if (result == null) {
                    return result;
                }
            }

            return result;
        }

        public static CodegenExpression EvaluateChainCodegen(
            CodegenMethod parent,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope,
            CodegenExpression inner,
            Type innerType,
            ExprDotForge[] forges,
            ExprDotStaticMethodWrap optionalResultWrapLambda)
        {
            if (forges.Length == 0) {
                return inner;
            }

            var last = forges[^1];
            var lastType = last.TypeInfo.GetCodegenReturnType();
            var methodNode = parent.MakeChild(lastType, typeof(ExprDotNodeUtility), codegenClassScope)
                .AddParam(innerType, "inner");

            var block = methodNode.Block;
            var currentTarget = "wrapped";
            Type currentTargetType;
            if (optionalResultWrapLambda != null) {
                currentTargetType = optionalResultWrapLambda.TypeInfo.GetCodegenReturnType();
                if (!lastType.IsTypeVoid()) {
                    block.IfRefNullReturnNull("inner");
                }

                block.DeclareVar(
                    currentTargetType,
                    "wrapped",
                    optionalResultWrapLambda.CodegenConvertNonNull(Ref("inner"), methodNode, codegenClassScope));
            }
            else {
                block.DeclareVar(innerType, "wrapped", Ref("inner"));
                currentTargetType = innerType;
            }

            string refname = null;
            var instrumentationName = new ExprDotEvalVisitorImpl();
            for (var i = 0; i < forges.Length; i++) {
                refname = "r" + i;
                forges[i].Visit(instrumentationName);
                block.Apply(
                    Instblock(
                        codegenClassScope,
                        "qExprDotChainElement",
                        Constant(i),
                        Constant(instrumentationName.MethodType),
                        Constant(instrumentationName.MethodName)));

                var typeInformation = ConstantNull();
                if (codegenClassScope.IsInstrumented) {
                    typeInformation = codegenClassScope.AddOrGetDefaultFieldSharable(
                        new EPChainableTypeCodegenSharable(forges[i].TypeInfo, codegenClassScope));
                }

                var reftype = forges[i].TypeInfo.GetCodegenReturnType();
                if (reftype.IsTypeVoid()) {
                    block.Expression(
                            forges[i]
                                .Codegen(
                                    Ref(currentTarget),
                                    currentTargetType,
                                    methodNode,
                                    exprSymbol,
                                    codegenClassScope))
                        .Apply(Instblock(codegenClassScope, "aExprDotChainElement", typeInformation, ConstantNull()));
                }
                else {
                    block.DeclareVar(
                        reftype,
                        refname,
                        forges[i]
                            .Codegen(
                                Ref(currentTarget),
                                currentTargetType,
                                methodNode,
                                exprSymbol,
                                codegenClassScope));
                    currentTarget = refname;
                    currentTargetType = reftype;
                    if (!reftype.IsPrimitive) {
                        var ifBlock = block.IfRefNull(refname)
                            .Apply(
                                Instblock(codegenClassScope, "aExprDotChainElement", typeInformation, ConstantNull()));
                        if (!lastType.IsTypeVoid()) {
                            ifBlock.BlockReturn(ConstantNull());
                        }
                        else {
                            ifBlock.BlockEnd();
                        }
                    }

                    block.Apply(Instblock(codegenClassScope, "aExprDotChainElement", typeInformation, Ref(refname)));
                }
            }

            if (lastType.IsTypeVoid()) {
                block.MethodEnd();
            }
            else {
                block.MethodReturn(Ref(refname));
            }

            return LocalMethod(methodNode, inner);
        }

        private static ExprNodeUtilMethodDesc GetValidateMethodDescriptor(
            Type methodTarget,
            string methodName,
            IList<ExprNode> parameters,
            ExprValidationContext validationContext)
        {
            ExprNodeUtilResolveExceptionHandler exceptionHandler = new ProxyExprNodeUtilResolveExceptionHandler(
                e => new ExprValidationException(
                    "Failed to resolve method '" + methodName + "': " + e.Message,
                    e)
            );
            
            var wildcardType = validationContext.StreamTypeService.EventTypes.Length != 1
                ? null
                : validationContext.StreamTypeService.EventTypes[0];
            return ExprNodeUtilityResolve.ResolveMethodAllowWildcardAndStream(
                methodTarget.Name,
                methodTarget,
                methodName,
                parameters,
                wildcardType != null,
                wildcardType,
                exceptionHandler,
                methodName,
                validationContext.StatementRawInfo,
                validationContext.StatementCompileTimeService);
        }
    }
} // end of namespace