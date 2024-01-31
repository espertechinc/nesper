///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Text.RegularExpressions;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.type
{
    public class ClassDescriptorTokenizer
    {
        private readonly LinkedList<ClassDescriptorTokenInfo> tokens = new LinkedList<ClassDescriptorTokenInfo>();

        public void Add(
            string pattern,
            ClassDescriptorTokenType token)
        {
            if (!pattern.StartsWith("^")) {
                pattern = $"^{pattern}";
            }
            var regex = new Regex(pattern, RegexOptions.None); 
            tokens.AddLast(new ClassDescriptorTokenInfo(regex, token));
        }

        public ArrayDeque<ClassDescriptorToken> Tokenize(string str)
        {
            var tokens = new ArrayDeque<ClassDescriptorToken>(4);
            while (!string.IsNullOrEmpty(str)) {
                var match = false;
                foreach (var info in this.tokens) {
                    var regex = info.Regex;
                    var m = regex.Match(str);
                    if (m != Match.Empty) {
                        match = true;

                        var tok = m.Groups[0].Value.Trim();
                        tokens.Add(new ClassDescriptorToken(info.Token, tok));

                        str = regex.Replace(str, "", 1).Trim();
                        break;
                    }
                }

                if (!match) {
                    throw new ClassDescriptorParseException($"Unexpected token '{str}'");
                }
            }

            return tokens;
        }
    }
} // end of namespace