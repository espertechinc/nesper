///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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

using com.espertech.esper.client;
using com.espertech.esper.client.hook;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.core;

namespace com.espertech.esper.util
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
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly IDictionary<Type, ICollection<Type>> WrappingConversions =
            new Dictionary<Type, ICollection<Type>>();

        static HashSet<Type> InitWrappingConversions<TX, TXN>()
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

            AddWideningConversion<Byte>(
                typeof(Byte),
                typeof(Nullable<Byte>)
            );
            AddWideningConversion<SByte>(
                typeof(Nullable<SByte>),
                typeof(SByte)
            );
            AddWideningConversion<Int16>(
                typeof(Byte),
                typeof(Int16),
                typeof(Nullable<Byte>),
                typeof(Nullable<Int16>),
                typeof(Nullable<SByte>),
                typeof(SByte)
            );
            AddWideningConversion<Int32>(
                typeof(Byte),
                typeof(Int16),
                typeof(Int32),
                typeof(Nullable<Byte>),
                typeof(Nullable<Int16>),
                typeof(Nullable<Int32>),
                typeof(Nullable<SByte>),
                typeof(Nullable<UInt16>),
                typeof(SByte),
                typeof(UInt16)
            );
            AddWideningConversion<Int64>(
                typeof(Byte),
                typeof(Int16),
                typeof(Int32),
                typeof(Int64),
                typeof(Nullable<Byte>),
                typeof(Nullable<Int16>),
                typeof(Nullable<Int32>),
                typeof(Nullable<Int64>),
                typeof(Nullable<SByte>),
                typeof(Nullable<UInt16>),
                typeof(Nullable<UInt32>),
                typeof(SByte),
                typeof(UInt16),
                typeof(UInt32)
            );
            AddWideningConversion<UInt16>(
                typeof(Byte),
                typeof(Nullable<Byte>),
                typeof(Nullable<UInt16>),
                typeof(UInt16)
            );
            AddWideningConversion<UInt32>(
                typeof(Byte),
                typeof(Nullable<Byte>),
                typeof(Nullable<UInt16>),
                typeof(Nullable<UInt32>),
                typeof(UInt16),
                typeof(UInt32)
            );
            AddWideningConversion<UInt64>(
                typeof(Byte),
                typeof(Nullable<Byte>),
                typeof(Nullable<UInt16>),
                typeof(Nullable<UInt32>),
                typeof(Nullable<UInt64>),
                typeof(UInt16),
                typeof(UInt32),
                typeof(UInt64)
            );
            AddWideningConversion<Single>(
                typeof(Byte),
                typeof(Int16),
                typeof(Int32),
                typeof(Int64),
                typeof(Nullable<Byte>),
                typeof(Nullable<Int16>),
                typeof(Nullable<Int32>),
                typeof(Nullable<Int64>),
                typeof(Nullable<SByte>),
                typeof(Nullable<Single>),
                typeof(Nullable<UInt16>),
                typeof(Nullable<UInt32>),
                typeof(Nullable<UInt64>),
                typeof(SByte),
                typeof(Single),
                typeof(UInt16),
                typeof(UInt32),
                typeof(UInt64)
            );
            AddWideningConversion<Double>(
                typeof(Byte),
                typeof(Double),
                typeof(Int16),
                typeof(Int32),
                typeof(Int64),
                typeof(Nullable<Byte>),
                typeof(Nullable<Double>),
                typeof(Nullable<Int16>),
                typeof(Nullable<Int32>),
                typeof(Nullable<Int64>),
                typeof(Nullable<SByte>),
                typeof(Nullable<Single>),
                typeof(Nullable<UInt16>),
                typeof(Nullable<UInt32>),
                typeof(Nullable<UInt64>),
                typeof(SByte),
                typeof(Single),
                typeof(UInt16),
                typeof(UInt32),
                typeof(UInt64)
            );
            AddWideningConversion<Decimal>(
                typeof(Decimal),
                typeof(Nullable<Decimal>)
            );
            AddWideningConversion<Char>(
                typeof(Byte),
                typeof(Nullable<Byte>),
                typeof(Nullable<UInt16>),
                typeof(UInt16)
            );
            AddWideningConversion<Nullable<Byte>>(
                typeof(Byte),
                typeof(Nullable<Byte>)
            );
            AddWideningConversion<Nullable<SByte>>(
                typeof(Nullable<SByte>),
                typeof(SByte)
            );
            AddWideningConversion<Nullable<Int16>>(
                typeof(Int16),
                typeof(Nullable<Int16>)
            );
            AddWideningConversion<Nullable<Int64>>(
                typeof(Int32),
                typeof(Int64),
                typeof(Nullable<Int64>)
            );
            AddWideningConversion<Nullable<Int32>>(
                typeof(Int32),
                typeof(Nullable<Int32>)
            );
            AddWideningConversion<Nullable<UInt16>>(
                typeof(Nullable<UInt16>),
                typeof(UInt16)
            );
            AddWideningConversion<Nullable<UInt32>>(
                typeof(Nullable<UInt32>),
                typeof(UInt32)
            );
            AddWideningConversion<Nullable<UInt64>>(
                typeof(Nullable<UInt64>),
                typeof(UInt64)
            );
            AddWideningConversion<Nullable<Single>>(
                typeof(Nullable<Single>),
                typeof(Single)
            );
            AddWideningConversion<Nullable<Double>>(
                typeof(Double),
                typeof(Nullable<Double>)
            );
            AddWideningConversion<Nullable<Decimal>>(
                typeof(Decimal),
                typeof(Nullable<Decimal>)
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

            wideningConversions.AddAll(shortWrappers);
            wideningConversions.AddAll(charWrappers);
            WIDENING_CONVERSIONS.Put(typeof(int), new HashSet<Type>(wideningConversions));
            WIDENING_CONVERSIONS.Put(typeof(int?), new HashSet<Type>(wideningConversions));

            wideningConversions.AddAll(intWrappers);
            WIDENING_CONVERSIONS.Put(typeof(long), new HashSet<Type>(wideningConversions));
            WIDENING_CONVERSIONS.Put(typeof(long?), new HashSet<Type>(wideningConversions));

            wideningConversions.AddAll(longWrappers);
            WIDENING_CONVERSIONS.Put(typeof(float), new HashSet<Type>(wideningConversions));
            WIDENING_CONVERSIONS.Put(typeof(float?), new HashSet<Type>(wideningConversions));

            wideningConversions.AddAll(floatWrappers);
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
        /// Attempts to find the static or instance method described by the parameters, or a method of the same name that will accept the same type of parameters.
        /// </summary>
        /// <param name="declaringClass">the class to search for the method</param>
        /// <param name="methodName">the name of the method</param>
        /// <param name="paramTypes">the parameter types for the method</param>
        /// <param name="allowInstance">true to allow instance methods as well, false to allow only static method</param>
        /// <param name="allowEventBeanType">Type of the allow event bean.</param>
        /// <param name="allowEventBeanCollType">Type of the allow event bean coll.</param>
        /// <returns>- the Method object for this method</returns>
        /// <throws>EngineNoSuchMethodException if the method could not be found</throws>
        public static MethodInfo ResolveMethod(
            Type declaringClass,
            String methodName,
            Type[] paramTypes,
            bool allowInstance,
            bool[] allowEventBeanType,
            bool[] allowEventBeanCollType)
        {
            // Get all the methods for this class
            MethodInfo[] methods = declaringClass.GetMethods()
                .OrderBy(m => m.IsVarArgs() ? 1 : 0)
                .ToArray();

            MethodInfo bestMatch = null;
            var bestConversionCount = -1;

            // Examine each method, checking if the signature is compatible
            MethodInfo conversionFailedMethod = null;

            for (int mm = 0; mm < methods.Length; mm++)
            {
                var method = methods[mm];

                // Check the modifiers: we only want public and static, if required
                if (!IsPublicAndStatic(method, allowInstance))
                {
                    continue;
                }

                // Check the name
                if (method.Name != methodName)
                {
                    continue;
                }

                var parameterTypes = method.GetParameters()
                    .Select(p => p.ParameterType)
                    .ToArray();

                if (method.IsGenericMethod && method.IsVarArgs())
                {
                    // we need to what arguments have been supplied for the
                    // remaining arguments since we need to coerce the remaining
                    // arguments to the same type
                    var commonArgs = paramTypes.Skip(paramTypes.Length - 1).ToArray();
                    var commonType = GetCommonCoersion(commonArgs);
                    if (commonArgs.Length == 1 && commonArgs[0].IsArray)
                    {
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
                int conversionCount = CompareParameterTypesAllowContext(
                    parameterTypes,
                    paramTypes,
                    allowEventBeanType,
                    allowEventBeanCollType,
                    parameterTypes, // method.GetGenericArguments(),
                    method.IsVarArgs()
                    );

                // Parameters don't match
                if (conversionCount == -1)
                {
                    conversionFailedMethod = method;
                    continue;
                }

                // Parameters match exactly
                if (conversionCount == 0)
                {
                    bestMatch = method;
                    break;
                }

                // No previous match
                if (bestMatch == null)
                {
                    bestMatch = method;
                    bestConversionCount = conversionCount;
                }
                else
                {
                    // Current match is better
                    if (conversionCount < bestConversionCount)
                    {
                        bestMatch = method;
                        bestConversionCount = conversionCount;
                    }
                }
            }

            if (bestMatch != null)
            {
                LogWarnBoxedToPrimitiveType(declaringClass, methodName, bestMatch, paramTypes);
                return bestMatch;
            }

            var paramList = new StringBuilder();
            if (paramTypes != null && paramTypes.Length != 0)
            {
                var appendString = "";
                foreach (var param in paramTypes)
                {
                    paramList.Append(appendString);
                    if (param == null)
                    {
                        paramList.Append("(null)");
                    }
                    else
                    {
                        paramList.Append(param.ToString());
                    }
                    appendString = ", ";
                }
            }

            throw new EngineNoSuchMethodException("Unknown method " + declaringClass.Name + '.' + methodName + '(' + paramList + ')', conversionFailedMethod);
        }

        public static MethodInfo ResolveExtensionMethod(Type declaringClass, String methodName, Type[] paramTypes, bool allowInstance, bool[] allowEventBeanType, bool[] allowEventBeanCollType)
        {
            var extensionMethods = TypeHelper.GetExtensionMethods(declaringClass)
                .Where(m => m.Name == methodName);
            foreach (var method in extensionMethods)
            {
                var parameterTypes = method.GetParameters().Select(p => p.ParameterType).Skip(1).ToArray();

                // Check the parameter list
                int conversionCount = CompareParameterTypesAllowContext(
                    parameterTypes,
                    paramTypes,
                    allowEventBeanType,
                    allowEventBeanCollType,
                    parameterTypes, // method.GetGenericArguments(),
                    method.IsVarArgs()
                    );

                // Parameters match exactly
                if (conversionCount == 0)
                {
                    return method;
                }
            }

            return null;
        }

        private static void LogWarnBoxedToPrimitiveType(Type declaringClass, String methodName, MethodInfo bestMatch, Type[] paramTypes)
        {
            var parametersMethod = bestMatch.GetParameters().Select(p => p.ParameterType).ToArray();
            for (int i = 0; i < parametersMethod.Length; i++)
            {
                if (!parametersMethod[i].IsPrimitive)
                {
                    continue;
                }
                // if null-type parameter, or non-CLR class and boxed type matches
                if (paramTypes[i] == null || (!declaringClass.GetType().FullName.StartsWith("System.") && (parametersMethod[i].GetBoxedType()) == paramTypes[i]))
                {
                    String paramTypeStr = paramTypes[i] == null ? "null" : paramTypes[i].Name;
                    Log.Info(
                        "Method '{0}' in class '{1}' expects primitive type '{2}' as parameter {3}, but receives a nullable (boxed) type {4}. This may cause null pointer exception at runtime if the actual value is null, please consider using boxed types for method parameters.",
                        methodName, declaringClass.Name, parametersMethod[i], i, paramTypeStr);
                    return;
                }
            }
        }

        private static Type GetCommonCoersion(IList<Type> typeList)
        {
            var typeHash = new HashSet<Type>();

            typeList[0].Visit(t => typeHash.Add(t));

            for (int ii = 1; ii < typeList.Count; ii++)
            {
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
            if (concretes.Count == 0)
                throw new EPRuntimeException("Unable to find common concrete root for type");

            // Concrete commonality with a count of one is going to be fairly common
            // and almost always reflects the case where System.Object is only class
            // that could be found.
            concretes.Remove(typeof(object));
            if (concretes.Count == 0)
            {
                // Look for an interface that might provide a better binding ... if none can
                // be found then use System.Object as the common coercion.
                if (interfaces.Count == 0)
                {
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

        private static bool IsWideningConversion(Type declarationType, Type invocationType)
        {
            return
                WIDENING_CONVERSIONS.ContainsKey(declarationType) &&
                WIDENING_CONVERSIONS.Get(declarationType).Contains(invocationType);
        }

        private static bool IsPublicAndStatic(MethodInfo method, bool allowInstance)
        {
            if (allowInstance)
            {
                return method.IsPublic;
            }
            return method.IsPublic && method.IsStatic;
        }

        private static int CompareParameterTypesAllowContext(
            Type[] declarationParameters,
            Type[] invocationParameters,
            Boolean[] optionalAllowEventBeanType,
            Boolean[] optionalAllowEventBeanCollType,
            Type[] genericParameterTypes,
            bool isVarArgs)
        {
            // determine if the last parameter is EPLMethodInvocationContext (no varargs)
            var declaredNoContext = declarationParameters;
            if (!isVarArgs &&
                declarationParameters.Length > 0 &&
                declarationParameters[declarationParameters.Length - 1] == typeof(EPLMethodInvocationContext))
            {
                declaredNoContext = declarationParameters.Take(declarationParameters.Length - 1).ToArray();
            }

            // determine if the previous-to-last parameter is EPLMethodInvocationContext (varargs-only)
            if (isVarArgs &&
                declarationParameters.Length > 1 &&
                declarationParameters[declarationParameters.Length - 2] == typeof(EPLMethodInvocationContext))
            {
                var rewritten = new Type[declarationParameters.Length - 1];
                Array.Copy(declarationParameters, 0, rewritten, 0, declarationParameters.Length - 2);
                rewritten[rewritten.Length - 1] = declarationParameters[declarationParameters.Length - 1];
                declaredNoContext = rewritten;
            }

            return CompareParameterTypesNoContext(
                declaredNoContext,
                invocationParameters,
                optionalAllowEventBeanType,
                optionalAllowEventBeanCollType,
                genericParameterTypes,
                isVarArgs);
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
            bool isVarArgs)
        {
            if (invocationParameters == null)
            {
                return declarationParameters.Length == 0 ? 0 : -1;
            }

            AtomicLong conversionCount;

            // handle varargs
            if (isVarArgs)
            {
                if (invocationParameters.Length < declarationParameters.Length - 1)
                {
                    return -1;
                }
                if (invocationParameters.Length == 0)
                {
                    return 0;
                }

                conversionCount = new AtomicLong();

                // check declared types (non-vararg)
                for (int i = 0; i < declarationParameters.Length - 1; i++)
                {
                    var compatible = CompareParameterTypeCompatible(
                        invocationParameters[i],
                        declarationParameters[i],
                        optionalAllowEventBeanType == null ? (bool?)null : optionalAllowEventBeanType[i],
                        optionalAllowEventBeanCollType == null ? (bool?)null : optionalAllowEventBeanCollType[i],
                        genericParameterTypes[i],
                        conversionCount
                        );

                    if (!compatible)
                    {
                        return -1;
                    }
                }

                var varargDeclarationParameter = declarationParameters[declarationParameters.Length - 1].GetElementType();

                // handle array of compatible type passed into vararg
                if (invocationParameters.Length == declarationParameters.Length)
                {
                    var providedType = invocationParameters[invocationParameters.Length - 1];
                    if (providedType != null && providedType.IsArray())
                    {
                        if (providedType.GetElementType() == varargDeclarationParameter)
                        {
                            return (int)conversionCount.Get();
                        }
                        if (TypeHelper.IsSubclassOrImplementsInterface(providedType.GetElementType(), varargDeclarationParameter))
                        {
                            conversionCount.IncrementAndGet();
                            return (int)conversionCount.Get();
                        }
                    }
                }

                // handle compatible types passed into vararg
                Type varargGenericParameterTypes = genericParameterTypes[genericParameterTypes.Length - 1];
                for (int i = declarationParameters.Length - 1; i < invocationParameters.Length; i++)
                {
                    var compatible = CompareParameterTypeCompatible(
                        invocationParameters[i],
                        varargDeclarationParameter,
                        optionalAllowEventBeanType == null ? (bool?)null : optionalAllowEventBeanType[i],
                        optionalAllowEventBeanCollType == null ? (bool?)null : optionalAllowEventBeanCollType[i],
                        varargGenericParameterTypes,
                        conversionCount);
                    if (!compatible)
                    {
                        return -1;
                    }
                }
                return (int)conversionCount.Get();
            }

            // handle non-varargs
            if (declarationParameters.Length != invocationParameters.Length)
            {
                return -1;
            }

            conversionCount = new AtomicLong();
            for (int i = 0; i < declarationParameters.Length; i++)
            {
                var compatible = CompareParameterTypeCompatible(
                    invocationParameters[i],
                    declarationParameters[i],
                    optionalAllowEventBeanType == null ? (bool?)null : optionalAllowEventBeanType[i],
                    optionalAllowEventBeanCollType == null ? (bool?)null : optionalAllowEventBeanCollType[i],
                    genericParameterTypes[i],
                    conversionCount);
                if (!compatible)
                {
                    return -1;
                }
            }
            return (int)conversionCount.Get();
        }

        private static bool CompareParameterTypeCompatible(
            Type invocationParameter,
            Type declarationParameter,
            bool? optionalAllowEventBeanType,
            bool? optionalAllowEventBeanCollType,
            Type genericParameterType,
            AtomicLong conversionCount)
        {
            if ((invocationParameter == null) && !(declarationParameter.IsPrimitive))
            {
                return true;
            }

            if (optionalAllowEventBeanType != null &&
                declarationParameter == typeof(EventBean) &&
                optionalAllowEventBeanType.GetValueOrDefault())
            {
                return true;
            }

            if (optionalAllowEventBeanCollType != null &&
                declarationParameter == typeof(ICollection<EventBean>) &&
                optionalAllowEventBeanCollType.GetValueOrDefault(false) &&
                genericParameterType.GetGenericType(0) == typeof(EventBean))
            {
                return true;
            }

            if (!IsIdentityConversion(declarationParameter, invocationParameter))
            {
                conversionCount.IncrementAndGet();
                if (!IsWideningConversion(declarationParameter, invocationParameter))
                {
                    return false;
                }
            }

            return true;
        }

        // Identity conversion means no conversion, wrapper conversion,
        // or conversion to a supertype
        private static bool IsIdentityConversion(Type declarationType, Type invocationType)
        {
            if (WrappingConversions.ContainsKey(declarationType))
            {
                return WrappingConversions.Get(declarationType).Contains(invocationType) || declarationType.IsAssignableFrom(invocationType);
            }
            if (invocationType == null)
            {
                return !declarationType.IsPrimitive;
            }
            return declarationType.IsAssignableFrom(invocationType);
        }

        public static ConstructorInfo ResolveCtor(Type declaringClass, Type[] paramTypes)
        {
            // Get all the methods for this class
            ConstructorInfo[] ctors = declaringClass.GetConstructors();

            ConstructorInfo bestMatch = null;
            int bestConversionCount = -1;

            // Examine each method, checking if the signature is compatible
            ConstructorInfo conversionFailedCtor = null;
            foreach (ConstructorInfo ctor in ctors)
            {
                // Check the modifiers: we only want public
                if (!ctor.IsPublic)
                {
                    continue;
                }

                // Check the parameter list
                var constructorParameters = ctor.GetParameters().Select(p => p.ParameterType).ToArray();
                int conversionCount = CompareParameterTypesNoContext(
                    constructorParameters,
                    paramTypes, null, null,
                    constructorParameters, // ctor.GetGenericArguments());
                    ctor.IsVarArgs());

                // MSDN
                //
                // NotSupportedException - The current object is a ConstructorInfo. Generic constructors are not 
                // supported in the .NET Framework version 2.0. This exception is the default behavior if this method
                // is not overridden in a derived class.

                // Parameters don't match
                if (conversionCount == -1)
                {
                    conversionFailedCtor = ctor;
                    continue;
                }

                // Parameters match exactly
                if (conversionCount == 0)
                {
                    bestMatch = ctor;
                    break;
                }

                // No previous match
                if (bestMatch == null)
                {
                    bestMatch = ctor;
                    bestConversionCount = conversionCount;
                }
                else
                {
                    // Current match is better
                    if (conversionCount < bestConversionCount)
                    {
                        bestMatch = ctor;
                        bestConversionCount = conversionCount;
                    }
                }

            }

            if (bestMatch != null)
            {
                return bestMatch;
            }

            var paramList = new StringBuilder();
            var message = "Constructor not found for " + declaringClass.Name + " taking ";
            if (paramTypes != null && paramTypes.Length != 0)
            {
                var appendString = "";
                foreach (var param in paramTypes)
                {
                    paramList.Append(appendString);
                    if (param == null)
                    {
                        paramList.Append("(null)");
                    }
                    else
                    {
                        paramList.Append(param.ToString());
                    }
                    appendString = ", ";
                }
                message += "('" + paramList + "')'";
            }
            else
            {
                message += "no parameters";
            }
            throw new EngineNoSuchCtorException(message, conversionFailedCtor);
        }
    }
}
