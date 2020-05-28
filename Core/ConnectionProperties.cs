using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KarambaIDEA.Core
{
    public class ConnectionProperties
    {
        Classification classification;
        public double Sj=double.NaN;
        public double SjR;
        public double SjH;
        public double Mjrd=double.NaN;
        public int k;
        public Element element;

        public ConnectionProperties()
        {

        }
        /// <summary>
        /// Create a ConnectionProperties instance
        /// </summary>
        /// <param name="_Sj">initial rotational stiffness</param>
        /// <param name="_Mjrd">Joint bending moment resistance</param>
        /// <param name="_k">k=25 for unbraced structures, k=8 for braced structures</param>
        public ConnectionProperties(Element element, double _Sj, double _Mjrd, int _k)
        {
            this.Sj = _Sj;
            this.Mjrd = _Mjrd;
            this.k = _k;
            SetSjH(element);
            SetSjR(element, k);
            SetClassification();
            //Set message
        }
        /// <summary>
        /// Set Eurocode boundary criteria for hinged connections
        /// </summary>
        /// <param name="element"></param>
        public void SetSjH(Element element)
        {
            double E = Project.EmodulusSteel;
            double Ib = element.crossSection.Iyy();
            double Lb = element.Line.Length;
            this.SjH = ((0.5 * E * Ib) / Lb )* Math.Pow(10, -6);
        }
        /// <summary>
        /// Set Eurocode boundary criteria for rigid connections
        /// </summary>
        /// <param name="element"></param>
        /// <param name="k">k factor for braced or unbraced structures</param>
        public void SetSjR(Element element, int k)
        {
            double E = Project.EmodulusSteel;
            double Ib = element.crossSection.Iyy();
            double Lb = element.Line.Length;
            this.SjR = ((k * E * Ib) / Lb) * Math.Pow(10, -6);
        }
        /// <summary>
        /// Define the classification of the connection based on the initial stiffness and the boundary criteria
        /// </summary>
        public void SetClassification()
        {
            if (this.Sj > this.SjR)
            {
                this.classification = Classification.Rigid;
            }
            if (this.Sj < this.SjH)
            {
                this.classification = Classification.Hinged;
            }
            else
            {
                this.classification = Classification.SemiRigid;
            }
        }
    }
    public enum Classification
    {
        Rigid,
        SemiRigid,
        Hinged
    }
}
