using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParameterEstimation
{
    public class EMAlgorithm
    {
        private const double sd = 1.0;
        private readonly double tau;
        private readonly int k;
        private readonly int n;
        private readonly double[] numbers;

        private double[] mu;
        private double[,] z;


        public EMAlgorithm(List<double> numbers, int numOfMixtures)
        {
            this.k = numOfMixtures;
            this.numbers = numbers.ToArray();
            this.n = this.numbers.Length;
            this.tau = 1.0 / k;

            this.mu = new double[this.k];
            this.z = new double[n, k];
        }

        private void InitMu()
        {
            /*double max = this.numbers.Max();
            double min = this.numbers.Min();

            double step = (max - min) / (k - 1);

            this.mu[0] = min;
            for(int i = 1; i < k; i++)
            {
                this.mu[i] = min + i * step;
            }*/

            this.mu[0] = -20;
            this.mu[1] = 6;
            //this.mu[2] = 55;
        }

        public void Calculate()
        {
            this.InitMu();

            for (int i = 0; i < 5; i++)
            {
                EStep();
                MStep();
            }          
        }

        private void EStep()
        {
            for (int i = 0; i < n; i++)
            {
                double number = this.numbers[i];

                double sum = 0.0;
                for (int j = 0; j < k; j++)
                {
                    z[i, j] = GuassianDensity(number, this.mu[j], EMAlgorithm.sd);
                    sum += z[i, j];
                }

                if (sum > 0)
                {
                    for (int j = 0; j < k; j++)
                    {
                        z[i, j] = z[i, j] / sum;
                    }
                }
            }
        }

        private void MStep()
        {
            for (int j = 0; j < k; j++)
            {
                double exi = 0.0;
                double e = 0.0;

                for (int i = 0; i < n; i++)
                {
                    exi += this.z[i, j] * this.numbers[i];
                    e += this.z[i, j];
                }

                if(e > 0)
                {
                    this.mu[j] = exi / e;
                }

                Console.Write(this.mu[j]);
                Console.Write("\t");
            }
            Console.WriteLine();
        }

        private static double GuassianDensity(double number, double average, double sd)
        {
            double d = (number - average) / sd;
            double exp = Math.Exp(-1.0 * (d * d) / 2);
            return exp / (sd * Math.Sqrt(2 * Math.PI));
        }
    }
}
