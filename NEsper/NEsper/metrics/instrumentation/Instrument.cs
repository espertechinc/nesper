///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.metrics.instrumentation
{
    public class Instrument : IDisposable
    {
        private readonly Action<Instrumentation> _onExit;
        private readonly Instrumentation _instrumentation;

        private static readonly Instrument VOID = new Instrument(
            i => { },
            i => { });

        public static IDisposable With(Action<Instrumentation> onEntry, Action<Instrumentation> onExit)
        {
            if (InstrumentationHelper.ENABLED)
            {
                return new Instrument(onEntry, onExit);
            }
            else
            {
                return VOID;
            }
        }

        public static void With(Action<Instrumentation> onEntry, Action<Instrumentation> onExit, Action action)
        {
            if (InstrumentationHelper.ENABLED)
            {
                var instrumentation = InstrumentationHelper.Get();
                onEntry(instrumentation);
                try
                {
                    action();
                }
                finally
                {
                    onExit(instrumentation);
                }
            }
            else
            {
                action();
            }
        }

        public static T With<T>(Action<Instrumentation> onEntry, Action<Instrumentation> onExit, Func<T> action)
        {
            if (InstrumentationHelper.ENABLED)
            {
                var instrumentation = InstrumentationHelper.Get();
                onEntry(instrumentation);
                try
                {
                    return action();
                }
                finally
                {
                    onExit(instrumentation);
                }
            }
            else
            {
                return action();
            }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="Instrument"/> class.
        /// </summary>
        /// <param name="onEntry">The on entry.</param>
        /// <param name="onExit">The on exit.</param>
        public Instrument(Action<Instrumentation> onEntry, Action<Instrumentation> onExit)
        {
            if (InstrumentationHelper.ENABLED)
            {
                _instrumentation = InstrumentationHelper.Get();
                _onExit = onExit;
                onEntry.Invoke(_instrumentation);
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (InstrumentationHelper.ENABLED)
            {
                _onExit.Invoke(_instrumentation);
            }
        }
    }
}
