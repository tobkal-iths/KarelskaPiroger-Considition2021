using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks.Dataflow;
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

        public PointPackage ReplacePack { get; private set; }
        public int PosX { get; private set; }
        public int PosY { get; private set; }
        public int PosZ { get; private set; }

        private int _lastPlacedPackageWidth;
        private int _nextSpace = 0;
        private bool _isYFull = false;
        private bool _isZFull = false;
        private bool _isXFull = false;

        public SuperSolver(List<Package> packages, Vehicle vehicle)
        {
            RowListY = new List<PointPackage>();
            RowListZ = new List<PointPackage>();

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

            SortPackages(Packages);

            while (PackagesLeft())
            {
                if (_isXFull)
                {
                    Console.WriteLine("TRUCK IS FULL, ABORT!");
                    break;
                }

                Package pack;
                if (PosZ == 0)
                    pack = FindBestPackage(FindNextAreaToFit());
                else
                    pack = FindBestPackage(FindNextAreaToFit(), false);
                
                if (pack != null)
                {
                    PlacePackage(pack);
                }
                // else
                // {
                //     //ToDo Change this. It is temporary for debugging purposes.
                //     if (_isZFull)
                //     {
                //         Console.WriteLine("Z IS FULL! ABORT!");
                //         break;
                //     }
                //
                // }
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

            if (_isZFull)
            {
                var lowest = new PointPackage();
                lowest = RowListZ.OrderBy(p => p.x5).ToList()[_nextSpace];
                
                if (_nextSpace < RowListZ.Count - 1)
                    _nextSpace++;
                else
                    _isXFull = true;
                
                // var nexts = RowListY.FindAll(p => p.y1 > lowest.y1);
                // if (nexts.Count > 0)
                // {
                //     var next = nexts.First();
                //
                //     tmpX = Truck.Length;
                //     tmpY = next.y1 - lowest.y1;
                //     tmpZ = Truck.Height - lowest.z5;
                // }
                // else if (lowest != null)
                // {
                //     tmpY = Truck.Width - lowest.y1;
                //     tmpZ = lowest.z5;
                //     tmpX = Truck.Length - lowest.x5;   
                // }
                // else
                // {
                //     tmpX = Truck.Length;
                //     tmpY = Truck.Width - _lastPlacedPackageWidth;
                //     tmpZ = Truck.Height;
                // }
                
                ReplacePack = lowest;

                PosX = lowest.x5;
                PosY = lowest.y1;
                PosZ = lowest.z1;
                
                tmpX = Truck.Length - lowest.x5;
                tmpY = lowest.y5 - lowest.y1;
                tmpZ = lowest.z5 - lowest.z1;
            }
            else if (_isYFull)
            {
                var lowest = new PointPackage();
                lowest = RowListY.OrderBy(p => p.z5).ToList()[_nextSpace];
                
                if (_nextSpace < RowListY.Count - 1)
                    _nextSpace++;
                else
                    _isZFull = true;


                var nexts = RowListY.FindAll(p => p.y1 > lowest.y1);
                if (nexts.Count > 0)
                {
                    var next = nexts.First();

                    tmpX = Truck.Length;
                    tmpY = next.y1 - lowest.y1;
                    tmpZ = Truck.Height - lowest.z5;
                }
                else if (lowest != null)
                {
                    tmpY = Truck.Width - lowest.y1;
                    tmpZ = lowest.z5;
                    tmpX = Truck.Length - lowest.x5;   
                }
                else
                {
                    tmpX = Truck.Length;
                    tmpY = Truck.Width - _lastPlacedPackageWidth;
                    tmpZ = Truck.Height;
                }
                
                ReplacePack = lowest;
                
                PosX = lowest.x1;
                PosY = lowest.y1;
                PosZ = lowest.z5;
            }
            else
            {
                tmpZ = Truck.Height;
                tmpX = Truck.Length;
                tmpY = Truck.Width - _lastPlacedPackageWidth;
                
                PosX = 0;
                PosY = _lastPlacedPackageWidth;
                PosZ = 0;
            }

            var area = (tmpX, tmpY, tmpZ);

            return area;
        }

        private void PlacePackage(Package pack)
        {
            var pointPack = MakePointPackage(pack);
            GameSolution.Add(pointPack);
            _lastPlacedPackageWidth += pack.Width;
            _nextSpace = 0;

            RemovePackFromList(pack);
            
            if (_isZFull)
            {
                RowListZ.Remove(ReplacePack);
                RowListZ.Add(pointPack);
            }
            else if (_isYFull)
            {
                RowListY.Remove(ReplacePack);
                RowListY.Add(pointPack);
                RowListZ.Add(pointPack);
            }
            else
            {
                RowListY.Add(pointPack);
                RowListZ.Add(pointPack);   
            }
        }

        private void RemovePackFromList(Package pack)
        {
            foreach (var list in SortedPackages)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i].Id == pack.Id)
                        list.RemoveAt(i);
                }

                //if (list.Contains(pack))
                //    list.Remove(pack);
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
        /// Finds the package that best fits the given area.
        /// </summary>
        /// <returns>Returns a Package, or null if none would fit the area.</returns>
        private Package FindBestPackage((int, int, int)area, bool heavyPrio = true)
        {
            if (heavyPrio)
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

                                //Todo Här någon stans ska vi börja snurra skiten!

                                for (int j = 0; j < 6; j++)
                                {
                                    var turnedPack = TurnPackage(pack, j);

                                    if (DoesPackageFit(turnedPack, area))
                                    {
                                        return turnedPack;
                                    }
                                }

                                //if (DoesPackageFit(pack, area))
                                //{
                                //    return pack;
                                //}
                            }
                            if (!_isYFull)
                            {
                                _isYFull = true;
                            }
                        }
                    }
                }
            }
            else
            {
                for (int w = 0; w < WeightClasses; w++)
                {
                    for (int o = OrderClasses - 1; o >= 0; o--)
                    {
                        if (SortedPackages[w, o].Count > 0)
                        {
                            for (int i = 0; i < SortedPackages[w, o].Count; i++)
                            {
                                var pack = SortedPackages[w, o][i];

                                //ToDo Implementera skiten bara!

                                for (int j = 0; j < 6; j++)
                                {
                                    var turnedPack = TurnPackage(pack, j);

                                    if (DoesPackageFit(turnedPack, area))
                                    {
                                        return turnedPack;
                                    }
                                }

                                //if (DoesPackageFit(pack, area))
                                //{
                                //    return pack;
                                //}
                            }
                            if (!_isYFull)
                            {
                                _isYFull = true;
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

        //private bool DoesPackageFitX(Package pack)
        //{
        //    return pack.Length < Truck.Length - PosX;
        //}

        //private bool DoesPackageFitZ(Package pack)
        //{
        //    return pack.Height < Truck.Height - PosZ;
        //}

        //private bool DoesPackageFitY(Package pack)
        //{
        //    return pack.Width < Truck.Width - PosY;
        //}


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

        private Package TurnPackage(Package pack, int turn)
        {
            Package turnedPack = new();
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
