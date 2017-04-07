namespace TGC.Group.Test
{
    using System.IO;
    using System.Windows.Forms;
    using Autofac;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TGC.Core.Direct3D;
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
            builder.RegisterType<TgcPlaneFactory>();
            builder.RegisterType<Vector3Factory>();
            builder.RegisterType<ScenarioCreator>();

            GameModel = new GameModel(Directory.GetCurrentDirectory() + @"\Media", Directory.GetCurrentDirectory() + @"\Shaders", builder.Build());
        }
        private GameModel GameModel { get; set; }

        [TestMethod]
        public void InitTestOk()
        {
            Assert.IsTrue(true);
        }
    }
}
