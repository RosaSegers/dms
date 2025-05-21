namespace AccessControl.Api.Common.Constants
{
    public static class UserConstants
    {
        public static string USERNAME_REQUIRED_STRING = "A username is required";
        public static string USERNAME_INVALID_LENGTH_STRING = $"A username needs to be between {USERNAME_MINIMUM_LENGTH} and {USERNAME_MAXIMUM_LENGTH} characters.";

        public static int USERNAME_MINIMUM_LENGTH = 4;
        public static int USERNAME_MAXIMUM_LENGTH = 50;

        public static string EMAIL_INVALID_STRING = "Supplied email is invalid";
        public static string EMAIL_INVALID_LENGTH_STRING = $"An email needs to be between {EMAIL_MINIMUM_LENGTH} and {EMAIL_MAXIMUM_LENGTH} characters.";


        public static int EMAIL_MINIMUM_LENGTH = 4;
        public static int EMAIL_MAXIMUM_LENGTH = 100;

        public static string PASSWORD_EMPTY_STRING = "A password is required";
        public static string PASSWORD_SHORT_STRING = "Your password length must be at least 12.";
        public static string PASSWORD_CONTAINS_CAPITAL_STRING = "Your password must contain at least one uppercase letter.";
        public static string PASSWORD_CONTAINS_LOWER_STRING = "Your password must contain at least one lowercase letter.";

    }
}
