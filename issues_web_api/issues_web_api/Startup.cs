using System;
using System.Net.Http.Headers;
using System.Web.Http;
using CollectionJson.Client;
using Drum;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Owin;

namespace issues_web_api
{
    public sealed class Startup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            var config = new HttpConfiguration();

            //TODO map '/api' route to handle redirections
            
            config.EnableCors();

            config.MapHttpAttributeRoutesAndUseUriMaker();
           
            ConfigFormaters(config);

            appBuilder.UseWebApi(config);
        }


        private void ConfigFormaters(HttpConfiguration config)
        {
            config.Formatters.Remove(config.Formatters.XmlFormatter);
            var collectionFormatter = new CollectionJsonFormatter
            {
                SerializerSettings =
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    DefaultValueHandling = DefaultValueHandling.Ignore
                }
            };
            config.Formatters.Add(collectionFormatter);

            JsonSerializerSettings settings = config.Formatters.JsonFormatter.SerializerSettings;
            settings.NullValueHandling = NullValueHandling.Ignore;
            settings.DefaultValueHandling = DefaultValueHandling.Ignore;
            settings.Formatting = Formatting.Indented;
            settings.ContractResolver = new CamelCasePropertyNamesContractResolver();

           
            config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/vnd.siren+json"));
            config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/vnd.collection+json"));
            
        }

    }
}
