
using System;
using System.Collections.Generic;


using KarambaIDEA.Core;
using KarambaIDEA.IDEA;
//using Grasshopper.Kernel;

//using IdeaRS.ConnectionLink;

namespace KarambaIDEA
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {

        public MainWindow()
        {
            
            //Test();
        }


        /// <summary>
        /// FUNCTION PRIMAIRY FOR DEVELOPMENT.
        /// </summary>
        public void Test(Joint joint)
        {
            //1.create test joint
            // joint.project.SetDefaultWeldType(); //Refactored, can be removed. todo after test run

            //2.Select template
            string templateFilePath = joint.project.templatePath;
#warning: template is not dependent on project, but on the joint itself. Should be part of the IdeaConnection instead of Project.

            //string dirpath = System.IO.Directory.GetCurrentDirectory();

            //string assemblypath = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().FullName);
            //string templateFilePath = dirpath + @"\Resources\ended2members.contemp";


            /*
            string templateFilePath = @"C:\Data\template.contemp";
            string templateFolder = @"C:\Data\TEMPLATES\";
            List<ConnectingMember> conmembers = joint.attachedMembers.OfType<ConnectingMember>().ToList();
            if (joint.IsContinues == false)
            {
                //templateFilePath = templateFolder + "ended2members.contemp";

                //select template saved in assembly/Resources folder
                

            }
            else
            {
                if (conmembers.Count == 1)
                {
                    templateFilePath = templateFolder + "continues1member.contemp";
                }
                if (conmembers.Count == 2)
                {
                    if (conmembers[0].ideaOperationID == 2)
                    {
                        templateFilePath = templateFolder + "continues2members.contemp";
                    }
                    else
                    {
                        templateFilePath = templateFolder + "continues2membersMirror.contemp";
                    }
                }
                if (conmembers.Count == 3)
                {
                    templateFilePath = templateFolder + "continues3members.contemp";
                }
                
            }
            */

            //3. create idea connection
            string path =joint.project.folderpath;

            // Initialize idea references, before calling code.
            AppDomain.CurrentDomain.AssemblyResolve -= new ResolveEventHandler(KarambaIDEA.IDEA.Utils.IdeaResolveEventHandler);
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(KarambaIDEA.IDEA.Utils.IdeaResolveEventHandler);

            IdeaConnection ideaConnection = new IdeaConnection(joint, templateFilePath, path);

            //4.Mapwelds
            ideaConnection.MapWeldsIdsAndOperationIds();

            //ideaConnection.OptimizeWelds();


            //6. save file
            string filePath2 = ideaConnection.filepath + "//" + joint.Name + "joint.ideaCon";
            ideaConnection.SaveIdeaConnectionProjectFile(filePath2);
        }
    }
}

