///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

namespace com.espertech.esper.pattern
{
    /// <summary>
    /// This class represents an 'or' operator in the evaluation tree representing any event expressions.
    /// </summary>
    [Serializable]
    public class EvalAuditFactoryNode : EvalNodeFactoryBase
    {
        private readonly bool _auditPattern;
        private readonly bool _auditPatternInstance;
        private readonly String _patternExpr;
        [NonSerialized]
        private readonly EvalAuditInstanceCount _instanceCount;
        private readonly bool _filterChildNonQuitting;

        public EvalAuditFactoryNode(bool auditPattern, bool auditPatternInstance, String patternExpr, EvalAuditInstanceCount instanceCount, bool filterChildNonQuitting)
        {
            _auditPattern = auditPattern;
            _auditPatternInstance = auditPatternInstance;
            _patternExpr = patternExpr;
            _instanceCount = instanceCount;
            _filterChildNonQuitting = filterChildNonQuitting;
        }
    
        public override EvalNode MakeEvalNode(PatternAgentInstanceContext agentInstanceContext, EvalNode parentNode)
        {
            EvalNode child = EvalNodeUtil.MakeEvalNodeSingleChild(ChildNodes, agentInstanceContext, parentNode);
            return new EvalAuditNode(agentInstanceContext, this, child);
        }

        public bool IsAuditPattern
        {
            get { return _auditPattern; }
        }

        public string PatternExpr
        {
            get { return _patternExpr; }
        }

        public override String ToString()
        {
            return ("EvalAuditFactoryNode children=" + ChildNodes.Count);
        }

        public void DecreaseRefCount(EvalAuditStateNode current, PatternContext patternContext)
        {
            if (!_auditPatternInstance)
            {
                return;
            }
            _instanceCount.DecreaseRefCount(
                ChildNodes[0], current, _patternExpr, patternContext.StatementName, patternContext.EngineURI);
        }

        public void IncreaseRefCount(EvalAuditStateNode current, PatternContext patternContext)
        {
            if (!_auditPatternInstance)
            {
                return;
            }
            _instanceCount.IncreaseRefCount(
                ChildNodes[0], current, _patternExpr, patternContext.StatementName, patternContext.EngineURI);
        }

        public override bool IsFilterChildNonQuitting
        {
            get { return _filterChildNonQuitting; }
        }

        public override bool IsStateful
        {
            get { return ChildNodes[0].IsStateful; }
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer) {
            ChildNodes[0].ToEPL(writer, Precedence);
        }

        public override PatternExpressionPrecedenceEnum Precedence
        {
            get { return ChildNodes[0].Precedence; }
        }
    }
}
