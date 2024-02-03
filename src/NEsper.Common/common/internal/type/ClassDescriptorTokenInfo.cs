///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Text.RegularExpressions;

namespace com.espertech.esper.common.@internal.type
{
    public class ClassDescriptorTokenInfo
    {
        private readonly Regex _regex;
        private readonly ClassDescriptorTokenType _token;

        public ClassDescriptorTokenInfo(
            Regex regex,
            ClassDescriptorTokenType token)
        {
            _regex = regex;
            _token = token;
        }

        public Regex Regex => _regex;

        public ClassDescriptorTokenType Token => _token;

        public override string ToString()
        {
            return $"ClassIdentifierWArrayTokenType{{regex={_regex}, token={_token}}}";
        }
    }
} // end of namespace