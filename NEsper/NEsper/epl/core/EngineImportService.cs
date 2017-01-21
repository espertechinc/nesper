///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.client.hook;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.epl.agg.factory;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;

namespace com.espertech.esper.epl.core
{
    /// <summary>
    /// Service for engine-level resolution of static methods and aggregation methods.
    /// </summary>
    public interface 
        EngineImportService
    {
        /// <summary>
        /// Returns the method invocation caches for the from-clause for a class.
        /// </summary>
        /// <param name="className">the class name providing the method</param>
        /// <returns>cache configs</returns>
        ConfigurationMethodRef GetConfigurationMethodRef(String className);
    
        /// <summary>
        /// Add an import, such as "com.mypackage" or "com.mypackage.MyClass".
        /// </summary>
        /// <param name="importName">is the import to add</param>
        /// <throws>EngineImportException if the information or format is invalid</throws>
        void AddImport(String importName) ;

        /// <summary>
        /// Add an explicit import.
        /// </summary>
        /// <param name="importDesc">The import desc.</param>
        void AddImport(AutoImportDesc importDesc);
    
        /// <summary>
        /// Add an import for annotation-only use, such as "MyPackage.Attributes"
        /// </summary>
        /// <param name="importName">Name of the import.</param>
        void AddAnnotationImport(String importName);

        /// <summary>
        /// Add an import for annotation-only use, such as "MyPackage.Attributes"
        /// </summary>
        /// <param name="importDesc">The import desc.</param>
        void AddAnnotationImport(AutoImportDesc importDesc);

        /// <summary>
        /// Add an aggregation function.
        /// </summary>
        /// <param name="functionName">is the name of the function to make known.</param>
        /// <param name="aggregationDesc">is the descriptor for the aggregation function</param>
        /// <throws>EngineImportException throw if format or information is invalid</throws>
        void AddAggregation(String functionName, ConfigurationPlugInAggregationFunction aggregationDesc) ;

        /// <summary>
        /// Add an single-row function.
        /// </summary>
        /// <param name="functionName">is the name of the function to make known.</param>
        /// <param name="singleRowFuncClass">is the class that provides the single row function</param>
        /// <param name="methodName">is the name of the public static method provided by the class that provides the single row function</param>
        /// <param name="valueCache">setting to control value cache behavior which may cache a result value when constant parameters are passed</param>
        /// <param name="filterOptimizable">The filter optimizable.</param>
        /// <param name="rethrowExceptions">if set to <c>true</c> [rethrow exceptions].</param>
        /// <throws>EngineImportException throw if format or information is invalid</throws>
        void AddSingleRow(String functionName, String singleRowFuncClass, String methodName, ValueCache valueCache, FilterOptimizable filterOptimizable, bool rethrowExceptions) ;
    
        /// <summary>
        /// Used at statement compile-time to try and resolve a given function name into an
        /// aggregation method. Matches function name case-neutral.
        /// </summary>
        /// <param name="functionName">is the function name</param>
        /// <returns>aggregation provider</returns>
        /// <throws>EngineImportUndefinedException if the function is not a configured aggregation function</throws>
        /// <throws>EngineImportException if the aggregation providing class could not be loaded or doesn't match</throws>
        AggregationFunctionFactory ResolveAggregationFactory(String functionName) ;
    
        ConfigurationPlugInAggregationMultiFunction ResolveAggregationMultiFunction(String name);
    
        /// <summary>
        /// Used at statement compile-time to try and resolve a given function name into an
        /// single-row function. Matches function name case-neutral.
        /// </summary>
        /// <param name="functionName">is the function name</param>
        /// <returns>class name and method name pair</returns>
        /// <throws>EngineImportUndefinedException if the function is not a configured single-row function</throws>
        /// <throws>EngineImportException if the function providing class could not be loaded or doesn't match</throws>
        Pair<Type, EngineImportSingleRowDesc> ResolveSingleRow(String functionName) ;
    
        /// <summary>
        /// Resolves a given class, method and list of parameter types to a static method.
        /// </summary>
        /// <param name="typeName">is the class name to use</param>
        /// <param name="methodName">is the method name</param>
        /// <param name="paramTypes">is parameter types match expression sub-nodes</param>
        /// <param name="allowEventBeanType">Type of the allow event bean.</param>
        /// <param name="allowEventBeanCollType">Type of the allow event bean coll.</param>
        /// <returns>method this resolves to</returns>
        /// <throws>EngineImportException if the method cannot be resolved to a visible static method</throws>
        MethodInfo ResolveMethod(String typeName, String methodName, Type[] paramTypes, bool[] allowEventBeanType, bool[] allowEventBeanCollType) ;
    
        /// <summary>
        /// Resolves a constructor matching list of parameter types.
        /// </summary>
        /// <param name="clazz">is the class to use</param>
        /// <param name="paramTypes">is parameter types match expression sub-nodes</param>
        /// <returns>method this resolves to</returns>
        /// <throws>EngineImportException if the ctor cannot be resolved</throws>
        ConstructorInfo ResolveCtor(Type clazz, Type[] paramTypes) ;

        /// <summary>
        /// Resolves a given class name, either fully qualified and simple and imported to a class.
        /// </summary>
        /// <param name="typeName">is the class name to use</param>
        /// <param name="forAnnotation">if set to <c>true</c> [for annotation].</param>
        /// <returns>
        /// class this resolves to
        /// </returns>
        /// <throws>EngineImportException if there was an error resolving the class</throws>
        Type ResolveType(String typeName, bool forAnnotation) ;
    
        /// <summary>
        /// Resolves a given class name, either fully qualified and simple and imported to a annotation.
        /// </summary>
        /// <param name="typeName">is the class name to use</param>
        /// <returns>annotation class this resolves to</returns>
        /// <throws>EngineImportException if there was an error resolving the class</throws>
        Type ResolveAnnotation(String typeName) ;
    
        /// <summary>
        /// Resolves a given class and method name to a static method, expecting the method to exist
        /// exactly once and not be overloaded, with any parameters.
        /// </summary>
        /// <param name="typeName">is the class name to use</param>
        /// <param name="methodName">is the method name</param>
        /// <returns>method this resolves to</returns>
        /// <throws>EngineImportException if the method cannot be resolved to a visible static method, orif the method is overloaded
        /// </throws>
        MethodInfo ResolveMethod(String typeName, String methodName) ;

        /// <summary>
        /// Resolves a given class and method name to a non-static method, expecting the method to exist
        /// exactly once and not be overloaded, with any parameters.
        /// </summary>
        /// <param name="clazz">The clazz.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <returns></returns>
        /// <throws>EngineImportException if the method cannot be resolved to a visible static method, or if the method is overloaded</throws>
        MethodInfo ResolveNonStaticMethod(Type clazz, String methodName) ;

        /// <summary>
        /// Resolves a given method name and list of parameter types to an instance or static method exposed by the given class.
        /// </summary>
        /// <param name="clazz">is the class to look for a fitting method</param>
        /// <param name="methodName">is the method name</param>
        /// <param name="paramTypes">is parameter types match expression sub-nodes</param>
        /// <param name="allowEventBeanType">whether EventBean footprint is allowed</param>
        /// <param name="allowEventBeanCollType">Type of the allow event bean coll.</param>
        /// <returns>method this resolves to</returns>
        /// <throws>EngineImportException if the method cannot be resolved to a visible static or instance method</throws>
        MethodInfo ResolveMethod(Type clazz, String methodName, Type[] paramTypes, bool[] allowEventBeanType, bool[] allowEventBeanCollType) ;
    
        /// <summary>
        /// Resolve an extended (non-SQL std) builtin aggregation.
        /// </summary>
        /// <param name="name">of func</param>
        /// <param name="isDistinct">indicator</param>
        /// <returns>aggregation func node</returns>
        ExprNode ResolveAggExtendedBuiltin(String name, bool isDistinct);

        /// <summary>
        /// Resolves an extended (non-SQL std) single-row function.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        ExprNode ResolveSingleRowExtendedBuiltin(String name);

        bool IsDuckType { get; }

        bool IsUdfCache { get; }

        bool IsSortUsingCollator { get; }

        void AddAggregationMultiFunction(ConfigurationPlugInAggregationMultiFunction desc) ;

        MathContext DefaultMathContext { get; }

        TimeZoneInfo TimeZone { get;  }

        ConfigurationEngineDefaults.ThreadingProfile ThreadingProfile { get; }

        AggregationFactoryFactory AggregationFactoryFactory { get; }
    }

    internal class EngineImportServiceConstants
    {
        public const String EXT_SINGLEROW_FUNCTION_TRANSPOSE = "transpose";
    }
}
