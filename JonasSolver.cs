using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNet.models;

namespace DotNet
{
    public class JonasSolver
    {
        private readonly int _truckX;
        private readonly int _truckY;
        private readonly int _truckZ;
        private List<List<Package>> _heaps;
        private List<Package> _packages;
        private int _xp, _yp, _zp;
        private int _lastKnownLongestPackage;
        public List<PointPackage> GameSolution { get; private set; }
        public JonasSolver(List<Package> packages, Vehicle vehicle)
        {
            _truckX = vehicle.Length;
            _truckY = vehicle.Width;
            _truckZ = vehicle.Height;
            _heaps = new List<List<Package>>();
            GameSolution = new List<PointPackage>();


            foreach (var pack in packages)
            {
                var dimensions = new int[3];
                dimensions[0] = pack.Length;
                dimensions[1] = pack.Width;
                dimensions[2] = pack.Height;
                Array.Sort(dimensions);

                pack.Length = dimensions[0];
                pack.Width = dimensions[1];
                pack.Height = dimensions[2];
            }

            _packages = packages.OrderByDescending(p => p.Height * p.Length * p.Width).ToList();
        }

        public List<PointPackage> Solve()
        {
            MakeHeaps();
            SortHeaps();
            PackTruck();

            return GameSolution;
        }

        private void MakeHeaps()
        {
            double accuracy = 1.4;
            while (_packages.Count > 0)
            {
                _heaps.Add(new List<Package>());
                var currentHeapOriginal = new List<Package>();
                int temp = 0;
                foreach (var pack in _packages)
                {
                    temp = _heaps[^1].Sum(p => p.Height);
                    if (temp + pack.Height < _truckZ)
                    {
                        if (_heaps[^1].Count > 0)
                        {
                            if ((_heaps[^1][0].Length < pack.Length * accuracy) || (_heaps[^1][0].Length * accuracy < pack.Length))
                            {
                                if (_heaps[^1][0].Width < pack.Width * accuracy || _heaps[^1][0].Width * accuracy < pack.Width)
                                {
                                    _heaps[^1].Add(pack);
                                    currentHeapOriginal.Add(pack);
                                }
                            }
                        }
                        else
                        {
                            _heaps[^1].Add(pack);
                            currentHeapOriginal.Add(pack);
                        }
                    }
                }

                foreach (var pack in currentHeapOriginal)
                    _packages.Remove(pack);
            }
        }

        // Används inte i dagsläget, men den får ligga kvar för framtida optimering.
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
                    break;
                case 1:
                    turnedPack.Length = l;
                    turnedPack.Width = h;
                    turnedPack.Height = w;
                    break;
                case 2:
                    turnedPack.Length = w;
                    turnedPack.Width = l;
                    turnedPack.Height = h;
                    break;
                case 3:
                    turnedPack.Length = w;
                    turnedPack.Width = h;
                    turnedPack.Height = l;
                    break;
                case 4:
                    turnedPack.Length = h;
                    turnedPack.Width = w;
                    turnedPack.Height = l;
                    break;
                case 5:
                    turnedPack.Length = h;
                    turnedPack.Width = l;
                    turnedPack.Height = w;
                    break;
                default:
                    return null;
            }
            turnedPack.Id = pack.Id;
            turnedPack.OrderClass = pack.OrderClass;
            turnedPack.WeightClass = pack.WeightClass;

            return turnedPack;
        }

        private void SortHeaps()
        {
            for (int i = 0; i < _heaps.Count; i++)
            {
                _heaps[i] = _heaps[i].OrderByDescending(h => h.WeightClass).ToList();
            }

            _heaps = _heaps.OrderByDescending(h => h.Sum(x => x.OrderClass / h.Count)).ToList();
        }

        private void PackTruck()
        {
            int tempY = 0;

            foreach (var heap in _heaps)
            {
                _zp = 0;
                foreach (var pack in heap)
                {
                    tempY = pack.Width > tempY ? pack.Width : tempY;
                }
                if (_yp + tempY < _truckY)
                {
                    foreach (var pack in heap)
                    {
                        AddPackage(pack);
                        _zp += pack.Height;
                    }
                    _yp += tempY;
                    tempY = 0;
                }
                else
                {
                    _yp = 0;
                    _xp = _xp < _lastKnownLongestPackage ? _lastKnownLongestPackage : _xp;
                    foreach (var pack in heap)
                    {
                        AddPackage(pack);
                        _zp += pack.Height;
                    }
                    _yp += tempY;
                    tempY = 0;
                }
            }
        }

        private void AddPackage(Package package)
        {
            var placedPackage = new PointPackage()
            {
                Id = package.Id,
                x1 = _xp,
                x2 = _xp,
                x3 = _xp,
                x4 = _xp,
                x5 = _xp + package.Length,
                x6 = _xp + package.Length,
                x7 = _xp + package.Length,
                x8 = _xp + package.Length,
                y1 = _yp,
                y2 = _yp,
                y3 = _yp,
                y4 = _yp,
                y5 = _yp + package.Width,
                y6 = _yp + package.Width,
                y7 = _yp + package.Width,
                y8 = _yp + package.Width,
                z1 = _zp,
                z2 = _zp,
                z3 = _zp,
                z4 = _zp,
                z5 = _zp + package.Height,
                z6 = _zp + package.Height,
                z7 = _zp + package.Height,
                z8 = _zp + package.Height,
                OrderClass = package.OrderClass,
                WeightClass = package.WeightClass
            };

            _lastKnownLongestPackage = _lastKnownLongestPackage < placedPackage.x5 ? placedPackage.x5 : _lastKnownLongestPackage;

            GameSolution.Add(placedPackage);
        }
    }
}
