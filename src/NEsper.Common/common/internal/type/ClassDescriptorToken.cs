///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.type
{
    public class ClassDescriptorToken
    {
        internal readonly string Sequence;
        internal readonly ClassDescriptorTokenType Token;

        public ClassDescriptorToken(
            ClassDescriptorTokenType token,
            string sequence)
        {
            Token = token;
            Sequence = sequence;
        }

        public override string ToString()
        {
            return $"ClassIdentifierWArrayToken{{token={Token}, sequence='{Sequence}'}}";
        }
    }
} // end of namespace