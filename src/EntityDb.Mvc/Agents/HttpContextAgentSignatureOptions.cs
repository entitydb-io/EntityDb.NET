using System;

namespace EntityDb.Mvc.Agents
{
    /// <summary>
    ///     Configuration options for the Http Context agent.
    /// </summary>
    public class HttpContextAgentSignatureOptions
    {
        /// <summary>
        ///     If there is a header whose value is sensitive and should not be recorded in the
        ///     agent signature, put it in this array.
        /// </summary>
        public string[] DoNotRecordHeaders { get; set; } = Array.Empty<string>();
    }
}
