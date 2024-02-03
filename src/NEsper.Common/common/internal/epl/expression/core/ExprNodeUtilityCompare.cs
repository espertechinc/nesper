///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    public class ExprNodeUtilityCompare
    {
        public static bool DeepEqualsIsSubset(
            ExprNode[] subset,
            ExprNode[] superset)
        {
            foreach (var subsetNode in subset) {
                var found = false;
                foreach (var supersetNode in superset) {
                    if (DeepEquals(subsetNode, supersetNode, false)) {
                        found = true;
                        break;
                    }
                }

                if (!found) {
                    return false;
                }
            }

            return true;
        }

        public static bool DeepEqualsIgnoreDupAndOrder(
            ExprNode[] setOne,
            ExprNode[] setTwo)
        {
            if ((setOne.Length == 0 && setTwo.Length != 0) || (setOne.Length != 0 && setTwo.Length == 0)) {
                return false;
            }

            // find set-one expressions in set two
            var foundTwo = new bool[setTwo.Length];
            foreach (var one in setOne) {
                var found = false;
                for (var i = 0; i < setTwo.Length; i++) {
                    if (DeepEquals(one, setTwo[i], false)) {
                        found = true;
                        foundTwo[i] = true;
                    }
                }

                if (!found) {
                    return false;
                }
            }

            // find any remaining set-two expressions in set one
            for (var i = 0; i < foundTwo.Length; i++) {
                if (foundTwo[i]) {
                    continue;
                }

                foreach (var one in setOne) {
                    if (DeepEquals(one, setTwo[i], false)) {
                        break;
                    }
                }

                return false;
            }

            return true;
        }

        /// <summary>
        ///     Compare two expression nodes and their children in exact child-node sequence,
        ///     returning true if the 2 expression nodes trees are equals, or false if they are not equals.
        ///     <para />
        ///     Recursive call since it uses this method to compare child nodes in the same exact sequence.
        ///     Nodes are compared using the equalsNode method.
        /// </summary>
        /// <param name="nodeOne">first expression top node of the tree to compare</param>
        /// <param name="nodeTwo">second expression top node of the tree to compare</param>
        /// <param name="ignoreStreamPrefix">when the equals-comparison can ignore prefix of event properties</param>
        /// <returns>false if this or all child nodes are not equal, true if equal</returns>
        public static bool DeepEquals(
            ExprNode nodeOne,
            ExprNode nodeTwo,
            bool ignoreStreamPrefix)
        {
            if (nodeOne.ChildNodes.Length != nodeTwo.ChildNodes.Length) {
                return false;
            }

            if (!nodeOne.EqualsNode(nodeTwo, ignoreStreamPrefix)) {
                return false;
            }

            for (var i = 0; i < nodeOne.ChildNodes.Length; i++) {
                var childNodeOne = nodeOne.ChildNodes[i];
                var childNodeTwo = nodeTwo.ChildNodes[i];

                if (!DeepEquals(childNodeOne, childNodeTwo, ignoreStreamPrefix)) {
                    return false;
                }
            }

            return true;
        }

        public static bool DeepEqualsNullChecked(
            ExprNode nodeOne,
            ExprNode nodeTwo,
            bool ignoreStreamPrefix)
        {
            if (nodeOne == null) {
                return nodeTwo == null;
            }

            return nodeTwo != null && DeepEquals(nodeOne, nodeTwo, ignoreStreamPrefix);
        }

        /// <summary>
        ///     Compares two expression nodes via deep comparison, considering all
        ///     child nodes of either side.
        /// </summary>
        /// <param name="one">array of expressions</param>
        /// <param name="two">array of expressions</param>
        /// <param name="ignoreStreamPrefix">indicator whether we ignore stream prefixes and instead use resolved property name</param>
        /// <returns>true if the expressions are equal, false if not</returns>
        public static bool DeepEquals(
            ExprNode[] one,
            ExprNode[] two,
            bool ignoreStreamPrefix)
        {
            if (one.Length != two.Length) {
                return false;
            }

            for (var i = 0; i < one.Length; i++) {
                if (!DeepEquals(one[i], two[i], ignoreStreamPrefix)) {
                    return false;
                }
            }

            return true;
        }

        public static bool DeepEquals(
            IList<ExprNode> one,
            IList<ExprNode> two)
        {
            if (one.Count != two.Count) {
                return false;
            }

            for (var i = 0; i < one.Count; i++) {
                if (!DeepEquals(one[i], two[i], false)) {
                    return false;
                }
            }

            return true;
        }
    }
} // end of namespace