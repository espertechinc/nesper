///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.epl.variable
{
    /// <summary>
    /// A callback interface for indicating a change in variable value.
    /// <param name="newValue">new value</param>
    /// <param name="oldValue">old value</param>
    /// </summary>
    public delegate void VariableChangeCallback(Object newValue, Object oldValue);
} // End of namespace
