///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.table;
using com.espertech.esper.epl.join.plan;
using com.espertech.esper.filter;

namespace com.espertech.esper.core.start
{
    public class EPPreparedExecuteIUDSingleStreamExecDelete : EPPreparedExecuteIUDSingleStreamExec
    {
        private readonly QueryGraph _queryGraph;
        private readonly ExprNode _optionalWhereClause;
        private readonly Attribute[] _annotations;
        private readonly ExprTableAccessNode[] _optionalTableNodes;
        private readonly EPServicesContext _services;
    
        public EPPreparedExecuteIUDSingleStreamExecDelete(QueryGraph queryGraph, ExprNode optionalWhereClause, Attribute[] annotations, ExprTableAccessNode[] optionalTableNodes, EPServicesContext services)
        {
            _queryGraph = queryGraph;
            _optionalWhereClause = optionalWhereClause;
            _annotations = annotations;
            _optionalTableNodes = optionalTableNodes;
            _services = services;
        }
    
        public EventBean[] Execute(FireAndForgetInstance fireAndForgetProcessorInstance)
        {
            return fireAndForgetProcessorInstance.ProcessDelete(this);
        }

        public QueryGraph QueryGraph
        {
            get => _queryGraph;
        }

        public ExprNode OptionalWhereClause
        {
            get => _optionalWhereClause;
        }

        public Attribute[] Annotations
        {
            get => _annotations;
        }

        public ExprTableAccessNode[] OptionalTableNodes
        {
            get => _optionalTableNodes;
        }

        public EPServicesContext Services
        {
            get => _services;
        }
    }
}
