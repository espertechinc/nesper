///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using Castle.Core.Internal;

using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.core.start
{
    /// <summary>
    /// Method to call to destroy an EPStatement.
    /// </summary>
    public class EPStatementDestroyCallbackList
    {
        private static readonly ILog Log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private Deque<Action> _callbacks;

        public void AddCallback(Action destroyCallback)
        {
            if (_callbacks == null)
            {
                _callbacks = new ArrayDeque<Action>(2);
            }
            _callbacks.Add(destroyCallback);
        }

        public void Destroy()
        {
            if (_callbacks != null)
            {
                _callbacks.ForEach(
                    destroyCallback =>
                    {
                        try
                        {
                            destroyCallback.Invoke();
                        }
                        catch (Exception ex)
                        {
                            Log.Error("Failed to destroy resource: " + ex.Message, ex);
                        }
                    });
            }
        }
    }
}
