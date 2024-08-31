using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoidsSimulator.Main
{
    /// <summary>
    /// Adds acceleration requests and magnitudes together until reaching the maximum magnitude value, then caps the acceleration at that magnitude.
    /// </summary>
    internal class AcceleratorAccumulator
    {
        float _currentMagnitude;
        float _maxMagnitude;
        public Vector2 Value;
        public AcceleratorAccumulator(float maxMagnitude)
        {
            _currentMagnitude = 0;
            _maxMagnitude = maxMagnitude;
        }
        public void AddAccelerationRequest(Vector2 request)
        {
            if (_currentMagnitude >= _maxMagnitude)
                return;

            float magnitude = Vector2.Distance(Vector2.Zero, request);
            _currentMagnitude += magnitude;
            Value += request;
            if (_currentMagnitude > _maxMagnitude)
            {
                float excess = _currentMagnitude - _maxMagnitude;
                float percentOverflow = excess / magnitude;
                //Debug.WriteLine("Percent overflow: " + percentOverflow);
                Value -= request * percentOverflow;
                //Debug.WriteLine("New value: " + Value);
                _currentMagnitude -= excess;
                //Debug.WriteLine("Current magnitude: " + _currentMagnitude);
            }
        }
    }
}
