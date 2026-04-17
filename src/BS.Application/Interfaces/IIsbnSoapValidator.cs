namespace BS.Application.Interfaces
{
    public interface IIsbnSoapValidator
    {
        Task<bool> IsValid(string isbn, CancellationToken cancellationToken = default);
    }
}
