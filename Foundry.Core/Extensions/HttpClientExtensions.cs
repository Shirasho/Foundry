using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Toolkit.Diagnostics;

namespace Foundry
{
    /// <summary>
    /// Stores the progress of a download.
    /// </summary>
    public readonly struct DownloadProgress
    {
        /// <summary>
        /// The number of bytes currently downloaded.
        /// </summary>
        public readonly long CurrentBytes { get; }

        /// <summary>
        /// The total number of bytes being downloaded.
        /// </summary>
        public readonly long TotalBytes { get; }

        public DownloadProgress(long currentBytes, long totalBytes)
        {
            CurrentBytes = currentBytes;
            TotalBytes = totalBytes;
        }

        public override readonly string ToString()
        {
            return $"{CurrentBytes}/{TotalBytes}";
        }
    }

    public static class HttpClientExtensions
    {
        /// <summary>
        /// Downloads a resource into the provided destination stream.
        /// </summary>
        /// <param name="client">The <see cref="HttpClient"/>.</param>
        /// <param name="requestUri">The URI of the requested resource.</param>
        /// <param name="destination">The stream in which to write the resource bytes.</param>
        /// <param name="progress"></param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="T:System.Net.Http.HttpRequestException">The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout.</exception>
        /// <remarks>https://stackoverflow.com/a/46497896</remarks>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="client"/> is <see langword="null"/></exception>
        /// <exception cref="T:System.ArgumentException"><paramref name="requestUri"/> is null, empty, or whitespace.</exception>
        public static async Task DownloadFileAsync(this HttpClient client, string requestUri, Stream destination, IProgress<DownloadProgress>? progress = null, CancellationToken cancellationToken = default)
        {
            Guard.IsNotNull(client, nameof(client));
            Guard.IsNotNull(requestUri, nameof(requestUri));
            Guard.IsNotNull(destination, nameof(destination));

            using var response = await client.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            long? contentLength = response.Content.Headers.ContentLength;

            using var download = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

            if (progress is null || !contentLength.HasValue)
            {
                await download.CopyToAsync(destination).ConfigureAwait(false);
                return;
            }

            var relativeProgress = new Progress<long>(bytesDownloaded => progress.Report(new DownloadProgress(bytesDownloaded, contentLength.Value)));
            await download.CopyToAsync(destination, 1024 * 32 /* KB */, relativeProgress, cancellationToken).ConfigureAwait(false);
            progress.Report(new DownloadProgress(contentLength.Value, contentLength.Value));
        }

        /// <summary>
        /// Send a GET request to the specified Uri and return the response body as a stream in an asynchronous operation.
        /// </summary>
        /// <exception cref="T:System.Net.Http.HttpRequestException">The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout.</exception>
        public static async Task<Stream> GetStreamAsync(this HttpClient client, string requestUri, CancellationToken cancellationToken = default)
        {
            Guard.IsNotNull(client, nameof(client));
            Guard.IsNotNull(requestUri, nameof(requestUri));

            var response = await client.GetAsync(requestUri, cancellationToken).ConfigureAwait(false);
            return await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Send a GET request to the specified Uri and return the response body as a stream in an asynchronous operation.
        /// </summary>
        /// <exception cref="T:System.Net.Http.HttpRequestException">The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout.</exception>
        public static async Task<Stream> GetStreamAsync(this HttpClient client, Uri requestUri, CancellationToken cancellationToken = default)
        {
            Guard.IsNotNull(client, nameof(client));
            Guard.IsNotNull(requestUri, nameof(requestUri));

            var response = await client.GetAsync(requestUri, cancellationToken).ConfigureAwait(false);
            return await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        }
    }
}
