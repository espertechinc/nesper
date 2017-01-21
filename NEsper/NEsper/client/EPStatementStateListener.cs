///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.client
{
    /// <summary>
    /// Interface for receiving callback events pertaining to statement creation and
    /// statement state transitions.
    /// <para/>
    /// Implementations must not block the operation.
    /// </summary>
    public interface EPStatementStateListener
    {
        /// <summary>
        /// Called to indicate that a new statement has been created in stopped state.
        /// <para/>
        /// The #onStatementStateChange method is also invoked upon statement start.
        /// </summary>
        /// <param name="serviceProvider">the service provider instance under which the statement has been created</param>
        /// <param name="statement">the new statement</param>
        void OnStatementCreate(EPServiceProvider serviceProvider, EPStatement statement);
    
        /// <summary>
        /// Called to indicate that a statement has changed state.
        /// </summary>
        /// <param name="serviceProvider">the service provider instance under which the statement has been created</param>
        /// <param name="statement">the statement that changed state</param>
        void OnStatementStateChange(EPServiceProvider serviceProvider, EPStatement statement);
    }

    public class StatementStateEventArgs : EventArgs
    {
        public EPServiceProvider ServiceProvider { get; set; }
        public EPStatement Statement { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="StatementStateEventArgs"/> class.
        /// </summary>
        public StatementStateEventArgs()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StatementStateEventArgs"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="statement">The statement.</param>
        public StatementStateEventArgs(EPServiceProvider serviceProvider, EPStatement statement)
        {
            ServiceProvider = serviceProvider;
            Statement = statement;
        }
    }
}
