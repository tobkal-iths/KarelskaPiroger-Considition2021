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

            _packages = packages.OrderByDescending(p => p.Height).ToList();
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
            while (_packages.Count > 0)
            {
                _heaps.Add(new List<Package>());

                foreach (var pack in _packages)
                {
                    int temp = _heaps[^1].Sum(p => p.Height);
                    if (temp + pack.Height < _truckZ)
                        _heaps[^1].Add(pack);
                }
                foreach (var pack in _heaps[^1])
                {
                    _packages.Remove(pack);
                }
            }
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
            int tempX = 0;
            int tempY = 0;

            foreach (var heap in _heaps)
            {
                _zp = 0;
                foreach (var pack in heap)
                {
                    tempY = pack.Width > tempY ? pack.Width : tempY;
                    tempX = pack.Length > tempX ? pack.Length : tempX;
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
                else // x1: 91 | X5: 122 || x1: 117 | x5: 129
                {
                    _yp = 0;
                    _xp += tempX;
                    if (_xp == 117)
                        _xp = 122;
                    tempX = 0;
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

            GameSolution.Add(placedPackage);
        }
    }
}
