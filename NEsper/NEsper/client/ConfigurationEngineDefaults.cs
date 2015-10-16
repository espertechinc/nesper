///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client.soda;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.client
{
    /// <summary>Provides access to engine configuration defaults for modification. </summary>
    [Serializable]
    public class ConfigurationEngineDefaults 
    {
        /// <summary>Ctor. </summary>
        public ConfigurationEngineDefaults()
        {
            ThreadingConfig = new Threading();
            ViewResourcesConfig = new ViewResources();
            EventMetaConfig = new EventMeta();
            LoggingConfig = new Logging();
            VariablesConfig = new Variables();
            StreamSelectionConfig = new StreamSelection();
            TimeSourceConfig = new TimeSource();
            MetricsReportingConfig = new ConfigurationMetricsReporting();
            LanguageConfig = new Language();
            ExpressionConfig = new Expression();
            ExecutionConfig = new Execution();
            ExceptionHandlingConfig = new ExceptionHandling();
            ConditionHandlingConfig = new ConditionHandling();
            AlternativeContextConfig = new AlternativeContext();
            ClusterConfig = new Cluster();
            PatternsConfig = new Patterns();
            MatchRecognizeConfig = new MatchRecognize();
            ScriptsConfig = new Scripts();
        }

        /// <summary>Returns threading settings. </summary>
        /// <value>threading settings object</value>
        public Threading ThreadingConfig { get; set; }

        /// <summary>Returns view resources defaults. </summary>
        /// <value>view resources defaults</value>
        public ViewResources ViewResourcesConfig { get; set; }

        /// <summary>Returns event representation default settings. </summary>
        /// <value>event representation default settings</value>
        public EventMeta EventMetaConfig { get; private set; }

        /// <summary>Returns logging settings applicable to the engine, other then Log4J settings. </summary>
        /// <value>logging settings</value>
        public Logging LoggingConfig { get; private set; }

        /// <summary>Returns engine defaults applicable to variables. </summary>
        /// <value>variable engine defaults</value>
        public Variables VariablesConfig { get; private set; }

        /// <summary>Returns engine defaults applicable to streams (insert and remove, insert only or remove only) selected for a statement. </summary>
        /// <value>stream selection defaults</value>
        public StreamSelection StreamSelectionConfig { get; private set; }

        /// <summary>Returns the time source configuration. </summary>
        /// <value>time source enum</value>
        public TimeSource TimeSourceConfig { get; private set; }

        /// <summary>Returns the metrics reporting configuration. </summary>
        /// <value>metrics reporting config</value>
        public ConfigurationMetricsReporting MetricsReportingConfig { get; private set; }

        /// <summary>Returns the language-related settings for the engine. </summary>
        /// <value>language-related settings</value>
        public Language LanguageConfig { get; private set; }

        /// <summary>Returns the expression-related settings for the engine. </summary>
        /// <value>expression-related settings</value>
        public Expression ExpressionConfig { get; private set; }

        /// <summary>Returns statement execution-related settings, settings that influence event/schedule to statement processing. </summary>
        /// <value>execution settings</value>
        public Execution ExecutionConfig { get; private set; }

        /// <summary>For software-provider-interface use. </summary>
        /// <value>alternative context</value>
        public AlternativeContext AlternativeContextConfig { get; set; }

        /// <summary>Returns the exception handling configuration. </summary>
        /// <value>exception handling configuration</value>
        public ExceptionHandling ExceptionHandlingConfig { get; set; }

        /// <summary>Returns the condition handling configuration. </summary>
        /// <value>condition handling configuration</value>
        public ConditionHandling ConditionHandlingConfig { get; set; }

        /// <summary>Returns cluster configuration. </summary>
        /// <value>cluster configuration</value>
        public Cluster ClusterConfig { get; set; }

        /// <summary>Return pattern settings. </summary>
        /// <value>pattern settings</value>
        public Patterns PatternsConfig { get; set; }

        /// <summary>Gets or sets the match-recognize settings.</summary>
        /// <value>The match-recognize settings.</value>
        public MatchRecognize MatchRecognizeConfig { get; set; }

        /// <summary>Returns script engine settings. </summary>
        /// <value>script engine settings</value>
        public Scripts ScriptsConfig { get; set; }

        /// <summary>Holds threading settings. </summary>
        [Serializable]
        public class Threading 
        {
            /// <summary>Ctor - sets up defaults. </summary>
            public Threading()
            {
                ListenerDispatchTimeout = 1000;
                IsListenerDispatchPreserveOrder = true;
                ListenerDispatchLocking = Locking.SPIN;
    
                InsertIntoDispatchTimeout = 100;
                IsInsertIntoDispatchPreserveOrder = true;
                InsertIntoDispatchLocking = Locking.SPIN;
    
                IsInternalTimerEnabled = true;
                InternalTimerMsecResolution = 100;

                ThreadLocalStyle = ThreadLocal.FAST;
    
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
            /// Gets or sets the thread local style.
            /// </summary>
            public ThreadLocal ThreadLocalStyle { get; set; }

            /// <summary>Returns true to indicate preserve order for dispatch to listeners, or false to indicate not to preserve order </summary>
            /// <value>true or false</value>
            public bool IsListenerDispatchPreserveOrder { get; set; }

            /// <summary>Returns the timeout in millisecond to wait for listener code to complete before dispatching the next result, if dispatch order is preserved </summary>
            /// <value>listener dispatch timeout</value>
            public long ListenerDispatchTimeout { get; set; }

            /// <summary>Returns true to indicate preserve order for inter-statement insert-into, or false to indicate not to preserve order </summary>
            /// <value>true or false</value>
            public bool IsInsertIntoDispatchPreserveOrder { get; set; }

            /// <summary>Returns true if internal timer is enabled (the default), or false for internal timer disabled. </summary>
            /// <value>true for internal timer enabled, false for internal timer disabled</value>
            public bool IsInternalTimerEnabled { get; set; }

            /// <summary>Returns the millisecond resolutuion of the internal timer thread. </summary>
            /// <value>number of msec between timer processing intervals</value>
            public long InternalTimerMsecResolution { get; set; }

            /// <summary>Returns the number of milliseconds that a thread may maximually be blocking to deliver statement results from a producing statement that employs insert-into to a consuming statement. </summary>
            /// <value>millisecond timeout for order-of-delivery blocking between statements</value>
            public long InsertIntoDispatchTimeout { get; set; }

            /// <summary>Returns the blocking strategy to use when multiple threads deliver results for a single statement to listeners, and the guarantee of order of delivery must be maintained. </summary>
            /// <value>is the blocking technique</value>
            public Locking ListenerDispatchLocking { get; set; }

            /// <summary>Returns the blocking strategy to use when multiple threads deliver results for a single statement to consuming statements of an insert-into, and the guarantee of order of delivery must be maintained. </summary>
            /// <value>is the blocking technique</value>
            public Locking InsertIntoDispatchLocking { get; set; }

            /// <summary>Returns true for inbound threading enabled, the default is false for not enabled. </summary>
            /// <value>indicator whether inbound threading is enabled</value>
            public bool IsThreadPoolInbound { get; set; }

            /// <summary>Returns true for timer execution threading enabled, the default is false for not enabled. </summary>
            /// <value>indicator whether timer execution threading is enabled</value>
            public bool IsThreadPoolTimerExec { get; set; }

            /// <summary>Returns true for route execution threading enabled, the default is false for not enabled. </summary>
            /// <value>indicator whether route execution threading is enabled</value>
            public bool IsThreadPoolRouteExec { get; set; }

            /// <summary>Returns true for outbound threading enabled, the default is false for not enabled. </summary>
            /// <value>indicator whether outbound threading is enabled</value>
            public bool IsThreadPoolOutbound { get; set; }

            /// <summary>Returns the number of thread in the inbound threading pool. </summary>
            /// <value>number of threads</value>
            public int ThreadPoolInboundNumThreads { get; set; }

            /// <summary>Returns the number of thread in the outbound threading pool. </summary>
            /// <value>number of threads</value>
            public int ThreadPoolOutboundNumThreads { get; set; }

            /// <summary>Returns the number of thread in the route execution thread pool. </summary>
            /// <value>number of threads</value>
            public int ThreadPoolRouteExecNumThreads { get; set; }

            /// <summary>Returns the number of thread in the timer execution threading pool. </summary>
            /// <value>number of threads</value>
            public int ThreadPoolTimerExecNumThreads { get; set; }

            /// <summary>Returns the capacity of the timer execution queue, or null if none defined (the unbounded case, default). </summary>
            /// <value>capacity or null if none defined</value>
            public int? ThreadPoolTimerExecCapacity { get; set; }

            /// <summary>Returns the capacity of the inbound execution queue, or null if none defined (the unbounded case, default). </summary>
            /// <value>capacity or null if none defined</value>
            public int? ThreadPoolInboundCapacity { get; set; }

            /// <summary>Returns the capacity of the route execution queue, or null if none defined (the unbounded case, default). </summary>
            /// <value>capacity or null if none defined</value>
            public int? ThreadPoolRouteExecCapacity { get; set; }

            /// <summary>Returns the capacity of the outbound queue, or null if none defined (the unbounded case, default). </summary>
            /// <value>capacity or null if none defined</value>
            public int? ThreadPoolOutboundCapacity { get; set; }

            public Locking ThreadPoolInboundBlocking { get; set; }
            public Locking ThreadPoolOutboundBlocking { get; set; }
            public Locking ThreadPoolTimerExecBlocking { get; set; }
            public Locking ThreadPoolRouteExecBlocking { get; set; }

            /// <summary>Returns true if the engine-level lock is configured as a fair lock (default is false). <para /> This lock coordinates event processing threads (threads that send events) with threads that perform administrative functions (threads that start or destroy statements, for example). </summary>
            /// <value>true for fair lock</value>
            public bool IsEngineFairlock { get; set; }

            /// <summary>Enumeration of blocking techniques. </summary>
            public enum Locking
            {
                /// <summary>Spin lock blocking is good for locks held very shortly or generally uncontended locks and is therefore the default. </summary>
                SPIN,
    
                /// <summary>Blocking that suspends a thread and notifies a thread to wake up can be more expensive then spin locks. </summary>
                SUSPEND
            }

            /// <summary>
            /// Enumeration of thread local techniques.
            /// </summary>
            public enum ThreadLocal
            {
                /// <summary>
                /// Uses customized thread local objects specifically designed for
                /// high-speed access.
                /// </summary>
                FAST,

                /// <summary>
                /// Uses LocalDataStoreSlot for thread local objects.  This uses the CLR's
                /// own mechanisms.
                /// </summary>
                SYSTEM
            }
        }
    
        /// <summary>Holds view resources settings. </summary>
        [Serializable]
        public class ViewResources 
        {
            /// <summary>Ctor - sets up defaults. </summary>
            internal ViewResources()
            {
                IsShareViews = true;
                IsAllowMultipleExpiryPolicies = false;
                IsIterableUnbound = false;
            }

            /// <summary>Returns true to indicate the engine shares view resources between statements, or false to indicate the engine does not share view resources between statements. </summary>
            /// <value>indicator whether view resources are shared between statements ifstatements share same-views and the engine sees opportunity to reuse an existing view. </value>
            public bool IsShareViews { get; set; }

            /// <summary>By default this setting is false and thereby multiple expiry policies provided by views can only be combined if any of the retain-keywords is also specified for the stream. <para /> If set to true then multiple expiry policies are allowed and the following statement compiles without exception: "select * from MyEvent.win:time(10).win:time(10)". </summary>
            /// <value>allowMultipleExpiryPolicies indicator whether to allow combining expiry policies provided by views</value>
            public bool IsAllowMultipleExpiryPolicies { get; set; }

            /// <summary>Returns true to indicate whether engine-wide unbound statements are iterable and return the last event.</summary>
            /// <value>indicate whether engine-wide unbound statements are iterable and return the last event.</value>
            public bool IsIterableUnbound { get; set; }
        }
    
        /// <summary>Event representation metadata. </summary>
        [Serializable]
        public class EventMeta
        {
            /// <summary>Ctor. </summary>
            public EventMeta()
            {
                AnonymousCacheSize = 5;
                ClassPropertyResolutionStyle = PropertyResolutionStyle.DEFAULT;
                DefaultAccessorStyle = AccessorStyleEnum.NATIVE;
                DefaultEventRepresentation = EventRepresentation.MAP;
            }

            /// <summary>Returns the default accessor style, native unless changed. </summary>
            /// <value>style enum</value>
            public AccessorStyleEnum DefaultAccessorStyle { get; set; }

            /// <summary>Returns the property resolution style to use for resolving property names of classes. </summary>
            /// <value>style of property resolution</value>
            public PropertyResolutionStyle ClassPropertyResolutionStyle { get; set; }

            /// <summary>
            /// Gets or sets the default event representation.
            /// </summary>
            /// <value>The default event representation.</value>
            public EventRepresentation DefaultEventRepresentation { get; set; }

            /// <summary>
            /// Returns the cache size for anonymous event types.
            /// </summary>
            public int AnonymousCacheSize { get; set; }
        }
    
        /// <summary>Holds view logging settings other then the Log4J settings. </summary>
        [Serializable]
        public class Logging 
        {
            /// <summary>Ctor - sets up defaults. </summary>
            public Logging()
            {
                IsEnableExecutionDebug = false;
                IsEnableTimerDebug = true;
                IsEnableQueryPlan = false;
                IsEnableADO = false;
            }

            /// <summary>
            /// Returns true if execution path debug logging is enabled.
            /// <para />
            /// Only if this flag is set to true, in addition to LOG4J settings set to DEBUG, 
            /// does an engine instance, produce debug out.
            /// </summary>
            /// <value>
            /// true if debug logging through Log4j is enabled for any event processing execution paths
            /// </value>
            public bool IsEnableExecutionDebug { get; set; }

            /// <summary>
            /// Returns true if timer debug level logging is enabled (true by default).
            /// <para />
            /// Set this value to false to reduce the debug-level logging output for the timer Thread(s).
            /// For use only when debug-level logging is enabled.
            /// </summary>
            /// <value>
            /// indicator whether timer execution is noisy in debug or not
            /// </value>
            public bool IsEnableTimerDebug { get; set; }

            /// <summary>Returns indicator whether query plan logging is enabled or not. </summary>
            /// <value>indicator</value>
            public bool IsEnableQueryPlan { get; set; }

            /// <summary>Returns an indicator whether ADO query reporting is enabled. </summary>
            /// <value>indicator</value>
            public bool IsEnableADO { get; set; }

            /// <summary>Returns the pattern that formats audit logs.
            /// <para /> Available conversion characters are:  
            /// <para /> 
            /// %m      - Used to output the audit message. 
            /// %s      - Used to output the statement name. 
            /// %u      - Used to output the engine URI.
            /// </summary>
            /// <value>audit formatting pattern</value>
            public string AuditPattern { get; set; }
        }
    
        /// <summary>Holds variables settings. </summary>
        [Serializable]
        public class Variables 
        {
            /// <summary>Ctor - sets up defaults. </summary>
            public Variables()
            {
                MsecVersionRelease = 15000;
            }

            /// <summary>Returns the number of milliseconds that a version of a variables is held stable for use by very long-running atomic statement execution. <para /> A slow-executing statement such as an SQL join may use variables that, at the time the statement starts to execute, have certain values. The engine guarantees that during statement execution the value of the variables stays the same as long as the statement does not take longer then the given number of milliseconds to execute. If the statement does take longer to execute then the variables release time, the current variables value applies instead. </summary>
            /// <value>millisecond time interval that a variables version is guaranteed to be stablein the context of an atomic statement execution </value>
            public long MsecVersionRelease { get; set; }
        }
    
        /// <summary>Holder for script settings. </summary>
        [Serializable]
        public class Scripts 
        {
            /// <summary>Returns the default script dialect. </summary>
            /// <value>dialect</value>
            public string DefaultDialect { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="Scripts"/> class.
            /// </summary>
            public Scripts()
            {
                DefaultDialect = "js"; // provide a javascript engine
            }
        }
    
        /// <summary>Holds pattern settings. </summary>
        [Serializable]
        public class Patterns 
        {
            public Patterns()
            {
                IsMaxSubexpressionPreventStart = true;
            }

            /// <summary>Returns the maximum number of subexpressions </summary>
            /// <value>subexpression count</value>
            public long? MaxSubexpressions { get; set; }

            /// <summary>Returns true, the default, to indicate that if there is a maximum defined it is being enforced and new subexpressions are not allowed. </summary>
            /// <value>indicate whether enforced or not</value>
            public bool IsMaxSubexpressionPreventStart { get; set; }
        }
    
        /// <summary>Holds match-reconize settings.</summary>
        [Serializable]
        public class MatchRecognize
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="MatchRecognize"/> class.
            /// </summary>
            public MatchRecognize()
            {
                IsMaxStatesPreventStart = true;
            }

            /// <summary>
            /// Returns the maximum number of states
            /// </summary>
            public long? MaxStates { get; set; }

            /// <summary>
            /// Returns true, the default, to indicate that if there is a maximum defined
            /// it is being enforced and new states are not allowed.
            /// </summary>
            public bool IsMaxStatesPreventStart { get; set; }
        }

        /// <summary>Holds default settings for stream selection in the select-clause. </summary>
        [Serializable]
        public class StreamSelection 
        {
            /// <summary>Ctor - sets up defaults. </summary>
            public StreamSelection()
            {
                DefaultStreamSelector = StreamSelector.ISTREAM_ONLY;
            }

            /// <summary>Returns the default stream selector. <para /> Statements that select data from streams and that do not use one of the explicit stream selection keywords (istream/rstream/irstream), by default, generate selection results for the insert stream only, and not for the remove stream. <para /> This setting can be used to change the default behavior: Use the RSTREAM_ISTREAM_BOTH value to have your statements generate both insert and remove stream results without the use of the "irstream" keyword in the select clause. </summary>
            /// <value>default stream selector, which is ISTREAM_ONLY unless changed</value>
            public StreamSelector DefaultStreamSelector { get; set; }
        }
    
        /// <summary>Time source configuration, the default in MILLI (millisecond resolution from System.currentTimeMillis). </summary>
        [Serializable]
        public class TimeSource 
        {
            /// <summary>Ctor. </summary>
            public TimeSource()
            {
                TimeSourceType = TimeSourceType.MILLI;
            }

            /// <summary>Returns the time source type. </summary>
            /// <value>time source type enum</value>
            public TimeSourceType TimeSourceType { get; set; }
        }
    
        /// <summary>Language settings in the engine are for string comparisons. </summary>
        [Serializable]
        public class Language 
        {
            /// <summary>Ctor. </summary>
            public Language()
            {
                IsSortUsingCollator = false;
            }

            /// <summary>Returns true to indicate to perform locale-independent string comparisons using Collator. <para /> By default this setting is false, i.e. string comparisons use the compare method. </summary>
            /// <value>indicator whether to use Collator for string comparisons</value>
            public bool IsSortUsingCollator { get; set; }
        }
    
        /// <summary>Expression evaluation settings in the engine are for results of expressions. </summary>
        [Serializable]
        public class Expression 
        {
            /// <summary>Ctor. </summary>
            public Expression()
            {
                IsIntegerDivision = false;
                IsDivisionByZeroReturnsNull = false;
                IsUdfCache = true;
                IsSelfSubselectPreeval = true;
                IsExtendedAggregation = true;
                TimeZone = TimeZoneInfo.Local;
            }

            /// <summary>Returns false (the default) for integer division returning double values. <para /> Returns true to signal that Java-convention integer division semantics are used for divisions, whereas the division between two non-FP numbers returns only the whole number part of the result and any fractional part is dropped. </summary>
            /// <value>indicator</value>
            public bool IsIntegerDivision { get; set; }

            /// <summary>Returns false (default) when division by zero returns Double.Infinity. Returns true when division by zero return null. <para /> If integer devision is set, then division by zero for non-FP operands also returns null. </summary>
            /// <value>indicator for division-by-zero results</value>
            public bool IsDivisionByZeroReturnsNull { get; set; }

            /// <summary>By default true, indicates that user-defined functions cache return results if the parameter set is empty or has constant-only return values. </summary>
            /// <value>cache flag</value>
            public bool IsUdfCache { get; set; }

            /// <summary>Set to true (the default) to indicate that sub-selects within a statement are updated first when a new event arrives. This is only relevant for statements in which both subselects and the from-clause may react to the same exact event. </summary>
            /// <value>indicator whether to evaluate sub-selects first or last on new event arrival</value>
            public bool IsSelfSubselectPreeval { get; set; }

            /// <summary>Enables or disables non-SQL standard builtin aggregation functions. </summary>
            /// <value>indicator</value>
            public bool IsExtendedAggregation { get; set; }

            /// <summary>Returns true to indicate that duck typing is enable for the specific syntax where it is allowed (check the documentation). </summary>
            /// <value>indicator</value>
            public bool IsDuckTyping { get; set; }

            /// <summary>
            /// Gets or sets the math context.
            /// </summary>
            /// <value>The math context.</value>
            public MathContext MathContext { get; set; }

            /// <summary>
            /// Gets or sets the time zone.
            /// </summary>
            public TimeZoneInfo TimeZone { get; set; }
        }
    
        /// <summary>Holds engine execution-related settings. </summary>
        [Serializable]
        public class Execution 
        {
            /// <summary>Ctor - sets up defaults. </summary>
            public Execution()
            {
                ThreadingProfile = ThreadingProfile.NORMAL;
                IsPrioritized = false;
                FilterServiceMaxFilterWidth = 16;
            }

            /// <summary>Returns false (the default) if the engine does not consider statement priority and preemptive instructions, or true to enable priority-based statement execution order. </summary>
            /// <value>false by default to indicate unprioritized statement execution</value>
            public bool IsPrioritized { get; set; }

            /// <summary>Returns true for fair locking, false for unfair locks. </summary>
            /// <value>fairness flag</value>
            public bool IsFairlock { get; set; }

            /// <summary>Returns indicator whether statement-level locks are disabled. The default is false meaning statement-level locks are taken by default and depending on EPL optimizations. If set to true statement-level locks are never taken. </summary>
            /// <value>indicator for statement-level locks</value>
            public bool IsDisableLocking { get; set; }

            /// <summary>
            /// Gets or sets the property that indicates whether isolated services providers are enabled or disabled (the default).
            /// </summary>
            /// <value>
            /// 	<c>true</c> if this instance is allow isolated service; otherwise, <c>false</c>.
            /// </value>
            public bool IsAllowIsolatedService { get; set; }

            /// <summary>Returns the threading profile </summary>
            /// <value>profile</value>
            public ThreadingProfile ThreadingProfile { get; set; }

            /// <summary>
            /// Gets or sets the filter service profile for tuning filtering operations.
            /// </summary>
            /// <value>The filter service profile.</value>
            public FilterServiceProfile FilterServiceProfile { get; set; }

            /// <summary>
            /// Gets or sets the maximum width for breaking up "or" expression in filters to subexpressions for reverse indexing.
            /// </summary>
            /// <value>
            /// The width of the filter service maximum filter.
            /// </value>
            public int FilterServiceMaxFilterWidth { get; set; }
        }
    
        /// <summary>Threading profile. </summary>
        public enum ThreadingProfile
        {
            /// <summary>Large for use with 100 threads or more. Please see the documentation for more information. </summary>
            LARGE,
    
            /// <summary>For use with 100 threads or less. </summary>
            NORMAL
        }

        /// <summary>
        /// Filter service profile.
        /// </summary>
        public enum FilterServiceProfile
        {
            /// <summary>
            /// If filters are mostly static, the default.
            /// </summary>
            READMOSTLY,

            /// <summary>
            /// For very dynamic filters that come and go in a highly threaded environment.
            /// </summary>
            READWRITE
        }
    
        /// <summary>Time source type. </summary>
        public enum TimeSourceType
        {
            /// <summary>Millisecond time source type with time originating from System.currentTimeMillis </summary>
            MILLI,
    
            /// <summary>Nanosecond time source from a wallclock-adjusted System.nanoTime </summary>
            NANO
        }
    
        /// <summary>Returns the provider for runtime and administrative interfaces. </summary>
        [Serializable]
        public class AlternativeContext 
        {
            /// <summary>Class name of runtime provider. </summary>
            /// <value>provider class</value>
            public string Runtime { get; set; }

            /// <summary>Class name of admin provider. </summary>
            /// <value>provider class</value>
            public string Admin { get; set; }

            /// <summary>Returns the class name of the event type id generator. </summary>
            /// <value>class name</value>
            public string EventTypeIdGeneratorFactory { get; set; }

            /// <summary>Returns the class name of the virtual data window view factory. </summary>
            /// <value>factory class name</value>
            public string VirtualDataWindowViewFactory { get; set; }

            /// <summary>Sets the class name of the statement metadata factory. </summary>
            /// <value>factory class name</value>
            public string StatementMetadataFactory { get; set; }

            /// <summary>
            /// Gets or sets the class name of the class for statement id generation, or 
            /// null if using default.
            /// </summary>
            /// <value>The statement id generator factory.</value>
            public String StatementIdGeneratorFactory { get; set; }

            /// <summary>
            /// Gets or sets the application-provided configuration object carried as 
            /// part of the configurations.
            /// </summary>
            /// <value>The user configuration.</value>
            public Object UserConfiguration { get; set; }

            /// <summary>
            /// Gets or sets the name of the member.
            /// </summary>
            /// <value>The name of the member.</value>
            public String MemberName { get; set; }
        }
    
        /// <summary>Configuration object for defining exception handling behavior. </summary>
        [Serializable]
        public class ExceptionHandling
        {
            /// <summary>Returns the list of exception handler factory class names, see <seealso cref="com.espertech.esper.client.hook.ExceptionHandlerFactory" /> </summary>
            /// <value>list of fully-qualified class names</value>
            public List<string> HandlerFactories { get; set; }

            /// <summary>Add an exception handler factory class name. <para /> Provide a fully-qualified class name of the implementation of the <seealso cref="com.espertech.esper.client.hook.ExceptionHandlerFactory" /> interface. </summary>
            /// <param name="exceptionHandlerFactoryClassName">class name of exception handler factory</param>
            public void AddClass(String exceptionHandlerFactoryClassName) {
                if (HandlerFactories == null) {
                    HandlerFactories = new List<String>();
                }
                HandlerFactories.Add(exceptionHandlerFactoryClassName);
            }
    
            /// <summary>
            /// Add a list of exception handler class names.
            /// </summary>
            /// <param name="classNames">to add</param>
            public void AddClasses(IEnumerable<String> classNames) {
                if (HandlerFactories == null) {
                    HandlerFactories = new List<String>();
                }
                HandlerFactories.AddAll(classNames);
            }
    
            /// <summary>
            /// Add an exception handler factory class.
            /// <para/> 
            /// The class provided should implement the <seealso cref="com.espertech.esper.client.hook.ExceptionHandlerFactory" /> interface.
            /// </summary>
            /// <param name="exceptionHandlerFactoryClass">class of implementation</param>
            public void AddClass(Type exceptionHandlerFactoryClass) {
                AddClass(exceptionHandlerFactoryClass.FullName);
            }

            /// <summary>
            /// Add an exception handler factory class. 
            /// <para />
            /// The class provided should implement the <seealso cref="com.espertech.esper.client.hook.ExceptionHandlerFactory" /> interface.
            ///  </summary>
            /// <typeparam name="T"></typeparam>
            public void AddClass<T>()
            {
                AddClass(typeof (T));
            }
        }
    
        /// <summary>Configuration object for defining condition handling behavior. </summary>
        [Serializable]
        public class ConditionHandling
        {
            /// <summary>Returns the list of condition handler factory class names, see <seealso cref="com.espertech.esper.client.hook.ConditionHandlerFactory" /> </summary>
            /// <value>list of fully-qualified class names</value>
            public List<string> HandlerFactories { get; set; }

            /// <summary>Add an condition handler factory class name. <para /> Provide a fully-qualified class name of the implementation of the <seealso cref="com.espertech.esper.client.hook.ConditionHandlerFactory" /> interface. </summary>
            /// <param name="className">class name of condition handler factory</param>
            public void AddClass(String className) {
                if (HandlerFactories == null) {
                    HandlerFactories = new List<String>();
                }
                HandlerFactories.Add(className);
            }
    
            /// <summary>Add a list of condition handler class names. </summary>
            /// <param name="classNames">to add</param>
            public void AddClasses(IEnumerable<String> classNames) {
                if (HandlerFactories == null) {
                    HandlerFactories = new List<String>();
                }
                HandlerFactories.AddAll(classNames);
            }
    
            /// <summary>Add an condition handler factory class. <para /> The class provided should implement the <seealso cref="com.espertech.esper.client.hook.ConditionHandlerFactory" /> interface. </summary>
            /// <param name="clazz">class of implementation</param>
            public void AddClass(Type clazz) {
                AddClass(clazz.FullName);
            }

            public void AddClass<T>()
            {
                AddClass(typeof (T));
            }
        }
    
        /// <summary>Cluster configuration. </summary>
        [Serializable]
        public class Cluster 
        {
            [NonSerialized] private Object _clusterConfig;

            /// <summary>Returns true if enabled. </summary>
            /// <value>enabled flag</value>
            public bool IsEnabled { get; set; }

            /// <summary>Returns the cluster configuration object. </summary>
            /// <value>cluster configuration object</value>
            public object ClusterConfig
            {
                get { return _clusterConfig; }
                set { _clusterConfig = value; }
            }

            /// <summary>Returns the cluster configurator class. </summary>
            /// <value>class</value>
            public string ClusterConfiguratorClass { get; set; }
        }
    
        /// <summary>Interface for cluster configurator. </summary>
        public interface ClusterConfigurator
        {
            /// <summary>Provide cluster configuration information. </summary>
            /// <param name="configuration">information</param>
            void Configure(Configuration configuration);
        }
    }
}
