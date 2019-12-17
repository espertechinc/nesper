namespace com.espertech.esper.compat.collections
{
    public class Continuation
    {
        public Continuation(bool continuationValue)
        {
            this.Value = continuationValue;
        }

        public bool Value { get; set; }
    }
}
