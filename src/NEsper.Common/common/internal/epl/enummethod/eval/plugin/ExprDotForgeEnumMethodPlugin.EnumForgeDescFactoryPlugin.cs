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
            private readonly EnumMethodModeStaticMethod _mode;
            private readonly string _enumMethodUsedName;
            private readonly DotMethodFP _footprint;
            private readonly IList<ExprNode> _parameters;
            private readonly EventType _inputEventType;
            private readonly Type _collectionComponentType;
            private readonly StatementRawInfo _raw;
            private readonly StatementCompileTimeServices _services;

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
                _mode = mode;
                _enumMethodUsedName = enumMethodUsedName;
                _footprint = footprint;
                _parameters = parameters;
                _inputEventType = inputEventType;
                _collectionComponentType = collectionComponentType;
                _raw = raw;
                _services = services;
            }

            public EnumForgeLambdaDesc GetLambdaStreamTypesForParameter(int parameterNum)
            {
                var desc = _footprint.Parameters[parameterNum];
                if (desc.LambdaParamNum == 0) {
                    return new EnumForgeLambdaDesc(Array.Empty<EventType>(), Array.Empty<string>());
                }

                var param = _parameters[parameterNum];
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
                        _mode.LambdaParameters.Invoke(new EnumMethodLambdaParameterDescriptor(parameterNum, i));

                    if (lambdaParamType is EnumMethodLambdaParameterTypeValue) {
                        if (_inputEventType == null) {
                            types[i] = ExprDotNodeUtility.MakeTransientOAType(
                                _enumMethodUsedName,
                                goesToNames[i],
                                _collectionComponentType,
                                _raw,
                                _services);
                        }
                        else {
                            types[i] = _inputEventType;
                        }
                    }
                    else if (lambdaParamType is EnumMethodLambdaParameterTypeIndex ||
                             lambdaParamType is EnumMethodLambdaParameterTypeSize) {
                        types[i] = ExprDotNodeUtility.MakeTransientOAType(
                            _enumMethodUsedName,
                            goesToNames[i],
                            typeof(int),
                            _raw,
                            _services);
                    }
                    else if (lambdaParamType is EnumMethodLambdaParameterTypeStateGetter getter) {
                        types[i] = ExprDotNodeUtility.MakeTransientOAType(
                            _enumMethodUsedName,
                            goesToNames[i],
                            getter.Type,
                            _raw,
                            _services);
                    }
                    else {
                        throw new UnsupportedOperationException(
                            "Unrecognized lambda parameter type " + lambdaParamType);
                    }

                    if (types[i] == _inputEventType) {
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
                parametersNext.Add(_mode.StateClass);

                // second parameter is the value: EventBean for event collection or the collection component type
                if (_inputEventType != null) {
                    parametersNext.Add(typeof(EventBean));
                }
                else {
                    parametersNext.Add(_collectionComponentType);
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
                        _mode.ServiceClass,
                        _mode.MethodName,
                        parametersNext.ToArray(),
                        false,
                        noFlags,
                        noFlags);
                }
                catch (MethodResolverNoSuchMethodException ex) {
                    throw new ExprValidationException(
                        "Failed to find service method for enumeration-method '" + _mode.MethodName + "': " + ex.Message,
                        ex);
                }

                if (!serviceMethod.ReturnType.IsTypeVoid()) {
                    throw new ExprValidationException(
                        "Failed to validate service method for enumeration-method '" +
                        _mode.MethodName +
                        "', expected void return type");
                }

                // obtain expected return type
                var returnType = _mode.ReturnType;
                Type expectedStateReturnType;
                if (returnType is EPChainableTypeClass @class) {
                    expectedStateReturnType = @class.Clazz;
                }
                else if (returnType is EPChainableTypeEventSingle) {
                    expectedStateReturnType = typeof(EventBean);
                }
                else if (returnType is EPChainableTypeEventMulti) {
                    expectedStateReturnType = typeof(ICollection<EventBean>);
                }
                else if (returnType is EPChainableTypeNull) {
                    expectedStateReturnType = null;
                }
                else {
                    throw new ExprValidationException("Unrecognized return type " + returnType);
                }

                // check state-class
                if (!TypeHelper.IsSubclassOrImplementsInterface(_mode.StateClass, typeof(EnumMethodState))) {
                    throw new ExprValidationException(
                        "State class " +
                        _mode.StateClass.CleanName() +
                        " does implement the " +
                        nameof(EnumMethodState) +
                        " interface");
                }

                var forge = new EnumForgePlugin(
                    bodiesAndParameters,
                    _mode,
                    expectedStateReturnType,
                    streamCountIncoming,
                    _inputEventType);
                return new EnumForgeDesc(returnType, forge);
            }
        }
    }
}