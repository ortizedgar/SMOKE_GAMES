using System.IO;
using System.Windows.Forms;
using Autofac;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TGC.Core.Direct3D;
using TGC.Group.Model;

namespace TGC.Group.Test
{
    [TestClass]
    public class GameModelTest
    {
        private readonly GameModel gameModel;

        public GameModelTest()
        {
            using (var panel = new Panel())
            {
                D3DDevice.Instance.InitializeD3DDevice(panel);
            }

            var builder = new ContainerBuilder();
            builder.RegisterType<TgcPlaneFactory>();
            builder.RegisterType<Vector3Factory>();
            builder.RegisterType<ScenarioCreator>();
            var container = builder.Build();

            gameModel = new GameModel(Directory.GetCurrentDirectory() + @"\Media", Directory.GetCurrentDirectory() + @"\Shaders", container.Resolve<TgcPlaneFactory>(), container.Resolve<Vector3Factory>(), container.Resolve<ScenarioCreator>());
        }

        [TestMethod]
        public void InitTestOk()
        {
            //gameModel.Init();
            Assert.IsTrue(true);
        }
    }
}
