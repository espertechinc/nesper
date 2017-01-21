///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.client;
using com.espertech.esper.util;

namespace com.espertech.esper.filter
{
    [Serializable]
    public class FilterSpecLookupable : MetaDefItem
    {
        [NonSerialized]
        private EventPropertyGetter _getter;

        public FilterSpecLookupable(String expression, EventPropertyGetter getter, Type returnType, bool isNonPropertyGetter)
        {
            Expression = expression;
            Getter = getter;
            ReturnType = returnType;
            IsNonPropertyGetter = isNonPropertyGetter;
        }

        public string Expression { get; private set; }

        public Type ReturnType { get; private set; }

        public bool IsNonPropertyGetter { get; private set; }

        public EventPropertyGetter Getter
        {
            get { return _getter; }
            private set { _getter = value; }
        }

        public bool Equals(FilterSpecLookupable other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Expression.Equals(other.Expression) && Equals(other.ReturnType, ReturnType);
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
        /// </returns>
        /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>. </param><filterpriority>2</filterpriority>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (FilterSpecLookupable)) return false;
            return Equals((FilterSpecLookupable) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Expression != null ? Expression.GetHashCode() : 0)*397) ^
                       (ReturnType != null ? ReturnType.GetHashCode() : 0);
            }
        }

        public void AppendTo(TextWriter writer)
        {
            writer.Write(Expression);
        }

        public override string ToString()
        {
            return "expression='" + Expression + '\'';
        }
    }
}
