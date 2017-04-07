namespace TGC.Group.Interfaces
{
    using Microsoft.DirectX;

    /// <summary>
    /// Fabrica de <see cref="Vector3"/>
    /// </summary>
    public interface IVector3Factory
    {
        /// <summary>
        /// Crea un <see cref="Vector3"/>
        /// </summary>
        /// <param name="x">Coordenada X</param>
        /// <param name="y">Coordenada Y</param>
        /// <param name="z">Coordenada Z</param>
        /// <returns>Un objeto <see cref="Vector3"/></returns>
        Vector3 CreateVector3(float x, float y, float z);
    }
}