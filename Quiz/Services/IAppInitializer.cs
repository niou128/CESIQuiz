namespace Quiz.Services;

public interface IAppInitializer
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
}
