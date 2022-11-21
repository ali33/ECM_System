using System.Runtime.InteropServices;

namespace Ecm.Utility.Victor
{
    /// <summary>
    /// This class wraps methods in vic32.dll which is a unmanaged dll into methods which are used by C# code
    /// </summary>
    public class VicWin
    {
        // Fields
        public const int BAD_BMP = -27;
        public const int BAD_BPP = -26;
        public const int BAD_BPS = -8;
        public const int BAD_CMP = -9;
        public const int BAD_CRT = -10;
        public const int BAD_DATA = -62;
        public const int BAD_DIB = -12;
        public const int BAD_DIGI_MEM = -72;
        public const int BAD_DIM = -73;
        public const int BAD_DSK = -3;
        public const int BAD_FAC = -5;
        public const int BAD_FTPE = -11;
        public const int BAD_GIF = -17;
        public const int BAD_HANDLE = -68;
        public const int BAD_IBUF = -42;
        public const int BAD_JPEG = -43;
        public const int BAD_LOCK = -40;
        public const int BAD_MEM = -14;
        public const int BAD_OPN = -4;
        public const int BAD_PCX = -16;
        public const int BAD_PIW = -15;
        public const int BAD_PNG = -63;
        public const int BAD_PNG_CMP = -64;
        public const int BAD_PTR = -52;
        public const int BAD_RANGE = -1;
        public const int BAD_TGA = -25;
        public const int BAD_TIFF = -6;
        public const int BAD_TN_SIZE = -70;
        public const int BMPRKE = 1;
        public const int BMPUNC = 0;
        public const int COLORDITHER16 = 0;
        public const int COLORDITHER256 = 1;
        public const int COLORSCATTER16 = 0;
        public const int COLORSCATTER256 = 1;
        public const int CR_HIGH = 3;
        public const int CR_LOW = 0;
        public const int CR_OCTREEDIFF = 1;
        public const int CR_OCTREENODIFF = 0;
        public const int CR_TSDDIFF = 3;
        public const int CR_TSDNODIFF = 2;
        public const int GIF_NOFRAME = -71;
        public const int GIFINTERLACE = 1;
        public const int GIFLZWCOMP = 0;
        public const int GIFNOCOMP = 8;
        public const int GIFTRANSPARENT = 2;
        public const int GIFWRITE4BIT = 4;
        public const int JPG_BAD_BLOCKNO = -107;
        public const int JPG_BAD_BPPIXEL = -108;
        public const int JPG_BAD_COMPINFO = -106;
        public const int JPG_BAD_COMPNO = -109;
        public const int JPG_BAD_EOF = -101;
        public const int JPG_BAD_EOI = -111;
        public const int JPG_BAD_FTYPE = -110;
        public const int JPG_BAD_JFIF = -112;
        public const int JPG_BAD_MEM = -114;
        public const int JPG_BAD_PRECISION = -100;
        public const int JPG_BAD_RESTART = -102;
        public const int JPG_BAD_SCAN_PARAM = -113;
        public const int JPG_INVALID_DATA = -105;
        public const int JPG_INVALID_MARKER = -103;
        public const int JPG_NO_DISK_SPACE = -115;
        public const int JPG_READ_ERR = -104;
        public const int JPGPROG = 1;
        public const int JPGSEQ = 0;
        public const int JPGSEQOPT = 2;
        public const int LZW_DISABLED = -53;
        public const int NO_ACK = -65;
        public const int NO_DEV_DATA = -33;
        public const int NO_DIG = -2;
        public const int NO_ERROR = 0;
        public const int NOT_AVAIL = -50;
        public const int PALETTEIMAGEASGRAY = 0;
        public const int PALETTEIMAGEASRGB = 1;
        public const int PIWRLE = 1;
        public const int PIWUNC = 0;
        public const int PNG_ERR_BAD_CRC = -111;
        public const int PNG_ERR_BAD_SIG = -110;
        public const int PNG_ERR_COMPRESSION = -116;
        public const int PNG_ERR_DECOMPRESSION = -115;
        public const int PNG_ERR_EARLY_EOF = -113;
        public const int PNG_ERR_IMAGE_SIZE = -109;
        public const int PNG_ERR_INV_BITCOL = -105;
        public const int PNG_ERR_INV_BITDEPTH = -103;
        public const int PNG_ERR_INV_COLORTYPE = -104;
        public const int PNG_ERR_INV_COMP = -107;
        public const int PNG_ERR_INV_FILTER = -108;
        public const int PNG_ERR_INV_IHDR_CHK = -102;
        public const int PNG_ERR_INV_INTERLACE = -106;
        public const int PNG_ERR_MEM_ERR = -114;
        public const int PNG_ERR_NO_DISK_SPACE = -117;
        public const int PNG_ERR_TOO_FEW_IDATS = -101;
        public const int PNG_ERR_TOO_MUCH_DATA = -112;
        public const int PNG_ERR_UNK_CRIT_CHK = -100;
        public const int PNGALLFILTER = 0;
        public const int PNGALLFILTERS = 0;
        public const int PNGAVGFILTER = 8;
        public const int PNGINTERLACE = 1;
        public const int PNGNOFILTER = 2;
        public const int PNGPAETHFILTER = 10;
        public const int PNGSUBFILTER = 4;
        public const int PNGUPFILTER = 6;
        public const int PRT_BUSY = -41;
        public const int PRT_ERR = -18;
        public const int PRTDEFAULT = 0;
        public const int PRTHALFTONE = 1;
        public const int PRTSCATTER = 2;
        public const int RESIZEBILINEAR = 1;
        public const int RESIZEFAST = 0;
        public const int SCAN_ERR = -19;
        public const int SCAN_LIDUP = -46;
        public const int SCAN_NOADF = -48;
        public const int SCAN_NOPAPER = -47;
        public const int SCAN_NOTREADY = -49;
        public const int SCAN_UNLOAD = -45;
        public const int TGARLE = 1;
        public const int TGAUNC = 0;
        public const short THUMB_CODE_JPEG = 0x10;
        public const short THUMB_CODE_PAL = 0x11;
        public const short THUMB_CODE_RGB = 0x13;
        public const int TIF_BAD_EOF = -102;
        public const int TIF_G4_COMPLEX = -103;
        public const int TIF_INVALID_DATA = -100;
        public const int TIF_READ_ERR = -101;
        public const int TIFF_MOTYPE = -69;
        public const int TIFF_NOPAGE = -51;
        public const int TIFG3 = 3;
        public const int TIFG4 = 4;
        public const int TIFLZW = 1;
        public const int TIFPB = 2;
        public const int TIFUNC = 0;
        public const int TIMEOUT = -34;
        public const int TOO_CPLX = -44;
        public const int TW_DONTCARE = 0xffff;
        public const int TWAIN_BAD_DATATYPE = -59;
        public const int TWAIN_BUSY = -61;
        public const int TWAIN_ERR = -57;
        public const int TWAIN_NO_PAPER = -66;
        public const int TWAIN_NODS = -56;
        public const int TWAIN_NODSM = -55;
        public const int TWAIN_NOMATCH = -58;
        public const int TWAIN_NOWND = -54;
        public const int TWAIN_SCAN_CANCEL = -60;
        public const int TWAIN_STOP_SCAN = -67;
        public const int TWON_ARRAY = 3;
        public const int TWON_ENUMERATION = 4;
        public const int TWON_ONEVALUE = 5;
        public const int TWON_RANGE = 6;
        public const int TWPT_BW = 0;
        public const int TWPT_CIEXYZ = 8;
        public const int TWPT_CMY = 4;
        public const int TWPT_CMYK = 5;
        public const int TWPT_GRAY = 1;
        public const int TWPT_PALETTE = 3;
        public const int TWPT_RGB = 2;
        public const int TWPT_YUV = 6;
        public const int TWPT_YUVK = 7;
        public const int TWUN_CENTIMETERS = 1;
        public const int TWUN_INCHES = 0;
        public const int TWUN_PICAS = 2;
        public const int TWUN_PIXELS = 5;
        public const int TWUN_POINTS = 3;
        public const int TWUN_TWIPS = 4;
        public const int VER_PLATFORM_WIN32_NT = 2;
        public const int VER_PLATFORM_WIN32_WINDOWS = 1;
        public const int VER_PLATFORM_WIN32s = 0;
        public const int VIC_VS_EVAL_LIB = 8;
        public const int VIC_VS_RELEASE = 4;
        public const int VIC_VS_STATIC_RTL = 1;
        public const int VIC_VS_THREAD_SAFE = 2;
        public const int VIEWDITHER = 1;
        public const int VIEWOPTPAL = 0;
        public const int VIEWSCATTER = 2;


        public const int TWCC_SUCCESS = 0; //	Operation successful
        public const int TWCC_BUMMER = 1; // 	Failure due to unknown causes
        public const int TWCC_LOWMEMORY = 2; // Not enough memory to perform operation
        public const int TWCC_NODS = 3 ; //	No Data Source
        public const int TWCC_MAXCONNECTIONS = 4; //	Source already in use
        public const int TWCC_OPERATIONERROR = 5; //	Source or Source Manager error already reported to user
        public const int TWCC_BADCAP = 6; //	Unknown capability requested
        public const int TWCC_BADPROTOCOL = 9; //	Unrecognized DataGroup / Data ArgType / Msg combination
        public const int TWCC_BADVALUE = 10; //	Parameter out of range
        public const int TWCC_SEQERROR = 11; //	Message received out of sequence
        public const int TWCC_BADDEST = 12; //	Unknown destination App/Src in DSM_Entry

        // Methods
        public VicWin()
        {
        }

        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int addimage(ref imgdes srcimg, ref imgdes oprimg, ref imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int addnoise(int emount, ref imgdes srcimg, ref imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int addtext(int color, int pointsize, [MarshalAs(UnmanagedType.VBByRefStr)] ref string font, [MarshalAs(UnmanagedType.VBByRefStr)] ref string atext, ref imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int addtextex(int color, ref LOGFONT lfont, [MarshalAs(UnmanagedType.VBByRefStr)] ref string atext, ref imgdes resimg);
        [DllImport("VICFX.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int addtexture(ref imgdes srcimg, ref imgdes oprimg, ref imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int allocDIB(ref imgdes image, int wid, int leng, int BPPixel);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int allocimage(ref imgdes image, int wid, int leng, int BPPixel);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int andimage(ref imgdes srcimg, ref imgdes oprimg, ref imgdes resimg);
        [DllImport("VICFX.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int bleach(ref imgdes srcimg, ref imgdes oprimg, ref imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int blur(ref imgdes srcimg, ref imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int blurthresh(int thres, ref imgdes srcimg, ref imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int bmpinfo([MarshalAs(UnmanagedType.VBByRefStr)] ref string filename, ref BITMAPINFOHEADER bdat);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int bmpinfofrombuffer(ref byte buffer, ref BITMAPINFOHEADER bminfo);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int brightenmidrange(ref imgdes srcimg, ref imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int calcavglevel(ref imgdes srcimg, ref int redavg, ref int grnavg, ref int bluavg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int calchisto(ref imgdes srcimg, ref int redtab, ref int grntab, ref int blutab);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int calchistorgb(ref imgdes srcimg, ref int redtab, ref int grntab, ref int blutab, int mode);
        [DllImport("vic32.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int calcminmax(ref imgdes srcimg, ref MINMAX redMinmax, ref MINMAX grnMinmax, ref MINMAX bluMinmax);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int changebright(int amt, ref imgdes srcimg, ref imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int clienttoimage(int hWnd, ref imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int cmykimagetorgbimage(ref imgdes srcimg, ref imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int colordither(ref imgdes srcimg, ref imgdes resimg, int colormode);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int colorscatter(ref imgdes srcimg, ref imgdes resimg, int colormode);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int colortogray(ref imgdes srcimg, ref imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int convert1bitto8bit(ref imgdes srcimg, ref imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int convert1bitto8bitsmooth(ref imgdes srcimg, ref imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int convert8bitto1bit(int mode, ref imgdes srcimg, ref imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int convert8bitto1bitex(int conmode, ref imgdes srcimg, ref imgdes resimg, ref byte dithermatrix_firstelem);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int convertgray16to8(ref imgdes srcimg, ref imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern long convertgray32to16(long min, long max, imgdes srcimg, imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern long convertgray32to8(long min, long max, imgdes srcimg, imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int convertgray8to16(ref imgdes srcimg, ref imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int convertpaltorgb(ref imgdes srcimg, ref imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int convertrgbtopal(int palcolors, ref imgdes srcimg, ref imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int convertrgbtopalex(int palcolors, ref imgdes srcimg, ref imgdes resimg, int mode);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int copyimage(ref imgdes srcimg, ref imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int copyimagebits(ref imgdes srcimg, ref imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern void copyimagepalette(ref imgdes srcimg, ref imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern void copyimgdes(ref imgdes srcimg, ref imgdes resimg);
        [DllImport("VICSTATS.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int correlateimages(ref imgdes srcimg, ref imgdes oprimg, ref imgdes resimg);
        [DllImport("VICSTATS.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int correlationcoef(ref imgdes srcimg, ref imgdes oprimg, ref double coefficient);
        [DllImport("VICSTATS.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int correlationcoefRGB(ref imgdes srcimg, ref imgdes oprimg, ref double redcoefficient, ref double grncoefficient, ref double blucoefficient);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int cover(int thres, ref imgdes srcimg, ref imgdes oprimg, ref imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int coverclear(int transColor, ref imgdes srcimg, ref imgdes oprimg, ref imgdes resimg);
        [DllImport("VICFX.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int darker(ref imgdes srcimg, ref imgdes oprimg, ref imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int ddbtoimage(int hBitmap, int hPal, ref imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int defaultpalette(ref imgdes image);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int dibsecttoimage(int hBitmap, ref imgdes image);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int dibtobitmap(int hdc, int dib, ref int hBitmap);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int dibtoimage(int dib, ref imgdes resimg);
        [DllImport("VICFX.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int difference(ref imgdes srcimg, ref imgdes oprimg, ref imgdes resimg);
        [DllImport("VICFX.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int differenceabs(ref imgdes srcimg, ref imgdes oprimg, ref imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int dilate(int amount, ref imgdes srcimg, ref imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int displace(int tile, ref imgdes srcimg, ref imgdes oprimg, ref imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int divide(int divsr, ref imgdes srcimg, ref imgdes resimg);
        [DllImport("VICFX.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int divideimage(ref imgdes srcimg, ref imgdes oprimg, ref imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int drawhisto(int hdc, ref RECT RECT, int BPPixel, ref int redtab, ref int grntab, ref int blutab);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int dropbits(int numbits, ref imgdes srcimg, ref imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int emboss(ref imgdes srcimg, ref imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int embossongray(ref imgdes srcimg, ref imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int erode(int amount, ref imgdes srcimg, ref imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int exchangelevel(int min, int max, int newval, ref imgdes srcimg, ref imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int expandcontrast(int min, int max, ref imgdes srcimg, ref imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int extractcolorrange(RGBTRIPLE rgbcolor, int extent, ref imgdes srcimg, ref imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int extractplane(int plane, ref imgdes srcimg, ref imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int flipimage(ref imgdes srcimg, ref imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int freebuffer(int buffaddr);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern void freeimage(ref imgdes image);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int gammabrighten(double amt, ref imgdes srcimg, ref imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int gaussianblur(int ksize, ref imgdes srcimg, ref imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int getbuffersize(int buffaddr);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int getgifcomment([MarshalAs(UnmanagedType.VBByRefStr)] ref string filename, [MarshalAs(UnmanagedType.VBByRefStr)] ref string buff, int maxbuff);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int getpixelcolor(ref imgdes image, int xcoord, int ycoord);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int getpngcomment([MarshalAs(UnmanagedType.VBByRefStr)] ref string filename, [MarshalAs(UnmanagedType.VBByRefStr)] ref string commenttype, [MarshalAs(UnmanagedType.VBByRefStr)] ref string buffer, int buffermax);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int gifframecount([MarshalAs(UnmanagedType.VBByRefStr)] ref string filename, ref int totalFrames);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int gifframecountfrombuffer(ref byte buffer, ref int totalFrames);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int gifinfo([MarshalAs(UnmanagedType.VBByRefStr)] ref string filename, ref GifData gdat);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int gifinfoallframes([MarshalAs(UnmanagedType.VBByRefStr)] ref string filename, ref GifGlobalData gdata, ref GifFrameData fdata, int frameElem);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int gifinfoallframesfrombuffer(ref byte buffer, ref GifGlobalData gdata, ref GifFrameData fdata, int frameElem);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int gifinfoframe([MarshalAs(UnmanagedType.VBByRefStr)] ref string filename, ref GifData ginfo, ref GifGlobalData gdata, ref GifFrameData fdata, int frameTarget);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int gifinfoframefrombuffer(ref byte buffer, ref GifData ginfo, ref GifGlobalData gdata, ref GifFrameData fdata, int frameTarget);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int gifinfofrombuffer(ref byte buffer, ref GifData gdat);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int histobrighten(ref imgdes srcimg, ref imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int histoequalize(ref imgdes srcimg, ref imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern void hsv2rgb(ref HSVTRIPLE hsvtab, ref RGBQUAD rgbtab, int colors);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int hsvimagetorgbimage(ref imgdes srcimg, ref imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern void imageareatorect(ref imgdes image, ref RECT RECT);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int imagetodib(ref imgdes srcimg, ref int dib);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int insertplane(int plane, ref imgdes srcimg, ref imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int isgrayscaleimage(ref imgdes image);
        [DllImport("VIC32.DLL", EntryPoint = "addimage", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int isolate(ref imgdes srcimg, ref imgdes oprimg, ref imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int jpeggeterror();
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int jpeginfo([MarshalAs(UnmanagedType.VBByRefStr)] ref string filename, ref JpegData jdat);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int jpeginfoex([MarshalAs(UnmanagedType.VBByRefStr)] ref string filename, ref JpegDataEx jdat);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int jpeginfofrombuffer(ref byte buffer, ref JpegData jdat);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int jpeginfofrombufferex(ref byte buffer, ref JpegDataEx jdat);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int jpegsetthumbnailsize(int longEdge);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern void jpegsetxyresolution(int xres, int yres, int resunit);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int kodalith(int thres, ref imgdes srcimg, ref imgdes resimg);
        [DllImport("VICFX.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int lighter(ref imgdes srcimg, ref imgdes oprimg, ref imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int limitlevel(int thres, ref imgdes srcimg, ref imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int loadbif([MarshalAs(UnmanagedType.VBByRefStr)] ref string filename, ref imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int loadbmp([MarshalAs(UnmanagedType.VBByRefStr)] ref string filename, ref imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int loadbmpfrombuffer(ref byte buffer, ref imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int loadbmppalette([MarshalAs(UnmanagedType.VBByRefStr)] ref string filename, ref RGBQUAD paltab);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int loadbmppalettefrombuffer(ref byte buffer, ref RGBQUAD paltab);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int loadgif([MarshalAs(UnmanagedType.VBByRefStr)] ref string filename, ref imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int loadgifframe([MarshalAs(UnmanagedType.VBByRefStr)] ref string filename, ref imgdes resimg, ref GifGlobalData gdata, ref GifFrameData fdata);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int loadgifframefrombuffer(ref byte buffer, ref imgdes resimg, ref GifGlobalData gdata, ref GifFrameData fdata);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int loadgifframepalette([MarshalAs(UnmanagedType.VBByRefStr)] ref string filename, ref RGBQUAD paltab, int frame);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int loadgifframepalettefrombuffer(ref byte buffer, ref RGBQUAD paltab, int frame);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int loadgiffrombuffer(ref byte buffer, ref imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int loadgifglobalpalette([MarshalAs(UnmanagedType.VBByRefStr)] ref string filename, ref RGBQUAD paltab);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int loadgifglobalpalettefrombuffer(ref byte buffer, ref RGBQUAD paltab);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int loadgifpalette([MarshalAs(UnmanagedType.VBByRefStr)] ref string filename, ref RGBQUAD paltab);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int loadgifpalettefrombuffer(ref byte buffer, ref RGBQUAD paltab);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int loadjpg([MarshalAs(UnmanagedType.VBByRefStr)] ref string filename, ref imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int loadjpgex([MarshalAs(UnmanagedType.VBByRefStr)] ref string filename, ref imgdes resimg, int mode);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int loadjpgfrombuffer(ref byte buffer, ref imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int loadjpgfrombufferex(ref byte buffer, ref imgdes resimg, int mode);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int loadjpgthumbnail([MarshalAs(UnmanagedType.VBByRefStr)] ref string filename, ref imgdes resimg, int createThumbNail);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int loadjpgthumbnailfrombuffer(ref byte buffer, ref imgdes resimg, int createThumbNail);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int loadpcx([MarshalAs(UnmanagedType.VBByRefStr)] ref string filename, ref imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int loadpcxpalette([MarshalAs(UnmanagedType.VBByRefStr)] ref string filename, ref RGBQUAD paltab);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int loadpng([MarshalAs(UnmanagedType.VBByRefStr)] ref string filename, ref imgdes image);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int loadpngfrombuffer(ref byte buffer, ref imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int loadpngpalette([MarshalAs(UnmanagedType.VBByRefStr)] ref string filename, ref RGBQUAD paltab);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int loadpngpalettefrombuffer(ref byte buffer, ref RGBQUAD paltab);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int loadtga([MarshalAs(UnmanagedType.VBByRefStr)] ref string filename, ref imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int loadtgapalette([MarshalAs(UnmanagedType.VBByRefStr)] ref string filename, ref RGBQUAD paltab);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int loadtgawithalpha([MarshalAs(UnmanagedType.VBByRefStr)] ref string filename, ref imgdes resimg, ref imgdes alphaimage);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int loadtif([MarshalAs(UnmanagedType.VBByRefStr)] ref string filename, ref imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int loadtiffrombuffer(ref byte buffer, ref imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int loadtifpage([MarshalAs(UnmanagedType.VBByRefStr)] ref string filename, ref imgdes resimg, int page);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int loadtifpagebyindex([MarshalAs(UnmanagedType.VBByRefStr)] ref string filename, ref imgdes resimg, int pageIndex);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int loadtifpagebyindexfrombuffer(ref byte buffer, ref imgdes resimg, int pageIndex);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int loadtifpagebyindexfrombufferwithalpha(ref byte buffer, ref imgdes resimg, int pageIndex, ref imgdes alphaimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int loadtifpalette([MarshalAs(UnmanagedType.VBByRefStr)] ref string filename, ref RGBQUAD paltab);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int loadtifpalettefrombuffer(ref byte buffer, ref RGBQUAD paltab);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int loadtifpalettepage([MarshalAs(UnmanagedType.VBByRefStr)] ref string filename, ref RGBQUAD paltab, int page);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int loadtifpalettepagebyindex([MarshalAs(UnmanagedType.VBByRefStr)] ref string filename, ref RGBQUAD paltab, int pageIndex);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int loadtifwithalpha([MarshalAs(UnmanagedType.VBByRefStr)] ref string filename, ref imgdes resimg, ref imgdes alphaimage);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int makepalette(ref imgdes srcimg, ref PALETTEPOINT ppa_firstelem, int numelem);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int matchcolorimage(ref imgdes srcimg, ref imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int matchcolorimageex(ref imgdes srcimg, ref imgdes resimg, int mode);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int matrixconv(ref byte kernel, ref imgdes srcimg, ref imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int matrixconvex(int ksize, ref byte firstelement, int divsr, ref imgdes srcimg, ref imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int medianfilter(int ksize, ref imgdes srcimg, ref imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int mirrorimage(ref imgdes srcimg, ref imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int multiply(int multr, ref imgdes srcimg, ref imgdes resimg);
        [DllImport("vic32.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int multiplyex(double multiplier, ref imgdes srcimg, ref imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int multiplyimage(ref imgdes srcimg, ref imgdes oprimg, ref imgdes resimg);
        [DllImport("VICFX.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int multiplynegative(ref imgdes srcimg, ref imgdes oprimg, ref imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int negative(ref imgdes srcimg, ref imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int orimage(ref imgdes srcimg, ref imgdes oprimg, ref imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int outline(ref imgdes srcimg, ref imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int outlineongray(ref imgdes srcimg, ref imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int pcxinfo([MarshalAs(UnmanagedType.VBByRefStr)] ref string filename, ref PcxData pdata);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int pixelcount(int min, int max, ref int redct, ref int grnct, ref int bluct, ref imgdes srcimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int pixellize(int factr, ref imgdes srcimg, ref imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int pnggeterror();
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int pnggetxyresolution([MarshalAs(UnmanagedType.VBByRefStr)] ref string filename, ref int xres, ref int yres, ref int resunit);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int pnginfo([MarshalAs(UnmanagedType.VBByRefStr)] ref string filename, ref PngData pinfo);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int pnginfofrombuffer(ref byte buffer, ref PngData pdat);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern void pngsetxyresolution(int xres, int yres, int resunit);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int printimage(int hWnd, int hdcprn, int mode, ref imgdes image, ref RECT pRect, int boxsiz, int dspfct);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern void printimageenddoc(int hdcprn, int ejectPage);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int printimagenoeject(int hWnd, int hdcprn, int mode, ref imgdes image, ref RECT pRect, int boxsiz, int dspfct);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int printimagestartdoc(int hdcprn, [MarshalAs(UnmanagedType.VBByRefStr)] ref string docname);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int rainbowpalette(ref imgdes image);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern void recttoimagearea(ref RECT RECT, ref imgdes image);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int reduceimagecolors(ref imgdes srcimg, ref imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int removenoise(ref imgdes srcimg, ref imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int resize(ref imgdes srcimg, ref imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int resizeex(ref imgdes srcimg, ref imgdes resimg, int mode);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern void rgb2hsv(ref RGBQUAD rgbtab, ref HSVTRIPLE hsvtab, int colors);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int rgbimagetocmykimage(ref imgdes srcimg, ref imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int rgbimagetohsvimage(ref imgdes srcimg, ref imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int rotate(double angle, ref imgdes srcimg, ref imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int rotate90(int direction, ref imgdes srcimg, ref imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int rotateex(double angle, ref imgdes srcimg, ref imgdes resimg, int mode);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int savebif([MarshalAs(UnmanagedType.VBByRefStr)] ref string filename, ref imgdes srcimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int savebmp([MarshalAs(UnmanagedType.VBByRefStr)] ref string filename, ref imgdes srcimg, int cmp);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int savebmptobuffer(ref int buffaddr, ref imgdes srcimg, int comp);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int saveeps([MarshalAs(UnmanagedType.VBByRefStr)] ref string filename, ref imgdes srcimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int savegif([MarshalAs(UnmanagedType.VBByRefStr)] ref string filename, ref imgdes srcimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int savegifex([MarshalAs(UnmanagedType.VBByRefStr)] ref string filename, ref imgdes srcimg, int mode, int transColor);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int savegifframe([MarshalAs(UnmanagedType.VBByRefStr)] ref string filename, ref imgdes srcimg, ref GifGlobalSaveData gsdata, ref GifFrameSaveData fsdata, int mode);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int savegiftobufferex(ref int buffaddr, ref imgdes srcimg, int mode, int transColor);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int savejpg([MarshalAs(UnmanagedType.VBByRefStr)] ref string filename, ref imgdes srcimg, int quality);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int savejpgex([MarshalAs(UnmanagedType.VBByRefStr)] ref string filename, ref imgdes image, int quality, int mode);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int savejpgtobuffer(ref int buffaddr, ref imgdes srcimg, int quality);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int savejpgtobufferex(ref int buffaddr, ref imgdes srcimg, int quality, int mode);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int savepcx([MarshalAs(UnmanagedType.VBByRefStr)] ref string filename, ref imgdes srcimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int savepng([MarshalAs(UnmanagedType.VBByRefStr)] ref string filename, ref imgdes image, int comp);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int savepngex([MarshalAs(UnmanagedType.VBByRefStr)] ref string filename, ref imgdes srcimg, int comp, int mode, ref Png_Data savedata);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int savepngtobuffer(ref int buffaddr, ref imgdes srcimg, int comp);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int savepngtobufferex(ref int buffer, ref imgdes srcimg, int comp, int mode, ref Png_Data savedata);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int savetga([MarshalAs(UnmanagedType.VBByRefStr)] ref string filename, ref imgdes srcimg, int cmp);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int savetif([MarshalAs(UnmanagedType.VBByRefStr)] ref string filename, ref imgdes srcimg, int cmp);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int savetifpage([MarshalAs(UnmanagedType.VBByRefStr)] ref string filename, ref imgdes srcimg, int cmp, int page);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int savetiftobuffer(ref int buffaddr, ref imgdes srcimg, int comp);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int screen(ref byte dithermatrix_firstelem, ref imgdes srcimg, ref imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern void setgifcomment(int version, [MarshalAs(UnmanagedType.VBByRefStr)] ref string gifcomment);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern void setimagearea(ref imgdes image, int stx, int sty, int endx, int endy);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int setpixelcolor(ref imgdes image, int xcoord, int ycoord, int level);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int setupimgdes(int dib, ref imgdes image);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int sharpen(ref imgdes srcimg, ref imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int sharpengentle(ref imgdes srcimg, ref imgdes resimg);
        [DllImport("VICSTATS.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int sortpixelsbyval(ref imgdes resimg, ref COORD_VAL coordinatearray, int nelem);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int subimage(ref imgdes srcimg, ref imgdes oprimg, ref imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int tgainfo([MarshalAs(UnmanagedType.VBByRefStr)] ref string filename, ref TgaData tdat);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int threshold(int thres, ref imgdes srcimg, ref imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int tiffgeterror();
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int tiffgetpageinfo([MarshalAs(UnmanagedType.VBByRefStr)] ref string filename, ref int totalpages, ref int pagearray, int arrayelems);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int tiffgetpageinfofrombuffer(ref byte buffer, ref int totalpages, ref int pageArray, int arrayelems);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int tiffgetSOIofspagebyindex([MarshalAs(UnmanagedType.VBByRefStr)] ref string filename, int soiOfs, int pageIndex);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int tiffgetSOIofspagebyindexfrombuffer(ref byte buffer, int soiOfs, int pageIndex);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int tiffgetxyresolution([MarshalAs(UnmanagedType.VBByRefStr)] ref string filename, ref int xres, ref int yres, ref int resunit);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int tiffgetxyresolutionpagebyindex([MarshalAs(UnmanagedType.VBByRefStr)] ref string filename, ref int xres, ref int yres, ref int resunit, int targetpage);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int tiffinfo([MarshalAs(UnmanagedType.VBByRefStr)] ref string filename, ref TiffData tdat);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int tiffinfofrombuffer(ref byte buffer, ref TiffData tdat);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int tiffinfopage([MarshalAs(UnmanagedType.VBByRefStr)] ref string filename, ref TiffData tdat, int page);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int tiffinfopagebyindex([MarshalAs(UnmanagedType.VBByRefStr)] ref string filename, ref TiffData tdat, int pageIndex);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int tiffinfopagebyindexex([MarshalAs(UnmanagedType.VBByRefStr)] ref string filename, ref TiffDataEx tinfo, int pageIndex);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int tiffinfopagebyindexfrombuffer(ref byte buffer, ref TiffData tinfo, int pageIndex);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int tiffinfopagebyindexfrombufferex(ref byte buffer, ref TiffDataEx tinfo, int pageIndex);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern void tiffsetxyresolution(int xres, int yres, int resunit);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int tile(ref imgdes srcimg, ref imgdes resimg);
        [DllImport("VICTW32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern void TWclose();
        [DllImport("VICTW32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int TWdetecttwain(int hWnd);
        [DllImport("VICTW32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int TWgetbrightness(int hWnd, ref TWAIN_CAP_DATA brightness);
        [DllImport("VICTW32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int TWgetcontrast(int hWnd, ref TWAIN_CAP_DATA contrast);
        [DllImport("VICTW32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int TWgeterror();
        [DllImport("VICTW32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int TWgetfeeder(int hWnd, ref int feederIsEnabled, ref int feederHasPaper);
        [DllImport("VICTW32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int TWgetmeasureunit(int hWnd, ref TWAIN_CAP_DATA typeUnit);
        [DllImport("VICTW32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int TWgetphysicalsize(int hWnd, ref int width, ref int height);
        [DllImport("VICTW32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int TWgetpixeltype(int hWnd, ref TWAIN_CAP_DATA pixelType);
        [DllImport("VICTW32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int TWgetsourcenames(int hWnd, ref TW_STR32 namelist, ref int nameCount);
        [DllImport("VICTW32.DLL", EntryPoint = "TWgetsourcenames", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int TWgetsourcencount(int hWnd, int nullval, ref int nameCount);
        [DllImport("VICTW32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int TWgetxresolution(int hWnd, ref TWAIN_CAP_DATA xres);
        [DllImport("VICTW32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int TWgetyresolution(int hWnd, ref TWAIN_CAP_DATA yres);
        [DllImport("VICTW32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int TWopen(int hWnd);
        [DllImport("VICTW32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int TWscancountimages(int hWnd, ref imgdes resimg, ref RECT pRect, int showIU, int maxPages, SCNFCT saveScan);
        [DllImport("VICTW32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int TWscanimage(int hWnd, ref imgdes resimg);
        [DllImport("VICTW32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int TWscanimageex(int hWnd, ref imgdes resimg, ref RECT pRect, int showIU);
        [DllImport("VICTW32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int TWscanmultipleimages(int hWnd, ref imgdes resimg, SCNFCT saveScan);
        [DllImport("VICTW32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int TWscanmultipleimagesex(int hWnd, ref imgdes resimg, ref RECT pRect, int showIU, SCNFCT saveScan);
        [DllImport("VICTW32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int TWselectsource(int hWnd);
        [DllImport("VICTW32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int TWselectsourcebyname(int hWnd, [MarshalAs(UnmanagedType.VBByRefStr)] ref string dsname);
        [DllImport("VICTW32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int TWsetbrightness(int hWnd, ref TWAIN_CAP_DATA brightness);
        [DllImport("VICTW32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int TWsetcontrast(int hWnd, ref TWAIN_CAP_DATA contrast);
        [DllImport("VICTW32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int TWsetduplex(int hWnd, int enableDuplex);
        [DllImport("VICTW32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int TWsetfeeder(int hWnd, int enableFeeder);
        [DllImport("VICTW32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int TWsetmeasureunit(int hWnd, ref TWAIN_CAP_DATA typeUnit);
        [DllImport("VICTW32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int TWsetpagesize(int hWnd, int pageConst);
        [DllImport("VICTW32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int TWsetpixeltype(int hWnd, ref TWAIN_CAP_DATA pixelType);
        [DllImport("VICTW32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern void TWsetproductname([MarshalAs(UnmanagedType.VBByRefStr)] ref string prodName);
        [DllImport("VICTW32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int TWsetxresolution(int hWnd, ref TWAIN_CAP_DATA xres);
        [DllImport("VICTW32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int TWsetyresolution(int hWnd, ref TWAIN_CAP_DATA yres);
        [DllImport("VICTW32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern short TWvicversion();
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern void unlockLZW(int key);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int updatebitmapcolortable(ref imgdes image);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int usetable(ref byte redtab, ref byte grntab, ref byte blutab, ref imgdes srcimg, ref imgdes resimg);
        [DllImport("VICSJ32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int vicSJversion();
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern short Victorversion();
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern short Victorversiondate([MarshalAs(UnmanagedType.VBByRefStr)] ref string desStr, int bufchars);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern short Victorversionex(ref VIC_VERSION_INFO vicVerInfo);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int victowinpal(ref imgdes srcimg, ref int hPal);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int viewimage(int hWnd, int hdc, ref int hPal, int xpos, int ypos, ref imgdes image);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int viewimageex(int hWnd, int hdc, ref int hPal, int xpos, int ypos, ref imgdes image, int scrx, int scry, int colRedMode);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int windowtoimage(int hWnd, ref imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int wintovicpal(int hPal, ref imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int wtaverage(int prct, ref imgdes srcimg, ref imgdes oprimg, ref imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int wtaveragemask(ref imgdes srcimg, ref imgdes oprimg, ref imgdes resimg, ref imgdes masimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int xorimage(ref imgdes srcimg, ref imgdes oprimg, ref imgdes resimg);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int zeroimage(int level, ref imgdes image);
        [DllImport("VIC32.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern void zeroimgdes(ref imgdes image);

        // Nested Types
        [StructLayout(LayoutKind.Sequential)]
        public struct BITMAPINFOHEADER
        {
            public int biSize;
            public int biWidth;
            public int biHeight;
            public short biPlanes;
            public short biBitCount;
            public int biCompression;
            public int biSizeImage;
            public int biXPelsPerMeter;
            public int biYPelsPerMeter;
            public int biClrUsed;
            public int biClrImportant;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct COORD_VAL
        {
            public int val;
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct GifData
        {
            public int width;
            public int length;
            public int BitsColRes;
            public int BitsPPixel;
            public int BckCol;
            public int Laceflag;
            public int codesize;
            public int GIFvers;
            public int vbitcount;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct GifFrameData
        {
            public VicWin.GifFrameSaveData saveData;
            public int vbitcount;
            public int width;
            public int length;
            public int frame;
            public int interlace;
            public int codesize;
            public int colors;
            public int colorMapOffset;
            public int rasterDataOffset;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct GifFrameSaveData
        {
            public int startx;
            public int starty;
            public int hasColorMap;
            public int delay;
            public int transColor;
            public int removeBy;
            public int waitForUserInput;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct GifGlobalData
        {
            public VicWin.GifGlobalSaveData saveData;
            public int BitsPPixel;
            public int colorRes;
            public int pixelAspectRatio;
            public int commentOffset;
            public int colors;
            public int colorMapOffset;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct GifGlobalSaveData
        {
            public int scrwidth;
            public int scrlength;
            public int hasColorMap;
            public int bckColor;
            public int looop;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct HSVTRIPLE
        {
            public byte hue;
            public byte saturation;
            public byte value;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct imgdes
        {
            public int ibuff;
            public int stx;
            public int sty;
            public int endx;
            public int endy;
            public int buffwidth;
            public int palette;
            public int colors;
            public int imgtype;
            public int bmh;
            public int hBitmap;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct JPEG_THUMB_DATA
        {
            public int hasThumbNail;
            public byte width;
            public byte length;
            public byte bitcount;
            public byte coding;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct JpegData
        {
            public int ftype;
            public int width;
            public int length;
            public int comps;
            public int precision;
            public int sampfac0;
            public int sampfac1;
            public int sampfac2;
            public int sampfac3;
            public int vbitcount;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct JpegDataEx
        {
            public int ftype;
            public int width;
            public int length;
            public int comps;
            public int precision;
            public int sampfac0;
            public int sampfac1;
            public int sampfac2;
            public int sampfac3;
            public int vbitcount;
            public int xres;
            public int yres;
            public int resunit;
            public VicWin.JPEG_THUMB_DATA thumbNail;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct lfFN
        {
            public byte lfFaceName0;
            public byte lfFaceName1;
            public byte lfFaceName2;
            public byte lfFaceName3;
            public byte lfFaceName4;
            public byte lfFaceName5;
            public byte lfFaceName6;
            public byte lfFaceName7;
            public byte lfFaceName8;
            public byte lfFaceName9;
            public byte lfFaceName10;
            public byte lfFaceName11;
            public byte lfFaceName12;
            public byte lfFaceName13;
            public byte lfFaceName14;
            public byte lfFaceName15;
            public byte lfFaceName16;
            public byte lfFaceName17;
            public byte lfFaceName18;
            public byte lfFaceName19;
            public byte lfFaceName20;
            public byte lfFaceName21;
            public byte lfFaceName22;
            public byte lfFaceName23;
            public byte lfFaceName24;
            public byte lfFaceName25;
            public byte lfFaceName26;
            public byte lfFaceName27;
            public byte lfFaceName28;
            public byte lfFaceName29;
            public byte lfFaceName30;
            public byte lfFaceName31;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct LOGFONT
        {
            public int lfHeight;
            public int lfWidth;
            public int lfEscapement;
            public int lfOrientation;
            public int lfWeight;
            public byte lfItalic;
            public byte lfUnderline;
            public byte lfStrikeOut;
            public byte lfCharSet;
            public byte lfOutPrecision;
            public byte lfClipPrecision;
            public byte lfQuality;
            public byte lfPitchAndFamily;
            public VicWin.lfFN lfFaceName;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MINMAX
        {
            public int min;
            public int max;
            public int res1;
            public int res2;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MYVERSIONINFO
        {
            public int version;
            public int platformID;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PALETTEPOINT
        {
            public byte index;
            public byte red;
            public byte green;
            public byte blue;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PcxData
        {
            public int PCXvers;
            public int width;
            public int length;
            public int BPPixel;
            public int Nplanes;
            public int BytesPerLine;
            public int PalInt;
            public int vbitcount;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Png_Data
        {
            public int transcolor;
            public int reserved1;
            public int reserved2;
            public int reserved3;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PngData
        {
            public int width;
            public int length;
            public int bitDepth;
            public int vbitcount;
            public int colorType;
            public int interlaced;
            public int imageId;
            public int channels;
            public int pixelDepth;
            public int rowBytes;
            public VicWin.PNGTRANSINFO transData;
            public VicWin.PNGTRANSINFO backData;
            public int igamma;
            public int physXres;
            public int physYres;
            public int physUnits;
            public VicWin.PNGSIGBITS sigBit;
            public int offsXoffset;
            public int offsYoffset;
            public int offsUnits;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PNGSIGBITS
        {
            public int sb_long;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PNGTRANSINFO
        {
            public int isPresent;
            public byte data0;
            public byte data1;
            public byte data2;
            public byte data3;
            public byte data4;
            public int byteCount;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RGBQUAD
        {
            public byte rgbBlue;
            public byte rgbGreen;
            public byte rgbRed;
            public byte rgbReserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RGBTRIPLE
        {
            public byte rgbtBlue;
            public byte rgbtGreen;
            public byte rgbtRed;
        }

        public delegate int SCNFCT(ref VicWin.imgdes resimg);

        [StructLayout(LayoutKind.Sequential)]
        public struct TgaData
        {
            public int IDfieldchars;
            public int width;
            public int length;
            public int ColorMapType;
            public int ImageType;
            public int ColorMapEntryBits;
            public int Xorigin;
            public int Yorigin;
            public int BPerPix;
            public int ABPerPix;
            public int ScreenOrigin;
            public int Interleave;
            public int vbitcount;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct TiffData
        {
            public int ByteOrder;
            public int width;
            public int length;
            public int BitsPSample;
            public int comp;
            public int SamplesPPixel;
            public int PhotoInt;
            public int PlanarCfg;
            public int vbitcount;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct TiffDataEx
        {
            public int ByteOrder;
            public int width;
            public int length;
            public int BitsPSample;
            public int comp;
            public int SamplesPPixel;
            public int PhotoInt;
            public int PlanarCfg;
            public int vbitcount;
            public int xres;
            public int yres;
            public int resunit;
            public int fillorder;
            public int rowsperstrip;
            public int orientation;
            public int IFDofs;
            public int page;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct TW_STR32
        {
            public byte items0;
            public byte items1;
            public byte items2;
            public byte items3;
            public byte items4;
            public byte items5;
            public byte items6;
            public byte items7;
            public byte items8;
            public byte items9;
            public byte items10;
            public byte items11;
            public byte items12;
            public byte items13;
            public byte items14;
            public byte items15;
            public byte items16;
            public byte items17;
            public byte items18;
            public byte items19;
            public byte items20;
            public byte items21;
            public byte items22;
            public byte items23;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct TWAIN_CAP_DATA
        {
            public short conType;
            public VicWin.TWAIN_ONEVALUE oneValue;
            public VicWin.TWAIN_ENUMTYPE enumType;
            public VicWin.TWAIN_RANGE range;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct TWAIN_ENUMTYPE
        {
            public short tarray0;
            public short tarray1;
            public short tarray2;
            public short tarray3;
            public short tarray4;
            public short tarray5;
            public short tarray6;
            public short tarray7;
            public short tarray8;
            public short tarray9;
            public short tarray10;
            public short tarray11;
            public short tarray12;
            public short tarray13;
            public short tarray14;
            public short tarray15;
            public short tarray16;
            public short tarray17;
            public short nelems;
            public short currentIndex;
            public short defaultIndex;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct TWAIN_ONEVALUE
        {
            public short val;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct TWAIN_RANGE
        {
            public short min;
            public short max;
            public short stepSize;
            public short currentVal;
            public short defaultVal;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct VIC_VERSION_INFO
        {
            public short version;
            public short flags;
        }
    }
}