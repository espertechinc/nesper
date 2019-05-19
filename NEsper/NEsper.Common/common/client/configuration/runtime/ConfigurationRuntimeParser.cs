///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Xml;
using com.espertech.esper.collection;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.util.DOMUtil;

namespace com.espertech.esper.common.client.configuration.runtime
{
    /// <summary>
    ///     Parser for the runtime section of configuration.
    /// </summary>
    public class ConfigurationRuntimeParser
    {
        /// <summary>
        ///     Configure the runtime section from a provided element
        /// </summary>
        /// <param name="runtime">runtime section</param>
        /// <param name="runtimeElement">element</param>
        public static void DoConfigure(
            ConfigurationRuntime runtime,
            XmlElement runtimeElement)
        {
            var eventTypeNodeIterator = DOMElementEnumerator.Create(runtimeElement.ChildNodes);
            while (eventTypeNodeIterator.MoveNext()) {
                var element = eventTypeNodeIterator.Current;
                var nodeName = element.Name;
                switch (nodeName) {
                    case "plugin-loader":
                        HandlePluginLoaders(runtime, element);
                        break;
                    case "threading":
                        HandleThreading(runtime, element);
                        break;
                    case "logging":
                        HandleLogging(runtime, element);
                        break;
                    case "variables":
                        HandleVariables(runtime, element);
                        break;
                    case "time-source":
                        HandleTimeSource(runtime, element);
                        break;
                    case "metrics-reporting":
                        HandleMetricsReporting(runtime, element);
                        break;
                    case "exceptionHandling":
                        HandleExceptionHandling(runtime, element);
                        break;
                    case "conditionHandling":
                        HandleConditionHandling(runtime, element);
                        break;
                    case "patterns":
                        HandlePatterns(runtime, element);
                        break;
                    case "match-recognize":
                        HandleMatchRecognize(runtime, element);
                        break;
                    case "expression":
                        HandleExpression(runtime, element);
                        break;
                    case "execution":
                        HandleExecution(runtime, element);
                        break;
                }
            }
        }

        private static void HandleExecution(
            ConfigurationRuntime runtime,
            XmlElement parentElement)
        {
            ParseOptionalBoolean(parentElement, "prioritized", b => runtime.Execution.IsPrioritized = b);
            ParseOptionalBoolean(parentElement, "fairlock", b => runtime.Execution.IsFairlock = b);
            ParseOptionalBoolean(parentElement, "disable-locking", b => runtime.Execution.IsDisableLocking = b);

            var filterServiceProfileStr = GetOptionalAttribute(parentElement, "filter-service-profile");
            if (filterServiceProfileStr != null) {
                var profile = EnumHelper.Parse<FilterServiceProfile>(filterServiceProfileStr);
                runtime.Execution.FilterServiceProfile = profile;
            }

            var declExprValueCacheSizeStr = GetOptionalAttribute(parentElement, "declared-expr-value-cache-size");
            if (declExprValueCacheSizeStr != null) {
                runtime.Execution.DeclaredExprValueCacheSize = int.Parse(declExprValueCacheSizeStr);
            }
        }

        private static void HandleExpression(
            ConfigurationRuntime runtime,
            XmlElement element)
        {
            ParseOptionalBoolean(element, "self-subselect-preeval", b => runtime.Expression.IsSelfSubselectPreeval = b);

            var timeZoneStr = GetOptionalAttribute(element, "time-zone");
            if (timeZoneStr != null) {
                var timeZone = TimeZoneHelper.GetTimeZoneInfo(timeZoneStr);
                runtime.Expression.TimeZone = timeZone;
            }
        }

        private static void HandleMatchRecognize(
            ConfigurationRuntime runtime,
            XmlElement element)
        {
            var nodeIterator = DOMElementEnumerator.Create(element.ChildNodes);
            while (nodeIterator.MoveNext()) {
                var subElement = nodeIterator.Current;
                if (subElement.Name == "max-state") {
                    var valueText = GetRequiredAttribute(subElement, "value");
                    var value = long.Parse(valueText);
                    runtime.MatchRecognize.MaxStates = value;

                    var preventText = GetOptionalAttribute(subElement, "prevent-start");
                    if (preventText != null) {
                        runtime.MatchRecognize.IsMaxStatesPreventStart = bool.Parse(preventText);
                    }
                }
            }
        }

        private static void HandlePatterns(
            ConfigurationRuntime runtime,
            XmlElement element)
        {
            var nodeIterator = DOMElementEnumerator.Create(element.ChildNodes);
            while (nodeIterator.MoveNext()) {
                var subElement = nodeIterator.Current;
                if (subElement.Name == "max-subexpression") {
                    var valueText = GetRequiredAttribute(subElement, "value");
                    var value = long.Parse(valueText);
                    runtime.Patterns.MaxSubexpressions = value;

                    var preventText = GetOptionalAttribute(subElement, "prevent-start");
                    if (preventText != null) {
                        runtime.Patterns.IsMaxSubexpressionPreventStart = bool.Parse(preventText);
                    }
                }
            }
        }

        private static void HandleConditionHandling(
            ConfigurationRuntime runtime,
            XmlElement element)
        {
            runtime.ConditionHandling.AddClasses(GetHandlerFactories(element));
        }

        private static void HandleExceptionHandling(
            ConfigurationRuntime runtime,
            XmlElement element)
        {
            runtime.ExceptionHandling.AddClasses(GetHandlerFactories(element));
            var enableUndeployRethrowStr = GetOptionalAttribute(element, "undeploy-rethrow-policy");
            if (enableUndeployRethrowStr != null) {
                runtime.ExceptionHandling.UndeployRethrowPolicy = EnumHelper.Parse<UndeployRethrowPolicy>(enableUndeployRethrowStr);
            }
        }

        private static void HandleMetricsReporting(
            ConfigurationRuntime runtime,
            XmlElement element)
        {
            ParseOptionalBoolean(element, "enabled", b => runtime.MetricsReporting.WithMetricsReporting(b));

            var runtimeInterval = GetOptionalAttribute(element, "runtime-interval");
            if (runtimeInterval != null) {
                runtime.MetricsReporting.RuntimeInterval = long.Parse(runtimeInterval);
            }

            var statementInterval = GetOptionalAttribute(element, "statement-interval");
            if (statementInterval != null) {
                runtime.MetricsReporting.StatementInterval = long.Parse(statementInterval);
            }

            var threading = GetOptionalAttribute(element, "threading");
            if (threading != null) {
                runtime.MetricsReporting.IsThreading = bool.Parse(threading);
            }

            var runtimeMetrics = GetOptionalAttribute(element, "runtime-metrics");
            if (runtimeMetrics != null) {
                runtime.MetricsReporting.IsRuntimeMetrics = bool.Parse(runtimeMetrics);
            }

            var nodeIterator = DOMElementEnumerator.Create(element.ChildNodes);
            while (nodeIterator.MoveNext()) {
                var subElement = nodeIterator.Current;
                if (subElement.Name == "stmtgroup") {
                    var name = GetRequiredAttribute(subElement, "name");
                    var interval = long.Parse(GetRequiredAttribute(subElement, "interval"));

                    var metrics = new ConfigurationRuntimeMetricsReporting.StmtGroupMetrics();
                    metrics.Interval = interval;
                    runtime.MetricsReporting.AddStmtGroup(name, metrics);

                    var defaultInclude = GetOptionalAttribute(subElement, "default-include");
                    if (defaultInclude != null) {
                        metrics.IsDefaultInclude = bool.Parse(defaultInclude);
                    }

                    var numStmts = GetOptionalAttribute(subElement, "num-stmts");
                    if (numStmts != null) {
                        metrics.NumStatements = int.Parse(numStmts);
                    }

                    var reportInactive = GetOptionalAttribute(subElement, "report-inactive");
                    if (reportInactive != null) {
                        metrics.IsReportInactive = bool.Parse(reportInactive);
                    }

                    HandleMetricsReportingPatterns(metrics, subElement);
                }
            }
        }

        private static void HandleTimeSource(
            ConfigurationRuntime runtime,
            XmlElement element)
        {
            var nodeIterator = DOMElementEnumerator.Create(element.ChildNodes);
            while (nodeIterator.MoveNext()) {
                var subElement = nodeIterator.Current;
                if (subElement.Name == "time-source-type") {
                    var valueText = GetRequiredAttribute(subElement, "value");
                    if (valueText == null) {
                        throw new ConfigurationException("No value attribute supplied for time-source element");
                    }

                    TimeSourceType timeSourceType;
                    valueText = valueText.ToUpperInvariant().Trim();
                    switch (valueText) {
                        case "NANO":
                            timeSourceType = TimeSourceType.NANO;
                            break;
                        case "MILLI":
                            timeSourceType = TimeSourceType.MILLI;
                            break;
                        default:
                            throw new ConfigurationException(
                                "Value attribute for time-source element invalid, " +
                                "expected one of the following keywords: nano, milli");
                    }

                    runtime.TimeSource.TimeSourceType = timeSourceType;
                }
            }
        }

        private static void HandleVariables(
            ConfigurationRuntime runtime,
            XmlElement element)
        {
            var nodeIterator = DOMElementEnumerator.Create(element.ChildNodes);
            while (nodeIterator.MoveNext()) {
                var subElement = nodeIterator.Current;
                if (subElement.Name == "msec-version-release") {
                    var valueText = GetRequiredAttribute(subElement, "value");
                    var value = long.Parse(valueText);
                    runtime.Variables.MsecVersionRelease = value;
                }
            }
        }

        private static void HandleLogging(
            ConfigurationRuntime runtime,
            XmlElement element)
        {
            var nodeIterator = DOMElementEnumerator.Create(element.ChildNodes);
            while (nodeIterator.MoveNext()) {
                var subElement = nodeIterator.Current;
                switch (subElement.Name) {
                    case "execution-path": {
                        var valueText = GetRequiredAttribute(subElement, "enabled");
                        var value = bool.Parse(valueText);
                        runtime.Logging.IsEnableExecutionDebug = value;
                        break;
                    }
                    case "timer-debug": {
                        var valueText = GetRequiredAttribute(subElement, "enabled");
                        var value = bool.Parse(valueText);
                        runtime.Logging.IsEnableTimerDebug = value;
                        break;
                    }
                    case "audit":
                        runtime.Logging.AuditPattern = GetOptionalAttribute(subElement, "pattern");
                        break;
                }
            }
        }

        private static void HandleThreading(
            ConfigurationRuntime runtime,
            XmlElement element)
        {
            ParseOptionalBoolean(element, "runtime-fairlock", b => runtime.Threading.IsRuntimeFairlock = b);

            var nodeIterator = DOMElementEnumerator.Create(element.ChildNodes);
            while (nodeIterator.MoveNext()) {
                var subElement = nodeIterator.Current;
                switch (subElement.Name) {
                    case "listener-dispatch": {
                        var preserveOrderText = GetRequiredAttribute(subElement, "preserve-order");
                        var preserveOrder = bool.Parse(preserveOrderText);
                        runtime.Threading.IsListenerDispatchPreserveOrder = preserveOrder;

                        if (subElement.Attributes.GetNamedItem("timeout-msec") != null) {
                            var timeoutMSecText = subElement.Attributes.GetNamedItem("timeout-msec").InnerText;
                            var timeoutMSec = long.Parse(timeoutMSecText);
                            runtime.Threading.ListenerDispatchTimeout = timeoutMSec;
                        }

                        if (subElement.Attributes.GetNamedItem("locking") != null) {
                            var value = subElement.Attributes.GetNamedItem("locking").InnerText;
                            runtime.Threading.ListenerDispatchLocking = EnumHelper.Parse<Locking>(value);
                        }

                        break;
                    }
                    case "insert-into-dispatch": {
                        var preserveOrderText = GetRequiredAttribute(subElement, "preserve-order");
                        var preserveOrder = bool.Parse(preserveOrderText);
                        runtime.Threading.IsInsertIntoDispatchPreserveOrder = preserveOrder;

                        if (subElement.Attributes.GetNamedItem("timeout-msec") != null) {
                            var timeoutMSecText = subElement.Attributes.GetNamedItem("timeout-msec").InnerText;
                            var timeoutMSec = long.Parse(timeoutMSecText);
                            runtime.Threading.InsertIntoDispatchTimeout = timeoutMSec;
                        }

                        if (subElement.Attributes.GetNamedItem("locking") != null) {
                            var value = subElement.Attributes.GetNamedItem("locking").InnerText;
                            runtime.Threading.InsertIntoDispatchLocking = EnumHelper.Parse<Locking>(value);
                        }

                        break;
                    }
                    case "named-window-consumer-dispatch": {
                        var preserveOrderText = GetRequiredAttribute(subElement, "preserve-order");
                        var preserveOrder = bool.Parse(preserveOrderText);
                        runtime.Threading.IsNamedWindowConsumerDispatchPreserveOrder = preserveOrder;

                        if (subElement.Attributes.GetNamedItem("timeout-msec") != null) {
                            var timeoutMSecText = subElement.Attributes.GetNamedItem("timeout-msec").InnerText;
                            var timeoutMSec = int.Parse(timeoutMSecText);
                            runtime.Threading.NamedWindowConsumerDispatchTimeout = timeoutMSec;
                        }

                        if (subElement.Attributes.GetNamedItem("locking") != null) {
                            var value = subElement.Attributes.GetNamedItem("locking").InnerText;
                            runtime.Threading.NamedWindowConsumerDispatchLocking = EnumHelper.Parse<Locking>(value);
                        }

                        break;
                    }
                    case "internal-timer": {
                        var enabledText = GetRequiredAttribute(subElement, "enabled");
                        var enabled = bool.Parse(enabledText);
                        var msecResolutionText = GetRequiredAttribute(subElement, "msec-resolution");
                        var msecResolution = long.Parse(msecResolutionText);
                        runtime.Threading.IsInternalTimerEnabled = enabled;
                        runtime.Threading.InternalTimerMsecResolution = msecResolution;
                        break;
                    }
                    case "threadpool-inbound": {
                        var result = ParseThreadPoolConfig(subElement);
                        runtime.Threading.IsThreadPoolInbound = result.IsEnabled;
                        runtime.Threading.ThreadPoolInboundNumThreads = result.NumThreads;
                        runtime.Threading.ThreadPoolInboundCapacity = result.Capacity;
                        break;
                    }
                    case "threadpool-outbound": {
                        var result = ParseThreadPoolConfig(subElement);
                        runtime.Threading.IsThreadPoolOutbound = result.IsEnabled;
                        runtime.Threading.ThreadPoolOutboundNumThreads = result.NumThreads;
                        runtime.Threading.ThreadPoolOutboundCapacity = result.Capacity;
                        break;
                    }
                    case "threadpool-timerexec": {
                        var result = ParseThreadPoolConfig(subElement);
                        runtime.Threading.IsThreadPoolTimerExec = result.IsEnabled;
                        runtime.Threading.ThreadPoolTimerExecNumThreads = result.NumThreads;
                        runtime.Threading.ThreadPoolTimerExecCapacity = result.Capacity;
                        break;
                    }
                    case "threadpool-routeexec": {
                        var result = ParseThreadPoolConfig(subElement);
                        runtime.Threading.IsThreadPoolRouteExec = result.IsEnabled;
                        runtime.Threading.ThreadPoolRouteExecNumThreads = result.NumThreads;
                        runtime.Threading.ThreadPoolRouteExecCapacity = result.Capacity;
                        break;
                    }
                }
            }
        }

        private static void HandlePluginLoaders(
            ConfigurationRuntime configuration,
            XmlElement element)
        {
            var loaderName = GetRequiredAttribute(element, "name");
            var className = GetRequiredAttribute(element, "class-name");
            var properties = new Properties();
            string configXML = null;
            var nodeIterator = DOMElementEnumerator.Create(element.ChildNodes);
            while (nodeIterator.MoveNext()) {
                var subElement = nodeIterator.Current;
                switch (subElement.Name) {
                    case "init-arg": {
                        var name = GetRequiredAttribute(subElement, "name");
                        var value = GetRequiredAttribute(subElement, "value");
                        properties.Put(name, value);
                        break;
                    }
                    case "config-xml": {
                        var nodeIter = DOMElementEnumerator.Create(subElement.ChildNodes);
                        if (!nodeIter.MoveNext()) {
                            throw new ConfigurationException(
                                "Error handling config-xml for plug-in loader '" + loaderName +
                                "', no child node found under initializer element, expecting an element node");
                        }

                        configXML = nodeIter.Current.InnerXml;
                        break;
                    }
                }
            }

            configuration.AddPluginLoader(loaderName, className, properties, configXML);
        }

        private static ThreadPoolConfig ParseThreadPoolConfig(XmlElement parentElement)
        {
            var enabled = GetRequiredAttribute(parentElement, "enabled");
            var isEnabled = bool.Parse(enabled);

            var numThreadsStr = GetRequiredAttribute(parentElement, "num-threads");
            var numThreads = int.Parse(numThreadsStr);

            var capacityStr = GetOptionalAttribute(parentElement, "capacity");
            int? capacity = null;
            if (capacityStr != null) {
                capacity = int.Parse(capacityStr);
            }

            return new ThreadPoolConfig(isEnabled, numThreads, capacity);
        }

        private static void HandleMetricsReportingPatterns(
            ConfigurationRuntimeMetricsReporting.StmtGroupMetrics groupDef,
            XmlElement parentElement)
        {
            var nodeIterator = DOMElementEnumerator.Create(parentElement.ChildNodes);
            while (nodeIterator.MoveNext()) {
                var subElement = nodeIterator.Current;
                switch (subElement.Name) {
                    case "include-regex": {
                        var text = subElement.ChildNodes.Item(0).InnerText;
                        groupDef.Patterns.Add(new Pair<StringPatternSet, bool>(new StringPatternSetRegex(text), true));
                        break;
                    }
                    case "exclude-regex": {
                        var text = subElement.ChildNodes.Item(0).InnerText;
                        groupDef.Patterns.Add(new Pair<StringPatternSet, bool>(new StringPatternSetRegex(text), false));
                        break;
                    }
                    case "include-like": {
                        var text = subElement.ChildNodes.Item(0).InnerText;
                        groupDef.Patterns.Add(new Pair<StringPatternSet, bool>(new StringPatternSetLike(text), true));
                        break;
                    }
                    case "exclude-like": {
                        var text = subElement.ChildNodes.Item(0).InnerText;
                        groupDef.Patterns.Add(new Pair<StringPatternSet, bool>(new StringPatternSetLike(text), false));
                        break;
                    }
                }
            }
        }

        private static IList<string> GetHandlerFactories(XmlElement parentElement)
        {
            IList<string> list = new List<string>();
            var nodeIterator = DOMElementEnumerator.Create(parentElement.ChildNodes);
            while (nodeIterator.MoveNext()) {
                var subElement = nodeIterator.Current;
                if (subElement.Name == "handlerFactory") {
                    var text = GetRequiredAttribute(subElement, "class");
                    list.Add(text);
                }
            }

            return list;
        }

        private class ThreadPoolConfig
        {
            public ThreadPoolConfig(
                bool enabled,
                int numThreads,
                int? capacity)
            {
                IsEnabled = enabled;
                NumThreads = numThreads;
                Capacity = capacity;
            }

            public bool IsEnabled { get; }

            public int NumThreads { get; }

            public int? Capacity { get; }
        }
    }
} // end of namespace