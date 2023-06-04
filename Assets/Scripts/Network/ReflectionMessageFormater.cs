using System;

public static class ReflectionMessageFormater
{
    #region CONSTANTS
    private const int isReflectionMessageStart = 0;
    private const int clientIdStart = isReflectionMessageStart + sizeof(bool);
    private const int datasAmountStart = clientIdStart + sizeof(int);
    #endregion

    #region PUBLIC_METHODS
    public static bool IsReflectionMessage(byte[] data)
    {
        return BitConverter.ToBoolean(data, isReflectionMessageStart);
    }

    public static int GetClientId(byte[] data)
    {
        return BitConverter.ToInt32(data, clientIdStart);
    }

    public static int GetDatasAmount(byte[] data)
    {
        return BitConverter.ToInt32(data, datasAmountStart);
    }
    #endregion
}
