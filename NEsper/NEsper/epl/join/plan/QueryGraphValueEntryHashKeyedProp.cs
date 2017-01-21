///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;

namespace com.espertech.esper.epl.join.plan
{
    public class QueryGraphValueEntryHashKeyedProp : QueryGraphValueEntryHashKeyed
    {
        public QueryGraphValueEntryHashKeyedProp(ExprNode keyExpr, String keyProperty)
            : base(keyExpr)
        {
            KeyProperty = keyProperty;
        }

        public string KeyProperty { get; private set; }

        public override String ToQueryPlan()
        {
            return KeyProperty;
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        {
            return string.Format("KeyProperty: {0}", KeyProperty);
        }
    }
}
