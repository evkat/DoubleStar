using System;

namespace DoubleStar
{
    class Program
    {
        static void Main(string[] args)
        {
            DoubleStar ds = new DoubleStar(4.5, 3.4, 3.9, 5.3);
            ds.Solve();
        }
    }

    public class DoubleStar
    {
        // doba pozorovania [rok]
        const double partOrbitalPeriodInY = 11;
        // hmotnost Slnka [kg]
        static readonly double massSun = 1.9891 * Math.Pow(10, 30);
        // ziarivy vykon Slnka [W]
        static readonly double luminositySun = 3.83 * Math.Pow(10, 26);
        // gravitacna konstanta [m^3*kg*s^-2]
        static readonly double kappa = 6.67408 * Math.Pow(10, -11);
        // absolutna magnituda Slnka
        double absMagSun = 4.8;
        // 1/3 ako double
        const double third = (double)1 / 3;


        // rozmer velkej poloosi v radianoch
        double aInRad;
        // rozmer malej poloosi v radianoch
        double bInRad;
        // relativna magnituda hviezdy 1
        double relativeMag1;
        // relativna magnituda hviezdy 2
        double relativeMag2;

        /// <summary>
        /// obezna doba sustavy v rokoch
        /// </summary>
        public double OrbitalPeriodInY { get; private set; }
        /// <summary>
        /// vzdialenost sustavy
        /// </summary>
        public double Distance { get; private set; }
        /// <summary>
        /// absolutna magnituda hviezdy 1
        /// </summary>
        public double AbsoluteMagnitude1 { get; private set; }
        /// <summary>
        /// absolutna magnituda hviezdy 2
        /// </summary>
        public double AbsoluteMagnitude2 { get; private set; }
        /// <summary>
        /// hmotnost hviezdy 1
        /// </summary>
        public double Mass1 { get; private set; }
        /// <summary>
        /// hmotnost hviezdy 2
        /// </summary>
        public double Mass2 { get; private set; }



        /// <summary>
        /// konstruktor
        /// </summary>
        /// <param name="_aInArcSec">uhlova velkost hlavnej poloosi v uhl sekundach</param>
        /// <param name="_bInArcSec">uhlova velkost vedlajsej poloosi v uhl sekundach</param>
        /// <param name="_relativeMag1">relativna magnituda 1 hviezdy</param>
        /// <param name="_relativeMag2">relativna magnituda 2 hviezdy</param>
        public DoubleStar(double _aInArcSec, double _bInArcSec, double _relativeMag1, double _relativeMag2)
        {
            // prepocet sekund na uhly a naslednena radiany
            aInRad = ConvertDegreeToRadian(_aInArcSec / 3600);
            bInRad = ConvertDegreeToRadian(_bInArcSec / 3600);
            relativeMag1 = _relativeMag1;
            relativeMag2 = _relativeMag2;
        }
        public void Solve()
        {
            OrbitalPeriodInY = CalcOrbitalPeriod();
            var result = CalcMassMagDist();
            Distance = result.Item1;
            AbsoluteMagnitude1 = result.Item2;
            AbsoluteMagnitude2 = result.Item3;
            Mass1 = result.Item4;
            Mass2 = result.Item5;

            Console.WriteLine($"******** VYSLEDKY ********");
            Console.WriteLine($"Obezna doba sustavy {OrbitalPeriodInY.ToString("F")} rokov");
            Console.WriteLine($"Vzdialenost sustavy je {Distance.ToString("F")} pc.");
            Console.WriteLine($"Hviezda 1: hmotnost {Mass1.ToString("G3")} kg, absolutna magnituda {AbsoluteMagnitude1.ToString("F")}.");
            Console.WriteLine($"Hviezda 2: hmotnost {Mass2.ToString("G3")} kg, absolutna magnituda {AbsoluteMagnitude2.ToString("F")}.");

            Console.ReadKey();
        }

        /// <summary>
        /// metoda pre vypocet obeznej doby sustavy
        /// </summary>
        /// <returns></returns>
        private double CalcOrbitalPeriod()
        {
            double hInRad = Math.Sqrt(aInRad * aInRad - bInRad * bInRad);
            // obsah usece
            double epsArea = aInRad * aInRad * (Math.Acos(hInRad / aInRad) - hInRad * Math.Sqrt(aInRad * aInRad - hInRad * hInRad) / (aInRad * aInRad));
            //vypocet obsahu celej elipsy
            double elipseArea = Math.PI * aInRad * aInRad;

            double areaRatio = elipseArea / epsArea;
            Console.WriteLine($"Pomer obsahu usece a elisy {areaRatio.ToString("F")}");
            // vypocet doby obehu celej sustavy - 2 KZ
            double orbitalPeriod = partOrbitalPeriodInY * elipseArea / epsArea;
            Console.WriteLine($"Doba obehu sustavy {Math.Round(orbitalPeriod, 2)} rokov.");
            return orbitalPeriod;
        }

        /// <summary>
        /// metoda pre vypocet hmotnosti hviezd, ich absolutnych magnitud a vzialenosti sustavy
        /// </summary>
        /// <returns></returns>
        private Tuple<double, double, double, double, double> CalcMassMagDist()
        {
            // prepocet doby obehu sustavy na sekundy
            double orbitalPeriodInSeconds = OrbitalPeriodInY * 365.25 * 24 * 60 * 60;

            double dist = 0; // v  pc
            double absMag1 = 0;
            double absMag2 = 0;
            double mass1 = massSun;
            double mass2 = massSun;
            bool moveToNext = false;
            int count = 0;
            do
            {
                count++;
                // prepocet vzdialenosti vzdialenosti
                // 3KZ - hlavna poloosa
                // rozdelene na casti kvoli preteceniu
                double aInMeters = Math.Pow(orbitalPeriodInSeconds * orbitalPeriodInSeconds, third) * Math.Pow(kappa * (mass1 + mass2) / (4 * Math.PI * Math.PI), third);
                double aInPc = aInMeters * 3.24077929 * Math.Pow(10, -17);
                double newDistInPc = aInPc / Math.Tan(aInRad);
                // vypocet absolutnej magnitudy - z pogsona
                double newAbsMag1 = CalculateAbsoluteMagnitude(relativeMag1, newDistInPc);
                double newAbsMag2 = CalculateAbsoluteMagnitude(relativeMag2, newDistInPc);
                // vypocet ziariveho vykonu
                double luminosity1 = CalculateLuminosity(newAbsMag1);
                double luminosity2 = CalculateLuminosity(newAbsMag2);
                // vypocet hmotnosti
                double newMass1 = CalculateMass(luminosity1);
                double newMass2 = CalculateMass(luminosity2);


                Console.WriteLine($"*** Iteracia {count} ***");
                Console.WriteLine($"Vzdialenost sustavy je {newDistInPc} pc.");
                Console.WriteLine($"Hviezda 1: hmotnost {newMass1} kg, absolutna magnituda {newAbsMag1}.");
                Console.WriteLine($"Hviezda 2: hmotnost {newMass2} kg, absolutna magnituda {newAbsMag2}.");

                if ((PercentageDifferenceTooBig(dist, newDistInPc)
                    || PercentageDifferenceTooBig(absMag1, newAbsMag1)
                    || PercentageDifferenceTooBig(absMag2, newAbsMag2)
                    || PercentageDifferenceTooBig(mass1, newMass1)
                    || PercentageDifferenceTooBig(mass2, newMass2)))
                {
                    moveToNext = true;
                }
                else
                {
                    moveToNext = false;
                }

                dist = newDistInPc;
                absMag1 = newAbsMag1;
                absMag2 = newAbsMag2;
                mass1 = newMass1;
                mass2 = newMass2;
            } while (moveToNext);

            return Tuple.Create(dist, absMag1, absMag2, mass1, mass2);
        }

        /// <summary>
        /// metoda pre konverziu stupnov na radiany
        /// </summary>
        /// <param name="degree">velkost v stupnoch</param>
        /// <returns>velkost v radianoch</returns>
        private double ConvertDegreeToRadian(double degree)
        {
            return degree * Math.PI / 180;
        }

        /// <summary>
        /// metoda, ktora urci, ci sa nova hodnota lisi o viac ako 1%
        /// </summary>
        /// <param name="oldValue">stara hodnota</param>
        /// <param name="newValue">nova hodnota</param>
        /// <returns>true ak je rozdiel hodnot vacsi ako 1, false ak nie</returns>
        private bool PercentageDifferenceTooBig(double oldValue, double newValue)
        {
            // if (oldValue == 0) return true;
            var diff = Math.Abs(oldValue - newValue) * 100 / oldValue;
            var result = !(diff >= 0 && diff < 1);

            //Console.WriteLine($"old {oldValue}, new {newValue}, diff {diff}, result {result}");
            return result;
        }

        /// <summary>
        /// metoda na vypocet absolutnej magnitudy
        /// </summary>
        /// <param name="relativeMagnitude">relativna magnituda</param>
        /// <param name="distanceInPc">vzdialenost</param>
        /// <returns>absolutna magnituda</returns>
        private double CalculateAbsoluteMagnitude(double relativeMagnitude, double distanceInPc)
        {
            return relativeMagnitude + 5 * (1 - Math.Log10(distanceInPc));
        }

        /// <summary>
        /// metoda pre vypocet ziariveho vykonu
        /// </summary>
        /// <param name="absoluteMaginute">absolutna magnituda</param>
        /// <returns>ziarivy vykon</returns>
        private double CalculateLuminosity(double absoluteMaginute)
        {
            return luminositySun * Math.Pow(100, (absMagSun - absoluteMaginute) / 5);
        }

        /// <summary>
        /// metoda pre vypocet hmotnosti
        /// </summary>
        /// <param name="luminosity">ziarivy vykon</param>
        /// <returns></returns>
        private double CalculateMass(double luminosity)
        {
            return massSun * Math.Pow(luminosity / luminositySun, (double)1 / 3.5);
        }
    }
}
