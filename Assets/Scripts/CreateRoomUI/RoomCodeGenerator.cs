public static class RoomCodeGenerator
{
    private const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    public static string Generate(int length = 5)
    {
        System.Text.StringBuilder sb = new();
        System.Random rng = new();
        for (int i = 0; i < length; i++)
        {
            sb.Append(chars[rng.Next(chars.Length)]);
        }
        return sb.ToString();
    }
}
