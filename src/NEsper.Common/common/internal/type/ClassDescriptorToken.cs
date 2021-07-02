///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.type
{
    public class ClassDescriptorToken
    {
        private readonly ClassDescriptorTokenType _token;
        private readonly string _sequence;

        public ClassDescriptorTokenType Token => _token;

        public string Sequence => _sequence;

        public ClassDescriptorToken(
            ClassDescriptorTokenType token,
            string sequence)
        {
            _token = token;
            _sequence = sequence;
        }

        public override string ToString()
        {
            return $"{nameof(_token)}: {_token}, {nameof(_sequence)}: {_sequence}";
        }
    }
} // end of namespace