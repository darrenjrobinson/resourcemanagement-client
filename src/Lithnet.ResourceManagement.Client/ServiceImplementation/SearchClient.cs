﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading;
using Microsoft.ResourceManagement.WebServices.WSEnumeration;
using System.Xml;
using System.Globalization;

namespace Lithnet.ResourceManagement.Client.ResourceManagementService
{
    internal partial class SearchClient : System.ServiceModel.ClientBase<Lithnet.ResourceManagement.Client.ResourceManagementService.Search>, Lithnet.ResourceManagement.Client.ResourceManagementService.Search
    {
        private const int DefaultPageSize = 200;

        private ResourceManagementClient client;

        public void Initialize(ResourceManagementClient client)
        {
            this.DisableContextManager();
            this.client = client;
        }

        public ISearchResultCollection EnumerateAsync(string filter, int pageSize, IEnumerable<string> attributesToReturn, IEnumerable<SortingAttribute> sortingAttributes, CultureInfo locale, CancellationTokenSource cancellationToken)
        {
            return new SearchResultCollectionAsync(this.EnumeratePaged(filter, pageSize, attributesToReturn, sortingAttributes, locale), cancellationToken);
        }

        public ISearchResultCollection EnumerateSync(string filter, int pageSize, IEnumerable<string> attributesToReturn, IEnumerable<SortingAttribute> sortingAttributes, CultureInfo locale)
        {
            return new SearchResultCollection(this.EnumeratePaged(filter, pageSize, attributesToReturn, sortingAttributes, locale));
        }

        public SearchResultPager EnumeratePaged(string filter, int pageSize, IEnumerable<string> attributesToReturn, IEnumerable<SortingAttribute> sortingAttributes, CultureInfo locale)
        {
            if (pageSize < 0)
            {
                pageSize = DefaultPageSize;
            }

            var response = this.Enumerate(filter, 0, attributesToReturn, sortingAttributes, locale);
            return new SearchResultPager(response, pageSize, this, this.client, locale);
        }
       
        private EnumerateResponse Enumerate(string filter, int pageSize, IEnumerable<string> attributesToReturn, IEnumerable<SortingAttribute> sortingAttributes, CultureInfo locale)
        {
            if (pageSize < 0)
            {
                pageSize = DefaultPageSize;
            }

            using (Message requestMessage = MessageComposer.CreateEnumerateMessage(filter, pageSize, attributesToReturn, sortingAttributes, locale))
            {
                using (Message responseMessage = this.Invoke((c) => c.Enumerate(requestMessage)))
                {
                    responseMessage.ThrowOnFault();

                    EnumerateResponse response = responseMessage.DeserializeMessageWithPayload<EnumerateResponse>();
                    return response;
                }
            }
        }

        internal PullResponse Pull(EnumerationContextType context, int pageSize)
        {
            using (Message pullRequest = MessageComposer.GeneratePullMessage(context, pageSize))
            {
                using (Message responseMessage = this.Invoke((c) => c.Pull(pullRequest)))
                {
                    responseMessage.ThrowOnFault();

                    PullResponse pullResponseTyped = responseMessage.DeserializeMessageWithPayload<PullResponse>();
                    return pullResponseTyped;
                }
            }
        }

        internal void Release(EnumerationContextType context)
        {
            using (Message releaseRequest = MessageComposer.GenerateReleaseMessage(context))
            {
                using (Message responseMessage = Release(releaseRequest))
                {
                    releaseRequest.ThrowOnFault();
                }
            }
        }
    }
}