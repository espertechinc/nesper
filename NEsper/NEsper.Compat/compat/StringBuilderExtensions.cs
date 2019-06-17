///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Text;

namespace com.espertech.esper.compat
{
    public static class StringBuilderExtensions
    {
        public static int IndexOf(this StringBuilder stringBuilder, string value, int startIndex = 0)
        {
            int valueLength = value.Length;
            int stringLength = stringBuilder.Length - valueLength;
            for(int ii = startIndex ; ii < stringLength ; ii++)
            {
                bool isMatch = true;

                for(int jj = 0 ; jj < valueLength ; jj++)
                {
                    if (value[jj] != stringBuilder[ii + jj])
                    {
                        isMatch = false;
                        break;
                    }
                }

                if (isMatch)
                {
                    return ii;
                }
            }

            return -1;
        }
    }
}
