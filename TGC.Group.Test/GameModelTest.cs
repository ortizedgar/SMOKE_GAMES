namespace TGC.Group.Test
{
    using System.IO;
    using System.Windows.Forms;
    using Autofac;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TGC.Core.Direct3D;
    using TGC.Core.SceneLoader;
    using TGC.Group.Interfaces;
    using TGC.Group.Model;

    [TestClass]
    public class GameModelTest
    {

        public GameModelTest()
        {
            using (var panel = new Panel())
            {
                D3DDevice.Instance.InitializeD3DDevice(panel);
            }

            var builder = new ContainerBuilder();
            builder.RegisterType<TgcFpsCamera>().SingleInstance();
            builder.Register(element => new TgcSceneLoader()).SingleInstance();
            builder.Register(element => new TgcPlaneFactory()).As<ITgcPlaneFactory>().SingleInstance();
            builder.Register(element => new Vector3Factory()).As<IVector3Factory>().SingleInstance();
            builder.Register(element => new ScenarioCreator()).As<IScenarioCreator>().SingleInstance();
            builder.Register(element => new TgcTextureFactory()).As<ITgcTextureFactory>().SingleInstance();

            this.GameModel = new GameModel(Directory.GetCurrentDirectory() + @"\Media", Directory.GetCurrentDirectory() + @"\Shaders", builder.Build());
        }

        private GameModel GameModel { get; set; }

        [TestMethod]
        public void InitTestOk() => Assert.IsTrue(true);
    }
}
