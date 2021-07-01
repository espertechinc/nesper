///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    public class ExprEnumerationForgeDesc
    {
        public ExprEnumerationForgeDesc(
            ExprEnumerationForge forge,
            bool istreamOnly,
            int directIndexStreamNumber)
        {
            Forge = forge;
            IsIstreamOnly = istreamOnly;
            DirectIndexStreamNumber = directIndexStreamNumber;
        }

        public ExprEnumerationForge Forge { get; }

        public bool IsIstreamOnly { get; }

        public int DirectIndexStreamNumber { get; }
    }
} // end of namespace