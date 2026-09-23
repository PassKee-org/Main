using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace PassKee.Business.Extensions
{
    public static class StreamExtensions
    {
        public static void PrepareToCopy(this Stream stream)
        {
            if (!stream.CanRead)
            {
                throw new NotSupportedException("Cannot read from the stream.");
            }
            stream.Position = 0;
        }
    }
}
