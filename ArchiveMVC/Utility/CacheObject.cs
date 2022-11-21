using ArchiveMVC.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Caching;

namespace ArchiveMVC.Utility
{
    public class CacheObject
    {
        public string ContentType { get; set; }
        public string OrginalFileName { get; set; }
    }
    public class CacheFileResult
    {
        public string KeyCache { get; set; }
        public string FileType { set; get; }
        public int Resolution { set; get; }
        public string FileExtension { get; set; }
        /// <summary>
        /// Split Tiff and Cache
        /// </summary>
        /// <param name="FileBinariesPosted">Tiff file binary</param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        /// <exception cref="">File not is Tiff, cann't split file tiff</exception>
        public static List<CacheFileResult> CacheTiff(byte[] FileBinariesPosted, string fileName)
        {
            List<byte[]> list;
            var dpi = 0.0;
            try
            {
                list = Ecm.Utility.ImageProcessing.SplitTiff(FileBinariesPosted);
            }
            catch (Exception e) { 
                throw e; 
            }            
            if(list.Count > 0)
                dpi = ProcessImages.GetHorizontalResolution(list.FirstOrDefault());
            var listCacheFileResult = new List<CacheFileResult>();
            foreach (byte[] b in list)
            {
                string keyCache = Guid.NewGuid().ToString();
                var obj = new CacheImage
                {
                    FileBinaries = b,
                    ContentType = ContentTypeEnumeration.Image.TIFF,
                    OrginalFileName = fileName,
                    Resolution = (int)dpi
                };
                System.Web.HttpContext.Current.Cache.Add(
                    keyCache, obj, null,
                    DateTime.Now.AddMinutes(15),
                    System.Web.Caching.Cache.NoSlidingExpiration,
                    System.Web.Caching.CacheItemPriority.Default, null);
                listCacheFileResult.Add(new CacheFileResult
                {
                    KeyCache = keyCache,
                    FileType = ContentTypeEnumeration.Image.IMAGE_TYPE,
                    Resolution = (int)dpi
                });
            }
            return listCacheFileResult;
        }

        public static CacheFileResult CacheImage(byte[] FileBinariesPosted, string contentType, string fileName)
        {
            var dpi = ProcessImages.GetHorizontalResolution(FileBinariesPosted);
            var keyCache = Guid.NewGuid().ToString();

            var obj = new CacheImage
            {
                FileBinaries = FileBinariesPosted,
                ContentType = contentType,
                OrginalFileName = fileName,
                Resolution = (int)dpi
            };
            System.Web.HttpContext.Current.Cache.Add(
                keyCache, obj, null,
                DateTime.Now.AddMinutes(15),
                System.Web.Caching.Cache.NoSlidingExpiration,
                System.Web.Caching.CacheItemPriority.Default,
                null);
            var rs = new CacheFileResult
            {
                KeyCache = keyCache,
                FileType = ContentTypeEnumeration.Image.IMAGE_TYPE,
                Resolution = (int)dpi,
                FileExtension = string.IsNullOrEmpty(fileName) ? string.Empty : fileName.Substring(fileName.IndexOf("."))
            };
            return rs;
        }
    }
    public class DocumentResult
    {
        public List<CacheFileResult> CacheFiles;
        public DocumentModel Document;
    }
    public class CacheImage : CacheObject
    {
        public Byte[] FileBinaries { get; set; }
        public int Resolution { set; get; }
    }
    public class CacheFilesBinary : CacheObject
    {

        public Byte[] FileBinaries { get; set; }
    }
    public class CacheTemporaryFile : CacheObject
    {
        public string FileName { get; set; }
    }
    public class TextContent
    {
        public string Content { set; get; }
        public string FileType { set; get; }
    }
    public class CacheHelper
    {
        public static List<CacheFileResult> CacheFile(string contentType, byte[] binary, string fileName, string tempFolder)
        {
            try
            {
                var listCacheFileResult = new List<CacheFileResult>();
                string keyCache = Guid.NewGuid().ToString(); ;
                CacheImage obj;
                var cTime = DateTime.Now.AddHours(24);
                var cExp = System.Web.Caching.Cache.NoSlidingExpiration;
                var cPri = System.Web.Caching.CacheItemPriority.Normal;

                var name = tempFolder + "/" + fileName;
                if (!Directory.Exists(tempFolder))
                {
                    Directory.CreateDirectory(tempFolder);
                }

                ConvertDocumentHelper convert = new ConvertDocumentHelper();
                File.WriteAllBytes(name, binary);

                if (contentType.StartsWith(ContentTypeEnumeration.Image.IMAGE_TYPE))
                {
                    #region CacheImage
                    try
                    {
                        switch (contentType)
                        {
                            case ContentTypeEnumeration.Image.TIFF:
                                {
                                    try
                                    {
                                        listCacheFileResult = CacheFileResult.CacheTiff(binary, fileName);
                                    }
                                    catch
                                    {
                                        //If exception, try get actual content type of image by binary
                                        contentType = ProcessImages.GetImageMimeType(binary);
                                        listCacheFileResult.Add(CacheFileResult.CacheImage(binary, contentType, fileName));
                                    }
                                    break;
                                }
                            case ContentTypeEnumeration.Image.BIMAP:
                            case ContentTypeEnumeration.Image.GIF:
                            case ContentTypeEnumeration.Image.JPEG:
                            case ContentTypeEnumeration.Image.PNG:
                            case ContentTypeEnumeration.Image.SVG:
                                {
                                    listCacheFileResult.Add(CacheFileResult.CacheImage(binary, contentType, fileName));
                                    break;
                                }
                        }
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }
                    return listCacheFileResult;
                    #endregion
                }
                else if (contentType.StartsWith(ContentTypeEnumeration.PlainText.TEXT_TYPE))
                {
                    #region Cache Text
                    obj = new CacheImage
                    {
                        FileBinaries = binary,
                        ContentType = contentType,
                        OrginalFileName = fileName
                    };
                    System.Web.HttpContext.Current.Cache.Add(keyCache, obj, null, cTime, cExp, cPri, null);
                    listCacheFileResult.Add(new CacheFileResult
                    {
                        KeyCache = keyCache,
                        FileType = ContentTypeEnumeration.PlainText.TEXT_TYPE
                    });
                    return listCacheFileResult;
                    #endregion
                }
                else if (contentType.Equals(ContentTypeEnumeration.Document.MSOffice.DOC) ||
                        contentType.Equals(ContentTypeEnumeration.Document.MSOffice.DOCX) ||
                        contentType.Equals(ContentTypeEnumeration.Document.OpenOffice.ODT))
                {
                    #region Cache Word
                    string outName = convert.Word2Html(name, tempFolder);
                    if (outName != null)
                    {
                        CacheTemporaryFile objDoc = new CacheTemporaryFile
                        {
                            FileName = HttpContext.Current.Request.ApplicationPath + "Temp/" + Path.GetFileNameWithoutExtension(fileName) + ".html",
                            ContentType = contentType,
                            OrginalFileName = Path.GetFileName(fileName)
                        };
                        System.Web.HttpContext.Current.Cache.Add(keyCache, objDoc, null, cTime, cExp, cPri, null);
                        listCacheFileResult.Add(new CacheFileResult
                        {
                            KeyCache = keyCache,
                            FileType = contentType
                        });
                        return listCacheFileResult;
                    }
                    return null;
                    #endregion
                }
                else if (contentType.Equals(ContentTypeEnumeration.Document.MSOffice.XLS) ||
                        contentType.Equals(ContentTypeEnumeration.Document.MSOffice.XLSX) ||
                        contentType.Equals(ContentTypeEnumeration.Document.OpenOffice.ODS))
                {
                    #region Cache Excel
                    string outName = convert.Excel2Html(name, tempFolder);
                    if (outName != null)
                    {
                        CacheTemporaryFile objDoc = new CacheTemporaryFile
                        {
                            FileName = HttpContext.Current.Request.ApplicationPath + "Temp/" + Path.GetFileNameWithoutExtension(fileName) + ".html",
                            ContentType = contentType,//ContentTypeEnumeration.PlainText.HTML
                            OrginalFileName = Path.GetFileName(fileName)
                        };
                        System.Web.HttpContext.Current.Cache.Add(keyCache, objDoc, null, cTime, cExp, cPri, null);
                        listCacheFileResult.Add(new CacheFileResult
                        {
                            KeyCache = keyCache,
                            FileType = contentType
                        });
                        return listCacheFileResult;
                    }
                    return null;
                    #endregion
                }
                else if (contentType.Equals(ContentTypeEnumeration.Document.MSOffice.PPT) ||
                        contentType.Equals(ContentTypeEnumeration.Document.MSOffice.PPTX) ||
                        contentType.Equals(ContentTypeEnumeration.Document.OpenOffice.ODP))
                {
                    #region Cache PPT
                    string outName = convert.PowerPoint2Html(name, tempFolder);
                    if (outName != null)
                    {
                        CacheTemporaryFile objDoc = new CacheTemporaryFile
                        {
                            FileName = HttpContext.Current.Request.ApplicationPath + "Temp/" + Path.GetFileNameWithoutExtension(fileName) + ".html",
                            ContentType = contentType,
                            OrginalFileName = Path.GetFileName(fileName),
                        };
                        System.Web.HttpContext.Current.Cache.Add(keyCache, objDoc, null, cTime, cExp, cPri, null);
                        listCacheFileResult.Add(new CacheFileResult
                        {
                            KeyCache = keyCache,
                            FileType = contentType
                        });
                        return listCacheFileResult;
                    }
                    return null;
                    #endregion
                }
                else if (contentType.Equals(ContentTypeEnumeration.Document.PDF))
                {
                    #region Cache PDF
                    CacheTemporaryFile objDoc = new CacheTemporaryFile
                    {
                        FileName = HttpContext.Current.Request.ApplicationPath + "Temp/" + Path.GetFileName(fileName),
                        ContentType = contentType,//ContentTypeEnumeration.PlainText.HTML
                        OrginalFileName = Path.GetFileName(fileName)
                    };
                    System.Web.HttpContext.Current.Cache.Add(keyCache, objDoc, null, cTime, cExp, cPri, null);
                    listCacheFileResult.Add(new CacheFileResult
                    {
                        KeyCache = keyCache,
                        FileType = contentType
                    });
                    return listCacheFileResult;
                    #endregion
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            return null;
        }
        public static void DeleteCache(string guid)
        {
            System.Web.HttpContext.Current.Cache.Remove(guid);
        }
        public static void DeleteCache(List<string> guids)
        {
            foreach (var guid in guids)
            {
                System.Web.HttpContext.Current.Cache.Remove(guid);
            }
        }
        private static List<string> Caching = new List<string>();
        public static void AddCacheNotIndex(string k)
        {
            var key = Utilities.UserName + "_NOT_INDEXED";
            if (System.Web.HttpContext.Current.Cache[key] == null)
            {
                //System.Web.HttpContext.Current.Cache.Add()
            }

        }
    }
}