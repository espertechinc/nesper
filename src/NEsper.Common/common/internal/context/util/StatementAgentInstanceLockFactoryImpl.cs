///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.epl.annotation;
using com.espertech.esper.compat;
using com.espertech.esper.compat.threading.locks;


namespace com.espertech.esper.common.@internal.context.util
{
    /// <summary>
    /// Provides statement-level locks.
    /// </summary>
    public class StatementAgentInstanceLockFactoryImpl : StatementAgentInstanceLockFactory
    {
        private readonly IReaderWriterLockManager _readerWriterLockManager;
        private readonly bool _fairlocks;
        private readonly bool _disableLocking;
        private readonly bool _lockLogging;

        public StatementAgentInstanceLockFactoryImpl(
            bool fairlocks,
            bool disableLocking,
            bool lockLogging,
            IReaderWriterLockManager readerWriterLockManager)
        {
            _readerWriterLockManager = readerWriterLockManager;
            _fairlocks = fairlocks;
            _disableLocking = disableLocking;
            _lockLogging = lockLogging;
        }

        public IReaderWriterLock GetStatementLock(
            string statementName,
            int cpid,
            Attribute[] annotations,
            bool stateless,
            StatementType statementType)
        {
            if (statementType.IsOnTriggerInfra()) {
                throw new UnsupportedOperationException("Operation not available for statement type " + statementType);
            }

            var foundNoLock = AnnotationUtil.HasAnnotation(annotations, typeof(NoLockAttribute));
            if (_disableLocking || foundNoLock || stateless) {
                return new VoidReaderWriterLock();
                //return new StatementAgentInstanceLockNoLockImpl(statementName);
            } else if (_fairlocks) {
                return new FairReaderWriterLock();
            }

            if (!_lockLogging) {
                return new StatementAgentInstanceLockRW(_fairlocks);
            }

            return new StatementAgentInstanceLockRWLogging(_fairlocks, statementName, cpid);
        }
    }
} // end of namespace