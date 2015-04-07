///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;

using com.espertech.esper.client.context;
using com.espertech.esper.client.dataflow;
using com.espertech.esper.client.soda;
using com.espertech.esper.client.util;

namespace com.espertech.esper.client
{
    using DataMap = IDictionary<string, object>;

    /// <summary>
    /// Interface to event stream processing runtime services.
    /// </summary>
    public interface EPRuntime
    {
        /// <summary>
        /// Send an event represented by a plain object to the event stream processing
        /// runtime.
        /// <para/>
        /// Use the route method for sending events into the runtime from within
        /// UpdateListener code, to avoid the possibility of a stack overflow due to nested calls to
        /// SendEvent.
        /// (except with the outbound-threading configuration), see <seealso cref="Route(object)" />.
        /// </summary>
        /// <param name="obj">is the event to sent to the runtime</param>
        /// <throws>EPException is thrown when the processing of the event lead to an error</throws>
        void SendEvent(Object obj);

        /// <summary>
        /// Send a map containing event property values to the event stream processing
        /// runtime.
        /// <para/>
        /// Use the route method for sending events into the runtime from within
        /// event handler code. to avoid the possibility of a stack overflow due to nested calls to
        /// SendEvent.
        /// (except with the outbound-threading configuration), see <seealso cref="Route(DataMap, string)" />.
        /// </summary>
        /// <param name="map">map that contains event property values. Keys are expected to be of type string while value scan be of any type. Keys and values should match those declared via Configuration for the given eventTypeName. </param>
        /// <param name="mapEventTypeName">the name for the Map event type that was previously configured</param>
        /// <throws>EPException - when the processing of the event leads to an error</throws>
        void SendEvent(DataMap map, string mapEventTypeName);

        /// <summary>
        /// Send an object array containing event property values to the event stream processing runtime.
        /// <para/>
        /// Use the route method for sending events into the runtime from within UpdateListener code.
        /// to avoid the possibility of a stack overflow due to nested calls to sendEvent.
        /// (except with the outbound-threading configuration), see <seealso cref="Route(Object[], string)" />.
        /// </summary>
        /// <param name="objectarray">array that contains event property values. Your application must ensure that property values match the exact same order that the property names and types have been declared, and that the array length matches the number of properties declared.</param>
        /// <param name="objectArrayEventTypeName">the name for the Object-array event type that was previously configured</param>
        /// <throws>EPException - when the processing of the event leads to an error</throws>
        void SendEvent(Object[] objectarray, String objectArrayEventTypeName);

        /// <summary>
        /// Send an event represented by a LINQ element to the event stream processing runtime.
        /// <para/>
        /// Use the route method for sending events into the runtime from within
        /// event handler code. to avoid the possibility of a stack overflow due to nested calls to
        /// SendEvent.
        /// (except with the outbound-threading configuration), see <seealso cref="Route(XElement)" />.
        /// </summary>
        /// <param name="element">The element.</param>
        void SendEvent(XElement element);

        /// <summary>
        /// Send an event represented by a DOM node to the event stream processing runtime.
        /// <para/>
        /// Use the route method for sending events into the runtime from within
        /// event handler code. to avoid the possibility of a stack overflow due to nested calls to
        /// SendEvent.
        /// (except with the outbound-threading configuration), see <seealso cref="Route(XmlNode)" />.
        /// </summary>
        /// <param name="node">is the DOM node as an event</param>
        /// <throws>EPException is thrown when the processing of the event lead to an error</throws>
        void SendEvent(XmlNode node);

        /// <summary>
        /// Number of events evaluated over the lifetime of the event stream processing
        /// runtime, or since the last ResetStats() call.
        /// </summary>
        /// <returns>
        /// number of events received
        /// </returns>
        long NumEventsEvaluated { get; }

        /// <summary>
        /// Reset number of events received and emitted
        /// </summary>
        void ResetStats();
    
        /// <summary>
        /// Route the event object back to the event stream processing runtime for internal
        /// dispatching, to avoid the possibility of a stack overflow due to nested calls to
        /// SendEvent. The route event is processed just like it was sent to the runtime,
        /// that is any active expressions seeking that event receive it. The routed event has
        /// priority over other events sent to the runtime. In a single-threaded application
        /// the routed event is processed before the next event is sent to the runtime through
        /// the EPRuntime.SendEvent method.
        /// <para>
        ///     Note: when outbound-threading is enabled, the thread delivering to listeners
        ///     is not the thread processing the original event. Therefore with outbound-threading
        ///     enabled the SendEvent method should be used by listeners instead.
        /// </para>
        /// </summary>
        /// <param name="evnt">to route internally for processing by the event stream processing runtime</param>
        void Route(Object evnt);

        /// <summary>
        /// Route the event object back to the event stream processing runtime for internal
        /// dispatching, to avoid the possibility of a stack overflow due to nested calls to
        /// SendEvent. The route event is processed just like it was sent to the runtime,
        /// that is any active expressions seeking that event receive it. The routed event has
        /// priority over other events sent to the runtime. In a single-threaded application
        /// the routed event is processed before the next event is sent to the runtime through
        /// the EPRuntime.SendEvent method.
        /// <para>
        ///     Note: when outbound-threading is enabled, the thread delivering to listeners
        ///     is not the thread processing the original event. Therefore with outbound-threading
        ///     enabled the SendEvent method should be used by listeners instead.
        /// </para>
        /// </summary>
        /// <param name="map">map that contains event property values. Keys are expected to be of type string while valuescan be of any type. Keys and values should match those declared via Configuration for the given eventTypeName. </param>
        /// <param name="eventTypeName">the name for Map event type that was previously configured</param>
        /// <throws>EPException - when the processing of the event leads to an error</throws>
        void Route(DataMap map, string eventTypeName);

        /// <summary>
        /// Route the event object back to the event stream processing runtime for internal 
        /// dispatching, to avoid the possibility of a stack overflow due to nested calls to sendEvent. 
        /// The route event is processed just like it was sent to the runtime, that is any active 
        /// expressions seeking that event receive it. The routed event has priority over other events 
        /// sent to the runtime. In a single-threaded application the routed event is processed before 
        /// the next event is sent to the runtime through the EPRuntime.sendEvent method.
        /// <para>
        ///     Note: when outbound-threading is enabled, the thread delivering to listeners
        ///     is not the thread processing the original event. Therefore with outbound-threading
        ///     enabled the SendEvent method should be used by listeners instead.
        /// </para>
        /// </summary>
        /// <param name="objectArray">object array that contains event property values. Your application must ensure that property valuesmatch the exact same order that the property names and types have been declared, and that the array length matches the number of properties declared.</param>
        /// <param name="eventTypeName">the name for Object-array event type that was previously configured</param>
        /// <throws>EPException - when the processing of the event leads to an error</throws>
        void Route(Object[] objectArray, String eventTypeName);

        /// <summary>
        /// Route the event object back to the event stream processing runtime for internal
        /// dispatching, to avoid the possibility of a stack overflow due to nested calls to
        /// SendEvent. The route event is processed just like it was sent to the runtime,
        /// that is any active expressions seeking that event receive it. The routed event has
        /// priority over other events sent to the runtime. In a single-threaded application
        /// the routed event is processed before the next event is sent to the runtime through
        /// the EPRuntime.SendEvent method.
        /// <para>
        ///     Note: when outbound-threading is enabled, the thread delivering to listeners
        ///     is not the thread processing the original event. Therefore with outbound-threading
        ///     enabled the SendEvent method should be used by listeners instead.
        /// </para>
        /// </summary>
        /// <param name="element">The LINQ element as an event.</param>
        /// <throws>EPException is thrown when the processing of the event lead to an error</throws>
        void Route(XElement element);

        /// <summary>
        /// Route the event object back to the event stream processing runtime for internal
        /// dispatching, to avoid the possibility of a stack overflow due to nested calls to
        /// SendEvent. The route event is processed just like it was sent to the runtime,
        /// that is any active expressions seeking that event receive it. The routed event has
        /// priority over other events sent to the runtime. In a single-threaded application
        /// the routed event is processed before the next event is sent to the runtime through
        /// the EPRuntime.SendEvent method.
        /// <para>
        ///     Note: when outbound-threading is enabled, the thread delivering to listeners
        ///     is not the thread processing the original event. Therefore with outbound-threading
        ///     enabled the SendEvent method should be used by listeners instead.
        /// </para>
        /// </summary>
        /// <param name="node">is the DOM node as an event</param>
        /// <throws>EPException is thrown when the processing of the event lead to an error</throws>
        void Route(XmlNode node);
    
        /// <summary>
        /// Gets or sets a listener to receive events that are unmatched by any statement.
        /// <para/>
        /// Events that can be unmatched are all events that are send into a runtime via
        /// one of the SendEvent methods, or that have been generated via insert-into clause.
        /// <para/>
        /// For an event to be unmatched by any statement, the event must not match any
        /// statement's event stream filter criteria (a where-clause is NOT a filter criteria
        /// for a stream, as below).
        /// <para/>
        /// Note: In the following statement a MyEvent event does always match this
        /// statement's event stream filter criteria, regardless of the value of the 'quantity'
        /// property.
        /// <pre>select * from MyEvent where quantity > 5</pre> 
        /// <para/>
        /// In the following statement only a MyEvent event with a 'quantity' property value of 5 or less does
        /// not match this statement's event stream filter criteria: 
        /// <pre>select * from MyEvent(quantity > 5)</pre>
        /// <para/>
        /// For patterns, if no pattern sub-expression is active for such event, the event
        /// is also unmatched.
        /// </summary>
        event EventHandler<UnmatchedEventArgs> UnmatchedEvent;

        /// <summary>
        /// Removes all unmatched event handlers.
        /// </summary>
        void RemoveAllUnmatchedEventHandlers();

        /// <summary>
        /// Returns the current variable value for a global variable. A null value is a valid value for a variable.
        /// Not for use with context-partitioned variables.
        /// </summary>
        /// <param name="variableName">is the name of the variable to return the value for</param>
        /// <returns>
        /// current variable value
        /// </returns>
        /// <throws>VariableNotFoundException if a variable by that name has not been declared</throws>
        Object GetVariableValue(string variableName);

        /// <summary>
        /// Returns the current variable value for a global variable. A null value is a valid value for a variable.
        /// Not for use with context-partitioned variables.
        /// </summary>
        /// <param name="variableNames">is a set of variable names for which to return values</param>
        /// <returns>
        /// map of variable name and variable value
        /// </returns>
        /// <throws>VariableNotFoundException if any of the variable names has not been declared</throws>
        DataMap GetVariableValue(ICollection<string> variableNames);

        /// <summary>
        /// Returns the current variable values for a context-partitioned variable, per context partition.
        /// A null value is a valid value for a variable.
        /// Only for use with context-partitioned variables.
        /// Variable names provided must all be associated to the same context partition.
        /// </summary>
        /// <param name="variableNames">are the names of the variables to return the value for</param>
        /// <param name="contextPartitionSelector">selector for the context partition to return the value for</param>
        /// <returns>current variable value</returns>
        /// <throws>VariableNotFoundException if a variable by that name has not been declared</throws>
        IDictionary<string, IList<ContextPartitionVariableState>> GetVariableValue(
            ISet<String> variableNames, ContextPartitionSelector contextPartitionSelector);

        /// <summary>
        /// Returns current variable values for all global variables,
        /// guaranteeing consistency in the face of concurrent updates to the variables.
        /// Not for use with context-partitioned variables.
        /// </summary>
        /// <returns>
        /// map of variable name and variable value
        /// </returns>
        DataMap VariableValueAll { get; }

        /// <summary>
        /// Sets the value of a single global variable.
        /// <para>
        /// Note that the thread setting the variable value queues the changes, i.e. it does not itself
        /// re-evaluate such new variable value for any given statement. The timer thread performs this work.
        /// </para>
        /// Not for use with context-partitioned variables.
        /// </summary>
        /// <param name="variableName">is the name of the variable to change the value of</param>
        /// <param name="variableValue">is the new value of the variable, with null an allowed value</param>
        /// <throws>VariableValueException if the value does not match variable type or cannot be safely coercedto the variable type </throws>
        /// <throws>VariableNotFoundException if the variable name has not been declared</throws>
        void SetVariableValue(string variableName, Object variableValue);

        /// <summary>
        /// Sets the value of multiple global variables in one Update, applying all or none of the
        /// changes to variable values in one atomic transaction.
        /// <para>
        /// Note that the thread setting the variable value queues the changes, i.e. it does not itself
        /// re-evaluate such new variable value for any given statement. The timer thread performs this work.
        /// </para>
        /// Not for use with context-partitioned variables.
        /// </summary>
        /// <param name="variableValues">is the map of variable name and variable value, with null an allowed value</param>
        /// <throws>VariableValueException if any value does not match variable type or cannot be safely coercedto the variable type </throws>
        /// <throws>VariableNotFoundException if any of the variable names has not been declared</throws>
        void SetVariableValue(DataMap variableValues);

        /// <summary>
        /// Sets the value of multiple context-partitioned variables in one update, applying all or none of the changes
        /// to variable values in one atomic transaction.
        /// <para>
        ///     Note that the thread setting the variable value queues the changes, i.e. it does not itself
        ///     re-evaluate such new variable value for any given statement. The timer thread performs this work.
        /// </para>
        /// Only for use with context-partitioned variables.
        /// <param name="variableValues">is the map of variable name and variable value, with null an allowed value</param>
        /// <param name="agentInstanceId">the id of the context partition</param>
        /// <throws>VariableValueException if any value does not match variable type or cannot be safely coerced to the variable type</throws>
        /// <throws>VariableNotFoundException if any of the variable names has not been declared</throws>
        void SetVariableValue(DataMap variableValues, int agentInstanceId);

        /// <summary>
        /// Returns a facility to process event objects that are of a known type.
        /// <para/>
        /// Given an event type name this method returns a sender that allows to send in
        /// event objects of that type. The event objects send in via the event sender are
        /// expected to match the event type, thus the event sender does not inspect the event
        /// object other then perform basic checking.
        /// <para/>
        /// For events backed by a type, the sender ensures that the object send in matches in 
        /// class, or implements or extends the class underlying the event type for the given event 
        /// type name. Note that event type identity for type events is the type.  When assigning 
        /// two different event type names to the same type the names are an alias for the same
        /// event type i.e. there is always a single event type to represent a given type class.
        /// <para/>
        /// For events backed by a Object[] (Object-array events), the sender does not perform any checking other
        /// then checking that the event object indeed is an array of object.
        /// <para/>
        /// For events backed by a Dictionary(Map events), the sender does not perform
        /// any checking other then checking that the event object indeed : Map.
        /// <para/>
        /// For events backed by a XmlNode (XML DOM events), the sender checks that
        /// the root element name indeed does match the root element name for the event type
        /// name.
        /// </summary>
        /// <param name="eventTypeName">is the name of the event type</param>
        /// <returns>
        /// sender for fast-access processing of event objects of known type (and content)
        /// </returns>
        /// <throws>EventTypeException thrown to indicate that the name does not exist</throws>
        EventSender GetEventSender(string eventTypeName);

        /// <summary>
        /// For use with plug-in event representations, returns a facility to process event
        /// objects that are of one of a number of types that one or more of the registered
        /// plug-in event representation extensions can reflect upon and provide an event for.
        /// </summary>
        /// <param name="uris">
        /// is the URIs that specify which plug-in event representations may process an event object.
        /// <para/>URIs do not need to match event representation URIs exactly, a child (hierarchical) match is enough for an event representation to participate.
        /// <para/>The order of URIs is relevant as each event representation's factory is asked in turn to process the event, until the first factory processes the event.
        /// </param>
        /// <returns>
        /// sender for processing of event objects of one of the plug-in event
        /// representations
        /// </returns>
        /// <throws>EventTypeException thrown to indicate that the URI list was invalid</throws>
        EventSender GetEventSender(Uri[] uris);
    
        /// <summary>
        /// Execute an on-demand query.
        /// <para/>
        /// On-demand queries are EPL queries that execute non-continuous fire-and-forget
        /// queries against named windows.
        /// </summary>
        /// <param name="epl">is the EPL to execute</param>
        /// <returns>
        /// query result
        /// </returns>
        EPOnDemandQueryResult ExecuteQuery(string epl);

        /// <summary>
        /// For use with named windows that have a context declared and that may therefore have multiple context partitions, allows to target context partitions for query execution selectively.
        /// </summary>
        /// <param name="epl">is the EPL query to execute</param>
        /// <param name="contextPartitionSelectors">selects context partitions to consider</param>
        /// <returns>result</returns>
        EPOnDemandQueryResult ExecuteQuery(String epl, ContextPartitionSelector[] contextPartitionSelectors);

        /// <summary>Execute an on-demand query. &lt;p&gt; On-demand queries are EPL queries that execute non-continuous fire-and-forget queries against named windows. </summary>
        /// <param name="model">is the EPL query to execute, obtain a model object using {@link EPAdministrator#compileEPL(String)}or via the API </param>
        /// <returns>query result</returns>
        EPOnDemandQueryResult ExecuteQuery(EPStatementObjectModel model);

        /// <summary>For use with named windows that have a context declared and that may therefore have multiple context partitions, allows to target context partitions for query execution selectively. </summary>
        /// <param name="model">is the EPL query to execute, obtain a model object using {@link EPAdministrator#compileEPL(String)}or via the API </param>
        /// <param name="contextPartitionSelectors">selects context partitions to consider</param>
        /// <returns>result</returns>
        EPOnDemandQueryResult ExecuteQuery(EPStatementObjectModel model, ContextPartitionSelector[] contextPartitionSelectors);

        /// <summary>Prepare an unparameterized on-demand query before execution and for repeated execution. </summary>
        /// <param name="epl">to prepare</param>
        /// <returns>proxy to execute upon, that also provides the event type of the returned results</returns>
        EPOnDemandPreparedQuery PrepareQuery(String epl);

        /// <summary>Prepare an unparameterized on-demand query before execution and for repeated execution. </summary>
        /// <param name="model">is the EPL query to prepare, obtain a model object using {@link EPAdministrator#compileEPL(String)}or via the API </param>
        /// <returns>proxy to execute upon, that also provides the event type of the returned results</returns>
        EPOnDemandPreparedQuery PrepareQuery(EPStatementObjectModel model);

        /// <summary>Prepare a parameterized on-demand query for repeated parameter setting and execution. Set all values on the returned holder then execute using {@link #executeQuery(EPOnDemandPreparedQueryParameterized)}. </summary>
        /// <param name="epl">to prepare</param>
        /// <returns>parameter holder upon which to set values</returns>
        EPOnDemandPreparedQueryParameterized PrepareQueryWithParameters(String epl);

        /// <summary>Execute an on-demand parameterized query. &lt;p&gt; On-demand queries are EPL queries that execute non-continuous fire-and-forget queries against named windows. </summary>
        /// <param name="parameterizedQuery">contains the query and parameter values</param>
        /// <returns>query result</returns>
        EPOnDemandQueryResult ExecuteQuery(EPOnDemandPreparedQueryParameterized parameterizedQuery);

        /// <summary>Execute an on-demand parameterized query. &lt;p&gt; On-demand queries are EPL queries that execute non-continuous fire-and-forget queries against named windows. </summary>
        /// <param name="parameterizedQuery">contains the query and parameter values</param>
        /// <param name="contextPartitionSelectors">selects context partitions to consider</param>
        /// <returns>query result</returns>
        EPOnDemandQueryResult ExecuteQuery(EPOnDemandPreparedQueryParameterized parameterizedQuery, ContextPartitionSelector[] contextPartitionSelectors);

        /// <summary>
        /// Returns the event renderer for events generated by this runtime.
        /// </summary>
        /// <returns>
        /// event renderer
        /// </returns>
        EventRenderer EventRenderer { get; }

        /// <summary>
        /// Returns current engine time.
        /// <para/>
        /// If time is provided externally via timer events, the function returns current
        /// time as externally provided.
        /// </summary>
        /// <returns>
        /// current engine time
        /// </returns>
        long CurrentTime { get; }

        /// <summary>
        /// Returns the time at which the next schedule execution is expected, returns null if no schedule execution is
        /// outstanding.
        /// </summary>
        long? NextScheduledTime { get; }

        /// <summary>Returns the data flow runtime. </summary>
        /// <returns>data flow runtime</returns>
        EPDataFlowRuntime DataFlowRuntime { get; }

        /// <summary>
        /// Returns true for external clocking, false for internal clocking.
        /// </summary>
        /// <value>clocking indicator</value>
        bool IsExternalClockingEnabled { get; }
    }
}
