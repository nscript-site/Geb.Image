using System.Drawing;

namespace Geb.Image.Transforms;

public static class ImageTransform
{
    public unsafe static float[] NormalizeToFloatByMeanAndStd(this ImageBgr24 imgSource, bool cvtToRgb, ValueTuple<float,float,float> mean, ValueTuple<float,float,float> std)
    {
        Span<float> meanSpan = stackalloc float[3];
        meanSpan[0] = mean.Item1;
        meanSpan[1] = mean.Item2;
        meanSpan[2] = mean.Item3;
        Span<float> scaleSpan = stackalloc float[3];
        scaleSpan[0] = 1.0f / std.Item1;
        scaleSpan[1] = 1.0f / std.Item2;
        scaleSpan[2] = 1.0f / std.Item3;

        float[] buff = new float[imgSource.Width * imgSource.Height * 3];
        fixed (float* pBuff = buff)
        {
            NormalizeToFloat(imgSource, cvtToRgb, pBuff, imgSource.Width, imgSource.Height, meanSpan, scaleSpan);
        }

        return buff;
    }

    public unsafe static float[] NormalizeToFloatByMeanAndStd(this ImageBgr24[] imgSources, bool cvtToRgb, ValueTuple<float, float, float> mean, ValueTuple<float, float, float> std)
    {
        Span<float> meanSpan = stackalloc float[3];
        meanSpan[0] = mean.Item1;
        meanSpan[1] = mean.Item2;
        meanSpan[2] = mean.Item3;
        Span<float> scaleSpan = stackalloc float[3];
        scaleSpan[0] = 1.0f / std.Item1;
        scaleSpan[1] = 1.0f / std.Item2;
        scaleSpan[2] = 1.0f / std.Item3;

        var imgSource = imgSources[0];
        float[] buff = new float[imgSource.Width * imgSource.Height * 3 * imgSources.Length];
        fixed (float* pBuff = buff)
        {
            float* pData = pBuff;
            for (int i = 0; i < imgSources.Length; i++)
            {
                imgSource = imgSources[i];
                NormalizeToFloat(imgSource, cvtToRgb, pData, imgSource.Width, imgSource.Height, meanSpan, scaleSpan);
                pData += imgSource.Width * imgSource.Height * 3;
            }
        }
        return buff;
    }

    public unsafe static float[] NormalizeToFloatByMeanAndStd(this ImageBgr24[] imgSources, bool cvtToRgb, 
        ValueTuple<float, float, float> mean, ValueTuple<float, float, float> std, 
        ValueTuple<int, int, int> paddingRights, ValueTuple<int, int, int> paddingBottoms)
    {
        Span<float> meanSpan = stackalloc float[3];
        meanSpan[0] = mean.Item1;
        meanSpan[1] = mean.Item2;
        meanSpan[2] = mean.Item3;
        Span<float> scaleSpan = stackalloc float[3];
        scaleSpan[0] = 1.0f / std.Item1;
        scaleSpan[1] = 1.0f / std.Item2;
        scaleSpan[2] = 1.0f / std.Item3;
        Span<int> paddingRightsSpan = stackalloc int[3];
        paddingRightsSpan[0] = paddingRights.Item1;
        paddingRightsSpan[1] = paddingRights.Item2;
        paddingRightsSpan[2] = paddingRights.Item3;
        Span<int> paddingBottomsSpan = stackalloc int[3];
        paddingBottomsSpan[0] = paddingBottoms.Item1;
        paddingBottomsSpan[1] = paddingBottoms.Item2;
        paddingBottomsSpan[2] = paddingBottoms.Item3;
        return NormalizeToFloatArray(imgSources, cvtToRgb, meanSpan, scaleSpan, paddingRightsSpan, paddingBottomsSpan);
    }

    internal static unsafe void NormalizeToFloat(ImageBgr24 imgSource, bool cvtToRgb, float* pData, int width, int height, Span<float> means, Span<float> scales)
    {
        float* pChannel0 = pData;
        float* pChannel1 = pChannel0 + width * height;
        float* pChannel2 = pChannel1 + width * height;

        Bgr24* pSrc = imgSource.Start;

        if(cvtToRgb == false)
        {
            for (int h = 0; h < imgSource.Height; h++)
            {
                for (int w = 0; w < imgSource.Width; w++)
                {
                    Bgr24 c = pSrc[w];
                    // val = (val - mean) * scale;
                    pChannel0[w] = (c.Blue - means[0]) * scales[0];
                    pChannel1[w] = (c.Green - means[1]) * scales[1];
                    pChannel2[w] = (c.Red - means[2]) * scales[2];
                }

                pSrc += imgSource.Width;
                pChannel0 += width;
                pChannel1 += width;
                pChannel2 += width;
            }
        }
        else
        {
            for (int h = 0; h < imgSource.Height; h++)
            {
                for (int w = 0; w < imgSource.Width; w++)
                {
                    Bgr24 c = pSrc[w];
                    // val = (val - mean) * scale;
                    pChannel0[w] = (c.Red - means[2]) * scales[2];
                    pChannel1[w] = (c.Green - means[1]) * scales[1];
                    pChannel2[w] = (c.Blue - means[0]) * scales[0];
                }

                pSrc += imgSource.Width;
                pChannel0 += width;
                pChannel1 += width;
                pChannel2 += width;
            }
        }
    }

    internal static unsafe float[] NormalizeToFloatArray(ImageBgr24 imgSource, bool cvtToRgb, Span<float> means, Span<float> scales)
    {
        float[] buff = new float[imgSource.Width * imgSource.Height * 3];
        fixed (float* pBuff = buff)
        {
            NormalizeToFloat(imgSource, cvtToRgb, pBuff, imgSource.Width, imgSource.Height, means, scales);
        }
        return buff;
    }

    internal static unsafe float[] NormalizeToFloatArray(ImageBgr24[] imgSources, bool cvtToRgb, Span<float> means, Span<float> scales)
    {
        if (imgSources == null || imgSources.Length == 0) return new float[] { };
        var imgSource = imgSources[0];
        float[] buff = new float[imgSource.Width * imgSource.Height * 3 * imgSources.Length];
        fixed (float* pBuff = buff)
        {
            float* pData = pBuff;
            for (int i = 0; i < imgSources.Length; i++)
            {
                imgSource = imgSources[i];
                NormalizeToFloat(imgSource, cvtToRgb, pData, imgSource.Width, imgSource.Height, means, scales);
                pData += imgSource.Width * imgSource.Height * 3;
            }
        }
        return buff;
    }

    internal static unsafe float[] NormalizeToFloatArray(ImageBgr24[] imgSources, bool cvtToRgb, Span<float> means, Span<float> scales, Span<int> paddingRights, Span<int> paddingBottoms)
    {
        if (imgSources == null || imgSources.Length == 0) return new float[] { };
        var imgSource = imgSources[0];
        float[] buff = new float[imgSource.Width * imgSource.Height * 3 * imgSources.Length];
        fixed (float* pBuff = buff)
        {
            int paddingRight = 0;
            int paddingBottom = 0;

            float* pData = pBuff;
            for (int i = 0; i < imgSources.Length; i++)
            {
                paddingRight = paddingRights[i];
                paddingBottom = paddingBottoms[i];

                imgSource = imgSources[i];
                NormalizeToFloat(imgSource, cvtToRgb, pData, imgSource.Width, imgSource.Height, means, scales);

                if (paddingRight > 0 && paddingRight <= imgSource.Width)
                {
                    for (int c = 0; c < 3; c++)
                    {
                        var span = new Span<float>(pData + imgSource.Width * imgSource.Height * c, imgSource.Width * imgSource.Height);
                        for (int h = 0; h < imgSource.Height; h++)
                        {
                            var spanLine = span.Slice(h * imgSource.Width, imgSource.Width);
                            var spanFill = span.Slice(h * imgSource.Width + imgSource.Width - paddingRight, paddingRight
                                );
                            spanFill.Fill(0);
                        }
                    }
                }

                if (paddingBottom > 0)
                {
                    for (int c = 0; c < 3; c++)
                    {
                        var span = new Span<float>(pData + imgSource.Width * imgSource.Height * c, imgSource.Width * imgSource.Height);

                        for (int h = imgSource.Height - paddingBottom; h < imgSource.Height; h++)
                        {
                            var spanLine = span.Slice(h * imgSource.Width, imgSource.Width);
                            spanLine.Fill(0);
                        }
                    }
                }

                pData += imgSource.Width * imgSource.Height * 3;
            }
        }
        return buff;
    }
}
