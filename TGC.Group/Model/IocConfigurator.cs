namespace TGC.Group.Model
{
    using Autofac;
    using TGC.Core.SceneLoader;
    using TGC.Group.Interfaces;

    public class IocConfigurator
    {
        /// <summary>
        /// Builder de Autofac
        /// </summary>
        private ContainerBuilder Builder { get; set; }

        /// <summary>
        /// Configura y crea un <see cref="IContainer"/>
        /// </summary>
        /// <returns>Un <see cref="IContainer"/> configurado</returns>
        public IContainer BuildAutofacContainer()
        {
            this.Builder = new ContainerBuilder();
            this.Builder.RegisterType<TgcPlaneFactory>().As<ITgcPlaneFactory>().SingleInstance();
            this.Builder.RegisterType<Vector3Factory>().As<IVector3Factory>().SingleInstance();
            this.Builder.RegisterType<TgcFpsCamera>().SingleInstance();
            this.Builder.RegisterType<TgcSceneLoader>().SingleInstance();
            this.Builder.RegisterType<ScenarioCreator>().As<IScenarioCreator>().SingleInstance();
            this.Builder.RegisterType<TgcTextureFactory>().As<ITgcTextureFactory>().SingleInstance();
            return this.Builder.Build();
        }
    }
}
