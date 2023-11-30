///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.hook.expr;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.util
{
    /// <summary>
    /// Used for retrieving static and instance method objects. It provides two points of added functionality
    /// over the standard reflection mechanism of retrieving methods. First, class names can be partial, and
    /// if the class name is partial then System is searched for the class. Second, invocation parameter
    /// types don't have to match the declaration parameter types exactly when the standard conversion mechanisms
    /// (currently autoboxing and widening conversions) will make the invocation valid. Preference is given to
    /// those methods that require the fewest widening conversions.
    /// </summary>
    public class MethodResolver
    {
        private static readonly ILog Log =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly IDictionary<Type, ICollection<Type>> WrappingConversions =
            new Dictionary<Type, ICollection<Type>>();

        private static HashSet<Type> InitWrappingConversions<TX, TXN>()
        {
            var wrappers = new HashSet<Type>();
            wrappers.Add(typeof(TX));
            wrappers.Add(typeof(TXN));
            WrappingConversions.Put(typeof(TX), wrappers);
            WrappingConversions.Put(typeof(TXN), wrappers);
            return wrappers;
        }

        static MethodResolver()
        {
            WIDENING_CONVERSIONS = new Dictionary<Type, ICollection<Type>>();

            AddWideningConversion<byte>(
                typeof(byte),
                typeof(byte?)
            );
            AddWideningConversion<sbyte>(
                typeof(sbyte?),
                typeof(sbyte)
            );
            AddWideningConversion<short>(
                typeof(byte?),
                typeof(byte),
                typeof(sbyte?),
                typeof(sbyte),
                typeof(short?),
                typeof(short)
            );
            AddWideningConversion<int>(
                typeof(byte?),
                typeof(byte),
                typeof(sbyte?),
                typeof(sbyte),
                typeof(short?),
                typeof(short),
                typeof(int?),
                typeof(int),
                typeof(ushort?),
                typeof(ushort)
            );
            AddWideningConversion<long>(
                typeof(byte?),
                typeof(byte),
                typeof(sbyte?),
                typeof(sbyte),
                typeof(short?),
                typeof(short),
                typeof(int?),
                typeof(int),
                typeof(ushort?),
                typeof(ushort),
                typeof(uint?),
                typeof(uint),
                typeof(long?),
                typeof(long)
            );
            AddWideningConversion<ushort>(
                typeof(byte?),
                typeof(byte),
                typeof(ushort?),
                typeof(ushort)
            );
            AddWideningConversion<uint>(
                typeof(byte),
                typeof(byte?),
                typeof(ushort?),
                typeof(ushort),
                typeof(uint?),
                typeof(uint)
            );
            AddWideningConversion<ulong>(
                typeof(byte?),
                typeof(byte),
                typeof(ushort?),
                typeof(ushort),
                typeof(uint?),
                typeof(uint),
                typeof(ulong?),
                typeof(ulong)
            );
            AddWideningConversion<float>(
                typeof(byte?),
                typeof(byte),
                typeof(short?),
                typeof(short),
                typeof(int?),
                typeof(int),
                typeof(long?),
                typeof(long),
                typeof(sbyte?),
                typeof(sbyte),
                typeof(float?),
                typeof(float),
                typeof(ushort?),
                typeof(ushort),
                typeof(uint?),
                typeof(uint),
                typeof(ulong?),
                typeof(ulong)
            );
            AddWideningConversion<double>(
                typeof(byte?),
                typeof(byte),
                typeof(short?),
                typeof(short),
                typeof(int?),
                typeof(int),
                typeof(long?),
                typeof(long),
                typeof(sbyte?),
                typeof(sbyte),
                typeof(float?),
                typeof(float),
                typeof(double?),
                typeof(double),
                typeof(ushort?),
                typeof(ushort),
                typeof(uint?),
                typeof(uint),
                typeof(ulong?),
                typeof(ulong)
            );
            AddWideningConversion<decimal>(
                typeof(byte?),
                typeof(byte),
                typeof(short?),
                typeof(short),
                typeof(int?),
                typeof(int),
                typeof(long?),
                typeof(long),
                typeof(sbyte?),
                typeof(sbyte),
                typeof(float?),
                typeof(float),
                typeof(double?),
                typeof(double),
                typeof(decimal?),
                typeof(decimal),
                typeof(ushort?),
                typeof(ushort),
                typeof(uint?),
                typeof(uint),
                typeof(ulong?),
                typeof(ulong)
            );
            AddWideningConversion<char>(
                typeof(byte),
                typeof(byte?),
                typeof(ushort?),
                typeof(ushort)
            );
            AddWideningConversion<byte?>(
                typeof(byte),
                typeof(byte?)
            );
            AddWideningConversion<sbyte?>(
                typeof(sbyte?),
                typeof(sbyte)
            );
            AddWideningConversion<short?>(
                typeof(byte?),
                typeof(byte),
                typeof(sbyte?),
                typeof(sbyte),
                typeof(short),
                typeof(short?)
            );
            AddWideningConversion<int?>(
                typeof(byte?),
                typeof(byte),
                typeof(sbyte?),
                typeof(sbyte),
                typeof(short?),
                typeof(short),
                typeof(ushort?),
                typeof(ushort),
                typeof(int),
                typeof(int?)
            );
            AddWideningConversion<long?>(
                typeof(byte?),
                typeof(byte),
                typeof(sbyte?),
                typeof(sbyte),
                typeof(short?),
                typeof(short),
                typeof(ushort?),
                typeof(ushort),
                typeof(int?),
                typeof(int),
                typeof(long?),
                typeof(long)
            );
            AddWideningConversion<ushort?>(
                typeof(byte?),
                typeof(byte),
                typeof(ushort?),
                typeof(ushort)
            );
            AddWideningConversion<uint?>(
                typeof(byte?),
                typeof(byte),
                typeof(ushort?),
                typeof(ushort),
                typeof(uint?),
                typeof(uint)
            );
            AddWideningConversion<ulong?>(
                typeof(byte?),
                typeof(byte),
                typeof(ushort?),
                typeof(ushort),
                typeof(uint?),
                typeof(uint),
                typeof(ulong?),
                typeof(ulong)
            );
            AddWideningConversion<float?>(
                typeof(byte?),
                typeof(byte),
                typeof(short?),
                typeof(short),
                typeof(int?),
                typeof(int),
                typeof(long?),
                typeof(long),
                typeof(sbyte?),
                typeof(sbyte),
                typeof(float?),
                typeof(float),
                typeof(ushort?),
                typeof(ushort),
                typeof(uint?),
                typeof(uint),
                typeof(ulong?),
                typeof(ulong)
            );
            AddWideningConversion<double?>(
                typeof(byte?),
                typeof(byte),
                typeof(short?),
                typeof(short),
                typeof(int?),
                typeof(int),
                typeof(long?),
                typeof(long),
                typeof(sbyte?),
                typeof(sbyte),
                typeof(float?),
                typeof(float),
                typeof(double?),
                typeof(double),
                typeof(ushort?),
                typeof(ushort),
                typeof(uint?),
                typeof(uint),
                typeof(ulong?),
                typeof(ulong)
            );
            AddWideningConversion<decimal?>(
                typeof(byte?),
                typeof(byte),
                typeof(short?),
                typeof(short),
                typeof(int?),
                typeof(int),
                typeof(long?),
                typeof(long),
                typeof(sbyte?),
                typeof(sbyte),
                typeof(float?),
                typeof(float),
                typeof(double?),
                typeof(double),
                typeof(decimal?),
                typeof(decimal),
                typeof(ushort?),
                typeof(ushort),
                typeof(uint?),
                typeof(uint),
                typeof(ulong?),
                typeof(ulong)
            );

            // Initialize the map of wrapper conversions
            var boolWrappers = InitWrappingConversions<bool, bool?>();
            var charWrappers = InitWrappingConversions<char, char?>();
            var byteWrappers = InitWrappingConversions<byte, byte?>();
            var sbyteWrappers = InitWrappingConversions<sbyte, sbyte?>();
            var shortWrappers = InitWrappingConversions<short, short?>();
            var ushortWrappers = InitWrappingConversions<ushort, ushort?>();
            var intWrappers = InitWrappingConversions<int, int?>();
            var uintWrappers = InitWrappingConversions<uint, uint?>();
            var longWrappers = InitWrappingConversions<long, long?>();
            var ulongWrappers = InitWrappingConversions<ulong, ulong?>();
            var floatWrappers = InitWrappingConversions<float, float?>();
            var doubleWrappers = InitWrappingConversions<double, double?>();
            var decimalWrappers = InitWrappingConversions<decimal, decimal?>();
            var bigIntWrappers = InitWrappingConversions<BigInteger, BigInteger?>();

#if false
	// Initialize the map of widening conversions
            var wideningConversions = new HashSet<Type>(byteWrappers);
            WIDENING_CONVERSIONS.Put(typeof(short), new HashSet<Type>(wideningConversions));
            WIDENING_CONVERSIONS.Put(typeof(short?), new HashSet<Type>(wideningConversions));

	wideningConversions.AddAll (shortWrappers);
	wideningConversions.AddAll (charWrappers);
            WIDENING_CONVERSIONS.Put(typeof(int), new HashSet<Type>(wideningConversions));
            WIDENING_CONVERSIONS.Put(typeof(int?), new HashSet<Type>(wideningConversions));

	wideningConversions.AddAll (intWrappers);
            WIDENING_CONVERSIONS.Put(typeof(long), new HashSet<Type>(wideningConversions));
            WIDENING_CONVERSIONS.Put(typeof(long?), new HashSet<Type>(wideningConversions));

wideningConversions.AddAll (longWrappers);
            WIDENING_CONVERSIONS.Put(typeof(float), new HashSet<Type>(wideningConversions));
            WIDENING_CONVERSIONS.Put(typeof(float?), new HashSet<Type>(wideningConversions));

wideningConversions.AddAll (floatWrappers);
            WIDENING_CONVERSIONS.Put(typeof(double), new HashSet<Type>(wideningConversions));
            WIDENING_CONVERSIONS.Put(typeof(double?), new HashSet<Type>(wideningConversions));

            wideningConversions.AddAll(doubleWrappers);
            WIDENING_CONVERSIONS.Put(typeof(decimal), new HashSet<Type>(wideningConversions));
            WIDENING_CONVERSIONS.Put(typeof(decimal?), new HashSet<Type>(wideningConversions));
#endif
        }

        private static void AddWideningConversion<T>(params Type[] sourceTypes)
        {
            WIDENING_CONVERSIONS.Put(typeof(T), new HashSet<Type>(sourceTypes));
        }

        /// <summary>
        /// Returns the allowable widening conversions.
        /// </summary>
        /// <value>
        /// map where key is the class that we are asking to be widened into, anda set of classes that can be widened from
        /// </value>
        public static IDictionary<Type, ICollection<Type>> WIDENING_CONVERSIONS { get; private set; }

        /// <summary>
        /// Attempts to find the static or instance method described by the parameters, or a method of the same name that
        /// will accept the same type of parameters.
        /// </summary>
        /// <param name="declaringClass">the class to search for the method</param>
        /// <param name="methodName">the name of the method</param>
        /// <param name="paramTypes">the parameter types for the method</param>
        /// <param name="allowInstance">true to allow instance methods as well, false to allow only static method</param>
        /// <param name="allowEventBeanCollType">whether event-bean-collection parameter type is allowed</param>
        /// <param name="allowEventBeanType">whether event-bean parameter type is allowed</param>
        /// <returns>- the Method object for this method</returns>
        /// <throws>MethodResolverNoSuchMethodException if the method could not be found</throws>
        public static MethodInfo ResolveMethod(
            Type declaringClass,
            string methodName,
            Type[] paramTypes,
            bool allowInstance,
            bool[] allowEventBeanType,
            bool[] allowEventBeanCollType)
        {
            // Get all the methods for this class
            var methods = declaringClass.GetMethods()
                .OrderBy(m => m.IsVarArgs() ? 1 : 0)
                .ToArray();

            MethodInfo bestMatch = null;
            MethodExecutableRank rank = null;

            // Examine each method, checking if the signature is compatible
            MethodInfo conversionFailedMethod = null;

            for (var mm = 0; mm < methods.Length; mm++) {
                var method = methods[mm];

                // Check the modifiers: we only want public and static, if required
                if (!IsPublicAndStatic(method, allowInstance)) {
                    continue;
                }

                if (!method.IsPublic) {
                    continue;
                }

                // Check the name
                if (method.Name != methodName) {
                    continue;
                }

                var parameterTypes = method.GetParameters()
                    .Select(p => p.ParameterType)
                    .ToArray();

                if (method.IsGenericMethod && method.IsVarArgs()) {
                    // we need to what arguments have been supplied for the
                    // remaining arguments since we need to coerce the remaining
                    // arguments to the same type
                    var commonArgs = paramTypes.Skip(paramTypes.Length - 1).ToArray();
                    var commonType = GetCommonCoersion(commonArgs);
                    if (commonArgs.Length == 1 && commonArgs[0].IsArray) {
                        // this is an annoying case where the inputs are an argument array...
                        // in this case we want to unpack the common coercion type from the
                        // underlying common args themselves.
                        commonType = commonArgs[0].GetElementType();
                    }

                    method = method.MakeGenericMethod(commonType);
                    parameterTypes = method.GetParameters()
                        .Select(p => p.ParameterType)
                        .ToArray();
                }

                // Check the parameter list
                var conversionCount = CompareParameterTypesAllowContext(
                    parameterTypes,
                    paramTypes,
                    allowEventBeanType,
                    allowEventBeanCollType,
                    parameterTypes, // method.GetGenericArguments(),
                    method.IsVarArgs()
                );

                // Parameters don't match
                if (conversionCount == -1) {
                    conversionFailedMethod = method;
                    continue;
                }

                // Parameters match exactly
                if (conversionCount == 0 && !method.IsVarArgs()) {
                    bestMatch = method;
                    break;
                }

                // No previous match
                if (bestMatch == null) {
                    bestMatch = method;
                    rank = new MethodExecutableRank(conversionCount, method.IsVarArgs());
                }
                else {
                    // Current match is better
                    if (rank.CompareTo(conversionCount, method.IsVarArgs()) == 1) {
                        bestMatch = method;
                        rank = new MethodExecutableRank(conversionCount, method.IsVarArgs());
                    }
                }
            }

            if (bestMatch != null) {
                LogWarnBoxedToPrimitiveType(declaringClass, methodName, bestMatch, paramTypes);
                return bestMatch;
            }

            var parametersPretty = GetParametersPretty(paramTypes);
            throw new MethodResolverNoSuchMethodException(
                "Unknown method " + declaringClass.Name + '.' + methodName + '(' + parametersPretty + ')',
                conversionFailedMethod);
        }

        public static MethodInfo ResolveExtensionMethod(
            Type declaringClass,
            string methodName,
            Type[] paramTypes,
            bool allowInstance,
            bool[] allowEventBeanType,
            bool[] allowEventBeanCollType)
        {
            var extensionMethods = declaringClass.GetExtensionMethods(methodName);
            foreach (var method in extensionMethods) {
                var parameterTypes = method.GetParameters().Select(p => p.ParameterType).Skip(1).ToArray();

                // Check the parameter list
                var conversionCount = CompareParameterTypesAllowContext(
                    parameterTypes,
                    paramTypes,
                    allowEventBeanType,
                    allowEventBeanCollType,
                    parameterTypes, // method.GetGenericArguments(),
                    method.IsVarArgs()
                );

                // Parameters match exactly
                if (conversionCount == 0) {
                    return method;
                }
            }

            return null;
        }

        private static string GetParametersPretty(Type[] paramTypes)
        {
            var parameters = new StringBuilder();
            if (paramTypes != null && paramTypes.Length != 0) {
                var appendString = "";
                foreach (object param in paramTypes) {
                    parameters.Append(appendString);
                    if (param == null) {
                        parameters.Append("(null)");
                    }
                    else {
                        parameters.Append(param);
                    }

                    appendString = ", ";
                }
            }

            return parameters.ToString();
        }

        private static void LogWarnBoxedToPrimitiveType(
            Type declaringClass,
            string methodName,
            MethodInfo bestMatch,
            Type[] paramTypes)
        {
            var parametersMethod = bestMatch.GetParameterTypes();
            for (var i = 0; i < parametersMethod.Length; i++) {
                var paramMethod = parametersMethod[i];
                if (!paramMethod.IsPrimitive) {
                    continue;
                }

                var paramType = paramTypes[i];
                var paramNull = paramType == null;
                // if null-type parameter, or non-CLR class and boxed type matches
                if (paramNull ||
                    (!declaringClass.GetType().Name.StartsWith("System") &&
                     paramMethod.GetBoxedType() == paramType)) {
                    var paramTypeStr = paramNull ? "null" : paramType.Name;
                    Log.Info(
                        "Method '" +
                        methodName +
                        "' in class '" +
                        declaringClass.CleanName() +
                        "' expects primitive type '" +
                        parametersMethod[i] +
                        "' as parameter " +
                        i +
                        ", but receives a nullable (boxed) type " +
                        paramTypeStr +
                        ". This may cause null pointer exception at runtime if the actual value is null, please consider using boxed types for method parameters.");
                    return;
                }
            }
        }

                private static Type GetCommonCoersion(IList<Type> typeList)
        {
            var typeHash = new HashSet<Type>();

            typeList[0].Visit(t => typeHash.Add(t));

            for (var ii = 1; ii < typeList.Count; ii++) {
                var moreTypes = new HashSet<Type>();
                typeList[ii].Visit(t => moreTypes.Add(t));
                typeHash.IntersectWith(moreTypes);
            }

            // What we are left with is a set of coercable types.  This will include
            // System.Object which is the defacto fallback when there are no other
            // types that have a stronger claim.
            var interfaces = typeHash
                .Where(t => t.IsInterface)
                .ToList();

            var concretes = typeHash
                .Where(t => t.IsInterface == false)
                .ToList();

            // We should never have a case where the concrete count is zero.  This would
            // indicate that even System.Object was not found as a common class...
            if (concretes.Count == 0) {
                throw new EPRuntimeException("Unable to find common concrete root for type");
            }

            // Concrete commonality with a count of one is going to be fairly common
            // and almost always reflects the case where System.Object is only class
            // that could be found.
            concretes.Remove(typeof(object));
            if (concretes.Count == 0) {
                // Look for an interface that might provide a better binding ... if none can
                // be found then use System.Object as the common coercion.
                if (interfaces.Count == 0) {
                    return typeof(object);
                }

                // Now the only thing to be concerned about with interfaces are constraints
                // that might be set somewhere else, like the parameters.  We will revisit
                // that bit of code should it become something we need to handle.
                return interfaces.First();
            }

            // We have multiple concrete classes ... none of which are System.Object.  As with
            // interfaces, what we have to concern ourselves with is a constraint that may
            // be in play elsewhere.  We will revisit that bit of code should it become
            // something we need to handle.

            return concretes.First();
        }
        
        private static bool IsWideningConversion(
            Type declarationType,
            Type invocationType)
        {
            if (WIDENING_CONVERSIONS.ContainsKey(declarationType)) {
                return WIDENING_CONVERSIONS.Get(declarationType).Contains(invocationType);
            }
            else {
                return false;
            }
        }

        private static bool IsPublicAndStatic(
            MethodInfo method,
            bool allowInstance)
        {
            if (allowInstance) {
                return method.IsPublic;
            }

            return method.IsPublic && method.IsStatic;
        }

        private static int CompareParameterTypesAllowContext(
            Type[] declarationParameters,
            Type[] invocationParameters,
            bool[] optionalAllowEventBeanType,
            bool[] optionalAllowEventBeanCollType,
            Type[] genericParameterTypes,
            bool isVarargs)
        {
            // determine if the last parameter is EPLMethodInvocationContext (no varargs)
            var declaredNoContext = declarationParameters;
            if (!isVarargs &&
                declarationParameters.Length > 0 &&
                declarationParameters[^1] == typeof(EPLMethodInvocationContext)) {
                declaredNoContext = declarationParameters.Take(declarationParameters.Length - 1).ToArray();
            }

            // determine if the previous-to-last parameter is EPLMethodInvocationContext (varargs-only)
            if (isVarargs &&
                declarationParameters.Length > 1 &&
                declarationParameters[^2] == typeof(EPLMethodInvocationContext)) {
                var rewritten = new Type[declarationParameters.Length - 1];
                Array.Copy(declarationParameters, 0, rewritten, 0, declarationParameters.Length - 2);
                rewritten[^1] = declarationParameters[^1];
                declaredNoContext = rewritten;
            }

            return CompareParameterTypesNoContext(
                declaredNoContext,
                invocationParameters,
                optionalAllowEventBeanType,
                optionalAllowEventBeanCollType,
                genericParameterTypes,
                isVarargs);
        }

        // Returns -1 if the invocation parameters aren't applicable
        // to the method. Otherwise returns the number of parameters
        // that have to be converted
        private static int CompareParameterTypesNoContext(
            Type[] declarationParameters,
            Type[] invocationParameters,
            bool[] optionalAllowEventBeanType,
            bool[] optionalAllowEventBeanCollType,
            Type[] genericParameterTypes,
            bool isVarargs)
        {
            if (invocationParameters == null) {
                return declarationParameters.Length == 0 ? 0 : -1;
            }

            // handle varargs
            if (isVarargs) {
                if (invocationParameters.Length < declarationParameters.Length - 1) {
                    return -1;
                }

                if (invocationParameters.Length == 0) {
                    return 0;
                }

                var conversionCount = new AtomicLong();

                // check declared types (non-vararg)
                for (var i = 0; i < declarationParameters.Length - 1; i++) {
                    var compatible = CompareParameterTypeCompatible(
                        invocationParameters[i],
                        declarationParameters[i],
                        optionalAllowEventBeanType?[i],
                        optionalAllowEventBeanCollType?[i],
                        genericParameterTypes[i],
                        conversionCount);
                    if (!compatible) {
                        return -1;
                    }
                }

                var varargDeclarationParameter = declarationParameters[^1].GetElementType();

                // handle array of compatible type passed into vararg
                if (invocationParameters.Length == declarationParameters.Length) {
                    var lastType = invocationParameters[^1];
                    if (lastType != null && lastType.IsArray()) {
                        if (lastType.GetElementType() == varargDeclarationParameter) {
                            return (int)conversionCount.Get();
                        }

                        if (TypeHelper.IsSubclassOrImplementsInterface(
                                lastType.GetElementType(),
                                varargDeclarationParameter)) {
                            conversionCount.IncrementAndGet();
                            return (int)conversionCount.Get();
                        }
                    }
                }

                // handle compatible types passed into vararg
                var varargGenericParameterTypes = genericParameterTypes[^1];
                for (var i = declarationParameters.Length - 1; i < invocationParameters.Length; i++) {
                    var compatible = CompareParameterTypeCompatible(
                        invocationParameters[i],
                        varargDeclarationParameter,
                        optionalAllowEventBeanType?[i],
                        optionalAllowEventBeanCollType?[i],
                        varargGenericParameterTypes,
                        conversionCount);
                    if (!compatible) {
                        return -1;
                    }
                }

                return (int)conversionCount.Get();
            }

            // handle non-varargs
            if (declarationParameters.Length != invocationParameters.Length) {
                return -1;
            }

            var conversionCountX = new AtomicLong();
            for (var i = 0; i < declarationParameters.Length; i++) {
                var compatible = CompareParameterTypeCompatible(
                    invocationParameters[i],
                    declarationParameters[i],
                    optionalAllowEventBeanType?[i],
                    optionalAllowEventBeanCollType?[i],
                    genericParameterTypes[i],
                    conversionCountX);
                if (!compatible) {
                    return -1;
                }
            }

            return (int)conversionCountX.Get();
        }

        private static bool CompareParameterTypeCompatible(
            Type invocationParameter,
            Type declarationParameter,
            bool? optionalAllowEventBeanType,
            bool? optionalAllowEventBeanCollType,
            Type genericParameterType,
            AtomicLong conversionCount)
        {
            if (invocationParameter == null) {
                return !declarationParameter.CanBeNull();
            }

            if (optionalAllowEventBeanType != null &&
                declarationParameter == typeof(EventBean) &&
                optionalAllowEventBeanType.GetValueOrDefault()) {
                return true;
            }

            if (optionalAllowEventBeanCollType != null &&
                declarationParameter == typeof(ICollection<EventBean>) &&
                optionalAllowEventBeanCollType.GetValueOrDefault(false) &&
                genericParameterType.GetGenericType(0) == typeof(EventBean)) {
                return true;
            }

            if (!IsIdentityConversion(declarationParameter, invocationParameter)) {
                conversionCount.IncrementAndGet();
                if (!IsWideningConversion(declarationParameter, invocationParameter) &&
                    declarationParameter != typeof(object)) {
                    return false;
                }
            }

            return true;
        }

        // Identity conversion means no conversion, wrapper conversion,
        // or conversion to a supertype
        private static bool IsIdentityConversion(
            Type declarationType,
            Type invocationType)
        {
            if (WrappingConversions.TryGetValue(declarationType, out var wrappingConversion)) {
                return wrappingConversion.Contains(invocationType) || declarationType.IsAssignableFrom(invocationType);
            }

            if (invocationType == null) {
                return declarationType.CanBeNull();
            }

            if (invocationType.IsPrimitive) {
                invocationType = invocationType.GetBoxedType();
            }

            return declarationType.IsAssignableFrom(invocationType);
        }

        public static ConstructorInfo ResolveCtor(
            Type declaringClass,
            Type[] paramTypes)
        {
            // Get all the methods for this class
            var ctors = declaringClass.GetConstructors();

            ConstructorInfo bestMatch = null;
            MethodExecutableRank rank = null;

            // Examine each method, checking if the signature is compatible
            ConstructorInfo conversionFailedCtor = null;
            foreach (var ctor in ctors) {
                // Check the modifiers: we only want public
                if (!ctor.IsPublic) {
                    continue;
                }

                var isVarArgs = ctor.IsVarArgs();

                // Check the parameter list
                var constructorParameters = ctor.GetParameters().Select(p => p.ParameterType).ToArray();
                var conversionCount = CompareParameterTypesNoContext(
                    constructorParameters,
                    paramTypes,
                    null,
                    null,
                    constructorParameters,
                    isVarArgs);

                // MSDN
                //
                // NotSupportedException - The current object is a ConstructorInfo. Generic constructors are not
                // supported in the .NET Framework version 2.0. This exception is the default behavior if this method
                // is not overridden in a derived class.

                // Parameters don't match
                if (conversionCount == -1) {
                    conversionFailedCtor = ctor;
                    continue;
                }

                // Parameters match exactly
                if (conversionCount == 0 && !isVarArgs) {
                    bestMatch = ctor;
                    break;
                }

                // No previous match
                if (bestMatch == null) {
                    bestMatch = ctor;
                    rank = new MethodExecutableRank(conversionCount, isVarArgs);
                }
                else {
                    // Current match is better
                    if (rank.CompareTo(conversionCount, isVarArgs) == 1) {
                        bestMatch = ctor;
                        rank = new MethodExecutableRank(conversionCount, isVarArgs);
                    }
                }
            }

            if (bestMatch != null) {
                return bestMatch;
            }

            var paramList = new StringBuilder();
            var message = "Constructor not found for " + declaringClass.Name + " taking ";
            if (paramTypes != null && paramTypes.Length != 0) {
                var appendString = "";
                foreach (var param in paramTypes) {
                    paramList.Append(appendString);
                    if (param == null) {
                        paramList.Append("(null)");
                    }
                    else {
                        paramList.Append(param.ToString());
                    }

                    appendString = ", ";
                }

                message += "('" + paramList + "')'";
            }
            else {
                message += "no parameters";
            }

            throw new MethodResolverNoSuchCtorException(message, conversionFailedCtor);
        }

        public static CodegenExpression ResolveMethodCodegenExactNonStatic(MethodInfo method)
        {
            return StaticMethod(
                typeof(MethodResolver),
                "ResolveMethodExactNonStatic",
                Constant(method.DeclaringType),
                Constant(method.Name),
                Constant(method.GetParameterTypes()));
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="declaringClass">declaring class</param>
        /// <param name="methodName">method name</param>
        /// <param name="parameters">parameters</param>
        /// <returns>method</returns>
        public static MethodInfo ResolveMethodExactNonStatic(
            Type declaringClass,
            string methodName,
            Type[] parameters)
        {
            try {
                var method = declaringClass.GetMethod(methodName, parameters);
                if (method.IsStatic) {
                    throw new EPException("Not an instance method");
                }

                return method;
            }
            catch (Exception ex) {
                var parametersPretty = GetParametersPretty(parameters);
                throw new EPException(
                    "Failed to resolve static method " +
                    declaringClass.Name +
                    '.' +
                    methodName +
                    '(' +
                    parametersPretty +
                    ": " +
                    ex.Message,
                    ex);
            }
        }
    }
} // end of namespace