///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.@internal.settings
{
    public class ExtensionClassEmpty : ExtensionClass
    {
        public static readonly ExtensionClassEmpty INSTANCE = new ExtensionClassEmpty();

        public Type FindClassByName(string className)
        {
            return null;
        }
    }
} // end of namespace