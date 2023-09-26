using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.collection;
using com.espertech.esper.common.client.hook.enummethod;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.enummethod.dot;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.dot.core;
using com.espertech.esper.common.@internal.epl.methodbase;
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.plugin
{
    public partial class ExprDotForgeEnumMethodPlugin
    {
        private class EnumForgeDescFactoryPlugin : EnumForgeDescFactory
        {
            private readonly EnumMethodModeStaticMethod mode;
            private readonly string enumMethodUsedName;
            private readonly DotMethodFP footprint;
            private readonly IList<ExprNode> parameters;
            private readonly EventType inputEventType;
            private readonly Type collectionComponentType;
            private readonly StatementRawInfo raw;
            private readonly StatementCompileTimeServices services;

            public EnumForgeDescFactoryPlugin(
                EnumMethodModeStaticMethod mode,
                string enumMethodUsedName,
                DotMethodFP footprint,
                IList<ExprNode> parameters,
                EventType inputEventType,
                Type collectionComponentType,
                StatementRawInfo raw,
                StatementCompileTimeServices services)
            {
                this.mode = mode;
                this.enumMethodUsedName = enumMethodUsedName;
                this.footprint = footprint;
                this.parameters = parameters;
                this.inputEventType = inputEventType;
                this.collectionComponentType = collectionComponentType;
                this.raw = raw;
                this.services = services;
            }

            public EnumForgeLambdaDesc GetLambdaStreamTypesForParameter(int parameterNum)
            {
                var desc = footprint.Parameters[parameterNum];
                if (desc.LambdaParamNum == 0) {
                    return new EnumForgeLambdaDesc(Array.Empty<EventType>(), Array.Empty<string>());
                }

                var param = parameters[parameterNum];
                if (!(param is ExprLambdaGoesNode goes)) {
                    throw new IllegalStateException("Parameter " + parameterNum + " is not a lambda parameter");
                }

                var goesToNames = goes.GoesToNames;

                // we allocate types for scalar-value-input and index-lambda-parameter-type; for event-input we use the existing input event type
                var types = new EventType[desc.LambdaParamNum];
                var names = new string[desc.LambdaParamNum];
                for (var i = 0; i < types.Length; i++) {
                    // obtain lambda parameter type
                    var lambdaParamType =
                        mode.LambdaParameters.Invoke(new EnumMethodLambdaParameterDescriptor(parameterNum, i));

                    if (lambdaParamType is EnumMethodLambdaParameterTypeValue) {
                        if (inputEventType == null) {
                            types[i] = ExprDotNodeUtility.MakeTransientOAType(
                                enumMethodUsedName,
                                goesToNames[i],
                                collectionComponentType,
                                raw,
                                services);
                        }
                        else {
                            types[i] = inputEventType;
                        }
                    }
                    else if (lambdaParamType is EnumMethodLambdaParameterTypeIndex ||
                             lambdaParamType is EnumMethodLambdaParameterTypeSize) {
                        types[i] = ExprDotNodeUtility.MakeTransientOAType(
                            enumMethodUsedName,
                            goesToNames[i],
                            typeof(int),
                            raw,
                            services);
                    }
                    else if (lambdaParamType is EnumMethodLambdaParameterTypeStateGetter getter) {
                        types[i] = ExprDotNodeUtility.MakeTransientOAType(
                            enumMethodUsedName,
                            goesToNames[i],
                            getter.Type,
                            raw,
                            services);
                    }
                    else {
                        throw new UnsupportedOperationException(
                            "Unrecognized lambda parameter type " + lambdaParamType);
                    }

                    if (types[i] == inputEventType) {
                        names[i] = goesToNames[i];
                    }
                    else {
                        names[i] = types[i].Name;
                    }
                }

                return new EnumForgeLambdaDesc(types, names);
            }

            public EnumForgeDesc MakeEnumForgeDesc(
                IList<ExprDotEvalParam> bodiesAndParameters,
                int streamCountIncoming,
                StatementCompileTimeServices services)
            {
                // determine static method
                IList<Type> parametersNext = new List<Type>();

                // first parameter is always the state
                parametersNext.Add(mode.StateClass);

                // second parameter is the value: EventBean for event collection or the collection component type
                if (inputEventType != null) {
                    parametersNext.Add(typeof(EventBean));
                }
                else {
                    parametersNext.Add(collectionComponentType);
                }

                // remaining parameters are the result of each DotMethodFPParam that returns a lambda (non-lambda is passed to state), always Object typed
                foreach (var param in bodiesAndParameters) {
                    if (param is ExprDotEvalParamLambda) {
                        parametersNext.Add(param.Body.Forge.EvaluationType.GetBoxedType());
                    }
                }

                // obtain service method
                var noFlags = new bool[parametersNext.Count];
                MethodInfo serviceMethod;
                try {
                    serviceMethod = MethodResolver.ResolveMethod(
                        mode.ServiceClass,
                        mode.MethodName,
                        parametersNext.ToArray(),
                        false,
                        noFlags,
                        noFlags);
                }
                catch (MethodResolverNoSuchMethodException ex) {
                    throw new ExprValidationException(
                        "Failed to find service method for enumeration-method '" + mode.MethodName + "': " + ex.Message,
                        ex);
                }

                if (!serviceMethod.ReturnType.IsTypeVoid()) {
                    throw new ExprValidationException(
                        "Failed to validate service method for enumeration-method '" +
                        mode.MethodName +
                        "', expected void return type");
                }

                // obtain expected return type
                var returnType = mode.ReturnType;
                Type expectedStateReturnType;
                if (returnType is EPChainableTypeClass @class) {
                    expectedStateReturnType = @class.Clazz;
                }
                else if (returnType is EPChainableTypeEventSingle) {
                    expectedStateReturnType = typeof(EventBean);
                }
                else if (returnType is EPChainableTypeEventMulti) {
                    expectedStateReturnType = typeof(FlexCollection);
                }
                else if (returnType is EPChainableTypeNull) {
                    expectedStateReturnType = null;
                }
                else {
                    throw new ExprValidationException("Unrecognized return type " + returnType);
                }

                // check state-class
                if (!TypeHelper.IsSubclassOrImplementsInterface(mode.StateClass, typeof(EnumMethodState))) {
                    throw new ExprValidationException(
                        "State class " +
                        mode.StateClass.CleanName() +
                        " does implement the " +
                        nameof(EnumMethodState) +
                        " interface");
                }

                var forge = new EnumForgePlugin(
                    bodiesAndParameters,
                    mode,
                    expectedStateReturnType,
                    streamCountIncoming,
                    inputEventType);
                return new EnumForgeDesc(returnType, forge);
            }
        }
    }
}