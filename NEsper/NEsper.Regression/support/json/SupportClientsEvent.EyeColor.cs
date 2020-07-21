namespace com.espertech.esper.regressionlib.support.json
{
    public partial class SupportClientsEvent
    {
        public enum EyeColor
        {
            BROWN,
            BLUE,
            GREEN
        }
        
        public static EyeColor FromNumber(int i)
        {
            switch (i) {
                case 0:
                    return EyeColor.BROWN;
                case 1:
                    return EyeColor.BLUE;
                default:
                    return EyeColor.GREEN;
            }
        }
    }
}