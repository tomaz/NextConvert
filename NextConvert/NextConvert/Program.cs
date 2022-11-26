using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");

using (var image = new Image<Rgb24>(Configuration.Default, 32, 32))
{
	image.Mutate(x =>
	{
		x.Fill(Color.White, new RectangleF(0, 0, 32, 32));

		x.DrawLines(new Pen(Color.Red, 1), new PointF[] { new PointF(0, 10), new PointF(10, 10) });

		x.Fill(Color.RebeccaPurple, new RectangleF(0, 0, 1, 1));
		x.Fill(Color.RebeccaPurple, new RectangleF(0, 1, 1, 1));
		x.Fill(Color.RebeccaPurple, new RectangleF(0, 2, 1, 1));
	});

	image.SaveAsBmp("result.bmp");
}

using (var image = Image.Load<Rgb24>("result.bmp"))
{
	Console.WriteLine($"w={image.Width} h={image.Height}");

	image.ProcessPixelRows(accessor =>
	{
		for (int y = 0; y < accessor.Height; y++) {
			Span<Rgb24> row = accessor.GetRowSpan(y);

			for (int x = 0; x < row.Length; x++)
			{
				ref Rgb24 pixel = ref row[x];

				if (x == 0)
				{
					Console.WriteLine($"({x},{y}): r={pixel.R} g={pixel.G} b={pixel.B}");
				}
			}
		}
	});
}