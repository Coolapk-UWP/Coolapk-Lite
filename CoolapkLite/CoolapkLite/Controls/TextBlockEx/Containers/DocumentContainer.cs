using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
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
            if (element is Span span)
            {
                container = new SpanDocumentContainer(span);
            }
            else if (element is Paragraph paragraph)
            {
                container = new ParagraphDocumentContainer(paragraph);
            }
            else if (element is StackPanel stackPanel)
            {
                container = new StackPanelDocumentContainer(stackPanel);
            }
            else if (element is ContentControl contentControl)
            {
                container = new ContentControlDocumentContainer(contentControl);
            }
            else if (element is RichTextBlock richTextBlock)
            {
                container = new RichTextBlockDocumentContainer(richTextBlock);
            }

            if (container != null)
            {
                container.Parent = this;
            }

            return container;
        }

        public DocumentContainer<T> FindAscendant<T>() where T : DependencyObject
        {
            DocumentContainer container = this;

            while (container != null)
            {
                if (container is DocumentContainer<T> result)
                {
                    return result;
                }
                container = container.Parent;
            }
            return null;
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

        protected static Binding CreateBinding(object source, string path, BindingMode mode = BindingMode.OneWay)
        {
            return new Binding
            {
                Path = new PropertyPath(path),
                Source = source,
                Mode = mode
            };
        }

        protected static void SetBinding(DependencyObject source, DependencyProperty property, Action applyChange)
        {
            if (source != null)
            {
                applyChange();

                source.RegisterPropertyChangedCallback(property, (sender, dp) =>
                {
                    applyChange();
                });
            }
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
