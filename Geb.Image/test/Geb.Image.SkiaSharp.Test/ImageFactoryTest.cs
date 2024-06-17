
namespace Geb.Image.Skia.Test
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SkiaSharp;

    [TestClass]
    public class ImageFactoryTest
    {
        [TestMethod]
        public void TestRead()
        {
            var im = ImageIO.ReadBgra32("Image/0001.png");
            var imU8 = ImageIO.ReadU8("Image/0001.png");
            Assert.IsTrue(225 == im.Width);
            Assert.IsTrue(225 == imU8.Width);
            Assert.IsTrue(ImageIO.ReadBgra32("Image/format-Penguins.webp").Width == 600);
            Assert.IsTrue(ImageIO.ReadBgra32("Image/format-Penguins.tif").Width == 600);
            Assert.IsTrue(ImageIO.ReadBgra32("Image/animated-pattern.gif").Width == 500);
        }

        [TestMethod]
        public void TestConvert()
        {
            TestConvert("Image/0001.png");
            TestConvert("Image/0001.jpg");
            TestConvert("Image/0001_png8.png");
            TestConvert("Image/0001_alpha.png");
        }

        private void TestConvert(String filePath)
        {
            var bmp = ImageIO.Read(filePath, SKColorType.Bgra8888);
            var bmp2 = ImageIO.Read(filePath, SKColorType.Gray8);
            var imBgra32 = bmp.ToImageBgra32();
            var imU8 = bmp.ToImageU8();
            var im3 = bmp2.ToImageU8();
            var bmp3 = imBgra32.ToBitmap();
            var bmp4 = im3.ToBitmap();
            var imBgr24 = bmp.ToImageBgr24();

            var sk = bmp.GetPixel(100, 100);
            var sk2 = bmp2.GetPixel(100, 100);
            var sk3 = bmp3.GetPixel(100, 100);
            var sk4 = bmp4.GetPixel(100, 100);
            var c1 = imBgra32[100, 100];
            var c2 = imU8[100, 100];
            var c3 = im3[100, 100];
            var c4 = imBgr24[100, 100];

            Assert.IsTrue(225 == imBgra32.Width);
            Assert.IsTrue(225 == imU8.Width);
            Assert.IsTrue(225 == im3.Width);
            Assert.IsTrue(sk.Alpha == c1.Alpha);
            Assert.IsTrue(sk.Red == c1.Red);
            Assert.IsTrue(sk.Green == c1.Green);
            Assert.IsTrue(sk.Blue == c1.Blue);
            Assert.IsTrue(sk3.Blue == sk.Blue);
            Assert.IsTrue(sk4.Blue == sk2.Blue);
            Assert.IsTrue(c2 == c3);
            Assert.IsTrue(c1.Blue == c4.Blue);
            Assert.IsTrue(c2 == sk2.Red);
        }
    }
}