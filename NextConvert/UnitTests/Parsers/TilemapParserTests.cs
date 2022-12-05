using UnitTests.Helpers;

namespace UnitTests.Parsers;

public class TilemapParserTests
{
	[Fact]
	public void ShouldParseTestTilemap()
	{
		// setup
		var stream = ResourcesUtils.Tilemap.SourceTilemapStream();
		var parser = TestObjects.CreateTilemapParser();

		// execute
		var tilemap = parser.Parse(stream);

		// verify
		var expected = ResourcesUtils.Tilemap.ExpectedTilemapData();
		Assert.Equal(expected, tilemap, new TilemapComparer());
	}
}
