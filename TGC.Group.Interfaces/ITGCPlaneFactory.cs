namespace TGC.Group.Interfaces
{
    using TGC.Core.Geometry;

    /// <summary>
    /// Fabrica de <see cref="TgcPlane"/>
    /// </summary>
    public interface ITgcPlaneFactory
    {
        /// <summary>
        /// Crea un <see cref="TgcPlane"/>
        /// </summary>
        /// <returns>Un objeto <see cref="TgcPlane"/></returns>
        TgcPlane CreateTgcPlane();
    }
}