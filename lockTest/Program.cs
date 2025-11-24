using System.Threading.Tasks;

namespace lockTest
{
    internal class Program
    {
        private static readonly AccsessLock cooldown = new AccsessLock();

        static async Task Main(string[] args)
        {
            Console.WriteLine("start.....");
            Console.ReadKey();

            await RunTest();
            Console.ReadKey();
        }



        public static async Task RunTest()
        {
            List<Task> tasks = new List<Task>();

            for (int i = 0; i < 2000000; i++)
            {
                int id = i;
                tasks.Add(Task.Run(async () =>
                {
                    bool enter = await cooldown.TryEnterAsync();

                    if (enter)
                    {
                        Console.WriteLine($"[OK] Request {id} Accept.");
                        await Task.Delay(500); // عملیات سنگین فرضی
                    }
                    //else
                    //{
                    //    Console.WriteLine($"[X] Request {id} Reject.");
                    //}
                }));
            }

            await Task.WhenAll(tasks);
        }
    }

}
