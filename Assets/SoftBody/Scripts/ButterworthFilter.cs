using System;
using System.Collections.Generic;
using UnityEngine;

namespace SoftBody.Scripts
{
    public class ButterworthFilter
    {
        
        //samplingFreq = 100; # sampled at 1 kHz = 100 samples / second
        //wc = 2*np.pi*3; # cutoff frequency (rad/s)
        //n = 2; # Filter order
        float[] b2_6pi = {0.007777f, 0.01555399f, 0.007777f};
        float[] a2_6pi = {1.73550016f, -0.76660814f};
        //wc = 2*np.pi*2; # cutoff frequency (rad/s)
        //n = 2; # Filter order
        float[] b2_4pi = {0.00361257f, 0.00722515f, 0.00361257f};
        float[] a2_4pi = {1.82292669f, -0.83737699f};
        //wc = 2*np.pi*1; # cutoff frequency (rad/s)
        //n = 3; # Filter order
        float[] b3_2pi = {2.91183374e-05f, 8.73550123e-05f, 8.73550123e-05f, 2.91183374e-05f};
        float[] a3_2pi = {2.87439819f, -2.75656072f, 0.88192959f};
        //wc = 2*np.pi*2; # cutoff frequency (rad/s)
        //n = 3; # Filter order
        float[] b3_4pi = {0.00021878f, 0.00065633f, 0.00065633f, 0.00021878f};
        float[] a3_4pi = {2.74916512f, -2.52881103f, 0.7778957f};
        //wc = 2*np.pi*3; # cutoff frequency (rad/s)
        //n = 3; # Filter order
        float[] b3_6pi = {0.00069354f, 0.00208062f, 0.00208062f, 0.00069354f};
        float[] a3_6pi = {2.62465742f, -2.31650672f, 0.68630099f};

        private float[] b;
        private float[] a;

        
        private int filterOrder;
        private int cutoffFrequency;

        //Record of vertices
        private List<Vector3[]> _verticesXn;
        private List<Vector3[]> _verticesYn;
        private int _verticesCount;


        public ButterworthFilter(int order,int cutoffFreq,Vector3[] m_vertices)
        {
            _verticesXn = new List<Vector3[]>();
            _verticesYn = new List<Vector3[]>();
            for (int i = 0; i < order+1; i++)
            {
                _verticesXn.Add(m_vertices);
                _verticesYn.Add(m_vertices);
            }

            _verticesCount = m_vertices.Length;
            filterOrder = order;
            cutoffFrequency = cutoffFreq;
            SetCoef();
        }

        private void SetCoef()
        {
            if (filterOrder==2 && cutoffFrequency==2)
            {
                b = b2_4pi;
                a = a2_4pi;
            }
            else if (filterOrder==2 && cutoffFrequency==3)
            {
                b = b2_6pi;
                a = a2_6pi;
            }
            else if (filterOrder==3 && cutoffFrequency==1)
            {
                b = b3_2pi;
                a = a3_2pi;
            }
            else if (filterOrder==3 && cutoffFrequency==2)
            {
                b = b3_4pi;
                a = a3_4pi;
            }
            else if (filterOrder==3 && cutoffFrequency==3)
            {
                b = b3_6pi;
                a = a3_6pi;
            }
            else
            {
                b = b2_4pi;
                a = a2_4pi;
            }
        }

        public Vector3[] Filter(Vector3[] m_vertices)
        {
            //Remove the old xn
            _verticesXn.RemoveAt(_verticesXn.Count - 1);
            //Add the fresh xn
            _verticesXn.Insert(0, m_vertices);
            
            //Remove the old yn
            _verticesYn.RemoveAt(_verticesYn.Count - 1);
            

            Vector3[] filteredYn=new Vector3[_verticesCount];

            
            for (int i = 0; i < _verticesCount; i++)
            {
                //Example filter order 2
                //filteredYn[i] = a[0] * _verticesYn[1][i] + a[1] * _verticesYn[2][i] + b[0] * _verticesXn[0][i] + b[1] * _verticesXn[1][i] + b[2] * _verticesXn[2][i];
                
                for (int j = 0; j < _verticesXn.Count; j++)
                {
                    filteredYn[i] += b[j] * _verticesXn[j][i];
                    
                }
                for (int j = 0; j < _verticesYn.Count; j++)
                {
                    filteredYn[i] += a[j] * _verticesYn[j][i];
                }
                
             
            }
            
            
            //Add the fresh yn
            _verticesYn.Insert(0, filteredYn);
            
            return filteredYn;

        }




    }
}  