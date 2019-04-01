///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
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
        public static void DoConfigure(ConfigurationRuntime runtime, XmlElement runtimeElement)
        {
            var eventTypeNodeIterator = DOMElementEnumerator.Create(runtimeElement.ChildNodes);
            while (eventTypeNodeIterator.MoveNext()) {
                XmlElement element = eventTypeNodeIterator.Current;
                string nodeName = element.Name;
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

        private static void HandleExecution(ConfigurationRuntime runtime, XmlElement parentElement)
        {
            ParseOptionalBoolean(parentElement, "prioritized", b => runtime.Execution.Prioritized = b);
            ParseOptionalBoolean(parentElement, "fairlock", b => runtime.Execution.Fairlock = b);
            ParseOptionalBoolean(parentElement, "disable-locking", b => runtime.Execution.DisableLocking = b);

            string filterServiceProfileStr = GetOptionalAttribute(parentElement, "filter-service-profile");
            if (filterServiceProfileStr != null) {
                FilterServiceProfile profile =
                    FilterServiceProfile.ValueOf(filterServiceProfileStr.ToUpperInvariant());
                runtime.Execution.FilterServiceProfile = profile;
            }

            string declExprValueCacheSizeStr = GetOptionalAttribute(parentElement, "declared-expr-value-cache-size");
            if (declExprValueCacheSizeStr != null) {
                runtime.Execution.DeclaredExprValueCacheSize = int.Parse(declExprValueCacheSizeStr);
            }
        }

        private static void HandleExpression(ConfigurationRuntime runtime, XmlElement element)
        {
            ParseOptionalBoolean(element, "self-subselect-preeval", b => runtime.Expression.SelfSubselectPreeval = b);

            string timeZoneStr = GetOptionalAttribute(element, "time-zone");
            if (timeZoneStr != null) {
                TimeZone timeZone = TimeZone.GetTimeZone(timeZoneStr);
                runtime.Expression.TimeZone = timeZone;
            }
        }

        private static void HandleMatchRecognize(ConfigurationRuntime runtime, XmlElement element)
        {
            var nodeIterator = DOMElementEnumerator.Create(element.ChildNodes);
            while (nodeIterator.MoveNext()) {
                XmlElement subElement = nodeIterator.Current;
                if (subElement.Name == ("max-state")) {
                    string valueText = GetRequiredAttribute(subElement, "value");
                    long? value = long.Parse(valueText);
                    runtime.MatchRecognize.MaxStates = value;

                    string preventText = GetOptionalAttribute(subElement, "prevent-start");
                    if (preventText != null) {
                        runtime.MatchRecognize.MaxStatesPreventStart = bool.Parse(preventText);
                    }
                }
            }
        }

        private static void HandlePatterns(ConfigurationRuntime runtime, XmlElement element)
        {
            var nodeIterator = DOMElementEnumerator.Create(element.ChildNodes);
            while (nodeIterator.MoveNext()) {
                XmlElement subElement = nodeIterator.Current;
                if (subElement.Name == ("max-subexpression")) {
                    string valueText = GetRequiredAttribute(subElement, "value");
                    long? value = long.Parse(valueText);
                    runtime.Patterns.MaxSubexpressions = value;

                    string preventText = GetOptionalAttribute(subElement, "prevent-start");
                    if (preventText != null) {
                        runtime.Patterns.MaxSubexpressionPreventStart = bool.Parse(preventText);
                    }
                }
            }
        }

        private static void HandleConditionHandling(ConfigurationRuntime runtime, XmlElement element)
        {
            runtime.ConditionHandling.AddClasses(GetHandlerFactories(element));
        }

        private static void HandleExceptionHandling(ConfigurationRuntime runtime, XmlElement element)
        {
            runtime.ExceptionHandling.AddClasses(GetHandlerFactories(element));
            string enableUndeployRethrowStr = GetOptionalAttribute(element, "undeploy-rethrow-policy");
            if (enableUndeployRethrowStr != null) {
                runtime.ExceptionHandling.UndeployRethrowPolicy =
                    UndeployRethrowPolicy.ValueOf(enableUndeployRethrowStr.ToUpperInvariant());
            }
        }

        private static void HandleMetricsReporting(ConfigurationRuntime runtime, XmlElement element)
        {
            ParseOptionalBoolean(element, "enabled", b => runtime.MetricsReporting.EnableMetricsReporting = b);

            string runtimeInterval = GetOptionalAttribute(element, "runtime-interval");
            if (runtimeInterval != null) {
                runtime.MetricsReporting.RuntimeInterval = long.Parse(runtimeInterval);
            }

            string statementInterval = GetOptionalAttribute(element, "statement-interval");
            if (statementInterval != null) {
                runtime.MetricsReporting.StatementInterval = long.Parse(statementInterval);
            }

            string threading = GetOptionalAttribute(element, "threading");
            if (threading != null) {
                runtime.MetricsReporting.Threading = bool.Parse(threading);
            }

            string jmxRuntimeMetrics = GetOptionalAttribute(element, "jmx-runtime-metrics");
            if (jmxRuntimeMetrics != null) {
                runtime.MetricsReporting.JmxRuntimeMetrics = bool.Parse(jmxRuntimeMetrics);
            }

            var nodeIterator = DOMElementEnumerator.Create(element.ChildNodes);
            while (nodeIterator.MoveNext()) {
                XmlElement subElement = nodeIterator.Current;
                if (subElement.Name == ("stmtgroup")) {
                    string name = GetRequiredAttribute(subElement, "name");
                    var interval = long.Parse(GetRequiredAttribute(subElement, "interval"));

                    var metrics = new ConfigurationRuntimeMetricsReporting.StmtGroupMetrics();
                    metrics.Interval = interval;
                    runtime.MetricsReporting.AddStmtGroup(name, metrics);

                    string defaultInclude = GetOptionalAttribute(subElement, "default-include");
                    if (defaultInclude != null) {
                        metrics.DefaultInclude = bool.Parse(defaultInclude);
                    }

                    string numStmts = GetOptionalAttribute(subElement, "num-stmts");
                    if (numStmts != null) {
                        metrics.NumStatements = int.Parse(numStmts);
                    }

                    string reportInactive = GetOptionalAttribute(subElement, "report-inactive");
                    if (reportInactive != null) {
                        metrics.ReportInactive = bool.Parse(reportInactive);
                    }

                    HandleMetricsReportingPatterns(metrics, subElement);
                }
            }
        }

        private static void HandleTimeSource(ConfigurationRuntime runtime, XmlElement element)
        {
            var nodeIterator = DOMElementEnumerator.Create(element.ChildNodes);
            while (nodeIterator.MoveNext()) {
                XmlElement subElement = nodeIterator.Current;
                if (subElement.Name == ("time-source-type")) {
                    string valueText = GetRequiredAttribute(subElement, "value");
                    if (valueText == null) {
                        throw new ConfigurationException("No value attribute supplied for time-source element");
                    }

                    TimeSourceType timeSourceType;
                    if (valueText.ToUpperInvariant().Trim() == ("NANO")) {
                        timeSourceType = TimeSourceType.NANO;
                    }
                    else if (valueText.ToUpperInvariant().Trim() == ("MILLI")) {
                        timeSourceType = TimeSourceType.MILLI;
                    }
                    else {
                        throw new ConfigurationException(
                            "Value attribute for time-source element invalid, " +
                            "expected one of the following keywords: nano, milli");
                    }

                    runtime.TimeSource.TimeSourceType = timeSourceType;
                }
            }
        }

        private static void HandleVariables(ConfigurationRuntime runtime, XmlElement element)
        {
            var nodeIterator = DOMElementEnumerator.Create(element.ChildNodes);
            while (nodeIterator.MoveNext()) {
                XmlElement subElement = nodeIterator.Current;
                if (subElement.Name == ("msec-version-release")) {
                    string valueText = GetRequiredAttribute(subElement, "value");
                    long? value = long.Parse(valueText);
                    runtime.Variables.MsecVersionRelease = value;
                }
            }
        }

        private static void HandleLogging(ConfigurationRuntime runtime, XmlElement element)
        {
            var nodeIterator = DOMElementEnumerator.Create(element.ChildNodes);
            while (nodeIterator.MoveNext()) {
                XmlElement subElement = nodeIterator.Current;
                if (subElement.Name == ("execution-path")) {
                    string valueText = GetRequiredAttribute(subElement, "enabled");
                    var value = bool.Parse(valueText);
                    runtime.Logging.EnableExecutionDebug = value;
                }

                if (subElement.Name == ("timer-debug")) {
                    string valueText = GetRequiredAttribute(subElement, "enabled");
                    var value = bool.Parse(valueText);
                    runtime.Logging.EnableTimerDebug = value;
                }

                if (subElement.Name == ("audit")) {
                    runtime.Logging.AuditPattern = GetOptionalAttribute(subElement, "pattern");
                }
            }
        }

        private static void HandleThreading(ConfigurationRuntime runtime, XmlElement element)
        {
            ParseOptionalBoolean(element, "runtime-fairlock", b => runtime.Threading.RuntimeFairlock = b);

            var nodeIterator = DOMElementEnumerator.Create(element.ChildNodes);
            while (nodeIterator.MoveNext()) {
                XmlElement subElement = nodeIterator.Current;
                if (subElement.Name == ("listener-dispatch")) {
                    string preserveOrderText = GetRequiredAttribute(subElement, "preserve-order");
                    var preserveOrder = bool.Parse(preserveOrderText);
                    runtime.Threading.ListenerDispatchPreserveOrder = preserveOrder;

                    if (subElement.Attributes.GetNamedItem("timeout-msec") != null) {
                        string timeoutMSecText = subElement.Attributes.GetNamedItem("timeout-msec").InnerText;
                        long? timeoutMSec = long.Parse(timeoutMSecText);
                        runtime.Threading.ListenerDispatchTimeout = timeoutMSec;
                    }

                    if (subElement.Attributes.GetNamedItem("locking") != null) {
                        string value = subElement.Attributes.GetNamedItem("locking").InnerText;
                        runtime.Threading.ListenerDispatchLocking =
                            Locking.ValueOf(value.ToUpperInvariant());
                    }
                }

                if (subElement.Name == ("insert-into-dispatch")) {
                    string preserveOrderText = GetRequiredAttribute(subElement, "preserve-order");
                    var preserveOrder = bool.Parse(preserveOrderText);
                    runtime.Threading.InsertIntoDispatchPreserveOrder = preserveOrder;

                    if (subElement.Attributes.GetNamedItem("timeout-msec") != null) {
                        string timeoutMSecText = subElement.Attributes.GetNamedItem("timeout-msec").InnerText;
                        long? timeoutMSec = long.Parse(timeoutMSecText);
                        runtime.Threading.InsertIntoDispatchTimeout = timeoutMSec;
                    }

                    if (subElement.Attributes.GetNamedItem("locking") != null) {
                        string value = subElement.Attributes.GetNamedItem("locking").InnerText;
                        runtime.Threading.InsertIntoDispatchLocking =
                            Locking.ValueOf(value.ToUpperInvariant());
                    }
                }

                if (subElement.Name == ("named-window-consumer-dispatch")) {
                    string preserveOrderText = GetRequiredAttribute(subElement, "preserve-order");
                    var preserveOrder = bool.Parse(preserveOrderText);
                    runtime.Threading.NamedWindowConsumerDispatchPreserveOrder = preserveOrder;

                    if (subElement.Attributes.GetNamedItem("timeout-msec") != null) {
                        string timeoutMSecText = subElement.Attributes.GetNamedItem("timeout-msec").InnerText;
                        long? timeoutMSec = long.Parse(timeoutMSecText);
                        runtime.Threading.NamedWindowConsumerDispatchTimeout = timeoutMSec;
                    }

                    if (subElement.Attributes.GetNamedItem("locking") != null) {
                        string value = subElement.Attributes.GetNamedItem("locking").InnerText;
                        runtime.Threading.NamedWindowConsumerDispatchLocking =
                            Locking.ValueOf(value.ToUpperInvariant());
                    }
                }

                if (subElement.Name == ("internal-timer")) {
                    string enabledText = GetRequiredAttribute(subElement, "enabled");
                    var enabled = bool.Parse(enabledText);
                    string msecResolutionText = GetRequiredAttribute(subElement, "msec-resolution");
                    long? msecResolution = long.Parse(msecResolutionText);
                    runtime.Threading.InternalTimerEnabled = enabled;
                    runtime.Threading.InternalTimerMsecResolution = msecResolution;
                }

                if (subElement.Name == ("threadpool-inbound")) {
                    var result = ParseThreadPoolConfig(subElement);
                    runtime.Threading.ThreadPoolInbound = result.IsEnabled;
                    runtime.Threading.ThreadPoolInboundNumThreads = result.NumThreads;
                    runtime.Threading.ThreadPoolInboundCapacity = result.Capacity;
                }

                if (subElement.Name == ("threadpool-outbound")) {
                    var result = ParseThreadPoolConfig(subElement);
                    runtime.Threading.ThreadPoolOutbound = result.IsEnabled;
                    runtime.Threading.ThreadPoolOutboundNumThreads = result.NumThreads;
                    runtime.Threading.ThreadPoolOutboundCapacity = result.Capacity;
                }

                if (subElement.Name == ("threadpool-timerexec")) {
                    var result = ParseThreadPoolConfig(subElement);
                    runtime.Threading.ThreadPoolTimerExec = result.IsEnabled;
                    runtime.Threading.ThreadPoolTimerExecNumThreads = result.NumThreads;
                    runtime.Threading.ThreadPoolTimerExecCapacity = result.Capacity;
                }

                if (subElement.Name == ("threadpool-routeexec")) {
                    var result = ParseThreadPoolConfig(subElement);
                    runtime.Threading.ThreadPoolRouteExec = result.IsEnabled;
                    runtime.Threading.ThreadPoolRouteExecNumThreads = result.NumThreads;
                    runtime.Threading.ThreadPoolRouteExecCapacity = result.Capacity;
                }
            }
        }

        private static void HandlePluginLoaders(ConfigurationRuntime configuration, XmlElement element)
        {
            string loaderName = GetRequiredAttribute(element, "name");
            string className = GetRequiredAttribute(element, "class-name");
            var properties = new Properties();
            string configXML = null;
            var nodeIterator = DOMElementEnumerator.Create(element.ChildNodes);
            while (nodeIterator.MoveNext()) {
                XmlElement subElement = nodeIterator.Current;
                if (subElement.Name == ("init-arg")) {
                    string name = GetRequiredAttribute(subElement, "name");
                    string value = GetRequiredAttribute(subElement, "value");
                    properties.Put(name, value);
                }

                if (subElement.Name == ("config-xml")) {
                    var nodeIter = DOMElementEnumerator.Create(subElement.ChildNodes);
                    if (!nodeIter.MoveNext()) {
                        throw new ConfigurationException(
                            "Error handling config-xml for plug-in loader '" + loaderName +
                            "', no child node found under initializer element, expecting an element node");
                    }

                    var output = new StringWriter();
                    try {
                        TransformerFactory.NewInstance().NewTransformer().Transform(
                            new DOMSource(nodeIter.Current), new StreamResult(output));
                    }
                    catch (TransformerException e) {
                        throw new ConfigurationException(
                            "Error handling config-xml for plug-in loader '" + loaderName + "' :" + e.Message, e);
                    }

                    configXML = output.ToString();
                }
            }

            configuration.AddPluginLoader(loaderName, className, properties, configXML);
        }

        private static ThreadPoolConfig ParseThreadPoolConfig(XmlElement parentElement)
        {
            string enabled = GetRequiredAttribute(parentElement, "enabled");
            var isEnabled = bool.Parse(enabled);

            string numThreadsStr = GetRequiredAttribute(parentElement, "num-threads");
            var numThreads = int.Parse(numThreadsStr);

            string capacityStr = GetOptionalAttribute(parentElement, "capacity");
            int? capacity = null;
            if (capacityStr != null) {
                capacity = int.Parse(capacityStr);
            }

            return new ThreadPoolConfig(isEnabled, numThreads, capacity);
        }

        private static void HandleMetricsReportingPatterns(
            ConfigurationRuntimeMetricsReporting.StmtGroupMetrics groupDef, XmlElement parentElement)
        {
            var nodeIterator = DOMElementEnumerator.Create(parentElement.ChildNodes);
            while (nodeIterator.MoveNext()) {
                XmlElement subElement = nodeIterator.Current;
                if (subElement.Name == ("include-regex")) {
                    string text = subElement.ChildNodes.Item(0).InnerText;
                    groupDef.Patterns.Add(new Pair<StringPatternSet, bool>(new StringPatternSetRegex(text), true));
                }

                if (subElement.Name == ("exclude-regex")) {
                    string text = subElement.ChildNodes.Item(0).InnerText;
                    groupDef.Patterns.Add(new Pair<StringPatternSet, bool>(new StringPatternSetRegex(text), false));
                }

                if (subElement.Name == ("include-like")) {
                    string text = subElement.ChildNodes.Item(0).InnerText;
                    groupDef.Patterns.Add(new Pair<StringPatternSet, bool>(new StringPatternSetLike(text), true));
                }

                if (subElement.Name == ("exclude-like")) {
                    string text = subElement.ChildNodes.Item(0).InnerText;
                    groupDef.Patterns.Add(new Pair<StringPatternSet, bool>(new StringPatternSetLike(text), false));
                }
            }
        }

        private static IList<string> GetHandlerFactories(XmlElement parentElement)
        {
            IList<string> list = new List<string>();
            var nodeIterator = DOMElementEnumerator.Create(parentElement.ChildNodes);
            while (nodeIterator.MoveNext()) {
                XmlElement subElement = nodeIterator.Current;
                if (subElement.Name == ("handlerFactory")) {
                    string text = GetRequiredAttribute(subElement, "class");
                    list.Add(text);
                }
            }

            return list;
        }

        private class ThreadPoolConfig
        {
            public ThreadPoolConfig(bool enabled, int numThreads, int? capacity)
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