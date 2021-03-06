﻿using System.Collections.Generic;
using System.IO;
using System.Text;
using CommandRouting.Router;
using CommandRouting.Router.ValueParsers;
using FluentAssertions;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Internal;
using Microsoft.AspNet.Mvc.Formatters;
using Microsoft.AspNet.Routing;
using Xunit;

namespace CommandRouting.UnitTests.Router
{
    public class RequestModelActivatorTests
    {
        public class Foo
        {
            public string Name { get; set; }
            public int Ranking { get; set; }
        }

        [Fact]
        public void CreateRequestModel_should_create_a_command_request_model_from_the_http_request_body()
        {
            // Given an HttpRequest with a json message in the body
            HttpContext httpContext = new DefaultHttpContext();
            httpContext.Request.Method = "POST";
            httpContext.Request.ContentType = "application/json";
            httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes("{ name: 'Bar', ranking: 10 }"));            

            // Plus some empty route data
            RouteData routeData = new RouteData();

            // When I try to bind to a request model
            IInputFormatter inputFormatter = new JsonInputFormatter();
            IEnumerable<IValueParser> valueParsers = new List<IValueParser> { new RouteValueParser(routeData) };
            RequestModelActivator modelActivator = new RequestModelActivator(httpContext, inputFormatter, valueParsers);
            var requestModel = modelActivator.CreateRequestModelAsync<Foo>().Result;

            // Then the result should be an instance of Foo with all of it's properties set correctly
            Foo result = requestModel;
            Assert.Equal("Bar", result?.Name);
            Assert.Equal(10, result?.Ranking);
        }

        [Fact]
        public void BindRequestParameters_should_create_a_command_request_model_from_parameters_in_route_template()
        {
            // Given an HttpRequest with an empty body
            HttpContext httpContext = new DefaultHttpContext();
            httpContext.Request.Method = "GET";
            httpContext.Request.ContentType = "application/json";
            httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(""));

            // And some model properties in the route data
            RouteData routeData = new RouteData();
            routeData.Values.Add("name", "Bar");
            routeData.Values.Add("Ranking", "10");

            // When I try to bind to a request model
            IInputFormatter inputFormatter = new JsonInputFormatter();
            IEnumerable<IValueParser> valueParsers = new List<IValueParser> { new RouteValueParser(routeData) };
            RequestModelActivator modelActivator = new RequestModelActivator(httpContext, inputFormatter, valueParsers);
            var requestModel = modelActivator.CreateRequestModelAsync<Foo>().Result;

            // Then the result should be an instance of Foo with all of it's properties set correctly
            Foo result = requestModel;
            Assert.Equal("Bar", result?.Name);
            Assert.Equal(10, result?.Ranking);
        }

        [Fact]
        public void CreateRequestModel_should_merge_message_body_and_route_data_to_create_a_command_request_model()
        {
            // Given an HttpRequest with a json message in the body
            HttpContext httpContext = new DefaultHttpContext();
            httpContext.Request.Method = "POST";
            httpContext.Request.ContentType = "application/json";
            httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes("{ name: 'Bar', ranking: 10 }"));

            // Plus some route data
            RouteData routeData = new RouteData();
            routeData.Values.Add("Ranking", "42");

            // When I try to bind to a request model
            IInputFormatter inputFormatter = new JsonInputFormatter();
            IEnumerable<IValueParser> valueParsers = new List<IValueParser> { new RouteValueParser(routeData) };
            RequestModelActivator modelActivator = new RequestModelActivator(httpContext, inputFormatter, valueParsers);
            var requestModel = modelActivator.CreateRequestModelAsync<Foo>().Result;

            // Then the result should be an instance of Foo with all of it's properties set correctly
            // from a combination of the message body and the route data
            Foo result = requestModel;
            result?.Name.Should().Be("Bar");
            result?.Ranking.Should().Be(42);
        }

    }
}
