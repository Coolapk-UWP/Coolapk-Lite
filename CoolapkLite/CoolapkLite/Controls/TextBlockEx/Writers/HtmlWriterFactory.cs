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
                        case "bdi":
                            return new BdiWriter();
                        case "bdo":
                            return new BdoWriter();
                        case "button":
                            return new ButtonWriter();
                        case "blockquote":
                            return new BlockQuoteWriter();
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
                        case "h1":
                        case "h2":
                        case "h3":
                        case "h4":
                        case "h5":
                        case "h6":
                            return new HeaderWriter();
                        case "img":
                            return new ImageWriter();
                        case "input":
                            return new InputWriter();
                        case "ins":
                        case "u":
                            return new InsWriter();
                        case "kbd":
                            return new KeyboardWriter();
                        case "p":
                            return new ParagraphWriter();
                        case "pre":
                            return new PreWriter();
                        case "progress":
                        case "meter":
                            return new ProgressWriter();
                        case "q":
                            return new QWriter();
                        case "label":
                        case "span":
                            return new SpanWriter();
                        case "strong":
                        case "b":
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
                        default:
                            return new SpanWriter();
                    }
            }
            return null;
        }
    }
}
