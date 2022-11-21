using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using Ecm.Utility.Victor;

namespace Ecm.Utility
{
    public class ImageProcessing
    {
        public static byte[] ConvertMultiplePageImage(List<byte[]> imageList)
        {
            if (imageList != null && imageList.Count() > 0)
            {
                var encoder = new TiffBitmapEncoder();
                MemoryStream memoryStream;
                foreach (var datas in imageList)
                {
                    memoryStream = new MemoryStream(datas);
                    encoder.Frames.Add(BitmapFrame.Create(memoryStream));
                }

                memoryStream = new MemoryStream();
                encoder.Compression = TiffCompressOption.Zip;
                encoder.Save(memoryStream);
                return memoryStream.ToArray();
            }

            return null;
        }

        public static unsafe byte[] ConvertToTiff(byte[] imageBytes, string fileType)
        {
            int compression = 0, xres = 0, yres = 0, runit = 0, width = 0, length = 0, bitCount = 0;
            GetInfo(
                imageBytes,
                fileType,
                ref compression,
                ref xres,
                ref yres,
                ref runit,
                ref width,
                ref length,
                ref bitCount);

            VicWin.tiffsetxyresolution(xres, yres, runit);

            var image = new VicWin.imgdes();
            int rCode = VicWin.allocimage(ref image, width, length, bitCount);
            if (rCode != VicWin.NO_ERROR)
            {
                throw new Exception(GetErrorMessage(rCode));
            }

            try
            {
                LoadImage(imageBytes, fileType, ref image);
                var outBuff = 0;
                rCode = VicWin.savetiftobuffer(ref outBuff, ref image, compression);
                if (rCode != VicWin.NO_ERROR)
                {
                    throw new Exception(GetErrorMessage(rCode));
                }

                var bufferSize = VicWin.getbuffersize(outBuff);
                var imgBytes = new byte[bufferSize];
                fixed (byte* iba = &imgBytes[0])
                {
                    copybuffertobytearray(iba, outBuff, bufferSize);
                }

                return imgBytes;
            }
            finally
            {
                VicWin.freeimage(ref image);
            }
        }

        public static string GetErrorMessage(int errcode)
        {
            if (errcode != VicWin.NO_ERROR)
            {
                // Ignore an error code of NO_ERROR
                // Load error string into szErrstr
                String szErrstr;
                switch (errcode)
                {
                    case VicWin.NO_ERROR:
                        szErrstr = "0";
                        break;
                    case VicWin.BAD_RANGE:
                        szErrstr = "Value(s) out of range";
                        break;
                    case VicWin.NO_DIG:
                        szErrstr = "Error in receiving image";
                        break;
                    case VicWin.BAD_DSK:
                        szErrstr = "Disk full, file not written: ";
                        break;
                    case VicWin.BAD_OPN:
                        szErrstr = "Cannot open ";
                        break;
                    case VicWin.BAD_FAC:
                        szErrstr = "Invalid Data";
                        break;
                    case VicWin.BAD_TIFF:
                        szErrstr = "Unreadable TIF format: ";
                        break;
                    case VicWin.BAD_BPS:
                        szErrstr = "Unsupported TIF bits per sample";
                        break;
                    case VicWin.BAD_CMP:
                        szErrstr = "Unsupported compression scheme";
                        break;
                    case VicWin.BAD_CRT:
                        szErrstr = "Cannot create ";
                        break;
                    case VicWin.BAD_FTPE:
                        szErrstr = "Invalid filetype: ";
                        break;
                    case VicWin.BAD_DIB:
                        szErrstr = "DIB is compressed and can not be processed";
                        break;
                    case VicWin.BAD_MEM:
                        szErrstr = "Insufficient memory";
                        break;
                    case VicWin.BAD_PCX:
                        szErrstr = "Unreadable PCX format: ";
                        break;
                    case VicWin.BAD_GIF:
                        szErrstr = "Unreadable GIF format: ";
                        break;
                    case VicWin.PRT_ERR:
                        szErrstr = "Printer error";
                        break;
                    case VicWin.SCAN_ERR:
                        szErrstr = "Scanner error";
                        break;
                    case VicWin.BAD_TGA:
                        szErrstr = "Unreadable Targa format: ";
                        break;
                    case VicWin.BAD_BPP:
                        szErrstr = "Image bits per pixel not supported by function";
                        break;
                    case VicWin.BAD_BMP:
                        szErrstr = "Unreadable BMP format: ";
                        break;
                    case VicWin.NO_DEV_DATA:
                        szErrstr = "No data from device";
                        break;
                    case VicWin.TIMEOUT:
                        szErrstr = "Function timed out";
                        break;
                    case VicWin.PRT_BUSY:
                        szErrstr = "Print function is busy";
                        break;
                    case VicWin.BAD_IBUF:
                        szErrstr = "Invalid image buffer address";
                        break;
                    case VicWin.TIFF_NOPAGE:
                        szErrstr = "TIF page not found";
                        break;
                    case VicWin.TOO_CPLX:
                        szErrstr = "Image is too complex to complete operation";
                        break;
                    case VicWin.NOT_AVAIL:
                        szErrstr = "Function not available due to missing module";
                        break;
                    // vicwin.Scanner ADF error codes;
                    case VicWin.SCAN_UNLOAD:
                        szErrstr = "Paper could not be unloaded from ADF";
                        break;
                    case VicWin.SCAN_LIDUP:
                        szErrstr = "ADF lid was opened";
                        break;
                    case VicWin.SCAN_NOPAPER:
                        szErrstr = "ADF bin is empty";
                        break;
                    case VicWin.SCAN_NOADF:
                        szErrstr = "ADF is not connected";
                        break;
                    case VicWin.SCAN_NOTREADY:
                        szErrstr = "ADF is connected but not ready";
                        break;
                    default: // 'Set any invalid error codes to default
                        szErrstr = "Error encountered when loading tif image";
                        break;
                }

                return szErrstr;
            }

            return string.Empty;
        }

        public static byte[] MergeTiffBytes(List<byte[]> imageBytes, string folderContainer)
        {
            if (!Directory.Exists(folderContainer))
            {
                Directory.CreateDirectory(folderContainer);
            }

            var savedFileName = folderContainer + "doc_" + Guid.NewGuid().GetHashCode() + ".tiff";
            var pageIndex = 0;

            foreach (var bytes in imageBytes)
            {
                var tiffbuffaddr = GetTiffBufferFromByteArray(bytes);
                var tdat = new VicWin.TiffDataEx();
                var rCode = tiffinfopagebyindexfrombufferex((IntPtr)tiffbuffaddr, ref tdat, 0);
                if (rCode != VicWin.NO_ERROR)
                {
                    throw new Exception(GetErrorMessage(rCode));
                }

                var image = new VicWin.imgdes();
                rCode = VicWin.allocimage(ref image, tdat.width, tdat.length, tdat.vbitcount);
                if (rCode != VicWin.NO_ERROR)
                {
                    throw new Exception(GetErrorMessage(rCode));
                }

                try
                {
                    rCode = loadtiffrombuffer((IntPtr)tiffbuffaddr, ref image);
                    if (rCode != VicWin.NO_ERROR)
                    {
                        throw new Exception(GetErrorMessage(rCode));
                    }

                    VicWin.tiffsetxyresolution(tdat.xres, tdat.yres, tdat.resunit);
                    rCode = VicWin.savetifpage(ref savedFileName, ref image, tdat.vbitcount == 1 ? 4 : 6, pageIndex);
                    if (rCode != VicWin.NO_ERROR)
                    {
                        throw new Exception(GetErrorMessage(rCode));
                    }
                }
                finally
                {
                    VicWin.freeimage(ref image);
                }

                pageIndex++;
            }

            byte[] retBytes = File.ReadAllBytes(savedFileName);
            File.Delete(savedFileName);

            return retBytes;
        }

        public static string MergeTiffFiles(List<string> imageFiles, string folderContainer)
        {
            if (!Directory.Exists(folderContainer))
            {
                Directory.CreateDirectory(folderContainer);
            }

            string savedFileName = folderContainer + "doc_" + Guid.NewGuid().GetHashCode() + ".tiff";
            int pageIndex = 0;

            foreach (string file in imageFiles)
            {
                int compression = 0, xres = 0, yres = 0, runit = 0, width = 0, length = 0, bitCount = 0;
                GetInfo(file, ref compression, ref xres, ref yres, ref runit, ref width, ref length, ref bitCount);

                VicWin.tiffsetxyresolution(xres, yres, runit);

                var image = new VicWin.imgdes();
                int rCode = VicWin.allocimage(ref image, width, length, bitCount);
                if (rCode != VicWin.NO_ERROR)
                {
                    throw new Exception(GetErrorMessage(rCode));
                }

                try
                {
                    LoadImage(file, ref image);
                    rCode = VicWin.savetifpage(ref savedFileName, ref image, compression, pageIndex);

                    if (rCode != VicWin.NO_ERROR)
                    {
                        throw new Exception(GetErrorMessage(rCode));
                    }
                }
                finally
                {
                    VicWin.freeimage(ref image);
                }

                pageIndex++;
            }

            return savedFileName;
        }

        public static unsafe List<byte[]> SplitTiff(byte[] binary)
        {
            List<VicWin.TiffDataEx> tdats;
            var pages = LoadImageFromTiffBuffer(binary, out tdats);
            var retBytes = new List<byte[]>();

            try
            {
                for (var i = 0; i < pages.Count; i++)
                {
                    var page = pages[i];
                    var outBuff = 0;
                    try
                    {
                        VicWin.tiffsetxyresolution(tdats[i].xres, tdats[i].yres, tdats[i].resunit);
                        var rCode = VicWin.savetiftobuffer(ref outBuff, ref page, tdats[i].vbitcount == 1 ? 4 : 6);
                        if (rCode != VicWin.NO_ERROR)
                        {
                            throw new Exception(GetErrorMessage(rCode));
                        }

                        var bufferSize = VicWin.getbuffersize(outBuff);
                        var imgBytes = new byte[bufferSize];
                        fixed (byte* iba = &imgBytes[0])
                        {
                            copybuffertobytearray(iba, outBuff, bufferSize);
                        }

                        retBytes.Add(imgBytes);
                    }
                    finally
                    {
                        GlobalFree(outBuff);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                for (var i = 0; i < pages.Count; i++)
                {
                    VicWin.imgdes page = pages[i];
                    VicWin.freeimage(ref page);
                }
            }

            return retBytes;
        }

        public static List<string> SplitTiffFile(string tiffFile, string folderContainer)
        {
            string fileName = "page_" + Guid.NewGuid().GetHashCode();

            // get total pages in tiff file
            int totalPages = 0;
            int pageArray = 0;
            int rCode = VicWin.tiffgetpageinfo(ref tiffFile, ref totalPages, ref pageArray, 0);

            if (rCode != VicWin.NO_ERROR)
            {
                throw new Exception(GetErrorMessage(rCode));
            }

            VicWin.TiffData tdat = new VicWin.TiffData();
            rCode = VicWin.tiffinfo(ref tiffFile, ref tdat);

            if (rCode != VicWin.NO_ERROR)
            {
                throw new Exception(GetErrorMessage(rCode));
            }

            if (rCode != VicWin.NO_ERROR)
            {
                throw new Exception(GetErrorMessage(rCode));
            }

            List<string> splitFiles = new List<string>();
            for (int i = 0; i < totalPages; i++)
            {
                rCode = VicWin.tiffinfopagebyindex(ref tiffFile, ref tdat, i);
                if (rCode != VicWin.NO_ERROR)
                {
                    throw new Exception(GetErrorMessage(rCode));
                }

                int xres = 0, yres = 0, runit = 0;
                VicWin.tiffgetxyresolutionpagebyindex(ref tiffFile, ref xres, ref yres, ref runit, i);

                VicWin.imgdes images = new VicWin.imgdes();
                rCode = VicWin.allocimage(ref images, tdat.width, tdat.length, tdat.vbitcount);
                if (rCode != VicWin.NO_ERROR)
                {
                    throw new Exception(GetErrorMessage(rCode));
                }

                rCode = VicWin.loadtifpagebyindex(ref tiffFile, ref images, i);
                if (rCode != VicWin.NO_ERROR)
                {
                    throw new Exception(GetErrorMessage(rCode));
                }

                try
                {
                    if (!Directory.Exists(folderContainer))
                    {
                        Directory.CreateDirectory(folderContainer);
                    }

                    string pageFile = Path.Combine(folderContainer, string.Format("{0}_{1}.tiff", fileName, i + 1));
                    VicWin.tiffsetxyresolution(xres, yres, runit);

                    int compression = 6;
                    if (tdat.vbitcount == 1)
                    {
                        compression = 4;
                    }

                    rCode = VicWin.savetif(ref pageFile, ref images, compression);
                    if (rCode != VicWin.NO_ERROR)
                    {
                        throw new Exception(GetErrorMessage(rCode));
                    }

                    splitFiles.Add(pageFile);
                }
                finally
                {
                    VicWin.freeimage(ref images);
                }
            }

            return splitFiles;
        }

        [DllImport("kernel32.DLL", EntryPoint = "RtlMoveMemory")]
        public static extern unsafe int copybuffertobytearray(byte* des, int src, int count);

        [DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory")]
        private static extern void CopyToBuffer(int des, ref byte src, int count);

        private static string GetFileExtension(string filePath)
        {
            int lastDotIndex = filePath.LastIndexOf(".");
            return filePath.Substring(lastDotIndex + 1);
        }

        private static void GetInfo(
            string filePath,
            ref int compression,
            ref int xres,
            ref int yres,
            ref int runit,
            ref int width,
            ref int length,
            ref int bitCount)
        {
            string extension = GetFileExtension(filePath).ToLower();
            compression = 6;
            xres = 96;
            yres = 96;
            runit = 2;
            int rCode;

            switch (extension)
            {
                case "bmp":
                    VicWin.BITMAPINFOHEADER bmpInfo = new VicWin.BITMAPINFOHEADER();
                    rCode = VicWin.bmpinfo(ref filePath, ref bmpInfo);
                    width = bmpInfo.biWidth;
                    length = bmpInfo.biHeight;
                    bitCount = bmpInfo.biBitCount;
                    break;
                case "jpg":
                case "jpeg":
                    VicWin.JpegData jpgInfo = new VicWin.JpegData();
                    rCode = VicWin.jpeginfo(ref filePath, ref jpgInfo);
                    width = jpgInfo.width;
                    length = jpgInfo.length;
                    bitCount = jpgInfo.vbitcount;
                    break;
                case "png":
                    VicWin.PngData pngInfo = new VicWin.PngData();
                    rCode = VicWin.pnginfo(ref filePath, ref pngInfo);
                    width = pngInfo.width;
                    length = pngInfo.length;
                    bitCount = pngInfo.vbitcount;
                    break;
                case "gif":
                    VicWin.GifData gifInfo = new VicWin.GifData();
                    rCode = VicWin.gifinfo(ref filePath, ref gifInfo);
                    width = gifInfo.width;
                    length = gifInfo.length;
                    bitCount = gifInfo.vbitcount;
                    break;
                default: // TIFF
                    VicWin.TiffData tifInfo = new VicWin.TiffData();
                    rCode = VicWin.tiffinfo(ref filePath, ref tifInfo);
                    if (tifInfo.vbitcount == 1)
                    {
                        compression = 4;
                    }

                    width = tifInfo.width;
                    length = tifInfo.length;
                    bitCount = tifInfo.vbitcount;
                    VicWin.tiffgetxyresolution(ref filePath, ref xres, ref yres, ref runit);
                    break;
            }

            if (rCode != VicWin.NO_ERROR)
            {
                throw new Exception(GetErrorMessage(rCode));
            }
        }

        private static void GetInfo(
            byte[] fileBinary,
            string fileType,
            ref int compression,
            ref int xres,
            ref int yres,
            ref int runit,
            ref int width,
            ref int length,
            ref int bitCount)
        {
            string extension = fileType.ToLower();
            compression = 6;
            xres = 96;
            yres = 96;
            runit = 2;
            int rCode;

            switch (extension)
            {
                case "bmp":
                    VicWin.BITMAPINFOHEADER bmpInfo = new VicWin.BITMAPINFOHEADER();
                    rCode = VicWin.bmpinfofrombuffer(ref fileBinary[0], ref bmpInfo);
                    width = bmpInfo.biWidth;
                    length = bmpInfo.biHeight;
                    bitCount = bmpInfo.biBitCount;
                    break;
                case "jpg":
                case "jpeg":
                    VicWin.JpegData jpgInfo = new VicWin.JpegData();
                    rCode = VicWin.jpeginfofrombuffer(ref fileBinary[0], ref jpgInfo);
                    width = jpgInfo.width;
                    length = jpgInfo.length;
                    bitCount = jpgInfo.vbitcount;
                    break;
                case "png":
                    VicWin.PngData pngInfo = new VicWin.PngData();
                    rCode = VicWin.pnginfofrombuffer(ref fileBinary[0], ref pngInfo);
                    width = pngInfo.width;
                    length = pngInfo.length;
                    bitCount = pngInfo.vbitcount;
                    break;
                case "gif":
                    VicWin.GifData gifInfo = new VicWin.GifData();
                    rCode = VicWin.gifinfofrombuffer(ref fileBinary[0], ref gifInfo);
                    width = gifInfo.width;
                    length = gifInfo.length;
                    bitCount = gifInfo.vbitcount;
                    break;
                default: // TIFF
                    VicWin.TiffDataEx tifInfo = new VicWin.TiffDataEx();
                    rCode = VicWin.tiffinfopagebyindexfrombufferex(ref fileBinary[0], ref tifInfo, 0);
                    if (tifInfo.vbitcount == 1)
                    {
                        compression = 4;
                    }

                    width = tifInfo.width;
                    length = tifInfo.length;
                    bitCount = tifInfo.vbitcount;
                    xres = tifInfo.xres;
                    yres = tifInfo.yres;
                    runit = tifInfo.resunit;
                    break;
            }

            if (rCode != VicWin.NO_ERROR)
            {
                throw new Exception(GetErrorMessage(rCode));
            }
        }

        private static int GetTiffBufferFromByteArray(byte[] tiffByteArray)
        {
            int hbuffer = GlobalAlloc(2, tiffByteArray.Length);
            int buffaddr = GlobalLock(hbuffer);
            if (buffaddr != 0)
            {
                CopyToBuffer(buffaddr, ref tiffByteArray[0], tiffByteArray.Length);
            }

            return buffaddr;
        }

        [DllImport("kernel32.dll", EntryPoint = "GlobalAlloc")]
        private static extern int GlobalAlloc(int flag, int bytes);

        [DllImport("kernel32.dll", EntryPoint = "GlobalFree")]
        private static extern int GlobalFree(int hMem);

        [DllImport("kernel32.dll", EntryPoint = "GlobalLock")]
        private static extern int GlobalLock(int hMem);

        private static void LoadImage(string filePath, ref VicWin.imgdes image)
        {
            string extension = GetFileExtension(filePath).ToLower();
            int rCode;

            switch (extension)
            {
                case "bmp":
                    rCode = VicWin.loadbmp(ref filePath, ref image);
                    break;
                case "jpg":
                case "jpeg":
                    rCode = VicWin.loadjpg(ref filePath, ref image);
                    break;
                case "png":
                    rCode = VicWin.loadpng(ref filePath, ref image);
                    break;
                case "gif":
                    rCode = VicWin.loadgif(ref filePath, ref image);
                    break;
                default: // TIFF
                    rCode = VicWin.loadtif(ref filePath, ref image);
                    break;
            }

            if (rCode != VicWin.NO_ERROR)
            {
                throw new Exception(GetErrorMessage(rCode));
            }
        }

        private static void LoadImage(byte[] fileBinary, string fileType, ref VicWin.imgdes image)
        {
            string extension = fileType.ToLower();
            int rCode;

            switch (extension)
            {
                case "bmp":
                    rCode = VicWin.loadbmpfrombuffer(ref fileBinary[0], ref image);
                    break;
                case "jpg":
                case "jpeg":
                    rCode = VicWin.loadjpgfrombuffer(ref fileBinary[0], ref image);
                    break;
                case "png":
                    rCode = VicWin.loadpngfrombuffer(ref fileBinary[0], ref image);
                    break;
                case "gif":
                    rCode = VicWin.loadgiffrombuffer(ref fileBinary[0], ref image);
                    break;
                default: // TIFF
                    rCode = VicWin.loadtiffrombuffer(ref fileBinary[0], ref image);
                    break;
            }

            if (rCode != VicWin.NO_ERROR)
            {
                throw new Exception(GetErrorMessage(rCode));
            }
        }

        private static List<VicWin.imgdes> LoadImageFromTiffBuffer(
            byte[] imageBinary, out List<VicWin.TiffDataEx> tdats)
        {
            int rcode = 0;
            int totalPages = 0;
            int arrayElems = 0;
            int pp = 0;
            tdats = new List<VicWin.TiffDataEx>();
            rcode = VicWin.tiffgetpageinfofrombuffer(ref imageBinary[0], ref totalPages, ref pp, arrayElems);
            if (rcode != VicWin.NO_ERROR)
            {
                throw new Exception(GetErrorMessage(rcode));
            }

            List<VicWin.imgdes> pages = new List<VicWin.imgdes>();
            int tiffbuffaddr = GetTiffBufferFromByteArray(imageBinary);
            for (int i = 0; i < totalPages; i++)
            {
                VicWin.TiffDataEx tdat = new VicWin.TiffDataEx();
                VicWin.imgdes tempimage = new VicWin.imgdes();
                rcode = tiffinfopagebyindexfrombufferex((IntPtr)tiffbuffaddr, ref tdat, i);
                if (rcode != VicWin.NO_ERROR)
                {
                    VicWin.freeimage(ref tempimage);
                    VicWin.freebuffer(tiffbuffaddr);
                    throw new Exception(GetErrorMessage(rcode));
                }

                tdats.Add(tdat);
                rcode = VicWin.allocimage(ref tempimage, tdat.width, tdat.length, tdat.vbitcount);
                if (rcode != VicWin.NO_ERROR)
                {
                    VicWin.freeimage(ref tempimage);
                    VicWin.freebuffer(tiffbuffaddr);
                    throw new Exception(GetErrorMessage(rcode));
                }

                rcode = loadtifpagebyindexfrombuffer((IntPtr)tiffbuffaddr, ref tempimage, i);
                if (rcode != VicWin.NO_ERROR)
                {
                    VicWin.freeimage(ref tempimage);
                    VicWin.freebuffer(tiffbuffaddr);
                    throw new Exception(GetErrorMessage(rcode));
                }

                // 16-bit grayscale
                if (tdat.BitsPSample == 16 && tdat.SamplesPPixel == 1)
                {
                    VicWin.imgdes newimage = new VicWin.imgdes();
                    rcode = VicWin.allocimage(ref newimage, tdat.width, tdat.length, 8);
                    if (rcode != VicWin.NO_ERROR)
                    {
                        VicWin.freeimage(ref tempimage);
                        VicWin.freeimage(ref newimage);
                        VicWin.freebuffer(tiffbuffaddr);
                        throw new Exception(GetErrorMessage(rcode));
                    }

                    rcode = VicWin.convertgray16to8(ref tempimage, ref newimage);
                    if (rcode != VicWin.NO_ERROR)
                    {
                        VicWin.freeimage(ref tempimage);
                        VicWin.freeimage(ref newimage);
                        VicWin.freebuffer(tiffbuffaddr);
                        throw new Exception(GetErrorMessage(rcode));
                    }

                    VicWin.freeimage(ref tempimage);
                    VicWin.copyimgdes(ref newimage, ref tempimage);
                    pages.Add(tempimage);
                }
                else
                {
                    pages.Add(tempimage);
                }
            }

            VicWin.freebuffer(tiffbuffaddr);
            return pages;
        }

        [DllImport("VIC32.DLL", EntryPoint = "loadtiffrombuffer")]
        private static extern int loadtiffrombuffer(IntPtr buffaddr, ref VicWin.imgdes timage);

        [DllImport("VIC32.DLL", EntryPoint = "loadtifpagebyindexfrombuffer")]
        private static extern int loadtifpagebyindexfrombuffer(IntPtr buffaddr, ref VicWin.imgdes timage, int pageindex);

        [DllImport("VIC32.DLL", EntryPoint = "tiffinfopagebyindexfrombufferex")]
        private static extern int tiffinfopagebyindexfrombufferex(
            IntPtr buffaddr, ref VicWin.TiffDataEx tdat, int pageindex);

    }
}
