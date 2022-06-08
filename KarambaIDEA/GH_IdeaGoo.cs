using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Grasshopper.GUI;
using KarambaIDEA.IDEA;
using KarambaIDEA.IDEA.Parameters;
using KarambaIDEA.Core;
using Rhino.Geometry;
using Rhino.Display;


namespace KarambaIDEA.Grasshopper
{

	public interface IGH_XmlOutputObject 
	{
		string getXmlString();
	}

	public class GH_KarambaIdeaProject : GH_Goo<Project>
	{
		public GH_KarambaIdeaProject() : base() { }
		public GH_KarambaIdeaProject(Project project) : base(project) { }
		public GH_KarambaIdeaProject(GH_KarambaIdeaProject ghProject) : base(ghProject.Value) { }

		public override string ToString() { return Value.ToString(); }
		public override IGH_Goo Duplicate() { return new GH_KarambaIdeaProject(this); }
		public override bool IsValid { get { return true; } }
		public override string TypeDescription { get { return "IDEAProject"; } }
		public override string TypeName { get { return "KarambaIDEAProject"; } }
		public override bool Read(GH_IO.Serialization.GH_IReader reader) { return false; }
		public override bool Write(GH_IO.Serialization.GH_IWriter writer) { return true; }
	}

	public class GH_KarambaIdeaJoint : GH_GeometricGoo<KarambaIdeaJoint>, IGH_PreviewData, IGH_XmlOutputObject
	{
		internal Point3d JointPosition;

		public GH_KarambaIdeaJoint() : base() { }
		public GH_KarambaIdeaJoint(KarambaIdeaJoint joint) : base(joint) 
		{
			KarambaIDEA.Core.Point pt = joint.centralNodeOfJoint;
			JointPosition = new Point3d(pt.X, pt.Y, pt.Z);
		}
		public GH_KarambaIdeaJoint(GH_KarambaIdeaJoint ghJoint) : base(ghJoint.Value) 
		{
			KarambaIDEA.Core.Point pt = ghJoint.Value.centralNodeOfJoint;
			JointPosition = new Point3d(pt.X, pt.Y, pt.Z);
		}

		public override string ToString() { return Value.ToString(); }
		public override IGH_Goo Duplicate() { return new GH_KarambaIdeaJoint(this); }
		public override bool IsValid { get { return true; } }
		public override string TypeDescription { get { return "KarambaIDEAJoint"; } }
		public override string TypeName { get { return "KarambaIDEAJoint"; } }
		public override bool CastTo<T>(out T target)
		{
			if (typeof(T).IsAssignableFrom(GH_TypeLib.t_gh_point))
			{
				object obj = new GH_Point(JointPosition);
				target = (T)obj;
				return true;
			}
			if (typeof(T).IsAssignableFrom(GH_TypeLib.t_rc_point3d))
			{
				object obj = JointPosition;
				target = (T)obj;
				return true;
			}
			if (typeof(T).IsAssignableFrom(GH_TypeLib.t_gh_int))
			{
				object obj = new GH_Integer((int)Value.id);
				target = (T)obj;
				return true;
			}
			if (typeof(T).IsAssignableFrom(GH_TypeLib.t_int32))
			{
				object obj = (int)Value.id;
				target = (T)obj;
				return true;
			}
			target = default(T);
			return false;
		}

		public BoundingBox ClippingBox { get { return this.Boundingbox; } }

		public override BoundingBox Boundingbox { get { return new BoundingBox(JointPosition, JointPosition); } }

		public override bool Read(GH_IO.Serialization.GH_IReader reader) { return false; }
		public override bool Write(GH_IO.Serialization.GH_IWriter writer) { return true; }

        public override IGH_GeometricGoo DuplicateGeometry(){ return new GH_KarambaIdeaJoint(this); }

        public override BoundingBox GetBoundingBox(Transform xform)
        {
			Point3d t = JointPosition;
			t.Transform(xform);
			BoundingBox box = new BoundingBox(t, t);
			box.MakeValid();
			return box;
		}

        public override IGH_GeometricGoo Transform(Transform xform) { return null; }

        public override IGH_GeometricGoo Morph(SpaceMorph xmorph) {	return null; }
		public void DrawViewportWires(GH_PreviewWireArgs args) { }

		public void DrawViewportMeshes(GH_PreviewMeshArgs args)
		{	
			args.Pipeline.DrawPoint(JointPosition, PointStyle.X, 5, args.Material.Diffuse);
		}

        public string getXmlString()
        {
            return Value.ToStringXML();
        }
    }

	public class GH_IdeaConnection : GH_Goo<IdeaConnection_2>
	{
		public GH_IdeaConnection() : base() { }
		public GH_IdeaConnection(IdeaConnection_2 connectionContainer) : base(connectionContainer) { }
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

	public class GH_JointTemplateAssign : GH_Goo<TemplateAssign>
	{
		public GH_JointTemplateAssign() : base() { }
		public GH_JointTemplateAssign(TemplateAssign templateAssign) : base(templateAssign) { }
		public GH_JointTemplateAssign(GH_JointTemplateAssign ghTemplateAssign) : base(ghTemplateAssign.Value) { }

		public override string ToString() { return Value.ToString(); }
		public override IGH_Goo Duplicate() { return new GH_JointTemplateAssign(); }
		public override bool IsValid { get { return true; } }
		public override string TypeDescription { get { return "JointTemplateAssign"; } }
		public override string TypeName { get { return "JointTemplateAssign"; } }
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

	public class GH_IdeaModification : GH_Goo<IdeaModification>
	{
		public GH_IdeaModification() : base() { }
		public GH_IdeaModification(IdeaModification modification) : base(modification) { }
		public GH_IdeaModification(GH_IdeaModification ghModification) : base(ghModification.Value) { }

		public override string ToString() { return Value.ToString(); }
		public override IGH_Goo Duplicate() { return new GH_IdeaModification(); }
		public override bool IsValid { get { return true; } }
		public override string TypeDescription { get { return "IdeaModfication"; } }
		public override string TypeName { get { return "IdeaModification"; } }
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

	public class GH_IdeaCodeSetup : GH_Goo<IdeaCodeSetup>
	{
		public GH_IdeaCodeSetup() : base() { }
		public GH_IdeaCodeSetup(IdeaCodeSetup setUp) : base(setUp) { }
		public GH_IdeaCodeSetup(GH_IdeaCodeSetup ghSetUp) : base(ghSetUp.Value) { }

		public override string ToString() { return Value.ToString(); }
		public override IGH_Goo Duplicate() { return new GH_IdeaCodeSetup(this); }
		public override bool IsValid { get { return true; } }
		public override string TypeDescription { get { return "IdeaItemResult"; } }
		public override string TypeName { get { return "IdeaItemResult"; } }
		public override bool Read(GH_IO.Serialization.GH_IReader reader) { return false; }
		public override bool Write(GH_IO.Serialization.GH_IWriter writer) { return true; }

		public void UpdateValue(IGH_Goo goo)
		{
			
		}
	}
}
