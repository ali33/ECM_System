using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Ecm.DocViewer.Controls;

namespace Ecm.DocViewer.Helper
{
    public class RotationToolHelper
    {
        public RotationToolHelper(CanvasElement canvas)
        {
            _canvas = canvas;
        }

        public void Rotate()
        {
            _canvas.PageInfo.RotateAngle = _canvas.PageInfo.RotateAngle % 360;
            var rotateTransform = _canvas.Background.RelativeTransform as RotateTransform;
            if (rotateTransform != null)
            {
                rotateTransform.Angle = _canvas.PageInfo.RotateAngle;
            }

            double tmpWidth = _canvas.Width;
            _canvas.Width = _canvas.Height;
            _canvas.Height = tmpWidth;

            // Update pageinfo
            tmpWidth = _canvas.PageInfo.Width;
            _canvas.PageInfo.Width = _canvas.PageInfo.Height;
            _canvas.PageInfo.Height = tmpWidth;
        }

        public void RotateLeft()
        {
            _canvas.PageInfo.RotateAngle -= 90f;

            List<AnnotationControl> shapes = _canvas.Children.OfType<AnnotationControl>().ToList();
            foreach (AnnotationControl shape in shapes)
            {
                shape.RotateLeft();
                RotateShapeLeft(shape);
            }

            Rotate();
        }

        public void RotateRight()
        {
            _canvas.PageInfo.RotateAngle += 90f;

            List<AnnotationControl> shapes = _canvas.Children.OfType<AnnotationControl>().ToList();
            foreach (AnnotationControl shape in shapes)
            {
                shape.RotateRight();
                RotateShapeRight(shape);
            }

            Rotate();
        }

        private void RotateShape(AnnotationControl shape, Point wantedPoint, double angle)
        {
            Point virtualCoordinate = TranslateVirtualCoordinate(wantedPoint);

            // We must -angle because our clockwise rotate is opposite with clockwise in mathematic
            double x1 = Math.Cos((-angle * Math.PI) / 180) * virtualCoordinate.X
                        - Math.Sin((-angle * Math.PI) / 180) * virtualCoordinate.Y;
            double y1 = Math.Sin((-angle * Math.PI) / 180) * virtualCoordinate.X
                        + Math.Cos((-angle * Math.PI) / 180) * virtualCoordinate.Y;

            Point newTopLeft = TranslateNewCanvasCoordinate(new Point(x1, y1));
            Canvas.SetTop(shape, newTopLeft.Y);
            Canvas.SetLeft(shape, newTopLeft.X);
            double tmpWidth = shape.Width;
            shape.Width = shape.Height;
            shape.Height = tmpWidth;
        }

        private void RotateShapeLeft(AnnotationControl shape)
        {
            var topRight = new Point(Canvas.GetLeft(shape) + shape.Width, Canvas.GetTop(shape));
            RotateShape(shape, topRight, -90);
        }

        private void RotateShapeRight(AnnotationControl shape)
        {
            var leftBottom = new Point(Canvas.GetLeft(shape), Canvas.GetTop(shape) + shape.Height);
            RotateShape(shape, leftBottom, 90);
        }

        private Point TranslateNewCanvasCoordinate(Point virtualCoordinate)
        {
            return new Point(_canvas.Height / 2 + virtualCoordinate.X, _canvas.Width / 2 - virtualCoordinate.Y);
        }

        private Point TranslateVirtualCoordinate(Point canvasCoordinate)
        {
            return new Point(canvasCoordinate.X - _canvas.Width / 2, _canvas.Height / 2 - canvasCoordinate.Y);
        }

        private readonly CanvasElement _canvas;
    }
}