
namespace MachineLearning
{

    // Starting from.NET6, there is thread-safe Random out of the box:
    // var rndIntValue = Random.Shared.Next();
    internal static class RandomHelper
    {


        [System.ThreadStatic]
        private static System.Random random = new System.Random();


        private static System.Random _global = new System.Random();
        private static System.Threading.ThreadLocal<System.Random> _local = new System.Threading.ThreadLocal<System.Random>(() =>
        {
            int seed;
            lock (_global) seed = _global.Next();
            return new System.Random(seed);
        });


        public static double NextDouble()
        {
            return random.NextDouble();
        } // End Function NextDouble 


        public static int Next(int min, int max)
        {
            if (min >= max)
            {
                throw new System.ArgumentOutOfRangeException("min", "Minimum value must be less than maximum value.");
            }

            return random.Next(min, max);
        } // End Function Next 


        public static double GenerateRandomNumber(double min = 0.0, double max = 0.23)
        {
            if (min >= max)
            {
                throw new System.ArgumentOutOfRangeException("min", "Minimum value must be less than maximum value.");
            }

            return random.NextDouble() * (max - min) + min;
        } // End Function GenerateRandomNumber 


    } // End static class RandomHelper 


} // End Namespace 
