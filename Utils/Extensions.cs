﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;

namespace EP94.LgSmartThinq.Utils
{
    public static class Extensions
    {
        public static HttpRequestMessage Clone(this HttpRequestMessage request)
        {
            var clone = new HttpRequestMessage(request.Method, request.RequestUri)
            {
                Content = request.Content.Clone(),
                Version = request.Version
            };
            foreach (KeyValuePair<string, object> prop in request.Properties)
            {
                clone.Properties.Add(prop);
            }
            foreach (KeyValuePair<string, IEnumerable<string>> header in request.Headers)
            {
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            return clone;
        }

        public static HttpContent Clone(this HttpContent content)
        {
            if (content == null) return null;

            var ms = new MemoryStream();
            content.CopyToAsync(ms).Wait();
            ms.Position = 0;

            var clone = new StreamContent(ms);
            foreach (KeyValuePair<string, IEnumerable<string>> header in content.Headers)
            {
                clone.Headers.Add(header.Key, header.Value);
            }
            return clone;
        }

        public static int ToUnixTimestamp(this DateTime dateTime)
        {
            return (int)(dateTime.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }
    }
}
