using System.ComponentModel.DataAnnotations.Schema;
using SQLite;
using SQLiteNetExtensions.Attributes;
using ColumnAttribute = SQLite.ColumnAttribute;
using ForeignKeyAttribute = SQLiteNetExtensions.Attributes.ForeignKeyAttribute;
using TableAttribute = SQLite.TableAttribute;

namespace MauiEpubReader.Models;
[TableAttribute("Chapters")]
public class Chapter
{
	[PrimaryKey, AutoIncrement, Column("Id")]
	public int Id { get; set; }
	[ForeignKey(typeof(Book))]
	public int BookId { get; set; }
	public int StartPage { get; set; } = 0;
	public int EndPage { get; set; } = 0;
	public string Title { get; set; } = string.Empty;
	public string Html { get; set; } = string.Empty;
	public string PlainText { get; set; } = string.Empty;
}
