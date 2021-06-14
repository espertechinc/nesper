///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.util;

namespace com.espertech.esper.common.@internal.context.util
{
    public class AgentInstanceArraySafeEnumerator : SafeEnumerator<EventBean>
    {
        private readonly AgentInstance[] _instances;
        private readonly IEnumerator<EventBean> _underlying;

        /// <summary>Initializes a new instance of the <see cref="AgentInstanceArraySafeEnumerator" /> class.</summary>
        /// <param name="instances">The instances.</param>
        public AgentInstanceArraySafeEnumerator(AgentInstance[] instances)
        {
            _instances = instances;

            foreach (var instance in instances) {
                instance
                    .AgentInstanceContext
                    .EpStatementAgentInstanceHandle
                    .StatementAgentInstanceLock
                    .AcquireWriteLock();
            }

            _underlying = CreateUnderlying();
        }

        /// <summary>Releases unmanaged and - optionally - managed resources.</summary>
        public void Dispose()
        {
            foreach (var instance in _instances) {
                var agentInstanceContext = instance.AgentInstanceContext;
                if (agentInstanceContext.StatementContext.EpStatementHandle.HasTableAccess) {
                    agentInstanceContext.TableExprEvaluatorContext.ReleaseAcquiredLocks();
                }
            }
        }

        /// <summary>Moves the next.</summary>
        /// <returns></returns>
        public bool MoveNext()
        {
            return _underlying.MoveNext();
        }

        /// <summary>Gets the current.</summary>
        /// <value>The current.</value>
        public EventBean Current => _underlying.Current;

        /// <summary>Gets the current.</summary>
        /// <value>The current.</value>
        object IEnumerator.Current => Current;

        /// <summary>
        ///     <para>
        ///         Resets this instance.
        ///     </para>
        /// </summary>
        /// <exception cref="System.NotSupportedException"></exception>
        public void Reset()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        ///     Creates the underlying enumerator.
        /// </summary>
        private IEnumerator<EventBean> CreateUnderlying()
        {
            foreach (var instance in _instances) {
                foreach (var eventBean in instance.FinalView) {
                    yield return eventBean;
                }
            }
        }
    }
} // end of namespace