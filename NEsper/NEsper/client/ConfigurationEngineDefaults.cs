///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client.hook;
using com.espertech.esper.client.soda;
using com.espertech.esper.client.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.client
{
    /// <summary>
    /// Provides access to engine configuration defaults for modification.
    /// </summary>
    [Serializable]
    public class ConfigurationEngineDefaults
    {
        /// <summary>Ctor.</summary>
        public ConfigurationEngineDefaults()
        {
            Threading = new ThreadingConfig();
            ViewResources = new ViewResourcesConfig();
            EventMeta = new EventMetaConfig();
            Logging = new LoggingConfig();
            Variables = new VariablesConfig();
            StreamSelection = new StreamSelectionConfig();
            TimeSource = new TimeSourceConfig();
            MetricsReporting = new ConfigurationMetricsReporting();
            Language = new LanguageConfig();
            Expression = new ExpressionConfig();
            Execution = new ExecutionConfig();
            ExceptionHandling = new ExceptionHandlingConfig();
            ConditionHandling = new ConditionHandlingConfig();
            AlternativeContext = new AlternativeContextConfig();
            Patterns = new PatternsConfig();
            MatchRecognize = new MatchRecognizeConfig();
            Scripts = new ScriptsConfig();
        }

        /// <summary>
        /// Returns threading settings.
        /// </summary>
        /// <value>threading settings object</value>
        public ThreadingConfig Threading { get; private set; }

        /// <summary>
        /// Returns view resources defaults.
        /// </summary>
        /// <value>view resources defaults</value>
        public ViewResourcesConfig ViewResources { get; private set; }

        /// <summary>
        /// Returns event representation default settings.
        /// </summary>
        /// <value>event representation default settings</value>
        public EventMetaConfig EventMeta { get; private set; }

        /// <summary>
        /// Returns logging settings applicable to the engine, other then Log4J settings.
        /// </summary>
        /// <value>logging settings</value>
        public LoggingConfig Logging { get; private set; }

        /// <summary>
        /// Returns engine defaults applicable to variables.
        /// </summary>
        /// <value>variable engine defaults</value>
        public VariablesConfig Variables { get; private set; }

        /// <summary>
        /// Returns engine defaults applicable to streams (insert and remove, insert only or remove only) selected for a statement.
        /// </summary>
        /// <value>stream selection defaults</value>
        public StreamSelectionConfig StreamSelection { get; private set; }

        /// <summary>
        /// Returns the time source configuration.
        /// </summary>
        /// <value>time source enum</value>
        public TimeSourceConfig TimeSource { get; private set; }

        /// <summary>
        /// Returns the metrics reporting configuration.
        /// </summary>
        /// <value>metrics reporting config</value>
        public ConfigurationMetricsReporting MetricsReporting { get; private set; }

        /// <summary>
        /// Returns the language-related settings for the engine.
        /// </summary>
        /// <value>language-related settings</value>
        public LanguageConfig Language { get; private set; }

        /// <summary>
        /// Returns the expression-related settings for the engine.
        /// </summary>
        /// <value>expression-related settings</value>
        public ExpressionConfig Expression { get; private set; }

        /// <summary>
        /// Returns statement execution-related settings, settings that
        /// influence event/schedule to statement processing.
        /// </summary>
        /// <value>execution settings</value>
        public ExecutionConfig Execution { get; private set; }

        /// <summary>
        /// For software-provider-interface use.
        /// </summary>
        /// <value>alternative context</value>
        public AlternativeContextConfig AlternativeContext { get; set; }

        /// <summary>
        /// Returns the exception handling configuration.
        /// </summary>
        /// <value>exception handling configuration</value>
        public ExceptionHandlingConfig ExceptionHandling { get; set; }

        /// <summary>
        /// Returns the condition handling configuration.
        /// </summary>
        /// <value>condition handling configuration</value>
        public ConditionHandlingConfig ConditionHandling { get; set; }

        /// <summary>
        /// Return pattern settings.
        /// </summary>
        /// <value>pattern settings</value>
        public PatternsConfig Patterns { get; set; }

        /// <summary>
        /// Return match-recognize settings.
        /// </summary>
        /// <value>match-recognize settings</value>
        public MatchRecognizeConfig MatchRecognize { get; set; }

        /// <summary>
        /// Returns script engine settings.
        /// </summary>
        /// <value>script engine settings</value>
        public ScriptsConfig Scripts { get; set; }

        /// <summary>Threading profile.</summary>
        public enum ThreadingProfile
        {
            /// <summary>
            /// Large for use with 100 threads or more. Please see the documentation for more information.
            /// </summary>
            LARGE,

            /// <summary>For use with 100 threads or less.</summary>
            NORMAL
        }

        /// <summary>Filter service profile.</summary>
        public enum FilterServiceProfile
        {
            /// <summary>If filters are mostly static, the default.</summary>
            READMOSTLY,

            /// <summary>
            /// For very dynamic filters that come and go in a highly threaded environment.
            /// </summary>
            READWRITE
        }

        /// <summary>TimeInMillis source type.</summary>
        public enum TimeSourceType
        {
            /// <summary>
            /// Millisecond time source type with time originating from System.currentTimeMillis
            /// </summary>
            MILLI,

            /// <summary>
            /// Nanosecond time source from a wallclock-adjusted System.nanoTime
            /// </summary>
            NANO
        }

        /// <summary>Interface for cluster configurator.</summary>
        public interface ClusterConfigurator
        {
            /// <summary>
            /// Provide cluster configuration information.
            /// </summary>
            /// <param name="configuration">information</param>
            void Configure(Configuration configuration);
        }

        /// <summary>Holds threading settings.</summary>
        [Serializable]
        public class ThreadingConfig
        {
            /// <summary>Ctor - sets up defaults.</summary>
            public ThreadingConfig()
            {
                ListenerDispatchTimeout = 1000;
                IsListenerDispatchPreserveOrder = true;
                ListenerDispatchLocking = Locking.SPIN;

                InsertIntoDispatchTimeout = 100;
                IsInsertIntoDispatchPreserveOrder = true;
                InsertIntoDispatchLocking = Locking.SPIN;

                NamedWindowConsumerDispatchTimeout = long.MaxValue;
                IsNamedWindowConsumerDispatchPreserveOrder = true;
                NamedWindowConsumerDispatchLocking = Locking.SPIN;

                IsInternalTimerEnabled = true;
                InternalTimerMsecResolution = 100;

                IsThreadPoolInbound = false;
                IsThreadPoolOutbound = false;
                IsThreadPoolRouteExec = false;
                IsThreadPoolTimerExec = false;

                ThreadPoolTimerExecNumThreads = 2;
                ThreadPoolInboundNumThreads = 2;
                ThreadPoolRouteExecNumThreads = 2;
                ThreadPoolOutboundNumThreads = 2;

                ThreadPoolInboundBlocking = Locking.SUSPEND;
                ThreadPoolOutboundBlocking = Locking.SUSPEND;
                ThreadPoolRouteExecBlocking = Locking.SUSPEND;
                ThreadPoolTimerExecBlocking = Locking.SUSPEND;
            }

            /// <summary>
            /// Returns true to indicate preserve order for dispatch to listeners,
            /// or false to indicate not to preserve order
            /// </summary>
            /// <value>true or false</value>
            public bool IsListenerDispatchPreserveOrder { get; set; }

            /// <summary>
            /// Returns the timeout in millisecond to wait for listener code to complete
            /// before dispatching the next result, if dispatch order is preserved
            /// </summary>
            /// <value>listener dispatch timeout</value>
            public long ListenerDispatchTimeout { get; set; }

            /// <summary>
            /// Returns true to indicate preserve order for inter-statement insert-into,
            /// or false to indicate not to preserve order
            /// </summary>
            /// <value>true or false</value>
            public bool IsInsertIntoDispatchPreserveOrder { get; set; }

            /// <summary>
            /// Returns true if internal timer is enabled (the default), or false for internal timer disabled.
            /// </summary>
            /// <value>
            ///   true for internal timer enabled, false for internal timer disabled
            /// </value>
            public bool IsInternalTimerEnabled { get; set; }

            /// <summary>
            /// Returns the millisecond resolutuion of the internal timer thread.
            /// </summary>
            /// <value>number of msec between timer processing intervals</value>
            public long InternalTimerMsecResolution { get; set; }

            /// <summary>
            /// Returns the number of milliseconds that a thread may maximually be blocking
            /// to deliver statement results from a producing statement that employs insert-into
            /// to a consuming statement.
            /// </summary>
            /// <value>
            ///   millisecond timeout for order-of-delivery blocking between statements
            /// </value>
            public long InsertIntoDispatchTimeout { get; set; }

            /// <summary>
            /// Returns the blocking strategy to use when multiple threads deliver results for
            /// a single statement to listeners, and the guarantee of order of delivery must be maintained.
            /// </summary>
            /// <value>is the blocking technique</value>
            public Locking ListenerDispatchLocking { get; set; }

            /// <summary>
            /// Returns the blocking strategy to use when multiple threads deliver results for
            /// a single statement to consuming statements of an insert-into, and the guarantee of order of delivery must be maintained.
            /// </summary>
            /// <value>is the blocking technique</value>
            public Locking InsertIntoDispatchLocking { get; set; }

            /// <summary>
            /// Returns true for inbound threading enabled, the default is false for not enabled.
            /// </summary>
            /// <value>indicator whether inbound threading is enabled</value>
            public bool IsThreadPoolInbound { get; set; }

            /// <summary>
            /// Returns true for timer execution threading enabled, the default is false for not enabled.
            /// </summary>
            /// <value>indicator whether timer execution threading is enabled</value>
            public bool IsThreadPoolTimerExec { get; set; }

            /// <summary>
            /// Returns true for route execution threading enabled, the default is false for not enabled.
            /// </summary>
            /// <value>indicator whether route execution threading is enabled</value>
            public bool IsThreadPoolRouteExec { get; set; }

            /// <summary>
            /// Returns true for outbound threading enabled, the default is false for not enabled.
            /// </summary>
            /// <value>indicator whether outbound threading is enabled</value>
            public bool IsThreadPoolOutbound { get; set; }

            /// <summary>
            /// Returns the number of thread in the inbound threading pool.
            /// </summary>
            /// <value>number of threads</value>
            public int ThreadPoolInboundNumThreads { get; set; }

            /// <summary>
            /// Returns the number of thread in the outbound threading pool.
            /// </summary>
            /// <value>number of threads</value>
            public int ThreadPoolOutboundNumThreads { get; set; }

            /// <summary>
            /// Returns the number of thread in the route execution thread pool.
            /// </summary>
            /// <value>number of threads</value>
            public int ThreadPoolRouteExecNumThreads { get; set; }

            /// <summary>
            /// Returns the number of thread in the timer execution threading pool.
            /// </summary>
            /// <value>number of threads</value>
            public int ThreadPoolTimerExecNumThreads { get; set; }

            /// <summary>
            /// Returns the capacity of the timer execution queue, or null if none defined (the unbounded case, default).
            /// </summary>
            /// <value>capacity or null if none defined</value>
            public int? ThreadPoolTimerExecCapacity { get; set; }

            /// <summary>
            /// Returns the capacity of the inbound execution queue, or null if none defined (the unbounded case, default).
            /// </summary>
            /// <value>capacity or null if none defined</value>
            public int? ThreadPoolInboundCapacity { get; set; }

            /// <summary>
            /// Returns the capacity of the route execution queue, or null if none defined (the unbounded case, default).
            /// </summary>
            /// <value>capacity or null if none defined</value>
            public int? ThreadPoolRouteExecCapacity { get; set; }

            /// <summary>
            /// Returns the capacity of the outbound queue, or null if none defined (the unbounded case, default).
            /// </summary>
            /// <value>capacity or null if none defined</value>
            public int? ThreadPoolOutboundCapacity { get; set; }

            public Locking ThreadPoolInboundBlocking { get; set; }
            public Locking ThreadPoolOutboundBlocking { get; set; }
            public Locking ThreadPoolTimerExecBlocking { get; set; }
            public Locking ThreadPoolRouteExecBlocking { get; set; }

            /// <summary>
            /// Returns true if the engine-level lock is configured as a fair lock (default is false).
            /// <para>
            /// This lock coordinates
            /// event processing threads (threads that send events) with threads that
            /// perform administrative functions (threads that start or destroy statements, for example).
            /// </para>
            /// </summary>
            /// <value>true for fair lock</value>
            public bool IsEngineFairlock { get; set; }

            /// <summary>
            /// In multithreaded environments, this setting controls whether named window dispatches to named window consumers preserve
            /// the order of events inserted and removed such that statements that consume a named windows delta stream
            /// behave deterministic (true by default).
            /// </summary>
            /// <value>flag</value>
            public bool IsNamedWindowConsumerDispatchPreserveOrder { get; set; }

            /// <summary>
            /// Returns the timeout millisecond value for named window dispatches to named window consumers.
            /// </summary>
            /// <value>timeout milliseconds</value>
            public long NamedWindowConsumerDispatchTimeout { get; set; }

            /// <summary>
            /// Returns the locking strategy value for named window dispatches to named window consumers (default is spin).
            /// </summary>
            /// <value>strategy</value>
            public Locking NamedWindowConsumerDispatchLocking { get; set; }

            /// <summary>Enumeration of blocking techniques.</summary>
            public enum Locking
            {
                /// <summary>
                /// Spin lock blocking is good for locks held very shortly or generally uncontended locks and
                /// is therefore the default.
                /// </summary>
                SPIN,

                /// <summary>
                /// Blocking that suspends a thread and notifies a thread to wake up can be
                /// more expensive then spin locks.
                /// </summary>
                SUSPEND
            }
        }

        /// <summary>Holds view resources settings.</summary>
        [Serializable]
        public class ViewResourcesConfig
        {
            /// <summary>Ctor - sets up defaults.</summary>
            public ViewResourcesConfig()
            {
                IsShareViews = false;
                IsAllowMultipleExpiryPolicies = false;
                IsIterableUnbound = false;
            }

            /// <summary>
            /// Returns false to indicate the engine does not implicitly share similar view resources between statements (false is the default),
            /// or true to indicate that the engine may implicitly share view resources between statements.
            /// </summary>
            /// <value>
            ///   indicator whether view resources are shared between statements if
            ///   statements share same-views and the engine sees opportunity to reuse an existing view.
            /// </value>
            public bool IsShareViews { get; set; }

            /// <summary>
            /// By default this setting is false and thereby multiple expiry policies
            /// provided by views can only be combined if any of the retain-keywords is also specified for the stream.
            /// <para>
            /// If set to true then multiple expiry policies are allowed and the following statement compiles without exception:
            /// "select * from MyEvent#time(10)#time(10)".
            /// </para>
            /// </summary>
            /// <value>
            ///   allowMultipleExpiryPolicies indicator whether to allow combining expiry policies provided by views
            /// </value>
            public bool IsAllowMultipleExpiryPolicies { get; set; }

            /// <summary>
            /// Returns flag to indicate whether engine-wide unbound statements are iterable and return the last event.
            /// </summary>
            /// <value>indicator</value>
            public bool IsIterableUnbound { get; set; }
        }

        /// <summary>Event representation metadata.</summary>
        [Serializable]
        public class EventMetaConfig
        {
            /// <summary>Ctor.</summary>
            public EventMetaConfig()
            {
                AnonymousCacheSize = 5;
                ClassPropertyResolutionStyle = PropertyResolutionStyle.DEFAULT;
                DefaultAccessorStyle = AccessorStyleEnum.NATIVE;
                DefaultEventRepresentation = EventUnderlyingTypeExtensions.GetDefault();
                AvroSettings = new AvroSettings();
            }

            /// <summary>
            /// Returns the default accessor style, native unless changed.
            /// </summary>
            /// <value>style enum</value>
            public AccessorStyleEnum DefaultAccessorStyle { get; set; }

            /// <summary>
            /// Returns the property resolution style to use for resolving property names
            /// of classes.
            /// </summary>
            /// <value>style of property resolution</value>
            public PropertyResolutionStyle ClassPropertyResolutionStyle { get; set; }

            /// <summary>
            /// Returns the default event representation.
            /// </summary>
            /// <value>setting</value>
            public EventUnderlyingType DefaultEventRepresentation { get; set; }

            /// <summary>
            /// Returns the cache size for anonymous event types.
            /// </summary>
            /// <value>cache size</value>
            public int AnonymousCacheSize { get; set; }

            /// <summary>
            /// Returns the Avro settings.
            /// </summary>
            /// <value>avro settings</value>
            public AvroSettings AvroSettings { get; set; }
        }

        /// <summary>Avro settings.</summary>
        [Serializable]
        public class AvroSettings
        {
            public AvroSettings()
            {
                IsEnableSchemaDefaultNonNull = true;
                IsEnableNativeString = true;
                IsEnableAvro = true;
            }

            /// <summary>
            /// Returns the indicator whether Avro support is enabled when available (true by default).
            /// </summary>
            /// <value>indicator</value>
            public bool IsEnableAvro { get; set; }

            /// <summary>
            /// Returns indicator whether for string-type values to use the "avro.string=string" (true by default)
            /// </summary>
            /// <value>indicator</value>
            public bool IsEnableNativeString { get; set; }

            /// <summary>
            /// Returns indicator whether generated schemas should assume non-null values (true by default)
            /// </summary>
            /// <value>indicator</value>
            public bool IsEnableSchemaDefaultNonNull { get; set; }

            /// <summary>
            /// Returns class name of mapping provider that maps types to an Avro schema; a mapper should implement <seealso cref="TypeRepresentationMapper" />
            /// (null by default, using default mapping)
            /// </summary>
            /// <value>class name</value>
            public string TypeRepresentationMapperClass { get; set; }

            /// <summary>
            /// Returns the class name of widening provider that widens, coerces or transforms object values to an Avro field value or record; a widener should implement <seealso cref="ObjectValueTypeWidenerFactory" />
            /// (null by default, using default widening)
            /// </summary>
            /// <value>class name</value>
            public string ObjectValueTypeWidenerFactoryClass { get; set; }
        }

        /// <summary>
        /// Holds view logging settings other then the Apache commons or Log4 settings.
        /// </summary>
        [Serializable]
        public class LoggingConfig
        {
            /// <summary>Ctor - sets up defaults.</summary>
            public LoggingConfig()
            {
                IsEnableExecutionDebug = false;
                IsEnableTimerDebug = true;
                IsEnableQueryPlan = false;
                IsEnableADO = false;
            }

            /// <summary>
            /// Returns true if execution path debug logging is enabled.
            /// <para>
            /// Only if this flag is set to true, in addition to LOG4 settings set to DEBUG, does an engine instance,
            /// produce debug out.
            /// </para>
            /// </summary>
            /// <value>
            ///   true if debug logging through Log4 is enabled for any event processing execution paths
            /// </value>
            public bool IsEnableExecutionDebug { get; set; }

            /// <summary>
            /// Returns true if timer debug level logging is enabled (true by default).
            /// <para>
            /// Set this value to false to reduce the debug-level logging output for the timer Thread(s).
            /// For use only when debug-level logging is enabled.
            /// </para>
            /// </summary>
            /// <value>indicator whether timer execution is noisy in debug or not</value>
            public bool IsEnableTimerDebug { get; set; }

            /// <summary>
            /// Returns indicator whether query plan logging is enabled or not.
            /// </summary>
            /// <value>indicator</value>
            public bool IsEnableQueryPlan { get; set; }

            /// <summary>
            /// Returns an indicator whether ADO query reporting is enabled.
            /// </summary>
            /// <value>indicator</value>
            public bool IsEnableADO { get; set; }

            /// <summary>
            /// Returns the pattern that formats audit logs.
            /// <para>
            /// Available conversion characters are:
            /// </para>
            /// <para>
            /// %m      - Used to output the audit message.
            /// %s      - Used to output the statement name.
            /// %u      - Used to output the engine URI.
            /// </para>
            /// </summary>
            /// <value>audit formatting pattern</value>
            public string AuditPattern { get; set; }
        }

        /// <summary>Holds variables settings.</summary>
        [Serializable]
        public class VariablesConfig
        {
            /// <summary>Ctor - sets up defaults.</summary>
            public VariablesConfig()
            {
                MsecVersionRelease = 15000;
            }

            /// <summary>
            /// Returns the number of milliseconds that a version of a variables is held stable for
            /// use by very long-running atomic statement execution.
            /// <para>
            /// A slow-executing statement such as an SQL join may use variables that, at the time
            /// the statement starts to execute, have certain values. The engine guarantees that during
            /// statement execution the value of the variables stays the same as long as the statement
            /// does not take longer then the given number of milliseconds to execute. If the statement does take longer
            /// to execute then the variables release time, the current variables value applies instead.
            /// </para>
            /// </summary>
            /// <value>
            ///   millisecond time interval that a variables version is guaranteed to be stable
            ///   in the context of an atomic statement execution
            /// </value>
            public long MsecVersionRelease { get; set; }
        }

        /// <summary>Holder for script settings.</summary>
        [Serializable]
        public class ScriptsConfig
        {
            public ScriptsConfig()
            {
                DefaultDialect = "jscript";
            }

            /// <summary>
            /// Returns the default script dialect.
            /// </summary>
            /// <value>dialect</value>
            public string DefaultDialect { get; set; }
        }

        /// <summary>Holds pattern settings.</summary>
        [Serializable]
        public class PatternsConfig
        {
            public PatternsConfig()
            {
                IsMaxSubexpressionPreventStart = true;
            }

            /// <summary>
            /// Returns the maximum number of subexpressions
            /// </summary>
            /// <value>subexpression count</value>
            public long? MaxSubexpressions { get; set; }

            /// <summary>
            /// Returns true, the default, to indicate that if there is a maximum defined
            /// it is being enforced and new subexpressions are not allowed.
            /// </summary>
            /// <value>indicate whether enforced or not</value>
            public bool IsMaxSubexpressionPreventStart { get; set; }
        }

        /// <summary>Holds match-reconize settings.</summary>
        [Serializable]
        public class MatchRecognizeConfig
        {
            public MatchRecognizeConfig()
            {
                IsMaxStatesPreventStart = true;
            }

            /// <summary>
            /// Returns the maximum number of states
            /// </summary>
            /// <value>state count</value>
            public long? MaxStates { get; set; }

            /// <summary>
            /// Returns true, the default, to indicate that if there is a maximum defined
            /// it is being enforced and new states are not allowed.
            /// </summary>
            /// <value>indicate whether enforced or not</value>
            public bool IsMaxStatesPreventStart { get; set; }
        }

        /// <summary>
        /// Holds default settings for stream selection in the select-clause.
        /// </summary>
        [Serializable]
        public class StreamSelectionConfig
        {
            /// <summary>Ctor - sets up defaults.</summary>
            public StreamSelectionConfig()
            {
                DefaultStreamSelector = StreamSelector.ISTREAM_ONLY;
            }

            /// <summary>
            /// Returns the default stream selector.
            /// <para>
            /// Statements that select data from streams and that do not use one of the explicit stream
            /// selection keywords (istream/rstream/irstream), by default,
            /// generate selection results for the insert stream only, and not for the remove stream.
            /// </para>
            /// <para>
            /// This setting can be used to change the default behavior: Use the RSTREAM_ISTREAM_BOTH
            /// value to have your statements generate both insert and remove stream results
            /// without the use of the "irstream" keyword in the select clause.
            /// </para>
            /// </summary>
            /// <value>
            ///   default stream selector, which is ISTREAM_ONLY unless changed
            /// </value>
            public StreamSelector DefaultStreamSelector { get; set; }
        }

        /// <summary>
        /// TimeInMillis source configuration, the default in MILLI (millisecond resolution from System.currentTimeMillis).
        /// </summary>
        [Serializable]
        public class TimeSourceConfig
        {
            /// <summary>Ctor.</summary>
            public TimeSourceConfig()
            {
                TimeUnit = TimeUnit.MILLISECONDS;
                TimeSourceType = TimeSourceType.MILLI;
            }

            /// <summary>
            /// Returns the time source type.
            /// </summary>
            /// <value>time source type enum</value>
            public TimeSourceType TimeSourceType { get; set; }

            /// <summary>
            /// Returns the time unit time resolution level of time tracking
            /// </summary>
            /// <value>time resolution</value>
            public TimeUnit TimeUnit { get; set; }
        }

        /// <summary>Language settings in the engine are for string comparisons.</summary>
        [Serializable]
        public class LanguageConfig
        {
            /// <summary>Ctor.</summary>
            public LanguageConfig()
            {
                IsSortUsingCollator = false;
            }

            /// <summary>
            /// Returns true to indicate to perform locale-independent string comparisons using Collator.
            /// <para>
            /// By default this setting is false, i.e. string comparisons use the compare method.
            /// </para>
            /// </summary>
            /// <value>indicator whether to use Collator for string comparisons</value>
            public bool IsSortUsingCollator { get; set; }
        }

        /// <summary>
        /// Expression evaluation settings in the engine are for results of expressions.
        /// </summary>
        [Serializable]
        public class ExpressionConfig
        {
            /// <summary>Ctor.</summary>
            public ExpressionConfig()
            {
                IsIntegerDivision = false;
                IsDivisionByZeroReturnsNull = false;
                IsUdfCache = true;
                IsSelfSubselectPreeval = true;
                IsExtendedAggregation = true;
                TimeZone = TimeZoneInfo.Local;
            }

            /// <summary>
            /// Returns false (the default) for integer division returning double values.
            /// <para>
            /// Returns true to signal that Java-convention integer division semantics
            /// are used for divisions, whereas the division between two non-FP numbers
            /// returns only the whole number part of the result and any fractional part is dropped.
            /// </para>
            /// </summary>
            /// <value>indicator</value>
            public bool IsIntegerDivision { get; set; }

            /// <summary>
            /// Returns false (default) when division by zero returns double?.Infinity.
            /// Returns true when division by zero return null.
            /// <para>
            /// If integer devision is set, then division by zero for non-FP operands also returns null.
            /// </para>
            /// </summary>
            /// <value>indicator for division-by-zero results</value>
            public bool IsDivisionByZeroReturnsNull { get; set; }

            /// <summary>
            /// By default true, indicates that user-defined functions cache return results
            /// if the parameter set is empty or has constant-only return values.
            /// </summary>
            /// <value>cache flag</value>
            public bool IsUdfCache { get; set; }

            /// <summary>
            /// Set to true (the default) to indicate that sub-selects within a statement are updated first when a new
            /// event arrives. This is only relevant for statements in which both subselects
            /// and the from-clause may react to the same exact event.
            /// </summary>
            /// <value>
            ///   indicator whether to evaluate sub-selects first or last on new event arrival
            /// </value>
            public bool IsSelfSubselectPreeval { get; set; }

            /// <summary>
            /// Enables or disables non-SQL standard builtin aggregation functions.
            /// </summary>
            /// <value>indicator</value>
            public bool IsExtendedAggregation { get; set; }

            /// <summary>
            /// Returns true to indicate that duck typing is enable for the specific syntax where it is allowed (check the documentation).
            /// </summary>
            /// <value>indicator</value>
            public bool IsDuckTyping { get; set; }

            /// <summary>
            /// Returns the math context for big decimal operations, or null to leave the math context undefined.
            /// </summary>
            /// <value>math context or null</value>
            public MathContext MathContext { get; set; }

            /// <summary>
            /// Returns the time zone for calendar operations.
            /// </summary>
            /// <value>time zone</value>
            public TimeZoneInfo TimeZone { get; set; }
        }

        /// <summary>Holds engine execution-related settings.</summary>
        [Serializable]
        public class ExecutionConfig
        {
            /// <summary>Ctor - sets up defaults.</summary>
            public ExecutionConfig()
            {
                ThreadingProfile = ThreadingProfile.NORMAL;
                FilterServiceProfile = FilterServiceProfile.READMOSTLY;
                FilterServiceMaxFilterWidth = 16;
                DeclaredExprValueCacheSize = 1;
                IsPrioritized = false;
                CodeGeneration = new CodeGeneration();
            }

            /// <summary>
            /// Returns false (the default) if the engine does not consider statement priority and preemptive instructions,
            /// or true to enable priority-based statement execution order.
            /// </summary>
            /// <value>
            ///   false by default to indicate unprioritized statement execution
            /// </value>
            public bool IsPrioritized { get; set; }

            /// <summary>
            /// Returns true for fair locking, false for unfair locks.
            /// </summary>
            /// <value>fairness flag</value>
            public bool IsFairlock { get; set; }

            /// <summary>
            /// Returns indicator whether statement-level locks are disabled.
            /// The default is false meaning statement-level locks are taken by default and depending on EPL optimizations.
            /// If set to true statement-level locks are never taken.
            /// </summary>
            /// <value>indicator for statement-level locks</value>
            public bool IsDisableLocking { get; set; }

            /// <summary>
            /// Returns the threading profile
            /// </summary>
            /// <value>profile</value>
            public ThreadingProfile ThreadingProfile { get; set; }

            /// <summary>
            /// Returns indicator whether isolated services providers are enabled or disabled (the default).
            /// </summary>
            /// <value>indicator value</value>
            public bool IsAllowIsolatedService { get; set; }

            /// <summary>
            /// Returns the filter service profile for tuning filtering operations.
            /// </summary>
            /// <value>filter service profile</value>
            public FilterServiceProfile FilterServiceProfile { get; set; }

            /// <summary>
            /// Returns the maximum width for breaking up "or" expression in filters to
            /// subexpressions for reverse indexing.
            /// </summary>
            /// <value>max filter width</value>
            public int FilterServiceMaxFilterWidth { get; set; }

            /// <summary>
            /// Returns the cache size for declared expression values
            /// </summary>
            /// <value>value</value>
            public int DeclaredExprValueCacheSize { get; set; }

            /// <summary>
            /// Gets or sets the code generation.
            /// </summary>
            /// <value>
            /// The code generation.
            /// </value>
            public CodeGeneration CodeGeneration { get; set; }
        }

        /// <summary>
        /// Returns the provider for runtime and administrative interfaces.
        /// </summary>
        [Serializable]
        public class AlternativeContextConfig
        {
            /// <summary>
            /// Type name of runtime provider.
            /// </summary>
            /// <value>provider class</value>
            public string Runtime { get; set; }

            /// <summary>
            /// Type name of admin provider.
            /// </summary>
            /// <value>provider class</value>
            public string Admin { get; set; }

            /// <summary>
            /// Returns the class name of the event type id generator.
            /// </summary>
            /// <value>class name</value>
            public string EventTypeIdGeneratorFactory { get; set; }

            /// <summary>
            /// Returns the class name of the virtual data window view factory.
            /// </summary>
            /// <value>factory class name</value>
            public string VirtualDataWindowViewFactory { get; set; }

            /// <summary>
            /// Sets the class name of the statement metadata factory.
            /// </summary>
            /// <value>factory class name</value>
            public string StatementMetadataFactory { get; set; }

            /// <summary>
            /// Returns the application-provided configurarion object carried as part of the configurations.
            /// </summary>
            /// <value>config user object</value>
            public object UserConfiguration { get; set; }

            /// <summary>
            /// Returns the member name.
            /// </summary>
            /// <value>member name</value>
            public string MemberName { get; set; }
        }

        /// <summary>
        /// Configuration object for defining exception handling behavior.
        /// </summary>
        [Serializable]
        public class ExceptionHandlingConfig
        {
            private List<string> _handlerFactories;

            public ExceptionHandlingConfig()
            {
                UndeployRethrowPolicy = UndeployRethrowPolicy.WARN;
            }

            /// <summary>
            /// Returns the list of exception handler factory class names,
            /// see <seealso cref="com.espertech.esper.client.hook.ExceptionHandlerFactory" />
            /// </summary>
            /// <value>list of fully-qualified class names</value>
            public IList<string> HandlerFactories
            {
                get { return _handlerFactories; }
            }

            /// <summary>
            /// Add an exception handler factory class name.
            /// <para>
            /// Provide a fully-qualified class name of the implementation
            /// of the <seealso cref="com.espertech.esper.client.hook.ExceptionHandlerFactory" />
            /// interface.
            /// </para>
            /// </summary>
            /// <param name="exceptionHandlerFactoryClassName">class name of exception handler factory</param>
            public void AddClass(string exceptionHandlerFactoryClassName)
            {
                if (_handlerFactories == null)
                {
                    _handlerFactories = new List<string>();
                }
                _handlerFactories.Add(exceptionHandlerFactoryClassName);
            }

            /// <summary>
            /// Add a list of exception handler class names.
            /// </summary>
            /// <param name="classNames">to add</param>
            public void AddClasses(IEnumerable<string> classNames)
            {
                if (_handlerFactories == null)
                {
                    _handlerFactories = new List<string>();
                }
                _handlerFactories.AddAll(classNames);
            }

            /// <summary>
            /// Add an exception handler factory class.
            /// <para>
            /// The class provided should implement the
            /// <seealso cref="com.espertech.esper.client.hook.ExceptionHandlerFactory" />
            /// interface.
            /// </para>
            /// </summary>
            /// <param name="exceptionHandlerFactoryClass">class of implementation</param>
            public void AddClass(Type exceptionHandlerFactoryClass)
            {
                AddClass(exceptionHandlerFactoryClass.AssemblyQualifiedName);
            }

            /// <summary>
            /// Add an exception handler factory class.
            /// </summary>
            /// <typeparam name="T"></typeparam>
            public void AddClass<T>() where T : ExceptionHandlerFactory
            {
                AddClass(typeof(T).AssemblyQualifiedName);
            }

            /// <summary>
            /// Returns the policy to instruct the engine whether a module un-deploy rethrows runtime exceptions that are encountered
            /// during the undeploy for any statement that is undeployed. By default we are logging exceptions.
            /// </summary>
            /// <value>indicator</value>
            public UndeployRethrowPolicy UndeployRethrowPolicy { get; set; }
        }

        /// <summary>Enumeration of blocking techniques.</summary>
        public enum UndeployRethrowPolicy
        {
            /// <summary>Warn.</summary>
            WARN,

            /// <summary>Rethrow First Encountered Exception.</summary>
            RETHROW_FIRST
        }

        /// <summary>
        /// Configuration object for defining condition handling behavior.
        /// </summary>
        [Serializable]
        public class ConditionHandlingConfig
        {
            private List<string> _handlerFactories;

            /// <summary>
            /// Returns the list of condition handler factory class names,
            /// see <seealso cref="com.espertech.esper.client.hook.ConditionHandlerFactory" />
            /// </summary>
            /// <value>list of fully-qualified class names</value>
            public IList<string> HandlerFactories
            {
                get { return _handlerFactories; }
            }

            /// <summary>
            /// Add an condition handler factory class name.
            /// <para>
            /// Provide a fully-qualified class name of the implementation
            /// of the <seealso cref="com.espertech.esper.client.hook.ConditionHandlerFactory" />
            /// interface.
            /// </para>
            /// </summary>
            /// <param name="className">class name of condition handler factory</param>
            public void AddClass(string className)
            {
                if (_handlerFactories == null)
                {
                    _handlerFactories = new List<string>();
                }
                _handlerFactories.Add(className);
            }

            /// <summary>
            /// Add a list of condition handler class names.
            /// </summary>
            /// <param name="classNames">to add</param>
            public void AddClasses(IEnumerable<string> classNames)
            {
                if (_handlerFactories == null)
                {
                    _handlerFactories = new List<string>();
                }
                _handlerFactories.AddAll(classNames);
            }

            /// <summary>
            /// Add a condition handler factory class.
            /// <para>
            /// The class provided should implement the
            /// <seealso cref="com.espertech.esper.client.hook.ConditionHandlerFactory" />
            /// interface.
            /// </para>
            /// </summary>
            /// <param name="clazz">class of implementation</param>
            public void AddClass(Type clazz)
            {
                AddClass(clazz.AssemblyQualifiedName);
            }

            /// <summary>
            /// Add a condition handler factory class.
            /// </summary>
            /// <typeparam name="T"></typeparam>
            public void AddClass<T>() where T : ConditionHandlerFactory
            {
                AddClass(typeof(T).AssemblyQualifiedName);
            }
        }

        /// <summary>
        /// Code generation settings
        /// </summary>
        [Serializable]
        public class CodeGeneration
        {
            private bool enablePropertyGetter = false;

            /// <summary>
            /// Returns indicator whether to enable code generation for event property getters (false by default).
            /// </summary>
            public bool IsEnablePropertyGetter
            {
                get => enablePropertyGetter;
                set => enablePropertyGetter = value;
            }
        }
    }
} // end of namespace
