///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.view;

namespace com.espertech.esper.rowregex
{
    /// <summary>
    /// Helper for match recognize.
    /// </summary>
    public class EventRowRegexHelper
    {
        public static EventRowRegexNFAViewService RecursiveFindRegexService(Viewable top) {
            if (top is EventRowRegexNFAViewService) {
                return (EventRowRegexNFAViewService) top;
            }

            return top.Views
                .Select(RecursiveFindRegexService)
                .FirstOrDefault();
        }

        public static readonly IComparer<RegexNFAStateEntry> END_STATE_COMPARATOR = new ProxyComparer<RegexNFAStateEntry>
        {
            ProcCompare = (o1, o2) =>
            {
                if (o1.MatchEndEventSeqNo > o2.MatchEndEventSeqNo)
                    return -1;
                if (o1.MatchEndEventSeqNo < o2.MatchEndEventSeqNo)
                    return 1;
                return 0;
            }
        };

        /// <summary>
        /// Inspect variables recursively.
        /// </summary>
        /// <param name="parent">parent regex expression node</param>
        /// <param name="isMultiple">if the variable in the stack is multiple of single</param>
        /// <param name="variablesSingle">single variables list</param>
        /// <param name="variablesMultiple">group variables list</param>
        public static void RecursiveInspectVariables(RowRegexExprNode parent, bool isMultiple, ISet<String> variablesSingle, ISet<String> variablesMultiple)
        {
            if (parent is RowRegexExprNodeNested)
            {
                var nested = (RowRegexExprNodeNested) parent;
                foreach (var child in parent.ChildNodes)
                {
                    RecursiveInspectVariables(child, nested.NFAType.IsMultipleMatches() || isMultiple, variablesSingle, variablesMultiple);
                }
            }
            else if (parent is RowRegexExprNodeAlteration)
            {
                foreach (var childAlteration in parent.ChildNodes)
                {
                    var singles = new LinkedHashSet<String>();
                    var multiples = new LinkedHashSet<String>();
    
                    RecursiveInspectVariables(childAlteration, isMultiple, singles, multiples);
    
                    variablesMultiple.AddAll(multiples);
                    variablesSingle.AddAll(singles);
                }
                variablesSingle.RemoveAll(variablesMultiple);
            }
            else if (parent is RowRegexExprNodeAtom)
            {
                var atom = (RowRegexExprNodeAtom) parent;
                var name = atom.Tag;
                if (variablesMultiple.Contains(name))
                {
                    return;
                }
                if (variablesSingle.Contains(name))
                {
                    variablesSingle.Remove(name);
                    variablesMultiple.Add(name);
                    return;
                }
                if (atom.NFAType.IsMultipleMatches())
                {
                    variablesMultiple.Add(name);
                    return;
                }
                if (isMultiple)
                {
                    variablesMultiple.Add(name);
                }
                else
                {
                    variablesSingle.Add(name);
                }
            }
            else
            {
                foreach (var child in parent.ChildNodes)
                {
                    RecursiveInspectVariables(child, isMultiple, variablesSingle, variablesMultiple);
                }
            }
        }

        /// <summary>
        /// Build a list of start states from the parent node.
        /// </summary>
        /// <param name="parent">to build start state for</param>
        /// <param name="variableDefinitions">each variable and its expressions</param>
        /// <param name="variableStreams">variable name and its stream number</param>
        /// <param name="exprRequiresMultimatchState">State of the expr requires multimatch.</param>
        /// <returns>strand of regex state nodes</returns>
        public static RegexNFAStrandResult RecursiveBuildStartStates(
            RowRegexExprNode parent,
            IDictionary<String, ExprNode> variableDefinitions,
            IDictionary<String, Pair<int, bool>> variableStreams,
            bool[] exprRequiresMultimatchState)
        {
            var nodeNumStack = new Stack<int>();

            var strand = RecursiveBuildStatesInternal(
                parent,
                variableDefinitions,
                variableStreams,
                nodeNumStack,
                exprRequiresMultimatchState);
    
            // add end state
            var end = new RegexNFAStateEnd();
            foreach (var endStates in strand.EndStates)
            {
                endStates.AddState(end);
            }
    
            // assign node num as a counter
            var nodeNumberFlat = 0;
            foreach (var theBase in strand.AllStates)
            {
                theBase.NodeNumFlat = nodeNumberFlat++;
            }
    
            return new RegexNFAStrandResult(new List<RegexNFAState>(strand.StartStates), strand.AllStates);
        }

        private static RegexNFAStrand RecursiveBuildStatesInternal(
            RowRegexExprNode node,
            IDictionary<String, ExprNode> variableDefinitions,
            IDictionary<String, Pair<int, Boolean>> variableStreams,
            Stack<int> nodeNumStack,
            bool[] exprRequiresMultimatchState)
        {
            if (node is RowRegexExprNodeAlteration)
            {
                var nodeNum = 0;
    
                var cumulativeStartStates = new List<RegexNFAStateBase>();
                var cumulativeStates = new List<RegexNFAStateBase>();
                var cumulativeEndStates = new List<RegexNFAStateBase>();
    
                var isPassthrough = false;
                foreach (var child in node.ChildNodes)
                {
                    nodeNumStack.Push(nodeNum);
                    var strand = RecursiveBuildStatesInternal(child,
                                                   variableDefinitions,
                                                   variableStreams,
                                                   nodeNumStack,
                                                   exprRequiresMultimatchState);
                    nodeNumStack.Pop();
    
                    cumulativeStartStates.AddAll(strand.StartStates);
                    cumulativeStates.AddAll(strand.AllStates);
                    cumulativeEndStates.AddAll(strand.EndStates);
                    if (strand.IsPassthrough)
                    {
                        isPassthrough = true;
                    }
    
                    nodeNum++;
                }
    
                return new RegexNFAStrand(cumulativeStartStates, cumulativeEndStates, cumulativeStates, isPassthrough);
            }
            else if (node is RowRegexExprNodeConcatenation)
            {
                var nodeNum = 0;
    
                var isPassthrough = true;
                var cumulativeStates = new List<RegexNFAStateBase>();
                var strands = new RegexNFAStrand[node.ChildNodes.Count];
    
                foreach (var child in node.ChildNodes)
                {
                    nodeNumStack.Push(nodeNum);
                    strands[nodeNum] = RecursiveBuildStatesInternal(child,
                                                   variableDefinitions,
                                                   variableStreams,
                                                   nodeNumStack,
                                                   exprRequiresMultimatchState);
                    nodeNumStack.Pop();
    
                    cumulativeStates.AddAll(strands[nodeNum].AllStates);
                    if (!strands[nodeNum].IsPassthrough)
                    {
                        isPassthrough = false;
                    }
    
                    nodeNum++;
                }
    
                // determine start states: all states until the first non-passthrough start state
                var startStates = new List<RegexNFAStateBase>();
                for (var i = 0; i < strands.Length; i++)
                {
                    startStates.AddAll(strands[i].StartStates);
                    if (!strands[i].IsPassthrough)
                    {
                        break;
                    }
                }
    
                // determine end states: all states from the back until the last non-passthrough end state
                var endStates = new List<RegexNFAStateBase>();
                for (var i = strands.Length - 1; i >= 0; i--)
                {
                    endStates.AddAll(strands[i].EndStates);
                    if (!strands[i].IsPassthrough)
                    {
                        break;
                    }
                }
    
                // hook up the end state of each strand with the start states of each next strand
                for (var i = strands.Length - 1; i >= 1; i--)
                {
                    var current = strands[i];
                    for (var j = i - 1; j >= 0; j--)
                    {
                        var prior = strands[j];
    
                        foreach (var endState in prior.EndStates)
                        {
                            foreach (var startState in current.StartStates)
                            {
                                endState.AddState(startState);
                            }
                        }
    
                        if (!prior.IsPassthrough)
                        {
                            break;
                        }
                    }
                }
    
                return new RegexNFAStrand(startStates, endStates, cumulativeStates, isPassthrough);
            }
            else if (node is RowRegexExprNodeNested)
            {
                var nested = (RowRegexExprNodeNested) node;
                nodeNumStack.Push(0);
                var strand = RecursiveBuildStatesInternal(node.ChildNodes[0],
                                               variableDefinitions,
                                               variableStreams,
                                               nodeNumStack,
                                               exprRequiresMultimatchState);
                nodeNumStack.Pop();
    
                var isPassthrough = strand.IsPassthrough || nested.NFAType.IsOptional();
    
                // if this is a repeating node then pipe back each end state to each begin state
                if (nested.NFAType.IsMultipleMatches())
                {
                    foreach (var endstate in strand.EndStates)
                    {
                        foreach (var startstate in strand.StartStates)
                        {
                            if (!endstate.NextStates.Contains(startstate))
                            {
                                endstate.NextStates.Add(startstate);
                            }
                        }
                    }
                }
                return new RegexNFAStrand(strand.StartStates, strand.EndStates, strand.AllStates, isPassthrough);
            }
            else
            {
                var atom = (RowRegexExprNodeAtom) node;
    
                // assign stream number for single-variables for most direct expression eval; multiple-variable gets -1
                var streamNum = variableStreams.Get(atom.Tag).First;
                var multiple = variableStreams.Get(atom.Tag).Second;
                var expressionDef = variableDefinitions.Get(atom.Tag);
                var exprRequiresMultimatch = exprRequiresMultimatchState[streamNum];
    
                RegexNFAStateBase nextState;
                if ((atom.NFAType == RegexNFATypeEnum.ZERO_TO_MANY) || (atom.NFAType == RegexNFATypeEnum.ZERO_TO_MANY_RELUCTANT))
                {
                    nextState = new RegexNFAStateZeroToMany(ToString(nodeNumStack), atom.Tag, streamNum, multiple, atom.NFAType.IsGreedy(), expressionDef, exprRequiresMultimatch);
                }
                else if ((atom.NFAType == RegexNFATypeEnum.ONE_TO_MANY) || (atom.NFAType == RegexNFATypeEnum.ONE_TO_MANY_RELUCTANT))
                {
                    nextState = new RegexNFAStateOneToMany(ToString(nodeNumStack), atom.Tag, streamNum, multiple, atom.NFAType.IsGreedy(), expressionDef, exprRequiresMultimatch);
                }
                else if ((atom.NFAType == RegexNFATypeEnum.ONE_OPTIONAL) || (atom.NFAType == RegexNFATypeEnum.ONE_OPTIONAL_RELUCTANT))
                {
                    nextState = new RegexNFAStateOneOptional(ToString(nodeNumStack), atom.Tag, streamNum, multiple, atom.NFAType.IsGreedy(), expressionDef, exprRequiresMultimatch);
                }
                else if (expressionDef == null)
                {
                    nextState = new RegexNFAStateAnyOne(ToString(nodeNumStack), atom.Tag, streamNum, multiple);
                }
                else
                {
                    nextState = new RegexNFAStateFilter(ToString(nodeNumStack), atom.Tag, streamNum, multiple, expressionDef, exprRequiresMultimatch);
                }
    
                return new RegexNFAStrand(
                    Collections.SingletonList(nextState), 
                    Collections.SingletonList(nextState),
                    Collections.SingletonList(nextState),
                    atom.NFAType.IsOptional());
            }
        }
    
        private static String ToString(IEnumerable<int> nodeNumStack)
        {
            var builder = new StringBuilder();
            var delimiter = "";
            foreach (int? atom in nodeNumStack)
            {
                builder.Append(delimiter);
                builder.Append(Convert.ToString(atom));
                delimiter = ".";
            }
            return builder.ToString();
        }
    
        public static IDictionary<String, ISet<String>> DetermineVisibility(RowRegexExprNode pattern) 
        {
            IDictionary<String, ISet<String>> map = new Dictionary<String, ISet<String>>();
            var path = new ArrayDeque<RowRegexExprNode>();
            RecursiveFindPatternAtoms(pattern, path, map);
            return map;
        }

        private static void RecursiveFindPatternAtoms(RowRegexExprNode parent, ArrayDeque<RowRegexExprNode> path, IDictionary<String, ISet<String>> map)
        {
            path.Add(parent);
            foreach (var child in parent.ChildNodes) {
                if (child is RowRegexExprNodeAtom) {
                    HandleAtom((RowRegexExprNodeAtom) child, path, map);
                }
                else {
                    RecursiveFindPatternAtoms(child, path, map);
                }
            }
            path.RemoveLast();
        }

        private static void HandleAtom(RowRegexExprNodeAtom atom, IEnumerable<RowRegexExprNode> path, IDictionary<String, ISet<String>> map)
        {
    
            var patharr = path.ToArray();
            ISet<String> identifiers = null;
    
            for (var i = 0; i < patharr.Length; i++) {
                var parent = patharr[i];
                if (!(parent is RowRegexExprNodeConcatenation)) {
                    continue;
                }
    
                var concat = (RowRegexExprNodeConcatenation) parent;
                int indexWithinConcat;
                if (i == patharr.Length - 1) {
                    indexWithinConcat = parent.ChildNodes.IndexOf(atom);
                }
                else {
                    indexWithinConcat = parent.ChildNodes.IndexOf(patharr[i + 1]);
                }
    
                if (identifiers == null && indexWithinConcat > 0) {
                    identifiers = new HashSet<String>();
                }
    
                for (var j = 0; j < indexWithinConcat; j++)
                {
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
    
        private static void RecursiveCollectAtomsWExclude(RowRegexExprNode node, ISet<String> identifiers, String excludedTag)
        {
            if (node is RowRegexExprNodeAtom) {
                var atom = (RowRegexExprNodeAtom) node;
                if (!excludedTag.Equals(atom.Tag)) {
                    identifiers.Add(atom.Tag);
                }
            }
            foreach (var child in node.ChildNodes) {
                RecursiveCollectAtomsWExclude(child, identifiers, excludedTag);
            }
        }
    }
}
