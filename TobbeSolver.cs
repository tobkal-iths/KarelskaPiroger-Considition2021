using System;
using System.Collections.Generic;
using System.Linq;
using DotNet.models;

namespace DotNet
{
    public class TobbeSolver
    {
        private const int WeightClasses = 2;
        private const int OrderClasses = 5;
        private List<Package> Packages { get; }
        private List<Package> PlacedPackages { get; }
        public Vehicle Truck { get; }

        private int _posX;
        private int _posY;
        private int _posZ;

        private int _maxX;
        private int _maxY;
        private int _maxZ;

        public TobbeSolver(List<Package> packages, Vehicle vehicle)
        {
            Packages = packages;
            Truck = vehicle;
            PlacedPackages = new List<Package>();

            _posX = 0;
            _posY = 0;
            _posZ = 0;

            _maxX = 0;
            _maxY = 0;
            _maxZ = 0;
        }

        public List<PointPackage> Solve()
        {
            var solution = new List<PointPackage>();
            while (Packages.Count > PlacedPackages.Count)
            {
                var area = FindArea();
                var pack = FindSuitablePackage(area);

                if (pack == null)
                {
                    if (_posY != 0)
                    {
                        _posY = 0;
                        _posZ += _maxZ;
                        _maxZ = 0;
                    }
                    else if (_posZ != 0)
                    {
                        _posX += _maxX;
                        _posY = 0;
                        _posZ = 0;
                        _maxX = 0;
                    }
                    else
                    {
                        Console.WriteLine("N A N I ? ! ! ? T R U C K   F U L L ?");
                        break;
                    }
                }
                else
                {
                    solution.Add(MakePointPackage(pack));
                    PlacedPackages.Add(pack);

                    _maxX = pack.Length > _maxX ? pack.Length : _maxX;
                    _maxY = pack.Width > _maxY ? pack.Width : _maxY;
                    _maxZ = pack.Height > _maxZ ? pack.Height : _maxZ;

                    _posY += pack.Width;
                }
            }
            return solution;
        }

        private Package FindSuitablePackage((int Length, int Width, int Height) area)
        {
            // var pack = Packages.OrderByDescending(l => l.FindAll(p => p.Length < area.Length && p.Width < area.Width && p.Height < area.Height).ToList()).First();
            var packs = Packages.FindAll(p => p.Length < area.Length && p.Width < area.Width && p.Height < area.Height);
            packs = Packages.OrderByDescending(p => p.OrderClass).ToList();
            if (_posZ == 0)
                packs = Packages.OrderByDescending(p => p.WeightClass).ToList();
            else
                packs = Packages.OrderBy(p => p.WeightClass).ToList();

            var pack = packs.First();
            return pack;
        }

        private (int, int, int) FindArea()
        {
            var area = (Truck.Length, Truck.Width, Truck.Height);

            area.Height = Truck.Height - _maxZ;

            area.Height = Truck.Width - _maxY;

            area.Height = Truck.Height - _maxX;

            return area;
        }

        private PointPackage MakePointPackage(Package package)
        {
            var pointPackage = new PointPackage();
            pointPackage.OrderClass = package.OrderClass;
            pointPackage.WeightClass = package.WeightClass;
            pointPackage.Id = package.Id;

            pointPackage.x1 = _posX;
            pointPackage.x2 = _posX;
            pointPackage.x3 = _posX;
            pointPackage.x4 = _posX;
            pointPackage.x5 = _posX + package.Length;
            pointPackage.x6 = _posX + package.Length;
            pointPackage.x7 = _posX + package.Length;
            pointPackage.x8 = _posX + package.Length;

            pointPackage.y1 = _posY;
            pointPackage.y2 = _posY;
            pointPackage.y3 = _posY;
            pointPackage.y4 = _posY;
            pointPackage.y5 = _posY + package.Width;
            pointPackage.y6 = _posY + package.Width;
            pointPackage.y7 = _posY + package.Width;
            pointPackage.y8 = _posY + package.Width;

            pointPackage.z1 = _posZ;
            pointPackage.z2 = _posZ;
            pointPackage.z3 = _posZ;
            pointPackage.z4 = _posZ;
            pointPackage.z5 = _posZ + package.Height;
            pointPackage.z6 = _posZ + package.Height;
            pointPackage.z7 = _posZ + package.Height;
            pointPackage.z8 = _posZ + package.Height;

            return pointPackage;
        }
    }
}