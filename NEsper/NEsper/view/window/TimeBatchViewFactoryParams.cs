///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.epl.expression.time;

namespace com.espertech.esper.view.window
{
    /// <summary>
    /// Parameters for batch views that provides common data flow parameter parsing.
    /// </summary>
    public class TimeBatchViewFactoryParams
    {
        /// <summary>Keyword for force Update, i.e. Update if no data. </summary>
        public const String FORCE_UPDATE_KEYWORD = "force_update";
    
        /// <summary>Keyword for starting eager, i.e. start early. </summary>
        public const String START_EAGER_KEYWORD = "start_eager";
    
        /// <summary>Event type </summary>
        private EventType _eventType;
    
        /// <summary>Number of msec before batch fires (either interval or number of events). </summary>
        private ExprTimePeriodEvalDeltaConst _timeDeltaComputation;
    
        /// <summary>Indicate whether to output only if there is data, or to keep outputting empty batches. </summary>
        private bool _isForceUpdate;
    
        /// <summary>Indicate whether to output only if there is data, or to keep outputting empty batches. </summary>
        private bool _isStartEager;
    
        /// <summary>Convert keywords into isForceUpdate and isStartEager members </summary>
    	/// <param name="keywords">flow control keyword string</param>
    	/// <param name="errorMessage">error message</param>
    	/// <throws>ViewParameterException if parsing failed</throws>
    	protected void ProcessKeywords(Object keywords, String errorMessage)
        {
    		if (!(keywords is String))
    		{
    		    throw new ViewParameterException(errorMessage);
    		}
    
    		String[] keyword = ((String) keywords).Split(',');
    		for (int i = 0; i < keyword.Length; i++)
    		{
    		    String keywordText = keyword[i].ToLower().Trim();
    		    if (keywordText.Length == 0)
    		    {
    		        continue;
    		    }
    		    if (keywordText.Equals(FORCE_UPDATE_KEYWORD))
    		    {
    		        _isForceUpdate = true;
    		    }
    		    else if (keywordText.Equals(START_EAGER_KEYWORD))
    		    {
    		        _isForceUpdate = true;
    		        _isStartEager = true;
    		    }
    		    else {
    		        const string keywordRange = FORCE_UPDATE_KEYWORD + "," + START_EAGER_KEYWORD;
    		        throw new ViewParameterException("Time-length-combination view encountered an invalid keyword '" + keywordText + "', valid control keywords are: " + keywordRange);
    		    }
    		}
    	}

        public virtual EventType EventType
        {
            get { return _eventType; }
            set { _eventType = value; }
        }

        public ExprTimePeriodEvalDeltaConst TimeDeltaComputation
        {
            get { return _timeDeltaComputation; }
            set { _timeDeltaComputation = value; }
        }

        public bool IsForceUpdate
        {
            get { return _isForceUpdate; }
            set { _isForceUpdate = value; }
        }

        public bool IsStartEager
        {
            get { return _isStartEager; }
            set { _isStartEager = value; }
        }
    }
}
