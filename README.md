NScript 图像库
========

本项目初始为 [GebImage](https://github.com/xiaotie/GebImage), 已迁移为 [NScript.Image](https://github.com/nscript-site/NScript.Image)。

代码中，Geb.Image 为旧代码。NScript.Image 中是基于 .Net7 新特性重写的版本。

## 简介

Geb.Image 是一款为图像分析目的而构建的易用、高性能的 C# 图像库。图像、视频这样的数据占人类数据总量的绝大部分，自 2008 年转型图像和视频开发时起，我就在找寻一个开发语言，能够高效的处理图像和视频数据，同时又具有高开发速度。经过多方比对，最终选择了 C#。C# 是一个成熟的快速开发语言，而打开 unsafe 后，可以直接用指针操作内存数据，能够实现接近于 C 语言的性能。Geb.Image 是我的一个尝试。

本项目有以下特色：

- 高性能：计算密集部分使用指针开发

- 自包含：是纯的 .net 库，不包含第三方 native dll

- 6.* 支持 .net 6 版本，不考虑对之前版本的支持

- 兼容 AOT，方便编译为独立 exe 程序发布

- 提供 csharp 风格的接口来操作图像

项目的设计目标：

- 提供对图像进行快捷操作的基础方法以及一些基本算法。写很多算法时，不需要引用第三方 c++ 库

- 对常见的 cv 深度学习算法，提供对应的图像处理支持，不需要依赖于 OpenCV 等第三方 c++ 库

项目内容

- Geb.Image: 基础图像处理库，不依赖于第三方 cpp 库，支持 AOT 操作
- Geb.Image.Analysis: 基础图像分析算法，不依赖于第三方 cpp 库，支持 AOT 操作
- Geb.Image.Edit: 基础图像编辑库，不依赖于第三方 cpp 库，支持 AOT 操作
- Geb.Image.Skia: 与 SkiaSharp 的桥接库。进行复杂的2D图像处理操作时，可结合该库使用。
- Geb.Image.OpenCV: 与 OpenCVSharp 的桥接库
- Geb.Media.IO: 提供与 FFmpeg 的桥接库，方便处理视频文件。未在本项目里维护。


[项目更新日志](doc/Release.md)

## 图像读写

Geb.Image 内置了基础图像读写IO，支持少量格式的读写，且性能有限。想要读写更多格式的图像，可通过 Geb.Image.Skia 库来进行操作。

### 基础图像读写

可通过 ImageReader.Read 方法读取 png, jpg, bmp 格式的图像文件或字节流，读取的图像为 ImageBgra32 格式。也可通过 ImageBgra32.Read 方法直接读取：

```csharp
var im = ImageReader.Read(@"./img/demo-bmp-24.bmp");
var im2 = ImageBgra32.Read(@"./img/demo-bmp-24.bmp");
```

可通过 ImageBgra32 等类型自带的 SaveBmp, SaveJpeg, SavePng 等方法将图像保存为文件。也可通过 ToJpegData 和 ToPngData 方法，将图像保存为数组。

### 通过 Geb.Image.Skia 库进行读写

可通过 ImageIO 类进行图像读写。ImageIO.Read 方法，直接将文件或流读取为 SKBitmap 文件。ImageIO.ReadBgra32 和 ImageIO.ReadU8 方法，直接将文件或流读取为 ImageBgra32 或 ImageU8 文件。

## 图像操作

待写

## 图像处理

待写

## Blob 分析

待写

## 深度学习相关图像处理方法

### 凸包相关算法

ConvexHull.CreateConvexHull 可以通过旋转卡壳算法创建凸包:

```csharp
public static List<PointF> CreateConvexHull(IList<PointF> pts)
{
    return GrahamScan.ConvexHull(pts);
}
```

ConvexHull.MinAreaRect 可以得到凸包的最小面积外接矩阵:

```csharp
public static unsafe RotatedRectF MinAreaRect(PointF[] points, bool isClockwiseConvexHullVertices = false)
{
    ...
}
```