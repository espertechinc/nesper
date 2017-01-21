///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.table;

namespace com.espertech.esper.core.start
{
    public class EPPreparedExecuteIUDSingleStreamExecInsert : EPPreparedExecuteIUDSingleStreamExec
    {
        private readonly ExprEvaluatorContext _exprEvaluatorContext;
        private readonly SelectExprProcessor _insertHelper;
        private readonly ExprTableAccessNode[] _optionalTableNodes;
        private readonly EPServicesContext _services;
    
        public EPPreparedExecuteIUDSingleStreamExecInsert(ExprEvaluatorContext exprEvaluatorContext, SelectExprProcessor insertHelper, ExprTableAccessNode[] optionalTableNodes, EPServicesContext services)
        {
            _exprEvaluatorContext = exprEvaluatorContext;
            _insertHelper = insertHelper;
            _optionalTableNodes = optionalTableNodes;
            _services = services;
        }
    
        public EventBean[] Execute(FireAndForgetInstance fireAndForgetProcessorInstance)
        {
            return fireAndForgetProcessorInstance.ProcessInsert(this);
        }

        public ExprEvaluatorContext ExprEvaluatorContext
        {
            get { return _exprEvaluatorContext; }
        }

        public SelectExprProcessor InsertHelper
        {
            get { return _insertHelper; }
        }

        public ExprTableAccessNode[] OptionalTableNodes
        {
            get { return _optionalTableNodes; }
        }

        public EPServicesContext Services
        {
            get { return _services; }
        }
    }
}
