///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.client.hook.condition
{
    /// <summary>
    /// Interface for a handler registered with an engine instance to receive reported 
    /// engine conditions.
    /// <para/>
    /// Handle the engine condition as contained in the context object passed.
    /// </summary>
    /// <param name="context">the condition information</param>
    public delegate void ConditionHandler(ConditionHandlerContext context);
}