
namespace MachineLearning
{


    public class TestAsyncWithTimeout
    {


        public static async System.Threading.Tasks.Task Test()
        {
            try
            {
                // Example task that takes 5 seconds to complete and returns an int
                System.Threading.Tasks.Task<int> longRunningTask = System.Threading.Tasks.Task.Run(async () =>
                {
                    await System.Threading.Tasks.Task.Delay(5000);
                    return 42;
                });

                // Call the method with a 3-second timeout
                int result = await RunWithTimeout(longRunningTask, System.TimeSpan.FromSeconds(3));

                System.Console.WriteLine($"Task completed successfully with result: {result}");
            }
            catch (System.TimeoutException)
            {
                System.Console.WriteLine("Task timed out.");
            }
        }


        public static async System.Threading.Tasks.Task Test2()
        {
            try
            {
                // Example task that takes 5 seconds to complete
                System.Threading.Tasks.Task longRunningTask = System.Threading.Tasks.Task.Delay(5000);

                // Call the method with a 3-second timeout
                await RunWithTimeout(longRunningTask, System.TimeSpan.FromSeconds(3));

                System.Console.WriteLine("Task completed successfully.");
            }
            catch (System.TimeoutException)
            {
                System.Console.WriteLine("Task timed out.");
            }
        }



        public static async System.Threading.Tasks.Task RunWithTimeout(System.Threading.Tasks.Task task, System.TimeSpan timeout)
        {
            // Create a timeout task
            System.Threading.Tasks.Task timeoutTask = System.Threading.Tasks.Task.Delay(timeout);

            // Wait for either the task or the timeout task to complete
            System.Threading.Tasks.Task completedTask = await System.Threading.Tasks.Task.WhenAny(task, timeoutTask);

            if (completedTask == timeoutTask)
            {
                throw new System.TimeoutException("The operation has timed out.");
            }

            // Await the original task to propagate exceptions if it faulted
            await task;
        }


        public static async System.Threading.Tasks.Task<T> RunWithTimeout<T>(System.Threading.Tasks.Task<T> task, System.TimeSpan timeout)
        {
            // Create a timeout task
            System.Threading.Tasks.Task timeoutTask = System.Threading.Tasks.Task.Delay(timeout);

            // Wait for either the task or the timeout task to complete
            System.Threading.Tasks.Task completedTask = await System.Threading.Tasks.Task.WhenAny(task, timeoutTask);

            if (completedTask == timeoutTask)
            {
                throw new System.TimeoutException("The operation has timed out.");
            }

            // Await the original task to propagate exceptions and get the result if it completes
            return await task;
        }


    }



}
