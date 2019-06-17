///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.statement.dispatch;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.runtime.client;

namespace com.espertech.esper.runtime.@internal.kernel.statement
{
    /// <summary>
    /// Statement SPI for statements operations for state transitions and internal management.
    /// </summary>
    public interface EPStatementSPI : EPStatement
    {
        /// <summary>Returns the statement id. </summary>
        /// <value>statement id</value>
        int StatementId { get; }

        /// <summary>Returns the statement context. </summary>
        /// <value>statement context</value>
        StatementContext StatementContext { get; }

        /// <summary>Gets or sets the parent view. </summary>
        /// <value>the statement viewable</value>
        Viewable ParentView { get; set; }

        void RecoveryUpdateListeners(EPStatementListenerSet listenerSet);

        UpdateDispatchView DispatchChildView { get; }

        void SetDestroyed();
    }
}