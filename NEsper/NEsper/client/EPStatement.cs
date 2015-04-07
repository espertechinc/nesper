///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

using com.espertech.esper.client.context;

namespace com.espertech.esper.client
{
	/// <summary>
	/// Statement interface that provides methods to start, stop and destroy a statement as well as
	/// get statement information such as statement name, expression text and current state.
	/// <para>
	/// Statements have 3 states: STARTED, STOPPED and DESTROYED.
    /// </para>
	/// <para>
	/// In started state, statements are actively evaluating event streams according to the statement expression. Started
	/// statements can be stopped and destroyed.
    /// </para>
	/// <para>
	/// In stopped state, statements are inactive. Stopped statements can either be started, in which case
	/// they begin to actively evaluate event streams, or destroyed.
    /// </para>
	/// <para>
	/// Destroyed statements have relinguished all statement resources and cannot be started or stopped.
    /// </para>
	/// </summary>

    public interface EPStatement : EPListenable, EPIterable, IDisposable
    {
        /// <summary> Returns the statement name.</summary>
        /// <returns> statement name</returns>
        String Name { get; }

        /// <summary> Returns the underlying expression text or XML.</summary>
        /// <returns> expression text</returns>
        String Text { get; }

        /// <summary>Gets the statement's current state</summary>
        EPStatementState State { get; }

        /// <summary>Returns true if the statement state is started.</summary>
        /// <returns>
        /// true for started statements, false for stopped or destroyed statements.
        /// </returns>
        bool IsStarted { get; }

        /// <summary>Returns true if the statement state is stopped.</summary>
        /// <returns>
        /// true for stopped statements, false for started or destroyed statements.
        /// </returns>
        bool IsStopped { get; }

        /// <summary>Returns true if the statement state is destroyed.</summary>
        /// <returns>
        /// true for destroyed statements, false for started or stopped statements.
        /// </returns>
        bool IsDisposed { get; }

        /// <summary> Start the statement.</summary>
        void Start();

        /// <summary> Stop the statement.</summary>
        void Stop();

        /// <summary>
        /// Returns the system time in milliseconds of when the statement last change state.
        /// </summary>
        /// <returns>time in milliseconds of last statement state change</returns>
        long TimeLastStateChange { get; }

        /// <summary>
        /// Gets or sets the current subscriber instance that receives statement results.
        /// <para/>
        /// Only a single subscriber may be set for a statement. If this method is invoked twice
        /// any previously-set subscriber is no longer used. 
        /// </summary>
        /// <value>The subscriber.</value>
        Object Subscriber { get; set; }

        /// <summary>Returns true if statement is a pattern</summary>
        /// <returns>true if statement is a pattern</returns>
        bool IsPattern { get; }

	    /// <summary>
	    /// Returns the application defined user data object associated with the statement,
	    /// or null if none was supplied at time of statement creation.
	    /// <para/>
	    /// The <em>user object</em> is a single, unnamed field that is stored with every
	    /// statement. Applications may put arbitrary objects in this field or a null value.
	    /// <para/>
	    /// User objects are passed at time of statement creation as a parameter the create
	    /// method.
	    /// </summary>
	    /// <returns>
	    /// user object or null if none defined
	    /// </returns>
	    object UserObject { get; }

        /// <summary>
        /// Add an event handler to the current statement and replays current statement 
        /// results to the handler.
        /// <para/>
        /// The handler receives current statement results as the first call to the Update
        /// method of the event handler, passing in the newEvents parameter the current statement
        /// results as an array of zero or more events. Subsequent calls to the Update
        /// method of the event handler are statement results.
        /// <para/>
        /// Current statement results are the events returned by the GetEnumerator or
        /// GetSafeEnumerator methods.
        /// <para/>
        /// Delivery of current statement results in the first call is performed by the
        /// same thread invoking this method, while subsequent calls to the event handler may
        /// deliver statement results by the same or other threads.
        /// <para/>
        /// Note: this is a blocking call, delivery is atomic: Events occurring during
        /// iteration and delivery to the event handler are guaranteed to be delivered in a separate
        /// call and not lost. The event handler implementation should minimize long-running or
        /// blocking operations.
        /// <para/>
        /// Delivery is only atomic relative to the current statement. If the same event handler
        /// instance is registered with other statements it may receive other statement
        /// result s simultaneously.
        /// <para/>
        /// If a statement is not started an therefore does not have current results, the
        /// event handler receives a single invocation with a null value in newEvents.
        /// </summary>
        /// <param name="eventHandler">eventHandler that will receive events</param>
	    void AddEventHandlerWithReplay(UpdateEventHandler eventHandler);

        /// <summary>
        /// Returns EPL or pattern statement attributes provided in the statement text, if any.
        /// <para/>
        /// See the annotation <seealso cref="com.espertech.esper.client.annotation"/> namespace
        /// for additional attributes / annotations.
        /// </summary>
        ICollection<Attribute> Annotations { get; }

	    /// <summary>
	    /// Returns the name of the isolated service provided is the statement is currently
	    /// isolated in terms of event visibility and scheduling, or returns null if the
	    /// statement is live in the engine.
	    /// </summary>
	    /// <returns>
	    /// isolated service name or null for statements that are not currently isolated
	    /// </returns>
        string ServiceIsolated { get; set; }

        /// <summary>
        /// For use with statements that have a context declared and that may therefore have
        /// multiple context partitions, allows to iterate over context partitions selectively.
        /// </summary>
        /// <param name="selector">selects context partitions to consider</param>
        /// <returns>iterator</returns>
        IEnumerator<EventBean> GetEnumerator(ContextPartitionSelector selector);

        /// <summary>
        /// For use with statements that have a context declared and that may therefore have 
        /// multiple context partitions, allows to safe-iterate over context partitions selectively.
        /// </summary>
        /// <param name="selector">selects context partitions to consider</param>
        /// <returns>safe iterator</returns>
        IEnumerator<EventBean> GetSafeEnumerator(ContextPartitionSelector selector);
    }

    public class ProxySubscriber
    {
        public Action<string> ProcUpdate { get; set; }

        public void Update(string value)
        {
            ProcUpdate.Invoke(value);
        }
    }
}
