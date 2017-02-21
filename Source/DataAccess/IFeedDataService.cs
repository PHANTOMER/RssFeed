using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Common;
using Core.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Core.DataAccess
{
    public interface IFeedDataService
    {
        Task<Rss> GetFeedAsync();

        Task<bool> UpdateFeedAsync(Feed feed);
    }

    class FeedDataService : IFeedDataService
    {
        private const string CRSS_VERSION = "2.0";

        private readonly IDbContext _feedContext;
        private readonly IConfigurationProvider _configuration;

        public FeedDataService(IDbContext feedContext, IConfigurationProvider configuration)
        {
            _feedContext = feedContext;
            _configuration = configuration;
        }

        public async Task<Rss> GetFeedAsync()
        {
            var feed = await _feedContext.GetAsync<Feed>(x => true, _configuration.AppSettings["FeedCollection"]);
            return new Rss()
            {
                Version = CRSS_VERSION,
                Feed = feed
            };
        }

        public async Task<bool> UpdateFeedAsync(Feed feed)
        {
            var fromDb = await _feedContext.GetAsync<Feed>(x => true, _configuration.AppSettings["FeedCollection"]);
            if (fromDb == null)
            {
                feed.Id = ObjectId.GenerateNewId();
                feed.Title = "Freelancer RSS";
                feed.Description = "RSS feed";
                feed.Link = "https://freelancer.com";
                if (feed.Items != null)
                {
                    foreach (var item in feed.Items)
                    {
                        item.CreatedDate = DateTime.UtcNow;
                    }
                }

                return await _feedContext.SaveAsync(feed, _configuration.AppSettings["FeedCollection"]);
            }

            bool itemsChanged = false;

            if (feed.Items != null)
            {
                if (fromDb.Items != null)
                {
                    var comparer = new GenericEqualityCompare<FeedItem, string>(x => x.Id, x => x.Id.GetHashCode());

                    List<FeedItem> resultItems = new List<FeedItem>();

                    var newItems = feed.Items.Except(fromDb.Items, comparer);
                    foreach (var item in newItems)
                    {
                        item.CreatedDate = DateTime.UtcNow;
                        resultItems.Add(item);
                    }

                    int oldCount = fromDb.Items.Length;

                    fromDb.Items = resultItems.Union(fromDb.Items.Except(resultItems, comparer)
                        .Where(x => DateTime.UtcNow.Subtract(x.CreatedDate) < TimeSpan.FromHours(12))).ToArray();

                    itemsChanged = oldCount != fromDb.Items.Length;
                }
                else
                {
                    itemsChanged = true;

                    foreach (var item in feed.Items)
                    {
                        item.CreatedDate = DateTime.UtcNow;
                    }

                    fromDb.Items = feed.Items;
                }
            }
            else if (fromDb.Items != null)
            {
                int oldCount = fromDb.Items.Length;
                fromDb.Items = fromDb.Items.Where(x => DateTime.UtcNow.Subtract(x.CreatedDate) < TimeSpan.FromHours(12)).ToArray();
                itemsChanged = oldCount > fromDb.Items.Length;
            }

            if (!itemsChanged)
                return await Task.FromResult(true);

            UpdateOneModel<Feed> update = new UpdateOneModel<Feed>(
                new FilterDefinitionBuilder<Feed>().Where(x => x.Id == fromDb.Id),
                new UpdateDefinitionBuilder<Feed>().Set(x => x.Items, fromDb.Items));

            return await _feedContext.UpdateAsync(update, _configuration.AppSettings["FeedCollection"]);
        }
    }

    class GenericEqualityCompare<T, TKey> : IEqualityComparer<T>
    {
        private readonly Func<T, TKey> _keySelector;
        private readonly Func<T, int> _hashCodeSelector;

        public GenericEqualityCompare(Func<T, TKey> keySelector, Func<T, int> hashCodeSelector)
        {
            _keySelector = keySelector;
            _hashCodeSelector = hashCodeSelector;
        }

        public bool Equals(T x, T y)
        {
            return _keySelector(x).Equals(_keySelector(y));
        }

        public int GetHashCode(T obj)
        {
            return _hashCodeSelector(obj);
        }
    }
}
