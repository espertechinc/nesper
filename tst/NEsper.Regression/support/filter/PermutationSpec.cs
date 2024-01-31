///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.regressionlib.support.filter
{
    public class PermutationSpec
    {
        public PermutationSpec(bool all)
        {
            IsAll = all;
            Specific = null;
        }

        public PermutationSpec(params int[] specific)
        {
            IsAll = false;
            Specific = specific;
        }

        public bool IsAll { get; }

        public int[] Specific { get; }
    }
} // end of namespace