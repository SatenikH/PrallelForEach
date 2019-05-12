using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ParallelForEach1
{
    public class Program
    {
        static void Main(string[] args)
        {
            string[] files = Directory.GetFiles(@"D:\Pictures", "*.jpg");
            string newDir = @"C:\Users\Public\Pictures\Sample Pictures\Modified";
            Directory.CreateDirectory(newDir);

            Parallel parallel = new Parallel();
            parallel.ForEach(files, (currentFile) =>
            {
                // The more computational work you do here, the greater 
                // the speedup compared to a sequential foreach loop.
                string filename = Path.GetFileName(currentFile);
                var bitmap = new Bitmap(currentFile);

                bitmap.RotateFlip(RotateFlipType.Rotate180FlipNone);
                bitmap.Save(Path.Combine(newDir, filename));

                // Peek behind the scenes to see how work is parallelized.
                // But be aware: Thread contention for the Console slows down parallel loops!!!

                Console.WriteLine($"Processing {filename} on thread {Thread.CurrentThread.ManagedThreadId}");
                //close lambda expression and method invocation

            });

            Console.ReadLine();
        }
    }

    /// <summary>
    /// Provides support for parallel loops.
    /// </summary>
    public class Parallel
    {
        /// <summary>
        /// Implementation parallel.ForEach method with the following signatures
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">anenumerable data source</param>
        /// <param name="body">delegate thatis invoked once per iteration</param>
        public void ForEach<T>(IEnumerable<T> source, Action<T> body)
        {
            if (source == null)
                throw new ArgumentNullException("enumerable");
            if (body == null)
                throw new ArgumentNullException("body");

            var resetEvents = new List<ManualResetEvent>();

            foreach (var item in source)
            {
                var evt = new ManualResetEvent(false);
                ThreadPool.QueueUserWorkItem((i) =>
                {
                    body((T)i);
                    evt.Set();
                }, item);
                resetEvents.Add(evt);
            }

            foreach (var re in resetEvents)
                re.WaitOne();
        }
    }
}

