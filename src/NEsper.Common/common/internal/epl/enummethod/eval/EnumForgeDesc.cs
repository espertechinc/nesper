///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.rettype;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval
{
    public class EnumForgeDesc
    {
        public EnumForgeDesc(
            EPChainableType type,
            EnumForge forge)
        {
            Type = type;
            Forge = forge;
        }

        public EPChainableType Type { get; }

        public EnumForge Forge { get; }
    }
} // end of namespace