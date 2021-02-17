namespace SporeMods.Core.ArgScript.Util
{
    public class ColorRGBA
    {
        public float R { get; private set; }
        public float G { get; private set; }
        public float B { get; private set; }
        public float A { get; private set; }

        public ColorRGBA(float r, float g, float b, float a) {
            R = r;
            G = g;
            B = b;
            A = a;
        }
	
        public ColorRGBA(int r, int g, int b, int a) {
            R = r / 255.0f;
            G = g / 255.0f;
            B = b / 255.0f;
            A = a / 255.0f;
        }

        public ColorRGBA(int code) {
            A = ((code & 0xFF000000) >> 24) / 255.0f;
            R = ((code & 0x00FF0000) >> 16) / 255.0f;
            G = ((code & 0x0000FF00) >> 8) / 255.0f;
            B = ((code & 0x000000FF) >> 0) / 255.0f;
        }
	
        public ColorRGBA(ColorRGBA color) {
            Copy(color);
        }
	
        public void Copy(ColorRGBA color) {
            R = color.R;
            G = color.G;
            B = color.B;
            A = color.A;
        }
	
        public ColorRGBA() {}
        
        public static ColorRGBA White() {
            return new ColorRGBA(1.0f, 1.0f, 1.0f, 1.0f);
        }
	
        public static ColorRGBA Black() {
            return new ColorRGBA(0.0f, 0.0f, 0.0f, 1.0f);
        }
	
        public static ColorRGBA Empty() {
            return new ColorRGBA(0.0f, 0.0f, 0.0f, 0.0f);
        }
        
        public bool IsWhite() {
            return R == 1.0f && G == 1.0f && B == 1.0f;
        }
        public bool IsBlack() {
            return R == 0.0f && G == 0.0f && B == 0.0f;
        }
        
        public override string ToString()
        {
            return $"({R:#.#######}, {G:#.#######}, {B:#.#######}, {A:#.#######})";
        }

        protected bool Equals(ColorRGBA other)
        {
            return R.Equals(other.R) && G.Equals(other.G) && B.Equals(other.B) && A.Equals(other.A);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ColorRGBA) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = R.GetHashCode();
                hashCode = (hashCode * 397) ^ G.GetHashCode();
                hashCode = (hashCode * 397) ^ B.GetHashCode();
                hashCode = (hashCode * 397) ^ A.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(ColorRGBA left, ColorRGBA right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ColorRGBA left, ColorRGBA right)
        {
            return !Equals(left, right);
        }
    }
}