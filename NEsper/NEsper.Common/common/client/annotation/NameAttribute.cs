///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;

namespace com.espertech.esper.common.client.annotation
{
    public class NameAttribute : Attribute
    {
        private string _value;

        public NameAttribute(string name)
        {
            _value = name;
        }

        public NameAttribute()
        {
        }

        public virtual string Value => _value ?? throw new IllegalStateException("name value not set");

        protected bool Equals(NameAttribute other)
        {
            return base.Equals(other) && string.Equals(Value, other.Value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != this.GetType()) {
                return false;
            }

            return Equals((NameAttribute) obj);
        }

        public override string ToString()
        {
            return string.Format("@Name(\"{0}\")", Value);
        }

        public override int GetHashCode()
        {
            unchecked {
                return (base.GetHashCode() * 397) ^ (Value != null ? Value.GetHashCode() : 0);
            }
        }
    }
}