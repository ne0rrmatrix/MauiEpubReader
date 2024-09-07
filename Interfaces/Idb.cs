using MauiEpubReader.Models;

namespace MauiEpubReader.Interfaces;

public interface Idb
{
	public Task<int> SaveBook(Book book, CancellationToken cancellationToken = default);
	public Task<List<Book>> GetBooks(CancellationToken cancellationToken = default);
	public Task<Book?> GetBook(int id, CancellationToken cancellationToken = default);
	public Task<int> RemoveBook(int id, CancellationToken cancellationToken = default);
	public Task<int> UpdateBook(Book book, CancellationToken cancellationToken = default);
}
