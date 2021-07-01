///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.compat.magic
{
    public class MagicString
    {
        public static Func<string, string> GetStringConformer(bool isCaseSensitive)
        {
            return isCaseSensitive ? (Func<string, string>) (s => s) : (s => s.ToUpper());
        }
    }
}