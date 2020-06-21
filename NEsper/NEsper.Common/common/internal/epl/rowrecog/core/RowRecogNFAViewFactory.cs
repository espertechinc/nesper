///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.configuration.runtime;
using com.espertech.esper.common.client.serde;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.rowrecog.nfa;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.rowrecog.core
{
    /// <summary>
    /// View factory for match-recognize view.
    /// </summary>
    public class RowRecogNFAViewFactory : ViewFactory
    {
        private RowRecogDesc _desc;

        private bool _trackMaxStates;
        private RowRecogNFAState[] _startStates;
        private RowRecogNFAState[] _allStates;
        private DataInputOutputSerde _partitionKeySerde;
        private int _scheduleCallbackId;

        public RowRecogDesc Desc {
            get => _desc;
            set => _desc = value;
        }

        public EventType EventType {
            get => _desc.RowEventType;
            set {
                // ignored
            }
        }

        public void Init(
            ViewFactoryContext viewFactoryContext,
            EPStatementInitServices services)
        {
            ConfigurationRuntimeMatchRecognize matchRecognize =
                services.RuntimeSettingsService.ConfigurationRuntime.MatchRecognize;
            this._trackMaxStates = matchRecognize != null && matchRecognize.MaxStates != null;

            // build start states
            this._startStates = new RowRecogNFAState[_desc.StartStates.Length];
            for (int i = 0; i < _desc.StartStates.Length; i++) {
                this._startStates[i] = _desc.StatesOrdered[_desc.StartStates[i]];
            }

            // build all states and state links
            foreach (Pair<int, int[]> stateLink in _desc.NextStatesPerState) {
                RowRecogNFAStateBase state = _desc.StatesOrdered[stateLink.First];
                RowRecogNFAState[] nextStates = new RowRecogNFAState[stateLink.Second.Length];
                state.NextStates = nextStates;
                for (int i = 0; i < stateLink.Second.Length; i++) {
                    int nextNum = stateLink.Second[i];
                    RowRecogNFAState nextState;
                    if (nextNum == -1) {
                        nextState = new RowRecogNFAStateEndEval();
                    }
                    else {
                        nextState = _desc.StatesOrdered[nextNum];
                    }

                    nextStates[i] = nextState;
                }
            }

            this._allStates = _desc.StatesOrdered;
        }

        public View MakeView(AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext)
        {
            RowRecogNFAViewScheduler scheduler = null;
            if (_desc.HasInterval) {
                scheduler = new RowRecogNFAViewSchedulerImpl();
            }

            RowRecogNFAView view = new RowRecogNFAView(
                this,
                agentInstanceViewFactoryContext.AgentInstanceContext,
                scheduler);

            scheduler?.SetScheduleCallback(agentInstanceViewFactoryContext.AgentInstanceContext, view);

            return view;
        }

        public bool IsTrackMaxStates {
            get => _trackMaxStates;
        }

        public RowRecogNFAState[] StartStates {
            get => _startStates;
        }

        public RowRecogNFAState[] AllStates {
            get => _allStates;
        }

        public DataInputOutputSerde PartitionKeySerde {
            get => _partitionKeySerde;
        }

        public int ScheduleCallbackId {
            get => _scheduleCallbackId;
            set => _scheduleCallbackId = value;
        }

        public string ViewName {
            get => "rowrecog";
        }
    }
} // end of namespace