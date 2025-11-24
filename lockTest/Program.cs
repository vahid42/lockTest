using System.Diagnostics;
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

            await RunAdvancedStressTest(800000);
            Console.ReadKey();
        }




        public static async Task RunAdvancedStressTest(int requestCount = 1000)
        {
            int accepted = 0;
            int rejected = 0;

            long fastest = long.MaxValue;
            long slowest = 0;
            long totalTime = 0;

            List<Task> tasks = new List<Task>();
            Stopwatch globalSw = Stopwatch.StartNew();

            for (int i = 0; i < requestCount; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    Stopwatch sw = Stopwatch.StartNew();

                    bool enter = await cooldown.TryEnterAsync();

                    sw.Stop();

                    Interlocked.Add(ref totalTime, sw.ElapsedMilliseconds);

                    if (sw.ElapsedMilliseconds < fastest)
                        fastest = sw.ElapsedMilliseconds;

                    if (sw.ElapsedMilliseconds > slowest)
                        slowest = sw.ElapsedMilliseconds;

                    if (enter)
                    {
                        Interlocked.Increment(ref accepted);
                        await Task.Delay(200); // عملیات سنگین فرضی
                    }
                    else
                    {
                        Interlocked.Increment(ref rejected);
                    }
                }));
            }

            await Task.WhenAll(tasks);

            globalSw.Stop();

            Console.WriteLine("=============== Advanced Stress Test ===============");
            Console.WriteLine($"Total Requests:          {requestCount}");
            Console.WriteLine($"Total Time:              {globalSw.ElapsedMilliseconds} ms");
            Console.WriteLine($"Accepted:                {accepted}");
            Console.WriteLine($"Rejected:                {rejected}");
            Console.WriteLine($"Avg Response Time:       {totalTime / requestCount} ms");
            Console.WriteLine($"Fastest Response:        {fastest} ms");
            Console.WriteLine($"Slowest Response:        {slowest} ms");
            Console.WriteLine("====================================================");
        }
    }
}

