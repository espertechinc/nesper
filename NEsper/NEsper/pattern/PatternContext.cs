///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.core.service;
using com.espertech.esper.epl.variable;
using com.espertech.esper.events;
using com.espertech.esper.filter;
using com.espertech.esper.schedule;

namespace com.espertech.esper.pattern
{
    /// <summary>Contains handles to implementations of services needed by evaluation nodes. </summary>
    public class PatternContext
    {
        private readonly int _streamNumber;
        private readonly StatementContext _statementContext;
        private readonly MatchedEventMapMeta _matchedEventMapMeta;
        private readonly bool _isResilient;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="statementContext">is the statement context</param>
        /// <param name="streamNumber">is the stream number</param>
        /// <param name="matchedEventMapMeta">The matched event map meta.</param>
        /// <param name="isResilient">if set to <c>true</c> [is resilient].</param>
        public PatternContext(StatementContext statementContext,
                              int streamNumber,
                              MatchedEventMapMeta matchedEventMapMeta,
                              bool isResilient)
        {
            _streamNumber = streamNumber;
            _statementContext = statementContext;
            _matchedEventMapMeta = matchedEventMapMeta;
            _isResilient = isResilient;
        }

        /// <summary>Returns service to use for filter evaluation. </summary>
        /// <value>filter evaluation service implemetation</value>
        public FilterService FilterService
        {
            get { return _statementContext.FilterService; }
        }

        /// <summary>Returns service to use for schedule evaluation. </summary>
        /// <value>schedule evaluation service implemetation</value>
        public SchedulingService SchedulingService
        {
            get { return _statementContext.SchedulingService; }
        }

        /// <summary>Returns the schedule bucket for ordering schedule callbacks within this pattern. </summary>
        /// <value>schedule bucket</value>
        public ScheduleBucket ScheduleBucket
        {
            get { return _statementContext.ScheduleBucket; }
        }

        /// <summary>Returns teh service providing event adaptering or wrapping. </summary>
        /// <value>event adapter service</value>
        public EventAdapterService EventAdapterService
        {
            get { return _statementContext.EventAdapterService; }
        }

        /// <summary>Returns the statement's resource handle for locking. </summary>
        /// <value>handle of statement</value>
        public EPStatementHandle EpStatementHandle
        {
            get { return _statementContext.EpStatementHandle; }
        }

        /// <summary>Returns the statement id. </summary>
        /// <value>statement id</value>
        public string StatementId
        {
            get { return _statementContext.StatementId; }
        }

        /// <summary>Returns the statement name. </summary>
        /// <value>statement name</value>
        public string StatementName
        {
            get { return _statementContext.StatementName; }
        }

        /// <summary>Returns the stream number. </summary>
        /// <value>stream number</value>
        public int StreamNumber
        {
            get { return _streamNumber; }
        }

        /// <summary>Returns the engine URI. </summary>
        /// <value>engine URI</value>
        public string EngineURI
        {
            get { return _statementContext.EngineURI; }
        }

        /// <summary>Returns extension services context for statement (statement-specific). </summary>
        /// <value>extension services</value>
        public StatementExtensionSvcContext StatementExtensionServicesContext
        {
            get { return _statementContext.StatementExtensionServicesContext; }
        }

        /// <summary>Returns the variable service. </summary>
        /// <value>variable service</value>
        public VariableService VariableService
        {
            get { return _statementContext.VariableService; }
        }

        public TimeProvider TimeProvider
        {
            get { return _statementContext.TimeProvider; }
        }

        public ExceptionHandlingService ExceptionHandlingService
        {
            get { return _statementContext.ExceptionHandlingService; }
        }

        public StatementContext StatementContext
        {
            get { return _statementContext; }
        }

        public MatchedEventMapMeta MatchedEventMapMeta
        {
            get { return _matchedEventMapMeta; }
        }

        public bool IsResilient
        {
            get { return _isResilient; }
        }
    }
}
