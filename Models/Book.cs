using System.ComponentModel.DataAnnotations.Schema;
using SQLite;
using SQLiteNetExtensions.Attributes;
using ColumnAttribute = SQLite.ColumnAttribute;
using TableAttribute = SQLite.TableAttribute;

namespace MauiEpubReader.Models;
[Table("Books")]
public class Book
{
	[PrimaryKey, AutoIncrement, Column("Id")]
	public int Id { get; set; }
	public int NumberOfPages { get; set; } = 0;
	public int NumberOfChapters { get; set; } = 0;
	public string FileName { get; set; } = string.Empty;
	public int CurrentPage { get; set; } = 0;
	public int CurrentChapter { get; set; } = 0;
	public string Title { get; set; } = string.Empty;
	public string CoverImage { get; set; } = string.Empty;
	[OneToMany(CascadeOperations = CascadeOperation.All)]
	public List<Author> Authors { get; set; } = [];
	[OneToMany(CascadeOperations = CascadeOperation.All)]
	public List<Chapter> Chapters { get; set; } = [];
	[OneToMany(CascadeOperations = CascadeOperation.All)]
	public List<Toc> TOC { get; set; } = [];
}
