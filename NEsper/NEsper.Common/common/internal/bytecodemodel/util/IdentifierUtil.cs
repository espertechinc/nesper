///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.bytecodemodel.util
{
    public static class IdentifierUtil
    {
        public static string GetIdentifierMayStartNumeric(string str)
        {
            return GetIdentifierMayStartNumericOrig(str);
        }

        public static string GetIdentifierMayStartNumericNew(string str)
        {
            using (var hash = SHA1.Create()) {
                return hash
                    .ComputeHash(str.GetUTF8Bytes())
                    .ToHexString();
            }
        }


        public static string GetIdentifierMayStartNumericOrig(string str)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < str.Length; i++) {
                var charAt = str[i];
                if (IsIdentifierPart(charAt)) {
                    sb.Append(charAt);
                }
                else {
                    sb.Append((int) charAt);
                }
            }

            return sb.ToString();
        }

        private static bool IsIdentifierPart(char cc)
        {
            if (char.IsLetterOrDigit(cc)) {
                return true;
            }

            switch (cc) {
                case '_':
                    return true;
            }

            return false;
        }

        public static string[] ProtectedKeyWords = new string[] {
            "event",
            "internal",
            "protected",
            "public",
            "private",
            "base",
            "lock",
            "object",
            "in",
            "out"
        };

        public static string CodeInclusionName(this string variableName)
        {
            if (variableName != null) {
                foreach (var keyword in ProtectedKeyWords) {
                    if (variableName == keyword) {
                        variableName = $"@{keyword}";
                    }
                }
            }

            return variableName;
        }
        
        public static string CodeInclusionTypeName(this string typeName)
        {
            if (typeName != null) {
                foreach (var keyword in ProtectedKeyWords) {
                    typeName = Regex.Replace(typeName, $"\\.({keyword})(\\.|$)", ".@$1$2");
                }

                typeName = typeName.Replace('+', '.');
            }

            return typeName;
        }
    }
} // end of namespace