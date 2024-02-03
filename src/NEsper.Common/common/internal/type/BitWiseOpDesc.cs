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
    public class BitWiseOpDesc
    {
        private readonly Type _type;
        private readonly BitWiseOpEnum _bitwise;

        public BitWiseOpDesc(
            Type type,
            BitWiseOpEnum bitwise)
        {
            _type = type;
            _bitwise = bitwise;
        }

        public Type Type => _type;

        public BitWiseOpEnum Bitwise => _bitwise;

        protected bool Equals(BitWiseOpDesc other)
        {
            return Equals(_type, other._type) && _bitwise == other._bitwise;
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

            return Equals((BitWiseOpDesc)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_type, (int)_bitwise);
        }

        public override string ToString()
        {
            return $"{nameof(Type)}: {Type}, {nameof(Bitwise)}: {Bitwise}";
        }
    }
}