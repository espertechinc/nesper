///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.expression.core
{
    /// <summary>
    /// Represents an stream selector that returns the streams underlying event, or null if undefined.
    /// </summary>
    [Serializable]
    public class ExprStreamUnderlyingNodeImpl
        : ExprNodeBase
        , ExprEvaluator
        , ExprStreamUnderlyingNode
    {
        private readonly bool _isWildcard;
        private int _streamNum = -1;
        private Type _type;

        [NonSerialized]
        private EventType _eventType;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="streamName">is the name of the stream for which to return the underlying event</param>
        /// <param name="isWildcard">if set to <c>true</c> [is wildcard].</param>
        /// <exception cref="ArgumentException">Stream name is null</exception>
        public ExprStreamUnderlyingNodeImpl(string streamName, bool isWildcard)
        {
            if ((streamName == null) && (!isWildcard))
            {
                throw new ArgumentException("Stream name is null");
            }
            StreamName = streamName;
            _isWildcard = isWildcard;
        }

        public override ExprEvaluator ExprEvaluator
        {
            get { return this; }
        }

        /// <summary>
        /// Returns the stream name.
        /// </summary>
        /// <value>stream name</value>
        public string StreamName { get; private set; }

        public int? StreamReferencedIfAny
        {
            get { return StreamId; }
        }

        public string RootPropertyNameIfAny
        {
            get { return null; }
        }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            if (StreamName == null && _isWildcard)
            {
                if (validationContext.StreamTypeService.StreamNames.Length > 1)
                {
                    throw new ExprValidationException("Wildcard must be stream wildcard if specifying multiple streams, use the 'streamname.*' syntax instead");
                }
                _streamNum = 0;
            }
            else
            {
                _streamNum = validationContext.StreamTypeService.GetStreamNumForStreamName(StreamName);
            }

            if (_streamNum == -1)
            {
                throw new ExprValidationException("Stream by name '" + StreamName + "' could not be found among all streams");
            }

            _eventType = validationContext.StreamTypeService.EventTypes[_streamNum];
            _type = _eventType.UnderlyingType;
            return null;
        }

        public virtual Type ReturnType
        {
            get
            {
                if (_streamNum == -1)
                {
                    throw new IllegalStateException("Stream underlying node has not been validated");
                }
                return _type;
            }
        }

        public override bool IsConstantResult
        {
            get { return false; }
        }

        /// <summary>
        /// Returns stream id supplying the property value.
        /// </summary>
        /// <value>stream number</value>
        public int StreamId
        {
            get
            {
                if (_streamNum == -1)
                {
                    throw new IllegalStateException("Stream underlying node has not been validated");
                }
                return _streamNum;
            }
        }

        public override string ToString()
        {
            return "streamName=" + StreamName +
                    " streamNum=" + _streamNum;
        }

        public virtual object Evaluate(EvaluateParams evaluateParams)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QExprStreamUnd(this); }
            EventBean theEvent = evaluateParams.EventsPerStream[_streamNum];
            if (theEvent == null)
            {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AExprStreamUnd(null); }
                return null;
            }
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AExprStreamUnd(theEvent.Underlying); }
            return theEvent.Underlying;
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            writer.Write(StreamName);
            if (_isWildcard)
            {
                writer.Write(".*");
            }
        }

        public override ExprPrecedenceEnum Precedence
        {
            get { return ExprPrecedenceEnum.UNARY; }
        }

        public EventType EventType
        {
            get { return _eventType; }
        }

        public override bool EqualsNode(ExprNode node, bool ignoreStreamPrefix)
        {
            var other = node as ExprStreamUnderlyingNodeImpl;
            if (other == null)
            {
                return false;
            }

            if (_isWildcard != other._isWildcard)
            {
                return false;
            }
            return _isWildcard || StreamName.Equals(other.StreamName);
        }
    }
} // end of namespace
