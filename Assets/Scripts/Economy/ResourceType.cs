namespace Reclaim.Economy
{
    /// <summary>
    /// Recursos do alpha. Valores explícitos porque o enum é serializado em assets:
    /// para adicionar um recurso, use um número novo no fim, nunca reaproveite um existente.
    /// </summary>
    public enum ResourceType
    {
        Food = 0,
        Water = 1,
        Wood = 2,
        Scrap = 3,
        Fuel = 4,
        Medicine = 5
    }
}
