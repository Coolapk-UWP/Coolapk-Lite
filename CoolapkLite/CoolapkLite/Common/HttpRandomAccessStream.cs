// Ported from https://github.com/kiewic/MediaPlayerElementWithHttpClient/blob/master/MediaPlayerElementWithHttpClient/HttpRandomAccessStream.cs

using CoolapkLite.Helpers;
using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.Web.Http;

namespace CoolapkLite.Common
{
    public class HttpRandomAccessStream : IRandomAccessStreamWithContentType
    {
        private readonly Uri _requestedUri;
        private HttpClient _client;
        private IInputStream _inputStream;
        private ulong _size;
        private string _etagHeader;
        private string _lastModifiedHeader;
        private bool _isDisposing;

        // No public constructor, factory methods instead to handle async tasks.
        private HttpRandomAccessStream(HttpClient client, Uri uri)
        {
            _client = client;
            _requestedUri = uri;
            Position = 0;
        }

        public ulong Position { get; private set; }

        public string ContentType { get; private set; } = string.Empty;

        public bool CanRead => true;

        public bool CanWrite => false;

        public ulong Size
        {
            get => _size;
            set => throw new NotSupportedException();
        }

        public static async Task<HttpRandomAccessStream> CreateAsync(HttpClient client, Uri uri, CancellationToken cancellationToken = default)
        {
            HttpRandomAccessStream randomStream = new HttpRandomAccessStream(client, uri);
            await randomStream.SendRequestAsync(cancellationToken).ConfigureAwait(false);
            return randomStream;
        }

        private async Task SendRequestAsync(CancellationToken cancellationToken = default)
        {
            if (_isDisposing)
            {
                return;
            }

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, _requestedUri);

            request.Headers.Add("Range", $"bytes={Position}-");
            request.Headers.Add("Connection", "Keep-Alive");

            if (!string.IsNullOrEmpty(_etagHeader))
            {
                request.Headers.Add("If-Match", _etagHeader);
            }

            if (!string.IsNullOrEmpty(_lastModifiedHeader))
            {
                request.Headers.Add("If-Unmodified-Since", _lastModifiedHeader);
            }

            if (_client == null)
            {
                return;
            }

            HttpResponseMessage response = await _client.SendRequestAsync(request, HttpCompletionOption.ResponseHeadersRead).AsTask(cancellationToken).ConfigureAwait(false);

            if (response.Content.Headers.ContentType != null)
            {
                ContentType = response.Content.Headers.ContentType.MediaType;
            }

            _size = response.Content.Headers.ContentLength ?? 0;

            if (response.StatusCode != HttpStatusCode.PartialContent && Position != 0)
            {
                throw new System.Net.Http.HttpRequestException("HTTP server did not reply with a '206 Partial Content' status.");
            }

            if (string.IsNullOrEmpty(_etagHeader) && response.Headers.ContainsKey("ETag"))
            {
                _etagHeader = response.Headers["ETag"];
            }

            if (string.IsNullOrEmpty(_lastModifiedHeader) && response.Content.Headers.ContainsKey("Last-Modified"))
            {
                _lastModifiedHeader = response.Content.Headers["Last-Modified"];
            }

            if (response.Content.Headers.ContainsKey("Content-Type"))
            {
                ContentType = response.Content.Headers["Content-Type"];
            }

            _inputStream = await response.Content.ReadAsInputStreamAsync().AsTask(cancellationToken).ConfigureAwait(false);
        }

        public void Seek(ulong position)
        {
            if (Position != position)
            {
                if (_inputStream != null)
                {
                    _inputStream.Dispose();
                    _inputStream = null;
                }

                Position = position;
            }
        }

        public IAsyncOperationWithProgress<IBuffer, uint> ReadAsync(IBuffer buffer, uint count, InputStreamOptions options)
        {
            return AsyncInfo.Run<IBuffer, uint>(async (cancellationToken, progress) =>
            {
                if (_isDisposing)
                {
                    return default;
                }

                progress.Report(0);

                try
                {
                    if (_inputStream == null)
                    {
                        await SendRequestAsync(cancellationToken).ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    SettingsHelper.LogManager.GetLogger(nameof(HttpRandomAccessStream)).Warn(ex.ExceptionToMessage(), ex);
                }

                if (_inputStream != null)
                {
                    IBuffer result = await _inputStream.ReadAsync(buffer, count, options).AsTask(cancellationToken, progress).ConfigureAwait(false);

                    // Move position forward.
                    Position += result.Length;
                    return result;
                }

                return default;
            });
        }

        public IRandomAccessStream CloneStream() => this;

        public IAsyncOperation<bool> FlushAsync() => throw new NotSupportedException();

        public IInputStream GetInputStreamAt(ulong position) => _inputStream;

        public IOutputStream GetOutputStreamAt(ulong position) => throw new NotSupportedException();

        public IAsyncOperationWithProgress<uint, uint> WriteAsync(IBuffer buffer) => throw new NotSupportedException();

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposing)
            {
                return;
            }

            if (disposing)
            {
                _isDisposing = true;
                if (_inputStream != null)
                {
                    _inputStream.Dispose();
                    _inputStream = null;
                }

                if (_client != null)
                {
                    _client.Dispose();
                    _client = null;
                }
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}