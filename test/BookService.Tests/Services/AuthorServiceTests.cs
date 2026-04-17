using BS.Application.Services;
using BS.Domain.Entity;
using BS.Domain.Interfaces;
using Moq;
using Xunit;

namespace BookService.Tests.Services;

/// <summary>
/// Pruebas de <see cref="AuthorService"/> con repositorio simulado (Moq).
/// </summary>
public sealed class AuthorServiceTests
{
    [Fact]
    public async Task GetById_cuando_el_repositorio_devuelve_null_devuelve_respuesta_404()
    {
        var mockRepo = new Mock<IAuthorRepository>();
        mockRepo
            .Setup(r => r.GetById(It.IsAny<Guid>()))
            .ReturnsAsync((Author?)null);

        var sut = new AuthorService(mockRepo.Object);

        var result = await sut.GetById(Guid.NewGuid());

        Assert.False(result.IsSuccessful);
        Assert.Equal(404, result.Status);
        Assert.Equal("Autor no encontrado.", result.Message);
        Assert.Contains("Autor no encontrado.", result.Errors);
        mockRepo.Verify(r => r.GetById(It.IsAny<Guid>()), Times.Once);
    }
}
