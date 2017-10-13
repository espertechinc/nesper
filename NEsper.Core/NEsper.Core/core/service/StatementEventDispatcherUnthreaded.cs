///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.core.service
{
    /// <summary>
    /// Dispatcher for statement lifecycle events to service provider statement state listeners.
    /// </summary>
    public class StatementEventDispatcherUnthreaded
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly EPServiceProvider _serviceProvider;
        private readonly IEnumerable<EPStatementStateListener> _statementListeners;

        /// <summary>Ctor. </summary>
        /// <param name="serviceProvider">engine instance</param>
        /// <param name="statementListeners">listeners to dispatch to</param>
        public StatementEventDispatcherUnthreaded(EPServiceProvider serviceProvider,
                                                  IEnumerable<EPStatementStateListener> statementListeners)
        {
            _serviceProvider = serviceProvider;
            _statementListeners = statementListeners;
        }

        #region StatementLifecycleObserver Members

        public void Observe(StatementLifecycleEvent theEvent)
        {
            if (theEvent.EventType == StatementLifecycleEvent.LifecycleEventType.CREATE)
            {
                IEnumerator<EPStatementStateListener> it = _statementListeners.GetEnumerator();
                for (; it.MoveNext(); )
                {
                    try
                    {
                        it.Current.OnStatementCreate(_serviceProvider, theEvent.Statement);
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Caught runtime exception in onStatementCreate callback:" + ex.Message, ex);
                    }
                }
            }
            else if (theEvent.EventType == StatementLifecycleEvent.LifecycleEventType.STATECHANGE)
            {
                IEnumerator<EPStatementStateListener> it = _statementListeners.GetEnumerator();
                for (; it.MoveNext(); )
                {
                    try
                    {
                        it.Current.OnStatementStateChange(_serviceProvider, theEvent.Statement);
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Caught runtime exception in onStatementCreate callback:" + ex.Message, ex);
                    }
                }
            }
        }

        #endregion
    }
}