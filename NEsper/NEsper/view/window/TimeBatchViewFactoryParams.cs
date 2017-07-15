///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.expression.time;
using com.espertech.esper.view;

namespace com.espertech.esper.view.window
{
    /// <summary>
    /// Parameters for batch views that provides common data flow parameter parsing.
    /// </summary>
    public class TimeBatchViewFactoryParams {
    
        /// <summary>Keyword for force update, i.e. update if no data.</summary>
        internal static readonly string FORCE_UPDATE_KEYWORD = "force_update";
    
        /// <summary>Keyword for starting eager, i.e. start early.</summary>
        internal static readonly string START_EAGER_KEYWORD = "start_eager";
    
        /// <summary>Event type</summary>
        protected EventType eventType;
    
        /// <summary>
        /// Number of msec before batch fires (either interval or number of events).
        /// </summary>
        protected ExprTimePeriodEvalDeltaConstFactory timeDeltaComputationFactory;
    
        /// <summary>
        /// Indicate whether to output only if there is data, or to keep outputting empty batches.
        /// </summary>
        protected bool isForceUpdate;
    
        /// <summary>
        /// Indicate whether to output only if there is data, or to keep outputting empty batches.
        /// </summary>
        protected bool isStartEager;
    
        /// <summary>
        /// Convert keywords into isForceUpdate and isStartEager members
        /// </summary>
        /// <param name="keywords">flow control keyword string expression</param>
        /// <param name="errorMessage">error message</param>
        /// <exception cref="ViewParameterException">if parsing failed</exception>
        protected void ProcessKeywords(Object keywords, string errorMessage) {
    
            if (!(keywords is string)) {
                throw new ViewParameterException(errorMessage);
            }
    
            string[] keyword = ((string) keywords).Split(",");
            for (int i = 0; i < keyword.Length; i++) {
                string keywordText = keyword[i].ToLowerInvariant().Trim();
                if (keywordText.Length() == 0) {
                    continue;
                }
                if (keywordText.Equals(FORCE_UPDATE_KEYWORD)) {
                    isForceUpdate = true;
                } else if (keywordText.Equals(START_EAGER_KEYWORD)) {
                    isForceUpdate = true;
                    isStartEager = true;
                } else {
                    string keywordRange = FORCE_UPDATE_KEYWORD + "," + START_EAGER_KEYWORD;
                    throw new ViewParameterException("TimeInMillis-length-combination view encountered an invalid keyword '" + keywordText + "', valid control keywords are: " + keywordRange);
                }
            }
        }
    
        public bool IsForceUpdate() {
            return isForceUpdate;
        }
    
        public bool IsStartEager() {
            return isStartEager;
        }
    }
} // end of namespace
