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

using com.espertech.esper.client;
using com.espertech.esper.client.hook;
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
    
        static HashSet<Type> InitWrappingConversions<TX,TXN>()
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
    	    WideningConversions = new Dictionary<Type, ICollection<Type>>();

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

    		// Initialize the map of widening conversions
            var wideningConversions = new HashSet<Type>(byteWrappers);
    		WideningConversions.Put(typeof(short), new HashSet<Type>(wideningConversions));
    		WideningConversions.Put(typeof(short?), new HashSet<Type>(wideningConversions));
    
    		wideningConversions.AddAll(shortWrappers);
    		wideningConversions.AddAll(charWrappers);
    		WideningConversions.Put(typeof(int), new HashSet<Type>(wideningConversions));
    		WideningConversions.Put(typeof(int?), new HashSet<Type>(wideningConversions));
    
    		wideningConversions.AddAll(intWrappers);
    		WideningConversions.Put(typeof(long), new HashSet<Type>(wideningConversions));
    		WideningConversions.Put(typeof(long?), new HashSet<Type>(wideningConversions));
    
    		wideningConversions.AddAll(longWrappers);
    		WideningConversions.Put(typeof(float), new HashSet<Type>(wideningConversions));
    		WideningConversions.Put(typeof(float?), new HashSet<Type>(wideningConversions));
    
    		wideningConversions.AddAll(floatWrappers);
    		WideningConversions.Put(typeof(double), new HashSet<Type>(wideningConversions));
    		WideningConversions.Put(typeof(double?), new HashSet<Type>(wideningConversions));

            wideningConversions.AddAll(doubleWrappers);
            WideningConversions.Put(typeof(decimal), new HashSet<Type>(wideningConversions));
            WideningConversions.Put(typeof(decimal?), new HashSet<Type>(wideningConversions));
        }

        /// <summary>
        /// Returns the allowable widening conversions.
        /// </summary>
        /// <value>
        /// map where key is the class that we are asking to be widened into, anda set of classes that can be widened from
        /// </value>
        public static IDictionary<Type, ICollection<Type>> WideningConversions { get; private set; }

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
    	public static MethodInfo ResolveMethod(Type declaringClass, String methodName, Type[] paramTypes, bool allowInstance, bool[] allowEventBeanType, bool[] allowEventBeanCollType)
    	{
    		// Get all the methods for this class
            MethodInfo[] methods = declaringClass.GetMethods();

            MethodInfo bestMatch = null;
    		var bestConversionCount = -1;
    
    		// Examine each method, checking if the signature is compatible
            MethodInfo conversionFailedMethod = null;
            foreach (MethodInfo method in methods)
    		{
    			// Check the modifiers: we only want public and static, if required
    			if(!IsPublicAndStatic(method, allowInstance))
    			{
    				continue;
    			}
    
    			// Check the name
    			if(method.Name != methodName)
    			{
    				continue;
    			}
    
    			// Check the parameter list
                int conversionCount = CompareParameterTypesAllowContext(
                    method.GetParameters().Select(p => p.ParameterType).ToArray(),
                    paramTypes, 
                    allowEventBeanType, 
                    allowEventBeanCollType,
                    method.GetGenericArguments()
                );

    			// Parameters don't match
    			if(conversionCount == -1)
    			{
                    conversionFailedMethod = method;
                    continue;
    			}
    
    			// Parameters match exactly
    			if(conversionCount == 0)
    			{
    				bestMatch = method;
    				break;
    			}
    
    			// No previous match
    			if(bestMatch == null)
    			{
    				bestMatch = method;
    				bestConversionCount = conversionCount;
    			}
    			else
    			{
    				// Current match is better
    				if(conversionCount < bestConversionCount)
    				{
    					bestMatch = method;
    					bestConversionCount = conversionCount;
    				}
    			}
    		}
    
    		if(bestMatch != null)
    		{
                LogWarnBoxedToPrimitiveType(declaringClass, methodName, bestMatch, paramTypes);
    			return bestMatch;
    		}
    
            var paramList = new StringBuilder();
            if(paramTypes != null && paramTypes.Length != 0)
            {
                var appendString = "";
                foreach (var param in paramTypes)
                {
                    paramList.Append(appendString);
                    if (param == null) {
                        paramList.Append("(null)");
                    }
                    else {
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
                // Check the parameter list
                int conversionCount = CompareParameterTypesAllowContext(
                    method.GetParameters().Select(p => p.ParameterType).Skip(1).ToArray(),
                    paramTypes,
                    allowEventBeanType,
                    allowEventBeanCollType,
                    method.GetGenericArguments()
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
            for (int i = 0; i < parametersMethod.Length; i++) {
                if (!parametersMethod[i].IsPrimitive) {
                    continue;
                }
                // if null-type parameter, or non-CLR class and boxed type matches
                if (paramTypes[i] == null || (!declaringClass.GetType().FullName.StartsWith("System.") && (parametersMethod[i].GetBoxedType()) == paramTypes[i])) {
                    String paramTypeStr = paramTypes[i] == null ? "null" : paramTypes[i].Name;
                    Log.Info(
                        "Method '{0}' in class '{1}' expects primitive type '{2}' as parameter {3}, but receives a nullable (boxed) type {4}. This may cause null pointer exception at runtime if the actual value is null, please consider using boxed types for method parameters.",
                        methodName, declaringClass.Name, parametersMethod[i], i, paramTypeStr);
                    return;
                }
            }
        }

        private static bool IsWideningConversion(Type declarationType, Type invocationType)
        {
            return
                WideningConversions.ContainsKey(declarationType) && 
                WideningConversions.Get(declarationType).Contains(invocationType);
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
            Type[] genericParameterTypes)
        {
            // determine if the last parameter is EPLMethodInvocationContext
            var declaredNoContext = declarationParameters;
            if (declarationParameters.Length > 0 &&
                declarationParameters[declarationParameters.Length - 1] == typeof(EPLMethodInvocationContext)) {
                declaredNoContext = new Type[declarationParameters.Length - 1];
                Array.Copy(declarationParameters, 0, declaredNoContext, 0, declaredNoContext.Length);
            }

            return CompareParameterTypesNoContext(declaredNoContext, invocationParameters,
                    optionalAllowEventBeanType, optionalAllowEventBeanCollType, genericParameterTypes);
        }

        // Returns -1 if the invocation parameters aren't applicable
    	// to the method. Otherwise returns the number of parameters
    	// that have to be converted
        private static int CompareParameterTypesNoContext(
            Type[] declarationParameters,
            Type[] invocationParameters,
            Boolean[] optionalAllowEventBeanType,
            Boolean[] optionalAllowEventBeanCollType,
            Type[] genericParameterTypes)
        {
    		if(invocationParameters == null)
    		{
    			return declarationParameters.Length == 0 ? 0 : -1;
    		}
    
    		if(declarationParameters.Length != invocationParameters.Length)
    		{
    			return -1;
    		}
    
    		int conversionCount = 0;
    		int count = 0;
            foreach (Type parameter in declarationParameters)
    		{
                if ((invocationParameters[count] == null) && !(parameter.IsPrimitive))
                {
                    count++;
                    continue;
                }
                if (optionalAllowEventBeanType != null && parameter == typeof(EventBean) && optionalAllowEventBeanType[count])
                {
                    count++;
                    continue;
                }
                if (optionalAllowEventBeanCollType != null &&
                    parameter == typeof(ICollection<EventBean>) &&
                    optionalAllowEventBeanCollType[count])
                {
                    count++;
                    continue;
                }
    			if(!IsIdentityConversion(parameter, invocationParameters[count]))
    			{
    				conversionCount++;
    				if(!IsWideningConversion(parameter, invocationParameters[count]))
    				{
    					conversionCount = -1;
    					break;
    				}
    			}
    			count++;
    		}
    
    		return conversionCount;
    	}
    
    	// Identity conversion means no conversion, wrapper conversion,
    	// or conversion to a supertype
        private static bool IsIdentityConversion(Type declarationType, Type invocationType)
    	{
    		if(WrappingConversions.ContainsKey(declarationType))
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
                int conversionCount = CompareParameterTypesNoContext(
                    ctor.GetParameters().Select(p => p.ParameterType).ToArray(),
                    paramTypes, null, null,
                    new Type[0]); // ctor.GetGenericArguments());

                // MSDN
                //
                // NotSupportedException - The current object is a ConstructorInfo. Generic constructors are not 
                // supported in the .NET Framework version 2.0. This exception is the default behavior if this method
                // is not overridden in a derived class.

                // Parameters don't match
                if(conversionCount == -1)
                {
                    conversionFailedCtor = ctor;
                    continue;
                }
    
                // Parameters match exactly
                if(conversionCount == 0)
                {
                    bestMatch = ctor;
                    break;
                }
    
                // No previous match
                if(bestMatch == null)
                {
                    bestMatch = ctor;
                    bestConversionCount = conversionCount;
                }
                else
                {
                    // Current match is better
                    if(conversionCount < bestConversionCount)
                    {
                        bestMatch = ctor;
                        bestConversionCount = conversionCount;
                    }
                }
    
            }
    
            if(bestMatch != null)
            {
                return bestMatch;
            }

            var paramList = new StringBuilder();
            var message = "Constructor not found for " + declaringClass.Name + " taking ";
            if(paramTypes != null && paramTypes.Length != 0)
            {
                var appendString = "";
                foreach (var param in paramTypes)
                {
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
            throw new EngineNoSuchCtorException(message, conversionFailedCtor);
        }
    }
}
