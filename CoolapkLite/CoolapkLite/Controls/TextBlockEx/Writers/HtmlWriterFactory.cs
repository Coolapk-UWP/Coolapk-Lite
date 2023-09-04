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
                        case "abbr":
                        case "acronym":
                            return new AcronymWriter();
                        case "a":
                            return new AnchorWriter();
                        case "br":
                            return new BrWriter();
                        case "code":
                            return new CodeWriter();
                        case "del":
                        case "s":
                        case "strike":
                            return new DelWriter();
                        case "em":
                        case "i":
                        case "dfn":
                        case "var":
                        case "cite":
                        case "address":
                            return new EmWriter();
                        case "img":
                            return new ImageWriter();
                        case "ins":
                        case "u":
                            return new InsWriter();
                        case "kbd":
                            return new KeyboardWriter();
                        case "p":
                            return new ParagraphWriter();
                        case "q":
                            return new QWriter();
                        case "span":
                            return new SpanWriter();
                        case "strong":
                        case "b":
                            return new StrongWriter();
                        case "div":
                            return new ContainerWriter();
                        default:
                            return new SpanWriter();
                    }
            }
            return null;
        }
    }
}
