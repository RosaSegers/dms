namespace DocumentFrontend.Models
{
    public interface ITokenCache
    {
        string? AccessToken { get; set; }
        string? RefreshToken { get; set; }
    }

    public class TokenCache : ITokenCache
    {
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
    }

}
