using System.ComponentModel.DataAnnotations.Schema;
using SQLite;
using SQLiteNetExtensions.Attributes;
using ColumnAttribute = SQLite.ColumnAttribute;
using ForeignKeyAttribute = SQLiteNetExtensions.Attributes.ForeignKeyAttribute;
using TableAttribute = SQLite.TableAttribute;

namespace MauiEpubReader.Models;

[TableAttribute("Toc")]
public class Toc
{
	[PrimaryKey, AutoIncrement, Column("Id")]
	public int Id { get; set; }
	[ForeignKey(typeof(Book))]
	public int BookId { get; set; }
	public string Title { get; set; } = string.Empty;
}
