
namespace MachineLearning
{


    internal class Program
    {


        public static async System.Threading.Tasks.Task<System.ConsoleKeyInfo> GetKeyAsync()
        {

            while (true)
            {
                if (System.Console.KeyAvailable)
                {
                    return System.Console.ReadKey();
                } // End if (System.Console.KeyAvailable) 

                await System.Threading.Tasks.Task.Delay(10); // Adjust delay as needed
            } // Whend 

        } // End Task GetKeyAsync 


        // See https://aka.ms/new-console-template for more information
        public static async System.Threading.Tasks.Task<int> Main(string[] args)
        {
            await System.Console.Out.WriteLineAsync(" --- Press any key to continue --- !");
            await GetKeyAsync();
            return 0;
        } // End Task Main 


    } // End Class Program 


} // End Namespace 
