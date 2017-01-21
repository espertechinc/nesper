///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat.threading;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.script;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.schedule;

namespace com.espertech.esper.support.core
{
    public class SupportExprEvaluatorContext : ExprEvaluatorContext
    {
        private readonly TimeProvider _timeProvider;

        public SupportExprEvaluatorContext(TimeProvider timeProvider)
        {
            _timeProvider = timeProvider;
        }

        public TimeProvider TimeProvider
        {
            get { return _timeProvider; }
        }

        public ExpressionResultCacheService ExpressionResultCacheService
        {
            get { return null; }
        }

        public int AgentInstanceId
        {
            get { return -1; }
        }

        public EventBean ContextProperties
        {
            get { return null; }
        }

        public AgentInstanceScriptContext AgentInstanceScriptContext
        {
            get { return null; }
        }

        public string StatementName
        {
            get { return null; }
        }

        public string EngineURI
        {
            get { return null; }
        }

        public int StatementId
        {
            get { return 1; }
        }

        public IReaderWriterLock AgentInstanceLock
        {
            get { return null; }
        }

        public StatementType? StatementType
        {
            get { return esper.core.service.StatementType.SELECT; }
        }

        public TableExprEvaluatorContext TableExprEvaluatorContext
        {
            get { return null; }
        }

        public object StatementUserObject
        {
            get { return null; }
        }
    }
}