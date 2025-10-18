namespace UserRegistrationServer.data
{
    public class RegisterResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public Dictionary<string, string>? Errors { get; set; }
        public User? User { get; set; }
    }
}
