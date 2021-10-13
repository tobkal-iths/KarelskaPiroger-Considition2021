using System;
using System.Collections.Generic;
using System.Linq;
using DotNet.models;

namespace DotNet
{
    public class SuperSolver
    {
        private const int WeightClasses = 2;
        private const int OrderClasses = 5;
        public List<Package> Packages { get; }
        public Vehicle Truck { get; }
        private List<Package>[,] SortedPackages { get; set; }
        private List<PointPackage> GameSolution { get; set; }
        private List<PointPackage> RowListY { get; set; }
        private List<PointPackage> RowListZ { get; set; }
        private PointPackage ReplacePack { get; set; }
        private int PosX { get; set; }
        private int PosY { get; set; }
        private int PosZ { get; set; }
        private int RowX { get; set; }

        private int _lastPlacedPackageWidth;
        private int _nextSpace = 0;

        private bool _YFull = false;
        private bool _ZFull = false;

        private int _iterator = 0;

        public SuperSolver(List<Package> packages, Vehicle vehicle)
        {
            RowListY = new List<PointPackage>();
            RowListZ = new List<PointPackage>();

            GameSolution = new List<PointPackage>();

            Packages = packages;
            Truck = vehicle;
        }

        /// <summary>
        /// Creates a solution from Class Properties.
        /// </summary>
        /// <returns>Returns a list of PointPackages.</returns>
        public List<PointPackage> Solve()
        {
            PosX = 0;
            PosY = 0;
            PosZ = 0;

            RowX = 0;
            _lastPlacedPackageWidth = 0;

            SortPackages(Packages);

            while (GetRemainingSortedPackages() > 0)
            {
                Package pack;
                if (PosZ == 0)
                    pack = FindBestPackage(FindNextAreaToFit());
                else
                    pack = FindBestPackage(FindNextAreaToFit(), false);

                if (pack != null)
                {
                    PlacePackage(pack);
                    _iterator = 0;
                    continue;
                }
                else if (_iterator >= GetRemainingSortedPackages())
                {
                    Console.WriteLine("TRUCK IS FULL!111");
                    break;
                }

                _iterator++;
            }
            return GameSolution;
        }

        /// <summary>
        /// Makes a point package from the given package and adds it to the solution. Then adds and removes the package from the relevant helper list.
        /// </summary>
        /// <param name="pack">The package to be added to solution.</param>
        private void PlacePackage(Package pack)
        {
            var pointPack = MakePointPackage(pack);
            GameSolution.Add(pointPack);
            _lastPlacedPackageWidth += pack.Width;
            _nextSpace = 0;

            RemovePackFromList(pack);

            RowListY.Add(pointPack);
            RowListZ.Add(pointPack);
        }

        /// <summary>
        /// Removes a package from the SortedPackages list.
        /// </summary>
        /// <param name="pack"></param>
        private void RemovePackFromList(Package pack)
        {
            foreach (var list in SortedPackages)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i].Id == pack.Id)
                        list.RemoveAt(i);
                }
            }
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

        /// <summary>
        /// Finds the package that best fits the given area.
        /// </summary>
        /// <returns>Returns a Package, or null if none would fit the area.</returns>
        private Package FindBestPackage((int, int, int) area, bool heavyPrio = true)
        {
            Package pack = null;
            if (heavyPrio)
            {
                for (int w = WeightClasses - 1; w >= 0; w--)
                {
                    pack = FindPackageFromOrderClass(area, w);
                }
            }
            else
            {
                for (int w = 0; w < WeightClasses; w++)
                {
                    pack = FindPackageFromOrderClass(area, w);
                }
            }
            return pack;
        }

        /// <summary>
        /// Finds the first package that fits the area searching by order class.
        /// </summary>
        /// <param name="area"></param>
        /// <param name="w"></param>
        /// <returns></returns>
        private Package FindPackageFromOrderClass((int, int, int) area, int w)
        {
            for (int o = OrderClasses - 1; o >= 0; o--)
            {
                if (SortedPackages[w, o].Count > 0)
                {
                    for (int i = 0; i < SortedPackages[w, o].Count; i++)
                    {
                        var pack = SortedPackages[w, o][i];

                        for (int j = 0; j < 6; j++)
                        {
                            var turnedPack = TurnPackage(pack, j);

                            if (DoesPackageFit(turnedPack, area))
                            {
                                return turnedPack;
                            }
                        }
                    }
                    if (!_YFull)
                    {
                        _YFull = true;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Tests a package against a given area to see if it fits.
        /// </summary>
        /// <param name="pack">The package to be tested.</param>
        /// <param name="area">The area to test against.</param>
        /// <returns>Returns true if the package fits within the area.</returns>
        private bool DoesPackageFit(Package pack, (int, int, int) area)
        {
            return pack.Length < area.Item1 && pack.Width < area.Item2 && pack.Height < area.Item3;
        }

        /// <summary>
        /// Checks if there are packages left to sort in the SortedPackages array.
        /// </summary>
        /// <returns>Returns true if there are packages left in the array.</returns>
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
        /// Rotates a package according to a given iterator. This is meant to be used in a loop.
        /// </summary>
        /// <param name="pack">The package to be rotated.</param>
        /// <param name="turn">Iterator controlling what rotations to apply to the given package.</param>
        /// <returns>returns a rotated package.</returns>
        private Package TurnPackage(Package pack, int turn)
        {
            var turnedPack = new Package();
            turnedPack.Id = pack.Id;
            turnedPack.OrderClass = pack.OrderClass;
            turnedPack.WeightClass = pack.WeightClass;
            int l = pack.Length;
            int w = pack.Width;
            int h = pack.Height;

            switch (turn)
            {
                case 0:
                    turnedPack.Length = l;
                    turnedPack.Width = w;
                    turnedPack.Height = h;
                    return turnedPack;
                case 1:
                    turnedPack.Length = l;
                    turnedPack.Width = h;
                    turnedPack.Height = w;
                    return turnedPack;
                case 2:
                    turnedPack.Length = w;
                    turnedPack.Width = l;
                    turnedPack.Height = h;
                    return turnedPack;
                case 3:
                    turnedPack.Length = w;
                    turnedPack.Width = h;
                    turnedPack.Height = l;
                    return turnedPack;
                case 4:
                    turnedPack.Length = h;
                    turnedPack.Width = w;
                    turnedPack.Height = l;
                    return turnedPack;
                case 5:
                    turnedPack.Length = h;
                    turnedPack.Width = l;
                    turnedPack.Height = w;
                    return turnedPack;
                default:
                    return null;
            }
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

            if (_ZFull)
            {
                RowX = RowListZ.Max(p => p.x5);

                _ZFull = false;
                _YFull = false;

                RowListY.Clear();
                RowListZ.Clear();

                _lastPlacedPackageWidth = 0;
                _nextSpace = 0;
            }

            if (_YFull)
            {
                var lowest = new PointPackage();
                if (RowListY.Count > 0)
                    lowest = RowListY.OrderBy(p => p.z5).ToList()[_nextSpace];

                if (_nextSpace < RowListY.Count - 1)
                    _nextSpace++;
                else
                {
                    _ZFull = true;
                    _nextSpace = 0;
                }


                var nexts = RowListY.FindAll(p => p.y1 > lowest.y1);
                if (nexts.Count > 0)
                {
                    var next = nexts.First();

                    tmpX = Truck.Length - RowX; // - RowX
                    tmpY = next.y1 - lowest.y1;
                    tmpZ = Truck.Height - lowest.z5;
                }
                else
                {
                    tmpX = Truck.Length - RowX; // Bytte från lowest.x5 till RowX
                    tmpY = Truck.Width - lowest.y1;
                    tmpZ = Truck.Height - lowest.z5;
                }

                ReplacePack = lowest;

                PosX = lowest.x1;
                PosY = lowest.y1;
                PosZ = lowest.z5;
            }
            else
            {
                tmpX = Truck.Length - RowX; // - RowX
                tmpY = Truck.Width - _lastPlacedPackageWidth;
                tmpZ = Truck.Height;

                PosX = RowX;
                PosY = _lastPlacedPackageWidth;
                PosZ = 0;
            }

            var area = (tmpX, tmpY, tmpZ);

            return area;
        }

        /// <summary>
        /// Checks if there are packages left to sort in the SortedPackages array.
        /// </summary>
        /// <returns>Returns true if there are packages left in the array.</returns>
        private int GetRemainingSortedPackages()
        {
            var remainingPackages = 0;

            foreach (var list in SortedPackages)
                remainingPackages += list.Count;

            return remainingPackages;
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

                    sortedPacks[i, j] = sortedPacks[i, j].OrderByDescending(p => (p.Length * p.Width * p.Height)).ToList();
                }
            }

            foreach (var list in sortedPacks)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    var dimensions = new int[3];
                    dimensions[0] = list[i].Length;
                    dimensions[1] = list[i].Width;
                    dimensions[2] = list[i].Height;
                    Array.Sort(dimensions);

                    list[i].Length = dimensions[0];
                    list[i].Width = dimensions[1];
                    list[i].Height = dimensions[2];
                }
            }

            SortedPackages = sortedPacks;
        }
    }
}
