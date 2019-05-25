using SimpleJSON;

public static class SimpleJsonUtility
{
    public static string RawString(this JSONNode node)
    {
        var len = node.ToString().Length - 2;
        return node.ToString().Substring(1, len);
    }
}