///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.resultset.select.core
{
    public class SelectSubscriberDescriptor
    {
        public SelectSubscriberDescriptor()
        {
            SelectClauseTypes = null;
            SelectClauseColumnNames = null;
            IsForClauseDelivery = false;
            GroupDelivery = null;
        }

        public SelectSubscriberDescriptor(
            Type[] selectClauseTypes,
            string[] selectClauseColumnNames,
            bool forClauseDelivery,
            ExprNode[] groupDelivery)
        {
            SelectClauseTypes = selectClauseTypes;
            SelectClauseColumnNames = selectClauseColumnNames;
            IsForClauseDelivery = forClauseDelivery;
            GroupDelivery = groupDelivery;
        }

        public Type[] SelectClauseTypes { get; }

        public string[] SelectClauseColumnNames { get; }

        public bool IsForClauseDelivery { get; }

        public ExprNode[] GroupDelivery { get; }
    }
} // end of namespace