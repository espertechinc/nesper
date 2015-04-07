namespace com.espertech.esper.compat.collections
{
    public class Tuple<TA,TB>
    {
        public TA A { get; set; }
        public TB B { get; set; }
    }

    public class Tuple<TA,TB,TC>
    {
        public TA A { get; set; }
        public TB B { get; set; }
        public TC C { get; set; }
    }
}
