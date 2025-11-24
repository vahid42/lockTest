using System.Diagnostics;
using System.Threading.Tasks;

namespace lockTest
{
    internal class Program
    {
        private static readonly AccsessLock cooldown = new AccsessLock();
        private static readonly AccsessLock cooldown2 = new AccsessLock();


        static async Task Main(string[] args)
        {
            Console.WriteLine("start.....");
            Console.ReadKey();

            await RunAdvancedStressTest(5000);
            await RunThreadPoolTest(60000);

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

        /// <summary>
        /// تست ThreadPool + CooldownLock
        /// این تست قبل، حین و بعد از اجرای ۱۰۰۰ درخواست:
        /// ThreadPool قبل از تست
        /// ThreadPool در لحظه اجرای همزمان
        /// ThreadPool بعد از تست
        /// </summary>
        /// <param name="requestCount"></param>
        /// <returns></returns>
     public static async Task RunThreadPoolTest(int requestCount = 1000)
        {
            Console.WriteLine("=============== ThreadPool Status Before ===============");
            ThreadPool.GetAvailableThreads(out int workerBefore, out int ioBefore);
            Console.WriteLine($"Worker Threads Available: {workerBefore}");
            Console.WriteLine($"IOCP Threads Available:   {ioBefore}");

            int accepted = 0;
            int rejected = 0;

            List<Task> tasks = new List<Task>();
            Stopwatch sw = Stopwatch.StartNew();

            for (int i = 0; i < requestCount; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    bool enter = await cooldown2.TryEnterAsync();

                    if (enter)
                    {
                        Interlocked.Increment(ref accepted);
                        await Task.Delay(200); // عملیات سنگین
                    }
                    else
                    {
                        Interlocked.Increment(ref rejected);
                    }
                }));
            }

            // بررسی ThreadPool در لحظه فشار کاری
            ThreadPool.GetAvailableThreads(out int workerMid, out int ioMid);

            await Task.WhenAll(tasks);
            sw.Stop();

            Console.WriteLine("\n=============== ThreadPool Status After ===============");
            ThreadPool.GetAvailableThreads(out int workerAfter, out int ioAfter);

            Console.WriteLine($"Total Requests:       {requestCount}");
            Console.WriteLine($"Accepted:             {accepted}");
            Console.WriteLine($"Rejected:             {rejected}");
            Console.WriteLine($"Test Duration:        {sw.ElapsedMilliseconds} ms");

            Console.WriteLine("\n--- ThreadPool Changes ---");
            Console.WriteLine($"Worker Before: {workerBefore}   | Mid: {workerMid}   | After: {workerAfter}");
            Console.WriteLine($"IOCP   Before: {ioBefore}       | Mid: {ioMid}       | After: {ioAfter}");

            Console.WriteLine("=======================================================");


                //نتیجه تست ThreadPool

                //این تست نشان می‌دهد:

                //✔️ درخواست‌های رد شده → تقریباً هیچ ThreadPool مصرف نمی‌کنند

                //چون WaitAsync(0) Non - Blocking است.

                //✔️ فقط یک درخواست سنگین → فقط یک Thread مشغول کار می‌شود
                //✔️ ThreadPool اشباع نمی‌شود

                //چون:

                //هیچ انتظار قفل نداریم

                //هیچ صف Lock طولانی نداریم

                //همه رد می‌شوند و فقط یکی وارد می‌شود


        }
    }
}

