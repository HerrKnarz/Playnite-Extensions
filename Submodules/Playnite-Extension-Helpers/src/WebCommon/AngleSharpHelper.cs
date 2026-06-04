using AngleSharp.Dom;

namespace PlayniteExtensionHelpers.WebCommon;

public static class AngleSharpHelper
{
    public static String? GetDirectText(this INode node) => node.ChildNodes.OfType<IText>().Select(m => m.Text).FirstOrDefault();
}