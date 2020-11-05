namespace com.espertech.esperio
{
    /// <summary>
    /// A utility to manage the state transitions for an InputAdapter.
    /// </summary>
    public class AdapterStateManager
    {
        private AdapterState _state = AdapterState.OPENED;
        private bool _stateTransitionsAllowed = true;

        /// <summary>
        /// Gets the state
        /// </summary>
        /// <returns></returns>

        public AdapterState State
        {
            get { return _state; }
        }

        /// <summary>
        /// Transition into the STARTED state (from the OPENED state).
        /// </summary>
        /// <throws>IllegalStateTransitionException if the transition is not allowed</throws>

        public void Start()
        {
            AssertStateTransitionsAllowed();
            if (_state != AdapterState.OPENED)
            {
                throw new IllegalStateTransitionException("Cannot start from the " + _state + " state");
            }
            _state = AdapterState.STARTED;
        }

        /// <summary>
        /// Transition into the OPENED state.
        ///@throws IllegalStateTransitionException if the transition isn't allowed
        /// </summary>
        public void Stop()
        {
            AssertStateTransitionsAllowed();
            if (_state != AdapterState.STARTED && _state != AdapterState.PAUSED)
            {
                throw new IllegalStateTransitionException("Cannot stop from the " + _state + " state");
            }
            _state = AdapterState.OPENED;
        }

        /// <summary>
        /// Transition into the PAUSED state.
        /// </summary>
        /// <throws>IllegalStateTransitionException if the transition isn't allowed</throws>

        public void Pause()
        {
            AssertStateTransitionsAllowed();
            if (_state != AdapterState.STARTED)
            {
                throw new IllegalStateTransitionException("Cannot pause from the " + _state + " state");
            }
            _state = AdapterState.PAUSED;
        }

        /// <summary>
        /// Transition into the STARTED state (from the PAUSED state).
        /// </summary>
        /// <throws>IllegalStateTransitionException if the state transition is not allowed</throws>

        public void Resume()
        {
            AssertStateTransitionsAllowed();
            if (_state != AdapterState.PAUSED)
            {
                throw new IllegalStateTransitionException("Cannot resume from the " + _state + " state");
            }
            _state = AdapterState.STARTED;
        }

        /// <summary>
        /// Transition into the DESTROYED state.
        /// </summary>
        /// <throws>IllegalStateTransitionException if the transition isn't allowed</throws>

        public void Destroy()
        {
            if (_state == AdapterState.DESTROYED)
            {
                throw new IllegalStateTransitionException("Cannot destroy from the " + _state + " state");
            }
            _state = AdapterState.DESTROYED;
        }

        /// <summary>
        /// Disallow future state changes, and throw an IllegalStateTransitionException if they
        /// are attempted.
        /// </summary>

        public void DisallowStateTransitions()
        {
            _stateTransitionsAllowed = false;
        }

        private void AssertStateTransitionsAllowed()
        {
            if (!_stateTransitionsAllowed)
            {
                throw new IllegalStateTransitionException("State transitions have been disallowed");
            }
        }
    }
}