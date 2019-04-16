///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.view.util
{
    public class TimeBatchFlags
    {
        /// <summary>
        ///     Keyword for force update, i.e. update if no data.
        /// </summary>
        private const string FORCE_UPDATE_KEYWORD = "force_update";

        /// <summary>
        ///     Keyword for starting eager, i.e. start early.
        /// </summary>
        private const string START_EAGER_KEYWORD = "start_eager";

        public TimeBatchFlags(
            bool isForceUpdate,
            bool isStartEager)
        {
            IsForceUpdate = isForceUpdate;
            IsStartEager = isStartEager;
        }

        public bool IsForceUpdate { get; }

        public bool IsStartEager { get; }

        public static TimeBatchFlags ProcessKeywords(
            object keywords,
            string errorMessage)
        {
            var isForceUpdate = false;
            var isStartEager = false;
            if (!(keywords is string)) {
                throw new ViewParameterException(errorMessage);
            }

            var keyword = ((string) keywords).SplitCsv();
            for (var i = 0; i < keyword.Length; i++) {
                var keywordText = keyword[i].ToLowerInvariant().Trim();
                if (keywordText.Length == 0) {
                    continue;
                }

                if (keywordText.Equals(FORCE_UPDATE_KEYWORD)) {
                    isForceUpdate = true;
                }
                else if (keywordText.Equals(START_EAGER_KEYWORD)) {
                    isForceUpdate = true;
                    isStartEager = true;
                }
                else {
                    var keywordRange = FORCE_UPDATE_KEYWORD + "," + START_EAGER_KEYWORD;
                    throw new ViewParameterException(
                        "Time-batch encountered an invalid keyword '" + keywordText +
                        "', valid control keywords are: " + keywordRange);
                }
            }

            return new TimeBatchFlags(isForceUpdate, isStartEager);
        }
    }
} // end of namespace