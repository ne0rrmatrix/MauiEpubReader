using MauiEpubReader.Interfaces;
using MauiEpubReader.Models;
using MetroLog;
using SQLite;
using SQLiteNetExtensionsAsync.Extensions;

namespace MauiEpubReader.Database;

public partial class Db : Idb
{
	public static string DbPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MyData.db");
	SQLiteAsyncConnection? db;
	
	public const SQLite.SQLiteOpenFlags Flags = SQLite.SQLiteOpenFlags.ReadWrite | SQLite.SQLiteOpenFlags.Create | SQLite.SQLiteOpenFlags.SharedCache;
	static readonly ILogger logger = LoggerFactory.GetLogger(nameof(Db));

	public Db()
	{
	}

	async Task Init(CancellationToken cancellationToken = default)
	{
		if (db is not null)
		{
			return;
		}
		
		db = new SQLiteAsyncConnection(DbPath, Flags);
		logger.Info("Database Connected.");
		await db.CreateTableAsync<Author>().WaitAsync(cancellationToken).ConfigureAwait(false);
		await db.CreateTableAsync<Chapter>().WaitAsync(cancellationToken).ConfigureAwait(false);
		await db.CreateTableAsync<Toc>().WaitAsync(cancellationToken).ConfigureAwait(false);
		await db.CreateTableAsync<Book>().WaitAsync(cancellationToken).ConfigureAwait(false);
		logger.Info("DB Tables Created.");
	}

	public async Task<List<Book>> GetBooks(CancellationToken cancellationToken = default)
	{
		await Init(cancellationToken).ConfigureAwait(false);
		if (db is null)
		{
			return [];
		}
		return await db.GetAllWithChildrenAsync<Book>(null, true, cancellationToken).ConfigureAwait(false);
	}

	public async Task<Book?> GetBook(int id, CancellationToken cancellationToken = default)
	{
		await Init(cancellationToken).ConfigureAwait(false);
		if (db is null)
		{
			return null;
		}
		return await db.GetWithChildrenAsync<Book>(id, true, cancellationToken).ConfigureAwait(false);
	}

	public async Task<int> SaveBook(Book book, CancellationToken cancellationToken = default)
	{
		await Init(cancellationToken).ConfigureAwait(false);
		if (db is null)
		{
			return 0;
		}
		logger.Info("Inserting book");
		await db.InsertOrReplaceWithChildrenAsync(book, recursive: false).WaitAsync(cancellationToken).ConfigureAwait(false);
		logger.Info("Book inserted");
		return 1;
	}

	public async Task<int> RemoveBook(int id, CancellationToken cancellationToken = default)
	{
		await Init(cancellationToken).ConfigureAwait(false);
		if (db is null)
		{
			return 0;
		}
		logger.Info("Deleting book");
		await db.DeleteAsync(id, recursive: true).WaitAsync(cancellationToken).ConfigureAwait(false);
		logger.Info("Book deleted");
		return 1;
	}

	public async Task<int> UpdateBook(Book book, CancellationToken cancellationToken = default)
	{
		await Init(cancellationToken).ConfigureAwait(false);
		if (db is null)
		{
			return 0;
		}
		logger.Info("Updating book");
		await db.UpdateWithChildrenAsync(book).WaitAsync(cancellationToken).ConfigureAwait(false);
		logger.Info("Book updated");
		return 1;
	}
}
