///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.IO;

using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.expression.funcs
{
	public class ExprEventIdentityEqualsNode : ExprNodeBase
	{
		public const string NAME = "event_identity_equals";

		private ExprEventIdentityEqualsNodeForge _forge;

		public override ExprNode Validate(ExprValidationContext validationContext)
		{
			if (ChildNodes.Length != 2) {
				throw new ExprValidationException(NAME + "requires two parameters");
			}

			ExprStreamUnderlyingNode undOne = CheckStreamUnd(ChildNodes[0]);
			ExprStreamUnderlyingNode undTwo = CheckStreamUnd(ChildNodes[1]);
			if (undOne.EventType != undTwo.EventType) {
				throw new ExprValidationException(
					NAME +
					" received two different event types as parameter, type '" +
					undOne.EventType.Name +
					"' is not the same as type '" +
					undTwo.EventType.Name +
					"'");
			}

			_forge = new ExprEventIdentityEqualsNodeForge(this, undOne, undTwo);
			return null;
		}

		private ExprStreamUnderlyingNode CheckStreamUnd(ExprNode childNode)
		{
			if (!(childNode is ExprStreamUnderlyingNode)) {
				throw new ExprValidationException(
					NAME +
					" requires a parameter that resolves to an event but received '" +
					ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(childNode) +
					"'");
			}

			return (ExprStreamUnderlyingNode) childNode;
		}

		public override ExprForge Forge {
			get {
				CheckValidated(_forge);
				return _forge;
			}
		}

		public override bool EqualsNode(
			ExprNode node,
			bool ignoreStreamPrefix)
		{
			return node is ExprEventIdentityEqualsNode;
		}

		public override void ToPrecedenceFreeEPL(
			TextWriter writer,
			ExprNodeRenderableFlags flags)
		{
			writer.Write(NAME);
			ExprNodeUtilityPrint.ToExpressionStringParams(writer, ChildNodes);
		}

		public override ExprPrecedenceEnum Precedence {
			get { return ExprPrecedenceEnum.UNARY; }
		}
	}
} // end of namespace
