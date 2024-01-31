///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.compile.multikey;
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
            GroupDeliveryMultiKey = null;
        }

        public SelectSubscriberDescriptor(
            Type[] selectClauseTypes,
            string[] selectClauseColumnNames,
            bool forClauseDelivery,
            ExprNode[] groupDelivery,
            MultiKeyClassRef groupDeliveryMultiKey)
        {
            SelectClauseTypes = selectClauseTypes;
            SelectClauseColumnNames = selectClauseColumnNames;
            IsForClauseDelivery = forClauseDelivery;
            GroupDelivery = groupDelivery;
            GroupDeliveryMultiKey = groupDeliveryMultiKey;
        }

        public Type[] SelectClauseTypes { get; }

        public string[] SelectClauseColumnNames { get; }

        public bool IsForClauseDelivery { get; }

        public ExprNode[] GroupDelivery { get; }

        public MultiKeyClassRef GroupDeliveryMultiKey { get; }
    }
} // end of namespace