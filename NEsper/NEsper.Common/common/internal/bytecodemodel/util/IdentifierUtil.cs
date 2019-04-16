///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Text;

namespace com.espertech.esper.common.@internal.bytecodemodel.util
{
    public class IdentifierUtil
    {
        public static string GetIdentifierMayStartNumeric(string str)
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
    }
} // end of namespace