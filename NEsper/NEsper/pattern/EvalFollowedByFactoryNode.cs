///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;

namespace com.espertech.esper.pattern
{
    /// <summary>
    /// This class represents a followed-by operator in the evaluation tree representing any event expressions.
    /// </summary>
    [Serializable]
    public class EvalFollowedByFactoryNode : EvalNodeFactoryBase
    {
        private IList<ExprNode> _optionalMaxExpressions;
        private readonly bool _hasEngineWidePatternCount;
    
        private EvalFollowedByNodeOpType? _opType;
        private int?[] _cachedMaxPerChild;
        [NonSerialized] private ExprEvaluator[] _cachedMaxEvaluatorPerChild;

        public EvalFollowedByFactoryNode(IList<ExprNode> optionalMaxExpressions, bool hasEngineWidePatternCount) {
            _optionalMaxExpressions = optionalMaxExpressions;
            _hasEngineWidePatternCount = hasEngineWidePatternCount;
        }
    
        public override EvalNode MakeEvalNode(PatternAgentInstanceContext agentInstanceContext) {
            if (_opType == null) {
                InitOpType();
            }
    
            EvalNode[] children = EvalNodeUtil.MakeEvalNodeChildren(ChildNodes, agentInstanceContext);
            return new EvalFollowedByNode(agentInstanceContext, this, children);
        }

        public IList<ExprNode> OptionalMaxExpressions
        {
            get { return _optionalMaxExpressions; }
            set { _optionalMaxExpressions = value; }
        }

        public override String ToString()
        {
            return ("EvalFollowedByNode children=" + ChildNodes.Count);
        }
    
        protected void InitOpType() {
            bool hasMax = _optionalMaxExpressions != null && !_optionalMaxExpressions.IsEmpty();
            if (!hasMax) {
                _opType = _hasEngineWidePatternCount ? EvalFollowedByNodeOpType.NOMAX_POOL : EvalFollowedByNodeOpType.NOMAX_PLAIN;
                return;
            }

            _cachedMaxPerChild = new int?[ChildNodes.Count - 1];
            _cachedMaxEvaluatorPerChild = new ExprEvaluator[ChildNodes.Count - 1];
    
            for (int i = 0; i < ChildNodes.Count - 1; i++) {
                if (_optionalMaxExpressions.Count <= i) {
                    continue;
                }
                var optionalMaxExpression = _optionalMaxExpressions[i];
                if (optionalMaxExpression == null) {
                    continue;
                }
                if (optionalMaxExpression.IsConstantResult)
                {
                    var result = optionalMaxExpression.ExprEvaluator.Evaluate(new EvaluateParams(null, true, null));
                    if (result != null) {
                        _cachedMaxPerChild[i] = result.AsInt();
                    }
                }
                else {
                    _cachedMaxEvaluatorPerChild[i] = _optionalMaxExpressions[i].ExprEvaluator;
                }
            }
    
            _opType = _hasEngineWidePatternCount ? EvalFollowedByNodeOpType.MAX_POOL : EvalFollowedByNodeOpType.MAX_PLAIN;
        }

        public EvalFollowedByNodeOpType OpType
        {
            get { return _opType.Value; }
        }

        public int GetMax(int position)
        {
            var cached = _cachedMaxPerChild[position];
            if (cached != null) {
                return cached.Value;  // constant value cached
            }
    
            var cachedExpr = _cachedMaxEvaluatorPerChild[position];
            if (cachedExpr == null) {
                return -1;  // no limit defined for this sub-expression
            }
    
            var result = cachedExpr.Evaluate(new EvaluateParams(null, true, null));
            if (result != null) {
                return result.AsInt();
            }
            return -1;  // no limit
        }

        public override bool IsFilterChildNonQuitting
        {
            get { return false; }
        }

        public override bool IsStateful
        {
            get { return true; }
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer) {
            if (_optionalMaxExpressions == null || _optionalMaxExpressions.IsEmpty()) {
                PatternExpressionUtil.ToPrecedenceFreeEPL(writer, "->", ChildNodes, Precedence);
            }
            else {
                ChildNodes[0].ToEPL(writer, PatternExpressionPrecedenceEnum.MINIMUM);
                for (int i = 1; i < ChildNodes.Count; i++) {
                    ExprNode optionalMaxExpression = null;
                    if (_optionalMaxExpressions.Count > (i - 1)) {
                        optionalMaxExpression = _optionalMaxExpressions[i - 1];
                    }
                    if (optionalMaxExpression == null) {
                        writer.Write(" -> ");
                    }
                    else {
                        writer.Write(" -[");
                        writer.Write(ExprNodeUtility.ToExpressionStringMinPrecedenceSafe(optionalMaxExpression));
                        writer.Write("]> ");
                    }
                    ChildNodes[i].ToEPL(writer, PatternExpressionPrecedenceEnum.MINIMUM);
                }
            }
        }

        public override PatternExpressionPrecedenceEnum Precedence
        {
            get { return PatternExpressionPrecedenceEnum.FOLLOWEDBY; }
        }
    }
}
