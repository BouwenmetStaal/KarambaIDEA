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
    public interface IIdeaModification
    {
        bool ModifyConnection(ConnectionHiddenCheckClient client, string ConnectionIdentifier);
    }

    public class IdeaModifyConnectionParameters : IIdeaModification
    {
        private readonly List<IIdeaParameter> _updateparameters;

        public IdeaModifyConnectionParameters(List<IIdeaParameter> updateParameters)
        {
            _updateparameters = updateParameters;
        }

        public bool ModifyConnection(ConnectionHiddenCheckClient client, string conIdentifier)
        {
            parameter[] projectParameters;
            List<parameter> updateParameters = new List<parameter>();

            string conParamsJson = client.GetParametersJSON(conIdentifier);

            projectParameters = JsonConvert.DeserializeObject<parameter[]>(conParamsJson);

            if (projectParameters == null)
                return false;

            foreach (var param in _updateparameters)
            {
                if(projectParameters.Select(x => x.identifier).Contains(param.GetIdentifier()))
                {
                    updateParameters.Add(param.ExtractParameter());
                }
            }

            string updatedParameters = JsonConvert.SerializeObject(updateParameters);

            string appliedparams = client.ApplyParameters(conIdentifier, updatedParameters);

            return true;
        }
    }

    public class IdeaModifyConnectionLoading : IIdeaModification
    {
        public bool ModifyConnection(ConnectionHiddenCheckClient client, string ConnectionIdentifier)
        {
            throw new NotImplementedException();
        }
    }
}
