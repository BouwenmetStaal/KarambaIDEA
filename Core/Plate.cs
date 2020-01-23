using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KarambaIDEA.Core
{
    public class Plate
    {
        public int id;
        public string name;
        public double thickness;
        public double width;
        public double length;
        public double area;
        public bool isTriangle = false;
        public double volume;

        public Plate()
        {

        }

        public Plate(string _name, double _length, double _width, double _thickness, bool _isTriangle = false)
        {
            this.name = _name;
            this.length = _length;
            this.width = _width;
            this.thickness = _thickness;
            SetVolume();
        }

        public void SetVolume()
        {
            double vol = this.length * this.width * this.thickness;
            if (isTriangle == false)
            {
                this.volume = vol;
            }
            else
            {
                this.volume = 0.5 * vol;
            }
        }
    }
}
