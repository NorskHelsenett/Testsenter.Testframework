using System.Collections.Generic;
using System.Linq;
using Shared.Common.Testing;

namespace TestFramework.Core
{
    public class TestExecutionSchedule
    {
        private class ExclusionQueue : Queue<Test>
        {
            public bool Busy = false;
        }

        private readonly Dictionary<string, ExclusionQueue> _exclusionQueues;
        private readonly Queue<Test> _freeQueue = new Queue<Test>();
        private readonly object _lock = new object();

        public TestExecutionSchedule(List<Test> tests)
        {
            _exclusionQueues = tests
                .Where(t => t.ExclusionGroup != null)
                .Select(t => t.ExclusionGroup)
                .Distinct()
                .ToDictionary(eg => eg, _ => new ExclusionQueue());

            foreach (var test in tests)
            {
                Enqueue(test, threadSafe: false);
            }
        }

        public void Enqueue(Test test, bool threadSafe = true)
        {
            if (threadSafe)
            {
                lock (_lock)
                {
                    EnqueueNotThreadSafe(test);
                }
            }
            else
            {
                EnqueueNotThreadSafe(test);
            }
        }

        private void EnqueueNotThreadSafe(Test test)
        {
            if (test.ExclusionGroup == null)
            {
                _freeQueue.Enqueue(test);
            }
            else
            {
                _exclusionQueues[test.ExclusionGroup].Enqueue(test);
            }
        }

        public bool TryBeginTestExecution(out Test test)
        {
            lock (_lock)
            {
                var availableExclusionQueue = _exclusionQueues
                    .Values
                    .Where(q => !q.Busy)
                    .OrderByDescending(q => q.Count)
                    .FirstOrDefault(q => q.Count > 0);

                if (availableExclusionQueue != null)
                {
                    test = availableExclusionQueue.Dequeue();
                    availableExclusionQueue.Busy = true;
                }
                else if (_freeQueue.Count > 0)
                {
                    test = _freeQueue.Dequeue();
                }
                else
                {
                    test = null;
                }
            }

            return test != null;
        }

        public void EndTestExcecution(Test test)
        {
            if (test.ExclusionGroup != null)
            {
                _exclusionQueues[test.ExclusionGroup].Busy = false;
            }
        }

        public bool Any()
        {
            return _freeQueue.Count > 0 || _exclusionQueues.Values.Any(q => q.Count > 0);
        }
    }
}