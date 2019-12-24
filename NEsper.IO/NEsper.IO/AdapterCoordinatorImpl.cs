// ---------------------------------------------------------------------------------- /
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
// ---------------------------------------------------------------------------------- /

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.runtime.client;
using com.espertech.esper.runtime.@internal.kernel.service;

namespace com.espertech.esperio
{
	/// <summary>
	/// An implementation of AdapterCoordinator.
	/// </summary>
	public class AdapterCoordinatorImpl
		: AbstractCoordinatedAdapter
		, AdapterCoordinator
	{
		private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

	    private readonly IDictionary<SendableEvent, CoordinatedAdapter> _eventsFromAdapters = new Dictionary<SendableEvent, CoordinatedAdapter>();
		private readonly ICollection<CoordinatedAdapter> _emptyAdapters = new HashSet<CoordinatedAdapter>();
	    private readonly bool _usingEngineThread;
        private readonly bool _usingExternalTimer;
		private readonly ScheduleBucket _scheduleBucket;
		private readonly EPRuntime _runtime;

		/// <summary>
		/// Ctor.
		/// </summary>
		/// <param name="runtime">the EPRuntime for services and runtime</param>
		/// <param name="usingEngineThread">true if the coordinator should set time by the scheduling service in the engine,
		///                           false if it should set time externally through the calling thread
		/// </param>
		public AdapterCoordinatorImpl(EPRuntime runtime, bool usingEngineThread)
            : this(runtime, usingEngineThread, false, false)
		{
		}

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="runtime">the EPRuntime for services and runtime</param>
        /// <param name="usingEngineThread">true if the coordinator should set time by the scheduling service in the engine, false if it should set time externally through the calling thread</param>
        /// <param name="usingExternalTimer">true to use esper's external timer mechanism instead of internal timing</param>
        /// <param name="usingTimeSpanEvents"></param>
        /// <exception cref="System.ArgumentNullException">epService;epService cannot be null</exception>
        /// <exception cref="System.ArgumentException">Illegal type of EPServiceProvider</exception>
	    public AdapterCoordinatorImpl(EPRuntime runtime, bool usingEngineThread, bool usingExternalTimer, bool usingTimeSpanEvents)
            : base(runtime, usingEngineThread, usingExternalTimer, usingTimeSpanEvents)
    	{
            if (runtime == null)
			{
				throw new ArgumentNullException("runtime", "runtime cannot be null");
			}

			if(!(runtime is EPRuntimeSPI))
			{
				throw new ArgumentException("Illegal type of EPServiceProvider");
			}
			this._runtime = runtime;
			this._scheduleBucket = new ScheduleBucket(-1);
			this._usingEngineThread = usingEngineThread;
            this._usingExternalTimer = usingExternalTimer;
        }

		/// <summary>
		///@see com.espertech.esper.adapter.ReadableAdapter#read()
		/// </summary>
		public override SendableEvent Read()
		{
            if ((ExecutionPathDebugLog.IsDebugEnabled) && (Log.IsDebugEnabled))
            {
                Log.Debug(".read");
            }

            PollEmptyAdapters();

		    var isEventsToSendEmpty = EventsToSend.IsEmpty();
		    var isEventsFromAdaptersEmpty = _eventsFromAdapters.IsEmpty();
		    var isEmptyAdaptersEmpty = _emptyAdapters.IsEmpty();

            if ((ExecutionPathDebugLog.IsDebugEnabled) && (Log.IsDebugEnabled))
            {
                Log.Debug(".read eventsToSend.isEmpty==" + isEventsToSendEmpty);
                Log.Debug(".read eventsFromAdapters.isEmpty==" + isEventsFromAdaptersEmpty);
                Log.Debug(".read emptyAdapters.isEmpty==" + isEmptyAdaptersEmpty);
            }

            if (isEventsToSendEmpty && isEventsFromAdaptersEmpty && isEmptyAdaptersEmpty)
			{
				Stop();
			}

			if(StateManager.State == AdapterState.DESTROYED || isEventsToSendEmpty)
			{
				return null;
			}

			var result = EventsToSend.First();
			ReplaceFirstEventToSend();

			return result;
		}

		/// <summary>
		///@see com.espertech.esper.adapter.AdapterCoordinator#add(com.espertech.esper.adapter.Adapter)
		/// </summary>
		public virtual void Coordinate(InputAdapter inputAdapter)
		{
			if(inputAdapter == null)
			{
				throw new ArgumentException("AdapterSpec cannot be null");
			}

			if(!(inputAdapter is CoordinatedAdapter coordinatedAdapter))
			{
				throw new ArgumentException("Cannot coordinate a Adapter of type " + inputAdapter.GetType());
			}
			
			var adapter = coordinatedAdapter;
			if(_eventsFromAdapters.Values.Contains(adapter) || _emptyAdapters.Contains(adapter))
			{
				return;
			}
			adapter.DisallowStateTransitions();
			adapter.Runtime = _runtime;
			adapter.UsingEngineThread = _usingEngineThread;
            adapter.UsingExternalTimer = _usingExternalTimer;
			adapter.ScheduleSlot = _scheduleBucket.AllocateSlot();
			AddNewEvent(adapter);
		}

		/// <summary>
		/// Does nothing.
		/// </summary>
		protected override void Close()
		{
			// Do nothing
		}

		/// <summary>
		/// Replace the first member of eventsToSend with the next
		/// event returned by the read() method of the same Adapter that
		/// provided the first event.
		/// </summary>
		protected override void ReplaceFirstEventToSend()
		{
            if ((ExecutionPathDebugLog.IsDebugEnabled) && (Log.IsDebugEnabled))
            {
                Log.Debug(".ReplaceFirstEventToSend Replacing event");
            }
		    var _event = EventsToSend.First();
			EventsToSend.Remove(_event);
			AddNewEvent(_eventsFromAdapters.Get(_event));
            _eventsFromAdapters.Remove(_event);
            PollEmptyAdapters();
		}

		/// <summary>
		/// Reset all the changeable state of this ReadableAdapter, as if it were just created.
		/// </summary>
		protected override void Reset()
		{
			_eventsFromAdapters.Clear();
			_emptyAdapters.Clear();
		}

		private void AddNewEvent(CoordinatedAdapter adapter)
		{
            if ((ExecutionPathDebugLog.IsDebugEnabled) && (Log.IsDebugEnabled))
            {
                Log.Debug(".addNewEvent eventsFromAdapters==" + _eventsFromAdapters);
            }
            var _event = adapter.Read();
			if(_event != null)
			{
                if ((ExecutionPathDebugLog.IsDebugEnabled) && (Log.IsDebugEnabled))
                {
                    Log.Debug(".addNewEvent event==" + _event);
                }
			    EventsToSend.Add(_event);
				_eventsFromAdapters[_event] = adapter;
			}
			else
			{
				if(adapter.State == AdapterState.DESTROYED)
				{
					var keyList = new LinkedList<SendableEvent>() ;

					foreach( var entry in _eventsFromAdapters )
					{
						if ( entry.Value == adapter )
						{
							keyList.AddFirst( entry.Key ) ;
						}
					}

					foreach( var keyEvent in keyList )
					{
						_eventsFromAdapters.Remove( keyEvent ) ;
					}
				}
				else
				{
					_emptyAdapters.Add(adapter);
				}
			}
		}

		private void PollEmptyAdapters()
		{
            if ((ExecutionPathDebugLog.IsDebugEnabled) && (Log.IsDebugEnabled))
            {
                Log.Debug(".pollEmptyAdapters emptyAdapters.size==" + _emptyAdapters.Count);
            }

			var tempList = new List<CoordinatedAdapter>() ;

			foreach( var adapter in _emptyAdapters )
			{
				if(adapter.State == AdapterState.DESTROYED)
				{
					tempList.Add( adapter ) ;
					continue;
				}

				var _event = adapter.Read();
				if(_event != null)
				{
					EventsToSend.Add(_event);
					_eventsFromAdapters[_event] = adapter;
				}
			}

			foreach( var adapter in tempList )
			{
				_emptyAdapters.Remove( adapter ) ;
			}
		}
	}
}
