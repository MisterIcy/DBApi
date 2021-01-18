using DBApi.Attributes;
using DBApi.Reflection;
using Xunit;

namespace OmegaTests
{
    public class AttributeTests
    {
        [Fact]
        public void TestEntityAttribute()
        {
            var metadata = new ClassMetadata(typeof(TestEntity));
            Assert.Equal(typeof(TestEntity), metadata.EntityType);
        }

        [Fact]
        public void TestNonEntityAttribute()
        {
            Assert.Throws<MetadataException>(() =>
            {
                var metadata = new ClassMetadata(typeof(string));
            });
        }
        [Fact]
        public void TestTableAttribute()
        {
            var metadata = new ClassMetadata(typeof(TestEntity));
            Assert.Equal("TestEntity", metadata.TableName);
        }

        [Fact]
        public void TestMissingTableAttribute()
        {
            Assert.Throws<MetadataException>(() =>
            {
                var metadata = new ClassMetadata(typeof(TestEntityWithOutTable));
            });
        }
        [Fact]
        public void TestMissingIdentifierAttribute()
        {
            Assert.Throws<MetadataException>(() =>
            {
                var metadata = new ClassMetadata(typeof(TestEntityWithoutIdentifier));
            });
        }
    }

    [Entity]
    internal class TestEntityWithOutTable
    {
        
    }
    [Entity]
    [Table("NoIdent")]
    internal class TestEntityWithoutIdentifier
    {
        
    }
}