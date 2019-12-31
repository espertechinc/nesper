///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.runtime.client.util
{
    /// <summary>
    /// A utility to manage the state transitions for an InputAdapter.
    /// </summary>
    public class AdapterStateManager
    {
        private bool _stateTransitionsAllowed = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdapterStateManager"/> class.
        /// </summary>
        public AdapterStateManager()
        {
            State = AdapterState.OPENED;
        }

        /// <summary>
        /// Gets the state.
        /// </summary>
        /// <returns>the state</returns>
        public AdapterState State { get; private set; }

        /// <summary>
        /// Transition into the STARTED state (from the OPENED state).
        /// </summary>
        /// <throws>IllegalStateTransitionException if the transition is not allowed</throws>
    	public void Start()
        {
            AssertStateTransitionsAllowed();
            if (State != AdapterState.OPENED)
            {
                throw new IllegalStateTransitionException("Cannot start from the " + State + " state");
            }
            State = AdapterState.STARTED;
        }

        /// <summary>
        /// Transition into the OPENED state.
        /// </summary>
        /// <throws>IllegalStateTransitionException if the transition isn't allowed</throws>
    	public void Stop()
        {
            AssertStateTransitionsAllowed();
            if (State != AdapterState.STARTED && State != AdapterState.PAUSED)
            {
                throw new IllegalStateTransitionException("Cannot stop from the " + State + " state");
            }
            State = AdapterState.OPENED;
        }

        /// <summary>
        /// Transition into the PAUSED state.
        /// </summary>
        /// <throws>IllegalStateTransitionException if the transition isn't allowed</throws>
    	public void Pause()
        {
            AssertStateTransitionsAllowed();
            if (State != AdapterState.STARTED)
            {
                throw new IllegalStateTransitionException("Cannot pause from the " + State + " state");
            }
            State = AdapterState.PAUSED;
        }

        /// <summary>
        /// Transition into the STARTED state (from the PAUSED state).
        /// </summary>
        /// <throws>IllegalStateTransitionException if the state transition is not allowed</throws>
    	public void Resume()
        {
            AssertStateTransitionsAllowed();
            if (State != AdapterState.PAUSED)
            {
                throw new IllegalStateTransitionException("Cannot resume from the " + State + " state");
            }
            State = AdapterState.STARTED;
        }

        /// <summary>
        /// Transition into the DESTROYED state.
        /// </summary>
        /// <throws>IllegalStateTransitionException if the transition isn't allowed</throws>
    	public void Dispose()
        {
            if (State == AdapterState.DESTROYED)
            {
                throw new IllegalStateTransitionException("Cannot destroy from the " + State + " state");
            }
            State = AdapterState.DESTROYED;
        }

        /// <summary>
        /// Disallow future state changes, and throw an IllegalStateTransitionException if they are attempted.
        /// </summary>
    	public void DisallowStateTransitions()
        {
            _stateTransitionsAllowed = false;
        }

        /// <summary>
        /// Asserts the state transitions allowed.
        /// </summary>
    	private void AssertStateTransitionsAllowed()
        {
            if (!_stateTransitionsAllowed)
            {
                throw new IllegalStateTransitionException("State transitions have been disallowed");
            }
        }
    }
}