using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Web.Composing;

namespace Cogworks.Meganav.Helpers
{
    public static class PreValueHelper
    {
        public static IDictionary<string, string> GetPreValues(int dataTypeId)
        {
            var context = Current.UmbracoContext;
            var dataTypeService = Current.Services.DataTypeService;

            var preValueCollection = dataTypeService.GetDataType(dataTypeId);

            return preValueCollection.ToDictionary(x => x.Key, x => x.Key);
        }
    }
}