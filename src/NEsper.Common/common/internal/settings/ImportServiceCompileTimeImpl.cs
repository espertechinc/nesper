///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.common.client.configuration.compiler;
using com.espertech.esper.common.client.hook.aggfunc;
using com.espertech.esper.common.@internal.epl.agg.access.linear;
using com.espertech.esper.common.@internal.epl.approx.countminsketch;
using com.espertech.esper.common.@internal.epl.expression.agg.accessagg;
using com.espertech.esper.common.@internal.epl.expression.agg.method;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.funcs;
using com.espertech.esper.common.@internal.epl.expression.time.abacus;
using com.espertech.esper.common.@internal.epl.index.advanced.index.quadtree;
using com.espertech.esper.common.@internal.epl.index.advanced.index.service;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.container;

namespace com.espertech.esper.common.@internal.settings
{
    public class ImportServiceCompileTimeImpl
        : ImportServiceBase,
            ImportServiceCompileTime
    {
        private readonly MathContext _mathContext;
        private readonly bool _allowExtendedAggregationFunc;

        private readonly IDictionary<string, ConfigurationCompilerPlugInAggregationFunction> _aggregationFunctions;

        private readonly IList<Pair<ISet<string>, ConfigurationCompilerPlugInAggregationMultiFunction>>
            _aggregationAccess;

        private readonly IDictionary<string, ImportSingleRowDesc> _singleRowFunctions =
            new Dictionary<string, ImportSingleRowDesc>();

        private readonly IDictionary<string, AdvancedIndexFactoryProvider> _advancedIndexProviders =
            new LinkedHashMap<string, AdvancedIndexFactoryProvider>();

        private readonly IDictionary<string, ConfigurationCompilerPlugInDateTimeMethod> _dateTimeMethods;
        private readonly IDictionary<string, ConfigurationCompilerPlugInEnumMethod> _enumMethods;

        public ImportServiceCompileTimeImpl(
            IContainer container,
            IDictionary<string, object> transientConfiguration,
            TimeAbacus timeAbacus,
            ISet<string> eventTypeAutoNames,
            MathContext mathContext,
            bool allowExtendedAggregationFunc,
            bool sortUsingCollator)
            : base(container, transientConfiguration, timeAbacus, eventTypeAutoNames)
        {
            _aggregationFunctions = new Dictionary<string, ConfigurationCompilerPlugInAggregationFunction>();
            _aggregationAccess = new List<Pair<ISet<string>, ConfigurationCompilerPlugInAggregationMultiFunction>>();
            _mathContext = mathContext;
            _allowExtendedAggregationFunc = allowExtendedAggregationFunc;
            IsSortUsingCollator = sortUsingCollator;
            _advancedIndexProviders.Put("pointregionquadtree", new AdvancedIndexFactoryProviderPointRegionQuadTree());
            _advancedIndexProviders.Put("mxcifquadtree", new AdvancedIndexFactoryProviderMXCIFQuadTree());
            _dateTimeMethods = new Dictionary<string, ConfigurationCompilerPlugInDateTimeMethod>();
            _enumMethods = new Dictionary<string, ConfigurationCompilerPlugInEnumMethod>();
        }

        public void AddSingleRow(
            string functionName,
            string singleRowFuncClass,
            string methodName,
            ConfigurationCompilerPlugInSingleRowFunction.ValueCacheEnum valueCache,
            ConfigurationCompilerPlugInSingleRowFunction.FilterOptimizableEnum filterOptimizable,
            bool rethrowExceptions,
            string optionalEventTypeName)
        {
            ValidateFunctionName("single-row", functionName);

            if (!IsClassName(singleRowFuncClass)) {
                throw new ImportException("Invalid class name for aggregation '" + singleRowFuncClass + "'");
            }

            _singleRowFunctions.Put(
                functionName.ToLowerInvariant(),
                new ImportSingleRowDesc(
                    singleRowFuncClass,
                    methodName,
                    valueCache,
                    filterOptimizable,
                    rethrowExceptions,
                    optionalEventTypeName));
        }

        public void AddPlugInDateTimeMethod(
            string dtmMethodName,
            ConfigurationCompilerPlugInDateTimeMethod config)
        {
            ValidateFunctionName("date-time-method", dtmMethodName);

            if (!IsClassName(config.ForgeClassName)) {
                throw new ImportException("Invalid class name for date-time-method '" + config.ForgeClassName + "'");
            }

            _dateTimeMethods.Put(dtmMethodName.ToLowerInvariant(), config);
        }

        public void AddPlugInEnumMethod(
            string dtmMethodName,
            ConfigurationCompilerPlugInEnumMethod config)
        {
            ValidateFunctionName("enum-method", dtmMethodName);

            if (!IsClassName(config.ForgeClassName)) {
                throw new ImportException("Invalid class name for enum-method '" + config.ForgeClassName + "'");
            }

            _enumMethods.Put(dtmMethodName.ToLowerInvariant(), config);
        }

        public Pair<Type, ImportSingleRowDesc> ResolveSingleRow(
            string name,
            ExtensionSingleRow classpathExtensionSingleRow)
        {
            var inlined = classpathExtensionSingleRow.ResolveSingleRow(name);
            if (inlined != null) {
                return inlined;
            }

            if (!_singleRowFunctions.TryGetValue(name, out var pair)) {
                _singleRowFunctions.TryGetValue(name.ToLowerInvariant(), out pair);
            }

            if (pair == null) {
                throw new ImportUndefinedException("A function named '" + name + "' is not defined");
            }

            Type clazz;
            try {
                clazz = TypeResolver.ResolveType(pair.ClassName);
            }
            catch (TypeLoadException ex) {
                throw new ImportException(
                    "Could not load single-row function class by name '" + pair.ClassName + "'",
                    ex);
            }

            return new Pair<Type, ImportSingleRowDesc>(clazz, pair);
        }

        public IEnumerable<string> GetAnnotationNames(string className)
        {
            if (!string.IsNullOrWhiteSpace(className)) {
                // Return the classname itself.
                yield return className;
                // Return the capitalized version of the classname, if it is different
                // than the classname itself.
                string capClassName = null;
                if (char.IsLower(className[0])) {
                    capClassName = char.ToUpperInvariant(className[0]) + className.Substring(1);
                    if (capClassName != className) {
                        yield return capClassName;
                    }
                }

                // Return the "attribute" name if applicable.
                if (!className.EndsWith("Attribute")) {
                    yield return className + "Attribute";
                    // Return the capitalized classname with attribute if applicable.
                    if (capClassName != null) {
                        yield return capClassName + "Attribute";
                    }
                }
            }
        }

        public Type ResolveAnnotation(string className)
        {
            Type clazz = null;
            TypeLoadException firstException = null;

            foreach (var classNameCandidate in GetAnnotationNames(className)) {
                try {
                    clazz = ResolveClassInternal(classNameCandidate, true, true, ExtensionClassEmpty.INSTANCE);
                }
                catch (TypeLoadException e) {
                    firstException ??= e;
                }
            }

            if (clazz == null) {
                throw new ImportException(
                    "Could not load annotation class by name '" + className + "', please check imports",
                    firstException);
            }

            return clazz;
        }

        public MethodInfo ResolveMethod(
            Type clazz,
            string methodName,
            Type[] paramTypes,
            bool[] allowEventBeanType)
        {
            try {
                return MethodResolver.ResolveMethod(
                    clazz,
                    methodName,
                    paramTypes,
                    true,
                    allowEventBeanType,
                    allowEventBeanType);
            }
            catch (MethodResolverNoSuchMethodException e) {
                var method = MethodResolver.ResolveExtensionMethod(
                    clazz,
                    methodName,
                    paramTypes,
                    true,
                    allowEventBeanType,
                    allowEventBeanType);
                if (method != null) {
                    return method;
                }

                throw Convert(clazz, methodName, paramTypes, e, true);
            }
        }

        public MethodInfo ResolveMethodOverloadChecked(
            Type clazz,
            string methodName)
        {
            return ResolveMethodInternalCheckOverloads(clazz, methodName, MethodModifiers.REQUIRE_STATIC_AND_PUBLIC);
        }

        public MethodInfo ResolveMethodOverloadChecked(
            string className,
            string methodName,
            ExtensionClass classpathExtension)
        {
            Type clazz;
            try {
                clazz = ResolveClassInternal(className, false, false, classpathExtension);
            }
            catch (TypeLoadException e) {
                throw new ImportException("Could not load class by name '" + className + "', please check imports", e);
            }

            return ResolveMethodInternalCheckOverloads(clazz, methodName, MethodModifiers.REQUIRE_STATIC_AND_PUBLIC);
        }

        public ExprNode ResolveSingleRowExtendedBuiltin(string name)
        {
            var nameLowerCase = name.ToLowerInvariant();
            if (nameLowerCase == "current_evaluation_context") {
                return new ExprCurrentEvaluationContextNode();
            }

            if (nameLowerCase == ExprEventIdentityEqualsNode.NAME) {
                return new ExprEventIdentityEqualsNode();
            }

            return null;
        }

        public MathContext DefaultMathContext => _mathContext;

        private void ValidateFunctionName(
            string functionType,
            string functionName)
        {
            var functionNameLower = functionName.ToLowerInvariant();
            if (_aggregationFunctions.ContainsKey(functionNameLower)) {
                throw new ImportException("Aggregation function by name '" + functionName + "' is already defined");
            }

            if (_singleRowFunctions.ContainsKey(functionNameLower)) {
                throw new ImportException("Single-row function by name '" + functionName + "' is already defined");
            }

            foreach (var pairs in _aggregationAccess) {
                if (pairs.First.Contains(functionNameLower)) {
                    throw new ImportException(
                        "Aggregation multi-function by name '" + functionName + "' is already defined");
                }
            }

            if (!IsFunctionName(functionName)) {
                throw new ImportException("Invalid " + functionType + " name '" + functionName + "'");
            }
        }

        private static bool IsFunctionName(string functionName)
        {
            var classNameRegEx = "\\w+";
            return functionName.Matches(classNameRegEx);
        }

        public ExprNode ResolveAggExtendedBuiltin(
            string name,
            bool isDistinct)
        {
            if (!_allowExtendedAggregationFunc) {
                return null;
            }

            var nameLowerCase = name.ToLowerInvariant();
            switch (nameLowerCase) {
                case "first":
                    return new ExprAggMultiFunctionLinearAccessNode(AggregationAccessorLinearType.FIRST);

                case "last":
                    return new ExprAggMultiFunctionLinearAccessNode(AggregationAccessorLinearType.LAST);

                case "window":
                    return new ExprAggMultiFunctionLinearAccessNode(AggregationAccessorLinearType.WINDOW);

                case "firstever":
                    return new ExprFirstLastEverNode(isDistinct, true);

                case "lastever":
                    return new ExprFirstLastEverNode(isDistinct, false);

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
            if (cmsType != null) {
                return new ExprAggMultiFunctionCountMinSketchNode(isDistinct, cmsType.Value);
            }

            return null;
        }

        public bool IsSortUsingCollator { get; }

        public MethodInfo ResolveNonStaticMethodOverloadChecked(
            Type clazz,
            string methodName)
        {
            return ResolveMethodInternalCheckOverloads(clazz, methodName, MethodModifiers.REQUIRE_NONSTATIC_AND_PUBLIC);
        }

        private MethodInfo ResolveMethodInternalCheckOverloads(
            Type clazz,
            string methodName,
            MethodModifiers methodModifiers)
        {
            var methods = clazz.GetMethods();
            ISet<MethodInfo> overloadeds = null;
            MethodInfo methodByName = null;

            // check each method by name, add to overloads when multiple methods for the same name
            foreach (var method in methods) {
                if (method.Name.Equals(methodName)) {
                    var isPublic = method.IsPublic;
                    var isStatic = method.IsStatic;
                    if (methodModifiers.AcceptsPublicFlag(isPublic) && methodModifiers.AcceptsStaticFlag(isStatic)) {
                        if (methodByName != null) {
                            if (overloadeds == null) {
                                overloadeds = new HashSet<MethodInfo>();
                            }

                            overloadeds.Add(method);
                        }
                        else {
                            methodByName = method;
                        }
                    }
                }
            }

            if (methodByName == null) {
                throw new ImportException(
                    "Could not find " +
                    methodModifiers.Text +
                    " method named '" +
                    methodName +
                    "' in class '" +
                    clazz.CleanName() +
                    "'");
            }

            if (overloadeds == null) {
                return methodByName;
            }

            // determine that all overloads have the same result type
            foreach (var overloaded in overloadeds) {
                if (overloaded.ReturnType != methodByName.ReturnType) {
                    throw new ImportException(
                        "Method by name '" +
                        methodName +
                        "' is overloaded in class '" +
                        clazz.CleanName() +
                        "' and overloaded methods do not return the same type");
                }
            }

            return methodByName;
        }

        public AdvancedIndexFactoryProvider ResolveAdvancedIndexProvider(string indexTypeName)
        {
            var provider = _advancedIndexProviders.Get(indexTypeName);
            if (provider == null) {
                throw new ImportException("Unrecognized advanced-type index '" + indexTypeName + "'");
            }

            return provider;
        }

        public AggregationFunctionForge ResolveAggregationFunction(
            string functionName,
            ExtensionAggregationFunction extension)
        {
            var inlined = extension.ResolveAggregationFunction(functionName);

            Type forgeClass;
            string className;
            if (inlined != null) {
                forgeClass = inlined;
                className = inlined.Name;
            }
            else {
                var desc = _aggregationFunctions.Get(functionName);
                if (desc == null) {
                    desc = _aggregationFunctions.Get(functionName.ToLowerInvariant());
                }

                if (desc == null || desc.ForgeClassName == null) {
                    throw new ImportUndefinedException("A function named '" + functionName + "' is not defined");
                }

                className = desc.ForgeClassName;
                try {
                    forgeClass = TypeResolver.ResolveType(className, true);
                    if (forgeClass == null) {
                        throw new ImportException(
                            "Could not load aggregation factory class by name '" + className + "'");
                    }
                }
                catch (TypeLoadException ex) {
                    throw new ImportException(
                        "Could not load aggregation factory class by name '" + className + "'",
                        ex);
                }
            }

            object @object;
            try {
                @object = TypeHelper.Instantiate(forgeClass);
            }
            catch (TypeLoadException e) {
                throw new ImportException(
                    "Error instantiating aggregation factory class by name '" + className + "'",
                    e);
            }
            catch (MemberAccessException e) {
                throw new ImportException(
                    "Illegal access instatiating aggregation factory class by name '" + className + "'",
                    e);
            }

            if (!(@object is AggregationFunctionForge forge)) {
                throw new ImportException(
                    "Class by name '" +
                    className +
                    "' does not implement the " +
                    nameof(AggregationFunctionForge) +
                    " interface");
            }

            return forge;
        }

        public void AddAggregation(
            string functionName,
            ConfigurationCompilerPlugInAggregationFunction aggregationDesc)
        {
            ValidateFunctionName("aggregation function", functionName);
            if (aggregationDesc.ForgeClassName == null || !IsClassName(aggregationDesc.ForgeClassName)) {
                throw new ImportException(
                    "Invalid class name for aggregation function forge '" + aggregationDesc.ForgeClassName + "'");
            }

            _aggregationFunctions.Put(functionName.ToLowerInvariant(), aggregationDesc);
        }

        public Pair<ConfigurationCompilerPlugInAggregationMultiFunction, Type> ResolveAggregationMultiFunction(
            string name,
            ExtensionAggregationMultiFunction extension)
        {
            foreach (var config in _aggregationAccess) {
                if (config.First.Contains(name.ToLowerInvariant())) {
                    return new Pair<ConfigurationCompilerPlugInAggregationMultiFunction, Type>(config.Second, null);
                }
            }

            var inlined = extension.ResolveAggregationMultiFunction(name);
            if (inlined != null) {
                var config =
                    new ConfigurationCompilerPlugInAggregationMultiFunction(inlined.Second, inlined.First.Name);
                return new Pair<ConfigurationCompilerPlugInAggregationMultiFunction, Type>(config, inlined.First);
            }

            return null;
        }

        public void AddAggregationMultiFunction(ConfigurationCompilerPlugInAggregationMultiFunction desc)
        {
            var orderedImmutableFunctionNames = new LinkedHashSet<string>();
            foreach (var functionName in desc.FunctionNames) {
                orderedImmutableFunctionNames.Add(functionName.ToLowerInvariant());
                ValidateFunctionName("aggregation multi-function", functionName.ToLowerInvariant());
            }

            if (!IsClassName(desc.MultiFunctionForgeClassName)) {
                throw new ImportException(
                    "Invalid class name for aggregation multi-function factory '" +
                    desc.MultiFunctionForgeClassName +
                    "'");
            }

            _aggregationAccess.Add(
                new Pair<ISet<string>, ConfigurationCompilerPlugInAggregationMultiFunction>(
                    orderedImmutableFunctionNames,
                    desc));
        }

        public Type ResolveDateTimeMethod(string name)
        {
            var dtm = _dateTimeMethods.Get(name);
            if (dtm == null) {
                dtm = _dateTimeMethods.Get(name.ToLowerInvariant());
            }

            if (dtm == null) {
                return null;
            }

            Type clazz;
            try {
                clazz = TypeResolver.ResolveType(dtm.ForgeClassName);
            }
            catch (TypeLoadException ex) {
                throw new ImportException(
                    "Could not load date-time-method forge class by name '" + dtm.ForgeClassName + "'",
                    ex);
            }

            return clazz;
        }

        public Type ResolveEnumMethod(string name)
        {
            var enumMethod = _enumMethods.Get(name);
            if (enumMethod == null) {
                enumMethod = _enumMethods.Get(name.ToLowerInvariant());
            }

            if (enumMethod == null) {
                return null;
            }

            Type clazz;
            try {
                clazz = TypeResolver.ResolveType(enumMethod.ForgeClassName);
            }
            catch (TypeLoadException ex) {
                throw new ImportException(
                    "Could not load enum-method forge class by name '" + enumMethod.ForgeClassName + "'",
                    ex);
            }

            return clazz;
        }

        private class MethodModifiers
        {
            public static readonly MethodModifiers REQUIRE_STATIC_AND_PUBLIC =
                new MethodModifiers("public static", true);

            public static readonly MethodModifiers REQUIRE_NONSTATIC_AND_PUBLIC =
                new MethodModifiers("public non-static", false);

            private readonly bool _requiredStaticFlag;

            private MethodModifiers(
                string text,
                bool requiredStaticFlag)
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