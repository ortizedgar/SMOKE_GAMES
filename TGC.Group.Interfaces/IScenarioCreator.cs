namespace TGC.Group.Interfaces
{
    using System.Collections.Generic;
    using Autofac;
    using BulletSharp;

    /// <summary>
    /// Creador del escenario
    /// </summary>
    public interface IScenarioCreator
    {
        /// <summary>
        /// Crea el escenario
        /// </summary>
        /// <param name="container">Container IOC</param>
        /// <param name="mediaDir">Directorio de medios</param>
        /// <param name="dynamicsWorld">Mundo fisico</param>
        /// <returns>Una lista con todos los elementos del escenario</returns>
        void CreateScenario(IContainer container, string mediaDir, DiscreteDynamicsWorld dynamicsWorld);

        List<IPortal> PortalUnionList { get; set; }

        List<IScenarioLayer> ScenarioLayers { get; set; }

    }
}