using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geb.Image
{
    /// <summary>
    /// 带旋转角度的矩阵，类似 OpenCV 里面的 RotatedRect
    /// </summary>
    public struct RotatedRectF
    {
        public PointF Center;
        public SizeF Size;

        /// <summary>
        /// 角度值，单位是度数。
        /// </summary>
        public float Angle;

        public RotatedRectF(PointF center, SizeF size, float angle)
        {
            this.Center = center;
            this.Size = size;
            this.Angle = angle;
        }

        /// <summary>
        /// 返回矩阵的四个顶点
        /// </summary>
        /// <returns></returns>
        public readonly PointF[] Points()
        {
            double radius = (double)Angle * Math.PI / 180.0;
            float cos = (float)Math.Cos(radius) * 0.5f;
            float sin = (float)Math.Sin(radius) * 0.5f;
            PointF[] array = new PointF[4];
            array[3].X = Center.X - sin * Size.Height - cos * Size.Width;
            array[3].Y = Center.Y + cos * Size.Height - sin * Size.Width;
            array[0].X = Center.X + sin * Size.Height - cos * Size.Width;
            array[0].Y = Center.Y - cos * Size.Height - sin * Size.Width;
            array[1].X = 2f * Center.X - array[3].X;
            array[1].Y = 2f * Center.Y - array[3].Y;
            array[2].X = 2f * Center.X - array[0].X;
            array[2].Y = 2f * Center.Y - array[0].Y;
            return array;
        }

        public readonly Rect BoundingRect()
        {
            PointF[] array = Points();
            Rect rect = default(Rect);
            rect.X = (int)Math.Floor(Math.Min(Math.Min(Math.Min(array[0].X, array[1].X), array[2].X), array[3].X));
            rect.Y = (int)Math.Floor(Math.Min(Math.Min(Math.Min(array[0].Y, array[1].Y), array[2].Y), array[3].Y));
            rect.Width = (int)Math.Ceiling(Math.Max(Math.Max(Math.Max(array[0].X, array[1].X), array[2].X), array[3].X));
            rect.Height = (int)Math.Ceiling(Math.Max(Math.Max(Math.Max(array[0].Y, array[1].Y), array[2].Y), array[3].Y));
            Rect result = rect;
            result.Width -= result.X - 1;
            result.Height -= result.Y - 1;
            return result;
        }
    }
}
