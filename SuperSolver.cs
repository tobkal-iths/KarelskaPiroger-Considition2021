using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using DotNet.models;

namespace DotNet
{
    public class SuperSolver
    {
        private const int WeightClasses = 2;
        private const int OrderClasses = 5;
        public List<Package> Packages { get; }
        public Vehicle Truck { get; }

        public SuperSolver(List<Package> packages, Vehicle vehicle)
        {
            Packages = packages;
            Truck = vehicle;
        }

        public List<PointPackage> Solve()
        {
            var sortedPackages = new List<Package>[WeightClasses, OrderClasses];
            for (int i = 0; i < WeightClasses; i++)
            {
                for (int j = 0; j < OrderClasses; j++)
                {
                    if (i == 1)
                        sortedPackages[i, j] = Packages.FindAll(p => p.WeightClass == 2 && p.OrderClass == j);
                    else
                        sortedPackages[i, j] = Packages.FindAll(p => p.WeightClass < 2 && p.OrderClass == j);

                    sortedPackages[i, j] = sortedPackages[i, j].OrderByDescending(p => (p.Length + p.Width + p.Height)).ToList();
                }
            }

            return MakeSoultion(sortedPackages);
        }

        private List<PointPackage> MakeSoultion(List<Package>[,] sortedPackages)
        {
            var solution = new List<PointPackage>();

            var xPos = 0;
            var yPos = 0;
            var zPos = 0;

            while (xPos < Truck.Length)
            {
                while (zPos < Truck.Height)
                {
                    while (yPos < Truck.Width)
                    {
                        if (zPos == 0)
                        {
                            for (int i = 4; i >= 0; i--)
                            {
                                if (sortedPackages[1, i].Count() > 0)
                                {
                                    var pack = sortedPackages[1, i].First();
                                    if (yPos + pack.Width <= Truck.Width)
                                    {
                                        solution.Add(MakePointPackage(pack, xPos, yPos, zPos));
                                        sortedPackages[1, i].Remove(pack);

                                        yPos = pack.Width;

                                        break;
                                    }
                                }
                            }
                            //else if (sortedPackages[1, 3].Count() > 0)
                        }
                        else
                        {
                            // TODO
                        }
                        // Find best package
                        // Add package dimensions to xPos, yPos, zPos
                    }
                }
            }

            solution.Add(MakePointPackage(new Package(), 1, 1, 1));

            return solution;
        }

        private PointPackage MakePointPackage(Package package, int px, int py, int pz)
        {
            var pointPackage = new PointPackage();
            pointPackage.OrderClass = package.OrderClass;
            pointPackage.WeightClass = package.WeightClass;

            pointPackage.x1 = px;
            pointPackage.x2 = px;
            pointPackage.x3 = px;
            pointPackage.x4 = px;
            pointPackage.x5 = px + package.Length;
            pointPackage.x6 = px + package.Length;
            pointPackage.x7 = px + package.Length;
            pointPackage.x8 = px + package.Length;

            pointPackage.y1 = py;
            pointPackage.y2 = py;
            pointPackage.y3 = py;
            pointPackage.y4 = py;
            pointPackage.y5 = py + package.Width;
            pointPackage.y6 = py + package.Width;
            pointPackage.y7 = py + package.Width;
            pointPackage.y8 = py + package.Width;

            pointPackage.z1 = pz;
            pointPackage.z2 = pz;
            pointPackage.z3 = pz;
            pointPackage.z4 = pz;
            pointPackage.z5 = pz + package.Height;
            pointPackage.z6 = pz + package.Height;
            pointPackage.z7 = pz + package.Height;
            pointPackage.z8 = pz + package.Height;

            return pointPackage;
        }
    }
}