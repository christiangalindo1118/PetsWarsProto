/// <summary>
/// Interfaz para cualquier objeto que pueda recibir daño
/// Implementa esto en enemigos, player, objetos destructibles, etc.
/// </summary>
public interface IDamageable
{
    /// <summary>
    /// Aplica daño al objeto
    /// </summary>
    /// <param name="damageAmount">Cantidad de daño a recibir</param>
    void TakeDamage(float damageAmount);

    /// <summary>
    /// Verifica si el objeto está vivo
    /// </summary>
    bool IsAlive();

    /// <summary>
    /// Obtiene la salud actual
    /// </summary>
    float GetCurrentHealth();
}