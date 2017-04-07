namespace TGC.Group.Interfaces
{
    using System;
    using System.Collections.Generic;
    using Autofac;
    using TGC.Core.SceneLoader;

    /// <summary>
    /// Creador del escenario
    /// </summary>
    public interface IScenarioCreator
    {
        /// <summary>
        /// Crea el escenario
        /// </summary>
        /// <param name="mediaDir">Directorio de Media</param>
        /// <param name="container">Container de IOC</param>
        /// <returns>La lista de objetos que componen el escenario</returns>
        List<Tuple<string, List<IRenderObject>>> CreateScenario(string mediaDir, IContainer container);
    }
}