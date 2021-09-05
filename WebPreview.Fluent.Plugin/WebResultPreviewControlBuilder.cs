using System;

namespace WebPreview.Fluent.Plugin
{
    public class WebResultPreviewControlBuilder : IResultPreviewControlBuilder
    {
        private readonly WebView _webView;

        public WebResultPreviewControlBuilder()
        {
            _webView = new WebView();
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
            var url = searchResult.Context;
            _webView.Address = url;
            return ValueTask.FromResult<Control>(_webView);
        }
    }
}