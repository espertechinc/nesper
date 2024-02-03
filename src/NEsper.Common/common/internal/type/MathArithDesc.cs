///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.@internal.type
{
    public class MathArithDesc
    {
        private readonly Type _type;
        private readonly MathArithTypeEnum _arith;

        public MathArithDesc(
            Type type,
            MathArithTypeEnum arith)
        {
            _type = type;
            _arith = arith;
        }

        public Type Type => _type;

        public MathArithTypeEnum Arith => _arith;

        protected bool Equals(MathArithDesc other)
        {
            return Equals(_type, other._type) && _arith == other._arith;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != GetType()) {
                return false;
            }

            return Equals((MathArithDesc)obj);
        }

        public override int GetHashCode()
        {
            unchecked {
                return ((_type != null ? _type.GetHashCode() : 0) * 397) ^ (int)_arith;
            }
        }

        public override string ToString()
        {
            return $"{nameof(Type)}: {Type}, {nameof(Arith)}: {Arith}";
        }
    }
}