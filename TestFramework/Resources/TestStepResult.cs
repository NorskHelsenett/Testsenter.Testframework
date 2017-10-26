using System;
using Newtonsoft.Json;

namespace TestFramework.Resources
{
    public class TestStepResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public Exception Exception { get; set; }
        public string AddtionalInformation { get; set; }
        public string RefToScreenshot { get; set; }

        public static TestStepResult Successful()
        {
            return new TestStepResult { Success = true };
        }

        public static TestStepResult Failed(string message)
        {
            return new TestStepResult { Success = false, ErrorMessage = message };
        }

        public static TestStepResult ImplementationError(Exception e)
        {
            var errorMessage = $"Feil i implementasjon (unexcepted exception). Exception-message: {e.GetType() } {e.Message} , stacktrace: {e.StackTrace}";
            if (e.InnerException != null)
                errorMessage += $", Innerexception message: {e.InnerException.Message}, stacktrace: {e.InnerException.StackTrace}";

            return new TestStepResult { Success = false, ErrorMessage = errorMessage };
        }

        public void SetException(Exception e)
        {
            var canBeSerialized = ExceptionCanBeSerialized(e);
            if (canBeSerialized)
                Exception = e;
            else
            {
                Exception = new Exception(e.Message);
            }
        }

        private bool ExceptionCanBeSerialized(Exception e)
        {
            try
            {
                var asString = JsonConvert.SerializeObject(e);
                JsonConvert.DeserializeObject<Exception>(asString);
                return true;
            }
            catch(Exception)
            {
                return false;
            }
        }
    }
}
