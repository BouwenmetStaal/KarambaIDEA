using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Grasshopper.GUI;
using KarambaIDEA.IDEA;
using KarambaIDEA.IDEA.Parameters;

namespace KarambaIDEA.Grasshopper
{
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

	public class GH_IdeaTemplate : GH_Goo<IdeaTemplate>
	{
		public GH_IdeaTemplate() : base() { }
		public GH_IdeaTemplate(IdeaTemplate template) : base(template) { }
		public GH_IdeaTemplate(GH_IdeaTemplate ghTemplate) : base(ghTemplate.Value) { }

		public override string ToString() { return Value.ToString(); }
		public override IGH_Goo Duplicate() { return new GH_IdeaTemplate(this); }
		public override bool IsValid { get { return true; } }
		public override string TypeDescription { get { return "IdeaTemplate"; } }
		public override string TypeName { get { return "IdeaTemplate"; } }
		public override bool Read(GH_IO.Serialization.GH_IReader reader) { return false; }
		public override bool Write(GH_IO.Serialization.GH_IWriter writer) { return true; }
	}

	public class GH_IdeaParameter : GH_Goo<IIdeaParameter>
	{
		public GH_IdeaParameter() : base() { }
		public GH_IdeaParameter(IIdeaParameter parameter) : base(parameter) { }
		public GH_IdeaParameter(GH_IdeaParameter ghParam) : base(ghParam.Value) { }

		public override string ToString() { return Value.ToString(); }
		public override IGH_Goo Duplicate() { return new GH_IdeaParameter(this.Value.Clone() as IIdeaParameter); }
		public override bool IsValid { get { return true; } }
		public override string TypeDescription { get { return "IdeaParam"; } }
		public override string TypeName { get { return "IdeaParam"; } }
		public override bool Read(GH_IO.Serialization.GH_IReader reader) { return false; }
		public override bool Write(GH_IO.Serialization.GH_IWriter writer) { return true; }

	}

	public class GH_IdeaItemResult : GH_Goo<IdeaItemResult>
	{
		public GH_IdeaItemResult() : base() { }
		public GH_IdeaItemResult(IdeaItemResult itemResult) : base(itemResult) { }
		public GH_IdeaItemResult(GH_IdeaItemResult ghItemResult) : base(ghItemResult.Value) { }

		public override string ToString() { return Value.ToString(); }
		public override IGH_Goo Duplicate() { return new GH_IdeaItemResult(this); }
		public override bool IsValid { get { return true; } }
		public override string TypeDescription { get { return "IdeaItemResult"; } }
		public override string TypeName { get { return "IdeaItemResult"; } }
		public override bool Read(GH_IO.Serialization.GH_IReader reader) { return false; }
		public override bool Write(GH_IO.Serialization.GH_IWriter writer) { return true; }

	}
}
