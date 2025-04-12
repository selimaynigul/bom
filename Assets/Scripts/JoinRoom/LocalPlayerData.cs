using System.Collections.Generic;

public static class LocalPlayerData
{
    // Oyuncunun girdiði ismi tutar
    public static string PlayerName = "Anon";

    // Katýlmak istenen room kodu (Join Room ekranýnda girilir)
    public static string RequestedRoomCode = "";

    // Oyuncunun elindeki kelimeler (Client tarafýnda tutulur)
    public static List<string> Hand = new();
}
