using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Grasshopper.GUI;
using KarambaIDEA.IDEA;

namespace KarambaIDEA.Grasshopper
{

	public class IdeaConnectionContainer
	{
		public int JointReference;
		public string Filepath;

		//Maybe later
		//internal string xMLGeometry;
		//internal string results;

        public IdeaConnectionContainer(IdeaConnection connection)
        {
			JointReference = connection.joint.id;
			Filepath = connection.filePath;
        }
	}

	public class GH_IdeaConnection : GH_Goo<IdeaConnectionContainer>
	{
		public GH_IdeaConnection() : base() { }
		public GH_IdeaConnection(IdeaConnectionContainer connectionContainer) : base(connectionContainer) { }
		public GH_IdeaConnection(GH_IdeaConnection ghConnection) : base(ghConnection.Value) { }

		public override string ToString() { return Value.ToString(); }
		public override IGH_Goo Duplicate() { return new GH_IdeaConnection(this); }
		public override bool IsValid { get { return true; } }
		public override string TypeDescription { get { return "IDEA Connection"; } }
		public override string TypeName { get { return "IDEA Connection"; } }
		public override bool Read(GH_IO.Serialization.GH_IReader reader) { return false; }
		public override bool Write(GH_IO.Serialization.GH_IWriter writer) { return true; }
	}
}
