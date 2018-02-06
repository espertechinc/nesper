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
using System.Reflection;
using System.Text.RegularExpressions;

using com.espertech.esper.client;
using com.espertech.esper.client.annotation;
using com.espertech.esper.client.hook;
using com.espertech.esper.client.util;
using com.espertech.esper.codegen.compile;
using com.espertech.esper.codegen.core;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.agg.factory;
using com.espertech.esper.epl.approx;
using com.espertech.esper.epl.expression.accessagg;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.methodagg;
using com.espertech.esper.epl.expression.time;
using com.espertech.esper.epl.index.quadtree;
using com.espertech.esper.epl.index.service;
using com.espertech.esper.events;
using com.espertech.esper.type;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.core
{
    /// <summary>Implementation for engine-level imports.</summary>
    public class EngineImportServiceImpl : EngineImportService
        , ClassLoaderProvider
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly Regex FunctionRegEx1 = new Regex(@"^\w+$", RegexOptions.None);

        private static readonly Regex TypeNameRegEx1 = new Regex(@"^(\w+\.)*\w+$", RegexOptions.None);
        private static readonly Regex TypeNameRegEx2 = new Regex(@"^(\w+\.)*\w+\+(\w+|)$", RegexOptions.None);
        private readonly LinkedHashMap<string, AdvancedIndexFactoryProvider> _advancedIndexProviders;
        private readonly IList<Pair<ISet<string>, ConfigurationPlugInAggregationMultiFunction>> _aggregationAccess;
        private readonly IDictionary<string, ConfigurationPlugInAggregationFunction> _aggregationFunctions;

        private readonly bool _allowExtendedAggregationFunc;
        private readonly IList<AutoImportDesc> _annotationImports;
        private readonly string _engineURI;

        private readonly IList<AutoImportDesc> _imports;
        private readonly IDictionary<string, ConfigurationMethodRef> _methodInvocationRef;
        private readonly IDictionary<string, EngineImportSingleRowDesc> _singleRowFunctions;
        private readonly IDictionary<string, object> _transientConfiguration;

        private readonly ClassLoaderProvider _classLoaderProvider;

        private ICodegenContext _context;

        public EngineImportServiceImpl(
            bool allowExtendedAggregationFunc,
            bool isUdfCache,
            bool isDuckType,
            bool sortUsingCollator,
            MathContext optionalDefaultMathContext,
            TimeZoneInfo timeZone,
            TimeAbacus timeAbacus,
            ConfigurationEngineDefaults.ThreadingProfile threadingProfile,
            IDictionary<string, object> transientConfiguration,
            AggregationFactoryFactory aggregationFactoryFactory,
            bool isCodegenEventPropertyGetters,
            string engineURI,
            ICodegenContext context,
            ClassLoaderProvider classLoaderProvider)
        {
            _imports = new List<AutoImportDesc>();
            _annotationImports = new List<AutoImportDesc>(2);
            _aggregationFunctions = new Dictionary<string, ConfigurationPlugInAggregationFunction>();
            _aggregationAccess = new List<Pair<ISet<string>, ConfigurationPlugInAggregationMultiFunction>>();
            _singleRowFunctions = new Dictionary<string, EngineImportSingleRowDesc>();
            _methodInvocationRef = new Dictionary<string, ConfigurationMethodRef>();
            _allowExtendedAggregationFunc = allowExtendedAggregationFunc;
            IsUdfCache = isUdfCache;
            IsDuckType = isDuckType;
            IsSortUsingCollator = sortUsingCollator;
            DefaultMathContext = optionalDefaultMathContext;
            TimeZone = timeZone;
            TimeAbacus = timeAbacus;
            ThreadingProfile = threadingProfile;
            _transientConfiguration = transientConfiguration;
            AggregationFactoryFactory = aggregationFactoryFactory;
            _advancedIndexProviders = new LinkedHashMap<string, AdvancedIndexFactoryProvider>();
            _advancedIndexProviders.Put("pointregionquadtree", new AdvancedIndexFactoryProviderPointRegionQuadTree());
            _advancedIndexProviders.Put("mxcifquadtree", new AdvancedIndexFactoryProviderMXCIFQuadTree());
            IsCodegenEventPropertyGetters = isCodegenEventPropertyGetters;
            _engineURI = engineURI;
            _context = context;
            _classLoaderProvider = classLoaderProvider;
        }

        /// <summary>
        ///     For testing, returns imports.
        /// </summary>
        /// <value>returns auto-import list as array</value>
        public AutoImportDesc[] Imports => _imports.ToArray();

        public bool IsUdfCache { get; }

        public bool IsDuckType { get; }

        public ConfigurationMethodRef GetConfigurationMethodRef(string className)
        {
            return _methodInvocationRef.Get(className);
        }

        public ClassForNameProvider GetClassForNameProvider()
        {
            return TransientConfigurationResolver.ResolveClassForNameProvider(_transientConfiguration);
        }

        public ClassLoader GetFastClassClassLoader(Type clazz)
        {
            return TransientConfigurationResolver.ResolveFastClassClassLoaderProvider(_transientConfiguration)
                .Classloader(clazz);
        }

        public ClassLoader GetClassLoader()
        {
            return TransientConfigurationResolver
                .ResolveClassLoader(_classLoaderProvider, _transientConfiguration)
                .GetClassLoader();
        }

        public void AddImport(string namespaceOrType)
        {
            ValidateImportAndAdd(new AutoImportDesc(namespaceOrType), _imports);
        }

        public void AddImport(AutoImportDesc importDesc)
        {
            ValidateImportAndAdd(importDesc, _imports);
        }

        public void AddAnnotationImport(string namespaceOrType)
        {
            ValidateImportAndAdd(new AutoImportDesc(namespaceOrType), _annotationImports);
        }

        public void AddAnnotationImport(AutoImportDesc importDesc)
        {
            ValidateImportAndAdd(importDesc, _annotationImports);
        }

        public void AddAggregation(string functionName, ConfigurationPlugInAggregationFunction aggregationDesc)
        {
            ValidateFunctionName("aggregation function", functionName);
            if (aggregationDesc.FactoryClassName == null || !IsTypeName(aggregationDesc.FactoryClassName))
            {
                throw new EngineImportException(
                    "Invalid class name for aggregation factory '" + aggregationDesc.FactoryClassName + "'");
            }

            _aggregationFunctions.Put(functionName.ToLowerInvariant(), aggregationDesc);
        }

        public void AddSingleRow(
            string functionName,
            string singleRowFuncClass,
            string methodName,
            ValueCacheEnum valueCache,
            FilterOptimizableEnum filterOptimizable,
            bool rethrowExceptions,
            string optionalEventTypeName)
        {
            ValidateFunctionName("single-row", functionName);

            if (!IsTypeName(singleRowFuncClass))
            {
                throw new EngineImportException("Invalid class name for aggregation '" + singleRowFuncClass + "'");
            }

            _singleRowFunctions.Put(
                functionName.ToLowerInvariant(),
                new EngineImportSingleRowDesc(
                    singleRowFuncClass, methodName, valueCache, filterOptimizable, rethrowExceptions,
                    optionalEventTypeName));
        }

        public AggregationFunctionFactory ResolveAggregationFactory(string name)
        {
            var desc = _aggregationFunctions.Get(name);
            if (desc == null)
            {
                desc = _aggregationFunctions.Get(name.ToLowerInvariant());
            }

            if (desc == null || desc.FactoryClassName == null)
            {
                throw new EngineImportUndefinedException("A function named '" + name + "' is not defined");
            }

            var className = desc.FactoryClassName;
            Type clazz;
            try
            {
                clazz = GetClassForNameProvider().ClassForName(className);
            }
            catch (TypeLoadException ex)
            {
                throw new EngineImportException(
                    "Could not load aggregation factory class by name '" + className + "'", ex);
            }

            object @object;
            try
            {
                @object = Activator.CreateInstance(clazz);
            }
            catch (TypeLoadException e)
            {
                throw new EngineImportException("Error instantiating aggregation class", e);
            }
            catch (MissingMethodException e)
            {
                throw new EngineImportException(
                    "Error instantiating aggregation class - Default constructor was not found", e);
            }
            catch (MethodAccessException e)
            {
                throw new EngineImportException(
                    "Error instantiating aggregation class - Caller does not have permission to use constructor", e);
            }
            catch (ArgumentException e)
            {
                throw new EngineImportException("Error instantiating aggregation class - Type is not a RuntimeType", e);
            }

            if (!(@object is AggregationFunctionFactory))
            {
                throw new EngineImportException(
                    "Aggregation class by name '" + Name.Of(clazz) + "' does not implement AggregationFunctionFactory");
            }

            return (AggregationFunctionFactory) @object;
        }

        public void AddAggregationMultiFunction(ConfigurationPlugInAggregationMultiFunction desc)
        {
            var orderedImmutableFunctionNames = new LinkedHashSet<string>();
            foreach (var functionName in desc.FunctionNames)
            {
                orderedImmutableFunctionNames.Add(functionName.ToLowerInvariant());
                ValidateFunctionName("aggregation multi-function", functionName.ToLowerInvariant());
            }

            if (!IsTypeName(desc.MultiFunctionFactoryClassName))
            {
                throw new EngineImportException(
                    "Invalid class name for aggregation multi-function factory '" + desc.MultiFunctionFactoryClassName +
                    "'");
            }

            _aggregationAccess.Add(
                new Pair<ISet<string>, ConfigurationPlugInAggregationMultiFunction>(
                    orderedImmutableFunctionNames, desc));
        }

        public ConfigurationPlugInAggregationMultiFunction ResolveAggregationMultiFunction(string name)
        {
            foreach (var config in _aggregationAccess)
            {
                if (config.First.Contains(name.ToLowerInvariant()))
                {
                    return config.Second;
                }
            }

            return null;
        }

        public Pair<Type, EngineImportSingleRowDesc> ResolveSingleRow(string name)
        {
            var pair = _singleRowFunctions.Get(name);
            if (pair == null)
            {
                pair = _singleRowFunctions.Get(name.ToLowerInvariant());
            }

            if (pair == null)
            {
                throw new EngineImportUndefinedException("A function named '" + name + "' is not defined");
            }

            Type clazz;
            try
            {
                clazz = GetClassForNameProvider().ClassForName(pair.ClassName);
            }
            catch (TypeLoadException ex)
            {
                throw new EngineImportException(
                    "Could not load single-row function class by name '" + pair.ClassName + "'", ex);
            }

            return new Pair<Type, EngineImportSingleRowDesc>(clazz, pair);
        }

        public MethodInfo ResolveMethodOverloadChecked(
            string className, string methodName, Type[] paramTypes, bool[] allowEventBeanType,
            bool[] allowEventBeanCollType)
        {
            Type clazz;
            try
            {
                clazz = ResolveTypeInternal(className, false, false);
            }
            catch (TypeLoadException e)
            {
                throw new EngineImportException(
                    "Could not load class by name '" + className + "', please check imports", e);
            }

            try
            {
                return MethodResolver.ResolveMethod(
                    clazz, methodName, paramTypes, false, allowEventBeanType, allowEventBeanCollType);
            }
            catch (EngineNoSuchMethodException e)
            {
                throw Convert(clazz, methodName, paramTypes, e, false);
            }
        }

        public ConstructorInfo ResolveCtor(Type clazz, Type[] paramTypes)
        {
            try
            {
                return MethodResolver.ResolveCtor(clazz, paramTypes);
            }
            catch (EngineNoSuchCtorException e)
            {
                throw Convert(clazz, paramTypes, e);
            }
        }

        public MethodInfo ResolveMethodOverloadChecked(string className, string methodName)
        {
            Type clazz;
            try
            {
                clazz = ResolveTypeInternal(className, false, false);
            }
            catch (TypeLoadException e)
            {
                throw new EngineImportException(
                    "Could not load class by name '" + className + "', please check imports", e);
            }

            return ResolveMethodInternalCheckOverloads(clazz, methodName, MethodModifiers.REQUIRE_STATIC_AND_PUBLIC);
        }

        public MethodInfo ResolveMethodOverloadChecked(Type clazz, string methodName)
        {
            return ResolveMethodInternalCheckOverloads(clazz, methodName, MethodModifiers.REQUIRE_STATIC_AND_PUBLIC);
        }

        public MethodInfo ResolveNonStaticMethodOverloadChecked(Type clazz, string methodName)
        {
            return ResolveMethodInternalCheckOverloads(clazz, methodName, MethodModifiers.REQUIRE_NONSTATIC_AND_PUBLIC);
        }

        public Type ResolveType(string className, bool forAnnotation)
        {
            Type clazz;
            try
            {
                clazz = ResolveTypeInternal(className, false, forAnnotation);
            }
            catch (TypeLoadException e)
            {
                throw new EngineImportException(
                    "Could not load class by name '" + className + "', please check imports", e);
            }

            return clazz;
        }

        public Type ResolveAnnotation(string className)
        {
            Type clazz;
            try
            {
                clazz = ResolveTypeInternal(className, true, true);
            }
            catch (TypeLoadException e)
            {
                throw new EngineImportException(
                    "Could not load annotation class by name '" + className + "', please check imports", e);
            }

            return clazz;
        }

        public MethodInfo ResolveMethod(
            Type clazz,
            string methodName,
            Type[] paramTypes,
            bool[] allowEventBeanType,
            bool[] allowEventBeanCollType)
        {
            try
            {
                return MethodResolver.ResolveMethod(
                    clazz, methodName, paramTypes, true, allowEventBeanType, allowEventBeanType);
            }
            catch (EngineNoSuchMethodException e)
            {
                var method = MethodResolver.ResolveExtensionMethod(
                    clazz, methodName, paramTypes, true, allowEventBeanType, allowEventBeanType);
                if (method == null)
                {
                    throw Convert(clazz, methodName, paramTypes, e, true);
                }

                return method;
            }
        }

        public ExprNode ResolveSingleRowExtendedBuiltin(string name)
        {
            var nameLowerCase = name.ToLowerInvariant();
            if (nameLowerCase.Equals("current_evaluation_context"))
            {
                return new ExprCurrentEvaluationContextNode();
            }

            return null;
        }

        public ExprNode ResolveAggExtendedBuiltin(string name, bool isDistinct)
        {
            if (!_allowExtendedAggregationFunc)
            {
                return null;
            }

            var nameLowerCase = name.ToLowerInvariant();
            switch (nameLowerCase)
            {
                case "first":
                    return new ExprAggMultiFunctionLinearAccessNode(AggregationStateType.FIRST);
                case "last":
                    return new ExprAggMultiFunctionLinearAccessNode(AggregationStateType.LAST);
                case "window":
                    return new ExprAggMultiFunctionLinearAccessNode(AggregationStateType.WINDOW);
                case "firstever":
                    return new ExprFirstEverNode(isDistinct);
                case "lastever":
                    return new ExprLastEverNode(isDistinct);
                case "countever":
                    return new ExprCountEverNode(isDistinct);
                case "minever":
                    return new ExprMinMaxAggrNode(isDistinct, MinMaxTypeEnum.MIN, false, true);
                case "maxever":
                    return new ExprMinMaxAggrNode(isDistinct, MinMaxTypeEnum.MAX, false, true);
                case "fminever":
                    return new ExprMinMaxAggrNode(isDistinct, MinMaxTypeEnum.MIN, true, true);
                case "fmaxever":
                    return new ExprMinMaxAggrNode(isDistinct, MinMaxTypeEnum.MAX, true, true);
                case "rate":
                    return new ExprRateAggNode(isDistinct);
                case "nth":
                    return new ExprNthAggNode(isDistinct);
                case "leaving":
                    return new ExprLeavingAggNode(isDistinct);
                case "maxby":
                    return new ExprAggMultiFunctionSortedMinMaxByNode(true, false, false);
                case "maxbyever":
                    return new ExprAggMultiFunctionSortedMinMaxByNode(true, true, false);
                case "minby":
                    return new ExprAggMultiFunctionSortedMinMaxByNode(false, false, false);
                case "minbyever":
                    return new ExprAggMultiFunctionSortedMinMaxByNode(false, true, false);
                case "sorted":
                    return new ExprAggMultiFunctionSortedMinMaxByNode(false, false, true);
            }

            var cmsType = CountMinSketchAggTypeExtensions.FromNameMayMatch(nameLowerCase);
            if (cmsType != null)
            {
                return new ExprAggCountMinSketchNode(isDistinct, cmsType.Value);
            }

            return null;
        }

        public MathContext DefaultMathContext { get; }

        public TimeZoneInfo TimeZone { get; }

        public TimeAbacus TimeAbacus { get; }

        public ConfigurationEngineDefaults.ThreadingProfile ThreadingProfile { get; }

        public bool IsSortUsingCollator { get; }

        public AggregationFactoryFactory AggregationFactoryFactory { get; }

        public AdvancedIndexFactoryProvider ResolveAdvancedIndexProvider(string indexTypeName)
        {
            var provider = _advancedIndexProviders.Get(indexTypeName);
            if (provider == null)
            {
                throw new EngineImportException("Unrecognized advanced-type index '" + indexTypeName + "'");
            }

            return provider;
        }

        public bool IsCodegenEventPropertyGetters { get; }

        /// <summary>
        ///     Adds cache configs for method invocations for from-clause.
        /// </summary>
        /// <param name="configs">cache configs</param>
        public void AddMethodRefs(IDictionary<string, ConfigurationMethodRef> configs)
        {
            _methodInvocationRef.PutAll(configs);
        }

        /// <summary>
        ///     Finds a class by class name using the auto-import information provided.
        /// </summary>
        /// <param name="className">is the class name to find</param>
        /// <param name="requireAnnotation">whether the class must be an annotation</param>
        /// <param name="forAnnotationUse">whether resolving class for use with annotations</param>
        /// <returns>class</returns>
        public Type ResolveTypeInternal(string className, bool requireAnnotation, bool forAnnotationUse)
        {
            // Attempt to retrieve the class with the name as-is
            try
            {
                return GetClassForNameProvider().ClassForName(className);
            }
            catch (TypeLoadException)
            {
                if (Log.IsDebugEnabled)
                {
                    Log.Debug("Type not found for resolving from name as-is '" + className + "'");
                }
            }

            // check annotation-specific imports first
            if (forAnnotationUse)
            {
                var clazzInner = CheckImports(_annotationImports, requireAnnotation, className);
                if (clazzInner != null)
                {
                    return clazzInner;
                }
            }

            // check all imports
            var clazz = CheckImports(_imports, requireAnnotation, className);
            if (clazz != null)
            {
                return clazz;
            }

            if (!forAnnotationUse)
            {
                // try to resolve from method references
                foreach (var name in _methodInvocationRef.Keys)
                {
                    if (TypeHelper.IsSimpleNameFullyQualfied(className, name))
                    {
                        try
                        {
                            var found = GetClassForNameProvider().ClassForName(name);
                            if (!requireAnnotation || found.IsAttribute())
                            {
                                return found;
                            }
                        }
                        catch (TypeLoadException)
                        {
                            if (Log.IsDebugEnabled)
                            {
                                Log.Debug("Type not found for resolving from method invocation ref:" + name);
                            }
                        }
                    }
                }
            }

            // No import worked, the class isn't resolved
            throw new TypeLoadException("Unknown class " + className);
        }

        private EngineImportException Convert(
            Type clazz,
            string methodName,
            Type[] paramTypes,
            EngineNoSuchMethodException e,
            bool isInstance)
        {
            var expected = TypeHelper.GetParameterAsString(paramTypes);
            var message = "Could not find ";
            if (!isInstance)
            {
                message += "static ";
            }
            else
            {
                message += "enumeration method, date-time method or instance ";
            }

            if (paramTypes.Length > 0)
            {
                message += string.Format(
                    "method named '{0}' in class '{1}' with matching parameter number and expected parameter type(s) '{2}'",
                    methodName, clazz.GetCleanName(), expected);
            }
            else
            {
                message += string.Format(
                    "method named '{0}' in class '{1}' taking no parameters", methodName,
                    clazz.GetCleanName());
            }

            if (e.NearestMissMethod != null)
            {
                message += " (nearest match found was '" + e.NearestMissMethod.Name;
                if (e.NearestMissMethod.GetParameterTypes().Length == 0)
                {
                    message += "' taking no parameters";
                }
                else
                {
                    message += "' taking type(s) '" +
                               TypeHelper.GetParameterAsString(e.NearestMissMethod.GetParameterTypes()) + "'";
                }

                message += ")";
            }

            return new EngineImportException(message, e);
        }

        private EngineImportException Convert(Type clazz, Type[] paramTypes, EngineNoSuchCtorException e)
        {
            var expected = TypeHelper.GetParameterAsString(paramTypes);
            var message = "Could not find constructor ";
            if (paramTypes.Length > 0)
            {
                message += "in class '" + clazz.GetCleanName() +
                           "' with matching parameter number and expected parameter type(s) '" + expected + "'";
            }
            else
            {
                message += "in class '" + clazz.GetCleanName() + "' taking no parameters";
            }

            if (e.NearestMissCtor != null)
            {
                message += " (nearest matching constructor ";
                if (e.NearestMissCtor.GetParameterTypes().Length == 0)
                {
                    message += "taking no parameters";
                }
                else
                {
                    message += "taking type(s) '" +
                               TypeHelper.GetParameterAsString(e.NearestMissCtor.GetParameterTypes()) + "'";
                }

                message += ")";
            }

            return new EngineImportException(message, e);
        }

        public EventPropertyGetter CodegenGetter(EventPropertyGetterSPI getterSPI, string propertyExpression)
        {
            return CodegenEventPropertyGetter.Compile(
                _context, _engineURI, this, getterSPI, propertyExpression);
        }

        private void ValidateFunctionName(string functionType, string functionName)
        {
            var functionNameLower = functionName.ToLowerInvariant();
            if (_aggregationFunctions.ContainsKey(functionNameLower))
            {
                throw new EngineImportException(
                    "Aggregation function by name '" + functionName + "' is already defined");
            }

            if (_singleRowFunctions.ContainsKey(functionNameLower))
            {
                throw new EngineImportException(
                    "Single-row function by name '" + functionName + "' is already defined");
            }

            if (_aggregationAccess.Any(pairs => pairs.First.Contains(functionNameLower)))
            {
                throw new EngineImportException(
                    "Aggregation multi-function by name '" + functionName + "' is already defined");
            }

            if (!IsFunctionName(functionName))
            {
                throw new EngineImportException("Invalid " + functionType + " name '" + functionName + "'");
            }
        }

        private static bool IsFunctionName(string functionName)
        {
            return FunctionRegEx1.IsMatch(functionName);
        }

        private static bool IsTypeName(string importName)
        {
            if (TypeNameRegEx1.IsMatch(importName) || TypeNameRegEx2.IsMatch(importName))
            {
                return true;
            }

            return TypeHelper.ResolveType(importName, false) != null;
        }

        private static bool IsTypeNameOrNamespace(string importName)
        {
            if (TypeNameRegEx1.IsMatch(importName) || TypeNameRegEx2.IsMatch(importName))
            {
                return true;
            }

            return TypeHelper.ResolveType(importName, false) != null;
        }

        public bool IsStrictTypeMatch(AutoImportDesc importDesc, string typeName)
        {
            var importName = importDesc.TypeOrNamespace;
            if (importName == typeName)
            {
                return true;
            }

            var lastIndex = importName.LastIndexOf(typeName);
            if (lastIndex == -1)
            {
                return false;
            }

            if (lastIndex == 0) {
                return false;
            }

            if (importName[lastIndex - 1] == '.' ||
                importName[lastIndex - 1] == '+')
            {
                return true;
            }

            return false;
        }


        private MethodInfo ResolveMethodInternalCheckOverloads(
            Type clazz, string methodName, MethodModifiers methodModifiers)
        {
            var methods = clazz.GetMethods();
            ISet<MethodInfo> overloadeds = null;
            MethodInfo methodByName = null;

            // check each method by name, add to overloads when multiple methods for the same name
            foreach (var method in methods)
            {
                if (method.Name == methodName)
                {
                    var isPublic = method.IsPublic;
                    var isStatic = method.IsStatic;
                    if (methodModifiers.AcceptsPublicFlag(isPublic) &&
                        methodModifiers.AcceptsStaticFlag(isStatic))
                    {
                        if (methodByName != null)
                        {
                            if (overloadeds == null)
                            {
                                overloadeds = new HashSet<MethodInfo>();
                            }

                            overloadeds.Add(method);
                        }
                        else
                        {
                            methodByName = method;
                        }
                    }
                }
            }

            if (methodByName == null)
            {
                throw new EngineImportException(
                    "Could not find " + methodModifiers.Text + " method named '" + methodName + "' in class '" +
                    clazz.FullName + "'");
            }

            if (overloadeds == null)
            {
                return methodByName;
            }

            // determine that all overloads have the same result type
            if (overloadeds.Any(overloaded => overloaded.ReturnType != methodByName.ReturnType))
            {
                throw new EngineImportException(
                    "Method by name '" + methodName + "' is overloaded in class '" + clazz.FullName +
                    "' and overloaded methods do not return the same type");
            }

            return methodByName;
        }

        private Type CheckImports(IList<AutoImportDesc> imports, bool requireAnnotation, string typeName)
        {
            foreach (var importDesc in imports)
            {
                var importName = importDesc.TypeOrNamespace;

                // Test as a class name
                if (IsStrictTypeMatch(importDesc, typeName))
                {
                    var type = TypeHelper.ResolveType(importName, importDesc.AssemblyNameOrFile);
                    if (type != null)
                    {
                        return type;
                    }

                    Log.Debug("Type not found for resolving from name as-is: '{0}'", typeName);
                }
                else
                {
                    if (requireAnnotation && importName == Configuration.ANNOTATION_IMPORT)
                    {
                        var clazz = BuiltinAnnotation.BUILTIN.Get(typeName.ToLower());
                        if (clazz != null)
                        {
                            return clazz;
                        }
                    }

                    if (importDesc.TypeOrNamespace.EndsWith("." + typeName) ||
                        importDesc.TypeOrNamespace.EndsWith("+" + typeName) ||
                        importDesc.TypeOrNamespace.Equals(typeName))
                    {
                        try
                        {
                            var type = TypeHelper.ResolveType(
                                importDesc.TypeOrNamespace, importDesc.AssemblyNameOrFile);
                            if (type != null)
                            {
                                return type;
                            }

                            Log.Debug("Type not found for resolving from name as-is: {0}", typeName);
                        }
                        catch (TypeLoadException)
                        {
                        }
                    }

                    // Import is a namespace
                    var prefixedClassName = importDesc.TypeOrNamespace + '.' + typeName;

                    try
                    {
                        var type = TypeHelper.ResolveType(prefixedClassName, importDesc.AssemblyNameOrFile);
                        if (type != null)
                        {
                            return type;
                        }

                        Log.Debug("Type not found for resolving from name as-is: {0}", typeName);
                    }
                    catch (TypeLoadException)
                    {
                    }

                    prefixedClassName = importDesc.TypeOrNamespace + '+' + typeName;

                    try
                    {
                        var type = TypeHelper.ResolveType(prefixedClassName, importDesc.AssemblyNameOrFile);
                        if (type != null)
                        {
                            return type;
                        }

                        Log.Debug("Type not found for resolving from name as-is: {0}", typeName);
                    }
                    catch (TypeLoadException)
                    {
                    }

                    // Import is a type
                }
            }

            return null;
        }

        private void ValidateImportAndAdd(AutoImportDesc autoImportDesc, ICollection<AutoImportDesc> imports)
        {
            if (!IsTypeNameOrNamespace(autoImportDesc.TypeOrNamespace))
            {
                throw new EngineImportException("Invalid import name '" + autoImportDesc + "'");
            }

            Log.Debug("Adding import {0}", autoImportDesc);

            imports.Add(autoImportDesc);
        }

        internal class MethodModifiers
        {
            public static readonly MethodModifiers REQUIRE_STATIC_AND_PUBLIC =
                new MethodModifiers("public static", true);

            public static readonly MethodModifiers REQUIRE_NONSTATIC_AND_PUBLIC =
                new MethodModifiers("public non-static", false);

            private readonly bool _requiredStaticFlag;

            private MethodModifiers(string text, bool requiredStaticFlag)
            {
                Text = text;
                _requiredStaticFlag = requiredStaticFlag;
            }

            public string Text { get; }

            public bool AcceptsPublicFlag(bool isPublic)
            {
                return isPublic;
            }

            public bool AcceptsStaticFlag(bool isStatic)
            {
                return _requiredStaticFlag == isStatic;
            }
        }
    }
} // end of namespace