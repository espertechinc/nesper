///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.IO;
using System.Text;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.rowrecog.nfa;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.rowrecog.core
{
    public class RowRecogNFAViewUtil
    {
        protected internal static EventBean[] GetMultimatchArray(
            int[] multimatchStreamNumToVariable,
            RowRecogNFAStateEntry state,
            int stream)
        {
            if (state.OptionalMultiMatches == null) {
                return null;
            }

            var index = multimatchStreamNumToVariable[stream];
            var multiMatches = state.OptionalMultiMatches[index];

            return multiMatches?.ShrinkEventArray;
        }

        protected internal static string PrintStates(
            IList<RowRecogNFAStateEntry> states,
            IDictionary<int, string> streamsVariables,
            IDictionary<string, Pair<int, bool>> variableStreams,
            int[] multimatchStreamNumToVariable)
        {
            var buf = new StringBuilder();
            var delimiter = "";
            foreach (var state in states) {
                buf.Append(delimiter);
                buf.Append(state.State.NodeNumNested);

                buf.Append("{");
                var eventsPerStream = state.EventsPerStream;
                if (eventsPerStream == null) {
                    buf.Append("null");
                }
                else {
                    var eventDelimiter = "";
                    foreach (var streamVariable in streamsVariables) {
                        buf.Append(eventDelimiter);
                        buf.Append(streamVariable.Value);
                        buf.Append('=');
                        var single = !variableStreams.Get(streamVariable.Value).Second;
                        if (single) {
                            if (eventsPerStream[streamVariable.Key] == null) {
                                buf.Append("null");
                            }
                            else {
                                buf.Append(eventsPerStream[streamVariable.Key].Underlying);
                            }
                        }
                        else {
                            var streamNum = state.State.StreamNum;
                            var index = multimatchStreamNumToVariable[streamNum];
                            if (state.OptionalMultiMatches == null) {
                                buf.Append("null-mm");
                            }
                            else if (state.OptionalMultiMatches[index] == null) {
                                buf.Append("no-entry");
                            }
                            else {
                                buf.Append("{");
                                var arrayEventDelimiter = "";
                                var multiMatch = state.OptionalMultiMatches[index].Buffer;
                                var count = state.OptionalMultiMatches[index].Count;
                                for (var i = 0; i < count; i++) {
                                    buf.Append(arrayEventDelimiter);
                                    buf.Append(multiMatch[i].Underlying);
                                    arrayEventDelimiter = ", ";
                                }

                                buf.Append("}");
                            }
                        }

                        eventDelimiter = ", ";
                    }
                }

                buf.Append("}");

                delimiter = ", ";
            }

            return buf.ToString();
        }

        protected internal static string Print(RowRecogNFAStateForge[] states)
        {
            var writer = new StringWriter();
            var currentStack = new Stack<RowRecogNFAStateForge>();
            Print(states, writer, 0, currentStack);
            return writer.ToString();
        }

        protected internal static void Print(
            IList<RowRecogNFAStateForge> states,
            TextWriter writer,
            int indent,
            Stack<RowRecogNFAStateForge> currentStack)
        {
            foreach (var state in states) {
                Indent(writer, indent);
                if (currentStack.Contains(state)) {
                    writer.WriteLine("(self)");
                }
                else {
                    writer.WriteLine(PrintState(state));

                    currentStack.Push(state);
                    Print(state.NextStates, writer, indent + 4, currentStack);
                    currentStack.Pop();
                }
            }
        }

        private static string PrintState(RowRecogNFAStateForge state)
        {
            if (state is RowRecogNFAStateEndEval) {
                return "#" + state.NodeNumNested;
            }

            return "#" +
                   state.NodeNumNested +
                   " " +
                   state.VariableName +
                   " s" +
                   state.StreamNum +
                   " defined as " +
                   state;
        }

        private static void Indent(
            TextWriter writer,
            int indent)
        {
            for (var i = 0; i < indent; i++) {
                writer.Write(' ');
            }
        }
    }
} // end of namespace