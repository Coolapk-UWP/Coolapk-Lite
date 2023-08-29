using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Documents;

namespace CoolapkLite.Controls.Containers
{
    public abstract class DocumentContainer
    {
        public DocumentContainer Parent { get; private set; }
        public abstract bool CanContain(DependencyObject element);

        protected abstract void Add(DependencyObject element);

        public DocumentContainer Append(DependencyObject element)
        {
            Add(element);
            return Create(element);
        }

        public DocumentContainer Create(DependencyObject element)
        {
            DocumentContainer container = null;
            if (element is Paragraph)
            {
                container = new ParagraphDocumentContainer(element as Paragraph);
            }
            else if (element is Span)
            {
                container = new SpanDocumentContainer(element as Span);
            }

            if (container != null)
            {
                container.Parent = this;
            }

            return container;
        }

        public DocumentContainer FindAscendant(DependencyObject element)
        {
            DocumentContainer container = this;

            while (container != null)
            {
                if (container.CanContain(element))
                {
                    return container;
                }
                container = container.Parent;
            }
            return null;
        }
    }

    public abstract class DocumentContainer<T> : DocumentContainer where T : DependencyObject
    {
        public T Control { get; private set; }

        public DocumentContainer(T element)
        {
            Control = element ?? throw new ArgumentNullException("element");
        }
    }
}
