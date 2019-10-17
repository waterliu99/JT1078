﻿using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using System;

namespace JT1078.Flv.Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            Summary summary = BenchmarkRunner.Run<JT1078FlvEncoderContext>();
        }
    }
}
