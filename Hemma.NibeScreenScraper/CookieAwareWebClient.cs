using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Hemma.NibeScreenScraper
{
    public class CookieAwareWebClient : WebClient
    {
        private int? _timeout = null;
        public int? Timeout
        {
            get
            {
                return _timeout;
            }
            set
            {
                _timeout = value;
            }
        }

        //A CookieContainer class to house the Cookie once it is contained within one of the Requests
        public CookieContainer CookieContainer { get; private set; }

        //Constructor
        public CookieAwareWebClient()
        {
            CookieContainer = new CookieContainer();
        }

        /// <summary>
        /// Method to handle setting the optional timeout (in milliseconds)
        /// </summary>
        public void SetTimeout(int timeout)
        {
            _timeout = timeout;
        }

        /// <summary>
        /// This handles using and storing the Cookie information as well as managing the Request timeout
        /// </summary>
        protected override WebRequest GetWebRequest(Uri address)
        {
            //Handles the CookieContainer
            var request = (HttpWebRequest)base.GetWebRequest(address);

            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            request.CookieContainer = CookieContainer;
            //Sets the Timeout if it exists
            if (_timeout.HasValue)
            {
                request.Timeout = _timeout.Value;
            }
            return request;
        }
    }
}
