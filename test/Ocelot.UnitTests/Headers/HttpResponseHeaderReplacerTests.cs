namespace Ocelot.UnitTests.Headers
{
    using Microsoft.AspNetCore.Http;
    using Ocelot.Infrastructure;
    using Ocelot.Middleware;
    using Ocelot.Infrastructure.RequestData;
    using Ocelot.Request.Middleware;
    using Xunit;
    using Shouldly;
    using TestStack.BDDfy;
    using System.Net.Http;
    using Ocelot.Headers;
    using Ocelot.Configuration;
    using System.Collections.Generic;
    using Ocelot.Responses;
    using System.Linq;
    using System.Net;
    using Moq;

    public class HttpResponseHeaderReplacerTests
    {
        private DownstreamResponse _response;
        private Placeholders _placeholders;
        private readonly HttpResponseHeaderReplacer _replacer;
        private List<HeaderFindAndReplace> _headerFindAndReplaces;
        private Response _result;
        private DownstreamRequest _request;
        private Mock<IBaseUrlFinder> _finder;
        private Mock<IRequestScopedDataRepository> _repo;
        private Mock<IHttpContextAccessor> _accessor;

        public HttpResponseHeaderReplacerTests()
        {
            _accessor = new Mock<IHttpContextAccessor>();
            _repo = new Mock<IRequestScopedDataRepository>();
            _finder = new Mock<IBaseUrlFinder>();
            _placeholders = new Placeholders(_finder.Object, _repo.Object, _accessor.Object);
            _replacer = new HttpResponseHeaderReplacer(_placeholders);
        }

        [Fact]
        public void should_replace_headers()
        {
            var response = new DownstreamResponse(new StringContent(string.Empty), HttpStatusCode.Accepted,
                new List<KeyValuePair<string, IEnumerable<string>>>()
                {
                    new KeyValuePair<string, IEnumerable<string>>("test", new List<string> {"test"})
                }, "");

            var fAndRs = new List<HeaderFindAndReplace> {new HeaderFindAndReplace("test", "test", "chiken", 0)};

            this.Given(x => GivenTheHttpResponse(response))
                .And(x => GivenTheFollowingHeaderReplacements(fAndRs))
                .When(x => WhenICallTheReplacer())
                .Then(x => ThenTheHeadersAreReplaced())
                .BDDfy();
        }

        [Fact]
        public void should_not_replace_headers()
        {
            var response = new DownstreamResponse(new StringContent(string.Empty), HttpStatusCode.Accepted,
                new List<KeyValuePair<string, IEnumerable<string>>>()
                {
                    new KeyValuePair<string, IEnumerable<string>>("test", new List<string> {"test"})
                }, "");

            var fAndRs = new List<HeaderFindAndReplace>();

            this.Given(x => GivenTheHttpResponse(response))
                .And(x => GivenTheFollowingHeaderReplacements(fAndRs))
                .When(x => WhenICallTheReplacer())
                .Then(x => ThenTheHeadersAreNotReplaced())
                .BDDfy();
        }

        [Fact]
        public void should_replace_downstream_base_url_with_ocelot_base_url()
        {
            const string downstreamUrl = "http://downstream.com/";

            var request =
                new HttpRequestMessage(HttpMethod.Get, "http://test.com") {RequestUri = new System.Uri(downstreamUrl)};

            var response = new DownstreamResponse(new StringContent(string.Empty), HttpStatusCode.Accepted,
                new List<KeyValuePair<string, IEnumerable<string>>>()
                {
                    new KeyValuePair<string, IEnumerable<string>>("Location", new List<string> {downstreamUrl})
                }, "");

            var fAndRs = new List<HeaderFindAndReplace>
            {
                new HeaderFindAndReplace("Location", "{DownstreamBaseUrl}", "http://ocelot.com/", 0)
            };

            this.Given(x => GivenTheHttpResponse(response))
                .And(x => GivenTheRequestIs(request))
                .And(x => GivenTheFollowingHeaderReplacements(fAndRs))
                .When(x => WhenICallTheReplacer())
                .Then(x => ThenTheHeaderShouldBe("Location", "http://ocelot.com/"))
                .BDDfy();
        }

        [Fact]
        public void should_replace_downstream_base_url_with_ocelot_base_url_with_port()
        {
            const string downstreamUrl = "http://downstream.com/";

            var request =
                new HttpRequestMessage(HttpMethod.Get, "http://test.com") {RequestUri = new System.Uri(downstreamUrl)};

            var response = new DownstreamResponse(new StringContent(string.Empty), HttpStatusCode.Accepted,
                new List<KeyValuePair<string, IEnumerable<string>>>()
                {
                    new KeyValuePair<string, IEnumerable<string>>("Location", new List<string> {downstreamUrl})
                }, "");

            var fAndRs = new List<HeaderFindAndReplace>
            {
                new HeaderFindAndReplace("Location", "{DownstreamBaseUrl}", "http://ocelot.com:123/", 0)
            };

            this.Given(x => GivenTheHttpResponse(response))
                .And(x => GivenTheRequestIs(request))
                .And(x => GivenTheFollowingHeaderReplacements(fAndRs))
                .When(x => WhenICallTheReplacer())
                .Then(x => ThenTheHeaderShouldBe("Location", "http://ocelot.com:123/"))
                .BDDfy();
        }

        [Fact]
        public void should_replace_downstream_base_url_with_ocelot_base_url_and_path()
        {
            const string downstreamUrl = "http://downstream.com/test/product";

            var request =
                new HttpRequestMessage(HttpMethod.Get, "http://test.com") {RequestUri = new System.Uri(downstreamUrl)};

            var response = new DownstreamResponse(new StringContent(string.Empty), HttpStatusCode.Accepted,
                new List<KeyValuePair<string, IEnumerable<string>>>()
                {
                    new KeyValuePair<string, IEnumerable<string>>("Location", new List<string> {downstreamUrl})
                }, "");

            var fAndRs = new List<HeaderFindAndReplace>
            {
                new HeaderFindAndReplace("Location", "{DownstreamBaseUrl}", "http://ocelot.com/", 0)
            };

            this.Given(x => GivenTheHttpResponse(response))
                .And(x => GivenTheRequestIs(request))
                .And(x => GivenTheFollowingHeaderReplacements(fAndRs))
                .When(x => WhenICallTheReplacer())
                .Then(x => ThenTheHeaderShouldBe("Location", "http://ocelot.com/test/product"))
                .BDDfy();
        }

        [Fact]
        public void should_replace_downstream_base_url_with_ocelot_base_url_with_path_and_port()
        {
            const string downstreamUrl = "http://downstream.com/test/product";

            var request =
                new HttpRequestMessage(HttpMethod.Get, "http://test.com") {RequestUri = new System.Uri(downstreamUrl)};

            var response = new DownstreamResponse(new StringContent(string.Empty), HttpStatusCode.Accepted,
                new List<KeyValuePair<string, IEnumerable<string>>>()
                {
                    new KeyValuePair<string, IEnumerable<string>>("Location", new List<string> {downstreamUrl})
                }, "");

            var fAndRs = new List<HeaderFindAndReplace>
            {
                new HeaderFindAndReplace("Location", "{DownstreamBaseUrl}", "http://ocelot.com:123/", 0)
            };

            this.Given(x => GivenTheHttpResponse(response))
                .And(x => GivenTheRequestIs(request))
                .And(x => GivenTheFollowingHeaderReplacements(fAndRs))
                .When(x => WhenICallTheReplacer())
                .Then(x => ThenTheHeaderShouldBe("Location", "http://ocelot.com:123/test/product"))
                .BDDfy();
        }

        [Fact]
        public void should_replace_downstream_base_url_and_port_with_ocelot_base_url()
        {
            const string downstreamUrl = "http://downstream.com:123/test/product";

            var request =
                new HttpRequestMessage(HttpMethod.Get, "http://test.com") {RequestUri = new System.Uri(downstreamUrl)};

            var response = new DownstreamResponse(new StringContent(string.Empty), HttpStatusCode.Accepted,
                new List<KeyValuePair<string, IEnumerable<string>>>()
                {
                    new KeyValuePair<string, IEnumerable<string>>("Location", new List<string> {downstreamUrl})
                }, "");

            var fAndRs = new List<HeaderFindAndReplace>
            {
                new HeaderFindAndReplace("Location", "{DownstreamBaseUrl}", "http://ocelot.com/", 0)
            };

            this.Given(x => GivenTheHttpResponse(response))
                .And(x => GivenTheRequestIs(request))
                .And(x => GivenTheFollowingHeaderReplacements(fAndRs))
                .When(x => WhenICallTheReplacer())
                .Then(x => ThenTheHeaderShouldBe("Location", "http://ocelot.com/test/product"))
                .BDDfy();
        }

        [Fact]
        public void should_replace_downstream_base_url_and_port_with_ocelot_base_url_and_port()
        {
            const string downstreamUrl = "http://downstream.com:123/test/product";

            var request =
                new HttpRequestMessage(HttpMethod.Get, "http://test.com") {RequestUri = new System.Uri(downstreamUrl)};

            var response = new DownstreamResponse(new StringContent(string.Empty), HttpStatusCode.Accepted,
                new List<KeyValuePair<string, IEnumerable<string>>>()
                {
                    new KeyValuePair<string, IEnumerable<string>>("Location", new List<string> {downstreamUrl})
                }, "");

            var fAndRs = new List<HeaderFindAndReplace>
            {
                new HeaderFindAndReplace("Location", "{DownstreamBaseUrl}", "http://ocelot.com:321/", 0)
            };

            this.Given(x => GivenTheHttpResponse(response))
                .And(x => GivenTheRequestIs(request))
                .And(x => GivenTheFollowingHeaderReplacements(fAndRs))
                .When(x => WhenICallTheReplacer())
                .Then(x => ThenTheHeaderShouldBe("Location", "http://ocelot.com:321/test/product"))
                .BDDfy();
        }

        private void GivenTheRequestIs(HttpRequestMessage request)
        {
            _request = new DownstreamRequest(request);
        }

        private void ThenTheHeadersAreNotReplaced()
        {
             _result.ShouldBeOfType<OkResponse>();
            foreach (var f in _headerFindAndReplaces)
            {
                var values = _response.Headers.First(x => x.Key == f.Key);
                values.Values.ToList()[f.Index].ShouldBe("test");
            }
        }

        private void GivenTheFollowingHeaderReplacements(List<HeaderFindAndReplace> fAndRs)
        {
            _headerFindAndReplaces = fAndRs;
        }

        private void GivenTheHttpResponse(DownstreamResponse response)
        {
            _response = response;
        }

        private void WhenICallTheReplacer()
        {
            var context = new DownstreamContext(new DefaultHttpContext()) {DownstreamResponse = _response, DownstreamRequest = _request};
            _result = _replacer.Replace(context, _headerFindAndReplaces);
        }

        private void ThenTheHeaderShouldBe(string key, string value)
        {
            var test = _response.Headers.First(x => x.Key == key);
            test.Values.First().ShouldBe(value);
        }

        private void ThenTheHeadersAreReplaced()
        {
            _result.ShouldBeOfType<OkResponse>();
            foreach (var f in _headerFindAndReplaces)
            {
                var values = _response.Headers.First(x => x.Key == f.Key);
                values.Values.ToList()[f.Index].ShouldBe(f.Replace);
            }
        }
    }
}
