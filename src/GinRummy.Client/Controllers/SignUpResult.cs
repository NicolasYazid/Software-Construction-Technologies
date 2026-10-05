namespace GinRummy.Client.Controllers
{
    // Outcome of trying to create an account: either the generated verification code, or the
    // localization key of the message to show and, when the message carries a placeholder,
    // the value that fills it.
    public class SignUpResult
    {
        private SignUpResult(bool succeeded, string errorMessageKey, string generatedCode)
        {
            Succeeded = succeeded;
            ErrorMessageKey = errorMessageKey;
            GeneratedCode = generatedCode;
        }

        public bool Succeeded { get; }
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
