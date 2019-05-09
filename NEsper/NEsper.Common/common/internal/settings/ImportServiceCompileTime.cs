///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.collection;
using com.espertech.esper.common.client.configuration.compiler;
using com.espertech.esper.common.client.hook.aggfunc;
using com.espertech.esper.common.@internal.epl.agg.access.linear;
using com.espertech.esper.common.@internal.epl.approx.countminsketch;
using com.espertech.esper.common.@internal.epl.expression.agg.accessagg;
using com.espertech.esper.common.@internal.epl.expression.agg.method;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.time.abacus;
using com.espertech.esper.common.@internal.epl.index.advanced.index.quadtree;
using com.espertech.esper.common.@internal.epl.index.advanced.index.service;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using DictionaryExtensions = com.espertech.esper.compat.collections.DictionaryExtensions;

namespace com.espertech.esper.common.@internal.settings
{
    public class ImportServiceCompileTime : ImportServiceBase
    {
        public const string EXT_SINGLEROW_FUNCTION_TRANSPOSE = "transpose";

        private readonly LinkedHashMap<string, AdvancedIndexFactoryProvider> advancedIndexProviders =
            new LinkedHashMap<string, AdvancedIndexFactoryProvider>();

        private readonly IList<Pair<ISet<string>, ConfigurationCompilerPlugInAggregationMultiFunction>>
            aggregationAccess;

        private readonly IDictionary<string, ConfigurationCompilerPlugInAggregationFunction> aggregationFunctions;
        private readonly bool allowExtendedAggregationFunc;

        private readonly IDictionary<string, ImportSingleRowDesc> singleRowFunctions =
            new Dictionary<string, ImportSingleRowDesc>();

        private readonly bool sortUsingCollator;

        public ImportServiceCompileTime(
            IDictionary<string, object> transientConfiguration,
            TimeAbacus timeAbacus,
            ISet<string> eventTypeAutoNames,
            MathContext mathContext,
            bool allowExtendedAggregationFunc,
            bool sortUsingCollator)
            : base(transientConfiguration, timeAbacus, eventTypeAutoNames)
        {
            aggregationFunctions = new Dictionary<string, ConfigurationCompilerPlugInAggregationFunction>();
            aggregationAccess = new List<Pair<ISet<string>, ConfigurationCompilerPlugInAggregationMultiFunction>>();
            DefaultMathContext = mathContext;
            this.allowExtendedAggregationFunc = allowExtendedAggregationFunc;
            this.sortUsingCollator = sortUsingCollator;
            advancedIndexProviders.Put("pointregionquadtree", new AdvancedIndexFactoryProviderPointRegionQuadTree());
            advancedIndexProviders.Put("mxcifquadtree", new AdvancedIndexFactoryProviderMXCIFQuadTree());
        }

        public MathContext DefaultMathContext { get; }

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

            if (!IsClassName(singleRowFuncClass))
            {
                throw new ImportException("Invalid class name for aggregation '" + singleRowFuncClass + "'");
            }

            DictionaryExtensions.Put(
                singleRowFunctions, functionName.ToLowerInvariant(),
                new ImportSingleRowDesc(
                    singleRowFuncClass, methodName, valueCache, filterOptimizable, rethrowExceptions,
                    optionalEventTypeName));
        }

        public Pair<Type, ImportSingleRowDesc> ResolveSingleRow(string name)
        {
            var pair = DictionaryExtensions.Get(singleRowFunctions, name);
            if (pair == null)
            {
                pair = DictionaryExtensions.Get(singleRowFunctions, name.ToLowerInvariant());
            }

            if (pair == null)
            {
                throw new ImportUndefinedException("A function named '" + name + "' is not defined");
            }

            Type clazz;
            try
            {
                clazz = ClassForNameProvider.ClassForName(pair.ClassName);
            }
            catch (TypeLoadException ex)
            {
                throw new ImportException(
                    "Could not load single-row function class by name '" + pair.ClassName + "'", ex);
            }

            return new Pair<Type, ImportSingleRowDesc>(clazz, pair);
        }

        public Type ResolveAnnotation(string className)
        {
            Type clazz;
            try
            {
                clazz = ResolveClassInternal(className, true, true);
            }
            catch (TypeLoadException e)
            {
                throw new ImportException(
                    "Could not load annotation class by name '" + className + "', please check imports", e);
            }

            return clazz;
        }

        public MethodInfo ResolveMethod(
            Type clazz,
            string methodName,
            Type[] paramTypes,
            bool[] allowEventBeanType)
        {
            try
            {
                return MethodResolver.ResolveMethod(
                    clazz, methodName, paramTypes, true, allowEventBeanType, allowEventBeanType);
            }
            catch (MethodResolverNoSuchMethodException e)
            {
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
            string methodName)
        {
            Type clazz;
            try
            {
                clazz = ResolveClassInternal(className, false, false);
            }
            catch (TypeLoadException e)
            {
                throw new ImportException(
                    "Could not load class by name '" + className + "', please check imports", e);
            }

            return ResolveMethodInternalCheckOverloads(clazz, methodName, MethodModifiers.REQUIRE_STATIC_AND_PUBLIC);
        }

        public ExprNode ResolveSingleRowExtendedBuiltin(string name)
        {
            var nameLowerCase = name.ToLowerInvariant();
            if (nameLowerCase == "current_evaluation_context")
            {
                return new ExprCurrentEvaluationContextNode();
            }

            return null;
        }

        private void ValidateFunctionName(
            string functionType,
            string functionName)
        {
            var functionNameLower = functionName.ToLowerInvariant();
            if (aggregationFunctions.ContainsKey(functionNameLower))
            {
                throw new ImportException(
                    "Aggregation function by name '" + functionName + "' is already defined");
            }

            if (singleRowFunctions.ContainsKey(functionNameLower))
            {
                throw new ImportException(
                    "Single-row function by name '" + functionName + "' is already defined");
            }

            foreach (var pairs in aggregationAccess)
            {
                if (pairs.First.Contains(functionNameLower))
                {
                    throw new ImportException(
                        "Aggregation multi-function by name '" + functionName + "' is already defined");
                }
            }

            if (!IsFunctionName(functionName))
            {
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
            if (!allowExtendedAggregationFunc)
            {
                return null;
            }

            var nameLowerCase = name.ToLowerInvariant();
            switch (nameLowerCase)
            {
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
            if (cmsType != null)
            {
                return new ExprAggMultiFunctionCountMinSketchNode(isDistinct, cmsType.Value);
            }

            return null;
        }

        public bool IsSortUsingCollator()
        {
            return sortUsingCollator;
        }

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
            MethodInfo[] methods = clazz.GetMethods();
            ISet<MethodInfo> overloadeds = null;
            MethodInfo methodByName = null;

            // check each method by name, add to overloads when multiple methods for the same name
            foreach (var method in methods)
            {
                if (method.Name.Equals(methodName))
                {
                    bool isPublic = method.IsPublic;
                    bool isStatic = method.IsStatic;
                    if (methodModifiers.AcceptsPublicFlag(isPublic) && methodModifiers.AcceptsStaticFlag(isStatic))
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
                throw new ImportException(
                    "Could not find " + methodModifiers.Text + " method named '" + methodName + "' in class '" +
                    clazz.Name + "'");
            }

            if (overloadeds == null)
            {
                return methodByName;
            }

            // determine that all overloads have the same result type
            foreach (var overloaded in overloadeds)
            {
                if (!overloaded.ReturnType.Equals(methodByName.ReturnType))
                {
                    throw new ImportException(
                        "Method by name '" + methodName + "' is overloaded in class '" + clazz.Name +
                        "' and overloaded methods do not return the same type");
                }
            }

            return methodByName;
        }

        public AdvancedIndexFactoryProvider ResolveAdvancedIndexProvider(string indexTypeName)
        {
            var provider = advancedIndexProviders.Get(indexTypeName);
            if (provider == null)
            {
                throw new ImportException("Unrecognized advanced-type index '" + indexTypeName + "'");
            }

            return provider;
        }

        public AggregationFunctionForge ResolveAggregationFunction(string functionName)
        {
            var desc = DictionaryExtensions.Get(aggregationFunctions, functionName);
            if (desc == null)
            {
                desc = DictionaryExtensions.Get(aggregationFunctions, functionName.ToLowerInvariant());
            }

            if (desc == null || desc.ForgeClassName == null)
            {
                throw new ImportUndefinedException("A function named '" + functionName + "' is not defined");
            }

            var className = desc.ForgeClassName;
            Type clazz;
            try
            {
                clazz = ClassForNameProvider.ClassForName(className);
            }
            catch (TypeLoadException ex)
            {
                throw new ImportException(
                    "Could not load aggregation factory class by name '" + className + "'", ex);
            }

            object @object;
            try {
                @object = TypeHelper.Instantiate(clazz);
            }
            catch (TypeLoadException e)
            {
                throw new ImportException(
                    "Error instantiating aggregation factory class by name '" + className + "'", e);
            }
            catch (MemberAccessException e)
            {
                throw new ImportException(
                    "Illegal access instatiating aggregation factory class by name '" + className + "'", e);
            }

            if (!(@object is AggregationFunctionForge))
            {
                throw new ImportException(
                    "Class by name '" + className + "' does not implement the " +
                    typeof(AggregationFunctionForge).Name + " interface");
            }

            return (AggregationFunctionForge) @object;
        }

        public void AddAggregation(
            string functionName,
            ConfigurationCompilerPlugInAggregationFunction aggregationDesc)
        {
            ValidateFunctionName("aggregation function", functionName);
            if (aggregationDesc.ForgeClassName == null || !IsClassName(aggregationDesc.ForgeClassName))
            {
                throw new ImportException(
                    "Invalid class name for aggregation function forge '" + aggregationDesc.ForgeClassName + "'");
            }

            DictionaryExtensions.Put(aggregationFunctions, functionName.ToLowerInvariant(), aggregationDesc);
        }

        public ConfigurationCompilerPlugInAggregationMultiFunction ResolveAggregationMultiFunction(string name)
        {
            foreach (var config in aggregationAccess)
            {
                if (config.First.Contains(name.ToLowerInvariant()))
                {
                    return config.Second;
                }
            }

            return null;
        }

        public void AddAggregationMultiFunction(ConfigurationCompilerPlugInAggregationMultiFunction desc)
        {
            var orderedImmutableFunctionNames = new LinkedHashSet<string>();
            foreach (var functionName in desc.FunctionNames)
            {
                orderedImmutableFunctionNames.Add(functionName.ToLowerInvariant());
                ValidateFunctionName("aggregation multi-function", functionName.ToLowerInvariant());
            }

            if (!IsClassName(desc.MultiFunctionForgeClassName))
            {
                throw new ImportException(
                    "Invalid class name for aggregation multi-function factory '" + desc.MultiFunctionForgeClassName +
                    "'");
            }

            aggregationAccess.Add(
                new Pair<ISet<string>, ConfigurationCompilerPlugInAggregationMultiFunction>(
                    orderedImmutableFunctionNames, desc));
        }

        private class MethodModifiers
        {
            public static readonly MethodModifiers REQUIRE_STATIC_AND_PUBLIC =
                new MethodModifiers("public static", true);

            public static readonly MethodModifiers REQUIRE_NONSTATIC_AND_PUBLIC =
                new MethodModifiers("public non-static", false);

            private readonly bool requiredStaticFlag;

            private MethodModifiers(
                string text,
                bool requiredStaticFlag)
            {
                Text = text;
                this.requiredStaticFlag = requiredStaticFlag;
            }

            public string Text { get; }

            public bool AcceptsPublicFlag(bool isPublic)
            {
                return isPublic;
            }

            public bool AcceptsStaticFlag(bool isStatic)
            {
                return requiredStaticFlag == isStatic;
            }
        }
    }
} // end of namespace