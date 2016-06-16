///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.view;

namespace com.espertech.esper.core.service
{
    /// <summary>
    /// Statement SPI for statements operations for state transitions and internal management.
    /// </summary>
    public interface EPStatementSPI : EPStatement
    {
        /// <summary>Returns the statement id. </summary>
        /// <value>statement id</value>
        int StatementId { get; }
        
        /// <summary>Returns the statements expression without annotations. </summary>
        /// <value>expression</value>
        string ExpressionNoAnnotations { get; }

        /// <summary>
        /// Returns the current set of listeners for read-only operations.
        /// </summary>
        /// <param name="value">listener set</param>
        /// <param name="isRecovery">if set to <c>true</c> [is recovery].</param>
        void SetListenerSet(EPStatementListenerSet value, bool isRecovery);

        /// <summary>
        /// Returns the current set of listeners for read-only operations.
        /// </summary>
        EPStatementListenerSet GetListenerSet();

        /// <summary>Set statement state. </summary>
        /// <param name="currentState">new current state</param>
        /// <param name="timeLastStateChange">the timestamp the statement changed state</param>
        void SetCurrentState(EPStatementState currentState, long timeLastStateChange);

        /// <summary>Gets or sets the parent view. </summary>
        /// <value>the statement viewable</value>
        Viewable ParentView { get; set; }

        /// <summary>Returns additional metadata about a statement. </summary>
        /// <value>statement metadata</value>
        StatementMetadata StatementMetadata { get; }

        /// <summary>Returns the statement context. </summary>
        /// <value>statement context</value>
        StatementContext StatementContext { get; }

        /// <summary>True if an explicit statement name has been provided, false if the statement name is system-generated. </summary>
        /// <value>indicator if statement name exists</value>
        bool IsNameProvided { get; }
    }
}
