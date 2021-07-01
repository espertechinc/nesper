///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.@internal.type
{
    public class RelationalOpDesc
    {
        private readonly Type _type;
        private readonly RelationalOpEnum _op;

        public RelationalOpDesc(Type type, RelationalOpEnum op) {
            _type = type;
            _op = op;
        }

        public Type Type => _type;

        public RelationalOpEnum Op => _op;

        protected bool Equals(RelationalOpDesc other)
        {
            return Equals(_type, other._type) && _op == other._op;
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

            return Equals((RelationalOpDesc) obj);
        }

        public override int GetHashCode()
        {
            unchecked {
                return ((_type != null ? _type.GetHashCode() : 0) * 397) ^ (int) _op;
            }
        }

        public override string ToString()
        {
            return $"{nameof(Type)}: {Type}, {nameof(Op)}: {Op}";
        }
    }
}