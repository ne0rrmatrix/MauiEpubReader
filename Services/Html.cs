using System.Xml;
using HtmlAgilityPack;

namespace MauiEpubReader.Services;

public static partial class Html
{
	public static string? GetContentAsPlainText(string? html)
	{
		if(html is null)
		{
			return null;
		}
		try
		{
			html = html.Replace("\r", "").Replace("\n", " ");
			HtmlDocument doc = new();
			doc.LoadHtml(html);
			foreach (HtmlNode node in doc.DocumentNode.SafeSelectNodes("//comment() | //script | //style | //head"))
			{
				node.ParentNode.RemoveChild(node);
			}

			foreach (HtmlNode node in doc.DocumentNode.SafeSelectNodes("//span | //label"))
			{
				node.ParentNode.ReplaceChild(HtmlNode.CreateNode(node.InnerHtml), node);
			}

			foreach (HtmlNode node in doc.DocumentNode.SafeSelectNodes("//p | //div"))
			{
				var txtNode = node.SelectSingleNode("text()");
				if (txtNode == null || txtNode.InnerHtml.Trim() == "")
				{
					continue;
				}
				node.ParentNode.InsertBefore(doc.CreateTextNode("\r\n"), node);
				node.ParentNode.InsertAfter(doc.CreateTextNode("\r\n"), node);
			}

			foreach (HtmlNode node in doc.DocumentNode.SafeSelectNodes("//br"))
			{
				node.ParentNode.ReplaceChild(doc.CreateTextNode("\r\n"), node);
			}

			string text = doc.DocumentNode.InnerText.Trim();
			text = text.Replace("&lt;", string.Empty);
			text = text.Replace("&gt;", string.Empty);
			text = text.Replace("&amp;", string.Empty);
			text = text.Replace("&nbsp;", string.Empty);
			return text;
		}
		catch (XmlException)
		{
			System.Diagnostics.Debug.WriteLine("Error parsing HTML");
			return null;
		}
	}
	static HtmlNodeCollection SafeSelectNodes(this HtmlNode node, string selector)
	{
		return node.SelectNodes(selector) ?? new HtmlNodeCollection(node);
	}
}

