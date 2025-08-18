using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using FluentAssertions;
using RESTClient.NET.Core.Models;
using Xunit;

namespace RESTClient.NET.Core.Tests.Models
{
    public class ResponseContextTests
    {
        [Fact]
        public void StoreResponse_WithValidData_StoresSuccessfully()
        {
            // Arrange
            var context = new ResponseContext();
            var responseData = CreateTestResponseData();

            // Act
            context.StoreResponse("test-request", responseData);

            // Assert
            context.Count.Should().Be(1);
            context.HasResponse("test-request").Should().BeTrue();
        }

        [Fact]
        public void StoreResponse_WithNullRequestName_ThrowsArgumentException()
        {
            // Arrange
            var context = new ResponseContext();
            var responseData = CreateTestResponseData();

            // Act & Assert
            context.Invoking(c => c.StoreResponse(null!, responseData))
                .Should().Throw<ArgumentException>()
                .WithParameterName("requestName");
        }

        [Fact]
        public void StoreResponse_WithNullResponseData_ThrowsArgumentNullException()
        {
            // Arrange
            var context = new ResponseContext();

            // Act & Assert
            context.Invoking(c => c.StoreResponse("test", null!))
                .Should().Throw<ArgumentNullException>()
                .WithParameterName("responseData");
        }

        [Fact]
        public void GetResponse_WithExistingRequest_ReturnsResponse()
        {
            // Arrange
            var context = new ResponseContext();
            var responseData = CreateTestResponseData();
            context.StoreResponse("test-request", responseData);

            // Act
            var result = context.GetResponse("test-request");

            // Assert
            result.Should().NotBeNull();
            result.Should().BeSameAs(responseData);
        }

        [Fact]
        public void GetResponse_WithNonExistentRequest_ReturnsNull()
        {
            // Arrange
            var context = new ResponseContext();

            // Act
            var result = context.GetResponse("non-existent");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void HasResponse_WithExistingRequest_ReturnsTrue()
        {
            // Arrange
            var context = new ResponseContext();
            var responseData = CreateTestResponseData();
            context.StoreResponse("test-request", responseData);

            // Act
            var result = context.HasResponse("test-request");

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void HasResponse_WithNonExistentRequest_ReturnsFalse()
        {
            // Arrange
            var context = new ResponseContext();

            // Act
            var result = context.HasResponse("non-existent");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void RemoveResponse_WithExistingRequest_RemovesAndReturnsTrue()
        {
            // Arrange
            var context = new ResponseContext();
            var responseData = CreateTestResponseData();
            context.StoreResponse("test-request", responseData);

            // Act
            var result = context.RemoveResponse("test-request");

            // Assert
            result.Should().BeTrue();
            context.HasResponse("test-request").Should().BeFalse();
            context.Count.Should().Be(0);
        }

        [Fact]
        public void RemoveResponse_WithNonExistentRequest_ReturnsFalse()
        {
            // Arrange
            var context = new ResponseContext();

            // Act
            var result = context.RemoveResponse("non-existent");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void Clear_RemovesAllResponses()
        {
            // Arrange
            var context = new ResponseContext();
            context.StoreResponse("request1", CreateTestResponseData());
            context.StoreResponse("request2", CreateTestResponseData());

            // Act
            context.Clear();

            // Assert
            context.Count.Should().Be(0);
            context.HasResponse("request1").Should().BeFalse();
            context.HasResponse("request2").Should().BeFalse();
        }

        [Fact]
        public void RequestNames_ReturnsAllStoredRequestNames()
        {
            // Arrange
            var context = new ResponseContext();
            context.StoreResponse("request1", CreateTestResponseData());
            context.StoreResponse("request2", CreateTestResponseData());
            context.StoreResponse("request3", CreateTestResponseData());

            // Act
            var requestNames = context.RequestNames;

            // Assert
            requestNames.Should().HaveCount(3);
            requestNames.Should().Contain(new[] { "request1", "request2", "request3" });
        }

        [Fact]
        public void Clone_CreatesIndependentCopy()
        {
            // Arrange
            var original = new ResponseContext();
            var responseData1 = CreateTestResponseData();
            var responseData2 = CreateTestResponseData();
            original.StoreResponse("request1", responseData1);
            original.StoreResponse("request2", responseData2);

            // Act
            var clone = original.Clone();

            // Assert
            clone.Should().NotBeSameAs(original);
            clone.Count.Should().Be(original.Count);
            clone.HasResponse("request1").Should().BeTrue();
            clone.HasResponse("request2").Should().BeTrue();

            // Verify independence
            clone.StoreResponse("request3", CreateTestResponseData());
            original.HasResponse("request3").Should().BeFalse();
        }

        [Fact]
        public void Responses_ReturnsReadOnlyDictionary()
        {
            // Arrange
            var context = new ResponseContext();
            var responseData = CreateTestResponseData();
            context.StoreResponse("test-request", responseData);

            // Act
            var responses = context.Responses;

            // Assert
            responses.Should().HaveCount(1);
            responses.Should().ContainKey("test-request");
            responses["test-request"].Should().BeSameAs(responseData);
        }

        private static HttpResponseData CreateTestResponseData()
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            return HttpResponseData.FromHttpResponse(response, """{"test": "data"}""");
        }
    }
}
