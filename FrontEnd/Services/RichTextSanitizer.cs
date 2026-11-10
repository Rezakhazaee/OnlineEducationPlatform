using Ganss.Xss;

namespace FrontEnd.Services;

public static class RichTextSanitizer
{
    public static string? Sanitize(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return html;

        var sanitizer = new HtmlSanitizer();

        sanitizer.AllowedTags.Clear();
        foreach (var tag in new[]
        {
            "p", "br", "div", "span", "strong", "b", "em", "i", "u",
            "s", "h1", "h2", "h3", "h4", "blockquote", "ul", "ol",
            "li", "a", "img", "table", "thead", "tbody", "tr", "th",
            "td", "hr", "sub", "sup"
        })
            sanitizer.AllowedTags.Add(tag);

        sanitizer.AllowedAttributes.Clear();
        foreach (var attribute in new[]
        {
            "style", "href", "src", "alt", "title", "width", "height",
            "colspan", "rowspan", "scope", "align", "valign"
        })
            sanitizer.AllowedAttributes.Add(attribute);

        sanitizer.AllowedCssProperties.Clear();
        foreach (var property in new[]
        {
            "color", "background-color",
            "font-family", "font-size", "font-weight", "font-style",
            "text-decoration", "text-align", "direction", "line-height",
            "text-indent", "vertical-align", "white-space",
            "width", "min-width", "max-width", "height",
            "border", "border-top", "border-right", "border-bottom", "border-left",
            "border-color", "border-style", "border-width",
            "border-collapse", "border-spacing",
            "padding", "padding-top", "padding-right", "padding-bottom", "padding-left",
            "margin", "margin-top", "margin-right", "margin-bottom", "margin-left",
            "letter-spacing", "list-style-type"
        })
            sanitizer.AllowedCssProperties.Add(property);

        sanitizer.AllowedSchemes.Clear();
        sanitizer.AllowedSchemes.Add("http");
        sanitizer.AllowedSchemes.Add("https");
        sanitizer.AllowedSchemes.Add("mailto");

        return sanitizer.Sanitize(html);
    }
}