using System;
using System.Threading;
using System.Threading.Tasks;

namespace TestFramework.Core
{
    public static class Utils
    {
        public static async Task<T> TryThisAsyncWait<T>(Func<string, string, T> func, string arg1, string arg2, int numberOfTries, string failmsg = "")
        {
            while (true)
            {
                try
                {
                    return func(arg1, arg2);
                }
                catch (Exception) when (numberOfTries-- > 0)
                {
                    await Task.Delay(3000);
                }
            }
        }

        public static async Task<T> TryThisAsyncWait<T>(Func<string, T> func, string arg1,  int numberOfTries, string failmsg = "")
        {
            while (true)
            {
                try
                {
                    return func(arg1);
                }
                catch (Exception) when (numberOfTries-- > 0)
                {
                    await Task.Delay(3000);
                }
            }
        }

        public static async void TryThisAsyncWait(Action action, int numberOfTries, string failmsg = "")
        {
            while (true)
            {
                try
                {
                    action();
                    break;
                }
                catch (Exception) when (numberOfTries-- > 0)
                {
                    await Task.Delay(3000);
                }
            }
        }

        public static void TryThis(Action action, int numberOfTries, string failmsg = "")
        {
            while (true)
            {
                try
                {
                    action();
                    break;
                }
                catch (Exception) when (numberOfTries-- > 0)
                {
                    Thread.Sleep(3000);
                }
            }
        }
    }
}
