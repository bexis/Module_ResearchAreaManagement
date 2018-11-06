using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BExIS.Pmm.Model
{
    public class CalcWithWgs84
    {
        public static void ComputePointWithAzimuth(double lon, double lat, double dist, int azimuth)
        {

        }

        public static double ComputeLat(double lat, double dist)
        {
            double endLat = 0.0;
            double Dnord = dist;
            double phi = Dnord / 1850;
            double DiffNord = GetDezimalMinute(lat);
            DiffNord = DiffNord + phi;

            if (DiffNord / 60 <= 1)
            {
                //   endLat = Math.Truncate(lat) + GetDezimalGrad(DiffNord);

                Double xx = GetDezimalGrad(DiffNord);
                endLat = Math.Truncate(lat);
                endLat = endLat + xx;
            }
            else
            {
                if (phi > 0)
                {
                    //  endLat = Math.Truncate(lat) + GetDezimalGrad(DiffNord);

                    Double xx = GetDezimalGrad(DiffNord);
                    endLat = Math.Truncate(lat);
                    endLat = endLat + xx;
                }
                else
                {
                    // endLat = Math.Truncate(lat) - GetDezimalGrad(DiffNord);

                    Double xx = GetDezimalGrad(DiffNord);
                    endLat = Math.Truncate(lat);
                    endLat = endLat - xx;
                }
            }

            return endLat;
        }

        public static double ComputeLon(double lon, double lat, double dist, bool useLat = true)
        {
            double endLon = 0.0;
            useLat = true;
            double Dost = dist;
            double lambda = Dost / (1850 * 1);
            if(useLat)
                lambda = Dost / (1850 * Math.Cos(DegreesToRad(lat)));
            double DiffOst = GetDezimalMinute(lon) + lambda;
            if (DiffOst / 60 <= 1)
            {
                endLon = Math.Truncate(lon) + GetDezimalGrad(DiffOst);
            }
            else
            {
                if (lambda > 0)
                {
                    endLon = Math.Truncate(lon) + GetDezimalGrad(DiffOst);
                }
                else
                {
                    endLon = Math.Truncate(lon) - GetDezimalGrad(DiffOst);
                }
            }
            return endLon;
        }



        private static double[] ComputePoint(double lon, double lat, double dist, int angle)
        {

            double endLon, endLat = 0.0;

            double Dnord = Math.Cos(angle) * dist;
            double Dost = Math.Sin(angle) * dist;

            double phi = Dnord / 1850;
            double lambda = Dost / 1850 * Math.Cos(lat);

            double DiffNord = GetDezimalMinute(lon) + phi;
            double DiffOst = GetDezimalMinute(lat) + lambda;


            if (DiffNord / 60 <= 1)
            {
                endLon = Math.Truncate(lon) + GetDezimalGrad(DiffNord);
            }
            else
            {
                if (phi > 0)
                {
                    endLon = Math.Truncate(lon) + GetDezimalGrad(DiffNord);
                }
                else
                {
                    endLon = Math.Truncate(lon) - GetDezimalGrad(DiffNord);
                }
            }

            if (DiffOst / 60 <= 1)
            {
                endLat = Math.Truncate(lat) + GetDezimalGrad(DiffOst);
            }
            else
            {
                if (phi > 0)
                {
                    endLat = Math.Truncate(lat) + GetDezimalGrad(DiffOst);
                }
                else
                {
                    endLat = Math.Truncate(lat) - GetDezimalGrad(DiffOst);
                }
            }

            return new double[] { endLon, endLat };

        }

        // input is dezimalgrad (format e.g. 50,8472)
        // returns dezimalminutes (format e.g. 50° 50,832') but just the decimal place = nachkomma (50,832)
        private static double GetDezimalMinute(double dezimalGrad)
        {
            return (dezimalGrad - Math.Truncate(dezimalGrad)) * 60;
        }

        // input is dezimalminutes (format e.g. 50° 50,832') but just the decimal place = nachkomma (50,832)
        // can also be greater than 60' or 
        // returns dezimalgrad (format e.g. 50,8472) but just dezimal place = nachkomma (0,8472)
        private static double GetDezimalGrad(double dezimalMinute)
        {
            return dezimalMinute / 60;
        }

        private static double DegreesToRad(double degrees)
        {
            return degrees * (Math.PI / 180.0);
        }
    }
}