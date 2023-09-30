using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JMC
{
    [MemoryDiagnoser]
    public class JMCLexerBenchmark
    {
        [Benchmark]
        public void SpanArray()
        {
            var arr = Enumerable.Range(0, 100_000).ToArray().AsSpan();
            for (var i = 0; i < arr.Length; i++)
            {
                ref var item = ref arr[i];
                Task.Delay(1).Wait();
            }
        }

        [Benchmark]
        public void ParrelleArray()
        {
            var arr = Enumerable.Range(0, 100_000).ToArray();
            Parallel.For(0, arr.Count() - 1, (i) =>
            {
                var item = arr[i];
                Task.Delay(1).Wait();
            });
        }
    }
}
