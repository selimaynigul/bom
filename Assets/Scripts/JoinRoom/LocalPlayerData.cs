using System.Collections.Generic;

public static class LocalPlayerData
{
    // Oyuncunun girdi�i ismi tutar
    public static string PlayerName = "Anon";

    // Kat�lmak istenen room kodu (Join Room ekran�nda girilir)
    public static string RequestedRoomCode = "";

    // Oyuncunun elindeki kelimeler (Client taraf�nda tutulur)
    public static List<string> Hand = new();
}
