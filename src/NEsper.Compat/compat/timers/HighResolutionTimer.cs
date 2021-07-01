///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

using com.espertech.esper.compat.logging;

namespace com.espertech.esper.compat.timers
{
#if NETCORE
#else
    /// <summary>
    /// Windows timers are based on the system timer.  The system timer runs at a
    /// frequency of about 50-60 hz depending on your machine.  This presents a 
    /// problem for applications that require finer granularity.  The HighRes timer
    /// allows us to get better granularity, but currently it only works on Windows.
    /// 
    /// Thanks to Luc Pattyn for clarifying some of the issues with high resolution
    /// timers with the post on CodeProject.
    /// </summary>

    public class HighResolutionTimer : ITimer
    {
        /// <summary>
        /// Delegate that is called by the windows multimedia timer upon trigger
        /// of the timer.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="msg"></param>
        /// <param name="userCtx"></param>
        /// <param name="rsv1"></param>
        /// <param name="rsv2"></param>

        public delegate void TimerEventHandler(uint id, uint msg, IntPtr userCtx, uint rsv1, uint rsv2);

        internal class NativeMethods
        {
	        [DllImport("WinMM.dll", SetLastError = true)]
            internal static extern uint timeSetEvent(uint msDelay, uint msResolution, TimerEventHandler handler, IntPtr userCtx, uint eventType);
	
	        [DllImport("WinMM.dll", SetLastError = true)]
            internal static extern uint timeKillEvent(uint timerEventId);
        }

        private const int TIME_ONESHOT    = 0x0000   ; /* program timer for single event */
        private const int TIME_PERIODIC   = 0x0001   ; /* program for continuous periodic event */

        /// <summary>
        /// Callback is a function
        /// </summary>
        private const uint TIME_CALLBACK_FUNCTION = 0x0000 ;
        /// <summary>
        /// Callback is an event -- use SetEvent
        /// </summary>
        private const uint TIME_CALLBACK_EVENT_SET = 0x0010 ;
        /// <summary>
        /// Callback is an event -- use PulseEvent
        /// </summary>
        private const uint TIME_CALLBACK_EVENT_PULSE = 0x0020 ;
        /// <summary>
        /// This flag prevents the event from occurring after the user calls timeKillEvent() to
        /// destroy it.
        /// </summary>
        private const uint TIME_KILL_SYNCHRONOUS = 0x0100 ;

        private readonly TimerCallback _timerCallback;
        private readonly object _state;
        private readonly uint _offsetInMillis;
        private readonly uint _intervalInMillis;
        private uint? _timer;
        private readonly IntPtr _data;

        /// <summary>
        /// Initializes a new instance of the <see cref="HighResolutionTimer"/> class.
        /// </summary>
        /// <param name="timerCallback">The timer callback.</param>
        /// <param name="state">The state.</param>
        /// <param name="offsetInMillis">The due time.</param>
        /// <param name="intervalInMillis">The period.</param>
        
        public HighResolutionTimer(
            TimerCallback timerCallback,
            object state,
            long offsetInMillis,
            long intervalInMillis )
        {
            _timerCallback = timerCallback;
            _state = state;
            _offsetInMillis = (uint) offsetInMillis;
            _intervalInMillis = (uint) intervalInMillis;
            _data = Marshal.GetIUnknownForObject( this ) ;
            _timer = null;

            if (Log.IsDebugEnabled)
            {
                Log.Debug(string.Format(
                    ".Constructor - Creating timer: dueTime={0}, period={1}, data={2}",
                    _offsetInMillis,
                    _intervalInMillis,
                    _data ) ) ;
            }

            Start();
        }

        /// <summary>
        /// Destructor
        /// </summary>
        
        ~HighResolutionTimer()
        {
        	Dispose() ;
        }
        
        /// <summary>
        /// Called when timer event occurs.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="msg">The MSG.</param>
        /// <param name="userCtx">The user CTX.</param>
        /// <param name="rsv1">The RSV1.</param>
        /// <param name="rsv2">The RSV2.</param>
        
        private static void OnTimerEvent(uint id, uint msg, IntPtr userCtx, uint rsv1, uint rsv2)
        {
        	// Check for an invalid pointer.  Appears that this condition
        	// occurs when the GC moves memory around.  Because we use the
        	// IntPtr to the data that is provided to use through the Marshaller
        	// we expect that this value should not change.
        	
            var ptrLongValue = userCtx.ToInt64();
            if ( ptrLongValue <= 0 )
            {
                return;
            }
            
            // Convert the IntPtr back into its object form.  Once in its object form, the
            // object resolution timer can be called.

            try
            {
                var timer = Marshal.GetObjectForIUnknown(userCtx) as HighResolutionTimer;
                timer?._timerCallback(timer._state);
            }
            catch (Exception) {
                // ignored
            }
        }

        /// <summary>
        /// Starts this instance.
        /// </summary>
        
        private void Start()
        {
            if (_offsetInMillis == 0)
            {
                _timer = NativeMethods.timeSetEvent(
                    _intervalInMillis,
                    _intervalInMillis,
                    _delegate,
                    _data,
                    TIME_PERIODIC | TIME_KILL_SYNCHRONOUS);

                Log.Debug(".Start - Timer#{0}", _timer);

                if (_timer.Value == 0)
                {
                    throw new TimerException("Unable to allocate multimedia timer");
                }

                _timerTable[_timer.Value] = _timer.Value;
                _timerCallback(_state);
            }
            else
            {
                _timer = NativeMethods.timeSetEvent(
                    _offsetInMillis,
                    _intervalInMillis,
                    _delegate,
                    _data,
                    TIME_PERIODIC | TIME_KILL_SYNCHRONOUS);

                Log.Debug(".Start - Timer#{0}", _timer);
                
                if (_timer.Value == 0)
                {
                    throw new TimerException("Unable to allocate multimedia timer");
                }

                _timerTable[_timer.Value] = _timer.Value;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        
        public void Dispose()
        {
            Log.Debug( ".Dispose" ) ;

            if (_timer.HasValue)
            {
                Log.Debug(".Dispose - Timer#{0}", _timer.Value) ;
                NativeMethods.timeKillEvent(_timer.Value);
                _timerTable.Remove(_timer.Value);
                _timer = null;
            }
            
            GC.SuppressFinalize( this ) ;
        }

        /// <summary>
        /// Reference to the appDomain for this instance
        /// </summary>

        static AppDomain _appDomain;
        static Dictionary<uint, uint> _timerTable;
        static readonly TimerEventHandler _delegate;

        static HighResolutionTimer()
        {
            _timerTable = new Dictionary<uint, uint>();
            _appDomain = AppDomain.CurrentDomain;
            _appDomain.DomainUnload += new EventHandler(OnAppDomainUnload);
            _delegate = new TimerEventHandler(OnTimerEvent);
        }

        /// <summary>
        /// Called when an AppDomain is unloaded.  Our goal here is to ensure that
        /// all timers created by this class under the banner of this AppDomain
        /// are cleaned up prior to the AppDomain unloading.  Failure to do so will
        /// cause applications to crash due to exceptions outside of the AppDomain.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        static void OnAppDomainUnload(object sender, EventArgs e)
        {
            if (sender == _appDomain)
            {
                Log.Info(".OnAppDomainUnload - Called; unloading timers");

                // Current AppDomain is about to unload.  It is vital that any
                // multimedia timers that were tied to this domain be killed
                // immediately so that they do not attempt to make invocations
                // back into this domain.

                foreach (uint timerId in _timerTable.Keys)
                {
                    NativeMethods.timeKillEvent(timerId);
                    Log.Warn(".OnAppDomainUnload - KillEvent #{0}", timerId) ;
                }
                
                _timerTable.Clear() ;

                // Give the timers a brief amount of time to recover

                Thread.Sleep(100);
            }
        }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
#endif
}
