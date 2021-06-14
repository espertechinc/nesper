///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.support.filter
{
    public class FilterTestMultiStmtRunner
    {
        public static IList<FilterTestMultiStmtExecution> ComputePermutations(
            Type originator,
            PermutationSpec permutationSpec,
            IList<FilterTestMultiStmtPermutable> cases,
            bool withStats)
        {
            // For each permutable test
            IList<FilterTestMultiStmtExecution> executions = new List<FilterTestMultiStmtExecution>();
            foreach (var permutableCase in cases) {
                executions.AddAll(
                    ComputePermutationsCase(
                        originator,
                        permutationSpec,
                        permutableCase,
                        withStats));
            }

            return executions;
        }

        private static IList<FilterTestMultiStmtExecution> ComputePermutationsCase(
            Type originator,
            PermutationSpec permutationSpec,
            FilterTestMultiStmtPermutable permutableCase,
            bool withStats)
        {
            if (!permutationSpec.IsAll) {
                return Collections.SingletonList(
                    CaseOf(originator, permutationSpec.Specific, permutableCase, withStats));
            }

            // determine that filters is different
            ISet<string> filtersUnique = new HashSet<string>(Arrays.AsList(permutableCase.Filters));
            if (filtersUnique.Count == 1 && permutableCase.Filters.Length > 1) {
                Assert.Fail("Filters are all the same, specify a single permutation instead");
            }

            IList<FilterTestMultiStmtExecution> executions = new List<FilterTestMultiStmtExecution>();
            var permutationEnumerator = PermutationEnumerator.Create(permutableCase.Filters.Length);
            foreach (var permutation in permutationEnumerator) {
                executions.Add(CaseOf(originator, permutation, permutableCase, withStats));
            }

            return executions;
        }

        private static FilterTestMultiStmtExecution CaseOf(
            Type originator,
            int[] permutation,
            FilterTestMultiStmtPermutable permutableCase,
            bool withStats)
        {
            var theCase = ComputePermutation(permutableCase, permutation);
            return new FilterTestMultiStmtExecution(originator, theCase, withStats);
        }

        private static FilterTestMultiStmtCase ComputePermutation(
            FilterTestMultiStmtPermutable permutableCase,
            int[] permutation)
        {
            // permute filters
            var filtersPermuted = new string[permutableCase.Filters.Length];
            for (var i = 0; i < permutation.Length; i++) {
                filtersPermuted[i] = permutableCase.Filters[permutation[i]];
            }

            // permute expected values
            IList<FilterTestMultiStmtAssertItem> itemsPermuted = new List<FilterTestMultiStmtAssertItem>();
            foreach (var items in permutableCase.Items) {
                var expectedPermuted = new bool[items.ExpectedPerStmt.Length];
                for (var i = 0; i < permutation.Length; i++) {
                    expectedPermuted[i] = items.ExpectedPerStmt[permutation[i]];
                }

                itemsPermuted.Add(new FilterTestMultiStmtAssertItem(items.Bean, expectedPermuted));
            }

            // find stats for this permutation
            FilterTestMultiStmtAssertStats statsPermuted = null;
            foreach (var stats in permutableCase.StatsPerPermutation) {
                if (Arrays.AreEqual(stats.Permutation, permutation)) {
                    if (statsPermuted != null) {
                        throw new IllegalStateException("Permutation " + permutation.RenderAny() + " exists twice");
                    }

                    statsPermuted = stats;
                }
            }

            if (statsPermuted == null) {
                throw new IllegalStateException("Failed to find stats for permutation " + permutation.RenderAny());
            }

            return new FilterTestMultiStmtCase(filtersPermuted, statsPermuted.Stats, itemsPermuted);
        }
    }
} // end of namespace