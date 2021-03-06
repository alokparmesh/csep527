﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParameterEstimation
{
    /// <summary>
    /// EM algorithm
    /// </summary>
    public class EMAlgorithm
    {
        private const double sd = 1.0;
        private const double epsilon = 0.001;
        private const int maxIteration = 1000;

        private readonly double tau;
        private readonly int k;
        private readonly int n;
        private readonly double[] numbers;

        private double[] mu;
        private double[,] z;
        private double logLikelihood;
        private double muChange;
        private double bic;


        public EMAlgorithm(List<double> numbers, int numOfMixtures)
        {
            this.k = numOfMixtures;
            this.numbers = numbers.ToArray();
            this.n = this.numbers.Length;
            this.tau = 1.0 / k;

            this.mu = new double[this.k];
            this.z = new double[n, k];
        }

        /// <summary>
        /// Initialize mu
        /// </summary>
        private void InitMu()
        {
            if (ConfigurationManager.AppSettings["InitializationType"] == "Random")
            {
                this.RandomInit();
            }
            else
            {
                this.RangeInit();
            }
            this.logLikelihood = this.GetLogLikelihood();
            this.bic = this.GetBIC();

            this.PrintMus(true);
        }

        /// <summary>
        /// Random intialization
        /// </summary>
        private void RandomInit()
        {
            HashSet<int> randomIndexes = new HashSet<int>();
            Random r = new Random();

            for (int i = 0; i < k; i++)
            {
                int randomIndex = -1;
                while (true)
                {
                    randomIndex = r.Next(this.n);

                    if(!randomIndexes.Contains(randomIndex))
                    {
                        randomIndexes.Add(randomIndex);
                        break;
                    }
                }

                this.mu[i] = this.numbers[randomIndex];
            }
        }

        /// <summary>
        /// Range based intialization
        /// </summary>
        private void RangeInit()
        {
            double max = this.numbers.Max();
            double min = this.numbers.Min();

            double step = (max - min) / (k - 1);

            this.mu[0] = min;
            for (int i = 1; i < k; i++)
            {
                this.mu[i] = min + i * step;
            }
        }

        /// <summary>
        /// Main function which used to execute operation
        /// </summary>
        public void Calculate()
        {
            this.InitMu();

            for (int i = 0; i < EMAlgorithm.maxIteration; i++)
            {
                bool progress = true;
                EStep();
                MStep();

                progress = this.ShouldContinue();
                this.PrintMus();

                if (!progress)
                {
                    break;
                }
            }

            this.PrintProbabilities();
        }

        /// <summary>
        /// Termination check
        /// </summary>
        /// <returns></returns>
        private bool ShouldContinue()
        {
            double newlikelihood = this.GetLogLikelihood();

            bool progress = ConfigurationManager.AppSettings["InitializationType"] == "Likelihood" ?
                IsLikelihoodDifferent(newlikelihood) : IsMuDifferent();

            this.logLikelihood = newlikelihood;
            this.bic = this.GetBIC();
            return progress;
        }

        private bool IsLikelihoodDifferent(double newlikelihood)
        {
            bool progress = true;

            if (Math.Abs(this.logLikelihood - newlikelihood) < EMAlgorithm.epsilon)
            {
                progress = false;
            }

            return progress;
        }

        private bool IsMuDifferent()
        {
            bool progress = true;

            if (Math.Abs(this.muChange) < EMAlgorithm.epsilon)
            {
                progress = false;
            }

            return progress;
        }

        /// <summary>
        /// Print mus
        /// </summary>
        /// <param name="header"></param>
        private void PrintMus(bool header = false)
        {
            if(header)
            {
                for (int j = 0; j < k; j++)
                {
                    Console.Write(string.Format("mu{0}",j+1));
                    Console.Write("\t");                   
                }
                Console.Write("LogLik");
                Console.Write("\t\t");
                Console.Write("BIC");
                Console.WriteLine();
            }

            for (int j = 0; j < k; j++)
            {
                Console.Write(this.mu[j].ToString("#.0000"));
                Console.Write("\t");
            }

            Console.Write(this.logLikelihood.ToString("#.0000"));
            Console.Write("\t");

            Console.Write(this.bic.ToString("#.0000"));
            Console.WriteLine();
        }

        /// <summary>
        /// Print Probabilities class
        /// </summary>
        private void PrintProbabilities()
        {
            Console.Write("\t");
            Console.Write("x");
            Console.Write("\t");
            for (int j = 0; j < k; j++)
            {
                Console.Write(string.Format("P(cls {0} | x)", j + 1));
                Console.Write("\t");
            }
            Console.WriteLine();

            for (int i = 0; i < Math.Min(n, Program.PrintprobabilitiesNRow); i++)
            {
                Console.Write(string.Format("[{0},]", i + 1));
                Console.Write("\t");

                double number = this.numbers[i];
                Console.Write(number);
                Console.Write("\t");

                for (int j = 0; j < k; j++)
                {
                    Console.Write(z[i, j].ToString("e"));
                    Console.Write("\t");
                }
                Console.WriteLine();
            }
        }

        /// <summary>
        /// E Step
        /// </summary>
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

        /// <summary>
        /// M step
        /// </summary>
        private void MStep()
        {
            this.muChange = 0.0;

            for (int j = 0; j < k; j++)
            {
                double exi = 0.0;
                double e = 0.0;

                for (int i = 0; i < n; i++)
                {
                    exi += this.z[i, j] * this.numbers[i];
                    e += this.z[i, j];
                }

                if (e > 0)
                {
                    double previousMu = this.mu[j];
                    this.mu[j] = exi / e;
                    this.muChange += Math.Abs(this.mu[j] - previousMu);
                }
            }
        }

        /// <summary>
        /// Get log likelihood
        /// </summary>
        /// <returns></returns>
        private double GetLogLikelihood()
        {
            double likelihood = 1.0;

            for (int i = 0; i < this.n; i++)
            {
                double number = this.numbers[i];

                double density = 0.0;
                for (int j = 0; j < k; j++)
                {
                    density += GuassianDensity(number, this.mu[j], EMAlgorithm.sd) * this.tau;
                }

                likelihood *= density;
            }

            return Math.Log(likelihood);
        }

        /// <summary>
        /// Get BIC score
        /// </summary>
        /// <returns></returns>
        private double GetBIC()
        {
            return 2 * this.logLikelihood - this.k * Math.Log(this.n);
        }

        /// <summary>
        /// Get density
        /// </summary>
        /// <param name="number"></param>
        /// <param name="average"></param>
        /// <param name="sd"></param>
        /// <returns></returns>
        private static double GuassianDensity(double number, double average, double sd)
        {
            double d = (number - average) / sd;
            double exp = Math.Exp(-1.0 * (d * d) / 2);
            return exp / (sd * Math.Sqrt(2 * Math.PI));
        }
    }
}
