///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.soda;
using com.espertech.esper.epl.spec;

namespace com.espertech.esper.core.service
{
    /// <summary>
    /// Handles statement management.
    /// </summary>
    public interface StatementLifecycleSvc 
        : StatementLifecycleStmtContextResolver
        , IDisposable
    {
        /// <summary>Initialized the service before use. </summary>
        void Init();

        /// <summary>
        /// Occurs when there is a corresponding lifecycle event.
        /// </summary>
        event EventHandler<StatementLifecycleEvent> LifecycleEvent;

        /// <summary>
        /// Dispatch event to observers.
        /// </summary>
        /// <param name="theEvent">to dispatch</param>
        void DispatchStatementLifecycleEvent(StatementLifecycleEvent theEvent);

        /// <summary>
        /// Create and start the statement.
        /// </summary>
        /// <param name="statementSpec">is the statement definition in bean object form, raw unvalidated and unoptimized.</param>
        /// <param name="expression">is the expression text</param>
        /// <param name="isPattern">is an indicator on whether this is a pattern statement and thus the iterator must return the last result,versus for non-pattern statements the iterator returns view content.</param>
        /// <param name="optStatementName">is an optional statement name, null if none was supplied</param>
        /// <param name="userObject">the application define user object associated to each statement, if supplied</param>
        /// <param name="isolationUnitServices">isolated service services</param>
        /// <param name="optionalStatementId">The statement id.</param>
        /// <param name="optionalModel">The optional model.</param>
        /// <returns>started statement</returns>
        EPStatement CreateAndStart(
            StatementSpecRaw statementSpec,
            string expression,
            bool isPattern,
            string optStatementName,
            object userObject,
            EPIsolationUnitServices isolationUnitServices,
            int? optionalStatementId,
            EPStatementObjectModel optionalModel);
    
        /// <summary>Start statement by statement id. </summary>
        /// <param name="statementId">of the statement to start.</param>
        void Start(int statementId);
    
        /// <summary>Stop statement by statement id. </summary>
        /// <param name="statementId">of the statement to stop.</param>
        void Stop(int statementId);
    
        /// <summary>Dispose statement by statement id. </summary>
        /// <param name="statementId">statementId of the statement to destroy</param>
        void Dispose(int statementId);
    
        /// <summary>Returns the statement by the given name, or null if no such statement exists. </summary>
        /// <param name="name">is the statement name</param>
        /// <returns>statement for the given name, or null if no such statement existed</returns>
        EPStatement GetStatementByName(string name);

        /// <summary>
        /// Returns an array of statement names. If no statement has been created, an empty array is returned.
        /// <para/>
        /// Only returns started and stopped statements. </summary>
        /// <value>statement names</value>
        string[] StatementNames { get; }

        /// <summary>Starts all stopped statements. First statement to fail supplies the exception. </summary>
        /// <throws>EPException to indicate a start error.</throws>
        void StartAllStatements();
    
        /// <summary>Stops all started statements. First statement to fail supplies the exception. </summary>
        /// <throws>EPException to indicate a start error.</throws>
        void StopAllStatements();
    
        /// <summary>Destroys all started statements. First statement to fail supplies the exception. </summary>
        /// <throws>EPException to indicate a start error.</throws>
        void DestroyAllStatements();

        /// <summary>
        /// Statements indicate that listeners have been added through this method.
        /// </summary>
        /// <param name="stmt">is the statement for which listeners were added</param>
        /// <param name="listeners">is the set of listeners after adding the new listener</param>
        /// <param name="isRecovery">if set to <c>true</c> [is recovery].</param>
        void UpdatedListeners(EPStatement stmt, EPStatementListenerSet listeners, bool isRecovery);

        string GetStatementNameById(int id);

        EPStatementSPI GetStatementById(int id);

        IDictionary<string, EPStatement> StmtNameToStmt { get; }

        StatementSpecCompiled GetStatementSpec(int statementName);
    }
}
