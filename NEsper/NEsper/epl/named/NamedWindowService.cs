///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.threading;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.lookup;
using com.espertech.esper.epl.metric;
using com.espertech.esper.events.vaevent;

namespace com.espertech.esper.epl.named
{
    /// <summary>
    /// Service to manage named window dispatches, locks and processors on an engine level.
    /// </summary>
    public interface NamedWindowService : IDisposable
    {
        /// <summary>Returns true to indicate that the name is a named window. </summary>
        /// <param name="name">is the window name</param>
        /// <returns>true if a named window, false if not a named window</returns>
        bool IsNamedWindow(String name);

        /// <summary>Returns the names of all named windows known. </summary>
        /// <value>named window names</value>
        string[] NamedWindows { get; }

        /// <summary>
        /// Create a new named window.
        /// </summary>
        /// <param name="name">window name</param>
        /// <param name="contextName">Name of the context.</param>
        /// <param name="singleInstanceContext">if set to <c>true</c> [single instance context].</param>
        /// <param name="eventType">the event type of the window</param>
        /// <param name="statementResultService">for coordinating on whether insert and remove stream events should be posted</param>
        /// <param name="revisionProcessor">handles Update events</param>
        /// <param name="eplExpression">is the expression</param>
        /// <param name="statementName">the name of the statement</param>
        /// <param name="isPrioritized">if the engine is running with prioritized execution</param>
        /// <param name="isEnableSubqueryIndexShare">if set to <c>true</c> [is enable subquery index share].</param>
        /// <param name="isBatchingDataWindow">if set to <c>true</c> [is batching data window].</param>
        /// <param name="isVirtualDataWindow">if set to <c>true</c> [is virtual data window].</param>
        /// <param name="statementMetricHandle">The statement metric handle.</param>
        /// <param name="optionalUniqueKeyProps">The optional unique key props.</param>
        /// <param name="eventTypeAsName">Name of the event type as.</param>
        /// <returns>processor for the named window</returns>
        /// <throws>ViewProcessingException if the named window already exists</throws>
        NamedWindowProcessor AddProcessor(String name,
                                          String contextName,
                                          bool singleInstanceContext,
                                          EventType eventType,
                                          StatementResultService statementResultService,
                                          ValueAddEventProcessor revisionProcessor,
                                          String eplExpression,
                                          String statementName,
                                          bool isPrioritized,
                                          bool isEnableSubqueryIndexShare,
                                          bool isBatchingDataWindow,
                                          bool isVirtualDataWindow,
                                          StatementMetricHandle statementMetricHandle,
                                          ICollection<String> optionalUniqueKeyProps,
                                          String eventTypeAsName);
    
        /// <summary>Returns the processing instance for a given named window. </summary>
        /// <param name="name">window name</param>
        /// <returns>processor for the named window</returns>
        NamedWindowProcessor GetProcessor(String name);
    
        /// <summary>Upon destroy of the named window creation statement, the named window processor must be removed. </summary>
        /// <param name="name">is the named window name</param>
        void RemoveProcessor(String name);

        /// <summary>Dispatch events of the insert and remove stream of named windows to consumers, as part of the main event processing or dispatch loop. </summary>
        /// <returns>send events to consuming statements</returns>
        bool Dispatch();
    
        /// <summary>For use to add a result of a named window that must be dispatched to consuming views. </summary>
        /// <param name="delta">is the result to dispatch</param>
        /// <param name="consumers">is the destination of the dispatch, a map of statements to one or more consuming views</param>
        void AddDispatch(NamedWindowDeltaData delta, IDictionary<EPStatementAgentInstanceHandle, IList<NamedWindowConsumerView>> consumers);
    
        /// <summary>Returns the statement lock for the named window, to be shared with on-delete statements for the same named window. </summary>
        /// <param name="windowName">is the window name</param>
        /// <returns>the lock for the named window, or null if the window dos not yet exists</returns>
        IReaderWriterLock GetNamedWindowLock(String windowName);
    
        /// <summary>Sets the lock to use for a named window. </summary>
        /// <param name="windowName">is the named window name</param>
        /// <param name="statementResourceLock">is the statement lock for the create window statement</param>
        /// <param name="statementName">the name of the statement that is the "create window"</param>
        void AddNamedWindowLock(String windowName, IReaderWriterLock statementResourceLock, String statementName);
    
        /// <summary>Remove the lock associated to the named window. </summary>
        /// <param name="statementName">the name of the statement that is the "create window"</param>
        void RemoveNamedWindowLock(String statementName);
    
        /// <summary>Add an observer to be called back when named window state changes occur. <para />Observers have set-semantics: the same Observer cannot be added twice </summary>
        /// <param name="observer">to add</param>
        void AddObserver(NamedWindowLifecycleObserver observer);
    
        /// <summary>Remove an observer to be called back when named window state changes occur. </summary>
        /// <param name="observer">to remove</param>
        void RemoveObserver(NamedWindowLifecycleObserver observer);
    
        /// <summary>Returns an index descriptor array describing all available indexes for the named window. </summary>
        /// <param name="windowName">window name</param>
        /// <returns>indexes</returns>
        IndexMultiKey[] GetNamedWindowIndexes(String windowName);
    }

    public class NamedWindowServiceConstants    {        /// <summary>Error message for data windows required. </summary>        public readonly static String ERROR_MSG_DATAWINDOWS = "Named windows require one or more child views that are data window views";
        /// <summary>Error message for no data window allowed. </summary>        public readonly static String ERROR_MSG_NO_DATAWINDOW_ALLOWED = "Consuming statements to a named window cannot declare a data window view onto the named window";    }}
