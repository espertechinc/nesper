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

using com.espertech.esper.client;
using com.espertech.esper.client.annotation;
using com.espertech.esper.client.hook;
using com.espertech.esper.client.util;
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
using com.espertech.esper.type;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.core
{
    /// <summary>Implementation for engine-level imports.</summary>
    public class EngineImportServiceImpl : EngineImportService
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    
        private readonly IList<string> _imports;
        private readonly IList<string> _annotationImports;
        private readonly IDictionary<string, ConfigurationPlugInAggregationFunction> _aggregationFunctions;
        private readonly IList<Pair<ISet<string>, ConfigurationPlugInAggregationMultiFunction>> _aggregationAccess;
        private readonly IDictionary<string, EngineImportSingleRowDesc> _singleRowFunctions;
        private readonly IDictionary<string, ConfigurationMethodRef> _methodInvocationRef;
        private readonly bool _allowExtendedAggregationFunc;
        private readonly bool _isUdfCache;
        private readonly bool _isDuckType;
        private readonly bool _sortUsingCollator;
        private readonly MathContext _optionalDefaultMathContext;
        private readonly TimeZoneInfo _timeZone;
        private readonly TimeAbacus _timeAbacus;
        private readonly ConfigurationEngineDefaults.ThreadingProfile _threadingProfile;
        private readonly IDictionary<string, Object> _transientConfiguration;
        private readonly AggregationFactoryFactory _aggregationFactoryFactory;

        public EngineImportServiceImpl(
            bool allowExtendedAggregationFunc,
            bool isUdfCache,
            bool isDuckType,
            bool sortUsingCollator,
            MathContext optionalDefaultMathContext,
            TimeZoneInfo timeZone,
            TimeAbacus timeAbacus,
            ConfigurationEngineDefaults.ThreadingProfile threadingProfile,
            IDictionary<string, Object> transientConfiguration,
            AggregationFactoryFactory aggregationFactoryFactory)
        {
            _imports = new List<string>();
            _annotationImports = new List<string>(2);
            _aggregationFunctions = new Dictionary<string, ConfigurationPlugInAggregationFunction>();
            _aggregationAccess = new List<Pair<ISet<string>, ConfigurationPlugInAggregationMultiFunction>>();
            _singleRowFunctions = new Dictionary<string, EngineImportSingleRowDesc>();
            _methodInvocationRef = new Dictionary<string, ConfigurationMethodRef>();
            _allowExtendedAggregationFunc = allowExtendedAggregationFunc;
            _isUdfCache = isUdfCache;
            _isDuckType = isDuckType;
            _sortUsingCollator = sortUsingCollator;
            _optionalDefaultMathContext = optionalDefaultMathContext;
            _timeZone = timeZone;
            _timeAbacus = timeAbacus;
            _threadingProfile = threadingProfile;
            _transientConfiguration = transientConfiguration;
            _aggregationFactoryFactory = aggregationFactoryFactory;
        }
    
        private static bool IsFunctionName(string functionName) {
            var classNameRegEx = "\\w+";
            return functionName.Matches(classNameRegEx);
        }
    
        private static bool IsClassName(string importName) {
            var classNameRegEx = "(\\w+\\.)*\\w+(\\$\\w+)?";
            return importName.Matches(classNameRegEx);
        }
    
        private static bool IsPackageName(string importName) {
            var classNameRegEx = "(\\w+\\.)+\\*";
            return importName.Matches(classNameRegEx);
        }
    
        // Strip off the final ".*"
        private static string GetPackageName(string importName) {
            return importName.Substring(0, importName.Length - 2);
        }

        public bool IsUdfCache
        {
            get { return _isUdfCache; }
        }

        public bool IsDuckType
        {
            get { return _isDuckType; }
        }

        public ConfigurationMethodRef GetConfigurationMethodRef(string className) {
            return _methodInvocationRef.Get(className);
        }
    
        public ClassForNameProvider GetClassForNameProvider() {
            return TransientConfigurationResolver.ResolveClassForNameProvider(_transientConfiguration);
        }
    
        public ClassLoader GetFastClassClassLoader(Type clazz) {
            return TransientConfigurationResolver.ResolveFastClassClassLoaderProvider(_transientConfiguration).Classloader(clazz);
        }
    
        public ClassLoader GetClassLoader() {
            return TransientConfigurationResolver.ResolveClassLoader(_transientConfiguration).Classloader();
        }

        /// <summary>
        /// Adds cache configs for method invocations for from-clause.
        /// </summary>
        /// <param name="configs">cache configs</param>
        public void AddMethodRefs(IDictionary<string, ConfigurationMethodRef> configs)
        {
            _methodInvocationRef.PutAll(configs);
        }

        public void AddImport(string importName)
        {
            ValidateImportAndAdd(importName, _imports);
        }

        public void AddAnnotationImport(string importName)
        {
            ValidateImportAndAdd(importName, _annotationImports);
        }

        public void AddAggregation(string functionName, ConfigurationPlugInAggregationFunction aggregationDesc)
        {
            ValidateFunctionName("aggregation function", functionName);
            if (aggregationDesc.FactoryClassName == null || !IsClassName(aggregationDesc.FactoryClassName))
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
            ConfigurationPlugInSingleRowFunction.ValueCacheEnum valueCache,
            ConfigurationPlugInSingleRowFunction.FilterOptimizableEnum filterOptimizable,
            bool rethrowExceptions,
            string optionalEventTypeName)
        {
            ValidateFunctionName("single-row", functionName);

            if (!IsClassName(singleRowFuncClass))
            {
                throw new EngineImportException("Invalid class name for aggregation '" + singleRowFuncClass + "'");
            }
            _singleRowFunctions.Put(
                functionName.ToLowerInvariant(),
                new EngineImportSingleRowDesc(
                    singleRowFuncClass, methodName, valueCache, filterOptimizable, rethrowExceptions,
                    optionalEventTypeName));
        }

        public AggregationFunctionFactory ResolveAggregationFactory(string name) {
            var desc = _aggregationFunctions.Get(name);
            if (desc == null) {
                desc = _aggregationFunctions.Get(name.ToLowerInvariant());
            }
            if (desc == null || desc.FactoryClassName == null) {
                throw new EngineImportUndefinedException("A function named '" + name + "' is not defined");
            }
    
            var className = desc.FactoryClassName;
            Type clazz;
            try {
                clazz = GetClassForNameProvider().ClassForName(className);
            } catch (TypeLoadException ex) {
                throw new EngineImportException("Could not load aggregation factory class by name '" + className + "'", ex);
            }
    
            Object @object;
            try {
                @object = clazz.NewInstance();
            } catch (InstantiationException e) {
                throw new EngineImportException("Error instantiating aggregation factory class by name '" + className + "'", e);
            } catch (IllegalAccessException e) {
                throw new EngineImportException("Illegal access instatiating aggregation factory class by name '" + className + "'", e);
            }
    
            if (!(@object is AggregationFunctionFactory)) {
                throw new EngineImportException("Aggregation class by name '" + className + "' does not implement AggregationFunctionFactory");
            }
            return (AggregationFunctionFactory) @object;
        }
    
        public void AddAggregationMultiFunction(ConfigurationPlugInAggregationMultiFunction desc) {
            var orderedImmutableFunctionNames = new LinkedHashSet<string>();
            foreach (var functionName in desc.FunctionNames) {
                orderedImmutableFunctionNames.Add(functionName.ToLowerInvariant());
                ValidateFunctionName("aggregation multi-function", functionName.ToLowerInvariant());
            }
            if (!IsClassName(desc.MultiFunctionFactoryClassName)) {
                throw new EngineImportException("Invalid class name for aggregation multi-function factory '" + desc.MultiFunctionFactoryClassName + "'");
            }
            _aggregationAccess.Add(new Pair<ISet<string>, ConfigurationPlugInAggregationMultiFunction>(orderedImmutableFunctionNames, desc));
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

        public MethodInfo ResolveMethodOverloadChecked(string className, string methodName, Type[] paramTypes, bool[] allowEventBeanType, bool[] allowEventBeanCollType)
                {
            Type clazz;
            try {
                clazz = ResolveClassInternal(className, false, false);
            }
            catch (TypeLoadException e)
            {
                throw new EngineImportException("Could not load class by name '" + className + "', please check imports", e);
            }
    
            try {
                return MethodResolver.ResolveMethod(clazz, methodName, paramTypes, false, allowEventBeanType, allowEventBeanCollType);
            } catch (EngineNoSuchMethodException e) {
                throw Convert(clazz, methodName, paramTypes, e, false);
            }
        }
    
        public ConstructorInfo ResolveCtor(Type clazz, Type[] paramTypes) {
            try {
                return MethodResolver.ResolveCtor(clazz, paramTypes);
            } catch (EngineNoSuchCtorException e) {
                throw Convert(clazz, paramTypes, e);
            }
        }
    
        public MethodInfo ResolveMethodOverloadChecked(string className, string methodName) {
            Type clazz;
            try {
                clazz = ResolveClassInternal(className, false, false);
            }
            catch (TypeLoadException e)
            {
                throw new EngineImportException("Could not load class by name '" + className + "', please check imports", e);
            }
            return ResolveMethodInternalCheckOverloads(clazz, methodName, MethodModifiers.REQUIRE_STATIC_AND_PUBLIC);
        }
    
        public MethodInfo ResolveMethodOverloadChecked(Type clazz, string methodName) {
            return ResolveMethodInternalCheckOverloads(clazz, methodName, MethodModifiers.REQUIRE_STATIC_AND_PUBLIC);
        }
    
        public MethodInfo ResolveNonStaticMethodOverloadChecked(Type clazz, string methodName) {
            return ResolveMethodInternalCheckOverloads(clazz, methodName, MethodModifiers.REQUIRE_NONSTATIC_AND_PUBLIC);
        }
    
        public Type ResolveClass(string className, bool forAnnotation) {
            Type clazz;
            try {
                clazz = ResolveClassInternal(className, false, forAnnotation);
            } catch (TypeLoadException e) {
                throw new EngineImportException("Could not load class by name '" + className + "', please check imports", e);
            }
    
            return clazz;
        }
    
        public Type ResolveAnnotation(string className) {
            Type clazz;
            try {
                clazz = ResolveClassInternal(className, true, true);
            }
            catch (TypeLoadException e)
            {
                throw new EngineImportException("Could not load annotation class by name '" + className + "', please check imports", e);
            }
    
            return clazz;
        }
    
        /// <summary>
        /// Finds a class by class name using the auto-import information provided.
        /// </summary>
        /// <param name="className">is the class name to find</param>
        /// <param name="requireAnnotation">whether the class must be an annotation</param>
        /// <param name="forAnnotationUse">whether resolving class for use with annotations</param>
        /// <exception cref="ClassNotFoundException">if the class cannot be loaded</exception>
        /// <returns>class</returns>
        protected Type ResolveClassInternal(string className, bool requireAnnotation, bool forAnnotationUse) {
            // Attempt to retrieve the class with the name as-is
            try {
                return GetClassForNameProvider().ClassForName(className);
            }
            catch (TypeLoadException e)
            {
                if (Log.IsDebugEnabled) {
                    Log.Debug("Type not found for resolving from name as-is '" + className + "'");
                }
            }
    
            // check annotation-specific imports first
            if (forAnnotationUse) {
                var clazzInner = CheckImports(_annotationImports, requireAnnotation, className);
                if (clazzInner != null) {
                    return clazzInner;
                }
            }
    
            // check all imports
            var clazz = CheckImports(_imports, requireAnnotation, className);
            if (clazz != null) {
                return clazz;
            }
    
            if (!forAnnotationUse) {
                // try to resolve from method references
                foreach (var name in _methodInvocationRef.Keys) {
                    if (TypeHelper.IsSimpleNameFullyQualfied(className, name)) {
                        try {
                            var found = GetClassForNameProvider().ClassForName(name);
                            if (!requireAnnotation || found.IsAttribute()) {
                                return found;
                            }
                        }
                        catch (TypeLoadException e1)
                        {
                            if (Log.IsDebugEnabled) {
                                Log.Debug("Type not found for resolving from method invocation ref:" + name);
                            }
                        }
                    }
                }
            }
    
            // No import worked, the class isn't resolved
            throw new TypeLoadException("Unknown class " + className);
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
                throw Convert(clazz, methodName, paramTypes, e, true);
            }
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
                message += string.Format("method named '{0}' in class '{1}' with matching parameter number and expected parameter Type(s) '{2}'", methodName, clazz.GetTypeNameFullyQualPretty(), expected);
            }
            else
            {
                message += string.Format("method named '{0}' in class '{1}' taking no parameters", methodName, clazz.GetTypeNameFullyQualPretty());
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
                    message += "' taking Type(s) '" +
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
                message += "in class '" + clazz.GetTypeNameFullyQualPretty() +
                           "' with matching parameter number and expected parameter Type(s) '" + expected + "'";
            }
            else
            {
                message += "in class '" + clazz.GetTypeNameFullyQualPretty() + "' taking no parameters";
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
                    message += "taking Type(s) '" +
                               TypeHelper.GetParameterAsString(e.NearestMissCtor.GetParameterTypes()) + "'";
                }
                message += ")";
            }
            return new EngineImportException(message, e);
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
                case ("first"):
                    return new ExprAggMultiFunctionLinearAccessNode(AggregationStateType.FIRST);
                case ("last"):
                    return new ExprAggMultiFunctionLinearAccessNode(AggregationStateType.LAST);
                case ("window"):
                    return new ExprAggMultiFunctionLinearAccessNode(AggregationStateType.WINDOW);
                case ("firstever"):
                    return new ExprFirstEverNode(isDistinct);
                case ("lastever"):
                    return new ExprLastEverNode(isDistinct);
                case ("countever"):
                    return new ExprCountEverNode(isDistinct);
                case ("minever"):
                    return new ExprMinMaxAggrNode(isDistinct, MinMaxTypeEnum.MIN, false, true);
                case ("maxever"):
                    return new ExprMinMaxAggrNode(isDistinct, MinMaxTypeEnum.MAX, false, true);
                case ("fminever"):
                    return new ExprMinMaxAggrNode(isDistinct, MinMaxTypeEnum.MIN, true, true);
                case ("fmaxever"):
                    return new ExprMinMaxAggrNode(isDistinct, MinMaxTypeEnum.MAX, true, true);
                case ("rate"):
                    return new ExprRateAggNode(isDistinct);
                case ("nth"):
                    return new ExprNthAggNode(isDistinct);
                case ("leaving"):
                    return new ExprLeavingAggNode(isDistinct);
                case ("maxby"):
                    return new ExprAggMultiFunctionSortedMinMaxByNode(true, false, false);
                case ("maxbyever"):
                    return new ExprAggMultiFunctionSortedMinMaxByNode(true, true, false);
                case ("minby"):
                    return new ExprAggMultiFunctionSortedMinMaxByNode(false, false, false);
                case ("minbyever"):
                    return new ExprAggMultiFunctionSortedMinMaxByNode(false, true, false);
                case ("sorted"):
                    return new ExprAggMultiFunctionSortedMinMaxByNode(false, false, true);
            }

            var cmsType = CountMinSketchAggTypeExtensions.FromNameMayMatch(nameLowerCase);
            if (cmsType != null)
            {
                return new ExprAggCountMinSketchNode(isDistinct, cmsType.Value);
            }
            return null;
        }

        public MathContext DefaultMathContext
        {
            get { return _optionalDefaultMathContext; }
        }

        public TimeZoneInfo TimeZone
        {
            get { return _timeZone; }
        }

        public TimeAbacus TimeAbacus
        {
            get { return _timeAbacus; }
        }

        public ConfigurationEngineDefaults.ThreadingProfile ThreadingProfile
        {
            get { return _threadingProfile; }
        }

        public bool IsSortUsingCollator
        {
            get { return _sortUsingCollator; }
        }

        public AggregationFactoryFactory AggregationFactoryFactory
        {
            get { return _aggregationFactoryFactory; }
        }

        /// <summary>
        /// For testing, returns imports.
        /// </summary>
        /// <value>returns auto-import list as array</value>
        protected string[] Imports
        {
            get { return _imports.ToArray(); }
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
                throw new EngineImportException("Single-row function by name '" + functionName + "' is already defined");
            }
            foreach (var pairs in _aggregationAccess)
            {
                if (pairs.First.Contains(functionNameLower))
                {
                    throw new EngineImportException(
                        "Aggregation multi-function by name '" + functionName + "' is already defined");
                }
            }
            if (!IsFunctionName(functionName))
            {
                throw new EngineImportException("Invalid " + functionType + " name '" + functionName + "'");
            }
        }

        private MethodInfo ResolveMethodInternalCheckOverloads(Type clazz, string methodName, MethodModifiers methodModifiers)
        {
            MethodInfo[] methods = clazz.GetMethods();
            ISet<MethodInfo> overloadeds = null;
            MethodInfo methodByName = null;
    
            // check each method by name, add to overloads when multiple methods for the same name
            foreach (var method in methods) {
                if (method.Name.Equals(methodName)) {
                    int modifiers = method.Modifiers;
                    bool isPublic = Modifier.IsPublic(modifiers);
                    bool isStatic = Modifier.IsStatic(modifiers);
                    if (methodModifiers.AcceptsPublicFlag(isPublic) && methodModifiers.AcceptsStaticFlag(isStatic)) {
                        if (methodByName != null) {
                            if (overloadeds == null) {
                                overloadeds = new HashSet<>();
                            }
                            overloadeds.Add(method);
                        } else {
                            methodByName = method;
                        }
                    }
                }
            }
            if (methodByName == null) {
                throw new EngineImportException("Could not find " + methodModifiers.Text + " method named '" + methodName + "' in class '" + clazz.Name + "'");
            }
            if (overloadeds == null) {
                return methodByName;
            }
    
            // determine that all overloads have the same result type
            foreach (var overloaded in overloadeds) {
                if (overloaded.ReturnType != methodByName.ReturnType) {
                    throw new EngineImportException("Method by name '" + methodName + "' is overloaded in class '" + clazz.Name + "' and overloaded methods do not return the same type");
                }
            }
    
            return methodByName;
        }
    
        private Type CheckImports(IEnumerable<string> imports, bool requireAnnotation, string className)
        {
            // Try all the imports
            foreach (var importName in imports)
            {
                var isClassName = IsClassName(importName);
                var containsPackage = importName.IndexOf('.') != -1;
                var classNameWithDot = "." + className;
                var classNameWithDollar = "$" + className;
    
                // Import is a class name
                if (isClassName) {
                    if ((containsPackage && importName.EndsWith(classNameWithDot)) ||
                            (containsPackage && importName.EndsWith(classNameWithDollar)) ||
                            (!containsPackage && importName.Equals(className)) ||
                            (!containsPackage && importName.EndsWith(classNameWithDollar))) {
                        return GetClassForNameProvider().ClassForName(importName);
                    }
    
                    var prefixedClassName = importName + '$' + className;
                    try {
                        var clazz = GetClassForNameProvider().ClassForName(prefixedClassName);
                        if (!requireAnnotation || clazz.IsAttribute()) {
                            return clazz;
                        }
                    }
                    catch (TypeLoadException e)
                    {
                        if (Log.IsDebugEnabled) {
                            Log.Debug("Type not found for resolving from name '" + prefixedClassName + "'");
                        }
                    }
                } else {
                    if (requireAnnotation && importName.Equals(Configuration.ANNOTATION_IMPORT)) {
                        var clazz = BuiltinAnnotation.BUILTIN.Get(className.ToLowerInvariant());
                        if (clazz != null) {
                            return clazz;
                        }
                    }
    
                    // Import is a package name
                    var prefixedClassName = GetPackageName(importName) + '.' + className;
                    try {
                        var clazz = GetClassForNameProvider().ClassForName(prefixedClassName);
                        if (!requireAnnotation || clazz.IsAttribute()) {
                            return clazz;
                        }
                    }
                    catch (TypeLoadException e)
                    {
                        if (Log.IsDebugEnabled) {
                            Log.Debug("Type not found for resolving from name '" + prefixedClassName + "'");
                        }
                    }
                }
            }
    
            return null;
        }

        private void ValidateImportAndAdd(string importName, IList<string> imports)
        {
            if (!IsClassName(importName) && !IsPackageName(importName))
            {
                throw new EngineImportException("Invalid import name '" + importName + "'");
            }
            if (Log.IsDebugEnabled)
            {
                Log.Debug("Adding import " + importName);
            }

            imports.Add(importName);
        }

        internal class MethodModifiers
        {
            public static readonly MethodModifiers REQUIRE_STATIC_AND_PUBLIC =
                new MethodModifiers("public static", true);

            public static readonly MethodModifiers REQUIRE_NONSTATIC_AND_PUBLIC =
                new MethodModifiers("public non-static", false);

            private readonly String _text;
            private readonly bool _requiredStaticFlag;

            MethodModifiers(String text, bool requiredStaticFlag)
            {
                _text = text;
                _requiredStaticFlag = requiredStaticFlag;
            }

            public bool AcceptsPublicFlag(bool isPublic)
            {
                return isPublic;
            }

            public bool AcceptsStaticFlag(bool isStatic)
            {
                return _requiredStaticFlag == isStatic;
            }

            public string Text
            {
                get { return _text; }
            }
        }
    }
} // end of namespace
