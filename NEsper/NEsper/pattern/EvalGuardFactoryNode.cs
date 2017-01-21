///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.spec;
using com.espertech.esper.pattern.guard;

namespace com.espertech.esper.pattern
{
    /// <summary>
    /// This class represents a guard in the evaluation tree representing an event expressions.
    /// </summary>
    [Serializable]
    public class EvalGuardFactoryNode : EvalNodeFactoryBase
    {
        private readonly PatternGuardSpec _patternGuardSpec;
        [NonSerialized] private GuardFactory _guardFactory;

        /// <summary>Constructor. </summary>
        /// <param name="patternGuardSpec">factory for guard construction</param>
        public EvalGuardFactoryNode(PatternGuardSpec patternGuardSpec)
        {
            _patternGuardSpec = patternGuardSpec;
        }

        public override EvalNode MakeEvalNode(PatternAgentInstanceContext agentInstanceContext, EvalNode parentNode)
        {
            EvalNode child = EvalNodeUtil.MakeEvalNodeSingleChild(ChildNodes, agentInstanceContext, parentNode);
            return new EvalGuardNode(agentInstanceContext, this, child);
        }

        /// <summary>Returns the guard object specification to use for instantiating the guard factory and guard. </summary>
        /// <value>guard specification</value>
        public PatternGuardSpec PatternGuardSpec
        {
            get { return _patternGuardSpec; }
        }

        /// <summary>Supplies the guard factory to the node. </summary>
        /// <value>is the guard factory</value>
        public GuardFactory GuardFactory
        {
            set { _guardFactory = value; }
            get { return _guardFactory; }
        }

        public override String ToString()
        {
            return ("EvalGuardNode guardFactory=" + _guardFactory +
                    "  children=" + ChildNodes.Count);
        }

        public override bool IsFilterChildNonQuitting
        {
            get { return false; }
        }

        public override bool IsStateful
        {
            get { return true; }
        }

        public String ToPrecedenceFreeEPL()
        {
            var writer = new StringWriter();
            ToPrecedenceFreeEPL(writer);
            return writer.ToString();
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            ChildNodes[0].ToEPL(writer, Precedence);
            if (_patternGuardSpec.ObjectNamespace == GuardEnum.WHILE_GUARD.GetNamespace() &&
                _patternGuardSpec.ObjectName == GuardEnum.WHILE_GUARD.GetName())
            {
                writer.Write(" while ");
            }
            else
            {
                writer.Write(" where ");
                writer.Write(_patternGuardSpec.ObjectNamespace);
                writer.Write(":");
                writer.Write(_patternGuardSpec.ObjectName);
            }
            writer.Write("(");
            ExprNodeUtility.ToExpressionStringParameterList(_patternGuardSpec.ObjectParameters, writer);
            writer.Write(")");
        }

        public override PatternExpressionPrecedenceEnum Precedence
        {
            get { return PatternExpressionPrecedenceEnum.GUARD_POSTFIX; }
        }
    }
}