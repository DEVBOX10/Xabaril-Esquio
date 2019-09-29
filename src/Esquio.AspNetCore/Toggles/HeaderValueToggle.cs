﻿using Esquio.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Esquio.AspNetCore.Toggles
{
    [DesignType(Description = "Toggle that is active depending on some header value.")]
    [DesignTypeParameter(ParameterName = HeaderName, ParameterType = EsquioConstants.STRING_PARAMETER_TYPE, ParameterDescription = "The header name to introspect and  check value.")]
    [DesignTypeParameter(ParameterName = HeaderValues, ParameterType = EsquioConstants.SEMICOLON_LIST_PARAMETER_TYPE, ParameterDescription = "The header value to check, multiple items separated by ';'.")]
    public class HeaderValueToggle
        : IToggle
    {
        internal const string HeaderName = nameof(HeaderName);
        internal const string HeaderValues = nameof(HeaderValues);

        private static char[] SPLIT_SEPARATOR = new char[] { ';' };

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRuntimeFeatureStore _featureStore;

        public HeaderValueToggle(IRuntimeFeatureStore store, IHttpContextAccessor httpContextAccessor)
        {
            _featureStore = store ?? throw new ArgumentNullException(nameof(store));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public async Task<bool> IsActiveAsync(string featureName, string productName = null, CancellationToken cancellationToken = default)
        {
            var feature = await _featureStore.FindFeatureAsync(featureName, productName, cancellationToken);
            var toggle = feature.GetToggle(this.GetType().FullName);
            var data = toggle.GetData();

            string headerName = data.HeaderName?.ToString();
            string allowedValues = data.HeaderValues?.ToString();

            if (headerName != null
                &&
                allowedValues != null)
            {
                var values = _httpContextAccessor.HttpContext
                    .Request
                    .Headers[headerName];

                foreach (var item in values)
                {
                    var tokenizer = new StringTokenizer(allowedValues, SPLIT_SEPARATOR);

                    if ( tokenizer.Contains(item, StringSegmentComparer.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
