using System;
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
        public List<Package>[,] SortedPackages { get; private set; }
        public Vehicle Truck { get; }
        public List<PointPackage> GameSolution { get; private set; }
        public List<PointPackage> RowListY { get; private set; }
        public List<PointPackage> RowListZ { get; private set; }

        public int PosX { get; private set; }
        public int PosY { get; private set; }
        public int PosZ { get; private set; }

        private int _lastPlacedPackageWidth;
        private bool _isYFull = false;
        private bool _isZFull = false;

        public SuperSolver(List<Package> packages, Vehicle vehicle)
        {
            GameSolution = new List<PointPackage>();
            Packages = packages;
            Truck = vehicle;

            _lastPlacedPackageWidth = 0;
        }

        /// <summary>
        /// Creates a solution from Class Properties.
        /// </summary>
        /// <returns>Returns a list of PointPackages</returns>
        public List<PointPackage> Solve()
        {
            PosX = 0;
            PosY = 0;
            PosZ = 0;

            var highestX = 0;
            var highestZ = 0;

            SortPackages(Packages);

            while (PackagesLeft())
            {
                // TODO Börja här!
                // Det här är en test!
                var pack = FindBestPackage(FindNextAreaToFit());
                if (pack != null)
                {
                    highestZ = pack.Height > highestZ ? pack.Height : highestZ;
                    highestX = pack.Length > highestX ? pack.Length : highestX;
                    PlacePackage(pack);
                    PosY += pack.Width;
                }
                else
                {
                    if (PosY != 0)
                    {
                        PosY = 0;
                        PosZ += highestZ;
                        highestZ = 0;
                    }   
                    else if (PosZ != 0)
                    {
                        PosZ = 0;
                        PosX += highestX;
                        highestX = 0;
                    }
                    else
                    {
                        Console.WriteLine("TRUCK IS FULL! ABORT!");
                        break;
                    }
                }
            }

            return GameSolution;
        }

        /// <summary>
        /// Finds the next area to place a package in.
        /// </summary>
        /// <returns>Returns a tuple (length, width, height)</returns>
        private (int, int, int) FindNextAreaToFit()
        {

            var tmpX = 0;
            var tmpY = 0;
            var tmpZ = 0;

            var area = (0, 0, 0);

            if (_isZFull)
            {
                
            }
            else
            {
                if (_isYFull)
                {
                    var lowest = RowListY.OrderBy(p => p.z5).First();

                    var nexts = RowListY.FindAll(p => p.y1 > lowest.y1);
                    var next = nexts.OrderBy(p => p.y1).First();

                    tmpZ = lowest.z5;
                    tmpY = next.y1 - lowest.y1;
                    tmpX = Truck.Length - lowest.x5;
                }
                else
                {
                    tmpZ = Truck.Height;
                    tmpX = Truck.Length;
                    tmpY = Truck.Width - _lastPlacedPackageWidth;
                }
            }

            area.Item1 = tmpX;
            area.Item2 = tmpY;
            area.Item3 = tmpZ;

            return area;
        }

        private void PlacePackage(Package pack)
        {
            GameSolution.Add(MakePointPackage(pack));
            _lastPlacedPackageWidth += pack.Width;
            RemovePackFromList(pack);
        }

        private void RemovePackFromList(Package pack)
        {
            foreach (var list in SortedPackages)
            {
                if (list.Contains(pack))
                    list.Remove(pack);
            }
        }

        /// <summary>
        /// Splits Packages into lists by WeightClass and OrderClass, each list sorted by Package size.
        /// </summary>
        private void SortPackages(List<Package> packages)
        {
            var sortedPacks = new List<Package>[WeightClasses, OrderClasses];
            for (int i = 0; i < WeightClasses; i++)
            {
                for (int j = 0; j < OrderClasses; j++)
                {
                    if (i == 1)
                        sortedPacks[i, j] = Packages.FindAll(p => p.WeightClass == 2 && p.OrderClass == j);
                    else
                        sortedPacks[i, j] = Packages.FindAll(p => p.WeightClass < 2 && p.OrderClass == j);

                    sortedPacks[i, j] = sortedPacks[i, j].OrderByDescending(p => (p.Length + p.Width + p.Height)).ToList();
                }
            }
            SortedPackages = sortedPacks;
        }

        /// <summary>
        /// Finds the best package to place based on current preferences.
        /// </summary>
        /// <returns>Returns a Package from the given array.</returns>
        private Package FindBestPackage((int, int, int)area)
        {

            for (int w = WeightClasses - 1; w >= 0; w--)
            {
                for (int o = OrderClasses - 1; o >= 0; o--)
                {
                    if (SortedPackages[w, o].Count > 0)
                    {
                        for (int i = 0; i < SortedPackages[w, o].Count; i++)
                        {
                            var pack = SortedPackages[w, o][i];

                            if (DoesPackageFit(pack, area))
                            {
                                return pack;
                            }
                        }
                    }
                }
            }
            return null;
        }

        private bool DoesPackageFit(Package pack, (int, int, int) area)
        {
            return pack.Length < area.Item1 && pack.Width < area.Item2 && pack.Height < area.Item3;
        }

        private bool DoesPackageFitX(Package pack)
        {
            return pack.Length < Truck.Length - PosX;
        }

        private bool DoesPackageFitZ(Package pack)
        {
            return pack.Height < Truck.Height - PosZ;
        }

        private bool DoesPackageFitY(Package pack)
        {
            return pack.Width < Truck.Width - PosY;
        }


        /// <summary>
        /// Checks if there are packages left to sort in the given array.
        /// </summary>
        /// <returns>Returns true if there are packages left, or false otherwise.</returns>
        private bool PackagesLeft()
        {
            foreach (var list in SortedPackages)
            {
                if (list.Count > 0)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Places a Package in the truck by coordinates.
        /// </summary>
        /// <param name="package">The package to be placed.</param>
        /// <returns>Returns the package as a PointPackage at the given coordinates.</returns>
        private PointPackage MakePointPackage(Package package)
        {
            var pointPackage = new PointPackage();
            pointPackage.OrderClass = package.OrderClass;
            pointPackage.WeightClass = package.WeightClass;
            pointPackage.Id = package.Id;

            pointPackage.x1 = PosX;
            pointPackage.x2 = PosX;
            pointPackage.x3 = PosX;
            pointPackage.x4 = PosX;
            pointPackage.x5 = PosX + package.Length;
            pointPackage.x6 = PosX + package.Length;
            pointPackage.x7 = PosX + package.Length;
            pointPackage.x8 = PosX + package.Length;

            pointPackage.y1 = PosY;
            pointPackage.y2 = PosY;
            pointPackage.y3 = PosY;
            pointPackage.y4 = PosY;
            pointPackage.y5 = PosY + package.Width;
            pointPackage.y6 = PosY + package.Width;
            pointPackage.y7 = PosY + package.Width;
            pointPackage.y8 = PosY + package.Width;

            pointPackage.z1 = PosZ;
            pointPackage.z2 = PosZ;
            pointPackage.z3 = PosZ;
            pointPackage.z4 = PosZ;
            pointPackage.z5 = PosZ + package.Height;
            pointPackage.z6 = PosZ + package.Height;
            pointPackage.z7 = PosZ + package.Height;
            pointPackage.z8 = PosZ + package.Height;

            return pointPackage;
        }
    }
}























// |      |
// |      |
// |      |
// |      |
// | X XX |
// |XXXXX |