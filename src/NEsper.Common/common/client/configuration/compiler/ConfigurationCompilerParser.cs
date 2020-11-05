///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Xml;

using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;

using static com.espertech.esper.common.@internal.util.DOMUtil;

namespace com.espertech.esper.common.client.configuration.compiler
{
    /// <summary>
    ///     Parser for the compiler section of configuration.
    /// </summary>
    public class ConfigurationCompilerParser
    {
        /// <summary>
        ///     Configure the compiler section from a provided element
        /// </summary>
        /// <param name="compiler">compiler section</param>
        /// <param name="compilerElement">element</param>
        public static void DoConfigure(
            ConfigurationCompiler compiler,
            XmlElement compilerElement)
        {
            var eventTypeNodeEnumerator = DOMElementEnumerator.Create(compilerElement.ChildNodes);
            while (eventTypeNodeEnumerator.MoveNext()) {
                XmlElement element = eventTypeNodeEnumerator.Current;
                var nodeName = element.Name;
                switch (nodeName) {
                    case "plugin-view":
                        HandlePlugInView(compiler, element);
                        break;

                    case "plugin-virtualdw":
                        HandlePlugInVirtualDW(compiler, element);
                        break;

                    case "plugin-aggregation-function":
                        HandlePlugInAggregation(compiler, element);
                        break;

                    case "plugin-aggregation-multifunction":
                        HandlePlugInMultiFunctionAggregation(compiler, element);
                        break;

                    case "plugin-singlerow-function":
                        HandlePlugInSingleRow(compiler, element);
                        break;

                    case "plugin-pattern-guard":
                        HandlePlugInPatternGuard(compiler, element);
                        break;

                    case "plugin-pattern-observer":
                        HandlePlugInPatternObserver(compiler, element);
                        break;
                    
                    case "plugin-method-datetime":
                        HandlePlugInDateTimeMethod(compiler, element);
                        break;

                    case "plugin-method-enum":
                        HandlePlugInEnumMethod(compiler, element);
                        break;

                    case "bytecode":
                        HandleByteCode(compiler, element);
                        break;

                    case "logging":
                        HandleLogging(compiler, element);
                        break;

                    case "stream-selection":
                        HandleStreamSelection(compiler, element);
                        break;

                    case "language":
                        HandleLanguage(compiler, element);
                        break;

                    case "scripts":
                        HandleScripts(compiler, element);
                        break;

                    case "expression":
                        HandleExpression(compiler, element);
                        break;

                    case "execution":
                        HandleExecution(compiler, element);
                        break;

                    case "view-resources":
                        HandleViewResources(compiler, element);
                        break;
                    
                    case "serde-settings":
                        HandleSerdeSettings(compiler, element);
                        break;
                }
            }
        }

        private static void HandleViewResources(
            ConfigurationCompiler compiler,
            XmlElement element)
        {
            var nodeEnumerator = DOMElementEnumerator.Create(element.ChildNodes);
            while (nodeEnumerator.MoveNext()) {
                XmlElement subElement = nodeEnumerator.Current;
                if (subElement.Name == "iterable-unbound") {
                    var valueText = DOMExtensions.GetRequiredAttribute(subElement, "enabled");
                    var value = bool.Parse(valueText);
                    compiler.ViewResources.IterableUnbound = value;
                }

                if (subElement.Name == "outputlimitopt") {
                    ParseRequiredBoolean(subElement, "enabled", b => compiler.ViewResources.OutputLimitOpt = b);
                }
            }
        }

        private static void HandleExecution(
            ConfigurationCompiler compiler,
            XmlElement element)
        {
            var filterServiceMaxFilterWidthStr =
                DOMExtensions.GetOptionalAttribute(element, "filter-service-max-filter-width");
            if (filterServiceMaxFilterWidthStr != null) {
                compiler.Execution.FilterServiceMaxFilterWidth = int.Parse(filterServiceMaxFilterWidthStr);
            }

            ParseOptionalBoolean(
                element,
                "enable-declared-expr-value-cache",
                b => compiler.Execution.EnabledDeclaredExprValueCache = b);
                
            var filterIndexPlanningStr = DOMExtensions.GetOptionalAttribute(element, "filter-index-planning");
            if (filterIndexPlanningStr != null) {
                compiler.Execution.FilterIndexPlanning = EnumHelper.Parse<ConfigurationCompilerExecution.FilterIndexPlanningEnum>(filterIndexPlanningStr);
            }
        }

        private static void HandleExpression(
            ConfigurationCompiler compiler,
            XmlElement element)
        {
            var integerDivision = DOMExtensions.GetOptionalAttribute(element, "integer-division");
            if (integerDivision != null) {
                var isIntegerDivision = bool.Parse(integerDivision);
                compiler.Expression.IntegerDivision = isIntegerDivision;
            }

            var divZero = DOMExtensions.GetOptionalAttribute(element, "division-by-zero-is-null");
            if (divZero != null) {
                var isDivZero = bool.Parse(divZero);
                compiler.Expression.DivisionByZeroReturnsNull = isDivZero;
            }

            var udfCache = DOMExtensions.GetOptionalAttribute(element, "udf-cache");
            if (udfCache != null) {
                var isUdfCache = bool.Parse(udfCache);
                compiler.Expression.UdfCache = isUdfCache;
            }

            var extendedAggregationStr = DOMExtensions.GetOptionalAttribute(element, "extended-agg");
            if (extendedAggregationStr != null) {
                var extendedAggregation = bool.Parse(extendedAggregationStr);
                compiler.Expression.ExtendedAggregation = extendedAggregation;
            }

            var duckTypingStr = DOMExtensions.GetOptionalAttribute(element, "ducktyping");
            if (duckTypingStr != null) {
                compiler.Expression.DuckTyping = bool.Parse(duckTypingStr);
            }

            var mathContextStr = DOMExtensions.GetOptionalAttribute(element, "math-context");
            if (mathContextStr != null) {
                try {
                    compiler.Expression.MathContext = new MathContext(mathContextStr);
                }
                catch (ArgumentException) {
                    throw new ConfigurationException("Failed to parse '" + mathContextStr + "' as a MathContext");
                }
            }
        }

        private static void HandleScripts(
            ConfigurationCompiler compiler,
            XmlElement element)
        {
            var defaultDialect = DOMExtensions.GetOptionalAttribute(element, "default-dialect");
            if (defaultDialect != null) {
                compiler.Scripts.DefaultDialect = defaultDialect;
            }

            ParseOptionalBoolean(element, "enabled", b => compiler.Scripts.IsEnabled = b);

        }

        private static void HandleLanguage(
            ConfigurationCompiler compiler,
            XmlElement element)
        {
            ParseOptionalBoolean(element, "sort-using-collator", b => compiler.Language.SortUsingCollator = b);
        }

        private static void HandleStreamSelection(
            ConfigurationCompiler compiler,
            XmlElement element)
        {
            var nodeEnumerator = DOMElementEnumerator.Create(element.ChildNodes);
            while (nodeEnumerator.MoveNext()) {
                XmlElement subElement = nodeEnumerator.Current;
                if (subElement.Name == "stream-selector") {
                    var valueText = DOMExtensions.GetRequiredAttribute(subElement, "value");
                    if (valueText == null) {
                        throw new ConfigurationException("No value attribute supplied for stream-selector element");
                    }

                    StreamSelector defaultSelector;
                    if (valueText.ToUpperInvariant().Trim() == "ISTREAM") {
                        defaultSelector = StreamSelector.ISTREAM_ONLY;
                    }
                    else if (valueText.ToUpperInvariant().Trim() == "RSTREAM") {
                        defaultSelector = StreamSelector.RSTREAM_ONLY;
                    }
                    else if (valueText.ToUpperInvariant().Trim() == "IRSTREAM") {
                        defaultSelector = StreamSelector.RSTREAM_ISTREAM_BOTH;
                    }
                    else {
                        throw new ConfigurationException(
                            "Value attribute for stream-selector element invalid, " +
                            "expected one of the following keywords: istream, irstream, rstream");
                    }

                    compiler.StreamSelection.DefaultStreamSelector = defaultSelector;
                }
            }
        }

        private static void HandleLogging(
            ConfigurationCompiler compiler,
            XmlElement element)
        {
            var nodeEnumerator = DOMElementEnumerator.Create(element.ChildNodes);
            while (nodeEnumerator.MoveNext()) {
                XmlElement subElement = nodeEnumerator.Current;
                switch (subElement.Name) {
                    case "code":
                        ParseRequiredBoolean(subElement, "enabled", b => compiler.Logging.EnableCode = b);
                        break;

                    case "audit-directory":
                        compiler.Logging.AuditDirectory = DOMExtensions.GetRequiredAttribute(subElement, "value");
                        break;

                    case "filter-plan":
                        ParseRequiredBoolean(subElement, "enabled", b => compiler.Logging.IsEnableFilterPlan = b);
                        break;
                }
            }
        }

        private static void HandleByteCode(
            ConfigurationCompiler compiler,
            XmlElement element)
        {
            var codegen = compiler.ByteCode;
            ParseOptionalBoolean(element, "include-debugsymbols", v => codegen.IncludeDebugSymbols = v);
            ParseOptionalBoolean(element, "include-comments", v => codegen.IncludeComments = v);
            ParseOptionalBoolean(element, "attach-epl", v => codegen.AttachEPL = v);
            ParseOptionalBoolean(element, "attach-module-epl", v => codegen.AttachModuleEPL = v);
            ParseOptionalBoolean(element, "attach-pattern-epl", v => codegen.AttachPatternEPL = v);
            ParseOptionalBoolean(element, "instrumented", v => codegen.Instrumented = v);
            ParseOptionalBoolean(element, "allow-subscriber", v => codegen.AllowSubscriber = v);
            ParseOptionalInteger(element, "threadpool-compiler-num-threads", v => codegen.ThreadPoolCompilerNumThreads = v);
            ParseOptionalInteger(element, "threadpool-compiler-capacity", v => codegen.ThreadPoolCompilerCapacity = v);
            ParseOptionalInteger(element, "max-methods-per-class", v => codegen.MaxMethodsPerClass = v);
            ParseOptionalBoolean(element, "allow-inlined-class", v => codegen.IsAllowInlinedClass = v);

            ParseOptionalAccessMod(element, "access-modifier-context", v => codegen.AccessModifierContext = v);
            ParseOptionalAccessMod(element, "access-modifier-event-type", v => codegen.AccessModifierEventType = v);
            ParseOptionalAccessMod(element, "access-modifier-expression", v => codegen.AccessModifierExpression = v);
            ParseOptionalAccessMod(element, "access-modifier-named-window", v => codegen.AccessModifierNamedWindow = v);
            ParseOptionalAccessMod(element, "access-modifier-script", v => codegen.AccessModifierScript = v);
            ParseOptionalAccessMod(element, "access-modifier-table", v => codegen.AccessModifierTable = v);
            ParseOptionalAccessMod(element, "access-modifier-variable", v => codegen.AccessModifierVariable = v);

            var busModifierEventType = DOMExtensions.GetOptionalAttribute(element, "bus-modifier-event-type");
            if (busModifierEventType != null) {
                try {
                    codegen.BusModifierEventType = EnumHelper.Parse<EventTypeBusModifier>(busModifierEventType.Trim());
                }
                catch (Exception t) {
                    throw new ConfigurationException(t.Message, t);
                }
            }
        }

        private static void ParseOptionalAccessMod(
            XmlElement element,
            string name,
            Consumer<NameAccessModifier> accessModifier)
        {
            var value = DOMExtensions.GetOptionalAttribute(element, name);
            if (value != null) {
                try {
                    accessModifier.Invoke(EnumHelper.Parse<NameAccessModifier>(value.Trim()));
                }
                catch (Exception t) {
                    throw new ConfigurationException(t.Message, t);
                }
            }
        }

        private static void HandlePlugInView(
            ConfigurationCompiler configuration,
            XmlElement element)
        {
            var @namespace = DOMExtensions.GetRequiredAttribute(element, "namespace");
            var name = DOMExtensions.GetRequiredAttribute(element, "name");
            var forgeClassName = DOMExtensions.GetRequiredAttribute(element, "forge-class");
            configuration.AddPlugInView(@namespace, name, forgeClassName);
        }

        private static void HandlePlugInVirtualDW(
            ConfigurationCompiler configuration,
            XmlElement element)
        {
            var @namespace = DOMExtensions.GetRequiredAttribute(element, "namespace");
            var name = DOMExtensions.GetRequiredAttribute(element, "name");
            var forgeClassName = DOMExtensions.GetRequiredAttribute(element, "forge-class");
            var config = DOMExtensions.GetOptionalAttribute(element, "config");
            configuration.AddPlugInVirtualDataWindow(@namespace, name, forgeClassName, config);
        }

        private static void HandlePlugInAggregation(
            ConfigurationCompiler configuration,
            XmlElement element)
        {
            var name = DOMExtensions.GetRequiredAttribute(element, "name");
            var forgeClassName = DOMExtensions.GetRequiredAttribute(element, "forge-class");
            configuration.AddPlugInAggregationFunctionForge(name, forgeClassName);
        }
        
        
        private static void HandlePlugInDateTimeMethod(
            ConfigurationCompiler configuration,
            XmlElement element)
        {
            String methodName = DOMExtensions.GetRequiredAttribute(element, "method-name");
            String forgeClassName = DOMExtensions.GetRequiredAttribute(element, "forge-class");
            configuration.AddPlugInDateTimeMethod(methodName, forgeClassName);
        }

        private static void HandlePlugInEnumMethod(
            ConfigurationCompiler configuration,
            XmlElement element)
        {
            String methodName = DOMExtensions.GetRequiredAttribute(element, "method-name");
            String forgeClassName = DOMExtensions.GetRequiredAttribute(element, "forge-class");
            configuration.AddPlugInEnumMethod(methodName, forgeClassName);
        }

        private static void HandlePlugInMultiFunctionAggregation(
            ConfigurationCompiler configuration,
            XmlElement element)
        {
            var functionNames = DOMExtensions.GetRequiredAttribute(element, "function-names");
            var forgeClassName = DOMExtensions.GetOptionalAttribute(element, "forge-class");

            var nodeEnumerator = DOMElementEnumerator.Create(element.ChildNodes);
            IDictionary<string, object> additionalProps = null;
            while (nodeEnumerator.MoveNext()) {
                XmlElement subElement = nodeEnumerator.Current;
                if (subElement.Name == "init-arg") {
                    var name = DOMExtensions.GetRequiredAttribute(subElement, "name");
                    var value = DOMExtensions.GetRequiredAttribute(subElement, "value");
                    if (additionalProps == null) {
                        additionalProps = new Dictionary<string, object>();
                    }

                    additionalProps.Put(name, value);
                }
            }

            var config = new ConfigurationCompilerPlugInAggregationMultiFunction(
                functionNames.SplitCsv(),
                forgeClassName);
            config.AdditionalConfiguredProperties = additionalProps;
            configuration.AddPlugInAggregationMultiFunction(config);
        }

        private static void HandlePlugInSingleRow(
            ConfigurationCompiler configuration,
            XmlElement element)
        {
            var name = element.Attributes.GetNamedItem("name").InnerText;
            var functionClassName = element.Attributes.GetNamedItem("function-class").InnerText;
            var functionMethodName = element.Attributes.GetNamedItem("function-method").InnerText;
            var valueCache = ConfigurationCompilerPlugInSingleRowFunction.ValueCacheEnum.DISABLED;
            var filterOptimizable = ConfigurationCompilerPlugInSingleRowFunction.FilterOptimizableEnum.ENABLED;
            var valueCacheStr = DOMExtensions.GetOptionalAttribute(element, "value-cache");
            if (valueCacheStr != null) {
                valueCache =
                    EnumHelper.Parse<ConfigurationCompilerPlugInSingleRowFunction.ValueCacheEnum>(valueCacheStr);
            }

            var filterOptimizableStr = DOMExtensions.GetOptionalAttribute(element, "filter-optimizable");
            if (filterOptimizableStr != null) {
                filterOptimizable =
                    EnumHelper.Parse<ConfigurationCompilerPlugInSingleRowFunction.FilterOptimizableEnum>(
                        filterOptimizableStr);
            }

            var rethrowExceptionsStr = DOMExtensions.GetOptionalAttribute(element, "rethrow-exceptions");
            var rethrowExceptions = false;
            if (rethrowExceptionsStr != null) {
                rethrowExceptions = bool.Parse(rethrowExceptionsStr);
            }

            var eventTypeName = DOMExtensions.GetOptionalAttribute(element, "event-type-name");
            configuration.AddPlugInSingleRowFunction(
                new ConfigurationCompilerPlugInSingleRowFunction(
                    name,
                    functionClassName,
                    functionMethodName,
                    valueCache,
                    filterOptimizable,
                    rethrowExceptions,
                    eventTypeName));
        }

        private static void HandlePlugInPatternGuard(
            ConfigurationCompiler configuration,
            XmlElement element)
        {
            var @namespace = DOMExtensions.GetRequiredAttribute(element, "namespace");
            var name = DOMExtensions.GetRequiredAttribute(element, "name");
            var forgeClassName = DOMExtensions.GetRequiredAttribute(element, "forge-class");
            configuration.AddPlugInPatternGuard(@namespace, name, forgeClassName);
        }

        private static void HandlePlugInPatternObserver(
            ConfigurationCompiler configuration,
            XmlElement element)
        {
            var @namespace = DOMExtensions.GetRequiredAttribute(element, "namespace");
            var name = DOMExtensions.GetRequiredAttribute(element, "name");
            var forgeClassName = DOMExtensions.GetRequiredAttribute(element, "forge-class");
            configuration.AddPlugInPatternObserver(@namespace, name, forgeClassName);
        }
        
        private static void HandleSerdeSettings(
            ConfigurationCompiler configuration,
            XmlElement parentElement)
        {
            String text = DOMExtensions.GetOptionalAttribute(parentElement, "enable-serializable");
            if (text != null) {
                configuration.Serde.IsEnableSerializable = bool.Parse(text);
            }

            text = DOMExtensions.GetOptionalAttribute(parentElement, "enable-externalizable");
            if (text != null) {
                configuration.Serde.IsEnableExternalizable = bool.Parse(text);
            }

            text = DOMExtensions.GetOptionalAttribute(parentElement, "enable-extended-builtin");
            if (text != null) {
                configuration.Serde.IsEnableExtendedBuiltin = bool.Parse(text);
            }

            text = DOMExtensions.GetOptionalAttribute(parentElement, "enable-serialization-fallback");
            if (text != null) {
                configuration.Serde.IsEnableSerializationFallback = bool.Parse(text);
            }

            var nodeEnumerator = DOMElementEnumerator.Create(parentElement.ChildNodes);
            while (nodeEnumerator.MoveNext()) {
                var subElement = nodeEnumerator.Current;
                if (subElement.Name == "serde-provider-factory") {
                    text = DOMExtensions.GetRequiredAttribute(subElement, "class");
                    configuration.Serde.AddSerdeProviderFactory(text);
                }
            }
        }
    }
} // end of namespace