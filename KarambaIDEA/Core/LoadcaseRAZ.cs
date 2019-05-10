// Copyright (c) 2019 Rayaan Ajouz. Please see the LICENSE file	
// for details. All rights reserved. Use of this source code is governed by a	
// Apache-2.0 license that can be found in the LICENSE file.	
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KarambaIDEA.Core
{
    public class LoadcaseRAZ
    {
        
        public List<LoadsPerLineRAZ> loadsPerLineRAZs = new List<LoadsPerLineRAZ>();
        public int id;
        public string name;
        public Project project;

        
        //Er kunnen een x aantal loadcases voorkomen in het project. Hoe ga ik dit daarvoor Robuust maken.
        public LoadcaseRAZ(Project _project, int _id)
        {
            this.project = _project;
            _project.loadcases.Add(this);           
            this.id = _id;
            
        }
    }
    
    public class LoadsPerLineRAZ
    {
        
        public LoadsRAZ startLoads;
        public LoadsRAZ endLoads;
        public ElementRAZ elementRAZ;
        public LoadcaseRAZ loadcase;

        
        public LoadsPerLineRAZ(ElementRAZ _elementRAZ, LoadcaseRAZ _loadcase,LoadsRAZ _Start, LoadsRAZ _End)
        {

            this.startLoads = _Start;
            this.endLoads = _End;
            this.elementRAZ = _elementRAZ;
            this.loadcase = _loadcase;
            _loadcase.loadsPerLineRAZs.Add(this);
        }

        public LoadsPerLineRAZ(LoadsRAZ _Start, LoadsRAZ _End)
        {
            this.startLoads = _Start;
            this.endLoads = _End;
        }
    }


    public class LoadsRAZ
    {
        public double N;
        public double Vz;
        public double My;

        public double Vy;
        public double Mt;
        public double Mz;


        public LoadsRAZ(double _N, double _Vz, double _My)
        {
            this.N = _N;
            this.Vz = _Vz;
            this.My = _My;

        }

        public LoadsRAZ(double _N, double _Vz, double _Vy, double _Mt, double _My, double _Mz)
        {
            this.N = _N;
            this.Vz = _Vz;
            this.My = _My;

            this.Vy = _Vy;
            this.Mt = _Mt;
            this.Mz = _Mz;
        }
    }
}
