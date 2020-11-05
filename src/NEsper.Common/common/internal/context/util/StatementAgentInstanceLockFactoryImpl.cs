///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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
        private readonly bool fairlocks;
        private readonly bool disableLocking;

        public StatementAgentInstanceLockFactoryImpl(
            bool fairlocks,
            bool disableLocking)
        {
            this.fairlocks = fairlocks;
            this.disableLocking = disableLocking;
        }

        public IReaderWriterLock GetStatementLock(
            string statementName,
            Attribute[] annotations,
            bool stateless,
            StatementType statementType)
        {
            if (statementType.IsOnTriggerInfra()) {
                throw new UnsupportedOperationException("Operation not available for statement type " + statementType);
            }

            bool foundNoLock = AnnotationUtil.HasAnnotation(annotations, typeof(NoLockAttribute));
            if (disableLocking || foundNoLock || stateless) {
                return new VoidReaderWriterLock();
                //return new StatementAgentInstanceLockNoLockImpl(statementName);
            } else if (fairlocks) {
                return new FairReaderWriterLock();
            }
            else {
                return new SlimReaderWriterLock();
            }
        }
    }
} // end of namespace