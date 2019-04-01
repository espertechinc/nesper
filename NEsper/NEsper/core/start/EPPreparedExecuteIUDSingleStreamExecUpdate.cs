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
using com.espertech.esper.epl.table.upd;
using com.espertech.esper.epl.updatehelper;
using com.espertech.esper.filter;

namespace com.espertech.esper.core.start
{
    public class EPPreparedExecuteIUDSingleStreamExecUpdate : EPPreparedExecuteIUDSingleStreamExec
    {
        public EPPreparedExecuteIUDSingleStreamExecUpdate(QueryGraph queryGraph, ExprNode optionalWhereClause, Attribute[] annotations, EventBeanUpdateHelper updateHelper, TableUpdateStrategy tableUpdateStrategy, ExprTableAccessNode[] optionalTableNodes, EPServicesContext services)
        {
            QueryGraph = queryGraph;
            OptionalWhereClause = optionalWhereClause;
            Annotations = annotations;
            UpdateHelper = updateHelper;
            TableUpdateStrategy = tableUpdateStrategy;
            OptionalTableNodes = optionalTableNodes;
            Services = services;
        }
    
        public EventBean[] Execute(FireAndForgetInstance fireAndForgetProcessorInstance)
        {
            return fireAndForgetProcessorInstance.ProcessUpdate(this);
        }

        public QueryGraph QueryGraph { get; private set; }

        public ExprNode OptionalWhereClause { get; private set; }

        public Attribute[] Annotations { get; private set; }

        public EventBeanUpdateHelper UpdateHelper { get; private set; }

        public TableUpdateStrategy TableUpdateStrategy { get; private set; }

        public ExprTableAccessNode[] OptionalTableNodes { get; private set; }

        public EPServicesContext Services { get; private set; }
    }
}
