namespace GinRummy.Client.Controllers
{
    public class SignUpResult
    {
        private SignUpResult(bool isSuccessful, string errorMessageKey, string generatedCode)
        {
            IsSuccessful = isSuccessful;
            ErrorMessageKey = errorMessageKey;
            GeneratedCode = generatedCode;
        }

        public bool IsSuccessful { get; }
        public string ErrorMessageKey { get; }
        public int? ErrorMessageArgument { get; private set; }
        public string GeneratedCode { get; }

        public static SignUpResult Success(string generatedCode)
        {
            return new SignUpResult(true, null, generatedCode);
        }

        public static SignUpResult Failure(string errorMessageKey)
        {
            return new SignUpResult(false, errorMessageKey, null);
        }

        public static SignUpResult Failure(string errorMessageKey, int errorMessageArgument)
        {
            SignUpResult failure = new SignUpResult(false, errorMessageKey, null);
            failure.ErrorMessageArgument = errorMessageArgument;

            return failure;
        }
    }
}
