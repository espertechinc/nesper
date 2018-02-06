///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.table.mgmt;

namespace com.espertech.esper.epl.expression.table
{
    [Serializable]
    public abstract class ExprTableAccessNode : ExprNodeBase
    {
        private readonly string _tableName;

        [NonSerialized] private ExprTableAccessEvalStrategy _strategy;
        [NonSerialized] private ExprEvaluator[] _groupKeyEvaluators;
    
        protected abstract void ValidateBindingInternal(ExprValidationContext validationContext, TableMetadata tableMetadata);
        protected abstract bool EqualsNodeInternal(ExprTableAccessNode other);
    
        protected ExprTableAccessNode(string tableName)
        {
            _tableName = tableName;
        }

        public string TableName
        {
            get { return _tableName; }
        }

        public ExprEvaluator[] GroupKeyEvaluators
        {
            get { return _groupKeyEvaluators; }
        }

        public ExprTableAccessEvalStrategy Strategy
        {
            internal get { return _strategy; }
            set { _strategy = value; }
        }

        public override bool IsConstantResult
        {
            get { return false; }
        }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            if (validationContext.TableService == null || !validationContext.IsAllowBindingConsumption)
            {
                throw new ExprValidationException("Invalid use of table access expression, expression '" + _tableName + "' is not allowed here");
            }
            TableMetadata metadata = validationContext.TableService.GetTableMetadata(_tableName);
            if (metadata == null) {
                throw new ExprValidationException("A table '" + _tableName + "' could not be found");
            }
    
            if (metadata.ContextName != null &&
                validationContext.ContextDescriptor != null &&
                !metadata.ContextName.Equals(validationContext.ContextDescriptor.ContextName)) {
                throw new ExprValidationException("Table by name '" + _tableName + "' has been declared for context '" + metadata.ContextName + "' and can only be used within the same context");
            }
    
            // additional validations depend on detail
            ValidateBindingInternal(validationContext, metadata);
            return null;
        }
    
        protected void ValidateGroupKeys(TableMetadata metadata)
        {
            if (ChildNodes.Count > 0) {
                _groupKeyEvaluators = ExprNodeUtility.GetEvaluators(ChildNodes);
            }
            else {
                _groupKeyEvaluators = new ExprEvaluator[0];
            }
            var typesReturned = ExprNodeUtility.GetExprResultTypes(_groupKeyEvaluators);
            ExprTableNodeUtil.ValidateExpressions(_tableName, typesReturned, "key", ChildNodes, metadata.KeyTypes, "key");
        }

        public override ExprPrecedenceEnum Precedence
        {
            get { return ExprPrecedenceEnum.UNARY; }
        }

        protected void ToPrecedenceFreeEPLInternal(TextWriter writer, string subprop)
        {
            ToPrecedenceFreeEPLInternal(writer);
            writer.Write(".");
            writer.Write(subprop);
        }
    
        protected void ToPrecedenceFreeEPLInternal(TextWriter writer)
        {
            writer.Write(_tableName);
            if (ChildNodes.Count > 0) {
                writer.Write("[");
                var delimiter = "";
                foreach (var expr in ChildNodes) {
                    writer.Write(delimiter);
                    expr.ToEPL(writer, ExprPrecedenceEnum.MINIMUM);
                    delimiter = ",";
                }
                writer.Write("]");
            }
        }
    
        protected TableMetadataColumn ValidateSubpropertyGetCol(TableMetadata tableMetadata, string subpropName)
        {
            var column = tableMetadata.TableColumns.Get(subpropName);
            if (column == null) {
                throw new ExprValidationException("A column '" + subpropName + "' could not be found for table '" + _tableName + "'");
            }
            return column;
        }
    
        public override bool EqualsNode(ExprNode o, bool ignoreStreamPrefix)
        {
            if (this == o) return true;
            if (o == null || GetType() != o.GetType()) return false;
    
            var that = (ExprTableAccessNode) o;
            if (!_tableName.Equals(that._tableName)) return false;
    
            return EqualsNodeInternal(that);
        }
    
        public override int GetHashCode()
        {
            return _tableName != null ? _tableName.GetHashCode() : 0;
        }
    }
}
