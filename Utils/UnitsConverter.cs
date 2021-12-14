namespace LocalDatabase_Client.Utils
{
    public static class UnitsConverter
    {

        public static double ConvertBytesToGigabytes(double bytes)
        {
            double resultGigabytes = bytes / 1024.0 / 1024.0 / 1024.0;
            return resultGigabytes;
        }

        public static long ConvertGigabytesToBytes(double gigabytes)
        {
            double resultBytes = gigabytes * 1024 * 1024 * 1024;
            return (long)resultBytes;
        }
    }
}
