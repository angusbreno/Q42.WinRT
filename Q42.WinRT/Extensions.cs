﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

#if NETFX_CORE
using System.Collections.Concurrent;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
#endif

namespace Q42.WinRT
{
    /// <summary>
    /// Extension methods
    /// </summary>
    public static class Extensions
    {


#if NETFX_CORE

      #region Windows 8 Extensions

      /// <summary>
      /// Similar in nature to Parallel.ForEach, with the primary difference being that Parallel.ForEach is a synchronous method and uses synchronous delegates.
      /// http://blogs.msdn.com/b/pfxteam/archive/2012/03/05/10278165.aspx
      /// </summary>
      /// <typeparam name="T"></typeparam>
      /// <param name="source"></param>
      /// <param name="dop"></param>
      /// <param name="body"></param>
      /// <returns></returns>
      public static Task ForEachAsync<T>(this IEnumerable<T> source, int dop, Func<T, Task> body)
      {
        return Task.WhenAll(
            from partition in Partitioner.Create(source).GetPartitions(dop)
            select Task.Run(async delegate
            {
              using (partition)
                while (partition.MoveNext())
                  await body(partition.Current).ConfigureAwait(false);
            }));
      }

  
      #endregion

#endif

      /// <summary>
      /// Converts Uri to cache key extension method
      /// The cache key has a maximum length, because it is stored a file and there is a max path length and because not all characters in an uri are allowed in a file path.
      /// Simply filtering those characters could also cause duplicate cache keys.
      /// </summary>
      /// <param name="uri"></param>
      /// <returns></returns>
      public static string ToCacheKey(this Uri uri)
      {
        if (uri == null)
          throw new ArgumentNullException("uri");

        string hashedResult = uri.GetHashCode().ToString();

        //http://stackoverflow.com/questions/3009284/using-regex-to-replace-invalid-characters
        string pattern = "[\\~#%&*{}/:<>?|\"-]";
        string replacement = " ";

        Regex regEx = new Regex(pattern);
        string sanitized = Regex.Replace(regEx.Replace(hashedResult, replacement), @"\s+", "_");

        return sanitized;
      }


        /// <summary>
        /// Extension method to check if file exist in folder
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
      public static async Task<bool> ContainsFileAsync(this StorageFolder folder, string fileName)
      {
        //This looks nicer, but gave a COM errors in some situations
        //TODO: Check again in final release of Windows 8 (or 9, or 10)
        //return (await folder.GetFilesAsync()).Where(file => file.Name == fileName).Any();

#if WINDOWS_PHONE
        return System.IO.File.Exists(string.Format(@"{0}\{1}", folder.Path, fileName));
#endif

//#if NETFX_CORE
//        var item = await ApplicationData.Current.LocalFolder.TryGetItemAsync(fileName);
//        return item != null;
//#endif

        try
        {
          await folder.GetFileAsync(fileName);
          return true;
        }
        catch (FileNotFoundException)
        {
          return false;
        }
        catch (Exception)
        {
          return false;
        }

      }


       
    }
}
