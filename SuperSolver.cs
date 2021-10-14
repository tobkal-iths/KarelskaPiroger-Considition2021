using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNet.models;

namespace DotNet
{
    public class SuperSolver
    {
        private readonly int _truckX;
        private readonly int _truckY;
        private readonly int _truckZ;
        private readonly int _orderClasses;
        private bool _isTruckOverFull;
        private List<List<Package>> _heaps;
        private List<Package> _packages;
        private int _xp, _yp, _zp;
        private int _lastKnownLongestPackage;
        private readonly double _percent = 0.25d;
        private readonly int _orderClassAccuracyStartingValue = 1;
        private readonly double _packageTurningAccuracy = 2;
        public List<PointPackage> GameSolution { get; private set; }
        public SuperSolver(List<Package> packages, Vehicle vehicle)
        {
            _truckX = vehicle.Length;
            _truckY = vehicle.Width;
            _truckZ = vehicle.Height;
            _heaps = new List<List<Package>>();
            GameSolution = new List<PointPackage>();
            _orderClasses = 5;

            var avarageVolume = packages.Sum(p => p.Height * p.Length * p.Width) / packages.Count;

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

                if (pack.Height - pack.Length > pack.Width * _packageTurningAccuracy)
                {
                    pack.Length = dimensions[1];
                    pack.Width = dimensions[2];
                    pack.Height = dimensions[0];
                }
            }

            _packages = packages.OrderByDescending(p => p.Height * p.Length * p.Width).ToList();
        }

        public List<PointPackage> Solve()
        {
            MakeHeaps();
            CheckHeaps();
            SortHeaps();
            PackTruck();

            if (_isTruckOverFull)
            {
                _xp = 0;
                _lastKnownLongestPackage = 0;
                RotatePacks();
                GameSolution = new List<PointPackage>();
                PackTruck();
            }

            return GameSolution;
        }

        private void MakeHeaps()
        {            
            var minusPercent = 1 - _percent;
            var plusPercent = 1 + _percent;
            while (_packages.Count > 0)
            {
                _heaps.Add(new List<Package>());
                var currentHeapOriginal = new List<Package>();
                int tempHeight = 0;
                for (int i = _orderClassAccuracyStartingValue; i < _orderClasses; i++)
                {
                    foreach (var pack in _packages)
                    {
                        var lastHeap = _heaps[^1];
                        tempHeight = lastHeap.Sum(p => p.Height);
                        if (tempHeight + pack.Height < _truckZ)
                        {
                            if (lastHeap.Count > 0)
                            {
                                if ((lastHeap[0].Length * plusPercent > pack.Length) && (lastHeap[0].Length * minusPercent < pack.Length))
                                {
                                    if ((lastHeap[0].Width * plusPercent > pack.Width) && (lastHeap[0].Width * minusPercent < pack.Width))
                                    {
                                        if ((lastHeap[0].OrderClass <= pack.OrderClass + i) && (lastHeap[0].OrderClass >= pack.OrderClass - i))
                                        {
                                            lastHeap.Add(pack);
                                            currentHeapOriginal.Add(pack);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                lastHeap.Add(pack);
                                currentHeapOriginal.Add(pack);
                            }
                        }
                    }
                    foreach (var pack in currentHeapOriginal)
                        _packages.Remove(pack);
                }
            }
        }

        private void RotatePacks()
        {
            foreach (var heap in _heaps)
            {
                foreach (var pack in heap)
                {
                    int width = pack.Width;
                    int length = pack.Length;
                    pack.Width = length;
                    pack.Length = width;
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

        private void CheckHeaps()
        {
            var soloHeaps = _heaps.FindAll(h => h.Count == 1).ToList();
            var soloPacks = new List<Package>();
            var newHeaps = new List<List<Package>>();
            foreach (var heap in soloHeaps)
                soloPacks.Add(heap[0]);
            soloPacks = soloPacks.OrderByDescending(p => p.Width).ToList();

            for (int i = 0; i < soloPacks.Count; i++)
            {
                var newHeap = new List<Package>();
                newHeap.Add(soloPacks[i]);
                var newHeapHeight = soloPacks[i].Height;
                for (int j = i + 1; j < soloPacks.Count; j++)
                {
                    if (soloPacks[i].OrderClass == soloPacks[j].OrderClass)
                    {
                        if (newHeapHeight + soloPacks[j].Height < _truckZ)
                        {
                            newHeap.Add(soloPacks[j]);
                            newHeapHeight += soloPacks[j].Height;
                        }
                    }
                }

                if (!IsAdded(newHeaps, newHeap))
                {
                    newHeaps.Add(newHeap);
                }
            }

            foreach (var heap in soloHeaps)
                _heaps.Remove(heap);

            foreach (var heap in newHeaps)
                _heaps.Add(heap);
        }

        private bool IsAdded(List<List<Package>> newHeaps, List<Package> newHeap)
        {
            foreach (var heap in newHeaps)
            {
                foreach (var pack in heap)
                {
                    if (newHeap.Contains(pack))
                        return true;
                }
            }
            return false;
        }

        private void PackTruck()
        {
            int tempY = 0;
            int tempX = 0;

            foreach (var heap in _heaps)
            {
                _zp = 0;

                tempY = heap.Max(p => p.Width);
                tempX = heap.Max(p => p.Length);

                if (_yp + tempY < _truckY && _xp + tempX < _truckX)
                {
                    foreach (var pack in heap)
                    {
                        AddPackage(pack);
                        _zp += pack.Height;
                    }
                    _yp += tempY;
                    tempY = 0;
                }
                else if (_xp + tempX < _truckX)
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
                else
                    Console.WriteLine("Skit ner dig!");
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

            if (_lastKnownLongestPackage > _truckX)
                _isTruckOverFull = true;

            GameSolution.Add(placedPackage);
        }
    }
}
