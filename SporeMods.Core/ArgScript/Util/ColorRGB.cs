namespace SporeMods.Core.ArgScript.Util
{
    public class ColorRGB
    {
        public float R { get; private set; }
        public float G { get; private set; }
        public float B { get; private set; }

        public ColorRGB(float r, float g, float b) {
            R = r;
            G = g;
            B = b;
        }
	
        public ColorRGB(int r, int g, int b) {
            R = r / 255.0f;
            G = g / 255.0f;
            B = b / 255.0f;
        }
        
        public ColorRGB(ColorRGB color) {
            Copy(color);
        }
        
        public static ColorRGB White() {
            return new ColorRGB(1.0f, 1.0f, 1.0f);
        }
	
        public static ColorRGB Black() {
            return new ColorRGB(0.0f, 0.0f, 0.0f);
        }
	
        public void Copy(ColorRGB color) {
            R = color.R;
            G = color.G;
            B = color.B;
        }
	
        public ColorRGB() {}
        
        public bool IsWhite() {
            return R == 1.0f && G == 1.0f && B == 1.0f;
        }
        public bool IsBlack() {
            return R == 0.0f && G == 0.0f && B == 0.0f;
        }

        public override string ToString()
        {
            return $"({R:#.#######}, {G:#.#######}, {B:#.#######})";
        }

        protected bool Equals(ColorRGB other)
        {
            return R.Equals(other.R) && G.Equals(other.G) && B.Equals(other.B);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ColorRGB) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = R.GetHashCode();
                hashCode = (hashCode * 397) ^ G.GetHashCode();
                hashCode = (hashCode * 397) ^ B.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(ColorRGB left, ColorRGB right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ColorRGB left, ColorRGB right)
        {
            return !Equals(left, right);
        }
    }
}