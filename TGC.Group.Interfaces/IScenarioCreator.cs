namespace TGC.Group.Interfaces
{
    using System;
    using System.Collections.Generic;
    using Autofac;
    using BulletSharp;
    using TGC.Core.SceneLoader;

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
        List<Tuple<string, List<Tuple<IRenderObject, RigidBody>>>> CreateScenario(IContainer container, string mediaDir, DiscreteDynamicsWorld dynamicsWorld);
    }
}