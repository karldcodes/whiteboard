namespace Whiteboard.Domain;

/*
* This result type lets the clients know if their changes were succesful 
* or if there was any conflicts due to stale requests etc
*/
public sealed record ApplyResult(
    bool Success,
    long? CurrentVersion = null,
    string? Error = null)
{
    public static ApplyResult Succeeded(long? newVersion) =>
        new(true, newVersion);

    public static ApplyResult Conflict(long? currentVersion, string message) =>
        new(false, currentVersion, message);

    public static ApplyResult NotFound() =>
        new(false, null, "Post-it was not found.");
}