namespace com.espertech.esperio
{
    /// <summary>
    /// A utility to manage the state transitions for an InputAdapter.
    /// </summary>
    public class AdapterStateManager
    {
        private AdapterState state = AdapterState.OPENED;
        private bool stateTransitionsAllowed = true;

        /// <summary>
        /// Gets the state
        /// </summary>
        /// <returns></returns>

        public AdapterState State
        {
            get { return state; }
        }

        /// <summary>
        /// Transition into the STARTED state (from the OPENED state).
        /// </summary>
        /// <throws>IllegalStateTransitionException if the transition is not allowed</throws>

        public void Start()
        {
            AssertStateTransitionsAllowed();
            if (state != AdapterState.OPENED)
            {
                throw new IllegalStateTransitionException("Cannot start from the " + state + " state");
            }
            state = AdapterState.STARTED;
        }

        /// <summary>
        /// Transition into the OPENED state.
        ///@throws IllegalStateTransitionException if the transition isn't allowed
        /// </summary>
        public void Stop()
        {
            AssertStateTransitionsAllowed();
            if (state != AdapterState.STARTED && state != AdapterState.PAUSED)
            {
                throw new IllegalStateTransitionException("Cannot stop from the " + state + " state");
            }
            state = AdapterState.OPENED;
        }

        /// <summary>
        /// Transition into the PAUSED state.
        /// </summary>
        /// <throws>IllegalStateTransitionException if the transition isn't allowed</throws>

        public void Pause()
        {
            AssertStateTransitionsAllowed();
            if (state != AdapterState.STARTED)
            {
                throw new IllegalStateTransitionException("Cannot pause from the " + state + " state");
            }
            state = AdapterState.PAUSED;
        }

        /// <summary>
        /// Transition into the STARTED state (from the PAUSED state).
        /// </summary>
        /// <throws>IllegalStateTransitionException if the state transition is not allowed</throws>

        public void Resume()
        {
            AssertStateTransitionsAllowed();
            if (state != AdapterState.PAUSED)
            {
                throw new IllegalStateTransitionException("Cannot resume from the " + state + " state");
            }
            state = AdapterState.STARTED;
        }

        /// <summary>
        /// Transition into the DESTROYED state.
        /// </summary>
        /// <throws>IllegalStateTransitionException if the transition isn't allowed</throws>

        public void Destroy()
        {
            if (state == AdapterState.DESTROYED)
            {
                throw new IllegalStateTransitionException("Cannot destroy from the " + state + " state");
            }
            state = AdapterState.DESTROYED;
        }

        /// <summary>
        /// Disallow future state changes, and throw an IllegalStateTransitionException if they
        /// are attempted.
        /// </summary>

        public void DisallowStateTransitions()
        {
            stateTransitionsAllowed = false;
        }

        private void AssertStateTransitionsAllowed()
        {
            if (!stateTransitionsAllowed)
            {
                throw new IllegalStateTransitionException("State transitions have been disallowed");
            }
        }
    }
}