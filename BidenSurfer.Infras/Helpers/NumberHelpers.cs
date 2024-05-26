namespace BidenSurfer.Infras.Helpers
{
    public static class NumberHelpers
    {
        public static decimal RandomDecimal(decimal minValue, decimal maxValue)
        {
            var random = new Random();
            double doubleMinValue = Convert.ToDouble(minValue);
            double doubleMaxValue = Convert.ToDouble(maxValue);
            double randomDouble = random.NextDouble() * (doubleMaxValue - doubleMinValue) + doubleMinValue;
            return Convert.ToDecimal(randomDouble);
        }

        public static int RandomInt(int minValue, int maxValue)
        {
            var random = new Random();
            return random.Next(minValue, maxValue);
        }
    }
}
