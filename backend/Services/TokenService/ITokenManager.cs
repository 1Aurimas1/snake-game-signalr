public interface ITokenManager
{
    bool IsCurrentActiveToken();
    void DeactivateCurrent();
    bool IsActive(string token);
    void Deactivate(string token);
}
