using System;

namespace cloudscribe.Web.SiteMap
{
    /// <summary>
    /// default implementation of ISiteMapNode in case you need it
    /// myself I see SiteMap as very related to navigation in terms of the data needed
    /// so I want to implement a class in my projects that implements both ISiteMapNode and INavigationNode
    /// and perhaps other interfaces for other purposes
    /// </summary>
    public class SiteMapNode : ISiteMapNode
    {
        public SiteMapNode(string url)
        {
            if (url == null)
            {
                throw new ArgumentNullException(nameof(url));
            }

            Url = url;
        }


        /// <summary>
        /// Gets or sets the URL of the page. This URL must begin with the protocol (such as http). 
        /// This value must be less than 2,048 characters.
        /// </summary>
        /// <value>
        /// The URL of the page.
        /// </value>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets how frequently the page is likely to change. This value provides general information to search
        /// engines and may not correlate exactly to how often they crawl the page.
        /// Please note that this value is considered a hint and not a command. Even though search engine
        /// crawlers may consider this information when making decisions, they may crawl pages marked "hourly" less
        /// frequently than that, and they may crawl pages marked "yearly" more frequently than that. Crawlers may
        /// periodically crawl pages marked "never" so that they can handle unexpected changes to those pages.
        /// </summary>
        /// <value>
        /// The frequency the page is likely to change.
        /// </value>
        public PageChangeFrequency? ChangeFrequency { get; set; }

        /// <summary>
        /// Gets or sets the priority of this URL relative to other URLs on your site. Valid values range from 0.0 to 1.0.
        /// This field is optional, if it is <c>null</c> the default priority of a page is 0.5. This value does not
        /// affect how your pages are compared to pages on other sites, it only lets the search engines know which
        /// pages you deem most important for the crawlers.
        /// Please note that the priority you assign to a page is not likely to influence the position of your URLs in
        /// a search engine's result pages. Search engines may use this information when selecting between URLs on the
        /// same site, so you can use this tag to increase the likelihood that your most important pages are present
        /// in a search index.
        /// Also, please note that assigning a high priority to all of the URLs on your site is not likely to help you.
        /// Since the priority is relative, it is only used to select between URLs on your site.
        /// </summary>
        /// <value>The priority.</value>
        public double? Priority { get; set; }

        /// <summary>
        /// Gets or sets the date of last modification of the file. This is an optional field.
        /// You may omit the time portion if desired.
        /// Note that this is separate from the If-Modified-Since (304) HTTP header the server can return,
        /// and search engines may use the information from both sources differently.
        /// </summary>
        /// <value>
        /// The date of last modification of the file.
        /// </value>
        public DateTime? LastModified { get; set; }

    }
}
