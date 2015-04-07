///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.spec;
using com.espertech.esper.pattern.observer;

namespace com.espertech.esper.pattern
{
    /// <summary>
    /// This class represents an observer expression in the evaluation tree representing an pattern expression.
    /// </summary>
    public class EvalObserverFactoryNode : EvalNodeFactoryBase
    {
        private readonly PatternObserverSpec _patternObserverSpec;
        [NonSerialized]
        private ObserverFactory _observerFactory;
    
        /// <summary>Constructor. </summary>
        /// <param name="patternObserverSpec">is the factory to use to get an observer instance</param>
        public EvalObserverFactoryNode(PatternObserverSpec patternObserverSpec)
        {
            _patternObserverSpec = patternObserverSpec;
        }
    
        public override EvalNode MakeEvalNode(PatternAgentInstanceContext agentInstanceContext)
        {
            return new EvalObserverNode(agentInstanceContext, this);
        }

        /// <summary>Returns the observer object specification to use for instantiating the observer factory and observer. </summary>
        /// <value>observer specification</value>
        public PatternObserverSpec PatternObserverSpec
        {
            get { return _patternObserverSpec; }
        }

        /// <summary>Returns the observer factory. </summary>
        /// <value>factory for observer instances</value>
        public ObserverFactory ObserverFactory
        {
            get { return _observerFactory; }
            set { _observerFactory = value; }
        }

        public override String ToString()
        {
            return ("EvalObserverNode observerFactory=" + _observerFactory +
                    "  children=" + ChildNodes.Count);
        }

        public override bool IsFilterChildNonQuitting
        {
            get { return false; }
        }

        public override bool IsStateful
        {
            get { return false; }
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            writer.Write(_patternObserverSpec.ObjectNamespace);
            writer.Write(":");
            writer.Write(_patternObserverSpec.ObjectName);
            writer.Write("(");
            ExprNodeUtility.ToExpressionStringParameterList(_patternObserverSpec.ObjectParameters, writer);
            writer.Write(")");
        }

        public String ToPrecedenceFreeEPL()
        {
            var writer = new StringWriter();
            ToPrecedenceFreeEPL(writer);
            return writer.ToString();
        }

        public override PatternExpressionPrecedenceEnum Precedence
        {
            get { return PatternExpressionPrecedenceEnum.ATOM; }
        }

        public virtual bool IsObserverStateNodeNonRestarting
        {
            get { return _observerFactory.IsNonRestarting(); }
        }
    }
}
