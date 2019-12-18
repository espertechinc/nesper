///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


namespace com.espertech.esper.compat
{
    /// <summary>
    /// Represents a delegate that can be called.
    /// </summary>
    public delegate void Runnable();

    /// <summary>
    /// Represents an interface that can be called.
    /// </summary>
    public interface IRunnable
    {
        void Run();
    }
}
