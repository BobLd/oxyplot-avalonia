using System;
using System.Collections.Generic;
using System.Linq;

namespace OxyPlot.Avalonia.Utilities
{
    // https://stackoverflow.com/a/22713674
    internal static class EllipsePolygonCreator
    {
        #region Public static methods

        public static IEnumerable<ScreenPoint> CreateEllipsePoints(OxyRect rect, double maxAngleErrorRadians)
        {
            return CreateEllipsePoints(maxAngleErrorRadians, rect.Width, rect.Height, rect.Center);
        }

        public static IEnumerable<ScreenPoint> CreateEllipsePoints(double maxAngleErrorRadians, double width, double height, ScreenPoint center)
        {
            IEnumerable<double> thetas = CreateEllipseThetas(maxAngleErrorRadians, width, height);
            return thetas.Select(theta => GetPointOnEllipse(center, theta, width, height));
        }
        #endregion

        #region Private methods

        private static IEnumerable<double> CreateEllipseThetas(double maxAngleErrorRadians, double width, double height)
        {
            double firstQuarterStart = 0;
            double firstQuarterEnd = Math.PI / 2;
            double startPrimeAngle = Math.PI / 2;
            double endPrimeAngle = 0;

            double[] thetasFirstQuarter = RecursiveCreateEllipsePoints(
                firstQuarterStart, firstQuarterEnd, maxAngleErrorRadians,
                width / height, startPrimeAngle, endPrimeAngle).ToArray();

            double[] thetasSecondQuarter = new double[thetasFirstQuarter.Length];
            for (int i = 0; i < thetasFirstQuarter.Length; ++i)
            {
                thetasSecondQuarter[i] = Math.PI - thetasFirstQuarter[thetasFirstQuarter.Length - i - 1];
            }

            IEnumerable<double> thetasFirstHalf = thetasFirstQuarter.Concat(thetasSecondQuarter);
            IEnumerable<double> thetasSecondHalf = thetasFirstHalf.Select(theta => theta + Math.PI);
            return thetasFirstHalf.Concat(thetasSecondHalf);
        }

        private static IEnumerable<double> RecursiveCreateEllipsePoints(double startTheta, double endTheta, double maxAngleError, double widthHeightRatio, double startPrimeAngle, double endPrimeAngle)
        {
            double yDelta = Math.Sin(endTheta) - Math.Sin(startTheta);
            double xDelta = Math.Cos(startTheta) - Math.Cos(endTheta);
            double averageAngle = Math.Atan2(yDelta, xDelta * widthHeightRatio);

            if (Math.Abs(averageAngle - startPrimeAngle) < maxAngleError &&
                Math.Abs(averageAngle - endPrimeAngle) < maxAngleError)
            {
                return new double[] { endTheta };
            }

            double middleTheta = (startTheta + endTheta) / 2.0;
            double middlePrimeAngle = GetPrimeAngle(middleTheta, widthHeightRatio);
            IEnumerable<double> firstPoints = RecursiveCreateEllipsePoints(
              startTheta, middleTheta, maxAngleError, widthHeightRatio, startPrimeAngle, middlePrimeAngle);
            IEnumerable<double> lastPoints = RecursiveCreateEllipsePoints(
              middleTheta, endTheta, maxAngleError, widthHeightRatio, middlePrimeAngle, endPrimeAngle);

            return firstPoints.Concat(lastPoints);
        }

        private static double GetPrimeAngle(double theta, double widthHeightRatio)
        {
            return Math.Atan(1.0 / (Math.Tan(theta) * widthHeightRatio)); // Prime of an ellipse
        }

        private static ScreenPoint GetPointOnEllipse(ScreenPoint center, double theta, double width, double height)
        {
            double x = center.X + width * Math.Cos(theta);
            double y = center.Y + height * Math.Sin(theta);
            return new ScreenPoint(x, y);
        }
        #endregion
    }
}
