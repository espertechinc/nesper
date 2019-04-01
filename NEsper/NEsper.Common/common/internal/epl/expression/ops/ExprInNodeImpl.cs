///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    /// <summary>
    /// Represents the in-clause (set check) function in an expression tree.
    /// </summary>
    [Serializable]
    public class ExprInNodeImpl : ExprNodeBase , ExprInNode {

	    private readonly bool isNotIn;

	    [NonSerialized] private ExprInNodeForge forge;

	    /// <summary>
	    /// Ctor.
	    /// </summary>
	    /// <param name="isNotIn">is true for "not in" and false for "in"</param>
	    public ExprInNodeImpl(bool isNotIn) {
	        this.isNotIn = isNotIn;
	    }

        public ExprEvaluator ExprEvaluator {
            get {
                CheckValidated(forge);
                return forge.ExprEvaluator;
            }
        }

        public override ExprForge Forge
	    {
	        get => forge;
	    }

	    /// <summary>
	    /// Returns true for not-in, false for regular in
	    /// </summary>
	    /// <returns>false for "val in (a,b,c)" or true for "val not in (a,b,c)"</returns>
	    public bool IsNotIn
	    {
	        get => isNotIn;
	    }

	    public override ExprNode Validate(ExprValidationContext validationContext) {
	        ValidateWithoutContext();
	        return null;
	    }

	    public void ValidateWithoutContext() {
	        if (this.ChildNodes.Length < 2) {
	            throw new ExprValidationException("The IN operator requires at least 2 child expressions");
	        }

	        // Must be the same boxed type returned by expressions under this
	        Type typeOne = Boxing.GetBoxedType(ChildNodes[0].Forge.EvaluationType);

	        // collections, array or map not supported
	        if ((typeOne.IsArray) 
	            || (TypeHelper.IsImplementsInterface(typeOne, typeof(ICollection<object>)))
	            || (TypeHelper.IsImplementsInterface(typeOne, typeof(IDictionary<object,object>)))) {
	            throw new ExprValidationException("Collection or array comparison is not allowed for the IN, ANY, SOME or ALL keywords");
	        }

	        IList<Type> comparedTypes = new List<Type>();
	        comparedTypes.Add(typeOne);
	        bool hasCollectionOrArray = false;
	        for (int i = 0; i < this.ChildNodes.Length - 1; i++) {
	            Type propType = ChildNodes[i + 1].Forge.EvaluationType;
	            if (propType == null) {
	                continue;
	            }
	            if (propType.IsArray) {
	                hasCollectionOrArray = true;
	                if (propType.GetElementType() != typeof(object)) {
	                    comparedTypes.Add(propType.GetElementType());
	                }
	            } else if (TypeHelper.IsImplementsInterface(propType, typeof(ICollection<object>))) {
	                hasCollectionOrArray = true;
	            } else if (TypeHelper.IsImplementsInterface(propType, typeof(IDictionary<object, object>))) {
	                hasCollectionOrArray = true;
	            } else {
	                comparedTypes.Add(propType);
	            }
	        }

	        // Determine common denominator type
	        Type coercionType;
	        try {
	            coercionType = TypeHelper.GetCommonCoercionType(comparedTypes.ToArray());
	        } catch (CoercionException ex) {
	            throw new ExprValidationException("Implicit conversion not allowed: " + ex.Message);
	        }

	        // Check if we need to coerce
	        bool mustCoerce = false;
	        Coercer coercer = null;
	        if (TypeHelper.IsNumeric(coercionType)) {
	            foreach (Type compareType in comparedTypes) {
	                if (coercionType != Boxing.GetBoxedType(compareType)) {
	                    mustCoerce = true;
	                }
	            }
	            if (mustCoerce) {
	                coercer = CoercerFactory.GetCoercer(null, Boxing.GetBoxedType(coercionType));
	            }
	        }

	        forge = new ExprInNodeForge(this, mustCoerce, coercer, coercionType, hasCollectionOrArray);
	    }

	    public bool IsConstantResult
	    {
	        get => false;
	    }

        public override bool EqualsNode(ExprNode node, bool ignoreStreamPrefix) {
	        if (!(node is ExprInNodeImpl)) {
	            return false;
	        }

	        ExprInNodeImpl other = (ExprInNodeImpl) node;
	        return other.isNotIn == this.isNotIn;
	    }

	    public override void ToPrecedenceFreeEPL(StringWriter writer) {
	        string delimiter = "";
	        IEnumerator<ExprNode> it = Arrays.AsList(this.ChildNodes).GetEnumerator();
	        it.Current.ToEPL(writer, Precedence);
	        if (isNotIn) {
	            writer.Write(" not in (");
	        } else {
	            writer.Write(" in (");
	        }

	        do {
	            ExprNode inSetValueExpr = it.Current;
	            writer.Write(delimiter);
	            inSetValueExpr.ToEPL(writer, Precedence);
	            delimiter = ",";
	        }
	        while (it.MoveNext());
	        writer.Write(')');
	    }

	    public override ExprPrecedenceEnum Precedence
	    {
	        get => ExprPrecedenceEnum.RELATIONAL_BETWEEN_IN;
	    }
	}
} // end of namespace