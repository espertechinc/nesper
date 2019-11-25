///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.datetime.eval;
using com.espertech.esper.common.@internal.epl.enummethod.dot;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.join.analyze;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.rettype;
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
            propsResult.Put(propertyName, Boxing.GetBoxedType(type));
            var eventTypeName =
                services.EventTypeNameGeneratorStatement.GetAnonymousTypeNameEnumMethod(enumMethod, propertyName);
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
                propsResult,
                null,
                null,
                null,
                null,
                services.BeanEventTypeFactoryPrivate,
                services.EventTypeCompileTimeResolver);
            services.EventTypeCompileTimeRegistry.NewType(oatype);
            return oatype;
        }

        public static bool IsDatetimeOrEnumMethod(string name)
        {
            return EnumMethodEnumExtensions.IsEnumerationMethod(name) || DatetimeMethodEnumHelper.IsDateTimeMethod(name);
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
            EPType info = null;

            if (rootNodeForge is ExprEnumerationForge) {
                rootLambdaForge = (ExprEnumerationForge) rootNodeForge;
                var eventTypeCollection =
                    rootLambdaForge.GetEventTypeCollection(statementRawInfo, compileTimeServices);
                if (eventTypeCollection != null) {
                    info = EPTypeHelper.CollectionOfEvents(eventTypeCollection);
                }

                if (info == null) {
                    var eventTypeSingle =
                        rootLambdaForge.GetEventTypeSingle(statementRawInfo, compileTimeServices);
                    if (eventTypeSingle != null) {
                        info = EPTypeHelper.SingleEvent(eventTypeSingle);
                    }
                }

                if (info == null) {
                    var componentType = rootLambdaForge.ComponentTypeCollection;
                    if (componentType != null) {
                        info = EPTypeHelper.CollectionOfSingleValue(
                            rootLambdaForge.ComponentTypeCollection,
                            typeof(ICollection<>).MakeGenericType(componentType));
                    }
                }

                if (info == null) {
                    rootLambdaForge = null; // not a lambda evaluator
                }
            }
            else if (inputExpression is ExprIdentNode) {
                var identNode = (ExprIdentNode) inputExpression;
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
            var typeInfo = EPTypeHelper.SingleValue(propertyType); // assume scalar for now

            // no enumeration methods, no need to expose as an enumeration
            if (!allowEnumType) {
                return new ExprDotEnumerationSourceForgeForProps(null, typeInfo, streamId, null);
            }

            var fragmentEventType = streamType.GetFragmentType(propertyName);
            var getter = ((EventTypeSPI) streamType).GetGetterSPI(propertyName);

            ExprEnumerationForge enumEvaluator = null;
            if (getter != null && fragmentEventType != null) {
                if (fragmentEventType.IsIndexed) {
                    enumEvaluator = new PropertyDotEventCollectionForge(
                        propertyName,
                        streamId,
                        fragmentEventType.FragmentType,
                        getter,
                        disablePropertyExpressionEventCollCache);
                    typeInfo = EPTypeHelper.CollectionOfEvents(fragmentEventType.FragmentType);
                }
                else { // we don't want native to require an eventbean instance
                    enumEvaluator = new PropertyDotEventSingleForge(streamId, fragmentEventType.FragmentType, getter);
                    typeInfo = EPTypeHelper.SingleEvent(fragmentEventType.FragmentType);
                }
            }
            else {
                var desc = EventTypeUtility.GetNestablePropertyDescriptor(streamType, propertyName);
                if (desc != null && desc.IsIndexed && !desc.IsRequiresIndex && desc.PropertyComponentType != null) {
                    if (propertyType.IsArray) {
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

                    typeInfo = EPTypeHelper.CollectionOfSingleValue(
                        desc.PropertyComponentType,
                        desc.PropertyType);
                }
            }

            return new ExprDotEnumerationSourceForgeForProps(
                enumEvaluator,
                typeInfo,
                streamId,
                (ExprEnumerationGivenEventForge) enumEvaluator);
        }

        public static EventType[] GetSingleLambdaParamEventType(
            string enumMethodUsedName,
            IList<string> goesToNames,
            EventType inputEventType,
            Type collectionComponentType,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services)
        {
            if (inputEventType != null) {
                return new EventType[] {inputEventType};
            }
            else {
                return new EventType[] {
                    ExprDotNodeUtility.MakeTransientOAType(
                        enumMethodUsedName,
                        goesToNames[0],
                        collectionComponentType,
                        statementRawInfo,
                        services)
                };
            }
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
            EPType inputType,
            IList<ExprChainedSpec> chainSpec,
            ExprValidationContext validationContext,
            bool isDuckTyping,
            ExprDotNodeFilterAnalyzerInput inputDesc)
        {
            IList<ExprDotForge> methodForges = new List<ExprDotForge>();
            var currentInputType = inputType;
            EnumMethodEnum? lastLambdaFunc = null;
            var lastElement = chainSpec.IsEmpty() ? null : chainSpec[chainSpec.Count - 1];
            FilterExprAnalyzerAffector filterAnalyzerDesc = null;

            Deque<ExprChainedSpec> chainSpecStack = new ArrayDeque<ExprChainedSpec>(chainSpec);
            while (!chainSpecStack.IsEmpty()) {
                var chainElement = chainSpecStack.RemoveFirst();
                lastLambdaFunc = null; // reset

                // compile parameters for chain element
                var paramForges = new ExprForge[chainElement.Parameters.Count];
                var paramTypes = new Type[chainElement.Parameters.Count];
                for (var i = 0; i < chainElement.Parameters.Count; i++) {
                    paramForges[i] = chainElement.Parameters[i].Forge;
                    paramTypes[i] = paramForges[i].EvaluationType;
                }

                // check if special 'size' method
                if (currentInputType is ClassMultiValuedEPType) {
                    var type = (ClassMultiValuedEPType) currentInputType;
                    if (chainElement.Name.Equals("size", StringComparison.InvariantCultureIgnoreCase) &&
                        paramTypes.Length == 0 &&
                        lastElement == chainElement) {
                        var sizeExpr = new ExprDotForgeArraySize();
                        methodForges.Add(sizeExpr);
                        currentInputType = sizeExpr.TypeInfo;
                        continue;
                    }

                    if (chainElement.Name.Equals("get", StringComparison.InvariantCultureIgnoreCase) &&
                        paramTypes.Length == 1 &&
                        Boxing.GetBoxedType(paramTypes[0]) == typeof(int?)) {
                        var componentType = Boxing.GetBoxedType(type.Component);
                        var get = new ExprDotForgeArrayGet(paramForges[0], componentType);
                        methodForges.Add(get);
                        currentInputType = get.TypeInfo;
                        continue;
                    }
                }

                // determine if there is a matching method
                var matchingMethod = false;
                var methodTarget = GetMethodTarget(currentInputType);
                if (methodTarget != null) {
                    try {
                        GetValidateMethodDescriptor(
                            methodTarget,
                            chainElement.Name,
                            chainElement.Parameters,
                            validationContext);
                        matchingMethod = true;
                    }
                    catch (ExprValidationException) {
                        // expected
                    }
                }

                if (EnumMethodEnumExtensions.IsEnumerationMethod(chainElement.Name) &&
                    (!matchingMethod || methodTarget.IsArray || methodTarget.IsGenericCollection())) {
                    var enumerationMethod = EnumMethodEnumExtensions.FromName(chainElement.Name);
                    if (enumerationMethod == null) {
                        throw new EPException("unable to determine enumeration method from name");
                    }

                    var eval = TypeHelper.Instantiate<ExprDotForgeEnumMethod>(
                        enumerationMethod.Value.GetImplementation());
                    eval.Init(
                        streamOfProviderIfApplicable,
                        enumerationMethod.Value,
                        chainElement.Name,
                        currentInputType,
                        chainElement.Parameters,
                        validationContext);
                    currentInputType = eval.TypeInfo;
                    if (currentInputType == null) {
                        throw new IllegalStateException(
                            "Enumeration method '" + chainElement.Name + "' has not returned type information");
                    }

                    methodForges.Add(eval);
                    lastLambdaFunc = enumerationMethod;
                    continue;
                }

                // resolve datetime
                if (DatetimeMethodEnumHelper.IsDateTimeMethod(chainElement.Name) &&
                    (!matchingMethod ||
                     methodTarget == typeof(DateTimeEx) ||
                     methodTarget == typeof(DateTimeOffset) ||
                     methodTarget == typeof(DateTime))) {
                    var dateTimeMethod = DatetimeMethodEnumHelper.FromName(chainElement.Name);
                    var datetimeImpl = ExprDotDTFactory.ValidateMake(
                        validationContext.StreamTypeService,
                        chainSpecStack,
                        dateTimeMethod,
                        chainElement.Name,
                        currentInputType,
                        chainElement.Parameters,
                        inputDesc,
                        validationContext.ImportService.TimeAbacus,
                        null,
                        validationContext.TableCompileTimeResolver);
                    currentInputType = datetimeImpl.ReturnType;
                    if (currentInputType == null) {
                        throw new IllegalStateException(
                            "Date-time method '" + chainElement.Name + "' has not returned type information");
                    }

                    methodForges.Add(datetimeImpl.Forge);
                    filterAnalyzerDesc = datetimeImpl.IntervalFilterDesc;
                    continue;
                }

                // try to resolve as property if the last method returned a type
                if (currentInputType is EventEPType) {
                    var inputEventType = (EventTypeSPI) ((EventEPType) currentInputType).EventType;
                    var type = inputEventType.GetPropertyType(chainElement.Name);
                    var getter = inputEventType.GetGetterSPI(chainElement.Name);
                    if (type != null && getter != null) {
                        var noduck = new ExprDotForgeProperty(
                            getter,
                            EPTypeHelper.SingleValue(Boxing.GetBoxedType(type)));
                        methodForges.Add(noduck);
                        currentInputType = EPTypeHelper.SingleValue(EPTypeHelper.GetClassSingleValued(noduck.TypeInfo));
                        continue;
                    }
                }

                // Finally try to resolve the method
                if (methodTarget != null) {
                    try {
                        // find descriptor again, allow for duck typing
                        var desc = GetValidateMethodDescriptor(
                            methodTarget,
                            chainElement.Name,
                            chainElement.Parameters,
                            validationContext);
                        paramForges = desc.ChildForges;
                        ExprDotForge forge;
                        if (currentInputType is ClassEPType) {
                            // if followed by an enumeration method, convert array to collection
                            if (desc.ReflectionMethod.ReturnType.IsArray &&
                                !chainSpecStack.IsEmpty() &&
                                EnumMethodEnumExtensions.IsEnumerationMethod(chainSpecStack.First.Name)) {
                                forge = new ExprDotMethodForgeNoDuck(
                                    validationContext.StatementName,
                                    desc.ReflectionMethod,
                                    paramForges,
                                    ExprDotMethodForgeNoDuck.DuckType.WRAPARRAY);
                            }
                            else {
                                forge = new ExprDotMethodForgeNoDuck(
                                    validationContext.StatementName,
                                    desc.ReflectionMethod,
                                    paramForges,
                                    ExprDotMethodForgeNoDuck.DuckType.PLAIN);
                            }
                        }
                        else {
                            forge = new ExprDotMethodForgeNoDuck(
                                validationContext.StatementName,
                                desc.ReflectionMethod,
                                paramForges,
                                ExprDotMethodForgeNoDuck.DuckType.UNDERLYING);
                        }

                        methodForges.Add(forge);
                        currentInputType = forge.TypeInfo;
                    }
                    catch (Exception e) {
                        if (!isDuckTyping) {
                            throw new ExprValidationException(e.Message, e);
                        }
                        else {
                            var duck = new ExprDotMethodForgeDuck(
                                validationContext.StatementName,
                                validationContext.ImportService,
                                chainElement.Name,
                                paramTypes,
                                paramForges);
                            methodForges.Add(duck);
                            currentInputType = duck.TypeInfo;
                        }
                    }

                    continue;
                }

                var message = "Could not find event property, enumeration method or instance method named '" +
                                 chainElement.Name +
                                 "' in " +
                                 EPTypeHelper.ToTypeDescriptive(currentInputType);
                throw new ExprValidationException(message);
            }

            var intermediateEvals = methodForges.ToArray();

            if (lastLambdaFunc != null) {
                ExprDotForge finalEval = null;
                if (currentInputType is EventMultiValuedEPType) {
                    var mvType = (EventMultiValuedEPType) currentInputType;
                    var tableMetadata =
                        validationContext.TableCompileTimeResolver.ResolveTableFromEventType(mvType.Component);
                    if (tableMetadata != null) {
                        finalEval = new ExprDotForgeUnpackCollEventBeanTable(mvType.Component, tableMetadata);
                    }
                    else {
                        finalEval = new ExprDotForgeUnpackCollEventBean(mvType.Component);
                    }
                }
                else if (currentInputType is EventEPType) {
                    var epType = (EventEPType) currentInputType;
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

        private static Type GetMethodTarget(EPType currentInputType)
        {
            if (currentInputType is ClassEPType) {
                return ((ClassEPType) currentInputType).Clazz;
            }
            else if (currentInputType is EventEPType) {
                return ((EventEPType) currentInputType).EventType.UnderlyingType;
            }

            return null;
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

            var last = forges[forges.Length - 1];
            var lastType = EPTypeHelper.GetCodegenReturnType(last.TypeInfo).GetBoxedType();
            var methodNode = parent
                .MakeChild(lastType, typeof(ExprDotNodeUtility), codegenClassScope)
                .AddParam(innerType, "inner");

            var block = methodNode.Block.DebugStack();

            var currentTarget = "wrapped";
            Type currentTargetType;
            if (optionalResultWrapLambda != null) {
                currentTargetType = EPTypeHelper.GetCodegenReturnType(optionalResultWrapLambda.TypeInfo);
                block
                    .IfRefNullReturnNull("inner")
                    .DeclareVar(
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
                        new EPTypeCodegenSharable(forges[i].TypeInfo, codegenClassScope));
                }

                var reftype = EPTypeHelper.GetCodegenReturnType(forges[i].TypeInfo).GetBoxedType();
                if (reftype == typeof(void)) {
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
                    var invocation = forges[i]
                        .Codegen(
                            Ref(currentTarget),
                            currentTargetType,
                            methodNode,
                            exprSymbol,
                            codegenClassScope);

                    // We can't assume anything about the return type.  Unfortunately, we do not know the resultant type
                    // of the forge... I must fix this. - TBD
                    invocation = CodegenLegoCast.CastSafeFromObjectType(reftype, invocation);
                    
                    block.DeclareVar(reftype, refname, invocation);
                    currentTarget = refname;
                    currentTargetType = reftype;
                    if (reftype.CanBeNull()) {
                        block.IfRefNull(refname)
                            .Apply(
                                Instblock(codegenClassScope, "aExprDotChainElement", typeInformation, ConstantNull()))
                            .BlockReturn(ConstantNull());
                    }

                    block.Apply(Instblock(codegenClassScope, "aExprDotChainElement", typeInformation, Ref(refname)));
                }
            }

            if (lastType == typeof(void)) {
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
                e => new ExprValidationException("Failed to resolve method '" + methodName + "': " + e.Message, e));
            var wildcardType = validationContext.StreamTypeService.EventTypes.Length != 1
                ? null
                : validationContext.StreamTypeService.EventTypes[0];
            return ExprNodeUtilityResolve.ResolveMethodAllowWildcardAndStream(
                methodTarget.FullName,
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