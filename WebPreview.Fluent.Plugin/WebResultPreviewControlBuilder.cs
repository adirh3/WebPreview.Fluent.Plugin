using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Blast.API.Core.UI;
using Blast.Core.Interfaces;

namespace WebPreview.Fluent.Plugin
{
    public class WebResultPreviewControlBuilder : IResultPreviewControlBuilder
    {
        private WebView _webView;

        public WebResultPreviewControlBuilder()
        {
            UiUtilities.UiDispatcher.Post(() =>
            {
                try
                {
                    _webView = new WebView();
                }
                catch (Exception)
                {
                    // ignored
                }
            });
        }

        public PreviewBuilderDescriptor PreviewBuilderDescriptor { get; } = new()
        {
            Name = "Web", Description = "Preview of web pages", ShowPreviewAutomatically = true
        };

        public bool CanBuildPreviewForResult(ISearchResult searchResult)
        {
            return searchResult.Context?.StartsWith("http") ?? false;
        }

        public ValueTask<Control> CreatePreviewControl(ISearchResult searchResult)
        {
            if (_webView == null)
                return ValueTask.FromResult<Control>(null);
            string url = searchResult.Context;
            _webView.Address = url;
            return ValueTask.FromResult<Control>(_webView);
        }
    }
}