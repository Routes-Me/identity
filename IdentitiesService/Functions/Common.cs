using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentitiesService.Functions
{
    public class Common
    {
        public static JArray SerializeJsonForIncludedRepo(List<dynamic> objList)
        {
            var modelsJson = JsonConvert.SerializeObject(objList,
                                 new JsonSerializerSettings
                                 {
                                     NullValueHandling = NullValueHandling.Ignore,
                                 });

            return JArray.Parse(modelsJson);
        }
    }
}
