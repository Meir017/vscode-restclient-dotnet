using AwesomeAssertions;
using RESTClient.NET.Core.Models;
using Xunit;

namespace RESTClient.NET.Core.Tests.Models
{
    public class HttpRequestMetadataTests
    {
        [Fact]
        public void Constructor_ShouldInitializeWithDefaults()
        {
            // Act
            var metadata = new HttpRequestMetadata();

            // Assert
            metadata.Name.Should().BeNull();
            metadata.Note.Should().BeNull();
            metadata.NoRedirect.Should().BeFalse();
            metadata.NoCookieJar.Should().BeFalse();
            metadata.CustomMetadata.Should().NotBeNull().And.BeEmpty();
            metadata.Expectations.Should().NotBeNull().And.BeEmpty();
        }

        [Fact]
        public void Properties_ShouldBeSettableAndGettable()
        {
            // Arrange
            var metadata = new HttpRequestMetadata
            {
                // Act
                Name = "test-request",
                Note = "This is a test request",
                NoRedirect = true,
                NoCookieJar = true
            };

            // Assert
            metadata.Name.Should().Be("test-request");
            metadata.Note.Should().Be("This is a test request");
            metadata.NoRedirect.Should().BeTrue();
            metadata.NoCookieJar.Should().BeTrue();
        }

        [Fact]
        public void CustomMetadata_ShouldAllowAddingKeyValuePairs()
        {
            // Arrange
            var metadata = new HttpRequestMetadata();

            // Act
            metadata.CustomMetadata["category"] = "authentication";
            metadata.CustomMetadata["priority"] = "high";
            metadata.CustomMetadata["team"] = "backend";

            // Assert
            metadata.CustomMetadata.Should().HaveCount(3);
            metadata.CustomMetadata["category"].Should().Be("authentication");
            metadata.CustomMetadata["priority"].Should().Be("high");
            metadata.CustomMetadata["team"].Should().Be("backend");
        }

        [Fact]
        public void AddExpectation_ShouldAddExpectationToList()
        {
            // Arrange
            var metadata = new HttpRequestMetadata();
            var expectation = new TestExpectation(ExpectationType.StatusCode, "200");

            // Act
            metadata.AddExpectation(expectation);

            // Assert
            metadata.Expectations.Should().HaveCount(1);
            metadata.Expectations[0].Should().Be(expectation);
        }

        [Fact]
        public void AddExpectation_WithMultipleExpectations_ShouldAddAllToList()
        {
            // Arrange
            var metadata = new HttpRequestMetadata();
            var expectation1 = new TestExpectation(ExpectationType.StatusCode, "200");
            var expectation2 = new TestExpectation(ExpectationType.Header, "application/json", "Content-Type");
            var expectation3 = new TestExpectation(ExpectationType.BodyContains, "success");

            // Act
            metadata.AddExpectation(expectation1);
            metadata.AddExpectation(expectation2);
            metadata.AddExpectation(expectation3);

            // Assert
            metadata.Expectations.Should().HaveCount(3);
            metadata.Expectations.Should().Contain(expectation1);
            metadata.Expectations.Should().Contain(expectation2);
            metadata.Expectations.Should().Contain(expectation3);
        }

        [Fact]
        public void GetExpectations_WithSpecificType_ShouldReturnMatchingExpectations()
        {
            // Arrange
            var metadata = new HttpRequestMetadata();
            var statusExpectation1 = new TestExpectation(ExpectationType.StatusCode, "200");
            var statusExpectation2 = new TestExpectation(ExpectationType.StatusCode, "201");
            var headerExpectation = new TestExpectation(ExpectationType.Header, "application/json", "Content-Type");

            metadata.AddExpectation(statusExpectation1);
            metadata.AddExpectation(headerExpectation);
            metadata.AddExpectation(statusExpectation2);

            // Act
            var statusExpectations = metadata.GetExpectations(ExpectationType.StatusCode).ToList();

            // Assert
            statusExpectations.Should().HaveCount(2);
            statusExpectations.Should().Contain(statusExpectation1);
            statusExpectations.Should().Contain(statusExpectation2);
            statusExpectations.Should().NotContain(headerExpectation);
        }

        [Fact]
        public void GetExpectations_WithNonExistentType_ShouldReturnEmpty()
        {
            // Arrange
            var metadata = new HttpRequestMetadata();
            var statusExpectation = new TestExpectation(ExpectationType.StatusCode, "200");
            metadata.AddExpectation(statusExpectation);

            // Act
            var headerExpectations = metadata.GetExpectations(ExpectationType.Header).ToList();

            // Assert
            headerExpectations.Should().BeEmpty();
        }

        [Fact]
        public void HasExpectations_WithNoExpectations_ShouldReturnFalse()
        {
            // Arrange
            var metadata = new HttpRequestMetadata();

            // Act
            bool result = metadata.HasExpectations();

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void HasExpectations_WithExpectations_ShouldReturnTrue()
        {
            // Arrange
            var metadata = new HttpRequestMetadata();
            var expectation = new TestExpectation(ExpectationType.StatusCode, "200");
            metadata.AddExpectation(expectation);

            // Act
            bool result = metadata.HasExpectations();

            // Assert
            result.Should().BeTrue();
        }

        [Theory]
        [InlineData(ExpectationType.StatusCode)]
        [InlineData(ExpectationType.Header)]
        [InlineData(ExpectationType.BodyContains)]
        [InlineData(ExpectationType.BodyPath)]
        [InlineData(ExpectationType.Schema)]
        [InlineData(ExpectationType.MaxTime)]
        public void GetExpectations_WithVariousTypes_ShouldReturnCorrectExpectations(ExpectationType type)
        {
            // Arrange
            var metadata = new HttpRequestMetadata();
            var expectation = new TestExpectation(type, "test-value");
            metadata.AddExpectation(expectation);

            // Act
            var result = metadata.GetExpectations(type).ToList();

            // Assert
            result.Should().HaveCount(1);
            result[0].Type.Should().Be(type);
        }

        [Fact]
        public void Name_ShouldAcceptNullAndStringValues()
        {
            // Arrange
            var metadata = new HttpRequestMetadata
            {
                // Act & Assert - null value
                Name = null
            };
            metadata.Name.Should().BeNull();

            // Act & Assert - string value
            metadata.Name = "test-request";
            metadata.Name.Should().Be("test-request");

            // Act & Assert - empty string
            metadata.Name = "";
            metadata.Name.Should().Be("");
        }

        [Fact]
        public void Note_ShouldAcceptNullAndStringValues()
        {
            // Arrange
            var metadata = new HttpRequestMetadata
            {
                // Act & Assert - null value
                Note = null
            };
            metadata.Note.Should().BeNull();

            // Act & Assert - string value
            metadata.Note = "This is a test note";
            metadata.Note.Should().Be("This is a test note");

            // Act & Assert - empty string
            metadata.Note = "";
            metadata.Note.Should().Be("");
        }

        [Fact]
        public void NoRedirect_ShouldBeBooleanProperty()
        {
            // Arrange
            var metadata = new HttpRequestMetadata();

            // Act & Assert - default false
            metadata.NoRedirect.Should().BeFalse();

            // Act & Assert - set to true
            metadata.NoRedirect = true;
            metadata.NoRedirect.Should().BeTrue();

            // Act & Assert - set back to false
            metadata.NoRedirect = false;
            metadata.NoRedirect.Should().BeFalse();
        }

        [Fact]
        public void NoCookieJar_ShouldBeBooleanProperty()
        {
            // Arrange
            var metadata = new HttpRequestMetadata();

            // Act & Assert - default false
            metadata.NoCookieJar.Should().BeFalse();

            // Act & Assert - set to true
            metadata.NoCookieJar = true;
            metadata.NoCookieJar.Should().BeTrue();

            // Act & Assert - set back to false
            metadata.NoCookieJar = false;
            metadata.NoCookieJar.Should().BeFalse();
        }

        [Fact]
        public void CustomMetadata_ShouldSupportModificationOperations()
        {
            // Arrange
            var metadata = new HttpRequestMetadata();

            // Act - Add items
            metadata.CustomMetadata["key1"] = "value1";
            metadata.CustomMetadata["key2"] = "value2";

            // Assert - Items added
            metadata.CustomMetadata.Should().HaveCount(2);

            // Act - Update item
            metadata.CustomMetadata["key1"] = "updated-value1";

            // Assert - Item updated
            metadata.CustomMetadata["key1"].Should().Be("updated-value1");
            metadata.CustomMetadata.Should().HaveCount(2);

            // Act - Remove item
            metadata.CustomMetadata.Remove("key2");

            // Assert - Item removed
            metadata.CustomMetadata.Should().HaveCount(1);
            metadata.CustomMetadata.Should().ContainKey("key1");
            metadata.CustomMetadata.Should().NotContainKey("key2");
        }

        [Fact]
        public void Expectations_ShouldSupportDirectAccess()
        {
            // Arrange
            var metadata = new HttpRequestMetadata();
            var expectation1 = new TestExpectation(ExpectationType.StatusCode, "200");
            var expectation2 = new TestExpectation(ExpectationType.Header, "application/json", "Content-Type");

            // Act
            metadata.Expectations.Add(expectation1);
            metadata.Expectations.Add(expectation2);

            // Assert
            metadata.Expectations.Should().HaveCount(2);
            metadata.Expectations[0].Should().Be(expectation1);
            metadata.Expectations[1].Should().Be(expectation2);
        }

        [Fact]
        public void GetExpectations_WithMixedTypes_ShouldFilterCorrectly()
        {
            // Arrange
            var metadata = new HttpRequestMetadata();
            var statusExpectation = new TestExpectation(ExpectationType.StatusCode, "200");
            var headerExpectation1 = new TestExpectation(ExpectationType.Header, "application/json", "Content-Type");
            var headerExpectation2 = new TestExpectation(ExpectationType.Header, "Bearer token", "Authorization");
            var bodyExpectation = new TestExpectation(ExpectationType.BodyContains, "success");

            metadata.AddExpectation(statusExpectation);
            metadata.AddExpectation(headerExpectation1);
            metadata.AddExpectation(bodyExpectation);
            metadata.AddExpectation(headerExpectation2);

            // Act
            var headerExpectations = metadata.GetExpectations(ExpectationType.Header).ToList();

            // Assert
            headerExpectations.Should().HaveCount(2);
            headerExpectations.Should().Contain(headerExpectation1);
            headerExpectations.Should().Contain(headerExpectation2);
            headerExpectations.Should().NotContain(statusExpectation);
            headerExpectations.Should().NotContain(bodyExpectation);
        }
    }
}
