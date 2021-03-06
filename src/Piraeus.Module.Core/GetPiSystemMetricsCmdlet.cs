﻿using Piraeus.Core.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace Piraeus.Module
{
    [Cmdlet(VerbsCommon.Get, "PiraeusEventMetrics")]
    public class GetPiSystemMetricsCmdlet : Cmdlet
    {
        [Parameter(HelpMessage = "Url of the service.", Mandatory = true)]
        public string ServiceUrl;

        [Parameter(HelpMessage = "Security token used to access the REST service.", Mandatory = true)]
        public string SecurityToken;

        [Parameter(HelpMessage = "Unique URI identifier of resource.", Mandatory = true)]
        public string ResourceUriString;

        protected override void ProcessRecord()
        {
            string url = String.Format("{0}/api/resource/GetPiSystemMetrics?ResourceUriString={1}", ServiceUrl, ResourceUriString);
            RestRequestBuilder builder = new RestRequestBuilder("GET", url, RestConstants.ContentType.Json, true, SecurityToken);
            RestRequest request = new RestRequest(builder);


            CommunicationMetrics metrics = request.Get<CommunicationMetrics>();

            WriteObject(metrics);
        }
    }
}
