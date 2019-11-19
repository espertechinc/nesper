///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.common.client;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    public class ExprFilterSpecLookupable
    {
        [NonSerialized] private readonly EventPropertyValueGetter getter;

        public ExprFilterSpecLookupable(
            string expression,
            EventPropertyValueGetter getter,
            Type returnType,
            bool isNonPropertyGetter)
        {
            this.getter = getter; // apparently, the getter can be null (wth)
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
            // For type consistency for recovery and serde define as boxed type
            ReturnType = returnType?.GetBoxedType();
            IsNonPropertyGetter = isNonPropertyGetter;
        }

        public string Expression { get; }

        public EventPropertyValueGetter Getter => getter;

        public Type ReturnType { get; }

        public bool IsNonPropertyGetter { get; }

        public override bool Equals(object o)
        {
            if (this == o) {
                return true;
            }

            if (o == null || GetType() != o.GetType()) {
                return false;
            }

            var that = (ExprFilterSpecLookupable) o;

            if (!Expression.Equals(that.Expression)) {
                return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            return Expression.GetHashCode();
        }

        public void AppendTo(TextWriter writer)
        {
            writer.Write(Expression);
        }

        public override string ToString()
        {
            return $"ExprFilterSpecLookupable{{expression='{Expression}'}}";
        }
    }
} // end of namespace