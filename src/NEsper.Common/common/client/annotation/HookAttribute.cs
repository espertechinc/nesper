///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.client.annotation
{
    public class HookAttribute : Attribute
    {
        /// <summary>Returns the simple class name (using imports) or fully-qualified class name of the hook. </summary>
        /// <returns>class name</returns>
        public virtual string Hook { get; set; }

        /// <summary>Returns hook type. </summary>
        /// <returns>hook type</returns>
        public virtual HookType HookType { get; set; }
    }
} // end of namespace