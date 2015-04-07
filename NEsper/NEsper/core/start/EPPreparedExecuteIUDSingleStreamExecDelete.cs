///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.filter;

namespace com.espertech.esper.core.start
{
    public class EPPreparedExecuteIUDSingleStreamExecDelete : EPPreparedExecuteIUDSingleStreamExec
    {
        private readonly FilterSpecCompiled _filter;
        private readonly ExprNode _optionalWhereClause;
        private readonly Attribute[] _annotations;
        private readonly ExprTableAccessNode[] _optionalTableNodes;
        private readonly EPServicesContext _services;
    
        public EPPreparedExecuteIUDSingleStreamExecDelete(FilterSpecCompiled filter, ExprNode optionalWhereClause, Attribute[] annotations, ExprTableAccessNode[] optionalTableNodes, EPServicesContext services)
        {
            _filter = filter;
            _optionalWhereClause = optionalWhereClause;
            _annotations = annotations;
            _optionalTableNodes = optionalTableNodes;
            _services = services;
        }
    
        public EventBean[] Execute(FireAndForgetInstance fireAndForgetProcessorInstance)
        {
            return fireAndForgetProcessorInstance.ProcessDelete(this);
        }

        public FilterSpecCompiled Filter
        {
            get { return _filter; }
        }

        public ExprNode OptionalWhereClause
        {
            get { return _optionalWhereClause; }
        }

        public Attribute[] Annotations
        {
            get { return _annotations; }
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
