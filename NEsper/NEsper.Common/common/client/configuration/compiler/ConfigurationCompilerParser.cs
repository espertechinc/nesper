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
using static com.espertech.esper.common.@internal.util.DOMExtensions;

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
        public static void DoConfigure(ConfigurationCompiler compiler, XmlElement compilerElement)
        {
            var eventTypeNodeEnumerator = DOMElementEnumerator.Create(compilerElement.ChildNodes);
            while (eventTypeNodeEnumerator.MoveNext()) {
                XmlElement element = eventTypeNodeEnumerator.Current;
                var nodeName = element.Name;
                if (nodeName == "plugin-view") {
                    HandlePlugInView(compiler, element);
                }
                else if (nodeName == "plugin-virtualdw") {
                    HandlePlugInVirtualDW(compiler, element);
                }
                else if (nodeName == "plugin-aggregation-function") {
                    HandlePlugInAggregation(compiler, element);
                }
                else if (nodeName == "plugin-aggregation-multifunction") {
                    HandlePlugInMultiFunctionAggregation(compiler, element);
                }
                else if (nodeName == "plugin-singlerow-function") {
                    HandlePlugInSingleRow(compiler, element);
                }
                else if (nodeName == "plugin-pattern-guard") {
                    HandlePlugInPatternGuard(compiler, element);
                }
                else if (nodeName == "plugin-pattern-observer") {
                    HandlePlugInPatternObserver(compiler, element);
                }
                else if (nodeName == "bytecode") {
                    HandleByteCode(compiler, element);
                }
                else if (nodeName == "logging") {
                    HandleLogging(compiler, element);
                }
                else if (nodeName == "stream-selection") {
                    HandleStreamSelection(compiler, element);
                }
                else if (nodeName == "language") {
                    HandleLanguage(compiler, element);
                }
                else if (nodeName == "scripts") {
                    HandleScripts(compiler, element);
                }
                else if (nodeName == "expression") {
                    HandleExpression(compiler, element);
                }
                else if (nodeName == "execution") {
                    HandleExecution(compiler, element);
                }
                else if (nodeName == "view-resources") {
                    HandleViewResources(compiler, element);
                }
            }
        }

        private static void HandleViewResources(ConfigurationCompiler compiler, XmlElement element)
        {
            var nodeEnumerator = DOMElementEnumerator.Create(element.ChildNodes);
            while (nodeEnumerator.MoveNext()) {
                XmlElement subElement = nodeEnumerator.Current;
                if (subElement.Name == "iterable-unbound") {
                    var valueText = GetRequiredAttribute(subElement, "enabled");
                    var value = bool.Parse(valueText);
                    compiler.ViewResources.IterableUnbound = value;
                }

                if (subElement.Name == "outputlimitopt") {
                    ParseRequiredBoolean(subElement, "enabled", b => compiler.ViewResources.OutputLimitOpt = b);
                }
            }
        }

        private static void HandleExecution(ConfigurationCompiler compiler, XmlElement element)
        {
            var filterServiceMaxFilterWidthStr = GetOptionalAttribute(element, "filter-service-max-filter-width");
            if (filterServiceMaxFilterWidthStr != null) {
                compiler.Execution.FilterServiceMaxFilterWidth = int.Parse(filterServiceMaxFilterWidthStr);
            }

            ParseOptionalBoolean(
                element, "enable-declared-expr-value-cache", b => compiler.Execution.EnabledDeclaredExprValueCache = b);
        }

        private static void HandleExpression(ConfigurationCompiler compiler, XmlElement element)
        {
            var integerDivision = GetOptionalAttribute(element, "integer-division");
            if (integerDivision != null) {
                var isIntegerDivision = bool.Parse(integerDivision);
                compiler.Expression.IntegerDivision = isIntegerDivision;
            }

            var divZero = GetOptionalAttribute(element, "division-by-zero-is-null");
            if (divZero != null) {
                var isDivZero = bool.Parse(divZero);
                compiler.Expression.DivisionByZeroReturnsNull = isDivZero;
            }

            var udfCache = GetOptionalAttribute(element, "udf-cache");
            if (udfCache != null) {
                var isUdfCache = bool.Parse(udfCache);
                compiler.Expression.UdfCache = isUdfCache;
            }

            var extendedAggregationStr = GetOptionalAttribute(element, "extended-agg");
            if (extendedAggregationStr != null) {
                var extendedAggregation = bool.Parse(extendedAggregationStr);
                compiler.Expression.ExtendedAggregation = extendedAggregation;
            }

            var duckTypingStr = GetOptionalAttribute(element, "ducktyping");
            if (duckTypingStr != null) {
                var duckTyping = bool.Parse(duckTypingStr);
                compiler.Expression.DuckTyping = duckTyping;
            }

            var mathContextStr = GetOptionalAttribute(element, "math-context");
            if (mathContextStr != null) {
                try {
                    var mathContext = new MathContext(mathContextStr);
                    compiler.Expression.MathContext = mathContext;
                }
                catch (ArgumentException ex) {
                    throw new ConfigurationException("Failed to parse '" + mathContextStr + "' as a MathContext");
                }
            }
        }

        private static void HandleScripts(ConfigurationCompiler compiler, XmlElement element)
        {
            var defaultDialect = GetOptionalAttribute(element, "default-dialect");
            if (defaultDialect != null) {
                compiler.Scripts.DefaultDialect = defaultDialect;
            }
        }

        private static void HandleLanguage(ConfigurationCompiler compiler, XmlElement element)
        {
            ParseOptionalBoolean(element, "sort-using-collator", b => compiler.Language.SortUsingCollator = b);
        }

        private static void HandleStreamSelection(ConfigurationCompiler compiler, XmlElement element)
        {
            var nodeEnumerator = DOMElementEnumerator.Create(element.ChildNodes);
            while (nodeEnumerator.MoveNext()) {
                XmlElement subElement = nodeEnumerator.Current;
                if (subElement.Name == "stream-selector") {
                    var valueText = GetRequiredAttribute(subElement, "value");
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

        private static void HandleLogging(ConfigurationCompiler compiler, XmlElement element)
        {
            var nodeEnumerator = DOMElementEnumerator.Create(element.ChildNodes);
            while (nodeEnumerator.MoveNext()) {
                XmlElement subElement = nodeEnumerator.Current;
                if (subElement.Name == "code") {
                    ParseRequiredBoolean(subElement, "enabled", b => compiler.Logging.EnableCode = b);
                }
            }
        }

        private static void HandleByteCode(ConfigurationCompiler compiler, XmlElement element)
        {
            var codegen = compiler.ByteCode;
            ParseOptionalBoolean(element, "include-debugsymbols", codegen::setIncludeDebugSymbols);
            ParseOptionalBoolean(element, "include-comments", codegen::setIncludeComments);
            ParseOptionalBoolean(element, "attach-epl", codegen::setAttachEPL);
            ParseOptionalBoolean(element, "attach-module-epl", codegen::setAttachModuleEPL);
            ParseOptionalBoolean(element, "instrumented", codegen::setInstrumented);
            ParseOptionalBoolean(element, "allow-subscriber", codegen::setAllowSubscriber);

            ParseOptionalAccessMod(element, "access-modifier-context", codegen::setAccessModifierContext);
            ParseOptionalAccessMod(element, "access-modifier-event-type", codegen::setAccessModifierEventType);
            ParseOptionalAccessMod(element, "access-modifier-expression", codegen::setAccessModifierExpression);
            ParseOptionalAccessMod(element, "access-modifier-named-window", codegen::setAccessModifierNamedWindow);
            ParseOptionalAccessMod(element, "access-modifier-script", codegen::setAccessModifierScript);
            ParseOptionalAccessMod(element, "access-modifier-table", codegen::setAccessModifierTable);
            ParseOptionalAccessMod(element, "access-modifier-variable", codegen::setAccessModifierVariable);

            var busModifierEventType = GetOptionalAttribute(element, "bus-modifier-event-type");
            if (busModifierEventType != null) {
                try {
                    codegen.BusModifierEventType =
                        EventTypeBusModifier.ValueOf(busModifierEventType.Trim().ToUpperInvariant());
                }
                catch (Throwable t) {
                    throw new ConfigurationException(t.Message, t);
                }
            }
        }

        private static void ParseOptionalAccessMod(
            XmlElement element, string name, Consumer<NameAccessModifier> accessModifier)
        {
            var value = GetOptionalAttribute(element, name);
            if (value != null) {
                try {
                    accessModifier.Accept(NameAccessModifier.ValueOf(value.Trim().ToUpperInvariant()));
                }
                catch (Throwable t) {
                    throw new ConfigurationException(t.Message, t);
                }
            }
        }

        private static void HandlePlugInView(ConfigurationCompiler configuration, XmlElement element)
        {
            var @namespace = GetRequiredAttribute(element, "namespace");
            var name = GetRequiredAttribute(element, "name");
            var forgeClassName = GetRequiredAttribute(element, "forge-class");
            configuration.AddPlugInView(@namespace, name, forgeClassName);
        }

        private static void HandlePlugInVirtualDW(ConfigurationCompiler configuration, XmlElement element)
        {
            var @namespace = GetRequiredAttribute(element, "namespace");
            var name = GetRequiredAttribute(element, "name");
            var forgeClassName = GetRequiredAttribute(element, "forge-class");
            var config = GetOptionalAttribute(element, "config");
            configuration.AddPlugInVirtualDataWindow(@namespace, name, forgeClassName, config);
        }

        private static void HandlePlugInAggregation(ConfigurationCompiler configuration, XmlElement element)
        {
            var name = GetRequiredAttribute(element, "name");
            var forgeClassName = GetRequiredAttribute(element, "forge-class");
            configuration.AddPlugInAggregationFunctionForge(name, forgeClassName);
        }

        private static void HandlePlugInMultiFunctionAggregation(
            ConfigurationCompiler configuration, XmlElement element)
        {
            var functionNames = GetRequiredAttribute(element, "function-names");
            var forgeClassName = GetOptionalAttribute(element, "forge-class");

            var nodeEnumerator = DOMElementEnumerator.Create(element.ChildNodes);
            IDictionary<string, object> additionalProps = null;
            while (nodeEnumerator.MoveNext()) {
                XmlElement subElement = nodeEnumerator.Current;
                if (subElement.Name == "init-arg") {
                    var name = GetRequiredAttribute(subElement, "name");
                    var value = GetRequiredAttribute(subElement, "value");
                    if (additionalProps == null) {
                        additionalProps = new Dictionary<string, object>();
                    }

                    additionalProps.Put(name, value);
                }
            }

            var config = new ConfigurationCompilerPlugInAggregationMultiFunction(
                functionNames.SplitCsv(), forgeClassName);
            config.AdditionalConfiguredProperties = additionalProps;
            configuration.AddPlugInAggregationMultiFunction(config);
        }

        private static void HandlePlugInSingleRow(ConfigurationCompiler configuration, XmlElement element)
        {
            var name = element.Attributes.GetNamedItem("name").InnerText;
            var functionClassName = element.Attributes.GetNamedItem("function-class").InnerText;
            var functionMethodName = element.Attributes.GetNamedItem("function-method").InnerText;
            var valueCache = ConfigurationCompilerPlugInSingleRowFunction.ValueCacheEnum.DISABLED;
            var filterOptimizable = ConfigurationCompilerPlugInSingleRowFunction.FilterOptimizableEnum.ENABLED;
            var valueCacheStr = GetOptionalAttribute(element, "value-cache");
            if (valueCacheStr != null) {
                valueCache = EnumHelper.Parse<ConfigurationCompilerPlugInSingleRowFunction.ValueCacheEnum>(valueCacheStr);
            }

            var filterOptimizableStr = GetOptionalAttribute(element, "filter-optimizable");
            if (filterOptimizableStr != null) {
                filterOptimizable = EnumHelper.Parse<ConfigurationCompilerPlugInSingleRowFunction.FilterOptimizableEnum>(
                        filterOptimizableStr);
            }

            var rethrowExceptionsStr = GetOptionalAttribute(element, "rethrow-exceptions");
            var rethrowExceptions = false;
            if (rethrowExceptionsStr != null) {
                rethrowExceptions = bool.Parse(rethrowExceptionsStr);
            }

            var eventTypeName = GetOptionalAttribute(element, "event-type-name");
            configuration.AddPlugInSingleRowFunction(
                new ConfigurationCompilerPlugInSingleRowFunction(
                    name, functionClassName, functionMethodName, valueCache, filterOptimizable, rethrowExceptions,
                    eventTypeName));
        }

        private static void HandlePlugInPatternGuard(ConfigurationCompiler configuration, XmlElement element)
        {
            var @namespace = GetRequiredAttribute(element, "namespace");
            var name = GetRequiredAttribute(element, "name");
            var forgeClassName = GetRequiredAttribute(element, "forge-class");
            configuration.AddPlugInPatternGuard(@namespace, name, forgeClassName);
        }

        private static void HandlePlugInPatternObserver(ConfigurationCompiler configuration, XmlElement element)
        {
            var @namespace = GetRequiredAttribute(element, "namespace");
            var name = GetRequiredAttribute(element, "name");
            var forgeClassName = GetRequiredAttribute(element, "forge-class");
            configuration.AddPlugInPatternObserver(@namespace, name, forgeClassName);
        }
    }
} // end of namespace