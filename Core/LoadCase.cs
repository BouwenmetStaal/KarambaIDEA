// Copyright (c) 2019 Rayaan Ajouz, Bouwen met Staal. Please see the LICENSE file	
// for details. All rights reserved. Use of this source code is governed by a	
// Apache-2.0 license that can be found in the LICENSE file.	
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KarambaIDEA.Core
{
    public class LoadCase
    {
        
        public List<LoadsPerLine> loadsPerLines = new List<LoadsPerLine>();
        public int id;
        public string name;
        public Project project;

        
        //Er kunnen een x aantal loadcases voorkomen in het project. Hoe ga ik dit daarvoor Robuust maken.
        public LoadCase(Project _project, int _id)
        {
            this.project = _project;
            _project.loadcases.Add(this);           
            this.id = _id;
            
        }
    }
    
    public class LoadsPerLine
    {
        
        public Load startLoad;
        public Load endLoad;
        public Element element;
        public LoadCase loadcase;

        
        public LoadsPerLine(Element _element, LoadCase _loadcase,Load _Start, Load _End)
        {

            this.startLoad = _Start;
            this.endLoad = _End;
            this.element = _element;
            this.loadcase = _loadcase;
            _loadcase.loadsPerLines.Add(this);
        }

        public LoadsPerLine(Load _Start, Load _End)
        {
            this.startLoad = _Start;
            this.endLoad = _End;
        }
    }


    public class Load
    {
        public double N;
        public double Vz;
        public double My;

        public double Vy;
        public double Mt;
        public double Mz;


        public Load(double _N, double _Vz, double _My)
        {
            this.N = _N;
            this.Vz = _Vz;
            this.My = _My;

        }

        public Load(double _N, double _Vz, double _Vy, double _Mt, double _My, double _Mz)
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
