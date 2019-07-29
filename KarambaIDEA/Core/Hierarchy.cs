// Copyright (c) 2019 Rayaan Ajouz, Bouwen met Staal. Please see the LICENSE file	
// for details. All rights reserved. Use of this source code is governed by a	
// Apache-2.0 license that can be found in the LICENSE file.	
namespace KarambaIDEA.Core
{
    public class Hierarchy
    {
        public int numberInHierarchy;
        public string groupname;

        public Hierarchy()
        {

        }

        public Hierarchy(int _numberInHierarchy, string _groupname)
        {
            this.numberInHierarchy = _numberInHierarchy;
            this.groupname = _groupname;
        }
    }
}
