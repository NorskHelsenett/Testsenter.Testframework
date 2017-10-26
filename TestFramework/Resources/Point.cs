using System;
using System.Collections.Generic;

namespace TestFramework.Resources
{
    public class Point
    {
        public int Id { get; set; }
        public int TestdataIteration { get; set; }
        public List<TestStep> TestSteps { get; set; }
        public TestData Data { get; set; }
        public bool RunOk { get; set; }

        public string Environment { get; set; }
        public string ServiceName { get; set; }
        public TimeSpan Duration { get; set; }

        public Point(int pointId, TestData testData, List<TestStep> steps, string environment, string serviceName,int iterationCount)
        {
            Id = pointId;
            Data = testData;
            TestSteps = steps;
            Environment = environment;
            ServiceName = serviceName;
            TestdataIteration = iterationCount;
        }
    }
}
