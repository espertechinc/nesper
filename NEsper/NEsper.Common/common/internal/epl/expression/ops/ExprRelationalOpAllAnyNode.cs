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

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
	/// <summary>
	/// Represents a lesser or greater then (&lt;/&lt;=/&gt;/&gt;=) expression in a filter expression tree.
	/// </summary>
	[Serializable]
	public class ExprRelationalOpAllAnyNode : ExprNodeBase {

	    private readonly RelationalOpEnum relationalOpEnum;
	    private readonly bool isAll;

	    [NonSerialized] private ExprRelationalOpAllAnyNodeForge forge;

	    /// <summary>
	    /// Ctor.
	    /// </summary>
	    /// <param name="relationalOpEnum">type of compare, ie. lt, gt, le, ge</param>
	    /// <param name="isAll">true if all, false for any</param>
	    public ExprRelationalOpAllAnyNode(RelationalOpEnum relationalOpEnum, bool isAll) {
	        this.relationalOpEnum = relationalOpEnum;
	        this.isAll = isAll;
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

	    public bool IsConstantResult
	    {
	        get => false;
	    }

	    /// <summary>
	    /// Returns true for ALL, false for ANY.
	    /// </summary>
	    /// <returns>indicator all or any</returns>
	    public bool IsAll
	    {
	        get => isAll;
	    }

	    /// <summary>
	    /// Returns the type of relational op used.
	    /// </summary>
	    /// <returns>enum with relational op type</returns>
	    public RelationalOpEnum RelationalOpEnum
	    {
	        get => relationalOpEnum;
	    }

	    public override ExprNode Validate(ExprValidationContext validationContext) {
	        // Must have 2 child nodes
	        if (this.ChildNodes.Length < 1) {
	            throw new IllegalStateException("Group relational op node must have 1 or more parameters");
	        }
	        Type typeOne = Boxing.GetBoxedType(ChildNodes[0].Forge.EvaluationType);

	        // collections, array or map not supported
	        if ((typeOne.IsArray) 
	            || (TypeHelper.IsImplementsInterface(typeOne, typeof(ICollection<object>)))
	            || (TypeHelper.IsImplementsInterface(typeOne, typeof(IDictionary<object, object>)))) {
	            throw new ExprValidationException("Collection or array comparison is not allowed for the IN, ANY, SOME or ALL keywords");
	        }

	        IList<Type> comparedTypes = new List<Type>();
	        comparedTypes.Add(typeOne);
	        bool hasCollectionOrArray = false;
	        for (int i = 0; i < this.ChildNodes.Length - 1; i++) {
	            Type propType = ChildNodes[i + 1].Forge.EvaluationType;
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

	        // Must be either numeric or string
	        if (coercionType != typeof(string)) {
	            if (!TypeHelper.IsNumeric(coercionType)) {
	                throw new ExprValidationException("Implicit conversion from datatype '" +
	                        coercionType.GetSimpleName() +
	                        "' to numeric is not allowed");
	            }
	        }

	        RelationalOpEnum.Computer computer = relationalOpEnum.GetComputer(coercionType, coercionType, coercionType);
	        forge = new ExprRelationalOpAllAnyNodeForge(this, computer, hasCollectionOrArray);
	        return null;
	    }

	    public override void ToPrecedenceFreeEPL(StringWriter writer) {
	        this.ChildNodes[0].ToEPL(writer, Precedence);
	        writer.Write(relationalOpEnum.ExpressionText);
	        if (isAll) {
	            writer.Write("all");
	        } else {
	            writer.Write("any");
	        }

	        writer.Write("(");
	        string delimiter = "";

	        for (int i = 0; i < this.ChildNodes.Length - 1; i++) {
	            writer.Write(delimiter);
	            this.ChildNodes[i + 1].ToEPL(writer, Precedence);
	            delimiter = ",";
	        }
	        writer.Write(")");
	    }

	    public override ExprPrecedenceEnum Precedence
	    {
	        get => ExprPrecedenceEnum.RELATIONAL_BETWEEN_IN;
	    }

	    public override bool EqualsNode(ExprNode node, bool ignoreStreamPrefix) {
	        if (!(node is ExprRelationalOpAllAnyNode)) {
	            return false;
	        }

	        ExprRelationalOpAllAnyNode other = (ExprRelationalOpAllAnyNode) node;

	        if ((other.relationalOpEnum != this.relationalOpEnum) ||
	                (other.isAll != this.isAll)) {
	            return false;
	        }

	        return true;
	    }
	}
} // end of namespace