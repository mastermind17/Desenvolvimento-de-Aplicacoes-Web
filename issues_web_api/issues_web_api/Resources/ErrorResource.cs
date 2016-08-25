using System.Collections.Generic;
using Newtonsoft.Json;

namespace issues_web_api.Resources
{
    /// <summary>
    /// This class should be used to represent an instance of
    /// a certain error. The HTTP response shouls use the mediatype
    /// defined by this class as well.
    /// </summary>
    public sealed class ErrorResource
    {
        /// <summary>
        /// The mediatype to be used when representing an error.
        /// </summary>
        public const string ErrorProblemMediaType = "application/problem+json";

        /// <summary>
        /// 
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Detail { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Instance { get; set; }

        [JsonProperty("invalid-params")]
        public List<InvalidParams> InvalidParames { get; set; }

        public class InvalidParams
        {
            public string Name { get; set; }
            public string Reason { get; set; }
        }
    }
}
