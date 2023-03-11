# 更新日志

## 20230304

- 移除 Geb.Image 中通过 System.Drawing 与 Bitmap 间相互转换的功能
- 增加 Geb.Image.SkiaSharp 中，SKBitmap 与 ImageU8 和 ImageBrga32 间交互的功能，如果需要强大的图像 IO 功能，可以通过 SkiaSharp 库来实现