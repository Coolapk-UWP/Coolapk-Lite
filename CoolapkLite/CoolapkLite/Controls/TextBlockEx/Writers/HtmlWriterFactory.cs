using HtmlAgilityPack;

namespace CoolapkLite.Controls.Writers
{
    public class HtmlWriterFactory
    {
        public static HtmlWriter Create(HtmlNode fragment)
        {
            HtmlNode node = fragment;
            switch (node?.NodeType)
            {
                case HtmlNodeType.Text:
                    return new TextWriter();
                case HtmlNodeType.Element:
                    switch (node.OriginalName.ToLowerInvariant())
                    {
                        case "a":
                            return new AnchorWriter();
                        case "br":
                            return new BrWriter();
                        case "em":
                            return new EmWriter();
                        case "img":
                            return new ImageWriter();
                        case "i":
                            return new IWriter();
                        case "p":
                            return new ParagraphWriter();
                        case "span":
                            return new SpanWriter();
                        case "b":
                        case "strong":
                            return new StrongWriter();
                        case "div":
                        case "ul":
                        case "ol":
                        case "dl":
                        case "section":
                        case "article":
                        case "header":
                        case "footer":
                        case "main":
                        case "figure":
                        case "details":
                        case "summary":
                        case "tbody":
                            return new ContainerWriter();
                    }
                    break;
            }
            return null;
        }
    }
}
