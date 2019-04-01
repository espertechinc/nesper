///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.espertech.esper.collection;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.rowrecog.expr;
using com.espertech.esper.common.@internal.epl.rowrecog.nfa;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.rowrecog.core
{
    /// <summary>
    ///     Helper for match recognize.
    /// </summary>
    public class RowRecogHelper
    {
        protected internal static readonly IComparer<RowRecogNFAStateEntry> END_STATE_COMPARATOR = new ProxyComparer<RowRecogNFAStateEntry> {
            ProcCompare = (o1, o2) => {
                if (o1.MatchEndEventSeqNo > o2.MatchEndEventSeqNo) {
                    return -1;
                }

                if (o1.MatchEndEventSeqNo < o2.MatchEndEventSeqNo) {
                    return 1;
                }

                return 0;
            }
        };

        public static RowRecogNFAViewService RecursiveFindRegexService(Viewable top)
        {
            if (top == null) {
                return null;
            }

            if (top is RowRecogNFAViewService) {
                return (RowRecogNFAViewService) top;
            }

            return RecursiveFindRegexService(top.Child);
        }

        /// <summary>
        ///     Inspect variables recursively.
        /// </summary>
        /// <param name="parent">parent regex expression node</param>
        /// <param name="isMultiple">if the variable in the stack is multiple of single</param>
        /// <param name="variablesSingle">single variables list</param>
        /// <param name="variablesMultiple">group variables list</param>
        public static void RecursiveInspectVariables(
            RowRecogExprNode parent,
            bool isMultiple,
            ISet<string> variablesSingle,
            ISet<string> variablesMultiple)
        {
            if (parent is RowRecogExprNodeNested) {
                var nested = (RowRecogExprNodeNested) parent;
                foreach (var child in parent.ChildNodes) {
                    RecursiveInspectVariables(child, nested.Type.IsMultipleMatches() || isMultiple, variablesSingle, variablesMultiple);
                }
            }
            else if (parent is RowRecogExprNodeAlteration) {
                foreach (var childAlteration in parent.ChildNodes) {
                    var singles = new LinkedHashSet<string>();
                    var multiples = new LinkedHashSet<string>();

                    RecursiveInspectVariables(childAlteration, isMultiple, singles, multiples);

                    variablesMultiple.AddAll(multiples);
                    variablesSingle.AddAll(singles);
                }

                variablesSingle.RemoveAll(variablesMultiple);
            }
            else if (parent is RowRecogExprNodeAtom) {
                var atom = (RowRecogExprNodeAtom) parent;
                var name = atom.Tag;
                if (variablesMultiple.Contains(name)) {
                    return;
                }

                if (variablesSingle.Contains(name)) {
                    variablesSingle.Remove(name);
                    variablesMultiple.Add(name);
                    return;
                }

                if (atom.Type.IsMultipleMatches()) {
                    variablesMultiple.Add(name);
                    return;
                }

                if (isMultiple) {
                    variablesMultiple.Add(name);
                }
                else {
                    variablesSingle.Add(name);
                }
            }
            else {
                foreach (var child in parent.ChildNodes) {
                    RecursiveInspectVariables(child, isMultiple, variablesSingle, variablesMultiple);
                }
            }
        }

        /// <summary>
        ///     Build a list of start states from the parent node.
        /// </summary>
        /// <param name="parent">to build start state for</param>
        /// <param name="variableDefinitions">each variable and its expressions</param>
        /// <param name="variableStreams">variable name and its stream number</param>
        /// <param name="exprRequiresMultimatchState">indicator whether multi-match state required</param>
        /// <returns>strand of regex state nodes</returns>
        protected internal static RowRecogNFAStrandResult BuildStartStates(
            RowRecogExprNode parent,
            IDictionary<string, ExprNode> variableDefinitions,
            IDictionary<string, Pair<int, bool>> variableStreams,
            bool[] exprRequiresMultimatchState
        )
        {
            var nodeNumStack = new Stack<int>();

            RowRecogNFAStrand strand = RecursiveBuildStatesInternal(
                parent,
                variableDefinitions,
                variableStreams,
                nodeNumStack,
                exprRequiresMultimatchState);

            // add end state
            var end = new RowRecogNFAStateEndForge();
            end.NodeNumFlat = -1;
            foreach (RowRecogNFAStateForgeBase endStates in strand.EndStates) {
                endStates.AddState(end);
            }

            // assign node num as a counter
            var nodeNumberFlat = 0;
            foreach (RowRecogNFAStateForgeBase theBase in strand.AllStates) {
                theBase.NodeNumFlat = nodeNumberFlat++;
            }

            return new RowRecogNFAStrandResult(new List<RowRecogNFAStateForge>(strand.StartStates), strand.AllStates);
        }

        private static RowRecogNFAStrand RecursiveBuildStatesInternal(
            RowRecogExprNode node,
            IDictionary<string, ExprNode> variableDefinitions,
            IDictionary<string, Pair<int, bool>> variableStreams,
            Stack<int> nodeNumStack,
            bool[] exprRequiresMultimatchState
        )
        {
            if (node is RowRecogExprNodeAlteration) {
                var nodeNum = 0;

                IList<RowRecogNFAStateForgeBase> cumulativeStartStates = new List<RowRecogNFAStateForgeBase>();
                IList<RowRecogNFAStateForgeBase> cumulativeStates = new List<RowRecogNFAStateForgeBase>();
                IList<RowRecogNFAStateForgeBase> cumulativeEndStates = new List<RowRecogNFAStateForgeBase>();

                var isPassthrough = false;
                foreach (var child in node.ChildNodes) {
                    nodeNumStack.Push(nodeNum);
                    var strand = RecursiveBuildStatesInternal(
                        child,
                        variableDefinitions,
                        variableStreams,
                        nodeNumStack,
                        exprRequiresMultimatchState);
                    nodeNumStack.Pop();

                    cumulativeStartStates.AddAll(strand.StartStates);
                    cumulativeStates.AddAll(strand.AllStates);
                    cumulativeEndStates.AddAll(strand.EndStates);
                    if (strand.IsPassthrough) {
                        isPassthrough = true;
                    }

                    nodeNum++;
                }

                return new RowRecogNFAStrand(cumulativeStartStates, cumulativeEndStates, cumulativeStates, isPassthrough);
            }

            if (node is RowRecogExprNodeConcatenation) {
                var nodeNum = 0;

                var isPassthrough = true;
                IList<RowRecogNFAStateForgeBase> cumulativeStates = new List<RowRecogNFAStateForgeBase>();
                var strands = new RowRecogNFAStrand[node.ChildNodes.Count];

                foreach (var child in node.ChildNodes) {
                    nodeNumStack.Push(nodeNum);
                    strands[nodeNum] = RecursiveBuildStatesInternal(
                        child,
                        variableDefinitions,
                        variableStreams,
                        nodeNumStack,
                        exprRequiresMultimatchState);
                    nodeNumStack.Pop();

                    cumulativeStates.AddAll(strands[nodeNum].AllStates);
                    if (!strands[nodeNum].IsPassthrough) {
                        isPassthrough = false;
                    }

                    nodeNum++;
                }

                // determine start states: all states until the first non-passthrough start state
                IList<RowRecogNFAStateForgeBase> startStates = new List<RowRecogNFAStateForgeBase>();
                for (var i = 0; i < strands.Length; i++) {
                    startStates.AddAll(strands[i].StartStates);
                    if (!strands[i].IsPassthrough) {
                        break;
                    }
                }

                // determine end states: all states from the back until the last non-passthrough end state
                IList<RowRecogNFAStateForgeBase> endStates = new List<RowRecogNFAStateForgeBase>();
                for (var i = strands.Length - 1; i >= 0; i--) {
                    endStates.AddAll(strands[i].EndStates);
                    if (!strands[i].IsPassthrough) {
                        break;
                    }
                }

                // hook up the end state of each strand with the start states of each next strand
                for (var i = strands.Length - 1; i >= 1; i--) {
                    var current = strands[i];
                    for (var j = i - 1; j >= 0; j--) {
                        var prior = strands[j];

                        foreach (RowRecogNFAStateForgeBase endState in prior.EndStates) {
                            foreach (RowRecogNFAStateForgeBase startState in current.StartStates) {
                                endState.AddState(startState);
                            }
                        }

                        if (!prior.IsPassthrough) {
                            break;
                        }
                    }
                }

                return new RowRecogNFAStrand(startStates, endStates, cumulativeStates, isPassthrough);
            }

            if (node is RowRecogExprNodeNested) {
                var nested = (RowRecogExprNodeNested) node;
                nodeNumStack.Push(0);
                var strand = RecursiveBuildStatesInternal(
                    node.ChildNodes[0],
                    variableDefinitions,
                    variableStreams,
                    nodeNumStack,
                    exprRequiresMultimatchState);
                nodeNumStack.Pop();

                var isPassthrough = strand.IsPassthrough || nested.Type.IsOptional();

                // if this is a repeating node then pipe back each end state to each begin state
                if (nested.Type.IsMultipleMatches()) {
                    foreach (RowRecogNFAStateForgeBase endstate in strand.EndStates) {
                        foreach (RowRecogNFAStateForgeBase startstate in strand.StartStates) {
                            if (!endstate.NextStates.Contains(startstate)) {
                                endstate.NextStates.Add(startstate);
                            }
                        }
                    }
                }

                return new RowRecogNFAStrand(strand.StartStates, strand.EndStates, strand.AllStates, isPassthrough);
            }

            var atom = (RowRecogExprNodeAtom) node;

            // assign stream number for single-variables for most direct expression eval; multiple-variable gets -1
            var streamNum = variableStreams.Get(atom.Tag).First;
            var multiple = variableStreams.Get(atom.Tag).Second;
            var expression = variableDefinitions.Get(atom.Tag);
            var exprRequiresMultimatch = exprRequiresMultimatchState[streamNum];

            RowRecogNFAStateForgeBase nextState;
            if (atom.Type == RowRecogNFATypeEnum.ZERO_TO_MANY || atom.Type == RowRecogNFATypeEnum.ZERO_TO_MANY_RELUCTANT) {
                nextState = new RowRecogNFAStateZeroToManyForge(
                    ToString(nodeNumStack), atom.Tag, streamNum, multiple, atom.Type.IsGreedy(), exprRequiresMultimatch, expression);
            }
            else if (atom.Type == RowRecogNFATypeEnum.ONE_TO_MANY || atom.Type == RowRecogNFATypeEnum.ONE_TO_MANY_RELUCTANT) {
                nextState = new RowRecogNFAStateOneToManyForge(
                    ToString(nodeNumStack), atom.Tag, streamNum, multiple, atom.Type.IsGreedy(), exprRequiresMultimatch, expression);
            }
            else if (atom.Type == RowRecogNFATypeEnum.ONE_OPTIONAL || atom.Type == RowRecogNFATypeEnum.ONE_OPTIONAL_RELUCTANT) {
                nextState = new RowRecogNFAStateOneOptionalForge(
                    ToString(nodeNumStack), atom.Tag, streamNum, multiple, atom.Type.IsGreedy(), exprRequiresMultimatch, expression);
            }
            else if (expression == null) {
                nextState = new RowRecogNFAStateAnyOneForge(ToString(nodeNumStack), atom.Tag, streamNum, multiple);
            }
            else {
                nextState = new RowRecogNFAStateFilterForge(
                    ToString(nodeNumStack), atom.Tag, streamNum, multiple, exprRequiresMultimatch, expression);
            }

            return new RowRecogNFAStrand(
                Collections.SingletonList(nextState), Collections.SingletonList(nextState),
                Collections.SingletonList(nextState), atom.Type.IsOptional());
        }

        private static string ToString(Stack<int> nodeNumStack)
        {
            var builder = new StringBuilder();
            var delimiter = "";
            foreach (var atom in nodeNumStack) {
                builder.Append(delimiter);
                builder.Append(Convert.ToString(atom));
                delimiter = ".";
            }

            return builder.ToString();
        }

        public static IDictionary<string, ISet<string>> DetermineVisibility(RowRecogExprNode pattern)
        {
            IDictionary<string, ISet<string>> map = new Dictionary<string, ISet<string>>();
            var path = new ArrayDeque<RowRecogExprNode>();
            RecursiveFindPatternAtoms(pattern, path, map);
            return map;
        }

        private static void RecursiveFindPatternAtoms(
            RowRecogExprNode parent,
            ArrayDeque<RowRecogExprNode> path,
            IDictionary<string, ISet<string>> map)
        {
            path.Add(parent);
            foreach (var child in parent.ChildNodes) {
                if (child is RowRecogExprNodeAtom) {
                    HandleAtom((RowRecogExprNodeAtom) child, path, map);
                }
                else {
                    RecursiveFindPatternAtoms(child, path, map);
                }
            }

            path.RemoveLast();
        }

        private static void HandleAtom(
            RowRecogExprNodeAtom atom,
            ArrayDeque<RowRecogExprNode> path,
            IDictionary<string, ISet<string>> map)
        {
            var patharr = path.ToArray();
            ISet<string> identifiers = null;

            for (var i = 0; i < patharr.Length; i++) {
                var parent = patharr[i];
                if (!(parent is RowRecogExprNodeConcatenation)) {
                    continue;
                }

                var concat = (RowRecogExprNodeConcatenation) parent;
                int indexWithinConcat;
                if (i == patharr.Length - 1) {
                    indexWithinConcat = parent.ChildNodes.IndexOf(atom);
                }
                else {
                    indexWithinConcat = parent.ChildNodes.IndexOf(patharr[i + 1]);
                }

                if (identifiers == null && indexWithinConcat > 0) {
                    identifiers = new HashSet<string>();
                }

                for (var j = 0; j < indexWithinConcat; j++) {
                    var concatChildNode = concat.ChildNodes[j];
                    RecursiveCollectAtomsWExclude(concatChildNode, identifiers, atom.Tag);
                }
            }

            if (identifiers == null) {
                return;
            }

            var existingVisibility = map.Get(atom.Tag);
            if (existingVisibility == null) {
                map.Put(atom.Tag, identifiers);
            }
            else {
                existingVisibility.AddAll(identifiers);
            }
        }

        private static void RecursiveCollectAtomsWExclude(
            RowRecogExprNode node,
            ISet<string> identifiers,
            string excludedTag)
        {
            if (node is RowRecogExprNodeAtom) {
                var atom = (RowRecogExprNodeAtom) node;
                if (!excludedTag.Equals(atom.Tag)) {
                    identifiers.Add(atom.Tag);
                }
            }

            foreach (var child in node.ChildNodes) {
                RecursiveCollectAtomsWExclude(child, identifiers, excludedTag);
            }
        }
    }
} // end of namespace