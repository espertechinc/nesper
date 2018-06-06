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
using com.espertech.esper.client.util;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.epl.agg.factory;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.time;
using com.espertech.esper.epl.index.service;
using com.espertech.esper.events;

namespace com.espertech.esper.epl.core
{
    /// <summary>
    /// Service for engine-level resolution of static methods and aggregation methods.
    /// </summary>
    public interface EngineImportService
    {
        /// <summary>
        /// Returns the method invocation caches for the from-clause for a class.
        /// </summary>
        /// <param name="className">the class name providing the method</param>
        /// <returns>cache configs</returns>
        ConfigurationMethodRef GetConfigurationMethodRef(string className);

        /// <summary>
        /// Add an import, such as "com.mypackage.*" or "com.mypackage.MyClass".
        /// </summary>
        /// <param name="namespaceOrType">Type of the namespace or.</param>
        /// <exception cref="EngineImportException">if the information or format is invalid</exception>
        void AddImport(String namespaceOrType);

        /// <summary>
        /// Add an import, such as "com.mypackage.*" or "com.mypackage.MyClass".
        /// </summary>
        /// <param name="import">The automatic import desc.</param>
        /// <exception cref="EngineImportException">if the information or format is invalid</exception>
        void AddImport(AutoImportDesc import);

        /// <summary>
        /// Add an import annotation-only use, such as "com.mypackage.*" or "com.mypackage.MyClass".
        /// </summary>
        /// <param name="namespaceOrType">Type of the namespace or.</param>
        /// <exception cref="EngineImportException">if the information or format is invalid</exception>
        void AddAnnotationImport(String namespaceOrType);

        /// <summary>
        /// Add an import for annotation-only use, such as "com.mypackage.*" or "com.mypackage.MyClass".
        /// </summary>
        /// <param name="autoImportDesc">The automatic import desc.</param>
        /// <exception cref="EngineImportException">if the information or format is invalid</exception>
        void AddAnnotationImport(AutoImportDesc autoImportDesc);
    
        /// <summary>
        /// Add an aggregation function.
        /// </summary>
        /// <param name="functionName">is the name of the function to make known.</param>
        /// <param name="aggregationDesc">is the descriptor for the aggregation function</param>
        /// <exception cref="EngineImportException">throw if format or information is invalid</exception>
        void AddAggregation(string functionName, ConfigurationPlugInAggregationFunction aggregationDesc) ;
    
        /// <summary>
        /// Add an single-row function.
        /// </summary>
        /// <param name="functionName">is the name of the function to make known.</param>
        /// <param name="singleRowFuncClass">is the class that provides the single row function</param>
        /// <param name="methodName">is the name of the static method provided by the class that provides the single row function</param>
        /// <param name="valueCache">setting to control value cache behavior which may cache a result value when constant parameters are passed</param>
        /// <param name="filterOptimizable">filter behavior setting</param>
        /// <param name="rethrowExceptions">for whether to rethrow</param>
        /// <param name="optionalEventTypeName">event type name when provided</param>
        /// <exception cref="EngineImportException">throw if format or information is invalid</exception>
        void AddSingleRow(string functionName, string singleRowFuncClass, string methodName, ValueCacheEnum valueCache, FilterOptimizableEnum filterOptimizable, bool rethrowExceptions, string optionalEventTypeName) ;
    
        /// <summary>
        /// Used at statement compile-time to try and resolve a given function name into an
        /// aggregation method. Matches function name case-neutral.
        /// </summary>
        /// <param name="functionName">is the function name</param>
        /// <exception cref="EngineImportUndefinedException">if the function is not a configured aggregation function</exception>
        /// <exception cref="EngineImportException">if the aggregation providing class could not be loaded or doesn't match</exception>
        /// <returns>aggregation provider</returns>
        AggregationFunctionFactory ResolveAggregationFactory(string functionName) ;
    
        ConfigurationPlugInAggregationMultiFunction ResolveAggregationMultiFunction(string name);
    
        /// <summary>
        /// Used at statement compile-time to try and resolve a given function name into an
        /// single-row function. Matches function name case-neutral.
        /// </summary>
        /// <param name="functionName">is the function name</param>
        /// <exception cref="EngineImportUndefinedException">if the function is not a configured single-row function</exception>
        /// <exception cref="EngineImportException">if the function providing class could not be loaded or doesn't match</exception>
        /// <returns>class name and method name pair</returns>
        Pair<Type, EngineImportSingleRowDesc> ResolveSingleRow(string functionName) ;
    
        /// <summary>
        /// Resolves a given class, method and list of parameter types to a static method.
        /// </summary>
        /// <param name="className">is the class name to use</param>
        /// <param name="methodName">is the method name</param>
        /// <param name="paramTypes">is parameter types match expression sub-nodes</param>
        /// <param name="allowEventBeanType">flag for whether event bean is allowed</param>
        /// <param name="allowEventBeanCollType">flag for whether event bean array is allowed</param>
        /// <exception cref="EngineImportException">if the method cannot be resolved to a visible static method</exception>
        /// <returns>method this resolves to</returns>
        MethodInfo ResolveMethodOverloadChecked(string className, string methodName, Type[] paramTypes, bool[] allowEventBeanType, bool[] allowEventBeanCollType);

        MethodInfo ResolveMethodOverloadChecked(Type clazz, string methodName);
    
        /// <summary>
        /// Resolves a constructor matching list of parameter types.
        /// </summary>
        /// <param name="clazz">is the class to use</param>
        /// <param name="paramTypes">is parameter types match expression sub-nodes</param>
        /// <exception cref="EngineImportException">if the ctor cannot be resolved</exception>
        /// <returns>method this resolves to</returns>
        ConstructorInfo ResolveCtor(Type clazz, Type[] paramTypes) ;
    
        /// <summary>
        /// Resolves a given class name, either fully qualified and simple and imported to a class.
        /// </summary>
        /// <param name="className">is the class name to use</param>
        /// <param name="forAnnotation">whether we are resolving an annotation</param>
        /// <exception cref="EngineImportException">if there was an error resolving the class</exception>
        /// <returns>class this resolves to</returns>
        Type ResolveType(string className, bool forAnnotation) ;
    
        /// <summary>
        /// Resolves a given class name, either fully qualified and simple and imported to a annotation.
        /// </summary>
        /// <param name="className">is the class name to use</param>
        /// <exception cref="EngineImportException">if there was an error resolving the class</exception>
        /// <returns>annotation class this resolves to</returns>
        Type ResolveAnnotation(string className) ;
    
        /// <summary>
        /// Resolves a given class and method name to a static method, expecting the method to exist
        /// exactly once and not be overloaded, with any parameters.
        /// </summary>
        /// <param name="className">is the class name to use</param>
        /// <param name="methodName">is the method name</param>
        /// <exception cref="EngineImportException">
        /// if the method cannot be resolved to a visible static method, or
        /// if the method is overloaded
        /// </exception>
        /// <returns>method this resolves to</returns>
        MethodInfo ResolveMethodOverloadChecked(string className, string methodName);
    
        /// <summary>
        /// Resolves a given class and method name to a non-static method, expecting the method to exist
        /// exactly once and not be overloaded, with any parameters.
        /// </summary>
        /// <param name="clazz">is the class</param>
        /// <param name="methodName">is the method name</param>
        /// <exception cref="EngineImportException">
        /// if the method cannot be resolved to a visible static method, or
        /// if the method is overloaded
        /// </exception>
        /// <returns>method this resolves to</returns>
        MethodInfo ResolveNonStaticMethodOverloadChecked(Type clazz, string methodName) ;
    
        /// <summary>
        /// Resolves a given method name and list of parameter types to an instance or static method exposed by the given class.
        /// </summary>
        /// <param name="clazz">is the class to look for a fitting method</param>
        /// <param name="methodName">is the method name</param>
        /// <param name="paramTypes">is parameter types match expression sub-nodes</param>
        /// <param name="allowEventBeanType">whether EventBean footprint is allowed</param>
        /// <param name="allowEventBeanCollType">whether EventBean array footprint is allowed</param>
        /// <exception cref="EngineImportException">if the method cannot be resolved to a visible static or instance method</exception>
        /// <returns>method this resolves to</returns>
        MethodInfo ResolveMethod(Type clazz, string methodName, Type[] paramTypes, bool[] allowEventBeanType, bool[] allowEventBeanCollType);
    
        /// <summary>
        /// Resolve an extended (non-SQL std) builtin aggregation.
        /// </summary>
        /// <param name="name">of func</param>
        /// <param name="isDistinct">indicator</param>
        /// <returns>aggregation func node</returns>
        ExprNode ResolveAggExtendedBuiltin(string name, bool isDistinct);
    
        /// <summary>
        /// Resolve an extended (non-SQL std) single-row function.
        /// </summary>
        /// <param name="name">of func</param>
        /// <returns>node or null</returns>
        ExprNode ResolveSingleRowExtendedBuiltin(string name);

        bool IsDuckType { get; }

        bool IsUdfCache { get; }

        bool IsSortUsingCollator { get; }

        void AddAggregationMultiFunction(ConfigurationPlugInAggregationMultiFunction desc) ;

        MathContext DefaultMathContext { get; }

        TimeZoneInfo TimeZone { get; }

        TimeAbacus TimeAbacus { get; }

        ConfigurationEngineDefaults.ThreadingProfile ThreadingProfile { get; }

        AggregationFactoryFactory AggregationFactoryFactory { get; }

        ClassForNameProvider GetClassForNameProvider();

        ClassLoader GetFastClassClassLoader(Type clazz);

        ClassLoader GetClassLoader();

        AdvancedIndexFactoryProvider ResolveAdvancedIndexProvider(String indexTypeName);

        bool IsCodegenEventPropertyGetters { get; }

        EventPropertyGetter CodegenGetter(EventPropertyGetterSPI getterSPI, string propertyExpression);
    }

    public class EngineImportServiceConstants
    {
        public const string EXT_SINGLEROW_FUNCTION_TRANSPOSE = "transpose";
    }
} // end of namespace
