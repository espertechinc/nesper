///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.client.deploy
{
    /// <summary>
    /// For use with server environments that support dynamic engine initialization
    /// (enterprise edition server), indicates that this method should be called after 
    /// the engine instance is initialized and the initial set of EPL statements have 
    /// been deployed, for example to set up listeners and subscribers.
    /// <para/>
    /// Apply this attribute to any method that accepts a single string parameter providing 
    /// the engine name.
    /// </summary>
    //@Retention(RetentionPolicy.RUNTIME)
    //@Target(ElementType.METHOD)
    public class EngineInitializerAttribute : Attribute
    {
    }
}
