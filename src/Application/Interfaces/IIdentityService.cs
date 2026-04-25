namespace Application.Interfaces
{
    public interface IIdentityService
    {
       public string GenerateToken(string username, string role);
    }
}
