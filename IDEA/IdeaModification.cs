using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IdeaStatiCa.Plugin;
using KarambaIDEA.IDEA.Parameters;
using Newtonsoft.Json;

namespace KarambaIDEA.IDEA
{
    public enum MODIFYHEIRACHY { GeometeryUpdate, Template_Full, Template_Partial, Loading, Parameters }

    public abstract class IdeaModification
    {
        public MODIFYHEIRACHY ModifyHeirachy = MODIFYHEIRACHY.Parameters;

        public abstract bool ModifyConnection(ConnectionHiddenCheckClient client, string ConnectionIdentifier);
    }

    public class IdeaModifyApplyTemplate_Full : IdeaModification
    {
        private readonly IdeaTemplateAssignFull _templateAssign;

        public IdeaModifyApplyTemplate_Full(IdeaTemplateAssignFull templateAssign)
        {
            ModifyHeirachy = MODIFYHEIRACHY.Template_Full;
            _templateAssign = templateAssign;
        }
        public override bool ModifyConnection(ConnectionHiddenCheckClient client, string connectionIdentifier)
        {
            try
            {
                IdeaRS.OpenModel.Connection.ApplyConnTemplateSetting settings = new IdeaRS.OpenModel.Connection.ApplyConnTemplateSetting();

                settings.UseMatFromOrigin = true;

                client.ApplyTemplate(connectionIdentifier, ((IdeaTemplate)_templateAssign.Template).FilePath, settings);

                if (_templateAssign.ParamModify != null)
                    _templateAssign.ParamModify.ModifyConnection(client, connectionIdentifier);

                return true;
            }
            catch(Exception e)
            {
                throw new Exception(string.Format("Error '{0}'", e.Message));
            }
        }
    }

    public class IdeaModifyApplyTemplate_Partial : IdeaModification
    {
        private readonly IdeaTemplateAssignPartial _templateAssign;

        public IdeaModifyApplyTemplate_Partial(IdeaTemplateAssignPartial templateAssign)
        {
            ModifyHeirachy = MODIFYHEIRACHY.Template_Partial;
            _templateAssign = templateAssign;
        }
        public override bool ModifyConnection(ConnectionHiddenCheckClient client, string connectionIdentifier)
        {
            try
            {
                client.ApplySimpleTemplate(connectionIdentifier, ((IdeaTemplate)_templateAssign.Template).FilePath, null, _templateAssign.SupportIndex, _templateAssign.ConnectingMembers);

                if (_templateAssign.ParamModify != null)
                    _templateAssign.ParamModify.ModifyConnection(client, connectionIdentifier);

                return true;
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("Error '{0}'", e.Message));
            }
        }
    }

    public class IdeaModifyConnectionParameters : IdeaModification
    {
        private readonly List<IIdeaParameter> _updateparameters = new List<IIdeaParameter>();

        public IdeaModifyConnectionParameters(List<IIdeaParameter> updateParameters)
        {
            _updateparameters = updateParameters;
        }

        public override bool ModifyConnection(ConnectionHiddenCheckClient client, string conIdentifier)
        {
            if (_updateparameters.Count > 0)
            {
                parameter[] projectParameters;
                List<parameter> updateParameters = new List<parameter>();

                string conParamsJson = client.GetParametersJSON(conIdentifier);

                projectParameters = JsonConvert.DeserializeObject<parameter[]>(conParamsJson);

                if (projectParameters == null)
                    return false;

                foreach (var param in _updateparameters)
                {
                    if (projectParameters.Select(x => x.identifier).Contains(param.GetIdentifier()))
                    {
                        updateParameters.Add(param.ExtractParameter());
                    }
                }

                string updatedParameters = JsonConvert.SerializeObject(updateParameters);

                string appliedparams = client.ApplyParameters(conIdentifier, updatedParameters);
            }
            return true;
        }
    }

    public class IdeaModifyConnectionLoading : IdeaModification
    {
        public override bool ModifyConnection(ConnectionHiddenCheckClient client, string ConnectionIdentifier)
        {
            throw new NotImplementedException();
        }
    }
}
