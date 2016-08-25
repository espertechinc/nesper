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
using System.Reflection;
using System.Text.RegularExpressions;

using com.espertech.esper.client;
using com.espertech.esper.client.annotation;
using com.espertech.esper.client.hook;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.approx;
using com.espertech.esper.epl.expression.accessagg;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.methodagg;
using com.espertech.esper.type;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.core
{
    /// <summary>
    /// Implementation for engine-level imports.
    /// </summary>
    public class EngineImportServiceImpl : EngineImportService
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IList<AutoImportDesc> _imports;
        private readonly IList<AutoImportDesc> _annotationImports;
        private readonly IDictionary<String, ConfigurationPlugInAggregationFunction> _aggregationFunctions;
        private readonly IList<Pair<ISet<String>, ConfigurationPlugInAggregationMultiFunction>> _aggregationAccess;
        private readonly IDictionary<String, EngineImportSingleRowDesc> _singleRowFunctions;
        private readonly IDictionary<String, ConfigurationMethodRef> _methodInvocationRef;
        private readonly bool _allowExtendedAggregationFunc;
        private readonly bool _sortUsingCollator;
        private readonly MathContext _optionalDefaultMathContext;
        private readonly TimeZoneInfo _timeZone;
        private readonly ConfigurationEngineDefaults.ThreadingProfile _threadingProfile;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="allowExtendedAggregationFunc">true to allow non-SQL standard builtin agg functions.</param>
        /// <param name="isUdfCache">if set to <c>true</c> [is udf cache].</param>
        /// <param name="isDuckType">if set to <c>true</c> [is duck type].</param>
        /// <param name="sortUsingCollator">if set to <c>true</c> [sort using collator].</param>
        /// <param name="optionalDefaultMathContext">The optional default math context.</param>
        public EngineImportServiceImpl(
            bool allowExtendedAggregationFunc,
            bool isUdfCache,
            bool isDuckType,
            bool sortUsingCollator,
            MathContext optionalDefaultMathContext,
            TimeZoneInfo timeZone,
            ConfigurationEngineDefaults.ThreadingProfile threadingProfile)
        {
            _imports = new List<AutoImportDesc>();
            _annotationImports = new List<AutoImportDesc>();
            _aggregationFunctions = new Dictionary<String, ConfigurationPlugInAggregationFunction>();
            _aggregationAccess = new List<Pair<ISet<String>, ConfigurationPlugInAggregationMultiFunction>>();
            _singleRowFunctions = new Dictionary<String, EngineImportSingleRowDesc>();
            _methodInvocationRef = new Dictionary<String, ConfigurationMethodRef>();
            _allowExtendedAggregationFunc = allowExtendedAggregationFunc;
            IsUdfCache = isUdfCache;
            IsDuckType = isDuckType;
            _sortUsingCollator = sortUsingCollator;
            _optionalDefaultMathContext = optionalDefaultMathContext;
            _timeZone = timeZone;
            _threadingProfile = threadingProfile;
        }

        public bool IsUdfCache { get; private set; }

        public bool IsDuckType { get; private set; }

        public ConfigurationMethodRef GetConfigurationMethodRef(String className)
        {
            return _methodInvocationRef.Get(className);
        }

        /// <summary>
        /// Adds cache configs for method invocations for from-clause.
        /// </summary>
        /// <param name="configs">cache configs</param>
        public void AddMethodRefs(IDictionary<String, ConfigurationMethodRef> configs)
        {
            _methodInvocationRef.PutAll(configs);
        }

        public void AddImport(String importName)
        {
            AddImport(new AutoImportDesc(importName, null));
        }

        public void AddImport(AutoImportDesc autoImportDesc)
        {
            ValidateImportAndAdd(autoImportDesc, _imports);
        }

        public void AddAnnotationImport(String importName)
        {
            ValidateImportAndAdd(new AutoImportDesc(importName, null), _annotationImports);
        }

        public void AddAnnotationImport(AutoImportDesc autoImportDesc)
        {
            ValidateImportAndAdd(autoImportDesc, _annotationImports);
        }

        public void AddAggregation(String functionName, ConfigurationPlugInAggregationFunction aggregationDesc)
        {
            ValidateFunctionName("aggregation function", functionName);
            if (aggregationDesc.FactoryClassName == null || !IsTypeNameOrNamespace(aggregationDesc.FactoryClassName))
            {
                throw new EngineImportException(
                    "Invalid class name for aggregation factory '" + aggregationDesc.FactoryClassName + "'");
            }
            _aggregationFunctions.Put(functionName.ToLower(), aggregationDesc);
        }

        public void AddSingleRow(
            String functionName,
            String singleRowFuncClass,
            String methodName,
            ValueCache valueCache,
            FilterOptimizable filterOptimizable,
            bool rethrowExceptions)
        {
            ValidateFunctionName("single-row", functionName);

            if (!IsTypeNameOrNamespace(singleRowFuncClass))
            {
                throw new EngineImportException("Invalid class name for aggregation '" + singleRowFuncClass + "'");
            }
            _singleRowFunctions.Put(
                functionName.ToLower(),
                new EngineImportSingleRowDesc(
                    singleRowFuncClass, methodName, valueCache, filterOptimizable, rethrowExceptions));
        }

        public AggregationFunctionFactory ResolveAggregationFactory(String name)
        {
            var desc = _aggregationFunctions.Get(name);
            if (desc == null)
            {
                desc = _aggregationFunctions.Get(name.ToLower());
            }
            if (desc == null || desc.FactoryClassName == null)
            {
                throw new EngineImportUndefinedException("A function named '" + name + "' is not defined");
            }

            var className = desc.FactoryClassName;
            Type clazz;
            try
            {
                clazz = TypeHelper.ResolveType(className);
            }
            catch (TypeLoadException ex)
            {
                throw new EngineImportException(
                    "Could not load aggregation factory class by name '" + className + "'", ex);
            }

            Object @object;
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
                throw new EngineImportException("Error instantiating aggregation class - Default constructor was not found", e);
            }
            catch (MethodAccessException e)
            {
                throw new EngineImportException("Error instantiating aggregation class - Caller does not have permission to use constructor", e);
            }
            catch (ArgumentException e)
            {
                throw new EngineImportException("Error instantiating aggregation class - Type is not a RuntimeType", e);
            }


            if (!(@object is AggregationFunctionFactory))
            {
                throw new EngineImportException(
                    "Aggregation class by name '" + className + "' does not implement AggregationFunctionFactory");
            }
            return (AggregationFunctionFactory)@object;
        }

        public void AddAggregationMultiFunction(ConfigurationPlugInAggregationMultiFunction desc)
        {
            var orderedImmutableFunctionNames = new LinkedHashSet<String>();
            foreach (var functionName in desc.FunctionNames)
            {
                orderedImmutableFunctionNames.Add(functionName.ToLower());
                ValidateFunctionName("aggregation multi-function", functionName.ToLower());
            }
            if (!IsTypeNameOrNamespace(desc.MultiFunctionFactoryClassName))
            {
                throw new EngineImportException(
                    "Invalid class name for aggregation multi-function factory '" + desc.MultiFunctionFactoryClassName +
                    "'");
            }
            _aggregationAccess.Add(
                new Pair<ISet<String>, ConfigurationPlugInAggregationMultiFunction>(orderedImmutableFunctionNames, desc));
        }

        public ConfigurationPlugInAggregationMultiFunction ResolveAggregationMultiFunction(String name)
        {
            name = name.ToLower();

            return _aggregationAccess
                .Where(e => e.First.Contains(name))
                .Select(config => config.Second)
                .FirstOrDefault();
        }

        public Pair<Type, EngineImportSingleRowDesc> ResolveSingleRow(String name)
        {
            var pair = _singleRowFunctions.Get(name);
            if (pair == null)
            {
                pair = _singleRowFunctions.Get(name.ToLower());
            }
            if (pair == null)
            {
                throw new EngineImportUndefinedException("A function named '" + name + "' is not defined");
            }

            Type clazz;
            try
            {
                clazz = TypeHelper.ResolveType(pair.ClassName);
            }
            catch (TypeLoadException ex)
            {
                throw new EngineImportException(
                    "Could not load single-row function class by name '" + pair.ClassName + "'", ex);
            }
            return new Pair<Type, EngineImportSingleRowDesc>(clazz, pair);
        }

        public MethodInfo ResolveMethod(
            String typeName,
            String methodName,
            Type[] paramTypes,
            bool[] allowEventBeanType,
            bool[] allowEventBeanCollType)
        {
            Type clazz;
            try
            {
                clazz = ResolveTypeInternal(typeName, false, false);
            }
            catch (TypeLoadException e)
            {
                throw new EngineImportException(
                    "Could not load class by name '" + typeName + "', please check imports", e);
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

        public MethodInfo ResolveMethod(String typeName, String methodName)
        {
            Type clazz;
            try
            {
                clazz = ResolveTypeInternal(typeName, false, false);
            }
            catch (TypeLoadException e)
            {
                throw new EngineImportException(
                    "Could not load class by name '" + typeName + "', please check imports", e);
            }

            return ResolveMethodInternal(clazz, methodName, MethodModifiers.REQUIRE_STATIC_AND_PUBLIC);
        }

        public MethodInfo ResolveNonStaticMethod(Type clazz, String methodName)
        {
            return ResolveMethodInternal(clazz, methodName, MethodModifiers.REQUIRE_NONSTATIC_AND_PUBLIC);
        }

        public Type ResolveType(String typeName, bool forAnnotation)
        {
            Type clazz;
            try
            {
                clazz = ResolveTypeInternal(typeName, false, forAnnotation);
            }
            catch (TypeLoadException e)
            {
                throw new EngineImportException(
                    "Could not load class by name '" + typeName + "', please check imports", e);
            }

            return clazz;
        }

        public Type ResolveAnnotation(String typeName)
        {
            Type clazz;
            try
            {
                clazz = ResolveTypeInternal(typeName, true, true);
            }
            catch (TypeLoadException e)
            {
                throw new EngineImportException(
                    "Could not load annotation class by name '" + typeName + "', please check imports", e);
            }

            return clazz;
        }

        /// <summary>
        /// Finds a class by class name using the auto-import information provided.
        /// </summary>
        /// <param name="typeName">is the class name to find</param>
        /// <param name="requireAnnotation">if set to <c>true</c> [require annotation].</param>
        /// <returns>class</returns>
        /// <throws>ClassNotFoundException if the class cannot be loaded</throws>
        internal Type ResolveTypeInternal(String typeName, bool requireAnnotation, bool forAnnotationUse)
        {
            // Attempt to retrieve the class with the name as-is
            try
            {
                return TypeHelper.ResolveType(typeName);
            }
            catch (TypeLoadException)
            {
                if (Log.IsDebugEnabled)
                {
                    Log.Debug("Class not found for resolving from name as-is '" + typeName + "'");
                }
            }

            Type clazz;

            // check annotation-specific imports first
            if (forAnnotationUse)
            {
                clazz = CheckImports(_annotationImports, requireAnnotation, typeName);
                if (clazz != null)
                {
                    return clazz;
                }
            }

            // check all imports
            clazz = CheckImports(_imports, requireAnnotation, typeName);
            if (clazz != null)
            {
                return clazz;
            }

            if (!forAnnotationUse) {
                // try to resolve from method references
                foreach (String name in _methodInvocationRef.Keys)
                {
                    if (TypeHelper.IsSimpleNameFullyQualfied(typeName, name))
                    {
                        try
                        {
                            var found = TypeHelper.ResolveType(name);
                            if (!requireAnnotation || found.IsAttribute()) {
                                return found;
                            }
                        }
                        catch (TypeLoadException e1)
                        {
                            if (Log.IsDebugEnabled)
                            {
                                Log.Debug("Class not found for resolving from method invocation ref:{0}", name);
                            }
                        }
                    }
                }
            }

            // No import worked, the class isn't resolved
            throw new TypeLoadException("Unknown class " + typeName);
        }

        public MethodInfo ResolveMethod(
            Type clazz,
            String methodName,
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
                // Lets go looking for an extension method before throwing an exception
                var method = MethodResolver.ResolveExtensionMethod(
                    clazz, methodName, paramTypes, true, allowEventBeanType, allowEventBeanType);
                if (method == null)
                {
                    throw Convert(clazz, methodName, paramTypes, e, true);
                }

                return method;
            }
        }

        private EngineImportException Convert(
            Type clazz,
            String methodName,
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
                message += "method named '" + methodName + "' in class '" +
                           clazz.GetTypeNameFullyQualPretty() +
                           "' with matching parameter number and expected parameter type(s) '" + expected + "'";
            }
            else
            {
                message += "method named '" + methodName + "' in class '" +
                           clazz.GetTypeNameFullyQualPretty() + "' taking no parameters";
            }

            if (e.NearestMissMethod != null)
            {
                message += " (nearest match found was '" + e.NearestMissMethod.Name;
                var parameterTypes = e.NearestMissMethod.GetParameterTypes();
                if (parameterTypes.Length == 0)
                {
                    message += "' taking no parameters";
                }
                else
                {
                    message += "' taking type(s) '" +
                               TypeHelper.GetParameterAsString(parameterTypes) + "'";
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
                           "' with matching parameter number and expected parameter type(s) '" + expected + "'";
            }
            else
            {
                message += "in class '" + clazz.GetTypeNameFullyQualPretty() + "' taking no parameters";
            }

            if (e.NearestMissCtor != null)
            {
                message += " (nearest matching constructor ";
                var parameterTypes = e.NearestMissCtor.GetParameterTypes();
                if (parameterTypes.Length == 0)
                {
                    message += "taking no parameters";
                }
                else
                {
                    message += "taking type(s) '" + TypeHelper.GetParameterAsString(parameterTypes) +
                               "'";
                }
                message += ")";
            }
            return new EngineImportException(message, e);
        }

        public ExprNode ResolveSingleRowExtendedBuiltin(String name)
        {
            var nameLowerCase = name.ToLower();
            if (nameLowerCase == "current_evaluation_context")
            {
                return new ExprCurrentEvaluationContextNode();
            }
            return null;
        }

        public ExprNode ResolveAggExtendedBuiltin(String name, bool isDistinct)
        {
            if (!_allowExtendedAggregationFunc)
            {
                return null;
            }

            String nameLowerCase = name.ToLower();
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

        public ConfigurationEngineDefaults.ThreadingProfile ThreadingProfile
        {
            get { return _threadingProfile; }
        }

        public bool IsSortUsingCollator
        {
            get { return _sortUsingCollator; }
        }

        /// <summary>
        /// For testing, returns imports.
        /// </summary>
        /// <value>returns auto-import list as array</value>
        internal AutoImportDesc[] Imports
        {
            get { return _imports.ToArray(); }
        }

        private void ValidateFunctionName(String functionType, String functionName)
        {
            var functionNameLower = functionName.ToLower();
            if (_aggregationFunctions.ContainsKey(functionNameLower))
            {
                throw new EngineImportException(
                    "Aggregation function by name '" + functionName + "' is already defined");
            }
            if (_singleRowFunctions.ContainsKey(functionNameLower))
            {
                throw new EngineImportException("Single-row function by name '" + functionName + "' is already defined");
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

        private static readonly Regex FunctionRegEx1 = new Regex(@"^\w+$", RegexOptions.None);

        private static bool IsFunctionName(String functionName)
        {
            return FunctionRegEx1.IsMatch(functionName);
        }

        private static readonly Regex TypeNameRegEx1 = new Regex(@"^(\w+\.)*\w+$", RegexOptions.None);
        private static readonly Regex TypeNameRegEx2 = new Regex(@"^(\w+\.)*\w+\+(\w+|)$", RegexOptions.None);

        private static bool IsTypeNameOrNamespace(String importName)
        {
            if (TypeNameRegEx1.IsMatch(importName) || TypeNameRegEx2.IsMatch(importName))
                return true;

            return TypeHelper.ResolveType(importName, false) != null;
        }

        public bool IsStrictTypeMatch(AutoImportDesc importDesc, String typeName)
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

            if ((importName[lastIndex - 1] == '.') ||
                (importName[lastIndex - 1] == '+'))
            {
                return true;
            }

            return false;
        }

        private MethodInfo ResolveMethodInternal(Type clazz, String methodName, MethodModifiers methodModifiers)
        {
            MethodInfo[] methods = clazz.GetMethods();
            MethodInfo methodByName = null;

            // check each method by name
            foreach (MethodInfo method in methods.Where(m => m.Name == methodName))
            {
                if (methodByName != null)
                {
                    throw new EngineImportException(string.Format("Ambiguous method name: method by name '{0}' is overloaded in class '{1}'", methodName, clazz.FullName));
                }

                var isPublic = method.IsPublic;
                var isStatic = method.IsStatic;
                if (methodModifiers.AcceptsPublicFlag(isPublic) && methodModifiers.AcceptsStaticFlag(isStatic))
                {
                    methodByName = method;
                }
            }

            if (methodByName == null)
            {
                throw new EngineImportException("Could not find " + methodModifiers.Text + " method named '" + methodName + "' in class '" + clazz.FullName + "'");
            }
            return methodByName;
        }

        private Type CheckImports(IList<AutoImportDesc> imports, bool requireAnnotation, String typeName)
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

                    Log.Warn("Type not found for resolving from name as-is: '{0}'", typeName);
                }
                else
                {
                    if (requireAnnotation && (importName == Configuration.ANNOTATION_IMPORT))
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
                            var type = TypeHelper.ResolveType(importDesc.TypeOrNamespace, importDesc.AssemblyNameOrFile);
                            if (type != null)
                            {
                                return type;
                            }

                            Log.Warn("Type not found for resolving from name as-is: {0}", typeName);
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

                        Log.Warn("Type not found for resolving from name as-is: {0}", typeName);
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

                        Log.Warn("Type not found for resolving from name as-is: {0}", typeName);
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
}