using System;
using UnityEngine;

namespace XPBD_Engine.Scripts.Utilities
{
    public class Hash
    {
        private float _spacing;
        private int _tableSize;
        private int[] _cellStart;
        private int[] _cellEntries;
        private int[] _queryIds;
        private int _querySize;
        
        public Hash(float spacing, int maxNumObjects)
        {
            _spacing = spacing;
            _tableSize = 2 * maxNumObjects;
            _cellStart = new int[_tableSize + 1];
            _cellEntries = new int[maxNumObjects];
            _queryIds = new int[maxNumObjects];
            _querySize = 0;
        }
        private int HashCoords(int xi,int yi, int zi) {
            var h = (xi * 92837111) ^ (yi * 689287499) ^ (zi * 283923481);	// fantasy function
            return Math.Abs(h) % _tableSize; 
        }
        private int IntCoord(float coord) {
            return (int) Math.Floor(coord /_spacing);
        }

        private int HashPos(Vector3 vector3)
        {
            return HashCoords(IntCoord(vector3.x), IntCoord(vector3.y), IntCoord(vector3.z));
        }

        public void Create(Vector3[] pos)
        {
            var numObjects = Math.Min(pos.Length / 3, _cellEntries.Length);
            
            // determine cell sizes
            for (var i = 0; i < numObjects; i++)
            {
                var h = HashPos( pos[i]);
                _cellStart[h]++;
            }
            
            // determine cells starts

            var start = 0;
            for (var i = 0; i < _tableSize; i++) {
                start += _cellStart[i];
                _cellStart[i] = start;
            }
            _cellStart[_tableSize] = start;	// guard
            
            // fill in objects ids

            for (var i = 0; i < numObjects; i++) {
                var h = HashPos(pos[i]);
                _cellStart[h]--;
                _cellEntries[_cellStart[h]] = i;
            }

            
        }

        public void Query(Vector3 pos,float maxDist)
        {
            var x0 = IntCoord(pos.x - maxDist);
            var y0 = IntCoord(pos.y - maxDist);
            var z0 = IntCoord(pos.z - maxDist);

            var x1 = IntCoord(pos.x + maxDist);
            var y1 = IntCoord(pos.y + maxDist);
            var z1 = IntCoord(pos.z + maxDist);
            
            _querySize = 0;
            
            for (var xi = x0; xi <= x1; xi++) {
                for (var yi = y0; yi <= y1; yi++) {
                    for (var zi = z0; zi <= z1; zi++) {
                        var h = HashCoords(xi, yi, zi);
                        var start = _cellStart[h];
                        var end = _cellStart[h + 1];

                        for (var i = start; i < end; i++) {
                            _queryIds[_querySize] = _cellEntries[i];
                            _querySize++;
                        }
                    }
                }
            }
        }

        public int[] GetQueryIds()
        {
            return _queryIds;
        }
    }
}