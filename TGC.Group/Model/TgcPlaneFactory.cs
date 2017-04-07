namespace TGC.Group.Model
{
    using TGC.Core.Geometry;
    using TGC.Group.Interfaces;

    /// <summary>
    /// Implementacion de fabrica de <see cref="TgcPlane"/>
    /// </summary>
    public class TgcPlaneFactory : ITgcPlaneFactory
    {
        /// <summary>
        /// Crea un objeto <see cref="TgcPlane"/>
        /// </summary>
        /// <returns>Un objeto <see cref="TgcPlane"/></returns>
        public TgcPlane CreateTgcPlane()
        {
            return new TgcPlane();
        }
    }
}