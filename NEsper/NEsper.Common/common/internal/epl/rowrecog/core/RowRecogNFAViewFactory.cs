///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.collection;
using com.espertech.esper.common.client;
using com.espertech.esper.common.client.configuration.runtime;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.rowrecog.nfa;
using com.espertech.esper.common.@internal.serde;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.rowrecog.core
{
    /// <summary>
    /// View factory for match-recognize view.
    /// </summary>
    public class RowRecogNFAViewFactory : ViewFactory
    {
        private RowRecogDesc desc;

        private bool trackMaxStates;
        private RowRecogNFAState[] startStates;
        private RowRecogNFAState[] allStates;
        protected DataInputOutputSerdeWCollation<object> partitionKeySerde;
        protected int scheduleCallbackId;

        public void SetDesc(RowRecogDesc desc)
        {
            this.desc = desc;
        }

        public void SetScheduleCallbackId(int scheduleCallbackId)
        {
            this.scheduleCallbackId = scheduleCallbackId;
        }

        public RowRecogDesc Desc {
            get => desc;
        }

        public EventType EventType {
            get => desc.RowEventType;
            set {
                // ignored
            }
        }

        public void Init(
            ViewFactoryContext viewFactoryContext,
            EPStatementInitServices services)
        {
            ConfigurationRuntimeMatchRecognize matchRecognize = services.RuntimeSettingsService.ConfigurationRuntime.MatchRecognize;
            this.trackMaxStates = matchRecognize != null && matchRecognize.MaxStates != null;

            // build start states
            this.startStates = new RowRecogNFAState[desc.StartStates.Length];
            for (int i = 0; i < desc.StartStates.Length; i++) {
                this.startStates[i] = desc.StatesOrdered[desc.StartStates[i]];
            }

            // build all states and state links
            foreach (Pair<int, int[]> stateLink in desc.NextStatesPerState) {
                RowRecogNFAStateBase state = desc.StatesOrdered[stateLink.First];
                RowRecogNFAState[] nextStates = new RowRecogNFAState[stateLink.Second.Length];
                state.NextStates = nextStates;
                for (int i = 0; i < stateLink.Second.Length; i++) {
                    int nextNum = stateLink.Second[i];
                    RowRecogNFAState nextState;
                    if (nextNum == -1) {
                        nextState = new RowRecogNFAStateEndEval();
                    }
                    else {
                        nextState = desc.StatesOrdered[nextNum];
                    }

                    nextStates[i] = nextState;
                }
            }

            this.allStates = desc.StatesOrdered;
        }

        public View MakeView(AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext)
        {
            RowRecogNFAViewScheduler scheduler = null;
            if (desc.HasInterval) {
                scheduler = new RowRecogNFAViewSchedulerImpl();
            }

            RowRecogNFAView view = new RowRecogNFAView(
                this,
                agentInstanceViewFactoryContext.AgentInstanceContext,
                scheduler);

            if (scheduler != null) {
                scheduler.SetScheduleCallback(agentInstanceViewFactoryContext.AgentInstanceContext, view);
            }

            return view;
        }

        public bool IsTrackMaxStates {
            get => trackMaxStates;
        }

        public RowRecogNFAState[] StartStates {
            get => startStates;
        }

        public RowRecogNFAState[] AllStates {
            get => allStates;
        }

        public DataInputOutputSerdeWCollation<object> PartitionKeySerde {
            get => partitionKeySerde;
        }

        public int ScheduleCallbackId {
            get => scheduleCallbackId;
        }

        public string ViewName {
            get => "rowrecog";
        }
    }
} // end of namespace