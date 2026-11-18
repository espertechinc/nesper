///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace com.espertech.esper.compat
{
    public class HighResolutionTimeProvider
    {
        public static readonly HighResolutionTimeProvider Instance = new HighResolutionTimeProvider();

        public long CurrentTime => DateTimeHelper.CurrentTimeTicks;
    }
}
