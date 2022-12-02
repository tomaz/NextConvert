using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace NextConvert.Sources.Data;

/// <summary>
/// Represents raw image with additional information
/// </summary>
public class ImageData
{
    /// <summary>
    /// The image itself.
    /// </summary>
#pragma warning disable CS8618 // We always assign image, but want to use property constructor                                                                 
    public Image<Argb32> Image { get; set; }
#pragma warning restore

    /// <summary>
    /// The position of the image block inside the source it was taken from. Positions use block-based coordinates - to get pixel coordinate multiply the value with image width and height.
    /// </summary>
    public Point Position { get; set; }

    /// <summary>
    /// Specifies whether the image is fully transparent or not.
    /// </summary>
    public bool IsTransparent { get; set; }

    /// <summary>
    /// Convenience for getting represented image width.
    /// </summary>
    public int Width { get => Image.Width; }

    /// <summary>
    /// Convenience for getting represented image height.
    /// </summary>
    public int Height { get => Image.Height; }

    /// <summary>
    /// Convenience for accessing represented image pixels.
    /// </summary>
    public Argb32 this[int x, int y] => Image[x, y];

    #region Overrides

    public override string ToString() => $"({Position.X},{Position.Y}){(IsTransparent ? "T" : "")}";

    #endregion

    #region Helpers

    public bool IsDuplicateOf(ImageData other)
    {
        for (int y = 0; y < Image.Height; y++)
        {
            for (int x = 0; x < Image.Width; x++)
            {
                if (Image[x, y] != other.Image[x, y]) return false;
            }
        }

        return true;
    }

    public bool IsHorizontalMirrorOf(ImageData other)
    {
        /* +---+---+---+    +---+---+---+
		 * | 1 | 2 | 3 |    | 3 | 2 | 1 |
		 * +---+---+---+    +---+---+---+
		 * | 4 | 5 | 6 | => | 6 | 5 | 4 |
		 * +---+---+---+    +---+---+---+
		 * | 7 | 8 | 9 |    | 9 | 8 | 7 |
		 * +---+---+---+    +---+---+---+
		 */
        var lastX = Image.Width - 1;

        for (int y = 0; y < Image.Height; y++)
        {
            for (int x = 0; x < Image.Width; x++)
            {
                if (Image[x, y] != other.Image[lastX - x, y]) return false;
            }
        }

        return true;
    }

    public bool IsVerticalMirrorOf(ImageData other)
    {
        /* +---+---+---+    +---+---+---+
		 * | 1 | 2 | 3 |    | 7 | 8 | 9 |
		 * +---+---+---+    +---+---+---+
		 * | 4 | 5 | 6 | => | 4 | 5 | 6 |
		 * +---+---+---+    +---+---+---+
		 * | 7 | 8 | 9 |    | 1 | 2 | 3 |
		 * +---+---+---+    +---+---+---+
		 */
        var lastY = Image.Height - 1;

        for (int y = 0; y < Image.Height; y++)
        {
            for (int x = 0; x < Image.Width; x++)
            {
                if (Image[x, y] != other.Image[x, lastY - y]) return false;
            }
        }

        return true;
    }

    public bool Is90CWRotationOf(ImageData other)
    {
        /* +---+---+---+    +---+---+---+
		 * | 1 | 2 | 3 |    | 7 | 4 | 1 |
		 * +---+---+---+    +---+---+---+
		 * | 4 | 5 | 6 | => | 8 | 5 | 2 |
		 * +---+---+---+    +---+---+---+
		 * | 7 | 8 | 9 |    | 9 | 6 | 3 |
		 * +---+---+---+    +---+---+---+
		 */
        var lastY = Image.Height - 1;

        for (int y = 0; y < Image.Height; y++)
        {
            for (int x = 0; x < Image.Width; x++)
            {
                if (Image[x, y] != other.Image[lastY - y, x]) return false;
            }
        }

        return true;
    }

    public bool Is90CCWRotationOf(ImageData other)
    {
        /* +---+---+---+    +---+---+---+
		 * | 1 | 2 | 3 |    | 3 | 6 | 9 |
		 * +---+---+---+    +---+---+---+
		 * | 4 | 5 | 6 | => | 2 | 5 | 8 |
		 * +---+---+---+    +---+---+---+
		 * | 7 | 8 | 9 |    | 1 | 4 | 7 |
		 * +---+---+---+    +---+---+---+
		 */
        var lastX = Image.Width - 1;

        for (int y = 0; y < Image.Height; y++)
        {
            for (int x = 0; x < Image.Width; x++)
            {
                if (Image[x, y] != other.Image[y, lastX - x]) return false;
            }
        }

        return true;
    }

    public bool Is180RotationOf(ImageData other)
    {
        /* +---+---+---+    +---+---+---+
		 * | 1 | 2 | 3 |    | 9 | 8 | 7 |
		 * +---+---+---+    +---+---+---+
		 * | 4 | 5 | 6 | => | 6 | 5 | 4 |
		 * +---+---+---+    +---+---+---+
		 * | 7 | 8 | 9 |    | 3 | 2 | 1 |
		 * +---+---+---+    +---+---+---+
		 */
        var lastX = Image.Width - 1;
        var lastY = Image.Height - 1;

        for (int y = 0; y < Image.Height; y++)
        {
            for (int x = 0; x < Image.Width; x++)
            {
                if (Image[x, y] != other.Image[lastX - x, lastY - y]) return false;
            }
        }

        return true;
    }

    #endregion
}
